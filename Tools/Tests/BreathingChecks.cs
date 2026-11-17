using System;
class BreathingChecks
{
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Main(){
  Check(BreathingState.HoldRounds(3)==5&&BreathingState.HoldRounds(10)==10&&BreathingState.HoldRounds(16)==40,"constitution modifier floors and minimum thirty seconds");
  var b=new BreathingState();b.Observe(false,0,16);b.Observe(false,20,16);
  Check(b.BreathEndsAt==40&&!b.EndTurn(1,39),"observing exposure does not refill breath");
  Check(b.EndTurn(2,40)&&b.SuffocationExhaustion==1&&!b.EndTurn(2,40),"one level at own turn end, no repeated same turn increment");
  Check(b.D20Penalty==-2&&b.SpeedPenalty==5,"2024 exhaustion penalties");
  b.Observe(true,41,16);Check(b.SuffocationExhaustion==0&&b.CanBreathe,"breathing removes suffocation exhaustion");
  b.Observe(false,42,16,true);Check(b.BreathEndsAt==42,"choking bypasses holding breath");
  for(int i=0;i<6;i++)b.EndTurn(3+i,42+i);
  Check(b.Fatal&&b.SuffocationExhaustion==6,"six levels are fatal");
  b.Observe(true,50,16);Check(b.Fatal,"fresh air cannot revive a dead creature");
  b.Reset();Check(!b.Fatal&&b.CanBreathe&&b.SuffocationExhaustion==0,"battle reset clears breath state");
  var protectedBreath=new BreathingState();var ward=new DeathWardEffect();ward.Cast(true,0);protectedBreath.Observe(false,0,10,true);
  int calls=0;Func<bool> guard=()=>{calls++;return ward.Consume(true,6);};
  for(int i=0;i<5;i++)protectedBreath.EndTurn(i,i,guard);
  Check(calls==0&&ward.Active(true,5),"ward untouched by nonfatal exhaustion");
  Check(!protectedBreath.EndTurn(5,5,guard)&&!protectedBreath.Fatal&&protectedBreath.SuffocationExhaustion==5&&!ward.Active(true,6),"ward negates sixth level and is consumed");
  Check(!protectedBreath.EndTurn(5,5,guard)&&calls==1,"same turn cannot retrigger or kill after ward protection");
  Check(protectedBreath.EndTurn(6,6,guard)&&protectedBreath.Fatal,"continued suffocation on next turn kills without ward");
  protectedBreath.Reset();protectedBreath.Observe(false,0,10,true);for(int i=0;i<5;i++)protectedBreath.EndTurn(i,i);
  protectedBreath.Observe(true,6,10);Check(!protectedBreath.EndTurn(6,6,guard)&&protectedBreath.SuffocationExhaustion==0,"regaining breath before next turn prevents further suffocation");
  var immune=new BreathingState{ExhaustionImmune=true};immune.Observe(false,0,10,true);
  Check(!immune.EndTurn(1,0,()=>{throw new Exception("immune creature must not consume ward");})&&immune.SuffocationExhaustion==0,"immunity prevents suffocation exhaustion");
  immune.ExhaustionImmune=false;for(int i=2;i<=6;i++)immune.EndTurn(i,i);
  Check(immune.SuffocationExhaustion==5,"ordinary suffocation resumes after immunity ends");immune.ExhaustionImmune=true;
  Check(immune.D20Penalty==0&&immune.SpeedPenalty==0&&!immune.EndTurn(7,7)&&immune.SuffocationExhaustion==5,"immunity suppresses existing penalties without deleting stored exhaustion");
  immune.ExhaustionImmune=false;Check(immune.D20Penalty==-10&&immune.EndTurn(8,8)&&immune.Fatal,"ending immunity restores penalties and fatal increase");
  immune.ExhaustionImmune=true;Check(immune.ReconcileRevival(false)&&!immune.Fatal&&immune.SuffocationExhaustion==6,"revival with immunity retains but suppresses fatal exhaustion");
  immune.Observe(true,9,10);immune.ExhaustionImmune=false;Check(!immune.Fatal&&immune.SuffocationExhaustion==0,"breathing after revival clears exhaustion without causing a later false death");
  Console.WriteLine("PASS: breath duration, choking, own-turn suffocation, recovery and irreversible death");
 }
}
