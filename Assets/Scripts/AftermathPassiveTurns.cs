using System.Collections;
using System.Collections.Generic;
public sealed partial class TrainingGame
{
    IEnumerator RunAftermathPassiveTurns()
    {
        // No manual AI/actions for paused units; Command alone may compel movement.
        web.ExpireMissingSummonOwners(id=>FindSummon(id)!=null);
        foreach(var unit in new List<SummonedUnit>(map.Summons)){
            if(!unit.Alive)continue;
            StartTerrainTurn(unit.Id,true);if(PendingTerrainCount>0)yield return DrainGreaseEntries();
            bool compelled=commandEffect.Next(unit.Id)!=null;
            if(compelled){unit.BeginTurn(combat.Round,false);var result=new CommandResolution();yield return FollowCommand(unit.Id,result);}
            if(unit.Alive)yield return FinishSummonPassiveEffects(unit,true);
            if(unit.TurnActive)unit.EndTurn();else ApplySharedTurnEnd(unit.Id);RefreshBestialSpirit();
        }
        // A dead monster has no effects to resolve, but its initiative point still
        // expires burning web cells started on its former turn.
        if(combat.Hp==0){web.StartTurn(-1);iceStorm.EndTurn(-1,++terrainTurn);yield break;}
        StartTerrainTurn(-1,true);if(PendingTerrainCount>0)yield return DrainGreaseEntries();
        if(combat.Hp>0&&commandEffect.Next(-1)!=null){combat.StartMonsterTurn(false);map.StartMonsterMovementTurn(combat.MonsterSpeed);var result=new CommandResolution();yield return FollowCommand(-1,result);}
        if(combat.Hp>0){yield return RepeatHoldPersonSave();FinishWaterBreathingTurn(false);yield return RepeatSensorySaves(false);combat.FinishMonsterTurnEffects();}
        else iceStorm.EndTurn(-1,terrainTurn);
        NoticeCastingInterruption();
    }
    IEnumerator FinishSummonPassiveEffects(SummonedUnit unit,bool aftermath=false)
    {
        if(unit==null||!unit.Alive)yield break;
        yield return FinishUnitEndEffects(unit.Id,unit);
        if(unit.Alive){ObserveWaterBreathing();if(unit.Breathing.EndTurn(terrainTurn,worldRounds+(aftermath?0:1))&&unit.Breathing.Fatal)unit.Dismiss();}
    }
}
