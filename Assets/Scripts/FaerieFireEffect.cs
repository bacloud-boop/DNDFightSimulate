using System;
using System.Collections.Generic;

public sealed class FaerieFireEffect
{
    readonly HashSet<int> targets=new HashSet<int>();
    public int Slot {get;private set;}
    public long Expires {get;private set;}=-1;
    public bool Active(long now)=>now>=0&&now<Expires;
    public bool Begin(int slot,long now){if(slot<1||slot>9||now<0||now>long.MaxValue-10)return false;Clear();Slot=slot;Expires=now+10;return true;}
    public bool Bind(int actor,long now)=>Active(now)&&targets.Add(actor);
    public bool Outlined(int actor,long now)=>Active(now)&&targets.Contains(actor);
    public bool AttackAdvantage(int actor,bool sees,long now)=>sees&&Outlined(actor,now);
    public static bool Intersects(int ax,int ay,int x,int y,int spaces)=>spaces>0&&x<ax+4&&x+spaces>ax&&y<ay+4&&y+spaces>ay;
    public void Clear(){targets.Clear();Expires=-1;}
}
