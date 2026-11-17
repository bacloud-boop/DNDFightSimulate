using System;
using System.Collections.Generic;
public sealed class EntangleEffect
{
    readonly HashSet<int> restrained=new HashSet<int>();
    public int X {get;private set;} public int Y {get;private set;}
    public long Dc {get;private set;}
    long begins=-1,expires=-1;
    public bool Active(long now)=>now>=begins&&begins>=0&&now<expires;
    public void Begin(int x,int y,long dc,long now){if(now<0||now>long.MaxValue-10)throw new ArgumentOutOfRangeException();Clear();X=x;Y=y;Dc=dc;begins=now;expires=now+10;}
    public bool Intersects(int x,int y,int spaces,long now)=>Active(now)&&spaces>0&&x<X+4&&x+spaces>X&&y<Y+4&&y+spaces>Y;
    public bool FullFootprint(int x,int y,int spaces,long now)=>Active(now)&&spaces>0&&x>=X&&y>=Y&&x+spaces<=X+4&&y+spaces<=Y+4;
    public bool Bind(int actor,long now)=>actor!=0&&Active(now)&&restrained.Add(actor);
    public bool Restrained(int actor,long now)=>Active(now)&&restrained.Contains(actor);
    public bool Release(int actor)=>restrained.Remove(actor);
    public void Clear(){restrained.Clear();begins=expires=-1;}
}
