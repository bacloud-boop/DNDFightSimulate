using System;
using System.Collections.Generic;
public sealed class MoonbeamEffect
{
 public int X{get;private set;}public int Y{get;private set;}public int Scene{get;private set;}public int Slot{get;private set;}
 public long Dc{get;private set;}public long Expires{get;private set;}public long CastTurn{get;private set;}public long Revision{get;private set;}
 long begins=-1;readonly Dictionary<int,long> saves=new Dictionary<int,long>();readonly HashSet<int> barred=new HashSet<int>();
 public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
 public bool Begin(int x,int y,int scene,int slot,long dc,long now,long turn){if(x<0||y<0||scene<0||slot<2||slot>9||now<0||now>long.MaxValue-10)return false;Clear();X=x;Y=y;Scene=scene;Slot=slot;Dc=dc;begins=now;Expires=now+10;CastTurn=turn;return true;}
 public static bool Covers(int cx,int cy,int x,int y,int n){if(n<1)return false;long dx=Math.Max(0,Math.Max((long)x-cx,(long)cx-x-n+1)),dy=Math.Max(0,Math.Max((long)y-cy,(long)cy-y-n+1));return dx<=1&&dy<=1&&dx*dx+dy*dy<=1;}
 public bool Intersects(int x,int y,int n,long now)=>Active(now)&&Covers(X,Y,x,y,n);
 public bool Available(int actor,long turn,long now){long last;return Active(now)&&(!saves.TryGetValue(actor,out last)||last!=turn);}
 public bool Claim(int actor,long turn,long now){if(!Available(actor,turn,now))return false;saves[actor]=turn;return true;}
 public bool CanMove(long now,long turn)=>Active(now)&&turn!=CastTurn;
 public void Place(int x,int y){X=x;Y=y;}
 public void Bar(int actor){barred.Add(actor);}public bool Barred(int actor)=>barred.Contains(actor);public void Leave(int actor){barred.Remove(actor);}
 public void Clear(){begins=-1;Expires=0;saves.Clear();barred.Clear();Revision++;}
}
