using UnityEngine;
public sealed partial class TrainingGame
{
    // All currently available rescue species breathe air. Surface state retains
    // the project's floating convention; Water Walk grants no underwater breath.
    bool RescueCanBreathe=>!map.RescueAquatic.Submerged;
    void ObserveRescueBreathing()
    {
        if(!map.RescuePosition.HasValue||rescueCarrier.Dead)return;
        rescueCarrier.Breathing.Observe(RescueCanBreathe,worldRounds,rescueCarrier.Ability(2));
        if(rescueCarrier.ApplyFatalSuffocation())RecordRescueSuffocationDeath();
    }
    void FinishRescueBreathingTurn()
    {
        if(!map.RescuePosition.HasValue||rescueCarrier.Dead)return;
        ObserveRescueBreathing();if(rescueCarrier.Dead)return;
        long end=worldRounds==long.MaxValue?worldRounds:worldRounds+1;
        if(rescueCarrier.Breathing.EndTurn(terrainTurn,end))history.Insert(0,"救援者窒息力竭 "+rescueCarrier.Breathing.SuffocationExhaustion+"级，D20调整 "+rescueCarrier.Breathing.D20Penalty+"，速度减少 "+rescueCarrier.Breathing.SpeedPenalty+"尺。");
        if(rescueCarrier.ApplyFatalSuffocation())RecordRescueSuffocationDeath();
    }
    void RecordRescueSuffocationDeath()
    {
        rescueCarrier.ObserveLife(worldRounds);ReconcileFallenRescue();
        history.Insert(0,"救援者因6级窒息力竭死亡；此效果不属于伤害，不触发守护之链。");
    }
    bool RestoreRescueBreathingAfterRevival()
    {
        if(rescueCarrier.ReconcileRevivalBreathing(RescueCanBreathe)){ObserveRescueBreathing();return true;}
        rescueCarrier.ObserveLife(worldRounds);ReconcileFallenRescue();
        history.Insert(0,"救援者复活后仍无法呼吸，保留6级窒息力竭并立即再次死亡；需先搬到可呼吸环境。");return false;
    }
    void DrawRescueBreathingStatus()
    {
        var state=rescueCarrier.Breathing;
        string text=rescueCarrier.Dead?"呼吸：已死亡":state.CanBreathe?"呼吸：正常":worldRounds<state.BreathEndsAt?"屏息剩余 "+(state.BreathEndsAt-worldRounds)*6+" 秒":"无法呼吸，窒息力竭 "+state.SuffocationExhaustion+" / 6";
        GUILayout.Label(text,small);
    }
}
