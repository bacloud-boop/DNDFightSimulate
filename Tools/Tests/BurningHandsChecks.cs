using System;
using System.Collections.Generic;
class BurningHandsChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var spell=DirectSpell.Find("Burning Hands");C(spell!=null&&spell.Level==1&&spell.BaseDice==3&&spell.UpcastDice==1&&spell.Sides==6&&spell.DamageType=="火焰"&&spell.SaveAbility==1&&spell.HalfSave&&!spell.Attack,"2024 Burning Hands is instantaneous save damage with no attack modifier");
        C(spell.DiceCount(1,1)==3&&spell.DiceCount(20,1)==3&&spell.DiceCount(1,9)==11,"only slot level increases Burning Hands damage");
        var turns=new CombatState();turns.AddSurgeAction();C(turns.TryUseMagicAction()&&!turns.TryUseMagicAction()&&turns.BonusActionAvailable,"one ordinary Magic action is spent; restricted Surge cannot cast and bonus action is untouched");
        var fighter=new FighterFeatures();fighter.RecordMiss(20);C(fighter.AttacksRemaining==0&&fighter.NextAttackAdvantage,"save-based spells neither begin Extra Attack nor consume Studied Attack advantage");
        var sequence=new MonsterSpellSequence();C(sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Burning Hands",4),new MonsterSpellEntry("Fire Bolt",0)}),"monster may configure upcast Burning Hands in its existing sequence");
        C(sequence.Take().Name=="Burning Hands"&&sequence.Take().Name=="Fire Bolt"&&sequence.Next.Name=="Burning Hands","casting or fallback advances the normal rotation");
        C(!sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Burning Hands",0)})&&sequence.Entries.Count==2,"invalid cantrip slot cannot erase monster configuration");
        BreathTemplate plan;var enemies=new List<BreathBody>{new BreathBody(0,6,0,1)};
        C(BreathWeaponRules.Choose(10,0,0,1,enemies,out plan)&&plan.Line,"long line could reach distant enemy for a racial breath");
        C(!BreathWeaponRules.Choose(10,0,0,1,enemies,out plan,false),"Burning Hands never substitutes the longer line when its cone cannot reach");
        enemies=new List<BreathBody>{new BreathBody(0,2,0,1),new BreathBody(1,3,1,1)};
        C(BreathWeaponRules.Choose(10,0,0,1,enemies,out plan,false)&&!plan.Line&&!plan.Intersects(0,0,1)&&plan.Intersects(2,0,1)&&plan.Intersects(3,1,1),"monster chooses a cone covering the most enemies and excludes itself");
        var damage=new CombatState();damage.PlayerUnderwater=true;damage.PlayerEnergyResistance=5;C(damage.AdjustTypedDamage(true,15/2,5)==3,"successful save halves first; overlapping underwater and spell fire resistance halve only once");
        damage.DamageImmunity=(player,type)=>type==5;C(damage.AdjustTypedDamage(true,15,5)==0,"fire immunity prevents all damage after a failed save");
        C(MonsterSpellSequence.SaveDc(int.MaxValue,20)==2147483660L,"monster cone DC retains unbounded manually entered proficiency safely");
        Console.WriteLine("PASS: Burning Hands slot scaling, magic-action budget, untouched attack resources, monster rotation and cone-only targeting, save/resistance order and extreme DC");
    }
}
