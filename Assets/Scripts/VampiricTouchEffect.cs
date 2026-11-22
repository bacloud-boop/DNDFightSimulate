using System;
public sealed class VampiricTouchEffect
{
    public int Slot {get;private set;}public int Caster {get;private set;}
    public long Expires {get;private set;}=-1;long begins=-1;
    public static int DiceCount(int slot,bool critical=false)=>slot>=3&&slot<=9?slot*(critical?2:1):0;
    public static int Healing(long necrotic)=> (int)Math.Min(int.MaxValue,Math.Max(0,necrotic)/2);
    public static bool ValidTime(long now)=>now>=0&&now<=long.MaxValue-10;
    public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
    public bool Begin(int slot,int caster,long now)
    {
        if(DiceCount(slot)==0||!ValidTime(now))return false;
        Slot=slot;Caster=caster;begins=now;Expires=now+10;return true;
    }
    public void Clear(){begins=Expires=-1;}
}
