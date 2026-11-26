public sealed class ShiningSmiteEffect
{
    public int Target {get;private set;}
    public int Slot {get;private set;}
    public long Expires {get;private set;}=-1;
    public static int Dice(int slot,bool critical=false)=>slot>=2&&slot<=9?slot*(critical?2:1):0;
    public static bool ValidTime(long now)=>now>=0&&now<=long.MaxValue-10;
    public bool Begin(int target,int slot,long now){if(Dice(slot)==0||!ValidTime(now))return false;Target=target;Slot=slot;Expires=now+10;return true;}
    public bool Active(long now)=>now>=0&&now>=Expires-10&&now<Expires;
    public bool Outlined(int target,long now)=>Active(now)&&target==Target;
    public void Clear(){Expires=-1;Slot=0;}
}
