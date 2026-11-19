using System;
using UnityEngine;
class WaterMapChecks
{
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void Main(){
  var m=new BattleMap();Check(!m.IsWater(V(0,0))&&m.SetWater(V(0,0),true),"maps begin dry and can be painted");
  Check(!m.SetWater(V(10,0),true)&&!m.IsWater(V(-1,0)),"brush cannot leave map");
  Check(m.WaterCells(V(0,0),2)==1,"multi-cell body coverage counted without deciding submerged policy");
  m.Select(1);Check(!m.IsWater(V(0,0))&&m.SetWater(V(14,14),true),"each map owns independent water layout");
  m.Select(0);Check(m.IsWater(V(0,0)),"switching back preserves original layout");
  m.Lock();Check(!m.SetWater(V(1,0),true)&&!m.ClearWater(),"terrain locked during combat");
  m.Unlock();Check(m.ClearWater()&&!m.IsWater(V(0,0)),"deployment can clear water");
  m.SetWater(V(0,0),true);m.Lock();
  Check(m.SwitchWaterState(true,true)&&m.MovementRemaining==25,"player dives for five movement");
  Check(!m.SwitchWaterState(true,true)&&m.MovementRemaining==25,"same state cannot spend movement twice");
  Check(m.MovePlayer(V(0,1))&&!m.PlayerAquatic.Submerged&&m.MovementRemaining==20,"moving onto land clears submerged state without another voluntary switch charge");
  Console.WriteLine("PASS: water painting, bounds, per-map persistence, body coverage and combat lock");
 }
}
