using System;
class RestorationChecks
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        var combat=new CombatState();var sensory=new SensoryAfflictions();
        sensory.Add(false,true,18,0);sensory.Add(false,true,12,1);var deaf=sensory.Add(false,false,16,2);sensory.Add(true,true,20,0);
        combat.ApplyPoison(false);combat.ApplyPoison(true);combat.MonsterParalyzed=combat.PlayerParalyzed=true;
        Check(RestorationRules.EndCondition(combat,sensory,false,0,2)==2,"removes active and suppressed matching sources");
        Check(sensory.Current(false,2).Id==deaf.Id&&sensory.Deafened(false,2)&&sensory.Blinded(true,2),"other mode resumes and other actor preserved");
        Check(combat.MonsterPoisoned&&combat.MonsterParalyzed,"other conditions preserved");
        Check(RestorationRules.EndCondition(combat,sensory,false,1,2)==1&&sensory.Current(false,2)==null,"deafness removal");
        Check(RestorationRules.EndCondition(combat,sensory,false,2,2)==1&&!combat.MonsterParalyzed&&combat.PlayerParalyzed,"paralysis actor isolation");
        Check(RestorationRules.EndCondition(combat,sensory,false,3,2)==1&&!combat.MonsterPoisoned&&combat.PlayerPoisoned,"poison actor isolation");
        Check(RestorationRules.EndCondition(combat,sensory,false,0,2)==0&&RestorationRules.EndCondition(combat,sensory,true,4,2)==0&&sensory.Blinded(true,2),"empty and invalid selection");
        Check(!combat.TryUseBonusAction()&&combat.BonusActionAvailable,"paralyzed caster cannot spend bonus action to self cure");
        combat.PlayerParalyzed=false;Check(combat.TryUseBonusAction()&&combat.ActionAvailable,"restoration uses bonus action only");
        var silence=new SilenceEffect();silence.Begin(0,0,0);RestorationRules.EndCondition(combat,sensory,false,1,2);
        Check(silence.WholeInside(0,0,1,2),"continuous silence remains after restoration");
        Console.WriteLine("PASS: all matching sources, suppressed effects, other statuses and actors, paralysis, poison and bonus action");
    }
}
