using System;
using System.Collections.Generic;
class SpeciesSpellChecks
{
    static void C(bool ok,string m){if(!ok)throw new Exception(m);}
    static SpeciesProfile P(Species2024 kind,int lineage=0)=>new SpeciesProfile{Kind=kind,Lineage=lineage};
    static void Main()
    {
        var high=P(Species2024.Elf,1);high.HighElfCantrip="Acid Splash";high.SpellAbility=3;
        C(SpeciesSpellRules.ValidHighElfCantrip("Acid Splash")&&!SpeciesSpellRules.ValidHighElfCantrip("Fireball"),"high elf cannot gain a leveled spell as its chosen cantrip");
        C(SpeciesSpellRules.Ability(high)==3&&SpeciesSpellRules.Find(high,1,"Acid Splash")!=null,"high elf uses its chosen Wizard cantrip and casting ability independently of classes");
        C(SpeciesSpellRules.Find(high,2,"Detect Magic")==null&&SpeciesSpellRules.Find(high,3,"Detect Magic")!=null&&SpeciesSpellRules.Find(high,4,"Misty Step")==null&&SpeciesSpellRules.Find(high,5,"Misty Step")!=null,"lineage unlocks use total character level thresholds");
        high.HighElfCantrip="Fire Bolt";C(SpeciesSpellRules.Find(high,20,"Acid Splash")==null&&SpeciesSpellRules.Find(high,20,"Fire Bolt")!=null,"cantrip replacement removes the former racial grant");
        C(SpeciesSpellRules.Ability(P(Species2024.Aasimar))==5,"Aasimar Light always uses Charisma");
        C(SpeciesSpellRules.Find(P(Species2024.Elf),5,"Darkness")!=null&&SpeciesSpellRules.Find(P(Species2024.Elf,2),5,"Longstrider")!=null,"drow and wood lineage spells");
        C(SpeciesSpellRules.Find(P(Species2024.Tiefling),5,"Hold Person")!=null&&SpeciesSpellRules.Find(P(Species2024.Tiefling,1),5,"Ray of Enfeeblement")!=null&&SpeciesSpellRules.Find(P(Species2024.Tiefling,2),3,"Hellish Rebuke")!=null,"all three fiendish lineages");
        var uses=new SpeciesSpellUses();var misty=SpeciesSpellRules.Find(high,5,"Misty Step");
        C(uses.CanCast(misty,2,2,true)&&!uses.CanCast(misty,2,3,true)&&uses.CanCast(misty,2,3,false),"free casting is base level; slot casting may upcast under the project's unlimited-slot rule");
        C(uses.Consume(misty,2)&&!uses.Consume(misty,2)&&!uses.CanCast(misty,2,2,true)&&uses.CanCast(misty,2,2,false),"expended free cast does not remove the learned spell or slot route");
        var detect=SpeciesSpellRules.Find(high,3,"Detect Magic");C(uses.Remaining(detect,2)==1,"different spells have independent free uses");
        var animals=SpeciesSpellRules.Find(P(Species2024.Gnome),1,"Speak with Animals");
        C(uses.Remaining(animals,-1)==0&&uses.Remaining(animals,4)==4,"forest gnome free uses follow proficiency with a zero floor");
        uses.LongRest();C(uses.Remaining(misty,2)==1,"long rest refreshes lineage free casts");
        var cantrip=SpeciesSpellRules.Find(high,1,"Fire Bolt");C(uses.CanCast(cantrip,2,0,true)&&!uses.Consume(cantrip,2)&&uses.Remaining(cantrip,2)==0,"cantrips need no free-use pool");
        Console.WriteLine("PASS: racial spell sources, chosen ability, lineage levels, cantrip replacement, independent free uses and slot routes");
    }
}
