using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly PassWithoutTraceEffect passTrace=new PassWithoutTraceEffect();
    readonly HashSet<int> passTraceTargets=new HashSet<int>();
    bool PassTraceBenefit(int actor)=>passTrace.Protects(actor,BeaconHopeTarget(actor),worldRounds);
    string PassTraceError()=>worldRounds<0||worldRounds>long.MaxValue-PassWithoutTraceEffect.Duration?"世界时间超出范围。":"";
    void EndPassTrace(){passTrace.Clear();}
    void DrawPassTraceChoices()
    {
        GUILayout.Label("自身始终受益；可选其他生物。受益者须处于随玩家移动的30尺光环内，敏捷（隐匿）检定+10，离开暂停、返回恢复。专注至多1小时，升环不增强。不会自动隐形或躲藏；不留踪迹的环境效果保留说明。",small);
        passTraceTargets.RemoveWhere(actor=>{Vector2Int p;int n;string name;return !HealingHandsTarget(actor,out p,out n,out name);});
        foreach(int actor in BreathActors())if(actor!=0){
            bool selected=GUILayout.Toggle(passTraceTargets.Contains(actor),TransitActorName(actor)+(BeaconHopeTarget(actor)?"":"（当前在光环外）"),label);
            if(selected)passTraceTargets.Add(actor);else passTraceTargets.Remove(actor);
        }
    }
    void CastPassTrace(IEnumerable<int> targets)
    {
        if(!passTrace.Begin(targets,worldRounds))return;
        concentrationSpell="无踪步";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=passTrace.Expires;
        history.Insert(0,"无踪步：自身及选中生物在30尺光环内敏捷（隐匿）检定+10，专注至多1小时。吞咽隔离内外，不要求可见；不自动躲藏或隐形。");
    }
}
