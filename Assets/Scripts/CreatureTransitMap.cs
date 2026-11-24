using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class BattleMap
{
    public Func<int,int> CreatureSizeIndex;
    public Func<int,bool> CreatureNimble,CreatureLiving,CreatureIncapacitated;
    public Action<int> ActorTurnEnded;
    int? transitStepActor;
    struct TransitBody {public int Id,Spaces;public Vector2Int Cell;public TransitBody(int id,Vector2Int cell,int spaces){Id=id;Cell=cell;Spaces=spaces;}}
    IEnumerable<TransitBody> TransitBodies(int actor)
    {
        if(actor!=0&&actor!=riderMountId&&OccupiedSpaces(true)>0&&!(actor==RescueId&&RescueCargoId==0))yield return new TransitBody(0,Player,PlayerSpaces);
        if(actor!=-1&&OccupiedSpaces(false)>0&&!(actor==RescueId&&RescueCargoId==-1))yield return new TransitBody(-1,Monster,MonsterSpaces);
        if(actor!=RescueId&&OnScene(RescueId)&&RescuePosition.HasValue)yield return new TransitBody(RescueId,RescuePosition.Value,RescueSpaces);
        foreach(var u in summons)if(u.Alive&&OnScene(u.Id)&&u.Id!=actor&&!(actor==0&&u.Id==riderMountId))yield return new TransitBody(u.Id,new Vector2Int(u.X,u.Y),u.Spaces);
    }
    public bool MovementBody(int actor,out Vector2Int cell,out int spaces)
    {
        if(actor==0){cell=Player;spaces=OccupiedSpaces(true);return spaces>0;}
        if(actor==-1){cell=Monster;spaces=OccupiedSpaces(false);return spaces>0;}
        if(actor==RescueId){cell=RescuePosition??Vector2Int.zero;spaces=RescueSpaces;return OnScene(RescueId)&&RescuePosition.HasValue;}
        foreach(var u in summons)if(u.Id==actor&&u.Alive){cell=new Vector2Int(u.X,u.Y);spaces=u.Spaces;return true;}
        cell=Vector2Int.zero;spaces=0;return false;
    }
    bool IsLivingCreature(int actor)=>CreatureLiving==null||CreatureLiving(actor);
    int TransitSize(int actor,int spaces)=>CreatureSizeIndex!=null?CreatureSizeIndex(actor):spaces<=1?2:spaces==2?3:spaces==3?4:5;
    bool TransitAllies(int a,int b)=>a!=-1&&b!=-1;
    bool CanPassBody(int actor,int spaces,TransitBody body)
    {
        if(!IsLivingCreature(body.Id))return false;
        // Preserve standalone legacy map behavior until creature data is bound.
        if(CreatureSizeIndex==null)return actor==0&&body.Id>0||actor>0&&body.Id!=-1;
        return CreatureTransitRules.CanPass(TransitSize(actor,spaces),TransitSize(body.Id,body.Spaces),TransitAllies(actor,body.Id),CreatureIncapacitated!=null&&CreatureIncapacitated(body.Id),CreatureNimble!=null&&CreatureNimble(actor));
    }
    bool RemainsClearForMove(int actor,Vector2Int from,Vector2Int to,int spaces)
    {
        foreach(var b in AllRemains()){
            if(actor==RescueId&&RescueCargoId==b.Id)continue;
            var cell=new Vector2Int(b.X,b.Y);int n=RemainsSpaces(b);
            if(Overlap(to,spaces,cell,n)&&!(transitStepActor==actor&&Overlap(from,spaces,cell,n)))return false;
        }return true;
    }
    bool CanOccupyForMove(int actor,Vector2Int from,Vector2Int to,int spaces)
    {
        if(!Inside(to,spaces)||!RemainsClearForMove(actor,from,to,spaces))return false;
        if((actor==0||actor==-1)&&(IsContained(actor==0)||IsCarried(actor==0)))return false;
        foreach(var body in TransitBodies(actor))if(Overlap(to,spaces,body.Cell,body.Spaces)&&!(transitStepActor==actor&&(CanPassBody(actor,spaces,body)||Overlap(from,spaces,body.Cell,body.Spaces))))return false;
        return true;
    }
    bool CreaturePathClear(int actor,Vector2Int from,Vector2Int to,int spaces)
    {
        foreach(var body in TransitBodies(actor))if(SegmentCrossesBody(from,to,spaces,body.Cell,body.Spaces)&&!Overlap(from,spaces,body.Cell,body.Spaces)&&!CanPassBody(actor,spaces,body))return false;
        return !CrossesRemains(from,to,spaces,actor==RescueId?RescueCargoId??0:0);
    }
    public bool CreaturePathNeedsSteps(int actor,Vector2Int from,Vector2Int to,int spaces)
    {
        foreach(var body in TransitBodies(actor))if(SegmentCrossesBody(from,to,spaces,body.Cell,body.Spaces))return true;
        foreach(var b in AllRemains())if(Overlap(from,spaces,new Vector2Int(b.X,b.Y),RemainsSpaces(b)))return true;
        return false;
    }
    // Scoped to a committed route section, never to player-selected destinations.
    public bool WithCreatureTransitStep(int actor,Func<bool> operation)
    {
        var previous=transitStepActor;transitStepActor=actor;try{return operation();}finally{transitStepActor=previous;}
    }
    public bool HasSharedPosition(int actor)
    {
        Vector2Int from;int spaces;if(!MovementBody(actor,out from,out spaces))return false;
        foreach(var body in TransitBodies(actor))if(Overlap(from,spaces,body.Cell,body.Spaces))return true;return false;
    }
    public bool SharedTurnNeedsProne(int actor)
    {
        Vector2Int from;int spaces;if(!IsLivingCreature(actor)||!MovementBody(actor,out from,out spaces))return false;
        foreach(var body in TransitBodies(actor))if(IsLivingCreature(body.Id)&&Overlap(from,spaces,body.Cell,body.Spaces)&&CreatureTransitRules.SharedTurnProne(TransitSize(actor,spaces),TransitSize(body.Id,body.Spaces)))return true;return false;
    }
    bool CreatureCellRough(int actor,int x,int y)
    {
        if(CreatureSizeIndex==null||actor==int.MaxValue)return false;
        foreach(var body in TransitBodies(actor))if(IsLivingCreature(body.Id)&&!TransitAllies(actor,body.Id)&&TransitSize(body.Id,body.Spaces)!=0&&x>=body.Cell.x&&y>=body.Cell.y&&x<body.Cell.x+body.Spaces&&y<body.Cell.y+body.Spaces)return true;return false;
    }
    bool RoughTerrainAt(int actor,Vector2Int cell,int spaces)
    {
        if(HasMovementFreedom(actor))return false;
        if(actor==int.MaxValue)return DifficultTerrain!=null&&DifficultTerrain(cell.x,cell.y,spaces);
        for(int y=0;y<spaces;y++)for(int x=0;x<spaces;x++)if(!(DifficultTerrain!=null&&DifficultTerrain(cell.x+x,cell.y+y,1))&&!CreatureCellRough(actor,cell.x+x,cell.y+y))return false;return spaces>0;
    }
}
