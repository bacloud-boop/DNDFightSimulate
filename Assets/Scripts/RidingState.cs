using System;
// Rider and mount retain their identities while sharing the mount's occupied footprint.
public sealed class RidingState
{
    public int? MountId {get;private set;}
    public bool Mounted=>MountId.HasValue;
    public static bool SizeAllowed(int riderSize,int mountSize)
        =>riderSize>=0&&riderSize<=5&&mountSize>=0&&mountSize<=5&&mountSize>riderSize;
    public bool TryMount(int id,int riderSize,int mountSize,bool inReach,bool willing,Func<bool> spendMovement)
    {
        if(Mounted||id<1||!SizeAllowed(riderSize,mountSize)||!inReach||!willing||spendMovement==null)return false;
        if(!spendMovement())return false;
        MountId=id;return true;
    }
    // The caller commits the size change first. Failure to place leaves a pending dismount.
    public bool NeedsForcedDismount(int riderSize,int mountSize,bool mountAlive)
        =>Mounted&&(!mountAlive||!SizeAllowed(riderSize,mountSize));
    public bool TryDismount(Func<bool> placeRider)
    {
        if(!Mounted||placeRider==null||!placeRider())return false;
        MountId=null;return true;
    }
    public void Reset(){MountId=null;}
}
public static class DismountPlacement
{
    public static bool FindNearest(int mapSize,int originX,int originY,int originSpaces,int riderSpaces,Func<int,int,bool> legal,out int resultX,out int resultY)
    {
        resultX=resultY=0;
        if(mapSize<1||originSpaces<1||riderSpaces<1||riderSpaces>mapSize||legal==null)return false;
        long nearest=long.MaxValue;
        for(int y=0;y<=mapSize-riderSpaces;y++)
        for(int x=0;x<=mapSize-riderSpaces;x++)
        {
            long dx=(long)x*2+riderSpaces-((long)originX*2+originSpaces);
            long dy=(long)y*2+riderSpaces-((long)originY*2+originSpaces);
            long distance=dx*dx+dy*dy;
            if(distance>=nearest||!legal(x,y))continue;
            nearest=distance;resultX=x;resultY=y;
        }
        return nearest<long.MaxValue;
    }
}
