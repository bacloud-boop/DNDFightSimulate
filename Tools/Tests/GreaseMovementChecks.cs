using System;
using UnityEngine;
class GreaseMovementChecks
{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void Main(){
  var map=new BattleMap();map.Lock();int walked=0,landings=0,restores=0;
  map.MovementObserved=(actor,from,to,oldSize,size,landing)=>{if(actor==0){if(landing)landings++;else walked++;if(oldSize!=size)restores++;}};
  var meter=map.RouteMeter(map.Player,V(2,1),false);var previous=map.Player;
  foreach(var cell in BattleMap.MovementAnchors(previous,V(2,1))){int cost=map.RouteStepCost(meter,previous,cell,1,0);Check(map.WithMovementCost(cell,cost,()=>map.MovePlayer(cell)),"paid section moves");previous=cell;}
  Check(map.Player==V(2,1)&&map.MovementSpent==11&&map.MovementRemaining==19&&walked==2,"split path preserves distance and reports each section");
  Check(!map.MovementCostOverride(V(2,1)).HasValue,"override cannot leak into later movement previews");
  int budget=map.MovementRemaining;Check(!map.WithMovementCost(V(3,1),100,()=>map.MovePlayer(V(3,1)))&&map.MovementRemaining==budget,"rejected section spends nothing");
  Check(map.Jump(true,V(3,1))&&landings==1&&map.MovementRemaining==9,"jump reports landing and costs ten");
  budget=map.MovementRemaining;Check(map.TeleportPlayer(V(4,1),30)&&landings==2&&map.MovementRemaining==budget,"teleport reports landing without movement cost");
  Check(map.TransformAt(true,2,V(4,1),1)&&restores==1&&map.RestoreForm(true)&&restores==2,"both transform and restore report footprint changes");
  int forced=0;map.MovementObserved=(actor,from,to,oldSize,size,landing)=>{if(actor==-1)forced++;};
  budget=map.MonsterMovementRemaining;Check(map.ForceMoveToward(false,V(0,0),1,10)>0&&forced>0&&map.MonsterMovementRemaining==budget,"forced entry reports route without charging movement");
  Console.WriteLine("PASS: movement sections, paid-distance conservation, override isolation, rejection, jump/teleport landings and forced/shape entry events");
 }
}
