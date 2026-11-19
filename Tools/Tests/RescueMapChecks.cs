using System;
using UnityEngine;
class RescueMapChecks
{
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void Main(){
  var m=new BattleMap();Check(!m.DeployRescue(V(0,2),1,true),"rescue cannot deploy before locked battle scene");m.Lock();
  Check(!m.CanDeployRescue(V(4,4),1,true)&&m.CanDeployRescue(V(4,4),1,false),"entry policy explicitly distinguishes edge from interior");
  Check(!m.DeployRescue(V(0,0),1,true)&&m.DeployRescue(V(0,2),1,true),"rescue spawn cannot overlap bodies");
  Check(!m.DeployRescue(V(0,3),1,true)&&!m.CanMoveTo(V(0,2)),"unique rescue and occupied destination");
  var r=new RescueCarrier();r.BeginTurn(1);Check(m.MoveRescue(V(0,1),r.Move)&&r.MovementRemaining==25&&m.MovementRemaining==30,"rescue movement spends only rescue budget");
  Check(!m.MoveRescue(V(0,0),r.Move)&&r.MovementRemaining==25&&m.RescuePosition==V(0,1),"blocked movement atomic");
  Check(!m.MoveRescue(V(0,2),cost=>false)&&m.RescuePosition==V(0,1),"insufficient movement cannot move geometry");
  m.RemoveRescue();Check(!m.RescuePosition.HasValue&&m.CanMoveTo(V(0,2)),"removing rescue clears occupancy");
  Console.WriteLine("PASS: rescue deployment policy, footprint, independent movement and atomic failures");
 }
}
