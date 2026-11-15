using System;
class CounterspellResourceChecks
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        var combat=new CombatState();
        Check(combat.TryUseMagicAction()&&!combat.MagicActionAvailable,"original cast pays action first");
        Check(combat.TryUseReaction(false)&&combat.TryUseReaction(true),"both sides may counter once");
        Check(!combat.TryUseReaction(false)&&!combat.TryUseReaction(true),"no infinite reaction chain");
        Check(!combat.MagicActionAvailable,"interruption never refunds original action");
        combat.StartMonsterTurn();Check(combat.MonsterReactionAvailable&&!combat.PlayerReactionAvailable,"monster turn restores only its own reaction");
        combat.EndRound();Check(combat.PlayerReactionAvailable&&combat.MagicActionAvailable,"player turn restores its resources");
        combat.ResetBattle();Check(combat.TryUseBonusAction()&&combat.ActionAvailable,"bonus spell preserves action");
        Check(combat.TryUseReaction(false)&&!combat.TryUseBonusAction(),"countered bonus spell still spends bonus action");
        var sequence=new MonsterSpellSequence();var rows=new[]{new MonsterSpellEntry("Inflict Wounds",1),new MonsterSpellEntry("Fire Bolt",0)};
        Check(sequence.Configure(4,1,false,rows),"sequence setup");sequence.Take();
        Check(sequence.Next.Name=="Fire Bolt","failed first attempt advances before fallback");
        Console.WriteLine("PASS: paid casting resources, bounded reaction chain, actor-specific refresh, bonus action and fallback cursor");
    }
}
