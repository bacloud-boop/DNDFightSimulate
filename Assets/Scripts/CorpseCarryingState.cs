using System;
public sealed class CorpseCarryingState
{
    public int? TargetId {get;private set;}
    public bool Dragging {get;private set;}
    public long WeightPounds {get;private set;}
    public bool Loaded=>TargetId.HasValue;
    public bool PickUp(int targetId,bool targetDead,BodyWeight body,int strength,int size,long ownEquipment,bool drag,Func<bool> useAction)
    {
        if(Loaded||!targetDead||body==null||!body.Known||useAction==null)return false;
        if(!CarryingRules.CanCarry(strength,size,ownEquipment,0))return false;
        long weight=body.TotalPounds;
        if(drag?!CarryingRules.CanDrag(strength,size,weight):!CarryingRules.CanCarry(strength,size,ownEquipment,weight))return false;
        if(!useAction())return false;
        TargetId=targetId;Dragging=drag;WeightPounds=weight;return true;
    }
    public int AllowedSpeed(int strength,int size,long ownEquipment,int speed)
    {
        if(!Loaded)return speed;
        if(!CarryingRules.CanCarry(strength,size,ownEquipment,0))return 0;
        return Dragging?CarryingRules.DragSpeed(strength,size,WeightPounds,speed):CarryingRules.CanCarry(strength,size,ownEquipment,WeightPounds)?speed:0;
    }
    public bool FitsForm(int strength,int size,long equipmentPounds,bool equipmentMelded)
    {
        long equipment=equipmentMelded?0:equipmentPounds;
        if(!CarryingRules.CanCarry(strength,size,equipment,0))return false;
        return !Loaded||(Dragging?CarryingRules.CanDrag(strength,size,WeightPounds):CarryingRules.CanCarry(strength,size,equipment,WeightPounds));
    }
    public bool RequiresForcedDrop(int strength,int size,long equipmentPounds,bool equipmentMelded)
        =>Loaded&&!FitsForm(strength,size,equipmentPounds,equipmentMelded);
    public bool Drop(Func<bool> placeCorpse)
    {
        if(!Loaded||placeCorpse==null||!placeCorpse())return false;
        Clear();return true;
    }
    public void Clear(){TargetId=null;Dragging=false;WeightPounds=0;}
}
