using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly WaterWalkEffect waterWalk=new WaterWalkEffect();
    readonly HashSet<int> waterWalkTargets=new HashSet<int>{0};
    bool waterWalkWilling;
    void InitializeWaterWalk()
    {
        map.WaterWalking=actor=>waterWalk.Active(actor,worldRounds);
        map.WaterWalkBonus=UseWaterWalkBonus;
    }
    bool UseWaterWalkBonus(int actor)
    {
        if(!waterWalk.Active(actor,worldRounds))return false;
        if(actor==0){if(!PlayerTurnAllowed||summonTurns.Active)return false;return combat.TryUseAftermathBonusAction();}
        if(actor==-1)return terrainActor==-1&&combat.TryUseMonsterBonusAction();
        if(actor==BattleMap.RescueId)return RescueTurnAllowed&&rescueCarrier.TryUseBonusAction();
        var unit=FindSummon(actor);
        return unit!=null&&(summonTurns.Current==unit||!summonTurns.Active&&ControlledSteed&&unit==steed)&&unit.TryUseBonus();
    }
    string WaterLayerCost(int actor)=>waterWalk.Active(actor,worldRounds)?"附赠动作":"5尺移动力";
    bool PlayerCanSwitchWaterLayer()
    {
        if(!PlayerTurnAllowed||summonTurns.Active||!combat.PlayerMovementReady||map.IsContained(true)||map.Riding)return false;
        return waterWalk.Active(0,worldRounds)?!combat.PlayerFeignDeath&&combat.BonusActionAvailable:map.MovementRemaining>=AquaticState.SwitchCost;
    }
    bool WaterWalkTarget(int actor)
    {
        Vector2Int p;int n;string name;
        if(!HealingHandsTarget(actor,out p,out n,out name)||BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n)>30)return false;
        if(actor!=0&&map.IsContained(true))return false;
        if((actor==0||actor==-1)&&map.IsContained(actor==0)&&actor!=0)return false;
        return true;
    }
    string WaterWalkError()
    {
        if(!waterWalkWilling)return "请确认所有选中的生物自愿接受水面行走。";
        if(waterWalkTargets.Count<1||waterWalkTargets.Count>10)return "请选择1～10名不同生物。";
        foreach(int actor in waterWalkTargets)if(!WaterWalkTarget(actor))return TransitActorName(actor)+"须存活且在30尺可触及范围。";
        return worldRounds<0||worldRounds>long.MaxValue-600?"世界时间超出范围。":"";
    }
    void DrawWaterWalkChoices()
    {
        waterWalkTargets.RemoveWhere(id=>{Vector2Int p;int n;string name;return !HealingHandsTarget(id,out p,out n,out name);});
        foreach(int actor in BreathActors()){
            Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))continue;
            bool selected=GUILayout.Toggle(waterWalkTargets.Contains(actor),name+(WaterWalkTarget(actor)?"":"（超出合法范围）"),label);
            if(selected)waterWalkTargets.Add(actor);else waterWalkTargets.Remove(actor);
        }
        waterWalkWilling=GUILayout.Toggle(waterWalkWilling,"确认所有选中目标自愿",label);
        GUILayout.Label("非专注1小时，至多10名目标。水面移动如陆地，潜水仍需游泳；主动切换水面／潜水消耗附赠动作，不再消耗5尺移动。被推拉或跳跃从陆地落水时直接潜水，普通行走与传送保持水面。不会自动上浮，也不提供水下呼吸。",small);
    }
    void CastWaterWalk(IEnumerable<int> selected)
    {
        var legal=new List<int>();foreach(int actor in selected)if(WaterWalkTarget(actor))legal.Add(actor);
        if(!waterWalk.Begin(legal,worldRounds)){history.Insert(0,"水面行走的目标已失效，施法动作已消耗。");return;}
        ObserveWaterBreathing();SyncTimedBuffs();
        history.Insert(0,"水面行走："+legal.Count+"名自愿生物获得1小时液面行走。当前潜水者保留水层，须用附赠动作上浮；该法术不赋予水下呼吸。");
    }
    void DrawRescueWaterLayer()
    {
        if(!map.RescuePosition.HasValue)return;
        bool old=GUI.enabled;int cells=map.WaterCells(map.RescuePosition.Value,map.RescueSpaces);
        GUI.enabled=old&&!rolling&&RescueTurnAllowed&&rescueCarrier.MovementReady&&(!waterWalk.Active(BattleMap.RescueId,worldRounds)||rescueCarrier.Conscious)&&rescueCarrier.TurnActive&&(map.RescueAquatic.Submerged||AquaticState.FullyInWater(cells,map.RescueSpaces*map.RescueSpaces));
        if(cells>0&&GUILayout.Button((map.RescueAquatic.Submerged?"浮出水面 · ":"潜入水下 · ")+WaterLayerCost(BattleMap.RescueId),button)){
            if(map.SwitchRescueWaterState(!map.RescueAquatic.Submerged,rescueCarrier.Move)){ObserveWaterBreathing();rescueMessage="救援者切换水层，消耗"+WaterLayerCost(BattleMap.RescueId)+"。";}
        }
        GUI.enabled=old;
    }
}
