using System;
class MassHealChecks
{
 static void C(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Main(){
  int total;C(MassHealRules.ValidAllocation(new[]{"100","250","350"},out total)&&total==700,"shared pool supports multiple units");
  C(MassHealRules.ValidAllocation(new[]{"0","10"},out total)&&total==10,"need not spend entire pool");
  foreach(var values in new[]{new[]{"701"},new[]{"400","301"},new[]{"1.5"},new[]{"-1"},new[]{"2147483647","2147483647"}})C(!MassHealRules.ValidAllocation(values,out total),"invalid allocation rejected before committing");
  var combat=new CombatState();var senses=new SensoryAfflictions();senses.Add(false,true,20,0);senses.Add(false,false,10,0);combat.ApplyPoison(false);
  C(MassHealRules.Apply(combat,senses,false,100,1)==0&&senses.Pending(false,1).Count==2&&combat.MonsterPoisoned,"full target restores zero and keeps conditions");
  combat.ApplyDamage(1);C(MassHealRules.Apply(combat,senses,false,100,1)==1&&senses.Pending(false,1).Count==0&&!combat.MonsterPoisoned,"one HP actually restored removes all listed sources");
  combat.ApplyDamage(100);combat.BlockHealing(false);combat.ApplyPoison(false);senses.Add(false,true,10,1);
  C(MassHealRules.Apply(combat,senses,false,100,2)==0&&combat.MonsterPoisoned&&senses.Blinded(false,2),"healing prevention also prevents mass-heal cleansing");
  var c=new CombatState();c.ApplyDamage(1);c.MonsterParalyzed=true;c.MonsterSleepStage=2;C(MassHealRules.Apply(c,senses,false,1,2)==1&&c.MonsterParalyzed&&c.MonsterSleepStage==2,"unrelated paralysis and sleep retained");
  Console.WriteLine("PASS: 700-point allocation, integers, overflow, actual-heal cleansing, full HP and healing prevention");
 }
}
