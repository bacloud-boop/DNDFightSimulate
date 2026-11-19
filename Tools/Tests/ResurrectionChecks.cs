using System;
class ResurrectionChecks
{
 static void C(bool ok,string m){if(!ok)throw new Exception(m);}
 static void Main(){
  var clock=new CorpseClock();clock.Observe(true,9,0);
  C(RevivalRules.Error("Resurrection",clock,long.MaxValue)!="","overflowing casting completion is rejected");
  long century=100L*365*14400;
  C(RevivalRules.Supported("Resurrection")&&RevivalRules.Duration("Resurrection")==600,"registered one-hour spell");
  C(RevivalRules.Error("Resurrection",clock,century-600)==""&&RevivalRules.Error("Resurrection",clock,century-599)!="","century deadline includes casting duration");
  clock.Clear();clock.Observe(true,13,0);C(RevivalRules.Error("Resurrection",clock,0)!="","undead at death rejected");
  C(RevivalRules.FullHp("Resurrection")&&RevivalRules.Ordeal("Resurrection")&&!RevivalRules.Ordeal("True Resurrection"),"full health plus ordeal, distinct from true resurrection");
  var c=new CombatState();c.KillMonster();C(c.Revive(false,c.MaxHp,RevivalRules.Ordeal("Resurrection"))&&c.Hp==c.MaxHp&&c.MonsterRevivalPenalty==-4,"resurrection restores full current maximum and adds minus four");
  c.KillMonster();c.Revive(false,c.MaxHp,true);C(c.MonsterRevivalPenalty==-4,"repeat ordeal does not stack");
  c.CompleteLongRest(false);C(c.MonsterRevivalPenalty==-3,"one rest reduces target ordeal by one");
  var player=new ResurrectionCasterState();var external=new ResurrectionCasterState();
  clock.Clear();clock.Observe(true,9,0);
  C(!ResurrectionCasterState.RequiresExertion("Resurrection",clock,ResurrectionCasterState.YearRounds-1)&&ResurrectionCasterState.RequiresExertion("Resurrection",clock,ResurrectionCasterState.YearRounds),"365-day threshold inclusive");
  clock.Preserve(ResurrectionCasterState.YearRounds-10);C(ResurrectionCasterState.RequiresExertion("Resurrection",clock,ResurrectionCasterState.YearRounds),"preservation extends revival deadline but does not change actual death date");
  C(!ResurrectionCasterState.RequiresExertion("True Resurrection",clock,ResurrectionCasterState.YearRounds),"true resurrection does not tax caster");
  external.ApplyExertion();C(!external.CanCast&&external.D20Disadvantage&&player.CanCast,"fixed external caster retains fatigue independently");
  C(external.Taxed&&!external.CompleteLongRest(0,4799)&&external.Taxed,"time alone or incomplete rest does not clear fatigue");
  C(external.CompleteLongRest(0,4800)&&external.CanCast&&!external.D20Disadvantage,"completed eight-hour rest clears both restrictions");
  external.ApplyExertion();C(!external.CanLongRest(14399)&&external.Taxed&&external.CanLongRest(14400),"external long-rest benefit obeys daily interval");
  C(!external.CanLongRest(long.MaxValue)&&!external.CompleteLongRest(-1,4800),"invalid and overflowing rest times rejected");
  player.ApplyExertion();C(player.D20Disadvantage&&external.Taxed,"both caster identities may be taxed without sharing recovery");
  player.CompleteLongRest(0,4800);C(player.CanCast&&external.Taxed,"player long rest does not clear external fatigue");
  C(PoisonProtectionEffect.SelectDie(20,1,false,external.D20Disadvantage)==1,"fatigue applies to chosen D20, retaining natural-one outcome");
  C(PoisonProtectionEffect.SelectDie(20,0,true,external.D20Disadvantage)==20,"one advantage cancels fatigue disadvantage without extra dice");
  external.Reset();C(external.CanCast&&external.LastLongRestStart==null,"training reset clears persistent caster state");
  Console.WriteLine("PASS: century deadline, undead restriction, full HP, ordeal, actual death threshold, separate casters, eight-hour rests and D20 cancellation");
 }
}
