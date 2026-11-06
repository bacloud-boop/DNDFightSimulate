using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class TrainingGame : MonoBehaviour
{
    readonly CombatState combat = new CombatState();
    readonly List<string> history = new List<string>();
    string page = "home", dice = "D12 · 待投掷";
    bool rolling;
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
    public void Attack() { if(!rolling) StartCoroutine(Roll()); }
    IEnumerator Roll()
    {
        rolling=true;
        for(int i=0;i<9;i++) { dice="滚动中 · "+Random.Range(1,13); PlayRoll(); yield return new WaitForSecondsRealtime(.10f+i*.012f); }
        int result=Random.Range(1,13); combat.ApplyDamage(result); dice="D12 → "+result;
        history.Insert(0,"投掷结果 "+result+"，本次伤害 "+result+"。");
        if(history.Count>5) history.RemoveAt(5); rolling=false;
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
            GUILayout.Label("训练战斗",title); GUILayout.Label("训练木桩 · 仅被动承伤",heading);
            GUILayout.Label(combat.Hp+" / "+CombatState.MaxHp+"     第 "+combat.Round+" 回合",label);
            Rect bar=GUILayoutUtility.GetRect(900,20); GUI.Box(bar,GUIContent.none);
            GUI.Box(new Rect(bar.x,bar.y,bar.width*combat.Hp/CombatState.MaxHp,bar.height),GUIContent.none);
            GUILayout.Label(dice,heading); GUI.enabled=!rolling;
            if(GUILayout.Button("攻击 · 投掷 D12",button)) Attack();
            if(GUILayout.Button("结束回合",button)) {combat.EndRound();dice="D12 · 待投掷";history.Clear();history.Add("回合结束，木桩恢复至 1000。");}
            if(GUILayout.Button("← 返回主界面",button)) page="home";
            GUI.enabled=true; foreach(string line in history) GUILayout.Label(line,label);
        }
        GUILayout.EndArea();
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
