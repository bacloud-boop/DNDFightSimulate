using System.Collections.Generic;
using UnityEngine;

public sealed partial class BattleMap
{
    readonly List<SummonedUnit> summons=new List<SummonedUnit>();
    public IReadOnlyList<SummonedUnit> Summons=>summons.AsReadOnly();
    public bool SummonFootprintClear(Vector2Int anchor,int spaces,int exceptId=0)
    {
        if(exceptId!=RescueId&&RescuePosition.HasValue&&Overlap(anchor,spaces,RescuePosition.Value,RescueSpaces))return false;
        foreach(var unit in summons)if(unit.Alive&&unit.Id!=exceptId&&Overlap(anchor,spaces,new Vector2Int(unit.X,unit.Y),unit.Spaces))return false;
        return true;
    }
    public bool CanPlaceSummon(Vector2Int anchor,int spaces,int exceptId=0)=>spaces>=1&&spaces<=4&&Inside(anchor,spaces)&&SummonFootprintClear(anchor,spaces,exceptId)
        &&!Overlap(anchor,spaces,Player,OccupiedSpaces(true))&&!Overlap(anchor,spaces,Monster,OccupiedSpaces(false));
    public bool AddSummon(SummonedUnit unit,Vector2Int anchor)
    {
        if(!Locked||unit==null||!unit.Alive||!CanPlaceSummon(anchor,unit.Spaces))return false;
        foreach(var existing in summons)if(existing.Id==unit.Id)return false;
        int oldX=unit.X,oldY=unit.Y;
        unit.X=anchor.x;unit.Y=anchor.y;summons.Add(unit);
        if(!RecoverySafe(Player,Monster)){summons.Remove(unit);unit.X=oldX;unit.Y=oldY;return false;}
        SyncWaterPositions();return true;
    }
    public bool CanAddSummon(SummonedUnit unit,Vector2Int anchor)
    {
        if(!Locked||unit==null||!unit.Alive||!CanPlaceSummon(anchor,unit.Spaces))return false;
        foreach(var existing in summons)if(existing.Id==unit.Id)return false;
        int oldX=unit.X,oldY=unit.Y;
        try{unit.X=anchor.x;unit.Y=anchor.y;summons.Add(unit);return RecoverySafe(Player,Monster);}
        finally{summons.Remove(unit);unit.X=oldX;unit.Y=oldY;}
    }
    // The dummy cannot walk through the player's allied summons.
    bool CrossesEnemySummon(Vector2Int from,Vector2Int to,int spaces)
    {
        foreach(var unit in summons)if(unit.Alive&&SegmentCrossesBody(from,to,spaces,new Vector2Int(unit.X,unit.Y),unit.Spaces))return true;
        return false;
    }
    public bool CanMoveSummon(int id,Vector2Int cell)
    {
        if(!Locked)return false;
        foreach(var unit in summons)if(unit.Id==id&&unit.Alive){
            var from=new Vector2Int(unit.X,unit.Y);
            if(!CanPlaceSummon(cell,unit.Spaces,id)||SegmentCrossesBody(from,cell,unit.Spaces,Monster,OccupiedSpaces(false))||!unit.CanSpendMovement(WaterMovementCost(from,cell,unit.Spaces,unit.SwimSpeed,unit.Prone)))return false;
            // Preserve both original-form recovery spaces along the entire route.
            int steps=System.Math.Max(System.Math.Abs(cell.x-from.x),System.Math.Abs(cell.y-from.y))*2;
            try{
                for(int i=0;i<=steps;i++){
                    float t=steps==0?0:(float)i/steps;
                    unit.X=Mathf.RoundToInt(from.x+(cell.x-from.x)*t);unit.Y=Mathf.RoundToInt(from.y+(cell.y-from.y)*t);
                    if(!RecoverySafe(Player,Monster))return false;
                }
                return true;
            }finally{unit.X=from.x;unit.Y=from.y;}
        }
        return false;
    }
    public bool ChooseSummonRetreat(int id,out Vector2Int destination)
    {
        destination=Vector2Int.zero;
        foreach(var unit in summons)if(unit.Id==id&&unit.Alive&&unit.TurnActive){
            var from=new Vector2Int(unit.X,unit.Y);destination=from;
            int bestDistance=BodyDistance(from,unit.Spaces,Monster,MonsterSpaces),bestCost=0;
            bool bestBreath=unit.Aquatic.CanBreathe(unit.BreathesAir,unit.BreathesWater,WaterCells(from,unit.Spaces));
            for(int y=0;y<Size;y++)for(int x=0;x<Size;x++){
                var cell=new Vector2Int(x,y);if(!CanMoveSummon(id,cell))continue;
                int waterCells=WaterCells(cell,unit.Spaces),distance=BodyDistance(cell,unit.Spaces,Monster,MonsterSpaces),cost=WaterMovementCost(from,cell,unit.Spaces,unit.SwimSpeed,unit.Prone);
                bool submerged=unit.Aquatic.Submerged&&AquaticState.FullyInWater(waterCells,unit.Spaces*unit.Spaces);
                bool breath=submerged?unit.BreathesWater:waterCells>0?unit.BreathesAir||unit.BreathesWater:unit.BreathesAir;
                if(breath&&!bestBreath||breath==bestBreath&&(distance>bestDistance||distance==bestDistance&&cost<bestCost)){destination=cell;bestBreath=breath;bestDistance=distance;bestCost=cost;}
            }
            return destination!=from;
        }
        return false;
    }
    public void RemoveSummon(int id){foreach(var unit in summons)if(unit.Id==id)unit.Dismiss();summons.RemoveAll(unit=>unit.Id==id);}
    public void ClearSummons(){foreach(var unit in summons)unit.Dismiss();summons.Clear();}
    // A pure target selector: null means no living candidate is currently in reach.
    // Callers may evaluate it again after movement; ties remain stable (player first).
    public int NearestEnemyInReach(bool playerAlive,int reachFeet)
    {
        int distance;return NearestEnemyFrom(Monster,MonsterSpaces,playerAlive,reachFeet,out distance);
    }
    public int NearestEnemyFrom(Vector2Int from,int spaces,bool playerAlive,int reachFeet,out int best)
    {
        int chosen=-1;best=int.MaxValue;
        if(playerAlive&&!IsContained(true)&&!IsCarried(true)){int distance=BodyDistance(from,spaces,Player,PlayerSpaces);if(distance<=reachFeet){chosen=0;best=distance;}}
        foreach(var unit in summons){if(!unit.Alive)continue;int distance=BodyDistance(from,spaces,new Vector2Int(unit.X,unit.Y),unit.Spaces);
            if(distance<=reachFeet&&distance<best){chosen=unit.Id;best=distance;}
        }
        return chosen;
    }
}
