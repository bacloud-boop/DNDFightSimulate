using System;
using System.Collections.Generic;

public sealed class SpeciesSpellGrant
{
    public readonly string Name;
    public readonly int SpellLevel,RequiredLevel;
    public readonly bool ProficiencyUses;
    public SpeciesSpellGrant(string name,int spellLevel=0,int requiredLevel=1,bool proficiencyUses=false){Name=name;SpellLevel=spellLevel;RequiredLevel=requiredLevel;ProficiencyUses=proficiencyUses;}
    public long FreeMaximum(int proficiency)=>SpellLevel==0?0:ProficiencyUses?Math.Max(0L,proficiency):1;
}
public static class SpeciesSpellRules
{
    public const int Caster=-2;
    public static readonly string[] HighElfCantrips={"Acid Splash","Blade Ward","Chill Touch","Dancing Lights","Elementalism","Fire Bolt","Friends","Light","Mage Hand","Mending","Message","Mind Sliver","Minor Illusion","Poison Spray","Prestidigitation","Ray of Frost","Shocking Grasp","Thunderclap","Toll the Dead","True Strike"};
    public static bool ValidHighElfCantrip(string name)=>Array.IndexOf(HighElfCantrips,name)>=0;
    public static int Ability(SpeciesProfile p)=>p.Kind==Species2024.Aasimar?5:p.SpellAbility;
    public static IEnumerable<SpeciesSpellGrant> Grants(SpeciesProfile p,int characterLevel)
    {
        var list=new List<SpeciesSpellGrant>();
        if(p.Kind==Species2024.Aasimar)list.Add(new SpeciesSpellGrant("Light"));
        if(p.Kind==Species2024.Elf){
            if(p.Lineage==0){list.Add(new SpeciesSpellGrant("Dancing Lights"));list.Add(new SpeciesSpellGrant("Faerie Fire",1,3));list.Add(new SpeciesSpellGrant("Darkness",2,5));}
            if(p.Lineage==1){list.Add(new SpeciesSpellGrant(p.HighElfCantrip));list.Add(new SpeciesSpellGrant("Detect Magic",1,3));list.Add(new SpeciesSpellGrant("Misty Step",2,5));}
            if(p.Lineage==2){list.Add(new SpeciesSpellGrant("Druidcraft"));list.Add(new SpeciesSpellGrant("Longstrider",1,3));list.Add(new SpeciesSpellGrant("Pass without Trace",2,5));}
        }
        if(p.Kind==Species2024.Gnome){
            if(p.Lineage==0){list.Add(new SpeciesSpellGrant("Minor Illusion"));list.Add(new SpeciesSpellGrant("Speak with Animals",1,1,true));}
            else {list.Add(new SpeciesSpellGrant("Mending"));list.Add(new SpeciesSpellGrant("Prestidigitation"));}
        }
        if(p.Kind==Species2024.Tiefling){
            list.Add(new SpeciesSpellGrant("Thaumaturgy"));
            if(p.Lineage==0){list.Add(new SpeciesSpellGrant("Poison Spray"));list.Add(new SpeciesSpellGrant("Ray of Sickness",1,3));list.Add(new SpeciesSpellGrant("Hold Person",2,5));}
            if(p.Lineage==1){list.Add(new SpeciesSpellGrant("Chill Touch"));list.Add(new SpeciesSpellGrant("False Life",1,3));list.Add(new SpeciesSpellGrant("Ray of Enfeeblement",2,5));}
            if(p.Lineage==2){list.Add(new SpeciesSpellGrant("Fire Bolt"));list.Add(new SpeciesSpellGrant("Hellish Rebuke",1,3));list.Add(new SpeciesSpellGrant("Darkness",2,5));}
        }
        foreach(var grant in list)if(characterLevel>=grant.RequiredLevel)yield return grant;
    }
    public static SpeciesSpellGrant Find(SpeciesProfile p,int characterLevel,string name)
    {foreach(var grant in Grants(p,characterLevel))if(grant.Name==name)return grant;return null;}
}
public sealed class SpeciesSpellUses
{
    readonly Dictionary<string,long> spent=new Dictionary<string,long>();
    public long Remaining(SpeciesSpellGrant grant,int proficiency){if(grant==null)return 0;long n;spent.TryGetValue(grant.Name,out n);return Math.Max(0,grant.FreeMaximum(proficiency)-n);}
    public bool CanCast(SpeciesSpellGrant grant,int proficiency,int slot,bool freeCast)
        =>grant!=null&&(grant.SpellLevel==0?slot==0:slot>=grant.SpellLevel&&slot<=9&&(!freeCast||slot==grant.SpellLevel&&Remaining(grant,proficiency)>0));
    public bool Consume(SpeciesSpellGrant grant,int proficiency){if(Remaining(grant,proficiency)<1)return false;long n;spent.TryGetValue(grant.Name,out n);spent[grant.Name]=n+1;return true;}
    public void LongRest(){spent.Clear();}
}
