using System;
using UnityEngine;
[Serializable] public sealed class ClassFeatureRecord {public int classIndex,level;public string name,source,status;}
[Serializable] public sealed class ClassFeatureRecords {public ClassFeatureRecord[] entries;}
public sealed partial class TrainingGame
{
    readonly int[] subclassChoices=System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Repeat(-1,PlayerClasses.Names.Length));
    ClassFeatureRecords featureRecords;
    Vector2 featureScroll;
    int catalogClassIndex;
    bool alarmLearned;
    void DrawFeatureCatalog()
    {
        if(featureRecords==null)featureRecords=JsonUtility.FromJson<ClassFeatureRecords>(Resources.Load<TextAsset>("Rules2024/ClassFeatures").text);
        Surface(new Rect(350,110,480,745),paperTexture);
        if(Btn(new Rect(720,125,90,46),"关闭")){panel="classes";return;}
        GUILayout.BeginArea(new Rect(375,185,430,635));GUILayout.Label("2024 职业特性目录",heading);
        GUILayout.Label("列出等级与来源。待接入条目不提供自动结算效果。",label);
        catalogClassIndex=GUILayout.SelectionGrid(catalogClassIndex,PlayerClasses.Names,3,new GUIStyle(button){fontSize=15,fixedHeight=32});
        if(GUILayout.Button("查看官方免费野兽资料（效果待接入）",button)){panel="beasts";}
        if(catalogClassIndex==11&&GUILayout.Button("荒野形态与已知形态",button))panel="druid";
        featureScroll=GUILayout.BeginScrollView(featureScroll);
        foreach(var feature in featureRecords.entries)if(feature.classIndex==catalogClassIndex){
            string status=FeatureStatus(feature);
            GUILayout.Label("Lv "+feature.level+" · "+feature.name+"\n"+(classes[catalogClassIndex]>=feature.level?"等级满足":"未解锁")+" · "+status,label);
            GUILayout.Space(9);
        }
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
    static string FeatureStatus(ClassFeatureRecord feature)
    {
        if(feature.classIndex==0 && (feature.name=="Second Wind"||feature.name.StartsWith("Action Surge")||feature.name.Contains("Extra Attack")||feature.name=="Tactical Shift"||feature.name=="Studied Attacks"))return "已接入";
        if(feature.classIndex==11&&feature.name=="Wild Shape")return "部分接入：10种地面战斗形态";
        if(feature.classIndex==11&&feature.name=="Beast Spells")return "已接入：18级解除兽形施法限制";
        if(feature.classIndex==0&&feature.name=="Tactical Mind")return "已接入：手动检定失败时选择";
        if(feature.name=="Expertise"||feature.name=="Scholar")return "已接入：技能创建选择";
        if(feature.name=="Jack of All Trades"||feature.name=="Reliable Talent")return "已接入：手动技能检定";
        if(feature.classIndex==2&&feature.name=="Deft Explorer")return "部分接入：专精；语言待接入";
        if(feature.name=="Spellcasting")return "部分接入：现有法术";
        return "待接入：说明索引，无自动效果";
    }
    bool ValidateSubclassDraft()
    {
        for(int i=0;i<PlayerClasses.Names.Length;i++)if(int.TryParse(classInputs[i],out int level)&&level>=3&&(subclassChoices[i]<0||subclassChoices[i]>=Subclasses2024.Names[i].Length))return false;
        return true;
    }
    void DrawSubclassChoice(int index)
    {
        int.TryParse(classInputs[index],out int level);
        GUILayout.Label(level>=3?"子职业（仅选择记录，特性效果待接入）":"子职业：该职业 3 级解锁",small);
        bool previous=GUI.enabled;GUI.enabled=previous&&level>=3;
        for(int choice=0;choice<4;choice++)if(GUILayout.Button((subclassChoices[index]==choice?"✓ ":"")+Subclasses2024.Names[index][choice],new GUIStyle(button){fontSize=16,fixedHeight=32}))subclassChoices[index]=choice;
        GUI.enabled=previous;
    }
}
