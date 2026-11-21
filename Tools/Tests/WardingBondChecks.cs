using System;
class WardingBondChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var bond=new WardingBondEffect();C(!bond.Begin(0,0)&&bond.Begin(-1,10)&&bond.Active(609)&&!bond.Active(610),"another creature only; duration one hour");
        C(bond.Validate(10,true,60)&&!bond.Validate(10,true,61)&&!bond.Validate(10,true,5),"crossing sixty permanently breaks link, returning cannot restore it");
        bond.Begin(-1,10);C(!bond.Validate(10,false,5),"caster zero HP ends link");
        var combat=new CombatState();bond.Begin(-1,0);combat.WardBonus=p=>!p&&bond.Active(0)?1:0;combat.UniversalResistance=p=>!p&&bond.Active(0);combat.DamageResistance=(p,t)=>p&&t==5;
        combat.DamageApplied=(p,packet)=>{if(p||!bond.Active(0))return;var shared=combat.ResolveTypedPacket(true,packet);int amount=0;foreach(long x in shared)amount+=(int)x;combat.ApplyPlayerDamage(amount,sourcePlayer:null,resolvedTypes:shared);};
        C(combat.MonsterArmorClass==11&&combat.PlayerArmorClass==10,"AC bonus applies only to protected creature");
        combat.GrantMonsterTemporaryHp(10);int hp=combat.PlayerHp;
        int fire=combat.AdjustTypedDamage(false,12,5);combat.ApplyDamage(fire);C(fire==6&&combat.MonsterTemporaryHp==4&&combat.PlayerHp==hp-3,"target resists to six; temporary HP absorbs it; caster separately resists original fire to three");
        var mixed=new long[13];mixed[5]=8;mixed[2]=6;hp=combat.PlayerHp;int dealt=combat.AdjustDamagePacket(false,mixed);combat.ApplyDamage(dealt);C(dealt==7&&combat.PlayerHp==hp-5,"mixed types preserve independent resistance without halving twice on target");
        // A simultaneous area has already calculated direct player damage when the
        // target's damage triggers a transfer. Its prepared packet must survive.
        combat.AdjustTypedDamage(true,8,5);int transferred=0;combat.DamageApplied=(p,packet)=>{if(p){if(packet.Length>5)transferred+=(int)packet[5];return;}var share=combat.ResolveTypedPacket(true,packet);combat.ApplyPlayerDamage(2,sourcePlayer:null,resolvedTypes:share);};
        combat.ApplyDamage(combat.AdjustTypedDamage(false,8,5));combat.ApplyPlayerDamage(4);C(transferred==6,"explicit transfer packets do not overwrite a pending direct area packet");
        var immune=new CombatState();immune.DamageImmunity=(p,t)=>t==5;C(immune.AdjustTypedDamage(true,8,5)==0,"caster immunity can remove transferred type entirely");immune.DamageVulnerability=(p,t)=>t==2;C(immune.AdjustTypedDamage(true,3,2)==6,"caster vulnerability doubles the corresponding transfer component");
        var unit=new SummonedUnit(1,"warded",20,12,30,2,1);unit.WardBonus=()=>1;int received=0;unit.DamageApplied=packet=>{foreach(long n in packet)received+=(int)n;};unit.ChooseTemporaryHp(10,true);unit.Damage(8,5);C(unit.Armor==13&&unit.Hp==20&&unit.TemporaryHp==6&&received==4,"summon AC, resistance and pre-temporary-HP event");
        unit.Damage(3,resolvedTypes:new long[]{0,0,3});C(received==7&&unit.TemporaryHp==3,"resolved mixed packet is not resisted a second time");
        var rescue=new RescueCarrier();rescue.ExtraResistance=t=>true;int rescued=0;rescue.DamageApplied=(n,t)=>rescued+=n;rescue.ChooseTemporaryHp(4,true);rescue.TakeDamage(8,damageType:1);C(rescued==4&&rescue.Hp==10&&rescue.TemporaryHp==0,"rescue target transfers damage absorbed by temporary HP");
        bond.Begin(1,0);bond.Begin(-1,1);C(!bond.Protects(1,1)&&bond.Protects(-1,1),"recasting replaces rather than stacks links");bond.Clear();C(!bond.Active(1),"reset clears link");
        Console.WriteLine("PASS: Warding Bond lifetime, range break, AC, typed transfer, two-stage resistance, temporary HP, mixed packets, immunity/vulnerability, simultaneous damage and auxiliary creatures");
    }
}
