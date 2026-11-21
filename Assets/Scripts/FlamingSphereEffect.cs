using System;
using System.Collections.Generic;
using UnityEngine;
public sealed class FlamingSphereEffect
{
    public int X,Y,Slot;public long Dc,Expires;long begins=-1;
    int stoppedRound=-1;
    readonly Dictionary<int,long> ended=new Dictionary<int,long>();
    public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
    public bool Begin(Vector2Int cell,int slot,long dc,long now)
    {
        if(slot<2||slot>9||now<0||now>long.MaxValue-10)return false;
        Clear();X=cell.x;Y=cell.y;Slot=slot;Dc=dc;begins=now;Expires=now+10;return true;
    }
    public bool CanMove(int round,long now)=>Active(now)&&stoppedRound!=round;
    public void Place(Vector2Int cell,bool collision,int round){X=cell.x;Y=cell.y;if(collision)stoppedRound=round;}
    public bool EndAvailable(int actor,long turn,long now)=>Active(now)&&(!ended.ContainsKey(actor)||ended[actor]!=turn);
    public bool ClaimEnd(int actor,long turn,long now){if(!EndAvailable(actor,turn,now))return false;ended[actor]=turn;return true;}
    public void Clear(){begins=-1;Expires=0;stoppedRound=-1;ended.Clear();}
    // Water is a gap. Up to ten feet of water may be crossed, but every waypoint
    // and committed landing must be on land. Airborne cells never hit creatures.
    public static bool Route(Vector2Int start,IEnumerable<Vector2Int> waypoints,Func<Vector2Int,bool> inside,Func<Vector2Int,bool> water,out List<Vector2Int> landings,out int feet)
    {
        landings=new List<Vector2Int>();feet=0;var from=start;
        if(!inside(start)||water(start)||waypoints==null)return false;
        foreach(var destination in waypoints){
            if(!inside(destination)||water(destination))return false;int wet=0;var previous=from;
            foreach(var p in BattleMap.MovementAnchors(from,destination)){
                if(!inside(p))return false;int cost=BattleMap.FeetBetween(previous,p);feet+=cost;if(feet>30)return false;
                if(water(p)){wet+=cost;if(wet>10)return false;}else {wet=0;landings.Add(p);}previous=p;
            }from=destination;
        }
        return landings.Count>0;
    }
}
