using System;
class EnfeeblementChecks
{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Main(){
  var effect=new EnfeeblementEffect();
  effect.Cast(false,false,14,10,2);
  Check(effect.Active(false,10,2)&&!effect.Weakened(false,10,2),"successful save only attack branch");
  Check(!effect.ConsumeAttack(true,0,10,2),"other actor cannot consume");
  Check(effect.ConsumeAttack(false,4,10,2)&&!effect.ConsumeAttack(false,0,10,2),"any attack consumes once");
  effect.Cast(false,false,14,10,2);Check(!effect.Active(false,11,3),"expires at caster next turn");
  effect.Cast(false,true,16,10,2);
  Check(effect.StrengthDisadvantage(false,0,10,2)&&!effect.StrengthDisadvantage(false,1,10,2)&&!effect.StrengthDisadvantage(false,-1,10,2),"only strength, not dexterity or no-ability");
  Check(effect.ConsumeAttack(false,0,10,2)&&effect.ConsumeAttack(false,0,11,3),"failed-save weakness persists across attacks and turns");
  Check(!effect.ConsumeAttack(false,4,11,3)&&effect.Weakened(false,11,3),"spell attack unaffected but damage remains weakened");
  Check(!effect.Active(false,9,2)&&!effect.Active(false,20,12),"duration boundaries");
  effect.Clear();Check(!effect.Active(false,11,3),"concentration and reset clear");
  Check(!effect.CanCast(long.MaxValue-9)&&effect.CanCast(long.MaxValue-10),"time overflow guarded");
  var raw=new long[13];raw[1]=10;raw[11]=4;raw[6]=6;
  var allocation=new DamageReductionAllocation(raw,8);var assigned=new int[13];assigned[1]=5;assigned[11]=3;long[] reduced=null;
  Check(allocation.TryAssign(assigned)&&allocation.TryCommit(out reduced),"one combined hit reduction");
  Check(reduced[1]==5&&reduced[11]==1&&reduced[6]==6,"weapon and additional damage share one budget");
  var combat=new CombatState();combat.MonsterEnergyResistance=1;
  Check(combat.AdjustDamagePacket(false,reduced)==9,"deduction precedes resistance with floor");
  combat.DamageImmunity=(player,type)=>type==11;
  Check(combat.AdjustDamagePacket(false,reduced)==8,"immunity after deduction");
  raw=new long[13];raw[1]=(long)int.MaxValue+9;
  Check(combat.AdjustDamagePacket(false,raw)==1073741828,"resistance before integer clipping");
  var negative=new long[13];negative[0]=-8;negative[5]=6;var floor=new DamageReductionAllocation(negative,8);
  Check(floor.Required==6,"negative typed bonus never offsets another type");
  Console.WriteLine("PASS: save branches, one-use attack, expiry, strength-only tests, concentration, combined damage, resistance order and overflow");
 }
}
