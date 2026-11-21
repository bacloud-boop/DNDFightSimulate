using System;
using UnityEngine;
public sealed partial class BattleMap
{
    public int PlayerSwimmingSpeed {get;set;}
    public int MonsterSwimmingSpeed {get;set;}
    public int RescueSwimmingSpeed {get;set;}
    // Preserve the existing straight-line integer distance on dry ground. Intermediate
    // grid anchors identify the fully flooded portion; proportional weighting avoids
    // changing the established distance merely because terrain is present.
    public int WaterMovementCost(Vector2Int from,Vector2Int to,int spaces,int swimSpeed,bool crawling=false,int actor=int.MaxValue)
    {
        var overridden=MovementCostOverride(to);if(overridden.HasValue)return overridden.Value;
        if(!Inside(from,spaces)||!Inside(to,spaces))return int.MaxValue;
        int distance=FeetBetween(from,to);if(distance==0)return 0;
        int steps=Math.Max(Math.Abs(to.x-from.x),Math.Abs(to.y-from.y)),total=0,wet=0;var previous=from;
        bool waterWalking=HasWaterWalking(actor),submerged=ActorAquatic(actor)?.Submerged??false;
        if(WaterCells(from,spaces)!=spaces*spaces)submerged=false;
        for(int i=1;i<=steps;i++){
            var cell=new Vector2Int(Mathf.RoundToInt(from.x+(to.x-from.x)*(float)i/steps),Mathf.RoundToInt(from.y+(to.y-from.y)*(float)i/steps));
            int part=FeetBetween(previous,cell);total+=part;
            bool fullyWet=WaterCells(cell,spaces)==spaces*spaces;
            if(!fullyWet)submerged=false;
            if(swimSpeed<=0&&fullyWet&&(!waterWalking||submerged))wet+=part;
            previous=cell;
        }
        int extra=total==0?0:Mathf.RoundToInt((float)distance*wet/total);
        return distance*(crawling?2:1)+extra+DifficultTerrainCost(from,to,spaces,actor);
    }
    public bool MonsterCrawling,RescueCrawling;
    public int MonsterMovementCost(Vector2Int to)=>WaterMovementCost(Monster,to,MonsterSpaces,MonsterSwimmingSpeed,MonsterCrawling,-1);
    public int RescueMovementCost(Vector2Int to)=>RescuePosition.HasValue?WaterMovementCost(RescuePosition.Value,to,RescueSpaces,RescueSwimmingSpeed,RescueCrawling,RescueId):int.MaxValue;
    public bool MoveSummon(int id,Vector2Int cell)
    {
        if(!CanMoveSummon(id,cell))return false;
        foreach(var unit in summons)if(unit.Id==id&&unit.Alive){
            var from=new Vector2Int(unit.X,unit.Y);
            int cost=WaterMovementCost(from,cell,unit.Spaces,unit.SwimSpeed,unit.Prone,unit.Id);
            if(!unit.SpendMovement(cost))return false;
            unit.X=cell.x;unit.Y=cell.y;SyncWaterWalkRoute(unit.Id,from,cell,unit.Spaces);SyncRiderToMount(unit);SyncWaterPositions();MovementObserved?.Invoke(unit.Id,from,cell,unit.Spaces,unit.Spaces,false);if(unit.Id==riderMountId)MovementObserved?.Invoke(0,from,cell,unit.Spaces,unit.Spaces,false);return true;
        }
        return false;
    }
}
