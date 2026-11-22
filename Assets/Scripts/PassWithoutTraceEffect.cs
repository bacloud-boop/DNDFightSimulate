using System.Collections.Generic;
public sealed class PassWithoutTraceEffect
{
    public const long Duration=600;
    readonly HashSet<int> chosen=new HashSet<int>();
    long begins=-1;
    public long Expires {get;private set;}
    public bool Begin(IEnumerable<int> targets,long now)
    {
        if(targets==null||now<0||now>long.MaxValue-Duration)return false;
        var draft=new HashSet<int>();foreach(int actor in targets)if(!draft.Add(actor))return false;
        draft.Add(0);chosen.Clear();chosen.UnionWith(draft);begins=now;Expires=now+Duration;return true;
    }
    public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
    public bool Protects(int actor,bool inAura,long now)=>Active(now)&&inAura&&chosen.Contains(actor);
    public int CheckBonus(int actor,int ability,int skill,bool inAura,long now)=>ability==1&&skill==3&&Protects(actor,inAura,now)?10:0;
    public void Clear(){chosen.Clear();begins=-1;Expires=0;}
}
