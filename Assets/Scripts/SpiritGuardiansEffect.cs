using System.Collections.Generic;
public sealed class SpiritGuardiansEffect
{
    public int Appearance {get;private set;}public int Slot {get;private set;}public int Type {get;private set;}public long Dc {get;private set;}public long Expires {get;private set;}public long Revision {get;private set;}
    long begins=-1;readonly HashSet<int> excluded=new HashSet<int>();readonly Dictionary<int,long> saves=new Dictionary<int,long>();
    public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
    public bool Begin(int slot,int alignment,long dc,IEnumerable<int> exclusions,long now,int appearance=0){if(slot<3||slot>9||!Alignment2024.Valid(alignment)||now<0||now>long.MaxValue-100)return false;Clear();Appearance=alignment>=6?2:appearance==1?1:0;Slot=slot;Type=Alignment2024.GuardianDamageType(alignment);Dc=dc;begins=now;Expires=now+100;if(exclusions!=null)foreach(int actor in exclusions)excluded.Add(actor);return true;}
    public bool Affects(int actor,long now)=>actor!=0&&Active(now)&&!excluded.Contains(actor);
    public bool Available(int actor,long turn,long now){long previous;return Affects(actor,now)&&(!saves.TryGetValue(actor,out previous)||previous!=turn);}
    public bool Claim(int actor,long turn,long now){if(!Available(actor,turn,now))return false;saves[actor]=turn;return true;}
    public void Clear(){begins=-1;Expires=0;excluded.Clear();saves.Clear();Revision++;}
}
