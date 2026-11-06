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
    public void Attack() { if(!rolling) StartCoroutine(Roll(selectedWeapon)); }
    IEnumerator Roll(Weapon attackWeapon)
    {
        rolling=true;
        for(int i=0;i<9;i++) { dice=attackWeapon.Name+" · D"+attackWeapon.DiceSides+" 滚动中 · "+attackWeapon.RollDamage(); PlayRoll(); yield return new WaitForSecondsRealtime(.10f+i*.012f); }
        int result=attackWeapon.RollDamage(); combat.ApplyDamage(result); dice=attackWeapon.Name+" · D"+attackWeapon.DiceSides+" → "+result;
        history.Insert(0,attackWeapon.Name+" 投掷结果 "+result+"，本次伤害 "+result+"。");
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
            float m=GUILayout.HorizontalSlider(musicVolume,0,1); GUILayout.Space(24);
            GUILayout.Label("音效音量  "+Mathf.RoundToInt(sfxVolume*100)+"%",label);
            float s=GUILayout.HorizontalSlider(sfxVolume,0,1); GUILayout.Space(24);
            if(m!=musicVolume || s!=sfxVolume) {musicVolume=m;sfxVolume=s;SaveVolume();}
            if(GUILayout.Button("试听投骰音效",button)) PlayRoll();
            if(GUILayout.Button("← 返回主界面",button)) page="home";
        } else {
            GUILayout.Label("训练战斗",title); GUILayout.Label("训练木桩 · 回合结束反击",heading);
            GUILayout.Label(combat.Hp+" / "+CombatState.MaxHp+"     第 "+combat.Round+" 回合",label);
            Rect bar=GUILayoutUtility.GetRect(900,20); GUI.Box(bar,GUIContent.none);
            GUI.Box(new Rect(bar.x,bar.y,bar.width*combat.Hp/CombatState.MaxHp,bar.height),GUIContent.none);
            GUILayout.Label("玩家血量："+combat.PlayerHp+" / "+CombatState.PlayerMaxHp,label);
            GUILayout.Label(dice,heading); GUI.enabled=!rolling && panel=="";
            if(GUILayout.Button("攻击 · "+selectedWeapon.Name+" · D"+selectedWeapon.DiceSides,button)) Attack();
            if(GUILayout.Button("结束回合",button)) {EndCombatRound();}
            if(GUILayout.Button("← 返回主界面",button)) page="home";
            GUI.enabled=true; foreach(string line in history) GUILayout.Label(line,label);
        }
        GUILayout.EndArea();
        if(page=="battle") {
            if(GUI.Button(new Rect(100,660,250,54),"玩家武器",button)) panel="weapons";
            if(GUI.Button(new Rect(780,160,280,54),"怪物伤害更改",button)) panel="monster";
            if(panel!="") DrawPanel();
        }
    }
    void EndCombatRound()
    {
        string details; int damage=monster.Roll(out details);
        combat.ApplyPlayerDamage(damage);
        history.Insert(0,"木桩反击："+details);
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
        } else {
            GUILayout.Label("怪物伤害更改",heading);
            for(int i=0;i<6;i++) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i<5?"D"+MonsterDamage.Sides[i]+" 数量":"固定加值",label,GUILayout.Width(200));
                damageInputs[i]=GUILayout.TextField(damageInputs[i],11,GUILayout.Height(32));
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("骰子数量为非负整数；固定加值可为负；总伤害最低为 0。",label);
            if(GUILayout.Button("应用",button)) ApplyMonsterSettings();
            GUILayout.Label(inputError,label);
        }
        GUILayout.EndArea();
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
    }    public void QuitGame()
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
