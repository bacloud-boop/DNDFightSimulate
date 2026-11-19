using System;
using UnityEngine;
class SummonChecks
{
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void Main(){
  var map=new BattleMap();map.Place(false,V(5,0));map.Lock();
  var a=new SummonedUnit(1,"A",30,13,30,1,2);var b=new SummonedUnit(2,"B",20,12,40,2,1);
  Check(map.AddSummon(a,V(4,0))&&map.AddSummon(b,V(3,0)),"distinct summons can be placed");
  Check(!map.CanMoveTo(V(3,0))&&!map.CanTeleportPlayer(V(4,0),30),"living summons block final player destinations");
  Check(!map.AddSummon(new SummonedUnit(1,"duplicate",10,10,30,2,1),V(2,0)),"unit identity unique");
  Check(map.NearestEnemyInReach(true,5)==1,"closest reachable summon becomes target");
  Check(a.BeginTurn(1)&&!a.BeginTurn(1),"reselecting active unit cannot refill resources");
  Check(a.Attack()&&a.AttacksRemaining==1&&a.Attack()&&!a.Attack(),"attacks individually consume one shared attack action");
  Check(!a.Dodge()&&a.SpendMovement(20)&&!a.SpendMovement(11)&&a.MovementRemaining==10,"summon action and movement budgets independent");
  a.EndTurn();Check(!a.SpendMovement(1)&&!a.Attack()&&a.UseReaction()&&!a.UseReaction(),"off turn actions blocked but one reaction available");
  Check(!a.BeginTurn(1)&&a.BeginTurn(2)&&a.ReactionAvailable&&a.MovementRemaining==30,"own turn refreshes reaction and movement");
  a.Damage(30);Check(!a.Alive&&!a.TurnActive&&!a.UseReaction()&&a.Heal(10)==0,"summon vanishes at zero without death saves");
  Check(map.NearestEnemyInReach(true,5)==-1&&map.NearestEnemyInReach(true,10)==2,"dead summons ignored by target selection");
  Check(map.CanTeleportPlayer(V(4,0),30),"dead summon no longer occupies space");
  map.TeleportPlayer(V(4,1),30);Check(map.NearestEnemyInReach(true,10)==0,"closest player also selectable");
  map.Unlock();Check(map.Summons.Count==0,"reset removes summoned occupancy");
  var spirit=new BestialSpiritRules(2,5);var high=new BestialSpiritRules(9,11);
  Check(spirit.MaxHp==30&&spirit.Armor==13&&spirit.Attacks==1&&spirit.FixedDamage==6,"second level spirit stats");
  Check(high.MaxHp==65&&high.Armor==20&&high.Attacks==4&&high.FixedDamage==13,"slot scaling and floor multiattack");
  Check(spirit.CreateLandUnit(3).Size==1&&spirit.Ability(3)==14&&spirit.Ability(4)==4,"small spirit with correct wisdom and intelligence order");
  Check(BestialSpiritRules.PackTactics(true,false)&&!BestialSpiritRules.PackTactics(true,true),"pack tactics requires active adjacent ally");
  Check(!spirit.CreateLandUnit(4).CanBePolymorphTarget&&!spirit.CreateWaterUnit(5).CanBePolymorphTarget,"spirits without CR or levels cannot be polymorphed");
  var barrier=new BattleMap();barrier.Place(false,V(5,0));barrier.Lock();
  var guard=spirit.CreateLandUnit(6);Check(barrier.AddSummon(guard,V(3,0)),"place guard in enemy path");
  Check(!barrier.CanMoveMonsterTo(V(1,0))&&barrier.CanMoveMonsterTo(V(5,2)),"dummy cannot walk through an enemy summon");
  var reserve=new BattleMap();reserve.Place(false,V(9,9));reserve.Lock();Check(reserve.TrySetRecoverySize(true,2),"reserve larger original body");
  var blocker=spirit.CreateLandUnit(7);Check(!reserve.AddSummon(blocker,V(1,0))&&reserve.Summons.Count==0,"summoning cannot strand original form recovery");
  Check(!reserve.CanAddSummon(blocker,V(1,0))&&reserve.Summons.Count==0&&blocker.X==0&&blocker.Y==0,"illegal placement preview preserves list and pose");
  Check(reserve.CanAddSummon(blocker,V(3,0))&&reserve.Summons.Count==0&&blocker.X==0&&blocker.Alive,"legal preview does not actually summon or dismiss unit");
  Check(reserve.AddSummon(blocker,V(3,0))&&blocker.BeginTurn(1),"place outside recovery area");
  Check(!reserve.CanMoveSummon(7,V(1,0))&&!reserve.MoveSummon(7,V(1,0))&&blocker.X==3&&blocker.MovementRemaining==30,"illegal ally movement restores pose and spends no movement");
  Console.WriteLine("PASS: summon occupancy, independent resources, manual multiattack, reaction refresh and nearest target selection");
 }
}
