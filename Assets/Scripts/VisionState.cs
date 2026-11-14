using System;
using System.Collections.Generic;
public sealed class VisionZone
{
    public double X,Y;public int Level;public bool Dark,Fog;public int FogRadius;public long End;
    public double Radius=>Fog?FogRadius:Dark?15:120;
}
public sealed class VisionState
{
    // 0 bright, 1 dim, 2 dark. Coordinates are in feet, at occupied cell centers.
    public int BaseLight,PlayerRange,MonsterRange;
    readonly long[] darkvisionEnd={-1,-1};
    public readonly List<VisionZone> Zones=new List<VisionZone>();
    public void GrantDarkvision(bool player,long now){darkvisionEnd[player?0:1]=now+4800;}
    public int SpellRange(bool player,long now)=>now<darkvisionEnd[player?0:1]?150:0;
    public int Range(bool player,long now)=>Math.Max(player?PlayerRange:MonsterRange,SpellRange(player,now));
    static bool Contains(VisionZone z,double x,double y,double radius)=>Distance(z.X,z.Y,x,y)<=radius+.000001;
    public static double Distance(double ax,double ay,double bx,double by)=>Math.Sqrt((ax-bx)*(ax-bx)+(ay-by)*(ay-by));
    public void Refresh(long now){Zones.RemoveAll(z=>now>=z.End);}
    public void AddFog(double x,double y,int slot,long now){Refresh(now);Zones.Add(new VisionZone{X=x,Y=y,Fog=true,FogRadius=20*slot,Level=slot,End=now+600});}
    public void EndFog(){Zones.RemoveAll(z=>z.Fog);}
    public bool FogAt(double x,double y,long now){Refresh(now);return Zones.Exists(z=>z.Fog&&Contains(z,x,y,z.Radius));}
    public void EndDarkness(){Zones.RemoveAll(z=>z.Dark);}
    public bool HasDarkness(long now){Refresh(now);return Zones.Exists(z=>z.Dark);}
    public void AddZone(bool dark,double x,double y,int slot,long now){
        Refresh(now);var added=new VisionZone{Dark=dark,X=x,Y=y,Level=slot,End=now+(dark?100:600)};
        // The outer dim-light region is part of Daylight's area.
        Zones.RemoveAll(z=>!z.Fog&&z.Dark!=dark&&Distance(z.X,z.Y,x,y)<=z.Radius+added.Radius&&z.Level<=(dark?2:3));
        if(dark&&slot<=3&&Zones.Exists(z=>!z.Fog&&!z.Dark&&Distance(z.X,z.Y,x,y)<=z.Radius+added.Radius))return;
        // Existing Darkness also dispels a newly overlapping light spell of level <=2.
        if(!dark&&slot<=2&&Zones.Exists(z=>z.Dark&&Distance(z.X,z.Y,x,y)<=z.Radius+added.Radius))return;
        Zones.Add(added);
    }
    public int Light(double x,double y,long now){
        Refresh(now);int magical=2;bool dark=false;
        foreach(var z in Zones){if(z.Fog)continue;if(z.Dark){dark|=Contains(z,x,y,15);}else if(Contains(z,x,y,120))magical=Math.Min(magical,Contains(z,x,y,60)?0:1);}
        return dark?magical:Math.Min(BaseLight,magical);
    }
    // Exact interval of a line segment inside a circular area.
    static bool Interval(VisionZone z,double ax,double ay,double bx,double by,out double lo,out double hi){
        double dx=bx-ax,dy=by-ay,a=dx*dx+dy*dy,ox=ax-z.X,oy=ay-z.Y;lo=0;hi=1;
        if(a<.000001)return Contains(z,ax,ay,z.Radius);
        double b=2*(ox*dx+oy*dy),c=ox*ox+oy*oy-z.Radius*z.Radius,d=b*b-4*a*c;if(d<0)return false;
        lo=Math.Max(0,(-b-Math.Sqrt(d))/(2*a));hi=Math.Min(1,(-b+Math.Sqrt(d))/(2*a));return lo<=hi;
    }
    bool MagicalBlock(double ax,double ay,double bx,double by,long now){
        foreach(var dark in Zones)if(dark.Dark){double lo,hi;if(!Interval(dark,ax,ay,bx,by,out lo,out hi))continue;
            var lights=new List<double[]>();foreach(var light in Zones)if(!light.Fog&&!light.Dark){double a,b;if(Interval(light,ax,ay,bx,by,out a,out b))lights.Add(new[]{a,b});}
            lights.Sort((a,b)=>a[0].CompareTo(b[0]));double covered=lo;foreach(var light in lights){if(light[1]<covered)continue;if(light[0]>covered+.000001)break;covered=Math.Max(covered,light[1]);}if(covered<hi-.000001||hi==lo&&Light(ax+(bx-ax)*lo,ay+(by-ay)*lo,now)==2)return true;
        }return false;
    }
    // 0 clear, 1 dim/visual Perception disadvantage, 2 cannot see.
    public int Quality(double ax,double ay,double bx,double by,int darkvision,long now,int blindsight=0,bool truesight=false){
        Refresh(now);double distance=Distance(ax,ay,bx,by);if(blindsight>0&&Math.Round(distance)<=blindsight)return 0;
        foreach(var fog in Zones)if(fog.Fog){double lo,hi;if(Interval(fog,ax,ay,bx,by,out lo,out hi))return 2;}
        if(truesight)return 0;
        if(MagicalBlock(ax,ay,bx,by,now))return 2;
        int light=Light(bx,by,now);if(darkvision>0&&Math.Round(distance)<=darkvision)light=Math.Max(0,light-1);return light;
    }
    // Last position inside any defender reach circle along a multi-cell mover's path.
    public static double ReachExit(double ax,double ay,double bx,double by,int moverSize,double defenderX,double defenderY,int defenderSize,int reach){
        double vx=bx-ax,vy=by-ay,a=vx*vx+vy*vy,best=-1,radius=(reach+.49999)/5.0;if(a==0)return -1;
        for(int my=0;my<moverSize;my++)for(int mx=0;mx<moverSize;mx++)for(int dy=0;dy<defenderSize;dy++)for(int dx=0;dx<defenderSize;dx++){
            double x=ax+mx-defenderX-dx,y=ay+my-defenderY-dy,b=2*(x*vx+y*vy),c=x*x+y*y-radius*radius,d=b*b-4*a*c;if(d<0)continue;
            double enter=(-b-Math.Sqrt(d))/(2*a),exit=(-b+Math.Sqrt(d))/(2*a);if(enter<=1&&exit>=0)best=Math.Max(best,Math.Min(1,exit));
        }return best;
    }
    public void ClearEffects(){Zones.Clear();darkvisionEnd[0]=darkvisionEnd[1]=-1;}
}
