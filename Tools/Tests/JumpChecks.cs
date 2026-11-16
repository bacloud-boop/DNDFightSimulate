using System;
using UnityEngine;
class JumpChecks
{
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Main(){
  var effect=new JumpEffect();Check(effect.Grant(true,10)&&!effect.Active(true,9)&&effect.Active(true,19)&&!effect.Active(true,20),"duration");
  Check(effect.Use(true,10,1)&&!effect.Use(true,10,1),"once per actor turn");Check(effect.Grant(true,11)&&!effect.Available(true,11,1),"recast cannot reset turn use");Check(effect.Available(true,11,2),"new turn");
  Check(effect.Grant(false,11)&&effect.Available(false,11,1),"independent actors");Check(!effect.Grant(false,long.MaxValue),"overflow");
  Check(!JumpEffect.EnvironmentAllowed(true,false,true)&&!JumpEffect.EnvironmentAllowed(false,true,true)&&!JumpEffect.EnvironmentAllowed(false,false,false)&&JumpEffect.EnvironmentAllowed(false,false,true),"surface, dragging and carried load restrictions");
  Check(!JumpEffect.EnvironmentAllowed(false,false,true,true),"prone actor must stand before jumping");
  var map=new BattleMap();map.Place(false,V(2,0));map.Lock();Check(!map.CanMoveTo(V(5,0))&&map.CanJump(true,V(5,0)),"jump crosses occupied path");
  Check(map.Jump(true,V(5,0))&&map.MovementRemaining==20,"fixed ten foot cost");
  int remaining=map.MovementRemaining;Check(!map.Jump(true,map.Monster)&&map.MovementRemaining==remaining,"occupied destination atomic");
  Check(!map.CanJump(true,V(9,9)),"range");Check(BattleMap.LeavesReach(V(1,0),V(5,0),V(0,0),5,1,1),"leaving reach still triggers");
  map.SpendPlayerMovement(15);Check(!map.CanJump(true,V(4,0)),"requires ten movement even for short jump");
  effect.Clear();Check(!effect.Active(true,11)&&!effect.Active(false,11),"battle reset");
  Console.WriteLine("PASS: jump duration, recast, turns, actors, path crossing, landing, cost, range and opportunity reach");
 }
}
