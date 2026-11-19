using System;
public sealed class MageArmorEffect
{
    readonly long[] expires={-1,-1};
    public bool Grant(bool player,long now){if(now<0||now>long.MaxValue-4800)return false;expires[player?0:1]=now+4800;return true;}
    public int BaseArmor(bool player,int dexterity,long now)=>now<expires[player?0:1]?13+(int)Math.Floor((dexterity-10)/2.0):-1;
    public void Observe(bool player,bool wearingArmor,long now){int i=player?0:1;if(wearingArmor||now>=expires[i])expires[i]=-1;}
    public void Clear(){expires[0]=expires[1]=-1;}
}
