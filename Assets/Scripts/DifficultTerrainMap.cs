using System;
using UnityEngine;
public sealed partial class BattleMap
{
    public Func<int,int,int,bool> DifficultTerrain;
    public Func<int,bool> MovementFreedom;
    bool HasMovementFreedom(int actor)=>MovementFreedom!=null&&MovementFreedom(actor);
    public Func<bool,bool> RestrainedActor;
    bool ActorRestrained(bool player)=>RestrainedActor!=null&&RestrainedActor(player);
    public int DifficultTerrainCost(Vector2Int from,Vector2Int to,int spaces,int actor=int.MaxValue)
    {
        if(HasMovementFreedom(actor)||DifficultTerrain==null&&CreatureSizeIndex==null)return 0;
        int distance=FeetBetween(from,to),steps=Math.Max(Math.Abs(to.x-from.x),Math.Abs(to.y-from.y)),total=0,rough=0;var previous=from;
        for(int i=1;i<=steps;i++){
            var cell=new Vector2Int(Mathf.RoundToInt(from.x+(to.x-from.x)*(float)i/steps),Mathf.RoundToInt(from.y+(to.y-from.y)*(float)i/steps));
            int part=FeetBetween(previous,cell);total+=part;if(RoughTerrainAt(actor,cell,spaces))rough+=part;previous=cell;
        }
        return total==0?0:Mathf.RoundToInt((float)distance*rough/total);
    }
}
