public sealed class WitchBoltEffect
{
    public int Target {get;private set;}public int CastRound {get;private set;}
    public long Expires {get;private set;}=-1;long begins=-1;
    public static int InitialDice(int slot,bool critical=false)=>slot>=1&&slot<=9?(slot+1)*(critical?2:1):0;
    public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
    public static bool ValidTime(long now)=>now>=0&&now<=long.MaxValue-10;
    public bool Begin(int target,int round,long now)
    {
        if(round<1||!ValidTime(now))return false;
        Target=target;CastRound=round;begins=now;Expires=now+10;return true;
    }
    public bool CanPulse(int round,long now)=>Active(now)&&round>CastRound;
    public void Clear(){begins=Expires=-1;CastRound=0;}
}
