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
    string panel="";
    readonly MonsterDamage monster = new MonsterDamage();
    readonly string[] damageInputs = {"0","0","0","0","0","0"};
    string inputError="";
    readonly PlayerAttributes attributes = new PlayerAttributes();
    readonly string[] attributeInputs = {"10","10","10","10","10","10"};
    string attributeError="";
    readonly PlayerAttributes monsterAttributes = new PlayerAttributes();
    readonly string[] monsterAttributeInputs = {"10","10","10","10","10","10"};
    string monsterHpInput="1000", monsterArmorInput="10", monsterProficiencyInput="0", monsterAttributeError="";
    readonly PlayerClasses classes = new PlayerClasses();
    readonly string[] classInputs = {"1","0","0","0","0","0","0","0","0","0","0"};
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
        if(rolling || panel!="" || page!="battle")return;
        if(fighter.AttacksRemaining==0) {
            if(!combat.TryUseAction())return;
            actionWeapon=selectedWeapon;fighter.BeginAttackAction(classes[0]);
        }
        fighter.ConsumeAttack();
        bool advantage=fighter.ConsumeAdvantage(classes[0]);
        StartCoroutine(Roll(actionWeapon,actionWeapon.AttackModifier(attributes),classes.ProficiencyBonus,actionWeapon.UsesDexterity(attributes)?"敏捷修正":"力量修正",advantage));
    }
    IEnumerator Roll(Weapon attackWeapon, int abilityModifier, int proficiencyBonus, string abilityLabel, bool advantage)
    {
        rolling=true;logScroll=Vector2.zero;
        int targetArmor=combat.MonsterArmorClass;
        int firstHit=Random.Range(1,21),secondHit=advantage?Random.Range(1,21):0;
        int hitRoll=advantage?Mathf.Max(firstHit,secondHit):firstHit;
        dice=advantage?"究明攻击 · 优势命中判定 2d20 取高":"玩家命中判定 · D20";
        yield return diceStage.Roll(20,advantage?new[]{firstHit,secondHit}:new[]{firstHit},RollDuration,PlayRoll);
        int hitTotal=hitRoll+proficiencyBonus+abilityModifier;
        string hitDetails=(advantage?"优势 ["+firstHit+", "+secondHit+"] 取高 ":"")+"D20="+hitRoll+"，熟练 "+proficiencyBonus+"，"+abilityLabel+" "+abilityModifier+"，命中总值 "+hitTotal+" / 目标护甲 "+targetArmor;
        bool critical=hitRoll==20;
        if(hitRoll==1 || (!critical && hitTotal<targetArmor)) {
            dice=hitDetails+"，未命中，伤害 0";
            history.Insert(0,attackWeapon.Name+" "+dice);
            fighter.RecordMiss(classes[0]);

            rolling=false; yield break;
        }
        string hitLabel=critical?"暴击（伤害骰数量翻倍）":"命中";
        dice=hitDetails+"，"+hitLabel;
        yield return new WaitForSecondsRealtime(fastAnimation?.15f:.5f);
        int firstRoll=attackWeapon.RollDamage();
        int secondRoll=critical?attackWeapon.RollDamage():0;
        dice=hitLabel+" · 投掷 "+(critical?"2":"1")+"d"+attackWeapon.DiceSides;
        yield return diceStage.Roll(attackWeapon.DiceSides,critical?new[]{firstRoll,secondRoll}:new[]{firstRoll},RollDuration,PlayRoll);
        int result=firstRoll+secondRoll;
        string damageDice=critical?"2d"+attackWeapon.DiceSides+"="+firstRoll+"+"+secondRoll:"1d"+attackWeapon.DiceSides+"="+firstRoll;
        int damage=Mathf.Max(0,result+abilityModifier);
        string modifierText=(abilityModifier>=0?"+":"")+abilityModifier;
        combat.ApplyDamage(damage);
        dice=hitLabel+" · "+damageDice+"，"+abilityLabel+" "+modifierText+"，伤害 "+damage;
        history.Insert(0,hitDetails+" "+hitLabel+"；"+attackWeapon.Name+" "+damageDice+"，"+abilityLabel+" "+modifierText+"，本次伤害 "+damage+"。");
        if(combat.Hp==0) { history.Insert(0,"木桩死亡，双方恢复满血，重新开始战斗。"); if(combat.ResetIfDead())fighter.ResetBattle(); }
        rolling=false;
    }
    void EndCombatRound()
    {
        if(!rolling && panel=="" && page=="battle") {
            combat.MarkBattleStarted();
            history.Insert(0,"结束第 "+combat.Round+" 回合：跳过剩余动作 "+combat.ActionsRemaining+"、附赠动作 "+(combat.BonusActionAvailable?1:0)+"。");
            StartCoroutine(Counterattack());
        }
    }
    IEnumerator Counterattack()
    {
        rolling=true;
        int hitRoll=Random.Range(1,21);
        int proficiency=combat.MonsterProficiencyBonus;
        int targetArmor=combat.PlayerArmorClass;
        long hitTotal=(long)hitRoll+proficiency;
        dice="木桩反击 · 命中判定 D20";
        yield return diceStage.Roll(20,new[]{hitRoll},RollDuration,PlayRoll);
        string hitDetails="木桩反击 D20="+hitRoll+"，熟练 "+proficiency+"，命中总值 "+hitTotal+" / 玩家护甲 "+targetArmor;
        bool critical=hitRoll==20;
        if(hitRoll!=1 && (critical || hitTotal>=targetArmor)) {
            string details;
            var sides=new List<int>();var results=new List<int[]>();var counts=new List<int>();
            int damage=monster.Roll(out details,critical,(s,v,c)=>{sides.Add(s);results.Add(v);counts.Add(c);});
            dice=critical?"木桩暴击 · 伤害骰数量翻倍":"木桩命中";
            yield return new WaitForSecondsRealtime(fastAnimation?.15f:.5f);
            for(int i=0;i<sides.Count;i++) {
                dice="木桩伤害 · "+counts[i]+"d"+sides[i]+(counts[i]>2?"（立体展示前两骰，完整合计见卷轴）":"");
                yield return diceStage.Roll(sides[i],results[i],RollDuration,PlayRoll);
            }
            combat.ApplyPlayerDamage(damage);
            dice=(critical?"木桩暴击":"木桩命中")+" · 伤害 "+damage;
            history.Insert(0,hitDetails+(critical?"，暴击；":"，命中；")+details);
        } else {dice="木桩未命中 · 伤害 0";history.Insert(0,hitDetails+"，未命中，伤害 0。");}
        if(combat.PlayerHp==0) { history.Insert(0,"玩家死亡，双方恢复满血，重新开始战斗。"); if(combat.ResetIfDead())fighter.ResetBattle(); }
        else {combat.EndRound();fighter.EndRound();}
        rolling=false;logScroll=Vector2.zero;
    }
    void DrawPanel()
    {
        bool left=panel=="weapons" || panel=="attributes" || panel=="classes";
        float targetX=left?350:770;
        float x=Mathf.Lerp(left?35:1120,targetX,Mathf.SmoothStep(0,1,drawerProgress));
        Surface(new Rect(x,110,480,745),paperTexture);
        if(Btn(new Rect(x+370,125,90,46),"关闭")) {panel=""; return;}
        GUILayout.BeginArea(new Rect(x+20,185,440,640));
        if(combat.BattleStarted && panel!="weapons")GUILayout.Label("战斗已开始，重置战斗后才能修改配置。",label);
        if(panel=="weapons" && (rolling||fighter.AttacksRemaining>0))GUILayout.Label("本次攻击动作尚未完成，暂时不能切换武器。",label);
        GUI.enabled=panel=="weapons"?(!rolling&&fighter.AttacksRemaining==0):!combat.BattleStarted;
        panelScroll=GUILayout.BeginScrollView(panelScroll);
        if(panel=="weapons") {
            GUILayout.Label("玩家武器",heading);
            weaponScroll=GUILayout.BeginScrollView(weaponScroll,GUILayout.Height(310));
            foreach(var weapon in Weapon.Available) {
                if(GUILayout.Button((selectedWeapon==weapon?"✓ ":"")+weapon.Name+" · D"+weapon.DiceSides+(weapon.HasFinesse?" · 灵巧":" · 无灵巧"),button)) {
                    selectedWeapon=weapon;
                    if(!rolling) dice="D"+weapon.DiceSides+" · 待投掷";
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
            }
            GUILayout.EndScrollView();
            GUILayout.Label("总等级 1～20，各职业 0～20，初始职业至少 1 级。当前总等级："+classes.TotalLevel,label);
            if(GUILayout.Button("应用",button)) {
                if(classes.TryApply(classInputs,selectedClass)) { combat.UpdatePlayerMaxHp(attributes[2],classes); classError="职业等级已应用。"; }
                else classError="各职业须为 0～20 整数，总等级须为 1～20，初始职业至少 1 级。";
            }
            GUILayout.Label(classError,label);
        } else if(panel=="monsterAttributes") {
            GUILayout.Label("怪物属性",heading);
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
            if(GUILayout.Button("应用",button)) {
                int maxHp, armorClass, proficiency;
                if(!int.TryParse(monsterHpInput,out maxHp) || maxHp<1) monsterAttributeError="最大 HP 必须是至少为 1 的整数。";
                else if(!int.TryParse(monsterArmorInput,out armorClass) || armorClass<5 || armorClass>30) monsterAttributeError="护甲等级必须为 5～30 的整数。";
                else if(!int.TryParse(monsterProficiencyInput,out proficiency)) monsterAttributeError="熟练加值必须为整数，允许负数。";
                else if(!monsterAttributes.TryApply(monsterAttributeInputs)) monsterAttributeError="六项属性必须为 3～20 的整数。";
                else {combat.UpdateMonsterMaxHp(maxHp); combat.UpdateMonsterArmorClass(armorClass); combat.MonsterProficiencyBonus=proficiency; monsterAttributeError="已应用；最大 HP 改变时同步回满。";}
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
