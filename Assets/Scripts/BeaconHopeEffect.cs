using System.Collections.Generic;

// A casting fixes its chosen creatures; leaving the initial selection area
// does not end the benefits. The runtime owns concentration and save advantage.
public sealed class BeaconHopeEffect
{
    public const long Duration=10;
    public const int RadiusFeet=30;
    readonly HashSet<int> targets=new HashSet<int>();
    long begins=-1;
    public long Expires {get;private set;}
    public long Revision {get;private set;}
    public bool Begin(IEnumerable<int> selected,long now)
    {
        if(selected==null||now<0||now>long.MaxValue-Duration)return false;
        var draft=new HashSet<int>();
        foreach(int actor in selected)if(!draft.Add(actor))return false;
        targets.Clear();targets.UnionWith(draft);begins=now;Expires=now+Duration;Revision++;return true;
    }
    public bool Active(int actor,long now)=>begins>=0&&now>=begins&&now<Expires&&targets.Contains(actor);
    // Pass the whole healing formula's maximum, including fixed modifiers.
    // Fixed transfers omit maximum so that they cannot be enlarged accidentally.
    public static int ResolveHealing(bool affected,int rolled,long? maximum=null)
    {
        long amount=affected&&maximum.HasValue?maximum.Value:rolled;
        return amount<=0?0:amount>=int.MaxValue?int.MaxValue:(int)amount;
    }
    public void Clear(){targets.Clear();begins=-1;Expires=0;Revision++;}
}
