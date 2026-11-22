using System;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    SpeciesProfile playerSpecies=SpeciesProfile.Human(),monsterSpecies=new SpeciesProfile();
    SpeciesProfile playerSpeciesDraft,monsterSpeciesDraft;
    string speciesMessage="";Vector2 speciesScroll;
    readonly Dictionary<SpeciesProfile,string> speciesLevelText=new Dictionary<SpeciesProfile,string>();
    bool ValidateSpeciesDraft(SpeciesProfile p){string text;int n;if(speciesLevelText.TryGetValue(p,out text)){if(!int.TryParse(text,out n)||n<1||n>20)return false;p.Level=n;}return p.Valid();}
    readonly HashSet<int> playerExpertiseChoices=new HashSet<int>();
    int SpeciesLevel(bool player)=>player?classes.TotalLevel:monsterSpecies.Level;
    SpeciesProfile Species(bool player)=>player?playerSpecies:monsterSpecies;
    bool SpeciesPhysicalActive(bool player)=>!(player?PlayerInBeastForm:MonsterPolymorphed);
    int NaturalPlayerSize=>playerSpecies.BodySize;
    int NaturalPlayerSpeed=>playerSpecies.Speed;
    int CarryingCreatureSize(bool player){int size=EffectiveCreatureSize(player);return SpeciesPhysicalActive(player)?Species(player).CapacitySize(size):size;}
    bool SpeciesResistance(bool player,int type)=>SpeciesPhysicalActive(player)&&Species(player).Resist(type);
    bool SpeciesSaveAdvantage(bool player,int ability,bool poison=false,bool charm=false,bool fear=false)=>SpeciesPhysicalActive(player)&&Species(player).SaveAdvantage(ability,poison,charm,fear);
    int EffectivePlayerSkillTier(int id)
    {
        if(id<0||id>=55)return 0;
        int tier=playerSkillTiers[id];foreach(int granted in playerSpecies.Proficiencies())if(granted==id)tier=Math.Max(1,tier);
        return tier>0&&playerExpertiseChoices.Contains(id)?2:tier;
    }
    int EffectiveMonsterSkillTier(int id){int tier=monsterSkillTiers[id];if(!MonsterPolymorphed)foreach(int granted in monsterSpecies.Proficiencies())if(granted==id)tier=Math.Max(1,tier);return tier;}
    void ApplySpeciesHp(bool player,bool refill)
    {
        combat.SetSpeciesHpBonus(player,Species(player).HpBonus(SpeciesLevel(player)),refill);
    }
    bool SpeciesEquipmentFits(bool player,SpeciesProfile draft)
    {
        var weight=player?playerWeight.Weight:monsterWeight.Weight;long gear=weight.GearOunces;
        return EquipmentLoadRules.AllowedOunces(player?attributes[0]:monsterAttributes[0],draft.CapacitySize(draft.Kind==Species2024.None&&!player?monsterProfile.SizeIndex:draft.BodySize),gear);
    }
    void BeginSpeciesDraft(bool player){var old=player?playerSpeciesDraft:monsterSpeciesDraft;if(old!=null)speciesLevelText.Remove(old);if(player)playerSpeciesDraft=playerSpecies.Copy();else monsterSpeciesDraft=monsterSpecies.Copy();}
    void DrawSpeciesConfiguration(SpeciesProfile p,bool player,bool showMonsterPriority=true,bool fixedKind=false,bool editLevel=true)
    {
        if(!fixedKind){int index=GUILayout.SelectionGrid((int)p.Kind+1,new[]{"无种族特性"}.ConcatNames(SpeciesProfile.Names),3,new GUIStyle(button){fontSize=15,fixedHeight=34})-1;
        if(index!=(int)p.Kind){p.Kind=(Species2024)index;p.Lineage=0;p.ChosenSkill=p.Kind==Species2024.Elf?12:0;}}else GUILayout.Label("新种族："+p.Name,heading);
        if(!player&&editLevel){GUILayout.Label("种族特性等级（1～20，独立于施法等级）",small);string text;if(!speciesLevelText.TryGetValue(p,out text))text=p.Level.ToString();speciesLevelText[p]=IntegerField(text,false);}
        if(!player&&showMonsterPriority){GUILayout.Label("自动使用与优先级在怪物法术面板中统一设置。",small);GUILayout.Label("当前支持龙裔吐息、阿斯莫治愈之手、兽人疾走与矮人石中精妙；动作和附赠动作各自共用额度。",small);}
        if(p.Kind==Species2024.None)return;
        if(p.SizeChoice)p.Size=GUILayout.SelectionGrid(p.Size-1,new[]{"小型","中型"},2,button)+1;
        if(p.Lineages.Length>1)p.Lineage=GUILayout.SelectionGrid(p.Lineage,p.Lineages,3,new GUIStyle(button){fontSize=15,fixedHeight=34});
        GUILayout.Label("种族基础："+CreatureProfile.SizeNames[p.BodySize]+" · 速度 "+p.Speed+"尺 · 黑暗视觉 "+p.Darkvision+"尺",small);
        if(p.Kind==Species2024.Elf||p.Kind==Species2024.Gnome||p.Kind==Species2024.Tiefling){int[] abilities={4,3,5};int chosen=Array.IndexOf(abilities,p.SpellAbility);p.SpellAbility=abilities[GUILayout.SelectionGrid(chosen,new[]{"智力施法","感知施法","魅力施法"},3,new GUIStyle(button){fontSize=15,fixedHeight=34})];GUILayout.Label("玩家已实现的血统法术可从法术书选择种族来源；未实现的法术保留待接入状态。",small);}
        DrawHighElfCantrip(p);
        if(p.Kind==Species2024.Human||p.Kind==Species2024.Elf){int[] options=p.Kind==Species2024.Elf?new[]{10,12,13}:SkillCreationRules.Range(0,18);string[] names=Array.ConvertAll(options,id=>SkillCheckRules.Names[id]);int chosen=Array.IndexOf(options,p.ChosenSkill);int next=GUILayout.SelectionGrid(chosen,names,3,new GUIStyle(button){fontSize=15,fixedHeight=32});if(next>=0)p.ChosenSkill=options[next];}
        if(p.Kind==Species2024.Human){GUILayout.Label("人类额外起源专长",heading);p.OriginFeat=GUILayout.SelectionGrid(p.OriginFeat,SpeciesProfile.OriginFeats,3,new GUIStyle(button){fontSize=15,fixedHeight=32});GUILayout.Label("本阶段已生效：熟习的熟练选择、健壮的HP；其他专长效果及英雄激励仍在接入。",small);
            if(p.OriginFeat==7)for(int i=0;i<3;i++){GUILayout.Label("熟习选择 "+(i+1)+"："+SkillCreationRules.Name(p.SkilledChoices[i]),small);p.SkilledChoices[i]=GUILayout.SelectionGrid(p.SkilledChoices[i],Array.ConvertAll(SkillCreationRules.Range(0,55),id=>SkillCreationRules.Name(id)),3,new GUIStyle(button){fontSize=14,fixedHeight=30});}}
    }
    void DrawSpeciesPanel(bool player)
    {
        if(player?playerSpeciesDraft==null:monsterSpeciesDraft==null)BeginSpeciesDraft(player);
        var draft=player?playerSpeciesDraft:monsterSpeciesDraft;
        Surface(new Rect(player?350:770,110,480,745),paperTexture);float x=player?350:770;
        if(Btn(new Rect(x+370,125,90,46),"关闭")){panel="";return;}
        GUILayout.BeginArea(new Rect(x+20,185,440,635));speciesScroll=GUILayout.BeginScrollView(speciesScroll);
        GUILayout.Label(player?"玩家种族":"怪物种族",heading);bool enabled=GUI.enabled;GUI.enabled=!combat.BattleStarted&&!rolling;
        DrawSpeciesConfiguration(draft,player);
        GUILayout.Label("已接入被动数值及治愈之手、兽人疾走、矮人石中精妙；其他主动特性与部分专长仍在实现。首次操作后锁定配置。",small);
        if(GUILayout.Button("应用种族资料",button)){
            if(!ValidateSpeciesDraft(draft))speciesMessage="种族选择无效，熟习选项不可重复。";
            else if(!SpeciesEquipmentFits(player,draft))speciesMessage="拒绝配置：所选体型／负重特性无法携带现有装备。";
            else {if(player){playerSpecies=draft.Copy();playerSpeciesAbilities.Reset();playerSpeciesSpellUses.LongRest();skillDirty=true;}else{monsterSpecies=draft.Copy();monsterSpeciesAbilities.Reset();if(draft.Kind!=Species2024.None){if(map.MonsterSpaces!=CreatureProfile.SpacesForSize(draft.BodySize)){map.Unlock();map.TrySetFootprint(false,CreatureProfile.SpacesForSize(draft.BodySize));page="deployment";deploymentMessage="种族改变了木桩占地，请重新确认初始位置。";}monsterProfile.TryApply(9,draft.BodySize);monsterTypeDraft=9;monsterSizeDraft=draft.BodySize;combat.UpdateMonsterBaseSpeed(draft.Speed);monsterSpeedInput=draft.Speed.ToString();}}
                ApplySpeciesHp(player,true);SyncTimedBuffs();speciesMessage="已应用种族被动特性；技能面板按当前种族动态合并熟练。";}
        }
        GUI.enabled=enabled;GUILayout.Label(speciesMessage,small);GUILayout.EndScrollView();GUILayout.EndArea();
    }
}
public static class SpeciesNameArray
{
    public static string[] ConcatNames(this string[] first,string[] second){var result=new string[first.Length+second.Length];Array.Copy(first,result,first.Length);Array.Copy(second,0,result,first.Length,second.Length);return result;}
}
