using System;
class RescueSpeciesChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var r=new RescueCarrier();var scores=new[]{10,12,14,16,18,20};var species=new SpeciesProfile{Kind=Species2024.Orc,Level=5};
        C(r.ConfigureProfile(scores,species,2,2,30,150,10,10),"valid complete rescue profile");scores[2]=3;species.Level=20;
        C(r.Ability(0)==10&&r.Ability(1)==12&&r.Ability(2)==14&&r.Ability(3)==16&&r.Ability(4)==18&&r.Ability(5)==20&&r.Species.Level==5,"configuration copies all six abilities and species independently");
        C(r.BeginTurn(0)&&r.UseSpeciesBonus(0)&&r.ActionsRemaining==1&&!r.BonusActionAvailable&&r.MovementRemaining==60&&r.TemporaryHp==2,"Orc Rush spends only bonus action, adds movement and grants nonstacking temporary HP");
        C(!r.UseSpeciesBonus(0)&&r.SpeciesAbilities.RushSpent==1,"same-turn second activation cannot consume another limited use");
        C(r.TryUseReaction()&&!r.TryUseReaction(),"one rescue reaction available between turns");r.EndTurn();r.BeginTurn(1);C(r.ReactionAvailable&&r.BonusActionAvailable,"turn start restores both independent resources");
        r.TakeDamage(1000);r.ObserveLife(1);var dwarf=new SpeciesProfile{Kind=Species2024.Dwarf,Level=5};
        C(r.Reincarnate(dwarf)&&r.Hp==15&&r.MaxHp==15&&r.Weight.BodyPounds==150&&r.Weight.GearOunces==0,"new body retains configured base HP and weight, adds new dwarf HP and leaves all equipment behind");
        C(r.Ability(2)==14&&r.Ability(5)==20&&r.ProficiencyBonus==2&&r.SpeciesAbilities.RushSpent==1&&!r.TurnActive,"reincarnation retains character abilities, proficiency, spent rest resources and paused turn state");
        var strong=new RescueCarrier();C(strong.ConfigureProfile(new[]{3,10,10,10,10,10},new SpeciesProfile{Kind=Species2024.Goliath},0,2,30,150,60,10),"powerful build applies to rescue equipment capacity without changing occupied body size");
        C(strong.Size==2&&strong.Speed==35&&!strong.ConfigureProfile(new[]{3,10,10,10,10,10},new SpeciesProfile{Kind=Species2024.Human},0,2,30,150,60,10)&&strong.Species.Kind==Species2024.Goliath,"overload rejects configuration atomically");
        var elf=new RescueCarrier();C(elf.ConfigureProfile(new[]{10,10,10,10,10,10},new SpeciesProfile{Kind=Species2024.Elf,ChosenSkill=12},-2,2,30,150,0,10),"negative manual proficiency is allowed");
        C(elf.SkillTier(12)==1&&elf.SpeciesAbilities.Remaining(Species2024.Orc,-2)==0,"racial proficiencies and zero-floor use counts");
        C(elf.FinishLongRest(0,2400)&&!elf.CanLongRest(2400)&&elf.CanLongRest(14400),"elf four-hour long rest respects daily benefit interval");
        var angel=new RescueCarrier();angel.ConfigureProfile(new[]{10,10,10,10,10,10},new SpeciesProfile{Kind=Species2024.Aasimar},0,2,30,150,0,10);C(angel.TakeDamage(8,damageType:11)==4&&angel.Hp==6,"rescue racial resistance applies before HP loss");
        angel.BeginTurn(0);C(!angel.UseSpeciesBonus(0)&&angel.BonusActionAvailable&&angel.UseHealingHands(0)&&angel.ActionsRemaining==0&&angel.BonusActionAvailable,"Healing Hands is a Magic action rather than a bonus action for rescuers");
        angel.ShortRest();C(angel.SpeciesAbilities.HealingHandsSpent,"rescue short rest leaves long-rest-only Healing Hands spent");angel.CompleteLongRest();C(!angel.SpeciesAbilities.HealingHandsSpent&&angel.Hp==angel.MaxHp,"rescue long rest restores Healing Hands and HP");
        Console.WriteLine("PASS: rescue six abilities, independent racial level/proficiency, actions, bonus actions, reactions, traits, HP, capacity, rests and reincarnation");
    }
}
