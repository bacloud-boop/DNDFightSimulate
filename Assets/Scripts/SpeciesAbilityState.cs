using System;

// Expenditure belongs to the creature, separately from its editable species data.
public sealed class SpeciesAbilityState
{
    public long RushSpent {get;private set;}
    public long StoneSpent {get;private set;}
    public long StoneUntil {get;private set;}
    public bool HealingHandsSpent {get;private set;}
    public long BreathSpent {get;private set;}
    public long Remaining(Species2024 species,int proficiency)
        =>species==Species2024.Aasimar?(HealingHandsSpent?0:1):species!=Species2024.Orc&&species!=Species2024.Dwarf&&species!=Species2024.Dragonborn?0:Math.Max(0,Math.Max(0L,proficiency)-(species==Species2024.Orc?RushSpent:species==Species2024.Dwarf?StoneSpent:BreathSpent));
    public bool StoneActive(long now)=>now>=0&&now<StoneUntil;
    public bool CanUse(Species2024 species,int proficiency,long now)
        =>(species==Species2024.Aasimar||species==Species2024.Orc||species==Species2024.Dwarf||species==Species2024.Dragonborn)&&now>=0&&Remaining(species,proficiency)>0&&(species!=Species2024.Dwarf||now<=long.MaxValue-100);
    public bool CanAutomaticallyUse(Species2024 species,int proficiency,long now)
        =>CanUse(species,proficiency,now)&&(species!=Species2024.Dwarf||!StoneActive(now));
    public bool Use(Species2024 species,int proficiency,long now)
    {
        if(!CanUse(species,proficiency,now))return false;
        if(species==Species2024.Aasimar)HealingHandsSpent=true;
        else if(species==Species2024.Dragonborn)BreathSpent++;
        else if(species==Species2024.Orc)RushSpent++;
        else {StoneSpent++;StoneUntil=now+100;}
        return true;
    }
    public void ShortRest(){RushSpent=0;}
    public void LongRest(){RushSpent=StoneSpent=BreathSpent=0;HealingHandsSpent=false;}
    public void Reset(){LongRest();StoneUntil=0;}
}
