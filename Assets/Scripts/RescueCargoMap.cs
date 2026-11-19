using UnityEngine;
public sealed partial class BattleMap
{
    public bool? RescueCargoPlayer {get;private set;}
    public bool RescueCargoDragging {get;private set;}
    Vector2Int rescueCargoOffset;
    public bool IsCarried(bool player)=>RescueCargoPlayer.HasValue&&RescueCargoPlayer.Value==player&&!RescueCargoDragging;
    public bool CanAttachRescueCorpse(bool player)=>RescuePosition.HasValue&&!RescueCargoPlayer.HasValue&&!IsContained(player)
        &&BodyDistance(RescuePosition.Value,RescueSpaces,player?Player:Monster,player?PlayerSpaces:MonsterSpaces)<=5;
    public bool AttachRescueCorpse(bool player,bool dragging)
    {
        if(!CanAttachRescueCorpse(player))return false;
        var body=player?Player:Monster;rescueCargoOffset=new Vector2Int(body.x-RescuePosition.Value.x,body.y-RescuePosition.Value.y);
        RescueCargoPlayer=player;RescueCargoDragging=dragging;SyncRescueCargo();return true;
    }
    void SyncRescueCargo()
    {
        if(!RescueCargoPlayer.HasValue||!RescuePosition.HasValue)return;
        var anchor=RescuePosition.Value;if(RescueCargoDragging)anchor=new Vector2Int(anchor.x+rescueCargoOffset.x,anchor.y+rescueCargoOffset.y);
        if(RescueCargoPlayer.Value)Player=anchor;else Monster=anchor;
        SyncWaterPositions();
    }
    public bool CanDropRescueCorpse(Vector2Int cell)
    {
        if(!RescueCargoPlayer.HasValue||!RescuePosition.HasValue)return false;
        bool player=RescueCargoPlayer.Value;int spaces=player?PlayerSpaces:MonsterSpaces;
        return Inside(cell,spaces)&&SummonFootprintClear(cell,spaces)&&!Overlap(cell,spaces,player?Monster:Player,OccupiedSpaces(!player))
            &&BodyDistance(cell,spaces,RescuePosition.Value,RescueSpaces)<=5;
    }
    public bool DropRescueCorpse(Vector2Int cell)
    {
        if(!CanDropRescueCorpse(cell))return false;
        if(RescueCargoPlayer.Value)Player=cell;else Monster=cell;
        RescueCargoPlayer=null;RescueCargoDragging=false;SyncWaterPositions();return true;
    }
    // Forced release is free and may exceed the normal adjacent manual drop range.
    public bool ForceReleaseRescueCargo()
    {
        if(!RescueCargoPlayer.HasValue)return true;
        if(!RescuePosition.HasValue)return false;
        if(RescueCargoDragging){RescueCargoPlayer=null;RescueCargoDragging=false;SyncWaterPositions();return true;}
        bool player=RescueCargoPlayer.Value;var oldCarrier=RescuePosition.Value;var oldBody=player?Player:Monster;int spaces=player?PlayerSpaces:MonsterSpaces;
        RescueReleasePlanner.Plan plan;
        bool found=RescueReleasePlanner.TryFind(Size,oldCarrier.x,oldCarrier.y,RescueSpaces,spaces,(cx,cy,bx,by)=>{
            var carrier=new Vector2Int(cx,cy);var body=new Vector2Int(bx,by);var other=player?Monster:Player;
            if(!SummonFootprintClear(carrier,RescueSpaces,RescueId)||!SummonFootprintClear(body,spaces,RescueId)||Overlap(carrier,RescueSpaces,other,OccupiedSpaces(!player))||Overlap(body,spaces,other,OccupiedSpaces(!player)))return false;
            RescuePosition=carrier;RescueCargoPlayer=null;if(player)Player=body;else Monster=body;
            try{return RecoverySafe(Player,Monster);}finally{RescuePosition=oldCarrier;RescueCargoPlayer=player;if(player)Player=oldBody;else Monster=oldBody;}
        },out plan);
        if(!found)return false;
        RescuePosition=new Vector2Int(plan.CarrierX,plan.CarrierY);var dropped=new Vector2Int(plan.BodyX,plan.BodyY);
        if(player)Player=dropped;else Monster=dropped;RescueCargoPlayer=null;RescueCargoDragging=false;SyncWaterPositions();
        if(dropped!=oldBody)MovementObserved?.Invoke(player?0:-1,oldBody,dropped,spaces,spaces,true);
        if(RescuePosition.Value!=oldCarrier)MovementObserved?.Invoke(RescueId,oldCarrier,RescuePosition.Value,RescueSpaces,RescueSpaces,true);
        return true;
    }
    bool RescueDestinationClear(Vector2Int cell)
    {
        if(!Inside(cell,RescueSpaces)||!SummonFootprintClear(cell,RescueSpaces,RescueId))return false;
        if((!RescueCargoPlayer.HasValue||!RescueCargoPlayer.Value)&&Overlap(cell,RescueSpaces,Player,OccupiedSpaces(true)))return false;
        if((!RescueCargoPlayer.HasValue||RescueCargoPlayer.Value)&&Overlap(cell,RescueSpaces,Monster,OccupiedSpaces(false)))return false;
        if(!RescueCargoPlayer.HasValue||!RescueCargoDragging)return true;
        bool player=RescueCargoPlayer.Value;int spaces=player?PlayerSpaces:MonsterSpaces;
        var dragged=new Vector2Int(cell.x+rescueCargoOffset.x,cell.y+rescueCargoOffset.y);
        var other=player?Monster:Player;
        if(!Inside(dragged,spaces)||!SummonFootprintClear(dragged,spaces,RescueId)||Overlap(dragged,spaces,cell,RescueSpaces)||Overlap(dragged,spaces,other,OccupiedSpaces(!player)))return false;
        var from=player?Player:Monster;
        if(SegmentCrossesBody(from,dragged,spaces,other,OccupiedSpaces(!player)))return false;
        foreach(var unit in summons)if(unit.Alive&&SegmentCrossesBody(from,dragged,spaces,new Vector2Int(unit.X,unit.Y),unit.Spaces))return false;
        return true;
    }
}
