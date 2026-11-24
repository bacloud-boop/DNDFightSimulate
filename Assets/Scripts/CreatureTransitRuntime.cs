using System.Collections.Generic;
using UnityEngine;

public sealed partial class TrainingGame
{
    void InitializeCreatureTransit()
    {
        map.CreatureSizeIndex=actor=>actor==0?EffectiveCreatureSize(true):actor==-1?EffectiveCreatureSize(false):actor==BattleMap.RescueId?rescueCarrier.Size:FindSummon(actor)?.Size??2;
        map.CreatureNimble=actor=>actor==BattleMap.RescueId?rescueCarrier.Species.Kind==Species2024.Halfling:(actor==0||actor==-1)&&SpeciesPhysicalActive(actor==0)&&Species(actor==0).Kind==Species2024.Halfling;
        map.CreatureLiving=actor=>actor==0?!combat.PlayerDead:actor==-1?combat.Hp>0:actor==BattleMap.RescueId?!rescueCarrier.Dead:FindSummon(actor)?.Alive??false;
        map.CreatureIncapacitated=actor=>actor==0?combat.PlayerFeignDeath||combat.PlayerHp==0||combat.PlayerParalyzed:actor==-1?combat.MonsterFeignDeath||combat.MonsterParalyzed||combat.MonsterSleepStage>0:actor==BattleMap.RescueId?!rescueCarrier.Conscious:Feigning(actor)||(FindSummon(actor)?.SleepStage??0)>0;
        map.ActorTurnEnded=ApplySharedTurnEnd;combat.TurnEnded=ApplySharedTurnEnd;rescueCarrier.TurnEnded=()=>ApplySharedTurnEnd(BattleMap.RescueId);
    }
    string TransitActorName(int actor)=>actor==0?"玩家":actor==-1?"木桩":actor==BattleMap.RescueId?"救援者":FindSummon(actor)?.Name??"生物";
    bool ActorProne(int actor)=>actor==0?combat.PlayerProne:actor==-1?combat.MonsterProne:actor==BattleMap.RescueId?rescueCarrier.Prone:FindSummon(actor)?.Prone??false;
    void SetActorProne(int actor)
    {
        if(actor==0)combat.PlayerProne=true;else if(actor==-1)combat.MonsterProne=true;else if(actor==BattleMap.RescueId)rescueCarrier.Prone=true;else {var unit=FindSummon(actor);if(unit!=null)unit.Prone=true;}
    }
    void ApplySharedTurnEnd(int actor)
    {
        if(actor==0){EndAnimateCastingTurn();EndRegenerateCastingTurn();}iceStorm.EndTurn(actor,terrainTurn);turnConditions.EndTurn(actor);starryWisp.EndTurn(actor);RefreshStarryWisp();
        if(!map.SharedTurnNeedsProne(actor)||ActorProne(actor))return;
        SetActorProne(actor);SyncTimedBuffs();history.Insert(0,TransitActorName(actor)+"回合结束时与其他生物共占，且非微型、体型未大于共占者，因此倒地。");
    }
    void ObserveForcedSharedStop(int actor)
    {
        if(!map.HasSharedPosition(actor))return;
        if(actor==0&&combat.PlayerHp==0&&!combat.PlayerDead)SetActorProne(actor);
        SyncTimedBuffs();string message=TransitActorName(actor)+"移动途中被迫停止并共占，保留当前位置；主动结束移动仍须选择空位，回合末按体型判定倒地。";
        history.Insert(0,message);if(actor==0)movementMessage=message;else if(actor==BattleMap.RescueId)rescueMessage=message;
    }
    void DrawSharedPositions(Rect board)
    {
        var actors=new List<int>{0,-1};if(map.RescuePosition.HasValue)actors.Add(BattleMap.RescueId);foreach(var u in map.SceneSummons)if(u.Alive)actors.Add(u.Id);
        var names=new List<string>();foreach(int actor in actors)if(map.HasSharedPosition(actor))names.Add(TransitActorName(actor)+(ActorProne(actor)?"（倒地）":""));
        if(names.Count==0)return;
        var rect=new Rect(board.x,board.y,board.width,42);Fill(rect,new Color(.16f,.08f,.03f,.94f));GUI.Label(rect,"被迫共占："+string.Join("、",names)+"\n移动终点须空闲；非微型且未大于共占者，回合末倒地",new GUIStyle(centered){fontSize=12,wordWrap=true,normal={textColor=Color.white}});
    }
}
