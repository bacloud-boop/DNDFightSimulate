using System;
using UnityEngine;

[Serializable] public sealed class BeastRecord
{
    public string name,size,challengeLabel,source;
    public float challenge;
    public int armor,hp,walk,climb,swim,fly,burrow;
    // Same order as player attributes: Str, Dex, Con, Wis, Int, Cha.
    public int[] abilities,saves,skillModifiers;
    public string[] actionNames;
    public bool EligibleForBaseWildShape(int druidLevel)
    {
        if(druidLevel<2 || fly>0)return false;
        return challenge<=(druidLevel>=8?1f:druidLevel>=4?.5f:.25f);
    }
}
[Serializable] public sealed class BeastRecords {public string description;public BeastRecord[] entries;}

public sealed partial class TrainingGame
{
    BeastRecords beastRecords;
    Vector2 beastScroll;
    bool eligibleBeastsOnly;
    void DrawBeastCatalog()
    {
        if(beastRecords==null)beastRecords=JsonUtility.FromJson<BeastRecords>(Resources.Load<TextAsset>("Rules2024/Beasts").text);
        Surface(new Rect(350,110,480,745),paperTexture);
        if(Btn(new Rect(720,125,90,46),"关闭")){panel="features";return;}
        GUILayout.BeginArea(new Rect(375,185,430,635));
        GUILayout.Label("官方免费野兽资料",heading);
        GUILayout.Label("63种，CR≤1。10种已接入当前地面战斗的荒野形态；其他动作与召唤控制待接入。",label);
        eligibleBeastsOnly=GUILayout.Toggle(eligibleBeastsOnly,"仅显示符合当前等级基础荒野形态限制的资料",label);
        GUILayout.Label("飞行暂缓。此筛选不包括月亮结社的额外许可。",small);
        beastScroll=GUILayout.BeginScrollView(beastScroll);
        foreach(var beast in beastRecords.entries){
            if(eligibleBeastsOnly&&!beast.EligibleForBaseWildShape(classes[11]))continue;
            GUILayout.Label(beast.name+" · "+beast.size+" · CR "+beast.challengeLabel,heading);
            GUILayout.Label("护甲 "+beast.armor+" · 生命 "+beast.hp+" · 步行 "+beast.walk+" 尺"+(beast.fly>0?" · 飞行暂缓":""),label);
            string values="";for(int i=0;i<6;i++)values+=PlayerAttributes.Names[i]+" "+beast.abilities[i]+(i==2?"\n":"  ");
            GUILayout.Label(values,small);
            GUILayout.Label("攀爬 "+beast.climb+" · 游泳 "+beast.swim+" · 掘穴 "+beast.burrow+" 尺",small);
            GUILayout.Label("特性/动作索引："+string.Join("、",beast.actionNames)+(BeastAttack.Find(beast.name)!=null?"（当前地面战斗可用）":"（效果待接入）"),small);
            GUILayout.Space(15);
        }
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
}
