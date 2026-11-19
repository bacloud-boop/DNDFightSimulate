using UnityEngine;
public sealed partial class BattleMap
{
    public int? RescueCargoId {get;private set;}
    public bool? RescueCargoPlayer=>RescueCargoId==0?(bool?)true:RescueCargoId==-1?(bool?)false:null;
    public bool RescueCargoDragging {get;private set;}
    Vector2Int rescueCargoOffset;
    public bool IsCarried(bool player)=>RescueCargoId==(player?0:-1)&&!RescueCargoDragging;
    bool CargoBody(int id,out Vector2Int body,out int spaces){if(id==0){body=Player;spaces=PlayerSpaces;return true;}if(id==-1){body=Monster;spaces=MonsterSpaces;return true;}if(id==RescueId){body=RescuePosition??Vector2Int.zero;spaces=RescueSpaces;return RescuePosition.HasValue;}var old=Remains.Find(id);body=old==null?Vector2Int.zero:new Vector2Int(old.X,old.Y);spaces=old==null?0:old.Spaces;return old!=null;}
    void SetCargoBody(int id,Vector2Int body){if(id==0)Player=body;else if(id==-1)Monster=body;else{var old=Remains.Find(id);if(old!=null){old.X=body.x;old.Y=body.y;}}}
    public bool CanAttachRescueCorpse(bool player)=>CanAttachRescueRemains(player?0:-1);
    public bool CanAttachRescueRemains(int id){Vector2Int body;int spaces;return id!=RescueId&&RescuePosition.HasValue&&!RescueCargoId.HasValue&&(id!=0&&id!=-1||!IsContained(id==0))&&CargoBody(id,out body,out spaces)&&BodyDistance(RescuePosition.Value,RescueSpaces,body,spaces)<=5;}
    public bool AttachRescueCorpse(bool player,bool dragging)=>AttachRescueRemains(player?0:-1,dragging);
    public bool AttachRescueRemains(int id,bool dragging){if(!CanAttachRescueRemains(id))return false;Vector2Int body;int spaces;CargoBody(id,out body,out spaces);rescueCargoOffset=new Vector2Int(body.x-RescuePosition.Value.x,body.y-RescuePosition.Value.y);RescueCargoId=id;RescueCargoDragging=dragging;SyncRescueCargo();return true;}
    void SyncRescueCargo(){if(!RescueCargoId.HasValue||!RescuePosition.HasValue)return;var anchor=RescuePosition.Value;if(RescueCargoDragging)anchor=new Vector2Int(anchor.x+rescueCargoOffset.x,anchor.y+rescueCargoOffset.y);SetCargoBody(RescueCargoId.Value,anchor);SyncWaterPositions();}
    bool CargoPlacement(int id,Vector2Int body,int spaces,Vector2Int carrier,bool checkCarrier){
        if(!Inside(body,spaces)||!SummonFootprintClear(body,spaces,RescueId,id)||checkCarrier&&Overlap(body,spaces,carrier,RescueSpaces))return false;
        if(id!=0&&Overlap(body,spaces,Player,OccupiedSpaces(true))||id!=-1&&Overlap(body,spaces,Monster,OccupiedSpaces(false)))return false;return true;
    }
    public bool CanDropRescueCorpse(Vector2Int cell){if(!RescueCargoId.HasValue||!RescuePosition.HasValue)return false;Vector2Int body;int spaces;if(!CargoBody(RescueCargoId.Value,out body,out spaces))return false;return CargoPlacement(RescueCargoId.Value,cell,spaces,RescuePosition.Value,true)&&BodyDistance(cell,spaces,RescuePosition.Value,RescueSpaces)<=5;}
    public bool DropRescueCorpse(Vector2Int cell){if(!CanDropRescueCorpse(cell))return false;SetCargoBody(RescueCargoId.Value,cell);RescueCargoId=null;RescueCargoDragging=false;SyncWaterPositions();return true;}
    public bool ForceReleaseRescueCargo(){
        if(!RescueCargoId.HasValue)return true;if(!RescuePosition.HasValue)return false;
        if(RescueCargoDragging){RescueCargoId=null;RescueCargoDragging=false;SyncWaterPositions();return true;}
        int id=RescueCargoId.Value;Vector2Int oldBody;int spaces;if(!CargoBody(id,out oldBody,out spaces))return false;var oldCarrier=RescuePosition.Value;
        RescueReleasePlanner.Plan plan;
        bool found=RescueReleasePlanner.TryFind(Size,oldCarrier.x,oldCarrier.y,RescueSpaces,spaces,(cx,cy,bx,by)=>{
            var carrier=new Vector2Int(cx,cy);var body=new Vector2Int(bx,by);
            if(!SummonFootprintClear(carrier,RescueSpaces,RescueId,id)||!CargoPlacement(id,body,spaces,carrier,true))return false;
            if(id!=0&&Overlap(carrier,RescueSpaces,Player,OccupiedSpaces(true))||id!=-1&&Overlap(carrier,RescueSpaces,Monster,OccupiedSpaces(false)))return false;
            RescuePosition=carrier;SetCargoBody(id,body);RescueCargoId=null;
            try{return RecoverySafe(Player,Monster);}finally{RescuePosition=oldCarrier;SetCargoBody(id,oldBody);RescueCargoId=id;}
        },out plan);
        if(!found)return false;
        RescuePosition=new Vector2Int(plan.CarrierX,plan.CarrierY);var dropped=new Vector2Int(plan.BodyX,plan.BodyY);SetCargoBody(id,dropped);RescueCargoId=null;RescueCargoDragging=false;SyncWaterPositions();
        if(dropped!=oldBody)MovementObserved?.Invoke(id,oldBody,dropped,spaces,spaces,true);
        if(RescuePosition.Value!=oldCarrier)MovementObserved?.Invoke(RescueId,oldCarrier,RescuePosition.Value,RescueSpaces,RescueSpaces,true);return true;
    }
    bool RescueDestinationClear(Vector2Int cell){
        int id=RescueCargoId??int.MaxValue;
        if(!CanOccupyForMove(RescueId,RescuePosition.Value,cell,RescueSpaces))return false;
        if(!RescueCargoId.HasValue||!RescueCargoDragging)return true;
        Vector2Int from;int spaces;if(!CargoBody(id,out from,out spaces))return false;var dragged=new Vector2Int(cell.x+rescueCargoOffset.x,cell.y+rescueCargoOffset.y);
        if(!CargoPlacement(id,dragged,spaces,cell,true)||CrossesRemains(from,dragged,spaces,id))return false;
        if(id!=0&&SegmentCrossesBody(from,dragged,spaces,Player,OccupiedSpaces(true))||id!=-1&&SegmentCrossesBody(from,dragged,spaces,Monster,OccupiedSpaces(false)))return false;
        foreach(var unit in summons)if(unit.Alive&&SegmentCrossesBody(from,dragged,spaces,new Vector2Int(unit.X,unit.Y),unit.Spaces))return false;return true;
    }
}
