using System;
using System.Collections.Generic;
public sealed class SleetStormEffect
{
    public int X {get;private set;}public int Y {get;private set;}public int Scene {get;private set;}
    public long Dc {get;private set;}public long Revision {get;private set;}public long Expires {get;private set;}long begins=-1;
    readonly Dictionary<int,long> entries=new Dictionary<int,long>(),starts=new Dictionary<int,long>();
    public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
    public bool Begin(int x,int y,int scene,long dc,long now){if(x<0||y<0||scene<0||now<0||now>long.MaxValue-10)return false;Clear();X=x;Y=y;Scene=scene;Dc=dc;begins=now;Expires=now+10;return true;}
    public bool Cell(int x,int y,long now){long dx=(long)x-X,dy=(long)y-Y;return Active(now)&&Math.Abs(dx)<=4&&Math.Abs(dy)<=4&&dx*dx+dy*dy<=16;}
    public bool Intersects(int x,int y,int spaces,long now){for(int j=0;j<spaces;j++)for(int i=0;i<spaces;i++)if(Cell(x+i,y+j,now))return true;return false;}
    public bool Claim(int actor,long turn,bool starting,long now){if(!Active(now))return false;var ledger=starting?starts:entries;long previous;if(ledger.TryGetValue(actor,out previous)&&previous==turn)return false;ledger[actor]=turn;return true;}
    public bool Blocks(double ax,double ay,double bx,double by,long now)
    {
        if(!Active(now))return false;double dx=bx-ax,dy=by-ay,length=dx*dx+dy*dy,t=length==0?0:Math.Max(0,Math.Min(1,((X*5-ax)*dx+(Y*5-ay)*dy)/length));
        double x=ax+t*dx-X*5,y=ay+t*dy-Y*5;return x*x+y*y<=400.000001;
    }
    public void Clear(){begins=-1;Expires=0;entries.Clear();starts.Clear();Revision++;}
}
