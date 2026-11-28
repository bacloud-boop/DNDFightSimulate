using System;
// The line stays attached to its caster; direction and save DC are snapshotted.
public sealed class GustWindEffect
{
    public int Edge{get;private set;}public int Offset{get;private set;}
    public double Dx{get;private set;}public double Dy{get;private set;}
    public long Dc{get;private set;}public long Expires{get;private set;}=-1;
    public long CastTurn{get;private set;}public long Revision{get;private set;}
    public bool Active(long now)=>Expires>=0&&now<Expires;
    public bool Begin(BreathTemplate line,int edge,int offset,long dc,long now,long turn)
    {
        if(!line.Valid||!line.Line||now<0||now>long.MaxValue-10)return false;
        Edge=edge;Offset=offset;Dx=line.Dx;Dy=line.Dy;Dc=dc;Expires=now+10;CastTurn=turn;Revision++;return true;
    }
    public bool Turn(BreathTemplate line,int edge,int offset,long now,long turn)
    {
        if(!Active(now)||turn==CastTurn||!line.Valid||!line.Line)return false;
        Edge=edge;Offset=offset;Dx=line.Dx;Dy=line.Dy;return true;
    }
    public BreathTemplate Template(int x,int y,int spaces,long now)
    {
        if(!Active(now))return default(BreathTemplate);
        int offset=Math.Min(Offset,spaces*2);
        double ox=Edge==0?x+spaces:Edge==1?x:Edge==4?x+spaces*.5:x+offset*.5;
        double oy=Edge==2?y+spaces:Edge==3?y:Edge==4?y+spaces*.5:y+offset*.5;
        BreathTemplate line;BreathTemplate.TryCreate(x,y,spaces,Edge,offset,ox+Dx*20,oy+Dy*20,true,out line,false,60,10);return line;
    }
    // Exact circle/rectangle overlap, including a cloud that only clips an edge.
    public static bool OverlapsCloud(BreathTemplate line,double x,double y,double radius)
    {
        if(!line.Valid||!line.Line||radius<0)return false;
        double ax=x-line.X,ay=y-line.Y,along=ax*line.Dx+ay*line.Dy,across=ax*line.Dy-ay*line.Dx;
        double dx=along-Math.Max(0,Math.Min(line.LengthFeet/5.0,along));
        double half=line.WidthFeet/10.0,dy=across-Math.Max(-half,Math.Min(half,across));
        return dx*dx+dy*dy<=radius*radius+.000001;
    }
    public static bool Against(BreathTemplate line,int fx,int fy,int tx,int ty,int spaces,BreathBody? caster=null)
    {
        if(!line.Intersects(fx,fy,spaces)&&!line.Intersects(tx,ty,spaces))return false;
        if(caster.HasValue){var c=caster.Value;return BodyDistanceSquared(tx,ty,spaces,c)<BodyDistanceSquared(fx,fy,spaces,c);}
        // Moving closer to the source, rather than merely in a compass direction.
        double x=fx+spaces*.5-line.X,y=fy+spaces*.5-line.Y;
        double a=tx+spaces*.5-line.X,b=ty+spaces*.5-line.Y;return a*a+b*b<x*x+y*y-.000001;
    }
    static long BodyDistanceSquared(int x,int y,int n,BreathBody c){long dx=Math.Max(0,Math.Max((long)x-c.X-c.Spaces+1,(long)c.X-x-n+1)),dy=Math.Max(0,Math.Max((long)y-c.Y-c.Spaces+1,(long)c.Y-y-n+1));return dx*dx+dy*dy;}
    public void Clear(){Expires=-1;Revision++;}
}
