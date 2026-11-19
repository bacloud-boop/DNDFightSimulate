using System;
using System.Collections.Generic;
public static class MassHealRules
{
    public static bool ValidAllocation(IEnumerable<string> values,out int total)
    {
        total=0;if(values==null)return false;
        foreach(string value in values){int amount;if(!int.TryParse(value,out amount)||amount<0||amount>700||amount>700-total)return false;total+=amount;}
        return true;
    }
    public static int Apply(CombatState combat,SensoryAfflictions sensory,bool player,int amount,long now)
    {
        if(amount<0||amount>700)throw new ArgumentOutOfRangeException(nameof(amount));
        if(amount==0||(player?combat.PlayerDead:combat.Hp==0))return 0;
        int healed=player?combat.HealPlayer(amount):combat.HealMonster(amount);
        // Project ruling: allocation alone is insufficient. The spell must restore HP.
        if(healed>0)foreach(int condition in new[]{0,1,3})RestorationRules.EndCondition(combat,sensory,player,condition,now);
        return healed;
    }
}
