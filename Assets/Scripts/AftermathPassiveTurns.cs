using System.Collections;
using System.Collections.Generic;
public sealed partial class TrainingGame
{
    IEnumerator RunAftermathPassiveTurns()
    {
        // No BeginTurn/AI calls: paused units receive no active resources or orders.
        web.ExpireMissingSummonOwners(id=>FindSummon(id)!=null);
        foreach(var unit in new List<SummonedUnit>(map.Summons)){
            if(!unit.Alive)continue;
            StartTerrainTurn(unit.Id,true);if(PendingTerrainCount>0)yield return DrainGreaseEntries();
            if(unit.Alive)yield return FinishSummonPassiveEffects(unit,true);
            ApplySharedTurnEnd(unit.Id);RefreshBestialSpirit();
        }
        // A dead monster has no effects to resolve, but its initiative point still
        // expires burning web cells started on its former turn.
        if(combat.Hp==0){web.StartTurn(-1);yield break;}
        StartTerrainTurn(-1,true);if(PendingTerrainCount>0)yield return DrainGreaseEntries();
        if(combat.Hp>0){yield return RepeatHoldPersonSave();FinishWaterBreathingTurn(false);yield return RepeatSensorySaves(false);combat.FinishMonsterTurnEffects();}
        NoticeCastingInterruption();
    }
    IEnumerator FinishSummonPassiveEffects(SummonedUnit unit,bool aftermath=false)
    {
        if(unit==null||!unit.Alive)yield break;
        yield return FinishUnitEndEffects(unit.Id,unit);
        if(unit.Alive){ObserveWaterBreathing();if(unit.Breathing.EndTurn(terrainTurn,worldRounds+(aftermath?0:1))&&unit.Breathing.Fatal)unit.Dismiss();}
    }
}
