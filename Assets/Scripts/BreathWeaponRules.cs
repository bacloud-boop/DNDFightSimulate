using System;
using System.Collections.Generic;

// Coordinates use grid-square units; 1 unit is 5 feet. Coverage uses cell centers.
public struct BreathTemplate
{
    public readonly double X,Y,Dx,Dy;
    public readonly bool Line,Valid,IncludeOrigin;
    BreathTemplate(double x,double y,double dx,double dy,bool line,bool includeOrigin){X=x;Y=y;Dx=dx;Dy=dy;Line=line;IncludeOrigin=includeOrigin;Valid=true;}
    public static bool TryCreate(int x,int y,int spaces,int edge,int offset,double aimX,double aimY,bool line,out BreathTemplate result,bool includeOrigin=false)
    {
        result=default(BreathTemplate);
        if(spaces<1||edge<0||edge>4||offset<0||offset>spaces*2)return false;
        double ox=edge==0?x+spaces:edge==1?x:edge==4?x+spaces*.5:x+offset*.5,oy=edge==2?y+spaces:edge==3?y:edge==4?y+spaces*.5:y+offset*.5;
        double dx=aimX-ox,dy=aimY-oy,length=Math.Sqrt(dx*dx+dy*dy);
        double outward=edge==0?dx:edge==1?-dx:edge==2?dy:edge==3?-dy:length;
        if(length<.000001||outward<=0)return false;
        result=new BreathTemplate(ox,oy,dx/length,dy/length,line,includeOrigin);return true;
    }
    public bool Cell(int x,int y)
    {
        if(!Valid)return false;double ax=x+.5-X,ay=y+.5-Y,along=ax*Dx+ay*Dy,across=Math.Abs(ax*Dy-ay*Dx);
        if(ax*ax+ay*ay<.000000001)return IncludeOrigin;
        return along>=-.000001&&along<=(Line?6:3)+.000001&&across<=(Line?.5:along*.5)+.000001;
    }
    public bool Intersects(int x,int y,int spaces){for(int j=0;j<spaces;j++)for(int i=0;i<spaces;i++)if(Cell(x+i,y+j))return true;return false;}
}
public struct BreathBody
{
    public int Id,X,Y,Spaces;
    public BreathBody(int id,int x,int y,int spaces){Id=id;X=x;Y=y;Spaces=spaces;}
}
public static class BreathWeaponRules
{
    public static int DiceCount(int level)=>level>=17?4:level>=11?3:level>=5?2:1;
    public static long SaveDc(int constitution,int proficiency)=>8L+(int)Math.Floor((constitution-10)/2.0)+proficiency;
    public static int DamageType(int lineage){int[] types={3,7,5,7,3,5,9,5,4,4};if(lineage<0||lineage>=types.Length)throw new ArgumentOutOfRangeException(nameof(lineage));return types[lineage];}
    // Aim points include grid intersections, edge midpoints and cell centers.
    public static bool Choose(int mapSize,int x,int y,int spaces,IReadOnlyList<BreathBody> enemies,out BreathTemplate best,bool allowLine=true)
    {
        best=default(BreathTemplate);int most=0;
        if(enemies==null||enemies.Count==0)return false;
        for(int shape=0;shape<(allowLine?2:1);shape++)for(int edge=0;edge<5;edge++)for(int offset=0;offset<=(edge==4?0:spaces*2);offset++)
        for(int row=0;row<=mapSize*2;row++)for(int column=0;column<=mapSize*2;column++){
            BreathTemplate candidate;if(!BreathTemplate.TryCreate(x,y,spaces,edge,offset,column*.5,row*.5,shape==1,out candidate))continue;
            if(candidate.Intersects(x,y,spaces))continue;
            int count=0;foreach(var body in enemies)if(candidate.Intersects(body.X,body.Y,body.Spaces))count++;
            if(count>most){most=count;best=candidate;}
        }return most>0;
    }
}
