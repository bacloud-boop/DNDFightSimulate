using System.Collections.Generic;
public sealed class FeignDeathEffect
{
    sealed class Entry {public long End;public bool Blind=true;}
    readonly Dictionary<int,Entry> effects=new Dictionary<int,Entry>();
    public bool Active(int actor,long now)=>effects.ContainsKey(actor)&&now>=0&&now<effects[actor].End;
    public bool Blind(int actor,long now)=>Active(actor,now)&&effects[actor].Blind;
    public bool Cast(int actor,long now){if(now<0||now>long.MaxValue-600)return false;effects[actor]=new Entry{End=now+600};return true;}
    public bool EndBlind(int actor,long now){if(!Blind(actor,now))return false;effects[actor].Blind=false;return true;}
    public long Remaining(int actor,long now)=>Active(actor,now)?effects[actor].End-now:0;
    public void Clear(){effects.Clear();}
}
