using System;
using System.Collections.Generic;
public sealed class SpikeGrowthEffect
{
 public int X {get;private set;}public int Y {get;private set;}public long Dc {get;private set;}
 long begins=-1,expires=-1;readonly Dictionary<int,int> remainders=new Dictionary<int,int>();
 public bool Active(long now)=>begins>=0&&now>=begins&&now<expires;
 public void Begin(int x,int y,long dc,long now){if(now<0||now>long.MaxValue-100)throw new ArgumentOutOfRangeException();Clear();X=x;Y=y;Dc=dc;begins=now;expires=now+100;}
 public bool Cell(int x,int y,long now){long dx=(long)x-X,dy=(long)y-Y;return Active(now)&&Math.Abs(dx)<=4&&Math.Abs(dy)<=4&&dx*dx+dy*dy<=16;}
 public bool Intersects(int x,int y,int spaces,long now){for(int j=0;j<spaces;j++)for(int i=0;i<spaces;i++)if(Cell(x+i,y+j,now))return true;return false;}
 public int Travel(int actor,int feet,long now){if(feet<0)throw new ArgumentOutOfRangeException(nameof(feet));if(!Active(now)||feet==0)return 0;int rest;remainders.TryGetValue(actor,out rest);long total=(long)feet+rest;remainders[actor]=(int)(total%5);return (int)(total/5);}
 public void Clear(){begins=expires=-1;remainders.Clear();}
}
