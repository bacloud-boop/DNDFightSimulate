using System.Collections.Generic;

// Each non-concentration casting lasts one hour for its selected creatures.
// A later casting refreshes selected targets without removing other live timers.
public sealed class FreedomMovementEffect
{
    struct Window {public long Begins,Expires;}
    readonly Dictionary<int,Window> targets=new Dictionary<int,Window>();
    public const long Duration=600;
    public static int TargetLimit(int slot)=>slot>=4&&slot<=9?slot-3:0;
    public bool Begin(IEnumerable<int> selected,int slot,long now)
    {
        int limit=TargetLimit(slot);
        if(selected==null||limit==0||now<0||now>long.MaxValue-Duration)return false;
        var draft=new HashSet<int>();
        foreach(int actor in selected)if(!draft.Add(actor)||draft.Count>limit)return false;
        if(draft.Count==0)return false;
        var window=new Window{Begins=now,Expires=now+Duration};
        foreach(int actor in draft)targets[actor]=window;
        return true;
    }
    public bool Active(int actor,long now)
    {
        Window window;return targets.TryGetValue(actor,out window)&&now>=window.Begins&&now<window.Expires;
    }
    public void Clear(){targets.Clear();}
}
