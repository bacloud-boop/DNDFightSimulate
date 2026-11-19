using System;
// This is spell-specific exertion, not a level of the Exhaustion condition.
public sealed class ResurrectionCasterState
{
    public const long YearRounds=365L*14400,LongRestRounds=4800,RestInterval=14400;
    public bool Taxed {get;private set;}
    public bool CanCast=>!Taxed;
    public bool D20Disadvantage=>Taxed;
    public long? LastLongRestStart {get;private set;}
    public static bool RequiresExertion(string spell,CorpseClock corpse,long now)
        =>spell=="Resurrection"&&corpse!=null&&corpse.Dead&&now>=corpse.DeathAt&&now-corpse.DeathAt>=YearRounds;
    public void ApplyExertion(){Taxed=true;}
    public bool CanLongRest(long now,long required=LongRestRounds)=>(required==2400||required==LongRestRounds)&&now>=0&&now<=long.MaxValue-required&&(!LastLongRestStart.HasValue||now>=LastLongRestStart.Value&&now-LastLongRestStart.Value>=RestInterval);
    public bool CompleteLongRest(long start,long finish,long required=LongRestRounds)
    {
        if(!CanLongRest(start,required)||finish<start||finish-start<required)return false;
        LastLongRestStart=start;Taxed=false;return true;
    }
    public void Reset(){Taxed=false;LastLongRestStart=null;}
}
