using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly BeaconHopeEffect beaconHope=new BeaconHopeEffect();
    readonly HashSet<int> beaconTargets=new HashSet<int>{0};
    bool BeaconHope(int actor)=>beaconHope.Active(actor,worldRounds);
    void EndBeaconHope(){beaconHope.Clear();}
    int RollHealingAmount(int actor,int rolled,long maximum)
    {
        int amount=BeaconHopeEffect.ResolveHealing(BeaconHope(actor),rolled,maximum);
        if(BeaconHope(actor))history.Insert(0,"希望信标："+TransitActorName(actor)+"本次治疗按最大值 "+amount+" HP计算，仍受生命上限与治疗封锁限制。");
        return amount;
    }
    bool BeaconHopeTarget(int actor)
    {
        Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))return false;
        if(actor==0)return true;
        if(actor==-1&&map.IsContained(false))return false;
        if(map.IsContained(true)&&actor!=-1)return false;
        return BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n)<=30;
    }
    string BeaconHopeError()
    {
        if(worldRounds<0||worldRounds>long.MaxValue-BeaconHopeEffect.Duration)return "世界时间超出范围。";
        foreach(int actor in beaconTargets)if(!BeaconHopeTarget(actor))return TransitActorName(actor)+"须存活且在30尺范围内。";
        return "";
    }
    void DrawBeaconHopeChoices()
    {
        beaconTargets.RemoveWhere(actor=>{Vector2Int p;int n;string name;return !HealingHandsTarget(actor,out p,out n,out name);});
        foreach(int actor in BreathActors()){
            Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))continue;
            bool selected=GUILayout.Toggle(beaconTargets.Contains(actor),name+(BeaconHopeTarget(actor)?"":"（超出范围）"),label);
            if(selected)beaconTargets.Add(actor);else beaconTargets.Remove(actor);
        }
        GUILayout.Label("选择30尺内任意数量生物，不需要可见或自愿。专注至多1分钟：感知豁免与死亡豁免具有优势，受到的治疗按最大可能值恢复。离开30尺后效果仍持续；升环不增强。不会使固定治疗或临时生命变成回满。",small);
    }
    void CastBeaconHope(IEnumerable<int> selected)
    {
        var legal=new List<int>();foreach(int actor in selected)if(BeaconHopeTarget(actor))legal.Add(actor);
        if(!beaconHope.Begin(legal,worldRounds))return;
        concentrationSpell="希望信标";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=beaconHope.Expires;
        history.Insert(0,"希望信标："+legal.Count+"名生物的感知与死亡豁免具有优势，骰子治疗取最大值；专注至多1分钟。");
    }
}
