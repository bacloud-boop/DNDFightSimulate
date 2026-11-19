using System;

public static class ReincarnationRules
{
    public const long CastingRounds=600,DeathLimit=144000;
    public static Species2024 SpeciesFromDie(int d10){if(d10<1||d10>10)throw new ArgumentOutOfRangeException(nameof(d10));return (Species2024)(d10-1);}
    public static string Error(CorpseClock corpse,long now)
    {
        if(corpse==null||!corpse.Dead)return "转生术需要已经死亡的目标。";
        if(corpse.Type!=9)return "转生术只能作用于死亡时为类人生物的目标。";
        if(now<0||now>long.MaxValue-CastingRounds)return "世界时间超出范围。";
        return corpse.ProjectedAge(now,CastingRounds)>DeathLimit?"计入1小时施法后，目标已死亡超过10天。":"";
    }
    public static int NewBodyPounds(BodyWeight former){if(former==null||!former.BodyPounds.HasValue)throw new ArgumentException("Former body weight is required");return former.BodyPounds.Value;}
    // The user chose full HP in the new body; this is distinct from configuration refill.
    public static int RestoredHp(int newMaximum){if(newMaximum<1)throw new ArgumentOutOfRangeException(nameof(newMaximum));return newMaximum;}
}
