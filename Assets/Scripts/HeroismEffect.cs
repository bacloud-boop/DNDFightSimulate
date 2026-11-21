using System;
using System.Collections.Generic;
public sealed class HeroismEffect
{
    readonly HashSet<int> targets=new HashSet<int>();
    readonly Dictionary<int,long> grantedTurn=new Dictionary<int,long>();
    long begins,expires,castingTurn;
    public long Revision {get;private set;}
    public int Amount {get;private set;}
    public bool Begin(IEnumerable<int> selected,int modifier,int slot,long now,long turn)
    {
        if(selected==null||slot<1||slot>9||now<0||now>long.MaxValue-10)return false;
        var draft=new HashSet<int>(selected);if(draft.Count<1||draft.Count>slot)return false;
        targets.Clear();targets.UnionWith(draft);grantedTurn.Clear();Amount=Math.Max(0,modifier);begins=now;expires=now+10;castingTurn=turn;Revision++;return true;
    }
    public bool Active(int actor,long now)=>targets.Contains(actor)&&now>=begins&&now<expires;
    public bool Claim(int actor,long turn,long now)
    {
        if(!Active(actor,now)||turn<=castingTurn||grantedTurn.ContainsKey(actor)&&grantedTurn[actor]==turn)return false;
        grantedTurn[actor]=turn;return true;
    }
    public void Clear(){targets.Clear();grantedTurn.Clear();Revision++;}
}
