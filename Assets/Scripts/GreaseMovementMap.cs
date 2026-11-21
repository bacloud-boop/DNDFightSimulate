using System;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class BattleMap
{
 public Action<int,Vector2Int,Vector2Int,int,int,bool> MovementObserved;
 public Func<Vector2Int,int,bool> SwimmingTerrain;
 public RouteCostMeter RouteMeter(Vector2Int from,Vector2Int to,bool crawling){int total=0;var previous=from;foreach(var cell in MovementAnchors(from,to)){total+=FeetBetween(previous,cell);previous=cell;}return new RouteCostMeter(FeetBetween(from,to),total,crawling);}
 public int RouteStepCost(RouteCostMeter meter,Vector2Int from,Vector2Int to,int spaces,int swimSpeed,int actor=int.MaxValue)=>meter.Next(FeetBetween(from,to),swimSpeed<=0&&!IsWalkingOnWaterSurface(actor)&&SwimmingTerrain!=null&&SwimmingTerrain(to,spaces),RoughTerrainAt(actor,to,spaces));
 public int? MovementTravelDistance {get;private set;}
 int? movementCostOverride;Vector2Int movementOverrideTarget;
 public int? MovementCostOverride(Vector2Int target)=>movementCostOverride.HasValue&&target==movementOverrideTarget?movementCostOverride:null;
 public bool WithMovementCost(Vector2Int target,int cost,Func<bool> move,int? travelDistance=null){if(cost<1)return false;var oldTravel=MovementTravelDistance;MovementTravelDistance=travelDistance;var old=movementCostOverride;var oldTarget=movementOverrideTarget;movementCostOverride=cost;movementOverrideTarget=target;try{return move();}finally{MovementTravelDistance=oldTravel;movementCostOverride=old;movementOverrideTarget=oldTarget;}}
 public static List<Vector2Int> MovementAnchors(Vector2Int from,Vector2Int to){var result=new List<Vector2Int>();int steps=Math.Max(Math.Abs(to.x-from.x),Math.Abs(to.y-from.y));var last=from;for(int i=1;i<=steps;i++){var cell=new Vector2Int(Mathf.RoundToInt(from.x+(to.x-from.x)*(float)i/steps),Mathf.RoundToInt(from.y+(to.y-from.y)*(float)i/steps));if(cell!=last){result.Add(cell);last=cell;}}return result;}
}
