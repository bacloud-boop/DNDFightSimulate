using System;
using System.Collections.Generic;
// Instantaneous hits linger until the end of the caster's next turn.
public sealed class StarryWispEffect
{
    public sealed class Mark
    {
        public int Caster,Target,Group;
        public long EndTurn,EndWorld;
    }
    readonly List<Mark> marks=new List<Mark>();
    readonly Dictionary<int,long> ended=new Dictionary<int,long>();
    int nextGroup=100;
    long Turns(int caster)=>ended.ContainsKey(caster)?ended[caster]:0;
    public bool Bind(int caster,int target,long now)
    {
        if(now<0||now>long.MaxValue-2||Turns(caster)>long.MaxValue-2)return false;
        var mark=marks.Find(m=>m.Caster==caster&&m.Target==target);
        if(mark==null){if(nextGroup==int.MaxValue)return false;mark=new Mark{Caster=caster,Target=target,Group=nextGroup++};marks.Add(mark);}
        mark.EndTurn=Turns(caster)+2;mark.EndWorld=now+2;return true;
    }
    public bool Suppresses(int target,long now)=>marks.Exists(m=>m.Target==target&&now>=0&&now<m.EndWorld&&Turns(m.Caster)<m.EndTurn);
    public List<Mark> Active(long now){marks.RemoveAll(m=>now>=m.EndWorld||Turns(m.Caster)>=m.EndTurn);return new List<Mark>(marks);}
    public void EndTurn(int caster){long n=Turns(caster);if(n<long.MaxValue)ended[caster]=n+1;marks.RemoveAll(m=>m.Caster==caster&&Turns(caster)>=m.EndTurn);}
    public void Remove(int group){marks.RemoveAll(m=>m.Group==group);}
    public void Clear(){marks.Clear();ended.Clear();nextGroup=100;}
}
