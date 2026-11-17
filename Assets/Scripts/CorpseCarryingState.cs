using System;
public sealed class CorpseCarryingState
{
    public int? TargetId {get;private set;}
    public bool Dragging {get;private set;}
    public long WeightOunces {get;private set;}
    public long WeightPounds=>WeightUnits.WholePoundsCeiling(WeightOunces);
    public bool Loaded=>TargetId.HasValue;
    public bool PickUp(int targetId,bool targetDead,BodyWeight body,int strength,int size,long ownEquipment,bool drag,Func<bool> useAction)
        =>ownEquipment>=0&&ownEquipment<=long.MaxValue/16&&PickUpOunces(targetId,targetDead,body,strength,size,ownEquipment*16,drag,useAction);
    public bool PickUpOunces(int targetId,bool targetDead,BodyWeight body,int strength,int size,long ownEquipment,bool drag,Func<bool> useAction)
    {
        if(Loaded||!targetDead||body==null||!body.Known||useAction==null)return false;
        if(!CarryingRules.CanCarryOunces(strength,size,ownEquipment,0))return false;
        long weight=body.TotalOunces;
        if(drag?!CarryingRules.CanDragOunces(strength,size,weight):!CarryingRules.CanCarryOunces(strength,size,ownEquipment,weight))return false;
        if(!useAction())return false;
        TargetId=targetId;Dragging=drag;WeightOunces=weight;return true;
    }
    public int AllowedSpeed(int strength,int size,long ownEquipment,int speed)=>ownEquipment>=0&&ownEquipment<=long.MaxValue/16?AllowedSpeedOunces(strength,size,ownEquipment*16,speed):0;
    public int AllowedSpeedOunces(int strength,int size,long ownEquipment,int speed)
    {
        if(!Loaded)return speed;
        if(!CarryingRules.CanCarryOunces(strength,size,ownEquipment,0))return 0;
        return Dragging?CarryingRules.DragSpeedOunces(strength,size,WeightOunces,speed):CarryingRules.CanCarryOunces(strength,size,ownEquipment,WeightOunces)?speed:0;
    }
    public bool FitsForm(int strength,int size,long equipmentPounds,bool equipmentMelded)=>equipmentPounds>=0&&equipmentPounds<=long.MaxValue/16&&FitsFormOunces(strength,size,equipmentPounds*16,equipmentMelded);
    public bool FitsFormOunces(int strength,int size,long equipmentOunces,bool equipmentMelded)
    {
        long equipment=equipmentMelded?0:equipmentOunces;
        if(!CarryingRules.CanCarryOunces(strength,size,equipment,0))return false;
        return !Loaded||(Dragging?CarryingRules.CanDragOunces(strength,size,WeightOunces):CarryingRules.CanCarryOunces(strength,size,equipment,WeightOunces));
    }
    public bool RequiresForcedDropOunces(int strength,int size,long equipmentOunces,bool equipmentMelded)=>Loaded&&!FitsFormOunces(strength,size,equipmentOunces,equipmentMelded);
    public bool RequiresForcedDrop(int strength,int size,long equipmentPounds,bool equipmentMelded)
        =>Loaded&&!FitsForm(strength,size,equipmentPounds,equipmentMelded);
    public bool Drop(Func<bool> placeCorpse)
    {
        if(!Loaded||placeCorpse==null||!placeCorpse())return false;
        Clear();return true;
    }
    public void Clear(){TargetId=null;Dragging=false;WeightOunces=0;}
}
