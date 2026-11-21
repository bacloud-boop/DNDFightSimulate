using System;
using System.Collections.Generic;
public sealed class CloudOfDaggersEffect
{
    public int X {get;private set;}public int Y {get;private set;}public int Slot {get;private set;}public int CastRound {get;private set;}
    public long Revision {get;private set;}long begins=-1,expires=-1;
    readonly Dictionary<int,long> damaged=new Dictionary<int,long>();
    public int Dice=>2*Slot;
    public bool Active(long now)=>begins>=0&&now>=begins&&now<expires;
    public void Begin(int x,int y,int slot,int round,long now){if(slot<2||slot>9||round<1||now<0||now>long.MaxValue-10)throw new ArgumentOutOfRangeException();Clear();X=x;Y=y;Slot=slot;CastRound=round;begins=now;expires=now+10;}
    public bool Intersects(int x,int y,int spaces,long now)=>Active(now)&&spaces>0&&X>=x&&X<(long)x+spaces&&Y>=y&&Y<(long)y+spaces;
    public bool Available(int actor,long turn,long now){long last;return Active(now)&&(!damaged.TryGetValue(actor,out last)||last!=turn);}
    public bool Claim(int actor,long turn,long now){if(!Available(actor,turn,now))return false;damaged[actor]=turn;return true;}
    public bool CanMove(int round,long now,int distance)=>Active(now)&&round>CastRound&&distance>=0&&distance<=30;
    public bool Move(int x,int y,int round,long now,int distance){if(!CanMove(round,now,distance))return false;X=x;Y=y;return true;}
    public void Clear(){begins=expires=-1;damaged.Clear();Revision++;}
}
