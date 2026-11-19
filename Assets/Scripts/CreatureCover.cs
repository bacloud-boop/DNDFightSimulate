using System;
using System.Collections.Generic;

// Coordinates use map cells. Callers provide bodies, including grounded corpses;
// vanished spirits and lifted corpses have no independent footprint.
public static class CreatureCover
{
    public struct Body
    {
        public readonly double X,Y,Size;
        public Body(double x,double y,double size){X=x;Y=y;Size=size;}
    }
    static bool Blocks(double ax,double ay,double bx,double by,Body obstacle)
    {
        if(obstacle.Size<=0)return false;
        if(ax>obstacle.X&&ax<obstacle.X+obstacle.Size&&ay>obstacle.Y&&ay<obstacle.Y+obstacle.Size)return false;
        double enter=0,leave=1;
        for(int axis=0;axis<2;axis++){
            double start=axis==0?ax:ay,delta=(axis==0?bx:by)-start;
            double low=(axis==0?obstacle.X:obstacle.Y)+1e-7,high=(axis==0?obstacle.X:obstacle.Y)+obstacle.Size-1e-7;
            if(Math.Abs(delta)<1e-10){if(start<low||start>high)return false;}
            else {double a=(low-start)/delta,b=(high-start)/delta;if(a>b){double swap=a;a=b;b=swap;}enter=Math.Max(enter,a);leave=Math.Min(leave,b);if(enter>leave)return false;}
        }
        return enter<=leave&&leave>1e-7&&enter<1-1e-7;
    }
    public static int Bonus(Body source,Body target,IReadOnlyList<Body> obstacles)
    {
        if(source.Size<0||target.Size<=0||obstacles==null)return 0;
        // The attacker chooses the most favourable origin corner. Multiple bodies
        // block a ray only once; creature cover never accumulates beyond +2.
        for(int origin=0;origin<4;origin++){
            double ax=source.X+(origin%2)*source.Size,ay=source.Y+(origin/2)*source.Size;
            int blocked=0;
            for(int corner=0;corner<4;corner++){
                double bx=target.X+(corner%2)*target.Size,by=target.Y+(corner/2)*target.Size;
                foreach(var body in obstacles)if(Blocks(ax,ay,bx,by,body)){blocked++;break;}
            }
            if(blocked<2)return 0;
        }
        return 2;
    }
}
