using System;
using System.Collections.Generic;
public sealed class SearingSmiteEffects
{
    public sealed class Entry {public long Id,Dc,Expires;public int Target,Dice;}
    readonly List<Entry> entries=new List<Entry>();readonly Dictionary<int,long> starts=new Dictionary<int,long>();long sequence;
    public static bool ValidTime(long now)=>now>=0&&now<=long.MaxValue-10;
    public static int Dice(int slot,bool critical=false)=>slot>=1&&slot<=9?slot*(critical?2:1):0;
    public bool CanAdd(long now)=>ValidTime(now)&&sequence<long.MaxValue;
    public Entry Add(int target,int slot,long dc,long now)
    {
        if(Dice(slot)==0||!CanAdd(now))return null;var e=new Entry{Id=++sequence,Target=target,Dice=slot,Dc=dc,Expires=now+10};entries.Add(e);return e;
    }
    public List<Entry> Pending(int target,long now)=>entries.FindAll(e=>e.Target==target&&now>=e.Expires-10&&now<e.Expires);
    public Entry Strongest(int target,long now)
    {
        Entry best=null;foreach(var e in Pending(target,now))if(best==null||e.Dice>best.Dice||e.Dice==best.Dice&&(e.Dc>best.Dc||e.Dc==best.Dc&&e.Id>best.Id))best=e;return best;
    }
    public bool ClaimStart(int target,long stamp,long now)
    {
        long old;if(stamp<0||starts.TryGetValue(target,out old)&&stamp<=old)return false;starts[target]=stamp;return Strongest(target,now)!=null;
    }
    public bool End(long id)=>entries.RemoveAll(e=>e.Id==id)>0;
    public void Refresh(long now)=>entries.RemoveAll(e=>now>=e.Expires);
    public void Clear(){entries.Clear();starts.Clear();sequence=0;}
}
