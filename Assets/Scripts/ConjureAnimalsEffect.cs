using System.Collections.Generic;
public sealed class ConjureAnimalsEffect
{
    public int X {get;private set;}public int Y {get;private set;}public int Scene {get;private set;}public int Slot {get;private set;}public int Appearance {get;private set;}
    public long Dc {get;private set;}public long Expires {get;private set;}public long Revision {get;private set;}
    long begins=-1,playerTurn=-1;bool moved;public int MovementRemaining {get;private set;}
    readonly Dictionary<int,long> saves=new Dictionary<int,long>();
    public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
    public bool Begin(int x,int y,int scene,int slot,long dc,int appearance,long now,long turn)
    {
        if(x<0||y<0||scene<0||slot<3||slot>9||appearance<0||appearance>2||now<0||now>long.MaxValue-100)return false;
        Clear();X=x;Y=y;Scene=scene;Slot=slot;Dc=dc;Appearance=appearance;begins=now;Expires=now+100;StartPlayerTurn(turn);return true;
    }
    public void StartPlayerTurn(long turn){playerTurn=turn;moved=false;MovementRemaining=30;}
    public void NoticePlayerMovement(long turn,long now){if(Active(now)&&turn==playerTurn)moved=true;}
    public bool CanMove(long turn,long now)=>Active(now)&&turn==playerTurn&&moved&&MovementRemaining>0;
    public bool Move(int x,int y,int feet,long turn,long now){if(x<0||y<0||feet<=0||feet>MovementRemaining||!CanMove(turn,now))return false;X=x;Y=y;MovementRemaining-=feet;return true;}
    public bool Available(int actor,long turn,long now){long previous;return Active(now)&&(!saves.TryGetValue(actor,out previous)||previous!=turn);}
    public bool Claim(int actor,long turn,long now){if(!Available(actor,turn,now))return false;saves[actor]=turn;return true;}
    public void Clear(){begins=-1;Expires=0;playerTurn=-1;moved=false;MovementRemaining=0;saves.Clear();Revision++;}
}
