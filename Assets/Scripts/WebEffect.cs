using System;
using System.Collections.Generic;
public sealed class WebEffect
{
    public int X {get;private set;}public int Y {get;private set;}
    public long Dc {get;private set;}public long Revision {get;private set;}
    long begins=-1,expires=-1;int mask;
    readonly HashSet<int> held=new HashSet<int>();
    readonly Dictionary<int,long> saves=new Dictionary<int,long>(),turns=new Dictionary<int,long>();
    sealed class Fire {public int Owner;public long EndTurn,Started;}
    readonly Dictionary<int,Fire> fires=new Dictionary<int,Fire>();
    long Turns(int actor)=>turns.ContainsKey(actor)?turns[actor]:0;
    public bool Active(long now)=>begins>=0&&now>=begins&&now<expires;
    public void Begin(int x,int y,int landMask,long dc,long now)
    {
        if(now<0||now>long.MaxValue-600||landMask<0||landMask>65535)throw new ArgumentOutOfRangeException();
        Clear();X=x;Y=y;mask=landMask;Dc=dc;begins=now;expires=now+600;
    }
    int Index(int x,int y){long dx=(long)x-X,dy=(long)y-Y;return dx<0||dx>=4||dy<0||dy>=4?-1:(int)(dy*4+dx);}
    public bool Cell(int x,int y,long now){int i=Index(x,y);return Active(now)&&i>=0&&(mask&(1<<i))!=0;}
    public bool Intersects(int x,int y,int spaces,long now){for(int j=0;j<spaces;j++)for(int i=0;i<spaces;i++)if(Cell(x+i,y+j,now))return true;return false;}
    public bool TrySave(int actor,long turn,long now){long last;if(!Active(now)||saves.TryGetValue(actor,out last)&&last==turn)return false;saves[actor]=turn;return true;}
    public bool Bind(int actor,long now)=>Active(now)&&held.Add(actor);
    public bool Restrained(int actor,long now)=>Active(now)&&held.Contains(actor);
    public bool Release(int actor)=>held.Remove(actor);
    public bool Ignite(int x,int y,int owner,long now)
    {
        int i=Index(x,y);if(!Cell(x,y,now)||fires.ContainsKey(i))return false;
        fires[i]=new Fire{Owner=owner,Started=now,EndTurn=Turns(owner)+1};return true;
    }
    public bool Burning(int x,int y,long now){int i=Index(x,y);return Cell(x,y,now)&&fires.ContainsKey(i);}
    public bool BurningBody(int x,int y,int spaces,long now){for(int j=0;j<spaces;j++)for(int i=0;i<spaces;i++)if(Burning(x+i,y+j,now))return true;return false;}
    public void StartTurn(int actor){turns[actor]=Turns(actor)+1;foreach(var item in new List<KeyValuePair<int,Fire>>(fires))if(item.Value.Owner==actor&&Turns(actor)>=item.Value.EndTurn){mask&=~(1<<item.Key);fires.Remove(item.Key);}}
    public void ExpireMissingSummonOwners(Func<int,bool> exists){foreach(var item in new List<KeyValuePair<int,Fire>>(fires))if(item.Value.Owner>0&&!exists(item.Value.Owner)){mask&=~(1<<item.Key);fires.Remove(item.Key);}}
    public void Refresh(long now,bool aftermath=false){foreach(var item in new List<KeyValuePair<int,Fire>>(fires))if(now-item.Value.Started>=(aftermath?1:2)){mask&=~(1<<item.Key);fires.Remove(item.Key);}}
    public void Clear(){begins=expires=-1;mask=0;held.Clear();saves.Clear();fires.Clear();Revision++;}
}
