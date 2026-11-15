using System;
public sealed class SilenceEffect
{
    public int X {get;private set;}
    public int Y {get;private set;}
    public long Expires {get;private set;}=-1;
    long begins;
    public bool Active(long now)=>now>=begins&&now>=0&&now<Expires;
    public bool Begin(int x,int y,long now){if(now<0||now>long.MaxValue-100)return false;X=x;Y=y;begins=now;Expires=now+100;return true;}
    public bool Contains(double x,double y,long now)=>Active(now)&&(x-X)*(x-X)+(y-Y)*(y-Y)<=16.00000001;
    public bool CentreInside(int x,int y,int spaces,long now)=>spaces>0&&Contains(x+(spaces-1)*.5,y+(spaces-1)*.5,now);
    public bool WholeInside(int x,int y,int spaces,long now)
    {
        if(spaces<1||!Active(now))return false;
        return Contains(x,y,now)&&Contains(x+spaces-1,y,now)&&Contains(x,y+spaces-1,now)&&Contains(x+spaces-1,y+spaces-1,now);
    }
    public bool BlocksSound(double ax,double ay,double bx,double by,long now)
    {
        if(!Active(now))return false;
        double dx=bx-ax,dy=by-ay,length=dx*dx+dy*dy;
        double t=length==0?0:Math.Max(0,Math.Min(1,((X-ax)*dx+(Y-ay)*dy)/length));
        return Contains(ax+t*dx,ay+t*dy,now);
    }
    public bool CanSeeOrHear(bool visible,double ax,double ay,double bx,double by,long now)=>visible||!BlocksSound(ax,ay,bx,by,now);
    public void Clear(){Expires=-1;}
}
