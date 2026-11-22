using System;
using System.Collections.Generic;
using UnityEngine;
class ConeOfColdChecks
{
    static void C(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        var spell=DirectSpell.Find("Cone of Cold");C(spell!=null&&spell.Level==5&&spell.SaveAbility==2&&spell.HalfSave&&spell.Sides==8&&spell.DamageType=="寒冷"&&!spell.Attack&&!spell.Bonus,"2024 fifth-level Magic action with Constitution save and cold damage");
        for(int slot=5;slot<=9;slot++)C(ConeOfColdRules.DiceCount(slot)==slot+3&&spell.DiceCount(1,slot)==slot+3&&spell.DiceCount(20,slot)==slot+3,"8d8 base, +1d8 per slot, independent of caster level");
        C(ConeOfColdRules.DiceCount(4)==0&&ConeOfColdRules.DiceCount(10)==0,"invalid slots rejected");
        BreathTemplate template;C(BreathTemplate.TryCreate(0,0,1,0,1,20,.5,false,out template,false,60),"edge-origin cone");
        C(template.Cell(12,0)&&!template.Cell(13,0)&&template.Cell(11,5)&&!template.Cell(11,6)&&!template.Cell(0,0),"sixty-foot cone uses grid centers and true width, with source excluded by default");
        C(template.Intersects(11,5,2)&&!template.Intersects(13,0,2),"multi-cell target counts if any occupied center is inside");
        C(BreathTemplate.TryCreate(0,0,1,0,1,20,.5,false,out template)&&template.LengthFeet==15&&!template.Cell(12,0),"ordinary Burning Hands and Dragonborn cones keep fifteen-foot reach");
        var enemies=new List<BreathBody>{new BreathBody(0,10,0,1),new BreathBody(1,11,2,1)};
        C(BreathWeaponRules.Choose(15,0,0,1,enemies,out template,false,60)&&template.LengthFeet==60&&!template.Intersects(0,0,1)&&template.Intersects(10,0,1)&&template.Intersects(11,2,1),"monster chooses broad enemy coverage without self-hit");
        var sequence=new MonsterSpellSequence();C(sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Cone of Cold",5)})&&MonsterSpellCatalog.IsCone(spell),"monster spell rotation uses the sixty-foot cone resolver");
        C(!sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Cone of Cold",4)})&&sequence.Next.Slot==5,"invalid monster slot leaves configured rotation intact");
        var state=new CombatState();state.DamageResistance=(p,t)=>t==4;C(ConeOfColdRules.AfterSave(19,true)==9&&state.AdjustTypedDamage(false,ConeOfColdRules.AfterSave(19,true),4)==4,"save halves before cold resistance; each step floors");
        state.DamageImmunity=(p,t)=>t==4;C(state.AdjustTypedDamage(false,64,4)==0,"cold immunity prevents damage");
        C(state.TryUseMagicAction()&&!state.TryUseMagicAction()&&state.BonusActionAvailable,"spell spends one Magic action and no bonus action");
        var corpse=new CorpseClock();C(!corpse.Freeze()&&!corpse.Frozen,"living or unconscious target cannot be frozen as a corpse");
        corpse.Observe(true,9,0);long generation=corpse.Generation;C(corpse.Freeze()&&corpse.Frozen,"actual death produces frozen remains");
        corpse.Age(14400);C(corpse.Frozen&&corpse.Age(14400)==14400,"time does not thaw or preserve a frozen corpse automatically");
        C(RevivalRules.Error("Revivify",corpse,14400)!=""&&RevivalRules.Error("Raise Dead",corpse,14400)=="","freezing does not extend revival deadlines or prevent valid revival");
        corpse.Thaw();C(!corpse.Frozen&&corpse.Dead&&corpse.Generation==generation&&corpse.Age(14400)==14400,"manual thaw changes no death clock or identity");
        corpse.Freeze();corpse.Observe(false,9,14400);C(!corpse.Frozen&&!corpse.Dead,"revived body no longer carries corpse freezing");
        corpse.Observe(true,9,14400);C(!corpse.Frozen&&corpse.Generation!=generation,"another cause of death does not inherit old freezing");
        var map=new BattleMap();map.Lock();var weight=new BodyWeight();long original=weight.TotalOunces;
        var old=map.Remains.Add(0,2,2,1,"frozen old body",weight,0,false);old.Frozen=true;C(old.Weight.TotalOunces==original&&map.RemainsSpaces(old)==1,"frozen archived corpse keeps weight and map footprint");
        var carrying=new RescueCarrier();C(carrying.BeginTurn(1)&&carrying.PickUp(old.Id,true,old.Weight,false),"freezing does not change normal corpse pickup eligibility or action cost");
        old.Frozen=false;C(carrying.Load.Loaded&&old.Weight.TotalOunces==original,"thawing a carried corpse neither drops it nor changes its weight");
        corpse.Clear();C(!corpse.Frozen,"battle reset clears freezing");
        Console.WriteLine("PASS: Cone of Cold 2024 metadata, slot scaling, geometry, monster cone planning, action cost, cold mitigation and frozen corpse lifetime/carrying/revival invariants");
    }
}
