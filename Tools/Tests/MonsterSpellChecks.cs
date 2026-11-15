using System;
class MonsterSpellChecks
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        var sequence=new MonsterSpellSequence();Check(sequence.Next==null&&sequence.Take()==null,"empty fallback");
        var entries=new[]{new MonsterSpellEntry("Fire Bolt",0),new MonsterSpellEntry("Cure Wounds",3)};
        Check(sequence.Configure(4,5,true,entries),"valid mixed spells");
        Check(sequence.Take().Name=="Fire Bolt"&&sequence.Take().Slot==3&&sequence.Take().Name=="Fire Bolt","cycle");
        Check(!sequence.Configure(4,21,false,entries)&&sequence.CasterLevel==5&&sequence.Counterspell,"bad level atomic");
        Check(!sequence.Configure(4,5,false,new[]{new MonsterSpellEntry("Fire Bolt",1)})&&sequence.Entries.Count==2,"cantrip slot rejected");
        Check(!sequence.Configure(6,5,false,entries),"ability bound");
        Check(!sequence.Configure(4,5,false,new[]{new MonsterSpellEntry("Unknown",1)}),"unsupported spell");
        Check(!sequence.Configure(4,5,false,new[]{new MonsterSpellEntry("Scorching Ray",1)}),"spell minimum slot");
        sequence.Reset();Check(sequence.Next.Name=="Fire Bolt","battle reset retains config");
        Check(MonsterSpellSequence.AttackBonus(-3,3)==-7&&MonsterSpellSequence.SaveDc(-3,3)==1,"negative proficiency and floor");
        Check(MonsterSpellSequence.SaveDc(int.MaxValue,20)==(long)int.MaxValue+13,"DC overflow safe");
        Check(CounterspellRules.CanTrigger(60,true,true,true,true)&&!CounterspellRules.CanTrigger(61,true,true,true,true),"range");
        Check(!CounterspellRules.CanTrigger(5,false,true,true,true)&&!CounterspellRules.CanTrigger(5,true,false,true,true)&&!CounterspellRules.CanTrigger(5,true,true,true,false),"visibility components reaction");
        Check(!CounterspellRules.Interrupted(1,20,21)&&CounterspellRules.Interrupted(20,-10,11),"saves have no natural auto outcomes");
        Console.WriteLine("PASS: sequence, atomic configuration, slots, caster level, integer DC and counterspell triggers");
    }
}
