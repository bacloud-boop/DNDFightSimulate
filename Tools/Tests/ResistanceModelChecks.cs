using System;
class ResistanceModelChecks
{
 static void C(bool b,string message){if(!b)throw new Exception(message);}
 static long[] Packet(int type,long n){var r=new long[14];r[type]=n;return r;}
 static void Main()
 {
  var effect=new ResistanceEffect();C(effect.Begin(0,0,0),"begin");var combat=new CombatState();combat.IncomingDamage=(p,r)=>effect.Reduce(p?0:-1,r,4,1,0);combat.DamageResistance=(p,t)=>true;C(combat.ResolveTypedPacket(true,Packet(0,10))[0]==5,"preview does not consume");C(combat.AdjustIncomingTypedDamage(true,10,0)==3,"numeric reduction before resistance");combat.ApplyPlayerDamage(3);C(combat.PlayerHp==7,"actual primary damage");
  effect=new ResistanceEffect();effect.Begin(8,5,0);var unit=new SummonedUnit(8,"unit",20,12,30,2,1);unit.Aquatic.TrySetSubmerged(true,1,1);unit.IncomingDamage=r=>effect.Reduce(8,r,4,1,0);C(unit.Damage(10,5)==3&&unit.Hp==17,"summon numerical reduction occurs before underwater fire resistance");
  effect=new ResistanceEffect();effect.Begin(8,5,0);unit.IntrinsicVulnerability=t=>t==5;C(unit.Damage(10,5)==6&&unit.Hp==11,"summon reduction then resistance then vulnerability");
  var rescue=new RescueCarrier();effect=new ResistanceEffect();effect.Begin(int.MinValue,0,0);rescue.IncomingDamage=r=>effect.Reduce(int.MinValue,r,4,1,0);rescue.ExtraResistance=t=>t==0;C(rescue.TakeDamage(10,damageType:0)==3&&rescue.Hp==7,"rescue single damage ordering");effect=new ResistanceEffect();effect.Begin(int.MinValue,0,0);var packet=Packet(0,8);packet[1]=2;C(rescue.TakeDamagePacket(packet)==4&&rescue.Hp==3,"rescue mixed packet reduces selected type before resistance");
  effect=new ResistanceEffect();effect.Begin(0,2,0);C(effect.Available(0,1,0),"new target starts without a prior benefit");C(effect.Reduce(0,Packet(2,2),4,1,0)[2]==0&&!effect.Available(0,1,0),"clamp and claim");C(effect.Available(0,2,0)&&!effect.Active(10),"global turn reset and minute expiry");effect.Clear();effect.Begin(0,2,0);C(!effect.Available(0,1,0),"recast preserves per-creature turn limit");effect.Clear(true);effect.Begin(0,2,0);C(effect.Available(0,1,0),"battle reset clears turn claims");
  int fires=0;combat=new CombatState();effect=new ResistanceEffect();effect.Begin(0,5,0);combat.IncomingDamage=(p,r)=>effect.Reduce(0,r,4,1,0);combat.FireExposure=p=>fires++;C(combat.AdjustIncomingTypedDamage(true,3,5)==0&&fires==1,"fire exposure survives full numerical damage prevention exactly once");
  effect=new ResistanceEffect();effect.Begin(8,5,0);unit=new SummonedUnit(8,"fire",20,12,30,2,1);unit.FireExposure=()=>fires++;unit.IncomingDamage=r=>effect.Reduce(8,r,4,1,0);C(unit.Damage(3,5)==0&&fires==2&&unit.LastDamageReceived==0,"summon exposure preserved without false received damage");
  effect=new ResistanceEffect();effect.Begin(int.MinValue,5,0);rescue=new RescueCarrier();rescue.FireExposure=()=>fires++;rescue.IncomingDamage=r=>effect.Reduce(int.MinValue,r,4,1,0);C(rescue.TakeDamagePacket(Packet(5,3))==0&&fires==3,"rescue mixed fire exposure preserved");
  Console.WriteLine("PASS: actual combat, summon and rescue model callbacks preserve damage order, hit points, mixed packets, underwater resistance and vulnerability");
 }
}
