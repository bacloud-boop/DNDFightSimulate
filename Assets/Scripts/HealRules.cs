using System;
public static class HealRules
{
    public static int Amount(int slot){if(slot<6||slot>9)throw new ArgumentOutOfRangeException(nameof(slot));return 70+10*(slot-6);}
    // Condition removal is an independent effect, even if healing is blocked or capped.
    public static int Apply(CombatState combat,SensoryAfflictions sensory,bool player,int slot,long now)
    {
        int amount=Amount(slot);if(player?combat.PlayerDead:combat.Hp==0)return 0;
        int healed=player?combat.HealPlayer(amount):combat.HealMonster(amount);
        foreach(int condition in new[]{0,1,3})RestorationRules.EndCondition(combat,sensory,player,condition,now);
        return healed;
    }
}
