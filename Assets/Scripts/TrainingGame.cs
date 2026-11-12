using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class TrainingGame : MonoBehaviour
{
    readonly CombatState combat = new CombatState();
    readonly List<string> history = new List<string>();
    string page = "home", dice = "D10 · 待投掷";
    Weapon selectedWeapon = Weapon.Available[0];
    bool rolling;
    readonly FighterFeatures fighter = new FighterFeatures();
    Weapon actionWeapon;
    readonly HashSet<Weapon> lightAttacksThisTurn=new HashSet<Weapon>();
    bool CanLightBonusAttack => !IsWildShaped && selectedWeapon.HasLight && System.Linq.Enumerable.Any(lightAttacksThisTurn,w=>w!=selectedWeapon);
    string panel="";
    readonly MonsterDamage monster = new MonsterDamage();
    readonly string[] damageInputs = {"0","0","0","0","0","0"};
    string inputError="";
    readonly PlayerAttributes attributes = new PlayerAttributes();
    readonly string[] attributeInputs = {"10","10","10","10","10","10"};
    string attributeError="";
    readonly PlayerAttributes monsterAttributes = new PlayerAttributes();
    readonly CreatureProfile monsterProfile=new CreatureProfile();
    int monsterTypeDraft=9,monsterSizeDraft=2;
    readonly string[] monsterAttributeInputs = {"10","10","10","10","10","10"};
    string monsterHpInput="1000", monsterArmorInput="10", monsterProficiencyInput="0", monsterRangeInput="5", monsterAttributeError="";
    readonly PlayerClasses classes = new PlayerClasses();
    readonly string[] classInputs = CreateClassInputs();
    static string[] CreateClassInputs(){var values=new string[PlayerClasses.Names.Length];for(int i=0;i<values.Length;i++)values[i]=i==0?"1":"0";return values;}
    Vector2 classScroll;
    Vector2 weaponScroll;
    int selectedClass;
    string classError="";
    float musicVolume, sfxVolume;
    AudioSource music, sfx;
    AudioClip rollClip;
    Font font;
    GUIStyle title, heading, label, button;

    void Awake()
    {
        Application.targetFrameRate = 60;
        font = Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei", "SimHei", "Arial"}, 24);
        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("MusicVolume", .35f));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("SfxVolume", .65f));
        music = gameObject.AddComponent<AudioSource>(); sfx = gameObject.AddComponent<AudioSource>();
        music.loop = true; music.volume = musicVolume; sfx.volume = sfxVolume;
        music.clip = MakeMusic(); rollClip = Tone(420, .09f); music.Play();
        InitializePresentation();
    }
    AudioClip Tone(float frequency, float seconds)
    {
        int count = (int)(44100 * seconds); float[] data = new float[count];
        for(int i=0;i<count;i++) data[i] = Mathf.Sin(2*Mathf.PI*frequency*i/44100f)*.22f*(1f-i/(float)count);
        var clip = AudioClip.Create("Dice",count,1,44100,false); clip.SetData(data,0); return clip;
    }
    AudioClip MakeMusic()
    {
        float[] notes = {130.81f,164.81f,196f,164.81f,146.83f,174.61f,220f,174.61f};
        int step = 28665; float[] data = new float[step*notes.Length];
        for(int n=0;n<notes.Length;n++) for(int i=0;i<step;i++)
            data[n*step+i] = Mathf.Sin(2*Mathf.PI*notes[n]*i/44100f)*.12f*Mathf.Sin(Mathf.PI*i/step);
        var clip = AudioClip.Create("Training Theme",data.Length,1,44100,false); clip.SetData(data,0); return clip;
    }
    void SaveVolume() { music.volume=musicVolume; sfx.volume=sfxVolume; PlayerPrefs.SetFloat("MusicVolume",musicVolume); PlayerPrefs.SetFloat("SfxVolume",sfxVolume); PlayerPrefs.Save(); }
    void PlayRoll() { sfx.pitch=Random.Range(.7f,1.5f); sfx.PlayOneShot(rollClip); }
    public void Attack()
    {
        if(!combat.PlayerCanAct || spellTargeting || rolling || panel!="" || page!="battle" || draggingToken>=0)return;
        if(IsWildShaped){NaturalAttack();return;}
        Weapon weapon=fighter.AttacksRemaining>0?actionWeapon:selectedWeapon;
        if(!CheckWeaponRange(weapon))return;
        if(fighter.AttacksRemaining==0) {
            if(!combat.TryUseAction())return;
            actionWeapon=selectedWeapon;fighter.BeginAttackAction(classes[0]);
        }
        fighter.ConsumeAttack();
        if(actionWeapon.HasLight)lightAttacksThisTurn.Add(actionWeapon);
        bool advantage=fighter.ConsumeAdvantage(classes[0]);
        StartCoroutine(Roll(actionWeapon,actionWeapon.AttackModifier(attributes),classes.ProficiencyBonus,actionWeapon.UsesDexterity(attributes)?"敏捷修正":"力量修正",advantage));
    }
    public void BonusAttack()
    {
        if(!combat.PlayerCanAct || spellTargeting || rolling || panel!="" || page!="battle" || draggingToken>=0 || !CanLightBonusAttack)return;
        if(!CheckWeaponRange(selectedWeapon) || !combat.TryUseBonusAction())return;
        Weapon weapon=selectedWeapon;
        bool advantage=fighter.ConsumeAdvantage(classes[0]);
        StartCoroutine(Roll(weapon,weapon.AttackModifier(attributes),classes.ProficiencyBonus,weapon.UsesDexterity(attributes)?"敏捷修正":"力量修正",advantage,"轻型附赠攻击"));
    }
    bool CheckWeaponRange(Weapon weapon)
    {
        if(weapon.CanReach(map.DistanceFeet))return true;
        dice="无法进行攻击：目标距离 "+map.DistanceFeet+" 尺，"+weapon.Name+"射程 "+weapon.RangeText+"。";
        return false;
    }
    IEnumerator Roll(Weapon attackWeapon, int abilityModifier, int proficiencyBonus, string abilityLabel, bool advantage, string attackKind="动作攻击", bool opportunity=false)
    {
        rolling=true;logScroll=Vector2.zero;
        int targetArmor=combat.MonsterArmorClass;
        advantage=advantage||combat.MonsterParalyzed||(combat.MonsterProne&&map.DistanceFeet<=5);
        bool disadvantage=(!opportunity && attackWeapon.HasRangeDisadvantage(map.DistanceFeet))||(combat.MonsterProne&&map.DistanceFeet>5);
        // Presence, not source count, determines advantage/disadvantage. Both cancel.
        bool rollHigh=advantage&&!disadvantage,rollLow=disadvantage&&!advantage;
        bool twoDice=rollHigh||rollLow;
        int firstHit=Random.Range(1,21),secondHit=twoDice?Random.Range(1,21):0;
        int hitRoll=rollHigh?Mathf.Max(firstHit,secondHit):rollLow?Mathf.Min(firstHit,secondHit):firstHit;
        string rollMode=rollHigh?"优势 2d20 取高":rollLow?"长距离劣势 2d20 取低":advantage&&disadvantage?"优势劣势抵消 · 单骰":"单骰";
        dice="玩家命中判定 · "+rollMode;
        yield return PresentRoll(20,twoDice?new[]{firstHit,secondHit}:new[]{firstHit},"采用 "+hitRoll+" + 熟练 "+proficiencyBonus+" + 属性 "+abilityModifier+" = "+(hitRoll+proficiencyBonus+abilityModifier)+" / 护甲 "+targetArmor+(hitRoll==20?"，暴击":hitRoll==1?"，必失":hitRoll+proficiencyBonus+abilityModifier>=targetArmor?"，命中":"，未命中"));
        int hitTotal=hitRoll+proficiencyBonus+abilityModifier;
        string hitDetails=attackKind+" · "+rollMode+(twoDice?" ["+firstHit+", "+secondHit+"]":"")+" D20="+hitRoll+"，熟练 "+proficiencyBonus+"，"+abilityLabel+" "+abilityModifier+"，命中总值 "+hitTotal+" / 目标护甲 "+targetArmor;
        bool critical=hitRoll==20;
        if(hitRoll==1 || (!critical && hitTotal<targetArmor)) {
            dice=hitDetails+"，未命中，伤害 0";
            history.Insert(0,attackWeapon.Name+" "+dice);
            fighter.RecordMiss(classes[0]);

            rolling=false; yield break;
        }
        critical=critical||(combat.MonsterParalyzed&&map.DistanceFeet<=5);
        string hitLabel=critical?"暴击（伤害骰数量翻倍）":"命中";
        dice=hitDetails+"，"+hitLabel;
        yield return new WaitForSecondsRealtime(fastAnimation?.15f:.5f);
        int damageModifier=attackWeapon.DiceCount==0?0:attackKind=="轻型附赠攻击"?Mathf.Min(0,abilityModifier):abilityModifier;
        int[] damageRolls=attackWeapon.RollDamageDice(critical);
        if(damageRolls.Length>0) {
            dice=hitLabel+" · 投掷 "+damageRolls.Length+"d"+attackWeapon.DiceSides;
            yield return PresentRoll(attackWeapon.DiceSides,damageRolls,"骰值 + 固定 "+attackWeapon.FixedDamage+" + 属性 "+damageModifier+"，伤害 "+Mathf.Max(0,System.Linq.Enumerable.Sum(damageRolls)+attackWeapon.FixedDamage+damageModifier));
        }
        int result=System.Linq.Enumerable.Sum(damageRolls)+attackWeapon.FixedDamage;
        string damageDice=damageRolls.Length==0?"固定伤害 "+attackWeapon.FixedDamage:damageRolls.Length+"d"+attackWeapon.DiceSides+"="+string.Join("+",damageRolls)+(attackWeapon.FixedDamage!=0?"，固定加值 "+attackWeapon.FixedDamage:"");
        int damage=Mathf.Max(0,result+damageModifier);
        string modifierText=(damageModifier>=0?"+":"")+damageModifier;
        combat.ApplyDamage(damage);
        dice=hitLabel+" · "+damageDice+"，"+abilityLabel+" "+modifierText+"，伤害 "+damage;
        history.Insert(0,hitDetails+" "+hitLabel+"；"+attackWeapon.Name+" "+damageDice+"，"+abilityLabel+" "+modifierText+"，本次伤害 "+damage+"。");
        CheckBattleOutcome();
        rolling=false;
    }
    void EndCombatRound()
    {
        if(!combat.BattleFinished && !spellTargeting && !rolling && panel=="" && page=="battle" && draggingToken<0) {
            combat.MarkBattleStarted();
            history.Insert(0,"结束第 "+combat.Round+" 回合：跳过剩余动作 "+combat.ActionsRemaining+"、附赠动作 "+(combat.BonusActionAvailable?1:0)+"。");
            StartCoroutine(Counterattack());
        }
    }
    IEnumerator Counterattack(bool endTurn=true)
    {
        rolling=true;
        if(combat.PlayerDead){rolling=false;yield break;}
        if(combat.Hp==0){if(endTurn){worldRounds++;combat.EndRound();fighter.EndRound();lightAttacksThisTurn.Clear();map.RefreshMovement();RefreshConcentration();yield return PlayerDeathSave();}rolling=false;yield break;}
        if(endTurn) {
            worldRounds++;shortRestReady=false;combat.StartMonsterTurn();map.StartMonsterMovementTurn();
            if(combat.MonsterProne&&combat.MonsterCanAct){combat.MonsterProne=false;map.SpendMonsterMovement(15);history.Insert(0,"木桩消耗15尺移动力起身。");}
            yield return ChasePlayer();
            if(page!="battle" || !map.Locked || combat.BattleFinished){rolling=false;yield break;}
            rolling=true;
        }
        if(endTurn&&!combat.MonsterCanAct){
            history.Insert(0,"木桩麻痹，跳过移动与攻击。");yield return RepeatHoldPersonSave();combat.EndRound();fighter.EndRound();lightAttacksThisTurn.Clear();map.RefreshMovement();tacticalMovement=false;RefreshConcentration();yield return PlayerDeathSave();rolling=false;yield break;
        }
        if(endTurn && map.DistanceFeet>combat.MonsterAttackRange) {
            dice="木桩无法攻击：玩家距离 "+map.DistanceFeet+" 尺，攻击距离 "+combat.MonsterAttackRange+" 尺。";
            history.Insert(0,dice);
            combat.EndRound();fighter.EndRound();lightAttacksThisTurn.Clear();map.RefreshMovement();tacticalMovement=false;
            movementMessage="拖动玩家棋子移动。";
            RefreshConcentration();yield return PlayerDeathSave();
            rolling=false;logScroll=Vector2.zero;yield break;
        }
        int firstHit=Random.Range(1,21);
        bool monsterAdvantage=combat.PlayerHp==0||combat.PlayerParalyzed,monsterDisadvantage=combat.MonsterProne;
        int secondHit=monsterAdvantage!=monsterDisadvantage?Random.Range(1,21):0;
        int hitRoll=secondHit>0?(monsterAdvantage?Mathf.Max(firstHit,secondHit):Mathf.Min(firstHit,secondHit)):firstHit;
        int proficiency=combat.MonsterProficiencyBonus;
        int targetArmor=combat.PlayerArmorClass;
        long hitTotal=(long)hitRoll+proficiency;
        dice="木桩反击 · 命中判定 D20";
        yield return PresentRoll(20,secondHit>0?new[]{firstHit,secondHit}:new[]{firstHit},"采用骰值 "+hitRoll+" + 熟练 "+proficiency+" = "+hitTotal+" / 护甲 "+targetArmor+(hitRoll==20?"，暴击":hitRoll==1?"，必失":hitTotal>=targetArmor?"，命中":"，未命中"));
        string hitDetails=(endTurn?"木桩反击":"木桩借机攻击")+" D20="+hitRoll+"，熟练 "+proficiency+"，命中总值 "+hitTotal+" / 玩家护甲 "+targetArmor;
        bool critical=hitRoll==20 || ((combat.PlayerHp==0||combat.PlayerParalyzed)&&map.DistanceFeet<=5&&hitRoll!=1&&hitTotal>=targetArmor);
        if(hitRoll!=1 && (critical || hitTotal>=targetArmor)) {
            string details;
            var sides=new List<int>();var results=new List<int[]>();var counts=new List<int>();
            int damage=monster.Roll(out details,critical,(s,v,c)=>{sides.Add(s);results.Add(v);counts.Add(c);});
            dice=critical?"木桩暴击 · 伤害骰数量翻倍":"木桩命中";
            yield return new WaitForSecondsRealtime(fastAnimation?.15f:.5f);
            for(int i=0;i<sides.Count;i++) {
                dice="木桩伤害 · "+counts[i]+"d"+sides[i]+(counts[i]>2?"（立体展示前两骰，完整合计见卷轴）":"");
                yield return PresentRoll(sides[i],results[i],i==sides.Count-1?details:"本组 "+counts[i]+"d"+sides[i]+"；完整伤害见最终结算");
            }
            combat.ApplyPlayerDamage(damage,critical);
            yield return MaintainConcentration(damage);
            dice=(critical?"木桩暴击":"木桩命中")+" · 伤害 "+damage;
            history.Insert(0,hitDetails+(critical?"，暴击；":"，命中；")+details);
        } else {dice="木桩未命中 · 伤害 0";history.Insert(0,hitDetails+"，未命中，伤害 0。");}
        CheckBattleOutcome();
        if(endTurn && !combat.PlayerDead) {
            combat.EndRound();fighter.EndRound();lightAttacksThisTurn.Clear();map.RefreshMovement();tacticalMovement=false;movementMessage="拖动玩家棋子移动。";
            RefreshConcentration();yield return PlayerDeathSave();
        }
        if(endTurn)rolling=false;logScroll=Vector2.zero;
    }
    void DrawPanel()
    {
        if(panel=="spells"){DrawSpellPanel();return;}
        if(panel=="rest"){DrawRestPanel();return;}
        if(panel=="features"){DrawFeatureCatalog();return;}
        if(panel=="beasts"){DrawBeastCatalog();return;}
        if(panel=="druid"){DrawDruidPanel();return;}
        bool left=panel=="weapons" || panel=="attributes" || panel=="classes";
        float targetX=left?350:770;
        float x=Mathf.Lerp(left?35:1120,targetX,Mathf.SmoothStep(0,1,drawerProgress));
        Surface(new Rect(x,110,480,745),paperTexture);
        if(Btn(new Rect(x+370,125,90,46),"关闭")) {panel=""; return;}
        GUILayout.BeginArea(new Rect(x+20,185,440,640));
        if(combat.BattleStarted && panel!="weapons")GUILayout.Label("战斗已开始，重置战斗后才能修改配置。",label);
        if(panel=="weapons"&&IsWildShaped)GUILayout.Label("武器已融入荒野形态，恢复原形后可切换。",label);
        if(panel=="weapons" && (rolling||fighter.AttacksRemaining>0))GUILayout.Label("本次攻击动作尚未完成，暂时不能切换武器。",label);
        GUI.enabled=panel=="weapons"?(!rolling&&fighter.AttacksRemaining==0&&!IsWildShaped):!combat.BattleStarted;
        panelScroll=GUILayout.BeginScrollView(panelScroll);
        if(panel=="weapons") {
            GUILayout.Label("玩家武器",heading);
            weaponScroll=GUILayout.BeginScrollView(weaponScroll,GUILayout.Height(310));
            foreach(var weapon in Weapon.Available) {
                if(GUILayout.Button((selectedWeapon==weapon?"✓ ":"")+weapon.Name+" · "+weapon.DamageDice+" · "+weapon.Traits,button)) {
                    selectedWeapon=weapon;
                    if(!rolling) dice=weapon.DamageDice+" · 待投掷";
                }
            }
            GUILayout.EndScrollView();
            GUILayout.Label("本次按攻击开始时的武器结算；切换从下次攻击生效。",label);
        } else if(panel=="classes") {
            GUILayout.Label("玩家职业 · 选择初始职业",heading);
            classScroll=GUILayout.BeginScrollView(classScroll,GUILayout.Height(230));
            for(int i=0;i<PlayerClasses.Names.Length;i++) {
                GUILayout.BeginHorizontal();
                if(GUILayout.Button((selectedClass==i?"✓ ":"")+PlayerClasses.Names[i],button,GUILayout.Width(130))) selectedClass=i;
                GUILayout.Label("等级（当前 "+classes[i]+"）",label,GUILayout.Width(150));
                classInputs[i]=IntegerField(classInputs[i],false);
                GUILayout.EndHorizontal();
                DrawSubclassChoice(i);
            }
            GUILayout.EndScrollView();
            GUILayout.Label("总等级 1～20，各职业 0～20，初始职业至少 1 级。当前总等级："+classes.TotalLevel,label);
            if(GUILayout.Button("应用",button)) {
                if(ValidateSubclassDraft() && classes.TryApply(classInputs,selectedClass) && classes.TryApplySubclasses(subclassChoices)) { ResetRestResources();druid.LongRest();knownFormsLevel=-1; combat.UpdatePlayerMaxHp(attributes[2],classes); classError="职业等级已应用。"; }
                else classError="等级须合法，初始职业至少1级；每个达到3级的职业须选择子职业。";
            }
            GUILayout.Label(classError,label);
            GUI.enabled=true;if(GUILayout.Button("查看职业特性目录 / 接入状态",button)){catalogClassIndex=selectedClass;panel="features";}
        } else if(panel=="monsterAttributes") {
            GUILayout.Label("怪物属性",heading);
            GUILayout.Label("生物类型（当前 "+CreatureProfile.TypeNames[monsterProfile.TypeIndex]+"）",label);
            monsterTypeDraft=GUILayout.SelectionGrid(monsterTypeDraft,CreatureProfile.TypeNames,3,new GUIStyle(button){fontSize=16,fixedHeight=34});
            GUILayout.Label("体型（当前 "+CreatureProfile.SizeNames[monsterProfile.SizeIndex]+"）",label);
            monsterSizeDraft=GUILayout.SelectionGrid(monsterSizeDraft,CreatureProfile.SizeNames,3,new GUIStyle(button){fontSize=16,fixedHeight=34});
            for(int i=0;i<6;i++) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(PlayerAttributes.Names[i]+"（当前 "+monsterAttributes[i]+"）",label,GUILayout.Width(230));
                monsterAttributeInputs[i]=IntegerField(monsterAttributeInputs[i],false);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("最大 HP（当前 "+combat.MaxHp+"）",label,GUILayout.Width(230));
            monsterHpInput=IntegerField(monsterHpInput,false);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("护甲等级（当前 "+combat.MonsterArmorClass+"）",label,GUILayout.Width(230));
            monsterArmorInput=IntegerField(monsterArmorInput,false);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("熟练加值（当前 "+combat.MonsterProficiencyBonus+"）",label,GUILayout.Width(230));
            monsterProficiencyInput=IntegerField(monsterProficiencyInput,true);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("攻击距离（尺，当前 "+combat.MonsterAttackRange+"）",label,GUILayout.Width(230));
            monsterRangeInput=IntegerField(monsterRangeInput,false);
            GUILayout.EndHorizontal();
            if(GUILayout.Button("应用",button)) {
                int maxHp, armorClass, proficiency, attackRange;
                if(!int.TryParse(monsterHpInput,out maxHp) || maxHp<1) monsterAttributeError="最大 HP 必须是至少为 1 的整数。";
                else if(!int.TryParse(monsterArmorInput,out armorClass) || armorClass<5 || armorClass>30) monsterAttributeError="护甲等级必须为 5～30 的整数。";
                else if(!int.TryParse(monsterProficiencyInput,out proficiency)) monsterAttributeError="熟练加值必须为整数，允许负数。";
                else if(!int.TryParse(monsterRangeInput,out attackRange) || attackRange<0) monsterAttributeError="攻击距离必须为非负整数（尺）。";
                else if(!monsterAttributes.TryApply(monsterAttributeInputs)) monsterAttributeError="六项属性必须为 3～20 的整数。";
                else {bool resized=map.MonsterSpaces!=CreatureProfile.SpacesForSize(monsterSizeDraft);if(resized){map.Unlock();map.TrySetFootprint(false,CreatureProfile.SpacesForSize(monsterSizeDraft));page="deployment";deploymentMessage="怪物占地已更新，请重新确认双方初始位置。";}monsterProfile.TryApply(monsterTypeDraft,monsterSizeDraft);combat.UpdateMonsterMaxHp(maxHp); combat.UpdateMonsterArmorClass(armorClass); combat.MonsterProficiencyBonus=proficiency; combat.UpdateMonsterAttackRange(attackRange); monsterAttributeError="已应用；最大 HP 改变时同步回满。";}
            }
            GUILayout.Label(monsterAttributeError,label);
        } else if(panel=="attributes") {
            GUILayout.Label("玩家属性",heading);
            for(int i=0;i<6;i++) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(PlayerAttributes.Names[i]+"（当前 "+attributes[i]+"）",label,GUILayout.Width(230));
                attributeInputs[i]=IntegerField(attributeInputs[i],false);
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("每项为 3～20 的整数；默认 10。",label);
            if(GUILayout.Button("应用",button)) {
                if(attributes.TryApply(attributeInputs)) { combat.UpdatePlayerMaxHp(attributes[2],classes); attributeError="属性已应用。"; }
                else attributeError="每项必须为 3～20 的整数，未保存本次修改。";
            }
            GUILayout.Label(attributeError,label);
        } else {
            GUILayout.Label("怪物伤害更改",heading);
            for(int i=0;i<6;i++) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i<5?"D"+MonsterDamage.Sides[i]+" 数量":"固定加值",label,GUILayout.Width(200));
                damageInputs[i]=IntegerField(damageInputs[i],i==5);
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("骰子数量为非负整数；固定加值可为负；总伤害最低为 0。",label);
            if(GUILayout.Button("应用",button)) ApplyMonsterSettings();
            GUILayout.Label(inputError,label);
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();GUI.enabled=true;
    }
    string IntegerField(string current, bool allowNegative)
    {
        string proposed=GUILayout.TextField(current,11,fieldStyle,GUILayout.Height(34));
        for(int i=0;i<proposed.Length;i++)
            if(!(proposed[i]>='0' && proposed[i]<='9') && !(allowNegative && i==0 && proposed[i]=='-')) return current;
        return proposed;
    }
    void ApplyMonsterSettings()
    {
        if(combat.BattleStarted)return;
        int[] values=new int[6];
        for(int i=0;i<6;i++) {
            if(!int.TryParse(damageInputs[i],out values[i]) || (i<5 && values[i]<0)) { inputError="请输入有效整数，骰子数量不能为负。";return; }
        }
        // Keep a mistyped huge input from freezing the main thread during dice rolls.
        long count=0;for(int i=0;i<5;i++) count+=values[i];
        if(count>10000) {inputError="单次反击骰子总数不能超过 10000。";return;}
        for(int i=0;i<5;i++) monster.Counts[i]=values[i];
        monster.Bonus=values[5]; inputError="已应用，下次回合结束时生效。";
    }
    public void QuitGame()
    {
        SaveVolume();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
#else
        Application.Quit();
#endif
    }
    void OnDestroy() { DisposePresentation(); if(music && music.clip) Destroy(music.clip); if(rollClip) Destroy(rollClip); if(font) Destroy(font); }
}
