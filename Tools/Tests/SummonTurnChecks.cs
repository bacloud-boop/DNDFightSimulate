using System;
using System.Collections.Generic;
class SummonTurnChecks
{
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static SummonedUnit U(int id)=>new SummonedUnit(id,"spirit",30,13,30,1,2);
 static void Main(){
  var a=U(1);var b=U(2);var c=U(3);var units=new List<SummonedUnit>{a,b,c};var queue=new SummonTurnQueue();bool changed;
  a.ReceiveVerbalOrders(SummonCommand.AssistCombat,out changed);a.UseReaction();b.UseReaction();
  Check(queue.Begin(units,1)&&queue.Current==a&&a.ReactionAvailable&&!b.ReactionAvailable,"refresh reaction only when own turn starts");
  Check(a.Attack()&&!queue.Begin(units,1)&&a.AttacksRemaining==1,"reopening cannot refill active follower");
  c.Damage(30);Check(queue.EndCurrent()&&queue.Current==b&&!a.TurnActive&&b.ReactionAvailable,"followers act sequentially and refresh independently");
  var late=U(4);units.Add(late);b.Damage(30);queue.Reconcile();Check(!queue.Active&&!late.TurnActive,"dead queued unit skipped and late arrivals excluded from snapshot");
  Check(!queue.Begin(units,1)&&queue.Begin(units,2)&&queue.Current==a&&a.HasVerbalOrders,"new phase retains old orders without speech");
  queue.Cancel();Check(!queue.Active&&!a.TurnActive&&!a.Attack()&&!queue.Begin(units,2),"cancel closes actions without allowing same-round replay");
  Check(queue.Begin(units,3)&&queue.EndCurrent()&&queue.Current==late,"surviving late arrival joins next phase");queue.Cancel();
  var rules=new BestialSpiritRules(2,5);var session=new BestialSpiritSession();var spirit=rules.CreateLandUnit(5);
  Check(session.Begin(spirit,rules,20)&&!session.ShouldDismiss(619,false,true)&&session.ShouldDismiss(620,false,true),"one hour duration with exact expiry boundary");
  Check(!session.Begin(U(6),rules,21)&&session.Unit==spirit,"invalid replacement leaves active session unchanged");
  Check(session.ShouldDismiss(21,true,true)&&session.ShouldDismiss(21,false,false),"incapacitation and concentration loss dismiss");
  spirit.BeginTurn(1);Check(session.Dismiss()==spirit&&!spirit.Alive&&!spirit.TurnActive&&!spirit.UseReaction()&&!session.Active,"dismissal clears all active resources and session ownership");
  Check(session.Dismiss()==null&&!session.Begin(U(7),rules,long.MaxValue),"dismiss is idempotent and invalid duration cannot overflow");
  var retained=U(8);var afterSession=new BestialSpiritSession();var afterQueue=new SummonTurnQueue();Check(afterSession.Begin(retained,rules,100)&&afterQueue.Begin(new[]{retained},1),"active summon before aftermath");
  retained.Damage(7);retained.SpendMovement(10);afterQueue.Cancel();
  Check(!afterQueue.Begin(new[]{retained},2,true)&&afterSession.Active&&retained.Hp==23&&!retained.TurnActive&&!retained.Attack(),"aftermath blocks control without dismissing or healing summon");
  Check(!afterSession.ShouldDismiss(200,false,true)&&afterSession.Expires==700,"aftermath does not reset or freeze remaining duration");
  Check(afterQueue.Begin(new[]{retained},2)&&retained.Hp==23&&retained.MovementRemaining==30,"resume starts own turn without resetting HP");
  afterQueue.Cancel();Check(afterSession.ShouldDismiss(700,false,true),"summon can expire while battle remains paused");
  Console.WriteLine("PASS: follower sequence, resource timing, phase snapshots, cancellation, persistent orders and concentration lifetime");
 }
}
