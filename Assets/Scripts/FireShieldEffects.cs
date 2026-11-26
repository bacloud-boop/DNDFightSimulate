using System.Collections.Generic;
public sealed class FireShieldEffects
{
    public sealed class Entry {public long Id,Expires;public int Slot;public bool Cold;public int DamageType=>Cold?4:5;public int Resistance=>Cold?5:4;}
    readonly List<Entry> entries=new List<Entry>();long sequence;
    public static bool ValidTime(long now)=>now>=0&&now<=long.MaxValue-100;
    public Entry Add(int slot,bool cold,long now){if(slot<4||slot>9||!ValidTime(now)||sequence==long.MaxValue)return null;var e=new Entry{Id=++sequence,Slot=slot,Cold=cold,Expires=now+100};entries.Add(e);return e;}
    public Entry Current(long now){entries.RemoveAll(e=>now>=e.Expires);Entry best=null;foreach(var e in entries)if(now>=e.Expires-100&&(best==null||e.Id>best.Id))best=e;return best;}
    public bool End(long id)=>entries.RemoveAll(e=>e.Id==id)>0;
    public void Clear(){entries.Clear();sequence=0;}
}
