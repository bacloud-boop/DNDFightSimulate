using System;
using UnityEngine;
class IceStormChecks
{
    static void C(bool v,string t){if(!v)throw new Exception(t);}
    static void Main()
    {
        var fields=new IceStormFields();Func<int,int,bool> land=(x,y)=>x!=4||y!=4;
        C(fields.Add(0,4,4,10,land,0)&&fields.Cell(8,4,0)&&!fields.Cell(8,5,0),"twenty-foot circle at cell centers");
        C(IceStormFields.Covers(4,4,4,4)&&!fields.Cell(4,4,0),"water remains a damage target but never becomes rough ground");
        C(IceStormFields.Covers(4,4,8,4,2)&&!IceStormFields.Covers(4,4,9,4,2),"a large target with any covered cell is hit once");
        C(!IceStormFields.Covers(int.MinValue,int.MinValue,int.MaxValue,int.MaxValue),"geometry cannot overflow into a false hit");
        fields.EndTurn(-1);C(fields.Cell(8,4,0),"other actor's turn cannot expire player terrain");
        fields.EndTurn(0);C(fields.Cell(8,4,1),"terrain remains through caster current turn and next turn");
        fields.EndTurn(0);C(!fields.Cell(8,4,1),"caster next turn end removes terrain");
        C(fields.Add(0,1,1,10,(x,y)=>true,2)&&fields.Add(-1,8,8,10,(x,y)=>true,2),"multiple independent nonconcentration fields");
        fields.EndTurn(0);fields.EndTurn(0);C(!fields.Cell(0,0,2)&&fields.Cell(9,9,2),"one caster expiry preserves the other field");
        C(!fields.Add(-1,10,0,10,land,2)&&!fields.Add(-1,0,0,10,null,2)&&fields.Cell(9,9,2),"invalid placement is atomic");
        C(!fields.Cell(9,9,4)&&!fields.CanAdd(0,long.MaxValue-1),"world fallback expires terrain even without a caster turn");
        fields.Clear();C(!fields.Cell(9,9,0),"battle reset clears regions and caster clocks");
        fields.Add(0,4,4,10,land,0);fields.EndTurn(0,11);fields.EndTurn(0,11);C(fields.Cell(8,4,1),"battle-outcome and ordinary end hooks cannot count the same turn twice");fields.EndTurn(0,13);C(!fields.Cell(8,4,1),"next distinct caster turn still expires region");
        for(int slot=4;slot<=9;slot++)C(IceStormRules.BludgeoningDice(slot)==slot-2,"only bludgeoning scales with slots");C(IceStormRules.BludgeoningDice(3)==0&&IceStormRules.BludgeoningDice(10)==0,"invalid slots rejected");
        var packet=IceStormRules.AfterSave(IceStormRules.Packet(11,9),true);C(packet[0]==5&&packet[4]==4,"successful Dexterity save halves each damage type separately");
        var state=new CombatState();state.DamageResistance=(p,t)=>t==4;var resolved=state.ResolveTypedPacket(false,packet);C(resolved[0]==5&&resolved[4]==2,"cold resistance applies after the save without reducing blunt");
        var map=new BattleMap();map.Lock();map.DifficultTerrain=(x,y,n)=>true;C(map.PlayerMovementCost(new Vector2Int(1,0))==10,"rough ground adds the established surcharge");map.MovementFreedom=id=>id==0;C(map.PlayerMovementCost(new Vector2Int(1,0))==5,"Freedom of Movement ignores the new terrain through the shared callback");
        var rescue=new RescueCarrier();C(rescue.Configure(10,2,30,150,0,10,100),"rescue configuration");rescue.ExtraResistance=t=>t==0;int events=0,legacyEvents=0;long[] received=null;
        rescue.TypedDamageApplied=p=>{events++;received=(long[])p.Clone();};rescue.DamageApplied=(a,t)=>legacyEvents++;
        C(rescue.TakeDamagePacket(IceStormRules.Packet(10,9))==14&&rescue.Hp==86&&events==1&&legacyEvents==0&&received[0]==5&&received[4]==9,"one mixed damage event with typed transfer, no duplicate single-type callbacks");
        rescue.ChooseTemporaryHp(5,true);rescue.TakeDamagePacket(IceStormRules.Packet(10,9));C(rescue.Hp==77&&rescue.TemporaryHp==0&&events==2,"temporary HP absorbs combined damage after one mitigation");
        var fallen=new RescueCarrier();fallen.Configure(10,2,30,150,0,10,30);fallen.TakeDamagePacket(IceStormRules.Packet(20,20));C(fallen.Hp==0&&!fallen.Dead,"combined nonfatal knockout");fallen.TakeDamagePacket(IceStormRules.Packet(1,1));C(fallen.DeathFailures==1,"mixed damage causes one failure rather than one for each type");
        var seq=new MonsterSpellSequence();C(seq.Configure(4,1,false,new[]{new MonsterSpellEntry("Ice Storm",4)})&&!seq.Configure(4,1,false,new[]{new MonsterSpellEntry("Ice Storm",3)}),"monster cycle enforces fourth-level minimum");
        C(MonsterSpellCatalog.IsArea(DirectSpell.Find("Ice Storm"))&&MonsterSpellCatalog.Radius(DirectSpell.Find("Ice Storm"))==20,"monster area planner receives correct radius");
        Console.WriteLine("PASS: Ice Storm geometry, water/land split, caster-turn expiry, independent fields, upcast packets, save/resistance order, Freedom, rescue atomic mixed damage and monster slots");
    }
}
