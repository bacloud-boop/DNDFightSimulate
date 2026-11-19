using System;
using UnityEngine;
public sealed partial class TrainingGame
{
    int playerArmorDraft,monsterArmorDraft;
    bool playerShieldDraft,monsterShieldDraft,monsterCustomDraft=true,protectorDraft,wardenDraft;
    ArmorTraining monsterTrainingDraft=ArmorTraining.Light|ArmorTraining.Medium|ArmorTraining.Heavy|ArmorTraining.Shield;
    Vector2 armorScroll;
    void DrawArmorPanel(bool player)
    {
        Surface(new Rect(player?350:770,110,480,745),paperTexture);float x=player?350:770;
        if(Btn(new Rect(x+370,125,90,46),"关闭")){panel="";return;}
        GUILayout.BeginArea(new Rect(x+20,185,440,640));armorScroll=GUILayout.BeginScrollView(armorScroll);
        GUILayout.Label(player?"玩家护甲与盾牌":"怪物护甲与盾牌",heading);
        GUILayout.Label("仅首次有效操作前可配置。无甲按10+敏捷计算；怪物可保留自定义／天生AC。",small);
        GUILayout.Label("多个基础AC公式自动取最高合法值，不相加；盾牌等合法加值另算。",small);
        bool enabled=GUI.enabled;GUI.enabled=enabled&&!combat.BattleStarted;
        if(!player)monsterCustomDraft=GUILayout.Toggle(monsterCustomDraft,"使用怪物属性中的自定义／天生护甲",label);
        if(player||!monsterCustomDraft)for(int i=0;i<ArmorDefinition.All.Length;i++){
            var item=ArmorDefinition.All[i];int chosen=player?playerArmorDraft:monsterArmorDraft;
            if(GUILayout.Button((chosen==i?"✓ ":"")+item.Name+" · "+item.Weight+"磅"+(item.Strength>0?" · 力量"+item.Strength:"")+(item.StealthDisadvantage?" · 隐匿劣势":""),button)){if(player)playerArmorDraft=i;else monsterArmorDraft=i;}
        }
        if(player)playerShieldDraft=GUILayout.Toggle(playerShieldDraft,"持用盾牌（6磅，受训时AC+2）",label);
        else monsterShieldDraft=GUILayout.Toggle(monsterShieldDraft,"持用盾牌（6磅，受训时AC+2）",label);
        if(player){
            if(classes[6]>0)protectorDraft=GUILayout.Toggle(protectorDraft,"牧师圣职：保护者（本模块接入重甲训练）",label);
            if(classes[11]>0)wardenDraft=GUILayout.Toggle(wardenDraft,"德鲁伊自然职能：守望者（本模块接入中甲训练）",label);
        }else foreach(ArmorTraining category in new[]{ArmorTraining.Light,ArmorTraining.Medium,ArmorTraining.Heavy,ArmorTraining.Shield}){
            bool chosen=GUILayout.Toggle((monsterTrainingDraft&category)!=0,"训练："+ArmorTrainingName(category),label);if(chosen)monsterTrainingDraft|=category;else monsterTrainingDraft&=~category;
        }
        if(GUILayout.Button("应用护甲配置",button)){
            if(!ArmorDraftFits(player)){history.Insert(0,"拒绝护甲配置：装备总重超过携带上限，原配置未变。");}
            else {
            if(player){playerArmor.Apply(playerArmorDraft,playerShieldDraft,false);clericProtector=protectorDraft;druidWarden=wardenDraft;}
            else {monsterArmor.Apply(monsterArmorDraft,monsterShieldDraft,monsterCustomDraft,combat.MonsterBaseArmorClass);monsterArmorTraining=monsterTrainingDraft;}
            SyncTimedBuffs();history.Insert(0,(player?"玩家":"木桩")+"护甲配置已应用；生命值不变。");
            }
        }
        GUI.enabled=enabled;
        var current=player?playerArmor:monsterArmor;ArmorTraining training=player?PlayerArmorTraining:monsterArmorTraining;
        GUILayout.Label("当前："+(current.Custom?"自定义／天生护甲":current.Armor.Name)+(current.Shield?" + 盾牌":"")+"\nAC "+(player?combat.PlayerArmorClass:combat.MonsterArmorClass)+" · 自动装备重量 "+current.Weight+"磅",small);
        if(current.Untrained(training,player?PlayerArmorMelded:MonsterPolymorphed))GUILayout.Label("未受训护甲：涉及力量／敏捷的d20检定劣势，不能施法。",small);
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
    static string ArmorTrainingName(ArmorTraining training)=>training==ArmorTraining.Light?"轻甲":training==ArmorTraining.Medium?"中甲":training==ArmorTraining.Heavy?"重甲":"盾牌";
}
