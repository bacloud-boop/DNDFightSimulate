using System;

public sealed partial class RescueCarrier
{
    readonly int[] abilityScores={10,10,10,10,10,10};
    public int Ability(int index)=>abilityScores[index];
    public SpeciesProfile Species {get;private set;}=new SpeciesProfile();
    public int ProficiencyBonus {get;private set;}
    public readonly SpeciesAbilityState SpeciesAbilities=new SpeciesAbilityState();
    public readonly SpeciesSpellUses SpeciesSpells=new SpeciesSpellUses();
    bool bonusAvailable=true;public bool BonusActionAvailable {get=>bonusAvailable&&!ConditionIncapacitated&&!(ActionBlock!=null&&ActionBlock());private set=>bonusAvailable=value;}
    bool reactionAvailable=true;public bool ReactionAvailable {get=>reactionAvailable&&!ConditionIncapacitated;private set=>reactionAvailable=value;}
    public int TemporaryHp {get;private set;}
    public void ChooseTemporaryHp(int amount,bool takeNew){if(amount<0)throw new ArgumentOutOfRangeException(nameof(amount));if(!Dead&&takeNew)TemporaryHp=amount;}
    public long LastLongRestStart {get;private set;}=-1;
    public bool ConfigureProfile(int[] scores,SpeciesProfile species,int proficiency,int size,int speed,int bodyPounds,int equipmentPounds,int baseMaxHp)
    {
        if(scores==null||scores.Length!=6||species==null||!species.Valid()||baseMaxHp<1||TurnActive||Load.Loaded||size<0||size>5||speed<0||bodyPounds<1||equipmentPounds<0)return false;
        foreach(int score in scores)if(score<3||score>20)return false;
        if(species.Kind!=Species2024.None){size=species.BodySize;speed=species.Speed;}
        if(!CarryingRules.CanCarry(scores[0],species.CapacitySize(size),equipmentPounds,0))return false;
        Reset();Array.Copy(scores,abilityScores,6);Species=species.Copy();ProficiencyBonus=proficiency;BaseMaxHp=baseMaxHp;Hp=MaxHp;Size=size;Speed=speed;
        Weight.SetStoredGear(0);Weight.SetArmorWeight(0);Weight.SetWeaponWeight(0);Weight.TryApply(bodyPounds.ToString(),equipmentPounds.ToString());LastLongRestStart=-1;return true;
    }
    public int SkillTier(int id){foreach(int skill in Species.Proficiencies())if(skill==id)return 1;return 0;}
    public bool TryUseBonusAction(){if(!Conscious||!TurnActive||!BonusActionAvailable)return false;BonusActionAvailable=false;return true;}
    public bool TryUseReaction(){if(!Conscious||!ReactionAvailable)return false;ReactionAvailable=false;return true;}
    public bool UseSpeciesBonus(long now)
    {
        if((Species.Kind!=Species2024.Orc&&Species.Kind!=Species2024.Dwarf)||!Conscious||!TurnActive||!BonusActionAvailable||!SpeciesAbilities.CanUse(Species.Kind,ProficiencyBonus,now))return false;
        if(!TryUseBonusAction()||!SpeciesAbilities.Use(Species.Kind,ProficiencyBonus,now))return false;
        if(Species.Kind==Species2024.Orc){Dashes++;TemporaryHp=Math.Max(TemporaryHp,Math.Max(0,ProficiencyBonus));}return true;
    }
    public bool UseHealingHands(long now){if(Species.Kind!=Species2024.Aasimar||!SpeciesAbilities.CanUse(Species.Kind,ProficiencyBonus,now)||!UseAction())return false;return SpeciesAbilities.Use(Species.Kind,ProficiencyBonus,now);}
    public bool CanLongRest(long now)=>Conscious&&!Load.Loaded&&now>=0&&now<=long.MaxValue-Species.LongRestRounds&&(LastLongRestStart<0||now-LastLongRestStart>=14400);
    public bool FinishLongRest(long start,long finish){if(!CanLongRest(start)||finish-start<Species.LongRestRounds)return false;LastLongRestStart=start;CompleteLongRest();return true;}
    public void ShortRest(){if(Conscious)SpeciesAbilities.ShortRest();}
    public bool Reincarnate(SpeciesProfile species)
    {
        if(!Dead||Load.Loaded||species==null||species.Kind==Species2024.None||!species.Valid()||!Weight.Known)return false;
        Species=species.Copy();Size=species.BodySize;Speed=species.Speed;
        int mass=Weight.BodyPounds.Value;Weight.SetStoredGear(0);Weight.SetArmorWeight(0);Weight.SetWeaponWeight(0);Weight.TryApply(mass.ToString(),"0");TemporaryHp=0;
        // Rest-limited expenditure belongs to the character and is not refreshed by revival.
        EndTurn();return Revive(MaxHp,false);
    }
}
