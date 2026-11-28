using System.Collections.Generic;
public sealed class StinkingCloudEffect
{
    public int X {get;private set;}public int Y {get;private set;}public int Scene {get;private set;}public long Dc {get;private set;}public long Revision {get;private set;}
    public long Expires {get;private set;}long begins=-1;
    readonly Dictionary<int,long> starts=new Dictionary<int,long>(),poisoned=new Dictionary<int,long>(),poisonTime=new Dictionary<int,long>();
    public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
    public bool Begin(int x,int y,int scene,long dc,long now){if(x<0||y<0||scene<0||now<0||now>long.MaxValue-10)return false;EndCloud();X=x;Y=y;Scene=scene;Dc=dc;begins=now;Expires=now+10;return true;}
    public bool Cell(int x,int y,long now){long dx=(long)x-X,dy=(long)y-Y;return Active(now)&&System.Math.Abs(dx)<=4&&System.Math.Abs(dy)<=4&&dx*dx+dy*dy<=16;}
    public bool Intersects(int x,int y,int n,long now){for(int j=0;j<n;j++)for(int i=0;i<n;i++)if(Cell(x+i,y+j,now))return true;return false;}
    public bool ClaimStart(int actor,long turn,long now){long previous;if(!Active(now)||starts.TryGetValue(actor,out previous)&&previous==turn)return false;starts[actor]=turn;return true;}
    public void Poison(int actor,long turn,long now){poisoned[actor]=turn;poisonTime[actor]=now;}
    public bool Poisoned(int actor,long turn,long now){long token;return poisoned.TryGetValue(actor,out token)&&token==turn&&now==poisonTime[actor];}
    public bool ClearPoison(int actor){poisonTime.Remove(actor);return poisoned.Remove(actor);}
    public void EndCloud(){begins=-1;Expires=0;starts.Clear();Revision++;}
    public void Clear(){EndCloud();poisoned.Clear();poisonTime.Clear();}
}
