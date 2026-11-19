public static class UnderwaterCombatRules
{
    public static bool Disadvantage(bool submerged,bool weapon,bool melee,int swimSpeed,bool piercing)
        =>submerged&&weapon&&(!melee||(swimSpeed<=0&&!piercing));
    public static bool AutoMiss(bool submerged,bool weapon,bool melee,int distance,int normalRange)
        =>submerged&&weapon&&!melee&&distance>normalRange;
}
