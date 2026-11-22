using System;
using System.Collections.Generic;

// Coordinates use grid-square units; 1 unit is 5 feet. Coverage uses cell centers.
public struct BreathTemplate
{
    public readonly double X,Y,Dx,Dy;
    public readonly bool Line,Valid,IncludeOrigin;
    public readonly int LengthFeet;
    BreathTemplate(double x,double y,double dx,double dy,bool line,bool includeOrigin,int lengthFeet){X=x;Y=y;Dx=dx;Dy=dy;Line=line;IncludeOrigin=includeOrigin;LengthFeet=lengthFeet;Valid=true;}
    public static bool TryCreate(int x,int y,int spaces,int edge,int offset,double aimX,double aimY,bool line,out BreathTemplate result,bool includeOrigin=false,int lengthFeet=0)
    {
        result=default(BreathTemplate);
        if(spaces<1||edge<0||edge>4||offset<0||offset>spaces*2||lengthFeet<0)return false;
        if(lengthFeet==0)lengthFeet=line?30:15;
        double ox=edge==0?x+spaces:edge==1?x:edge==4?x+spaces*.5:x+offset*.5,oy=edge==2?y+spaces:edge==3?y:edge==4?y+spaces*.5:y+offset*.5;
        double dx=aimX-ox,dy=aimY-oy,length=Math.Sqrt(dx*dx+dy*dy);
        double outward=edge==0?dx:edge==1?-dx:edge==2?dy:edge==3?-dy:length;
        if(length<.000001||outward<=0)return false;
        result=new BreathTemplate(ox,oy,dx/length,dy/length,line,includeOrigin,lengthFeet);return true;
    }
    public bool Cell(int x,int y)
    {
        if(!Valid)return false;double ax=x+.5-X,ay=y+.5-Y,along=ax*Dx+ay*Dy,across=Math.Abs(ax*Dy-ay*Dx);
        if(ax*ax+ay*ay<.000000001)return IncludeOrigin;
        return along>=-.000001&&along<=LengthFeet/5.0+.000001&&across<=(Line?.5:along*.5)+.000001;
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
    static long BodyDistanceSquared(int x,int y,int spaces,BreathBody target)
    {
        long dx=Math.Max(0,Math.Max((long)x-target.X-target.Spaces+1,(long)target.X-x-spaces+1));
        long dy=Math.Max(0,Math.Max((long)y-target.Y-target.Spaces+1,(long)target.Y-y-spaces+1));return dx*dx+dy*dy;
    }
    // Keep the cone/breath selector's defaults unchanged. This selector considers
    // lines only, and breaks equal coverage toward the nearest covered enemy.
    public static bool ChooseLine(int mapSize,int x,int y,int spaces,IReadOnlyList<BreathBody> enemies,out BreathTemplate best,int lengthFeet=100)
    {
        best=default(BreathTemplate);if(mapSize<1||spaces<1||enemies==null||enemies.Count==0||lengthFeet<1)return false;
        int most=0;long nearest=long.MaxValue;
        for(int edge=0;edge<5;edge++)for(int offset=0;offset<=(edge==4?0:spaces*2);offset++)
        for(int row=0;row<=mapSize*2;row++)for(int column=0;column<=mapSize*2;column++){
            BreathTemplate candidate;if(!BreathTemplate.TryCreate(x,y,spaces,edge,offset,column*.5,row*.5,true,out candidate,false,lengthFeet)||candidate.Intersects(x,y,spaces))continue;
            int count=0;long close=long.MaxValue;
            foreach(var body in enemies)if(candidate.Intersects(body.X,body.Y,body.Spaces)){count++;close=Math.Min(close,BodyDistanceSquared(x,y,spaces,body));}
            if(count>most||count==most&&count>0&&close<nearest){most=count;nearest=close;best=candidate;}
        }return most>0;
    }
    // Aim points include grid intersections, edge midpoints and cell centers.
    public static bool Choose(int mapSize,int x,int y,int spaces,IReadOnlyList<BreathBody> enemies,out BreathTemplate best,bool allowLine=true,int coneLengthFeet=15,bool excludeSource=true)
    {
        best=default(BreathTemplate);int most=0;
        if(enemies==null||enemies.Count==0||coneLengthFeet<1)return false;
        for(int shape=0;shape<(allowLine?2:1);shape++)for(int edge=0;edge<5;edge++)for(int offset=0;offset<=(edge==4?0:spaces*2);offset++)
        for(int row=0;row<=mapSize*2;row++)for(int column=0;column<=mapSize*2;column++){
            BreathTemplate candidate;if(!BreathTemplate.TryCreate(x,y,spaces,edge,offset,column*.5,row*.5,shape==1,out candidate,false,shape==1?30:coneLengthFeet))continue;
            if(excludeSource&&candidate.Intersects(x,y,spaces))continue;
            int count=0;foreach(var body in enemies)if(candidate.Intersects(body.X,body.Y,body.Spaces))count++;
            if(count>most){most=count;best=candidate;}
        }return most>0;
    }
}

public static class LightningBoltRules
{
    public const int LengthFeet=100,WidthFeet=5;
    public static int DiceCount(int slot)=>slot>=3&&slot<=9?slot+5:0;
    public static bool DirectionAim(int x,int y,int spaces,int edge,int offset,int direction,out double aimX,out double aimY)
    {
        aimX=aimY=0;if(spaces<1||edge<0||edge>4||offset<0||offset>spaces*2||direction<0||direction>7)return false;
        int[] dx={1,1,0,-1,-1,-1,0,1},dy={0,-1,-1,-1,0,1,1,1};
        double ox=edge==0?x+spaces:edge==1?x:edge==4?x+spaces*.5:x+offset*.5;
        double oy=edge==2?y+spaces:edge==3?y:edge==4?y+spaces*.5:y+offset*.5;
        aimX=ox+dx[direction]*20;aimY=oy+dy[direction]*20;return true;
    }
}
