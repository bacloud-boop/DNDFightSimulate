using System;
class FlameStrikeChecks
{
    static void C(bool v,string name){if(!v)throw new Exception(name);}
    static void Main()
    {
        var spell=DirectSpell.Find("Flame Strike");C(spell.Level==5&&spell.Range==60&&spell.BurstRadius==10&&spell.SaveAbility==1&&!spell.Attack&&!spell.Bonus&&spell.HalfSave,"fifth-level Dexterity save cylinder, no attack roll");
        for(int slot=5;slot<=9;slot++)C(FlameStrikeRules.DicePerType(slot)==slot&&spell.DiceCount(1,slot)==slot,"each damage type separately gains a d6 per slot");
        C(FlameStrikeRules.DicePerType(4)==0&&FlameStrikeRules.DicePerType(10)==0,"invalid slots rejected");
        C(FlameStrikeRules.Covers(4,4,6,4)&&!FlameStrikeRules.Covers(4,4,6,5)&&!FlameStrikeRules.Covers(4,4,7,4),"exact ten-foot grid-center circle");
        C(FlameStrikeRules.Covers(4,4,1,3,2)&&!FlameStrikeRules.Covers(int.MaxValue,0,int.MinValue,0),"any covered cell includes a large creature, without coordinate overflow");
        var packet=IceStormRules.AfterSave(FlameStrikeRules.Packet(11,13),true);C(packet[5]==5&&packet[11]==6,"successful save rounds each damage type independently");
        var state=new CombatState();state.PlayerUnderwater=true;var resolved=state.ResolveTypedPacket(true,packet);C(resolved[5]==2&&resolved[11]==6,"submerged fire resistance does not reduce radiant damage");
        state.PlayerEnergyResistance=5;resolved=state.ResolveTypedPacket(true,packet);C(resolved[5]==2,"overlapping fire resistances do not stack");
        state.DamageImmunity=(p,t)=>t==5;state.DamageVulnerability=(p,t)=>t==11;resolved=state.ResolveTypedPacket(true,packet);C(resolved[5]==0&&resolved[11]==12,"fire immunity and radiant vulnerability resolve independently");
        state.ChooseTemporaryHp(true,4,true);int before=state.PlayerHp,events=0;long[] transferred=null;state.DamageApplied=(p,types)=>{events++;transferred=types;};state.ApplyPlayerDamage(12,resolvedTypes:resolved,sourcePlayer:true);C(events==1&&state.PlayerHp==before-8&&state.PlayerTemporaryHp==0&&transferred[11]==12,"one mixed hit consumes temporary HP once and emits one typed transfer");
        var rescue=new RescueCarrier();rescue.Configure(10,2,30,150,0,10,100);rescue.ExtraResistance=t=>t==5;int fire=0,hits=0;rescue.FireExposure=()=>fire++;rescue.TypedDamageApplied=p=>hits++;
        C(rescue.TakeDamagePacket(packet)==8&&rescue.Hp==92&&fire==1&&hits==1,"rescue fire exposure and mixed hit both trigger once");
        var sequence=new MonsterSpellSequence();C(sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Flame Strike",5)})&&!sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Flame Strike",4)})&&sequence.Next.Name=="Flame Strike","monster configuration enforces the minimum slot atomically");
        C(MonsterSpellCatalog.IsArea(spell)&&MonsterSpellCatalog.Radius(spell)==10,"monster planner receives the ten-foot radius");
        Console.WriteLine("PASS: Flame Strike two-type upcasting, exact cylinder bounds, save rounding, underwater resistance, immunity/vulnerability, one damage event, rescue ignition and monster slots");
    }
}
