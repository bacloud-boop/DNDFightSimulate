using UnityEngine;
public sealed partial class BattleMap
{
    public bool CommandCanEnd(int actor,Vector2Int cell)
    {
        Vector2Int from;int spaces;if(!MovementBody(actor,out from,out spaces)||!Inside(cell,spaces)||!RemainsClearForMove(actor,from,cell,spaces))return false;
        foreach(var body in TransitBodies(actor))if(Overlap(cell,spaces,body.Cell,body.Spaces))return false;return true;
    }
    // Probe from a hypothetical origin without spending resources, notifying terrain,
    // moving cargo, or leaving the rider at a hypothetical location.
    public int CommandStepCost(int actor,Vector2Int from,Vector2Int to)
    {
        if(!Locked||from==to)return int.MaxValue;
        var oldPlayer=Player;var oldMonster=Monster;var oldRescue=RescuePosition;var oldTransit=transitStepActor;transitStepActor=actor;
        SummonedUnit unit=null;foreach(var u in summons)if(u.Id==actor&&u.Alive){unit=u;break;}
        int oldX=unit==null?0:unit.X,oldY=unit==null?0:unit.Y;
        try{
            int spaces,swim;bool prone;
            if(actor==0){if(Riding||IsContained(true)||IsCarried(true)||ActorRestrained(true))return int.MaxValue;Player=from;spaces=PlayerSpaces;swim=PlayerSwimmingSpeed;prone=PlayerCrawling;}
            else if(actor==-1){if(IsContained(false)||IsCarried(false)||ActorRestrained(false))return int.MaxValue;Monster=from;spaces=MonsterSpaces;swim=MonsterSwimmingSpeed;prone=MonsterCrawling;}
            else if(actor==RescueId){if(!RescuePosition.HasValue)return int.MaxValue;RescuePosition=from;spaces=RescueSpaces;swim=RescueSwimmingSpeed;prone=RescueCrawling;}
            else {if(unit==null||unit.Restrained)return int.MaxValue;unit.X=from.x;unit.Y=from.y;SyncRiderToMount(unit);spaces=unit.Spaces;swim=unit.SwimSpeed;prone=unit.Prone;}
            if(!Inside(from,spaces)||!VoluntaryMovementAllowed(actor,to)||!CanOccupyForMove(actor,from,to,spaces)||!CreaturePathClear(actor,from,to,spaces))return int.MaxValue;
            if(actor==0||actor==-1){if(!RecoveryPathSafe(actor==0,from,to))return int.MaxValue;}
            else if(actor==RescueId){if(!RescueDestinationClear(to))return int.MaxValue;}
            else {unit.X=to.x;unit.Y=to.y;SyncRiderToMount(unit);if(!RecoverySafe(Player,Monster))return int.MaxValue;}
            return WaterMovementCost(from,to,spaces,swim,prone,actor);
        }finally{transitStepActor=oldTransit;Player=oldPlayer;Monster=oldMonster;RescuePosition=oldRescue;if(unit!=null){unit.X=oldX;unit.Y=oldY;}}
    }
}
