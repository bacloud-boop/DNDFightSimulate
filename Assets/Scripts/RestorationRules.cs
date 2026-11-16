public static class RestorationRules
{
    public static bool Valid(int condition)=>condition>=0&&condition<4;
    public static int EndCondition(CombatState combat,SensoryAfflictions sensory,bool player,int condition,long now)
    {
        if(!Valid(condition))return 0;
        if(condition<2)return sensory.EndCondition(player,condition==0,now);
        if(condition==2){bool active=player?combat.PlayerParalyzed:combat.MonsterParalyzed;if(player)combat.PlayerParalyzed=false;else combat.MonsterParalyzed=false;return active?1:0;}
        bool poisoned=player?combat.PlayerPoisoned:combat.MonsterPoisoned;combat.ClearPoison(player);return poisoned?1:0;
    }
}
