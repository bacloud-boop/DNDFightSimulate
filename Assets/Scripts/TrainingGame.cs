using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class TrainingGame : MonoBehaviour
{
    readonly CombatState combat = new CombatState();
    readonly List<string> history = new List<string>();
    string page = "home", dice = "D10 · 待投掷";
    Weapon selectedWeapon = Weapon.Available[0];
    bool rolling;
    string panel="";
    readonly MonsterDamage monster = new MonsterDamage();
    readonly string[] damageInputs = {"0","0","0","0","0","0"};
    string inputError="";
    readonly PlayerAttributes attributes = new PlayerAttributes();
    readonly string[] attributeInputs = {"10","10","10","10","10","10"};
    string attributeError="";
    readonly PlayerAttributes monsterAttributes = new PlayerAttributes();
    readonly string[] monsterAttributeInputs = {"10","10","10","10","10","10"};
    string monsterHpInput="1000", monsterArmorInput="10", monsterAttributeError="";
    readonly PlayerClasses classes = new PlayerClasses();
    readonly string[] classInputs = {"1","0","0","0","0","0","0","0","0","0"};
    Vector2 classScroll;
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
    public void Attack() { if(!rolling) StartCoroutine(Roll(selectedWeapon, attributes.StrengthModifier)); }
    IEnumerator Roll(Weapon attackWeapon, int strengthModifier)
    {
        rolling=true;
        int targetArmor=combat.MonsterArmorClass;
        for(int i=0;i<6;i++) { dice="命中判定 D20 · 滚动中 · "+Random.Range(1,21); PlayRoll(); yield return new WaitForSecondsRealtime(.12f); }
        int hitRoll=Random.Range(1,21);
        string hitDetails="D20="+hitRoll+" / 目标护甲 "+targetArmor;
        bool critical=hitRoll==20;
        if(hitRoll==1 || (!critical && hitRoll<targetArmor)) {
            dice=hitDetails+"，未命中，伤害 0";
            history.Insert(0,attackWeapon.Name+" "+dice);
            if(history.Count>3) history.RemoveRange(3,history.Count-3);
            rolling=false; yield break;
        }
        string hitLabel=critical?"暴击（伤害骰数量翻倍）":"命中";
        dice=hitDetails+"，"+hitLabel;
        yield return new WaitForSecondsRealtime(.5f);
        for(int i=0;i<9;i++) { dice=attackWeapon.Name+" · D"+attackWeapon.DiceSides+" 滚动中 · "+attackWeapon.RollDamage(); PlayRoll(); yield return new WaitForSecondsRealtime(.10f+i*.012f); }
        int firstRoll=attackWeapon.RollDamage();
        int secondRoll=critical?attackWeapon.RollDamage():0;
        int result=firstRoll+secondRoll;
        string damageDice=critical?"2d"+attackWeapon.DiceSides+"="+firstRoll+"+"+secondRoll:"1d"+attackWeapon.DiceSides+"="+firstRoll;
        int damage=Mathf.Max(0,result+strengthModifier);
        string modifierText=(strengthModifier>=0?"+":"")+strengthModifier;
        combat.ApplyDamage(damage);
        dice=hitLabel+" · "+damageDice+"，力量修正 "+modifierText+"，伤害 "+damage;
        history.Insert(0,hitDetails+" "+hitLabel+"；"+attackWeapon.Name+" "+damageDice+"，力量修正 "+modifierText+"，本次伤害 "+damage+"。");
        if(combat.Hp==0) { history.Insert(0,"木桩死亡，双方恢复满血，重新开始战斗。"); combat.ResetIfDead(); }
        if(history.Count>3) history.RemoveRange(3,history.Count-3); rolling=false;
    }
    void OnGUI()
    {
        if(title==null) {
            title=new GUIStyle(GUI.skin.label){font=font,fontSize=44,normal={textColor=new Color(.9f,.85f,.68f)}};
            heading=new GUIStyle(title){fontSize=28}; label=new GUIStyle(title){fontSize=20,wordWrap=true};
            button=new GUIStyle(GUI.skin.button){font=font,fontSize=22,fixedHeight=54};
        }
        GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1100f,Screen.height/760f,1));
        GUI.backgroundColor=new Color(.19f,.32f,.28f);
        GUI.Box(new Rect(0,0,1100,760),GUIContent.none);
        GUILayout.BeginArea(new Rect(100,55,900,660));
        GUILayout.Label("DND · COMBAT SIMULATOR",label); GUILayout.Space(20);
        if(page=="home") {
            GUILayout.Label("木桩训练场",title); GUILayout.Label("一面骰子，一次攻击。",label); GUILayout.Space(35);
            if(GUILayout.Button("进入战斗 →",button)) page="battle";
            if(GUILayout.Button("设置",button)) page="settings";
            if(GUILayout.Button("退出游戏",button)) QuitGame();
        } else if(page=="settings") {
            GUILayout.Label("声音设置",title); GUILayout.Space(24);
            GUILayout.Label("音乐音量  "+Mathf.RoundToInt(musicVolume*100)+"%",label);
            float m=Mathf.Round(GUILayout.HorizontalSlider(musicVolume,0,1)*100)/100f; GUILayout.Space(24);
            GUILayout.Label("音效音量  "+Mathf.RoundToInt(sfxVolume*100)+"%",label);
            float s=Mathf.Round(GUILayout.HorizontalSlider(sfxVolume,0,1)*100)/100f; GUILayout.Space(24);
            if(m!=musicVolume || s!=sfxVolume) {musicVolume=m;sfxVolume=s;SaveVolume();}
            if(GUILayout.Button("试听投骰音效",button)) PlayRoll();
            if(GUILayout.Button("← 返回主界面",button)) page="home";
        } else {
            GUILayout.Label("训练战斗",title); GUILayout.Label("训练木桩 · 回合结束反击",heading);
            GUILayout.Label(combat.Hp+" / "+combat.MaxHp+"     第 "+combat.Round+" 回合",label);
            Rect bar=GUILayoutUtility.GetRect(900,20); GUI.Box(bar,GUIContent.none);
            GUI.Box(new Rect(bar.x,bar.y,bar.width*combat.Hp/combat.MaxHp,bar.height),GUIContent.none);
            GUILayout.Label("玩家血量："+combat.PlayerHp+" / "+combat.PlayerMaxHp+"   玩家护甲："+combat.PlayerArmorClass+"   木桩护甲："+combat.MonsterArmorClass,label);
            GUILayout.Label(dice,heading); GUI.enabled=!rolling && panel=="";
            if(GUILayout.Button("攻击 · "+selectedWeapon.Name+" · D"+selectedWeapon.DiceSides,button)) Attack();
            if(GUILayout.Button("结束回合",button)) {EndCombatRound();}
            if(GUILayout.Button("← 返回主界面",button)) page="home";
            GUI.enabled=true; foreach(string line in history) GUILayout.Label(line,label);
        }
        GUILayout.EndArea();
        if(page=="battle") {
            GUI.enabled=panel=="";
            if(GUI.Button(new Rect(780,300,280,54),"玩家职业",button)) panel="classes";
            if(GUI.Button(new Rect(100,600,250,54),"玩家属性",button)) panel="attributes";
            if(GUI.Button(new Rect(100,660,250,54),"玩家武器",button)) panel="weapons";
            if(GUI.Button(new Rect(780,160,280,54),"怪物伤害更改",button)) panel="monster";
            if(GUI.Button(new Rect(780,230,280,54),"怪物属性",button)) panel="monsterAttributes";
            GUI.enabled=true;
            if(panel!="") DrawPanel();
        }
    }
    void EndCombatRound()
    {
        int hitRoll=Random.Range(1,21);
        string hitDetails="木桩反击 D20="+hitRoll+" / 玩家护甲 "+combat.PlayerArmorClass;
        bool critical=hitRoll==20;
        if(hitRoll!=1 && (critical || hitRoll>=combat.PlayerArmorClass)) {
            string details; int damage=monster.Roll(out details,critical);
            combat.ApplyPlayerDamage(damage);
            history.Insert(0,hitDetails+(critical?"，暴击（伤害骰数量翻倍）；":"，命中；")+details);
        } else history.Insert(0,hitDetails+"，未命中，伤害 0。");
        if(combat.PlayerHp==0) { history.Insert(0,"玩家死亡，双方恢复满血，重新开始战斗。"); combat.ResetIfDead(); }
        else combat.EndRound();
        if(history.Count>3) history.RemoveRange(3,history.Count-3);
        dice="D"+selectedWeapon.DiceSides+" · 待投掷";
    }
    void DrawPanel()
    {
        GUI.Box(new Rect(160,110,780,530),GUIContent.none);
        if(GUI.Button(new Rect(840,120,80,45),"关闭",button)) {panel=""; return;}
        GUILayout.BeginArea(new Rect(190,175,720,450));
        if(panel=="weapons") {
            GUILayout.Label("玩家武器",heading);
            foreach(var weapon in Weapon.Available) {
                if(GUILayout.Button((selectedWeapon==weapon?"✓ ":"")+weapon.Name+" · D"+weapon.DiceSides,button)) {
                    selectedWeapon=weapon;
                    if(!rolling) dice="D"+weapon.DiceSides+" · 待投掷";
                }
            }
            GUILayout.Label("本次按攻击开始时的武器结算；切换从下次攻击生效。",label);
        } else if(panel=="classes") {
            GUILayout.Label("玩家职业 · 选择初始职业",heading);
            classScroll=GUILayout.BeginScrollView(classScroll,GUILayout.Height(230));
            for(int i=0;i<PlayerClasses.Names.Length;i++) {
                GUILayout.BeginHorizontal();
                if(GUILayout.Button((selectedClass==i?"✓ ":"")+PlayerClasses.Names[i],button,GUILayout.Width(230))) selectedClass=i;
                GUILayout.Label("等级（当前 "+classes[i]+"）",label,GUILayout.Width(220));
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
                GUILayout.Label(PlayerAttributes.Names[i]+"（当前 "+monsterAttributes[i]+"）",label,GUILayout.Width(260));
                monsterAttributeInputs[i]=IntegerField(monsterAttributeInputs[i],false);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("最大 HP（当前 "+combat.MaxHp+"）",label,GUILayout.Width(260));
            monsterHpInput=IntegerField(monsterHpInput,false);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("护甲等级（当前 "+combat.MonsterArmorClass+"）",label,GUILayout.Width(260));
            monsterArmorInput=IntegerField(monsterArmorInput,false);
            GUILayout.EndHorizontal();
            if(GUILayout.Button("应用",button)) {
                int maxHp, armorClass;
                if(!int.TryParse(monsterHpInput,out maxHp) || maxHp<1) monsterAttributeError="最大 HP 必须是至少为 1 的整数。";
                else if(!int.TryParse(monsterArmorInput,out armorClass) || armorClass<5 || armorClass>30) monsterAttributeError="护甲等级必须为 5～30 的整数。";
                else if(!monsterAttributes.TryApply(monsterAttributeInputs)) monsterAttributeError="六项属性必须为 3～20 的整数。";
                else {combat.UpdateMonsterMaxHp(maxHp); combat.UpdateMonsterArmorClass(armorClass); monsterAttributeError="已应用；最大 HP 改变时同步回满。";}
            }
            GUILayout.Label(monsterAttributeError,label);
        } else if(panel=="attributes") {
            GUILayout.Label("玩家属性",heading);
            for(int i=0;i<6;i++) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(PlayerAttributes.Names[i]+"（当前 "+attributes[i]+"）",label,GUILayout.Width(260));
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
        GUILayout.EndArea();
    }
    string IntegerField(string current, bool allowNegative)
    {
        string proposed=GUILayout.TextField(current,11,GUILayout.Height(32));
        for(int i=0;i<proposed.Length;i++)
            if(!(proposed[i]>='0' && proposed[i]<='9') && !(allowNegative && i==0 && proposed[i]=='-')) return current;
        return proposed;
    }
    void ApplyMonsterSettings()
    {
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
    void OnDestroy() { if(music && music.clip) Destroy(music.clip); if(rollClip) Destroy(rollClip); if(font) Destroy(font); }
}
