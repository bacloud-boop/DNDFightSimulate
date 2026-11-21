using System;
using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly AftermathTurns aftermathTurns=new AftermathTurns();
    bool aftermathAdvanceCompleted,automaticAftermathAdvance,advancingPlayerCast,revivalCastInterrupted;
    void NoticeCastingInterruption(){if(advancingPlayerCast&&!PlayerCanSustainCast)revivalCastInterrupted=true;}
    bool PlayerTurnAllowed=>!combat.Aftermath||aftermathTurns.PlayerTurn;
    bool RescueTurnAllowed=>combat.Aftermath&&aftermathTurns.Active&&aftermathTurns.RescueTurn;
    void EnsureAftermathTurns(){if(!combat.Aftermath)return;aftermathTurns.Enter();combat.AftermathPlayerTurn=aftermathTurns.PlayerTurn;}
    void ExitAftermathTurns(){aftermathTurns.Exit();combat.AftermathPlayerTurn=true;}
    void DrawAftermathPhaseControls()
    {
        EnsureAftermathTurns();GUILayout.Label(RescueTurnAllowed?"当前：救援者回合（玩家暂停主动操作）":"当前：玩家回合（救援者尚未行动）",small);
        bool old=GUI.enabled;GUI.enabled=old&&!rolling&&PendingTerrainCount==0;
        string text=RescueTurnAllowed?"结束救援者回合 · 推进6秒":map.RescuePosition.HasValue?"结束玩家回合 → 救援者":"结束玩家回合 · 推进6秒";
        if(GUILayout.Button(text,button)){panel="";StartCoroutine(EndAftermathTurn());}GUI.enabled=old;
    }
    IEnumerator EndAftermathTurn()
    {
        if(!combat.Aftermath)yield break;EnsureAftermathTurns();bool busy=rolling;rolling=true;
        try{
            if(aftermathTurns.PlayerTurn){yield return RepeatSensorySaves(true);FinishWaterBreathingTurn(true);NoticeCastingInterruption();ApplySharedTurnEnd(0);combat.ExpirePlayerTurnEffects();}
            else {
                if(map.RescuePosition.HasValue){if(!rescueCarrier.Dead)yield return FinishUnitEndEffects(BattleMap.RescueId);rescueDropMode=false;bool active=rescueCarrier.TurnActive;rescueCarrier.EndTurn();if(!active)ApplySharedTurnEnd(BattleMap.RescueId);}
            }
            bool complete=aftermathTurns.Finish(map.RescuePosition.HasValue);combat.AftermathPlayerTurn=aftermathTurns.PlayerTurn;
            if(!complete){
                if(!rescueCarrier.BeginTurn(combat.Round))StartWebTurn(BattleMap.RescueId);
                if(PendingTerrainCount>0)yield return DrainGreaseEntries();yield return RescueDeathSave();
                var commanded=new CommandResolution();yield return FollowCommand(BattleMap.RescueId,commanded);if(commanded.Consumed){yield return EndAftermathTurn();yield break;}
                if(!automaticAftermathAdvance)history.Insert(0,"战后切换至救援者回合；世界时间尚未推进，玩家主动操作暂停。");
            }else{
                AdvanceAftermathClock(1);combat.EndRound(false);fighter.EndRound();lightAttacksThisTurn.Clear();actionWeapon=null;map.RefreshMovement();
                yield return RunAftermathPassiveTurns();StartWebTurn(0);if(PendingTerrainCount>0)yield return DrainGreaseEntries();NoticeCastingInterruption();yield return PlayerDeathSave();
                if(!automaticAftermathAdvance)history.Insert(0,"战后双方回合结束，推进6秒并开始下一玩家回合；救援者资源在其回合开始时刷新。");
            }
        }finally{rolling=busy;}
    }
    IEnumerator AdvanceAftermathRounds(long rounds,Func<bool> canContinue=null)
    {
        aftermathAdvanceCompleted=false;if(rounds<0||worldRounds>long.MaxValue-rounds)yield break;
        if(!combat.Aftermath){AdvanceAftermathClock(rounds);aftermathAdvanceCompleted=true;yield break;}
        bool busy=rolling,previousAutomatic=automaticAftermathAdvance;string savedPanel=panel,savedPage=page;panel="";page="battle";rolling=true;automaticAftermathAdvance=true;long finish=worldRounds+rounds;
        try{
            while(worldRounds<finish){
                if(!combat.Aftermath||canContinue!=null&&!canContinue())yield break;
                yield return EndAftermathTurn();
                // Yield during long waits even when no dice require animation.
                if((worldRounds&31)==0)yield return null;
            }
            aftermathAdvanceCompleted=true;
        }finally{rolling=busy;automaticAftermathAdvance=previousAutomatic;panel=savedPanel;page=savedPage;}
    }
    IEnumerator WaitAftermathMinutes(int minutes)
    {
        if(minutes<1||!combat.Aftermath||rolling)yield break;panel="";yield return AdvanceAftermathRounds((long)minutes*10);aftermathMessage=aftermathAdvanceCompleted?"等待完成：已按双方回合逐轮结算。":"等待未完成。";
    }
    bool PlayerCanSustainCast=>combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&!combat.PlayerFeignDeath&&CanCastInCurrentForm;
    IEnumerator AdvanceRevivalTime(string name,bool external)
    {
        aftermathAdvanceCompleted=false;if(!external&&!combat.TryUseAftermathMagicAction())yield break;
        revivalCastInterrupted=false;advancingPlayerCast=!external;try{yield return AdvanceAftermathRounds(RevivalRules.Duration(name),external?(Func<bool>)null:()=>PlayerCanSustainCast&&!revivalCastInterrupted);aftermathAdvanceCompleted&=!revivalCastInterrupted;}finally{advancingPlayerCast=false;}
    }
}
