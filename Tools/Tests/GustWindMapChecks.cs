using System;
using UnityEngine;
class GustWindMapChecks
{
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void C(bool b,string text){if(!b)throw new Exception(text);}
 static void Main()
 {
  BreathTemplate line;C(BreathTemplate.TryCreate(0,0,1,0,1,25,.5,true,out line,false,60,10),"create 10-foot line");C(line.Cell(4,1)&&!line.Cell(4,2),"line width is ten feet");
  C(GustWindEffect.OverlapsCloud(line,5,5,4)&&!GustWindEffect.OverlapsCloud(line,5,6,4),"exact edge circle overlap rather than cloud-center check");
  var m=new BattleMap();m.Place(false,V(8,0));m.SetWater(V(8,0),true);m.SetWater(V(9,0),true);m.MonsterSwimmingSpeed=30;m.Lock();m.AgainstGust=(id,a,b,n)=>GustWindEffect.Against(line,a.x,a.y,b.x,b.y,n);
  C(m.MonsterMovementCost(V(5,0))==30,"15 feet approaching costs 30");C(m.MonsterMovementCost(V(9,0))==5,"moving away costs normal");
  var meter=m.RouteMeter(V(8,0),V(5,0),false);int cost=0;var previous=V(8,0);foreach(var p in BattleMap.MovementAnchors(previous,V(5,0))){cost+=m.RouteStepCost(meter,previous,p,1,0,-1);previous=p;}C(cost==30,"whole route and per-step wind costs match");
  C(m.SwitchWaterState(false,true),"dive monster");C(!m.WindStep(-1,V(8,0),V(9,0),1)&&m.MonsterMovementCost(V(9,0))==5,"submerged route avoids wind surcharge");C(m.MonsterMovementCost(V(5,0))==30,"leaving water resumes wind accounting during route preview");m.SwitchWaterState(false,false);
  int before=m.MonsterMovementRemaining,notified=0;m.MovementObserved=(id,a,b,n,o,land)=>notified++;C(m.ForceWindStep(-1,V(9,0))&&m.MonsterMovementRemaining==before&&notified==1,"forced push observes terrain without spending movement");C(!m.ForceWindStep(-1,V(10,0)),"boundary stops push");
  m=new BattleMap();m.Place(false,V(7,0));m.Lock();var u=new SummonedUnit(7,"spirit",10,10,30,2,1);C(m.AddSummon(u,V(3,3)),"deploy summon");int spent=u.MovementRemaining;C(m.ForceWindStep(7,V(4,3))&&u.X==4&&u.MovementRemaining==spent,"summon forced push has no movement cost");
  C(m.DeployRescue(V(0,3),1,true),"deploy rescue");C(m.ForceWindStep(BattleMap.RescueId,V(1,3))&&m.RescuePosition.Value==V(1,3),"rescue forced movement");
  m=new BattleMap();m.Place(false,V(4,0));m.Lock();C(!m.ForceWindStep(-1,V(0,0)),"cannot cross multiple cells or occupy another body");
  m=new BattleMap();m.Place(false,V(8,0));m.Lock();m.AgainstGust=(id,a,b,n)=>true;C(m.JumpMovementCost(false,V(2,0))==20,"full wind jump costs 20 rather than 40");C(m.Jump(false,V(2,0))&&m.MonsterMovementRemaining==10,"actual jump spends wind-adjusted movement");
  m=new BattleMap();m.Place(false,V(8,0));m.Lock();m.AgainstGust=(id,a,b,n)=>b.x>=4;C(m.JumpMovementCost(false,V(2,0))==17,"four of six inverse-wind steps add rounded 10*4/6");m.SpendMonsterMovement(14);C(!m.CanJump(false,V(2,0))&&!m.Jump(false,V(2,0)),"insufficient wind jump budget is rejected atomically");
  C(m.JumpMovementCost(false,V(int.MaxValue,int.MaxValue))==int.MaxValue,"invalid huge jump coordinates return without enumerating path");
  var diagonal=new RouteCostMeter(14,14,false);C(diagonal.Next(7,false,false,true)+diagonal.Next(7,false,false,true)==28,"diagonal integer rounding is conserved");
  Console.WriteLine("PASS: production Gust geometry, partial-cloud intersection, route/step costs, forced movement, summons, rescue and integer rounding");
 }
}
