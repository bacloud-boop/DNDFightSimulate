using System;
class PoisonProtectionChecks
{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Main(){
  var effect=new PoisonProtectionEffect();var combat=new CombatState();long now=10;
  Check(effect.Grant(true,now)&&!effect.Active(true,9)&&effect.Active(true,609)&&!effect.Active(true,610),"one hour boundary");
  combat.DamageResistance=(player,type)=>type==9&&effect.Active(player,now);combat.PoisonConditionSaveAdvantage=player=>effect.Active(player,now);
  Check(combat.AdjustTypedDamage(true,11,9)==5&&combat.AdjustTypedDamage(false,11,9)==11&&combat.AdjustTypedDamage(true,11,5)==11,"typed actor-specific resistance");
  combat.PlayerEnergyResistance=9;Check(combat.AdjustTypedDamage(true,11,9)==5,"resistance does not stack");
  var packet=new long[13];packet[9]=11;packet[0]=6;Check(combat.AdjustPlayerPacket(packet)==11,"mixed packet");
  Check(combat.HasPoisonConditionSaveAdvantage(true)&&!combat.HasPoisonConditionSaveAdvantage(false)&&effect.SaveAdvantage(true,true,now)&&!effect.SaveAdvantage(true,false,now),"only poisoned-condition saves");
  Check(PoisonProtectionEffect.SelectDie(3,17,true,false)==17&&PoisonProtectionEffect.SelectDie(3,17,true,true)==3,"advantage and disadvantage cancel");
  combat.ApplyPoison(true);combat.ClearPoison(true);Check(!combat.PlayerPoisoned,"initial poison cured");combat.ApplyPoison(true);Check(combat.PlayerPoisoned,"new poison without save is not immune");
  Check(!effect.Grant(false,long.MaxValue)&&!effect.Active(false,now),"overflow atomic");
  now=610;combat.PlayerEnergyResistance=-1;Check(combat.AdjustTypedDamage(true,11,9)==11&&!combat.HasPoisonConditionSaveAdvantage(true),"expiry");
  effect.Clear();Check(!effect.Active(true,10),"reset");Console.WriteLine("PASS: duration, poison cure, resistance stacking, mixed packets, tagged saves, cancellation and expiry");
 }
}
