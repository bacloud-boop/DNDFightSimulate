using System;
using UnityEngine;
public sealed partial class BattleMap
{
    public bool CanReactionStep(int actor,Vector2Int cell)
    {
        Vector2Int from;int spaces;
        return IsLivingCreature(actor)&&(CreatureIncapacitated==null||!CreatureIncapacitated(actor))&&MovementBody(actor,out from,out spaces)&&Math.Abs((long)cell.x-from.x)<=1&&Math.Abs((long)cell.y-from.y)<=1&&cell!=from&&CommandStepCost(actor,from,cell)!=int.MaxValue;
    }
    public bool MoveReactionStep(int actor,Vector2Int cell)
    {
        if(!CanReactionStep(actor,cell))return false;
        Vector2Int from;int spaces;MovementBody(actor,out from,out spaces);
        if(actor==RescueId)return WithCreatureTransitStep(actor,()=>MoveRescue(cell,cost=>true));
        if(actor==0)Player=cell;else if(actor==-1)Monster=cell;
        else {foreach(var unit in summons)if(unit.Id==actor){unit.X=cell.x;unit.Y=cell.y;SyncRiderToMount(unit);break;}}
        SyncWaterWalkRoute(actor,from,cell,spaces);SyncContainedPosition();
        MovementObserved?.Invoke(actor,from,cell,spaces,spaces,false);
        if(Riding&&actor==riderMountId)MovementObserved?.Invoke(0,from,cell,spaces,spaces,false);
        return true;
    }
    public bool CanReactionDismountAt(Vector2Int cell)=>Riding&&DismountCellLegal(cell,playerBodySpaces)&&BodyDistance(Player,PlayerSpaces,cell,playerBodySpaces)<=5;
    public T PreviewReactionDismount<T>(Vector2Int cell,Func<T> preview)
    {
        if(!CanReactionDismountAt(cell))throw new ArgumentException("Illegal dismount preview");
        int mount=riderMountId;var from=Player;try{riderMountId=0;Player=cell;return preview();}finally{riderMountId=mount;Player=from;}
    }
    public bool ReactionDismountAt(Vector2Int cell)
    {
        if(!CanReactionDismountAt(cell))return false;var from=Player;int spaces=PlayerSpaces;riderMountId=0;Player=cell;SyncWaterPositions();MovementObserved?.Invoke(0,from,cell,spaces,PlayerSpaces,true);return true;
    }
}
