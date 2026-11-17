using System;

// Six-second world rounds. Only exhaustion caused by suffocation is tracked here.
public sealed class BreathingState
{
    public bool CanBreathe {get;private set;}=true;
    public long BreathEndsAt {get;private set;}
    public int SuffocationExhaustion {get;private set;}
    bool fatal;
    public bool ExhaustionImmune {get;set;}
    public bool Fatal=>(fatal||SuffocationExhaustion>=6)&&!ExhaustionImmune;
    long lastTurn=-1;
    public int D20Penalty=>ExhaustionImmune?0:-2*SuffocationExhaustion;
    public int SpeedPenalty=>ExhaustionImmune?0:5*SuffocationExhaustion;
    public static int HoldRounds(int constitution)
    {
        if(constitution<1||constitution>30)throw new ArgumentOutOfRangeException(nameof(constitution));
        int modifier=(int)Math.Floor((constitution-10)/2.0);
        return Math.Max(5,(1+modifier)*10);
    }
    public void Observe(bool canBreathe,long now,int constitution,bool choking=false)
    {
        if(now<0)throw new ArgumentOutOfRangeException(nameof(now));
        int duration=HoldRounds(constitution);
        if(canBreathe){CanBreathe=true;SuffocationExhaustion=0;BreathEndsAt=0;return;}
        if(CanBreathe)BreathEndsAt=now>long.MaxValue-duration?long.MaxValue:now+duration;
        CanBreathe=false;if(choking)BreathEndsAt=Math.Min(BreathEndsAt,now);
    }
    public bool EndTurn(long sequence,long now,Func<bool> preventFatalIncrease=null)
    {
        if(sequence<0||now<0)throw new ArgumentOutOfRangeException();
        if(sequence<=lastTurn||Fatal)return false;lastTurn=sequence;
        if(ExhaustionImmune||CanBreathe||now<BreathEndsAt)return false;
        // The ward negates only the increment that would reach six, without healing
        // or undoing the five existing levels of suffocation exhaustion.
        if(SuffocationExhaustion==5&&preventFatalIncrease!=null&&preventFatalIncrease())return false;
        SuffocationExhaustion=Math.Min(6,SuffocationExhaustion+1);if(SuffocationExhaustion==6)fatal=true;
        return true;
    }
    public void Reset(){CanBreathe=true;BreathEndsAt=0;SuffocationExhaustion=0;fatal=false;lastTurn=-1;}
    public bool ReconcileRevival(bool canBreathe)
    {
        if(canBreathe){Reset();return true;}
        if(ExhaustionImmune){fatal=false;return true;}
        return !Fatal&&SuffocationExhaustion<6;
    }
}
