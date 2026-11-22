using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly FreedomMovementEffect freedomMovement=new FreedomMovementEffect();
    readonly HashSet<int> freedomTargets=new HashSet<int>{0};
    bool freedomWilling;
    bool MovementFreedom(int actor)=>freedomMovement.Active(actor,worldRounds);
    bool MagicalRestraint(int actor)=>!MovementFreedom(actor)&&(entangle.Restrained(actor,worldRounds)||web.Restrained(actor,worldRounds));
    void InitializeFreedomMovement()
    {
        map.MovementFreedom=MovementFreedom;
        combat.MovementFreedom=player=>MovementFreedom(player?0:-1);
        rescueCarrier.MovementFreedom=()=>MovementFreedom(BattleMap.RescueId);
        var summonAdded=map.SummonAdded;
        map.SummonAdded=unit=>{summonAdded?.Invoke(unit);unit.MovementFreedom=()=>MovementFreedom(unit.Id);};
    }
    bool FreedomTarget(int actor)
    {
        Vector2Int p;int n;string name;
        return HealingHandsTarget(actor,out p,out n,out name)&&HealingHandsCanTouch(0,actor);
    }
    string FreedomMovementError(int slot)
    {
        if(!freedomWilling)return "请确认所有选中生物自愿接受行动自如。";
        int limit=FreedomMovementEffect.TargetLimit(slot);
        if(limit==0||freedomTargets.Count<1||freedomTargets.Count>limit)return "请选择1～"+limit+"名不同生物。";
        foreach(int actor in freedomTargets)if(!FreedomTarget(actor))return TransitActorName(actor)+"须存活且在5尺接触范围内。";
        return worldRounds<0||worldRounds>long.MaxValue-FreedomMovementEffect.Duration?"世界时间超出范围。":"";
    }
    void DrawFreedomMovementChoices()
    {
        freedomTargets.RemoveWhere(id=>{Vector2Int p;int n;string name;return !HealingHandsTarget(id,out p,out n,out name);});
        GUILayout.Label("本次至多"+FreedomMovementEffect.TargetLimit(directSlotLevel)+"名自愿生物，须在5尺接触范围内。",small);
        foreach(int actor in BreathActors()){
            Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))continue;
            bool selected=GUILayout.Toggle(freedomTargets.Contains(actor),name+(FreedomTarget(actor)?"":"（超出接触范围）"),label);
            if(selected)freedomTargets.Add(actor);else freedomTargets.Remove(actor);
        }
        freedomWilling=GUILayout.Toggle(freedomWilling,"确认所有选中目标自愿",label);
        GUILayout.Label("非专注1小时：忽略困难地形；魔法不能降低速度或造成麻痹、束缚；获得等于速度的游泳速度。已有魔法状态的持续时间继续流逝，受保护期间不生效。不会提供水下呼吸，也不会消除负重、爬行或力竭的移动限制。不能花5尺移动逃出吞咽。",small);
    }
    void CastFreedomMovement(IEnumerable<int> selected,int slot)
    {
        var legal=new List<int>();foreach(int actor in selected)if(FreedomTarget(actor))legal.Add(actor);
        if(!freedomMovement.Begin(legal,slot,worldRounds)){history.Insert(0,"行动自如的目标已失效，施法动作已消耗。");return;}
        SyncTimedBuffs();RefreshConcentration();
        history.Insert(0,"行动自如："+legal.Count+"名自愿生物受到1小时保护，忽略困难地形、魔法减速、麻痹与束缚，并获得游泳速度；吞咽不会自动解除。");
    }
    int SummonSwimSpeed(SummonedUnit unit)=>MovementFreedom(unit.Id)?Mathf.Max(unit.SwimSpeed,unit.EffectiveSpeed):unit.SwimSpeed;
}
