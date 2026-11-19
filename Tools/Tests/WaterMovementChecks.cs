using System;
using UnityEngine;
class WaterMovementChecks
{
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void Main(){
  var m=new BattleMap();for(int y=0;y<5;y++)m.SetWater(V(0,y),true);m.Lock();
  Check(m.PlayerMovementCost(V(0,1))==10&&m.MovePlayer(V(0,1))&&m.MovementRemaining==20,"fully wet move costs double without swim speed");
  Check(m.MovePlayer(V(0,2))&&m.MovePlayer(V(0,3))&&m.MovementRemaining==0&&!m.MovePlayer(V(0,4)),"cumulative water movement cannot exceed budget");
  m.RefreshMovement();m.PlayerSwimmingSpeed=30;Check(m.PlayerMovementCost(V(0,4))==5&&m.MovePlayer(V(0,4))&&m.MovementRemaining==25,"swim speed removes water surcharge");
  var large=new BattleMap();large.TrySetFootprint(true,2);for(int y=1;y<=2;y++)for(int x=1;x<=2;x++)large.SetWater(V(x,y),true);
  Check(large.WaterMovementCost(V(0,0),V(0,1),2,0)==5,"partly flooded footprint remains normal movement");
  Check(large.WaterMovementCost(V(0,0),V(1,1),2,0)==14,"fully flooded large destination costs double diagonal distance");
  Check(large.WaterMovementCost(V(0,0),V(1,1),2,0,true)==21,"crawling and swimming extra costs add rather than multiply");
  Check(large.WaterMovementCost(V(0,0),V(1,1),2,30)==7,"swimming retains established diagonal distance");
  var summons=new BattleMap();summons.SetWater(V(0,2),true);summons.Lock();var sea=new BestialSpiritRules(2,5).CreateWaterUnit(1);summons.AddSummon(sea,V(0,1));sea.BeginTurn(1);
  Check(summons.MoveSummon(1,V(0,2))&&sea.MovementRemaining==25,"sea summon pays only ordinary distance");
  Check(!summons.MoveSummon(1,V(0,0))&&sea.MovementRemaining==25,"occupied destination leaves summon budget unchanged");
  Console.WriteLine("PASS: full-footprint swimming costs, cumulative budgets, size boundaries and sea summon movement");
 }
}
