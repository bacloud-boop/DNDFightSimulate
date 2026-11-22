using System;
using System.Collections.Generic;
class ConjuredWeaponChecks
{
    static void C(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        var barrage=DirectSpell.Find("Conjure Barrage");var volley=DirectSpell.Find("Conjure Volley");
        C(barrage.Level==3&&barrage.Sides==8&&barrage.SaveAbility==1&&barrage.HalfSave&&!barrage.Attack&&!barrage.Bonus&&barrage.DamageType=="力场","barrage uses a Magic action and Dexterity save, not a weapon attack");
        for(int slot=3;slot<=9;slot++)C(barrage.DiceCount(1,slot)==slot+2&&ConjuredWeaponRules.DiceCount(false,slot)==slot+2,"barrage scales only with slot");
        for(int slot=5;slot<=9;slot++)C(volley.DiceCount(20,slot)==8&&ConjuredWeaponRules.DiceCount(true,slot)==8,"volley never upcasts damage");
        C(ConjuredWeaponRules.DiceCount(false,2)==0&&ConjuredWeaponRules.DiceCount(true,4)==0&&ConjuredWeaponRules.DiceCount(true,10)==0,"invalid slots rejected");
        BreathTemplate cone;C(BreathTemplate.TryCreate(0,0,1,0,1,20,.5,false,out cone,false,60),"sixty-foot cone can be aimed from a creature edge");
        C(cone.Cell(12,0)&&!cone.Cell(13,0)&&!cone.Cell(0,0)&&cone.Cell(11,5)&&!cone.Cell(11,6),"cone cell centers respect length, width and outward direction");
        C(ConjuredWeaponRules.CylinderCell(8,8,16,8)&&!ConjuredWeaponRules.CylinderCell(8,8,16,9),"exact forty-foot radius never rounds an outside cell into the cylinder");
        C(ConjuredWeaponRules.CylinderCell(8,8,0,7,2)&&!ConjuredWeaponRules.CylinderCell(8,8,17,8,2),"large creature is covered if any occupied cell qualifies");
        C(!ConjuredWeaponRules.CylinderCell(int.MinValue,0,int.MaxValue,0),"radius arithmetic cannot overflow");
        var chosen=new HashSet<int>{0,-1,1,2};var targets=ConjuredWeaponRules.Select(new[]{0,-1,1,1,3},chosen,id=>id!=0);
        C(targets.Count==2&&targets.Contains(-1)&&targets.Contains(1),"selection intersects coverage and visibility, excludes unchosen targets and deduplicates multi-cell hits");
        chosen.Remove(1);C(ConjuredWeaponRules.Select(new[]{-1,1},chosen,id=>true).Count==1,"player can exclude an allied creature without changing the area");
        var enemies=new List<BreathBody>{new BreathBody(0,10,0,1),new BreathBody(1,11,2,1)};
        C(!BreathWeaponRules.Choose(15,0,0,1,enemies,out cone,false),"existing fifteen-foot cones retain their original reach");
        C(BreathWeaponRules.Choose(15,0,0,1,enemies,out cone,false,60)&&!cone.Line&&cone.LengthFeet==60&&!cone.Intersects(0,0,1)&&cone.Intersects(10,0,1)&&cone.Intersects(11,2,1),"monster long-cone planner covers both distant enemies and avoids itself");
        var sequence=new MonsterSpellSequence();C(sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Conjure Barrage",3),new MonsterSpellEntry("Conjure Volley",5)}),"both spells enter the monster rotation");
        C(!sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Conjure Barrage",2)})&&!sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Conjure Volley",4)})&&sequence.Entries.Count==2,"invalid lower slots leave the configured rotation intact");
        C(MonsterSpellCatalog.IsCone(barrage)&&MonsterSpellCatalog.IsArea(volley)&&MonsterSpellCatalog.Radius(volley)==40,"monster geometry catalog matches both spell shapes");
        var state=new CombatState();state.AddSurgeAction();C(state.TryUseMagicAction()&&!state.TryUseMagicAction()&&state.BonusActionAvailable,"one normal Magic action, no restricted Surge casting, bonus action unchanged");
        state.DamageResistance=(p,t)=>t==6;C(state.AdjustTypedDamage(false,19/2,6)==4,"save halves first, then Force resistance rounds down again");
        state.DamageImmunity=(p,t)=>t==6;C(state.AdjustTypedDamage(false,19,6)==0,"Force immunity prevents all damage");
        Console.WriteLine("PASS: chosen visible targets, multi-cell bounds, 60-foot cone AI, legacy cone reach, exact 40-foot cylinder, slot scaling, action budgets, rotation and save/resistance order");
    }
}
