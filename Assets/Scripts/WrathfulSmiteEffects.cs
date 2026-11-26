using System.Collections.Generic;
public sealed class WrathfulSmiteEffects
{
    public sealed class Entry {public long Id,Dc,Expires;public int Target;}
    readonly List<Entry> entries=new List<Entry>();long sequence;
    public static int Dice(int slot,bool critical=false)=>slot>=1&&slot<=9?slot*(critical?2:1):0;
    public bool CanAdd(long now)=>now>=0&&now<=long.MaxValue-10&&sequence<long.MaxValue;
    public Entry Add(int target,long dc,long now){if(!CanAdd(now))return null;var e=new Entry{Id=++sequence,Target=target,Dc=dc,Expires=now+10};entries.Add(e);return e;}
    public List<Entry> Pending(int target,long now)=>entries.FindAll(e=>e.Target==target&&now>=e.Expires-10&&now<e.Expires);
    public bool Active(int target,long now)=>Pending(target,now).Count>0;
    public bool End(long id)=>entries.RemoveAll(e=>e.Id==id)>0;
    public void Refresh(long now)=>entries.RemoveAll(e=>now>=e.Expires);
    public void Clear(){entries.Clear();sequence=0;}
}
