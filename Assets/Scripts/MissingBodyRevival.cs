using System;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly Dictionary<int,Action> undeadEquipmentReset=new Dictionary<int,Action>();
    readonly string[] replacementColumns={"1","2","3"},replacementRows={"1","1","1"};
    int ReplacementIndex(int actor)=>actor==0?0:actor==-1?1:2;
    bool ReplacementPoint(int actor,out Vector2Int cell)
    {
        int index=ReplacementIndex(actor),x,y;bool valid=int.TryParse(replacementColumns[index],out x)&int.TryParse(replacementRows[index],out y);cell=new Vector2Int(x-1,y-1);return valid&&map.Inside(cell,1);
    }
    string MissingBodyError(string spell,int actor,bool external)
    {
        if(map.BodyPresent(actor))return "";
        if(spell!="True Resurrection")return "原遗体已转化为独立亡灵；此法术需要原遗体，不能重复使用同一身体。";
        Vector2Int cell;if(!ReplacementPoint(actor,out cell))return "请为完全复生术选择地图内的新身体落点。";
        int n=actor==0?map.PlayerSpaces:actor==-1?map.MonsterSpaces:map.RescueSpaces;
        if(!external&&BattleMap.BodyDistance(map.Player,map.PlayerSpaces,cell,n)>10)return "新身体必须出现在玩家施法者10尺内。";
        return map.CanRestoreBodyAt(actor,cell,n)?"":"新身体完整占地被占用或会阻挡恢复原形，请更换落点。";
    }
    void DrawMissingBodyChoices()
    {
        foreach(int actor in new[]{0,-1,BattleMap.RescueId})if(!map.BodyPresent(actor)){
            int index=ReplacementIndex(actor);GUILayout.Label(TransitActorName(actor)+"原遗体已消耗 · 完全复生术新身体列／行",small);
            replacementColumns[index]=IntegerField(replacementColumns[index],false);replacementRows[index]=IntegerField(replacementRows[index],false);
        }
        if(!map.BodyPresent(0)||!map.BodyPresent(-1)||!map.BodyPresent(BattleMap.RescueId))GUILayout.Label("新身体不复制原装备；玩家施法须在10尺内。外部施法者不占格，沿用可接触目标的救援抽象，在所选新身体旁完成施法。",small);
    }
    void SaveUndeadEquipmentReset(int actor)
    {
        if(actor!=0&&actor!=-1&&actor!=BattleMap.RescueId||undeadEquipmentReset.ContainsKey(actor))return;
        var weight=CurrentBodyWeight(actor);var snapshot=weight.Copy();var armor=actor==0?playerArmor:actor==-1?monsterArmor:null;
        int index=armor==null?0:armor.ArmorIndex,ac=armor==null?10:armor.CustomAc;bool shield=armor!=null&&armor.Shield,custom=armor!=null&&armor.Custom;
        undeadEquipmentReset[actor]=()=>{weight.TryApply(snapshot.BodyPounds.Value.ToString(),snapshot.EquipmentPounds.ToString());weight.SetStoredGear(0);weight.SetArmorWeight(snapshot.ArmorPounds);weight.SetWeaponWeight(snapshot.WeaponOunces);weight.SetStoredGear(snapshot.StoredGearOunces);if(armor!=null)armor.Apply(index,shield,custom,ac);};
    }
    bool RestoreMissingBody(int actor,bool external)
    {
        if(map.BodyPresent(actor))return true;
        if(MissingBodyError("True Resurrection",actor,external)!="")return false;
        Vector2Int cell;ReplacementPoint(actor,out cell);int n=actor==0?map.PlayerSpaces:actor==-1?map.MonsterSpaces:map.RescueSpaces;
        if(!map.RestoreBodyAt(actor,cell,n))return false;
        var weight=CurrentBodyWeight(actor);weight.SetStoredGear(0);weight.SetWeaponWeight(0);weight.SetArmorWeight(0);weight.TryApply(weight.BodyPounds.Value.ToString(),"0");
        if(actor==0){playerArmor.Apply(0,false,false);weaponGrip.DropHeld();}else if(actor==-1)monsterArmor.Apply(0,false,false);
        return true;
    }
}
