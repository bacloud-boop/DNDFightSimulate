using System;

// The owner links dismissal to map removal and concentration; no spell is marked
// available merely because this lifecycle model exists.
public sealed class BestialSpiritSession
{
    public const int DurationRounds=600;
    public SummonedUnit Unit {get;private set;}
    public BestialSpiritRules Rules {get;private set;}
    public long Expires {get;private set;}
    public bool Active=>Unit!=null;
    public bool Begin(SummonedUnit unit,BestialSpiritRules rules,long now)
    {
        if(unit==null||!unit.Alive||rules==null||now<0||now>long.MaxValue-DurationRounds)return false;
        if(Active)return false;
        Unit=unit;Rules=rules;Expires=now+DurationRounds;return true;
    }
    public bool ShouldDismiss(long now,bool casterIncapacitated,bool concentrating)
        =>Active&&(!Unit.Alive||casterIncapacitated||!concentrating||now>=Expires);
    public SummonedUnit Dismiss()
    {
        var removed=Unit;if(removed==null)return null;
        removed.Dismiss();Unit=null;Rules=null;Expires=0;return removed;
    }
}
