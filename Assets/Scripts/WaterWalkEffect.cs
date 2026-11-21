using System.Collections.Generic;

// Non-concentration castings keep independent target timers. Recasting refreshes
// only the selected creatures; it never removes another casting's other targets.
public sealed class WaterWalkEffect
{
    struct Window {public long Begins,Expires;}
    readonly Dictionary<int,Window> targets=new Dictionary<int,Window>();
    public const long Duration=600;
    public bool Begin(IEnumerable<int> selected,long now)
    {
        if(selected==null||now<0||now>long.MaxValue-Duration)return false;
        var draft=new HashSet<int>(selected);
        if(draft.Count<1||draft.Count>10)return false;
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
