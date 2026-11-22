public sealed class DragonBreathEffect
{
    public int Target {get;private set;}public int Slot {get;private set;}public int Type {get;private set;}
    public long Dc {get;private set;}public long Expires {get;private set;}=-1;
    long begins=-1;
    public static int DiceCount(int slot)=>slot>=2&&slot<=9?slot+1:0;
    public static bool ValidType(int type)=>type==3||type==4||type==5||type==7||type==9;
    public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
    public bool Begin(int target,int slot,int type,long dc,long now)
    {
        if(target==int.MaxValue||DiceCount(slot)==0||!ValidType(type)||now<0||now>long.MaxValue-10)return false;
        Target=target;Slot=slot;Type=type;Dc=dc;begins=now;Expires=now+10;return true;
    }
    public void Clear(){begins=Expires=-1;}
}
