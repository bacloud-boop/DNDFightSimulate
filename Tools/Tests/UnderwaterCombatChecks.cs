using System;
class UnderwaterCombatChecks
{
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Main(){
  Check(UnderwaterCombatRules.Disadvantage(true,true,true,0,false),"nonpiercing melee weapon without swim speed disadvantaged");
  Check(!UnderwaterCombatRules.Disadvantage(true,true,true,0,true),"piercing melee and natural weapon exception");
  Check(!UnderwaterCombatRules.Disadvantage(true,true,true,30,false),"swim speed avoids melee penalty");
  Check(UnderwaterCombatRules.Disadvantage(true,true,false,30,true),"swim speed does not remove ranged weapon penalty");
  Check(!UnderwaterCombatRules.Disadvantage(true,false,false,0,false)&&!UnderwaterCombatRules.AutoMiss(true,false,false,100,30),"spell attacks unaffected by weapon specific rule");
  Check(UnderwaterCombatRules.AutoMiss(true,true,false,31,30)&&!UnderwaterCombatRules.AutoMiss(true,true,false,30,30),"normal range boundary and automatic miss");
  Check(!UnderwaterCombatRules.Disadvantage(false,true,true,0,false),"surface attacks unaffected");
  var c=new CombatState();var form=new MonsterFormData();Check(c.MonsterAttackIsWeapon&&form.Weapon,"current and original forms default to weapons");
  form.Weapon=false;Check(!form.Copy().Weapon,"independent original form preserves attack nature");
  c.PlayerUnderwater=true;c.MonsterUnderwater=true;
  Check(c.AdjustTypedDamage(true,9,5)==4&&c.AdjustTypedDamage(false,9,5)==4,"submerged actors resist fire with integer floor");
  c.PlayerEnergyResistance=5;Check(c.AdjustTypedDamage(true,9,5)==4,"spell and underwater resistance do not stack");
  var packet=new long[13];packet[5]=9;packet[1]=4;Check(c.AdjustPlayerPacket(packet)==8,"only fire component of mixed packet halved");
  c.PlayerUnderwater=false;c.PlayerEnergyResistance=-1;Check(c.AdjustTypedDamage(true,9,5)==9,"surface state grants no underwater resistance");
  Console.WriteLine("PASS: underwater weapon classification, natural attack exceptions, ranged limits and spell exclusions");
 }
}
