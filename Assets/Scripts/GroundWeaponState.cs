using System;
// Ground coordinates use half-cell units so even-sized footprints have exact centers.
public sealed class GroundWeaponState
{
 public bool Submerged {get;private set;}
 public int Owner {get;}public bool Dropped {get;private set;}
 public int CenterX2 {get;private set;}public int CenterY2 {get;private set;}
 public GroundWeaponState(int owner){Owner=owner;}
 public bool Drop(int x,int y,int spaces,bool water=false){if(Dropped||x<0||y<0||spaces<1||spaces>4)return false;CenterX2=checked(x*2+spaces);CenterY2=checked(y*2+spaces);Submerged=water;Dropped=true;return true;}
 public bool WithinReach(int x,int y,int spaces){if(!Dropped||spaces<1||spaces>4)return false;long dx=Math.Max(0,Math.Max((long)x*2+1-CenterX2,(long)CenterX2-((long)x*2+spaces*2-1)));long dy=Math.Max(0,Math.Max((long)y*2+1-CenterY2,(long)CenterY2-((long)y*2+spaces*2-1)));return dx<=2&&dy<=2&&dx*dx+dy*dy<=4;}
 public bool PickUp(int actor,int x,int y,int spaces,bool conscious,bool freeHand,Func<bool> fits,Func<bool> pay,bool submerged=false){if(Submerged&&!submerged)return false;if(actor!=Owner||!conscious||!freeHand||!WithinReach(x,y,spaces)||fits==null||pay==null||!fits()||!pay())return false;Reset();return true;}
 public void Reset(){Submerged=false;Dropped=false;CenterX2=CenterY2=0;}
}
