using System;
using UnityEngine;
class SummonIntegrationChecks
{
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void Main(){
  var combat=new CombatState();int playerEvents=0,monsterEvents=0;
  combat.DamageDealt=player=>{if(player)playerEvents++;else monsterEvents++;};
  combat.ApplyDamage(1,sourcePlayer:null);combat.ApplyPlayerDamage(1,sourcePlayer:null);
  Check(playerEvents==0&&monsterEvents==0,"summon damage does not break another actor's invisibility");
  combat.ApplyDamage(1);combat.ApplyPlayerDamage(1);
  Check(playerEvents==1&&monsterEvents==1,"existing player and monster damage attribution retained");
  var unit=new SummonedUnit(1,"spirit",30,13,30,1,2);unit.BeginTurn(1);unit.SpendMovement(10);
  unit.Breathing.Observe(false,0,16,true);unit.Breathing.EndTurn(1,1);
  Check(unit.EffectiveSpeed==25&&unit.MovementRemaining==15,"suffocation speed penalty preserves spent movement");
  Check(unit.Dash()&&unit.MovementRemaining==40,"dash uses effective speed rather than unaffected base speed");
  unit.Breathing.Observe(true,2,16);Check(unit.MovementRemaining==50,"breathing recovery restores speed without refunding spent distance");
  var map=new BattleMap();map.Place(false,V(5,0));map.Lock();var spirit=new SummonedUnit(2,"ally",30,13,30,1,1);
  Check(map.AddSummon(spirit,V(4,0))&&spirit.BeginTurn(1),"place controllable ally");
  int distance;Check(map.NearestEnemyFrom(V(5,1),1,true,int.MaxValue,out distance)==2&&distance==7,"hypothetical chase chooses nearest summon rather than player");
  Vector2Int retreat;Check(map.ChooseSummonRetreat(2,out retreat)&&BattleMap.BodyDistance(retreat,1,map.Monster,1)>5&&spirit.MovementRemaining==30,"retreat preview improves safety without spending movement");
  Check(map.MoveSummon(2,retreat)&&spirit.MovementRemaining<30,"retreat commit pays actual movement");
  map.RemoveSummon(2);Check(!spirit.Alive&&!spirit.TurnActive&&!spirit.UseReaction(),"removed reference cannot continue acting");
  var queue=new SummonTurnQueue();queue.Begin(new[]{new SummonedUnit(3,"old",10,10,30,1,1)},8);queue.Reset();
  Check(queue.Begin(new[]{new SummonedUnit(4,"new",10,10,30,1,1)},1)&&queue.Active,"reset accepts new battle sequence");
  Check(SummonGeometry.Distance(new CreatureCover.Body(0,0,1),new CreatureCover.Body(1,1,1))==7,"snapshot geometry preserves diagonal integer feet");
  Check(SummonGeometry.Distance(new CreatureCover.Body(0,0,2),new CreatureCover.Body(2,0,1))==5,"snapshot geometry includes full occupied body");
  Check(SummonGeometry.Distance(new CreatureCover.Body(0,0,1),new CreatureCover.Body(1.04,0,1))==5&&SummonGeometry.Distance(new CreatureCover.Body(0,0,1),new CreatureCover.Body(4,0,1))==20,"pack tactics uses reaction snapshot instead of original moving position");
  Console.WriteLine("PASS: third-party damage, speed changes, nearest-target chase, retreat, removed resources and battle reset");
 }
}
