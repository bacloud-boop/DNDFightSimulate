using UnityEngine;
public sealed partial class BattleMap
{
    int riderMountId;
    public int RiderMountId=>riderMountId;
    public SummonedUnit RiderMount {get{foreach(var u in summons)if(u.Id==riderMountId)return u;return null;}}
    public bool Riding=>RiderMount!=null;
    public int RiderBodySpaces=>playerBodySpaces;
    public bool CanMountSummon(int id)
    {
        if(!Locked||Riding||HasContainedCreature||IsCarried(true))return false;
        foreach(var u in summons)if(u.Id==id&&u.Alive&&!u.Prone&&u.SleepStage==0&&u.Spaces>playerBodySpaces&&BodyDistance(Player,playerBodySpaces,new Vector2Int(u.X,u.Y),u.Spaces)<=5){
            var old=Player;Vector2Int ignored;try{riderMountId=id;Player=new Vector2Int(u.X,u.Y);return RecoverySafe(Player,Monster)&&FindDismount(playerBodySpaces,out ignored);}
            finally{riderMountId=0;Player=old;}
        }
        return false;
    }
    public bool MountSummon(int id)
    {
        if(!CanMountSummon(id))return false;
        var from=Player;int old=PlayerSpaces;riderMountId=id;Player=new Vector2Int(RiderMount.X,RiderMount.Y);SyncWaterPositions();
        MovementObserved?.Invoke(0,from,Player,old,PlayerSpaces,true);return true;
    }
    bool DismountCellLegal(Vector2Int cell,int spaces)
    {
        if(!Inside(cell,spaces)||!SummonFootprintClear(cell,spaces)||Overlap(cell,spaces,Monster,OccupiedSpaces(false)))return false;
        Vector2Int ignored;
        return FindRecovery(cell,spaces,playerRecoverySpaces,Monster,OccupiedSpaces(false),out ignored)
            &&FindRecovery(Monster,MonsterSpaces,monsterRecoverySpaces,cell,spaces,out ignored);
    }
    public bool FindDismount(int spaces,out Vector2Int cell)
    {
        cell=Player;int x,y;
        if(!DismountPlacement.FindNearest(Size,Player.x,Player.y,PlayerSpaces,spaces,(a,b)=>DismountCellLegal(new Vector2Int(a,b),spaces),out x,out y))return false;
        cell=new Vector2Int(x,y);return true;
    }
    public bool DismountRider()
    {
        if(!Riding)return false;Vector2Int cell;if(!FindDismount(playerBodySpaces,out cell))return false;
        var from=Player;int old=PlayerSpaces;riderMountId=0;Player=cell;SyncWaterPositions();MovementObserved?.Invoke(0,from,cell,old,PlayerSpaces,true);return true;
    }
    // Preflight the changed body, then commit the transformation and compulsory
    // dismount together. A size change never spends mounting movement.
    bool MountedSizeChange(int spaces,out Vector2Int cell)
    {
        cell=Player;return !Riding||spaces<RiderMount.Spaces||FindDismount(spaces,out cell);
    }
    void CommitMountedBody(int spaces,ref Vector2Int anchor)
    {
        if(!Riding)return;
        if(spaces>=RiderMount.Spaces){Vector2Int dismount;if(FindDismount(spaces,out dismount))anchor=dismount;riderMountId=0;}
        else anchor=new Vector2Int(RiderMount.X,RiderMount.Y);
    }
    void SyncRiderToMount(SummonedUnit unit)
    {
        if(unit.Id!=riderMountId)return;Player=new Vector2Int(unit.X,unit.Y);
    }
    public void SyncRiderWater(){var unit=RiderMount;if(unit!=null)PlayerAquatic.TrySetSubmerged(unit.Aquatic.Submerged,WaterCells(Player,PlayerSpaces),PlayerSpaces*PlayerSpaces);}
    public bool CanTeleportSummon(int id,Vector2Int cell,int range)
    {
        foreach(var u in summons)if(u.Id==id&&u.Alive){var from=new Vector2Int(u.X,u.Y);if(!VoluntaryMovementAllowed(id,cell)||FeetBetween(from,cell)>range||!CanPlaceSummon(cell,u.Spaces,id))return false;int x=u.X,y=u.Y;var p=Player;try{u.X=cell.x;u.Y=cell.y;SyncRiderToMount(u);return RecoverySafe(Player,Monster);}finally{u.X=x;u.Y=y;Player=p;}}return false;
    }
    public bool TeleportSummon(int id,Vector2Int cell,int range)
    {
        foreach(var u in summons)if(u.Id==id&&u.Alive){var from=new Vector2Int(u.X,u.Y);
            if(!CanTeleportSummon(id,cell,range))return false;
            int x=u.X,y=u.Y;var p=Player;u.X=cell.x;u.Y=cell.y;SyncRiderToMount(u);
            if(!RecoverySafe(Player,Monster)){u.X=x;u.Y=y;Player=p;return false;}
            SyncWaterPositions();MovementObserved?.Invoke(id,from,cell,u.Spaces,u.Spaces,true);
            if(id==riderMountId)MovementObserved?.Invoke(0,from,cell,u.Spaces,u.Spaces,true);return true;
        }return false;
    }
}
