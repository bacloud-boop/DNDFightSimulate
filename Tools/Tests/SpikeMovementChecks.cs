using System;
using UnityEngine;
class SpikeMovementChecks
{
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void Main(){
  var map=new BattleMap();map.Lock();var spikes=new SpikeGrowthEffect();spikes.Begin(2,2,13,0);int ticks=0,physical=0;
  map.DifficultTerrain=(x,y,n)=>true;
  map.MovementObserved=(actor,from,to,oldSize,size,landing)=>{if(landing)return;int feet=map.MovementTravelDistance??BattleMap.FeetBetween(from,to);physical+=feet;if(spikes.Intersects(to.x,to.y,size,0))ticks+=spikes.Travel(actor,feet,0);};
  var meter=map.RouteMeter(map.Player,V(2,1),false);var previous=map.Player;
  foreach(var cell in BattleMap.MovementAnchors(previous,V(2,1))){int before=meter.Travelled;int cost=map.RouteStepCost(meter,previous,cell,1,0);Check(map.WithMovementCost(cell,cost,()=>map.MovePlayer(cell),meter.Travelled-before),"walk section succeeds");previous=cell;}
  Check(physical==11&&ticks==2&&map.MovementSpent==22,"damage measures eleven physical feet, not twenty-two terrain cost");
  Check(!map.MovementTravelDistance.HasValue&&!map.MovementCostOverride(V(2,1)).HasValue,"physical override cannot leak beyond move");
  try{map.WithMovementCost(V(3,1),5,()=>{throw new Exception("expected");},5);}catch(Exception){}
  Check(!map.MovementTravelDistance.HasValue,"throwing scoped callback still restores travel override");
  map.RefreshMovement();int prior=ticks;
  Check(map.Jump(true,V(3,1))&&map.TeleportPlayer(V(4,1),30)&&ticks==prior,"jump and teleport do not accrue ground damage");
  Check(map.TransformAt(true,2,V(4,1),1)&&map.RestoreForm(true)&&ticks==prior,"footprint changes are not ground travel");
  spikes.Begin(9,9,13,0);int budget=map.MonsterMovementRemaining;int pushed=map.ForceMoveToward(false,V(0,0),1,10);
  Check(pushed>0&&ticks>prior&&map.MonsterMovementRemaining==budget,"forced ground movement accrues damage but spends no movement");
  Console.WriteLine("PASS: physical travel versus terrain cost, scoped overrides, jump, teleport, transformation and forced travel");
 }
}
