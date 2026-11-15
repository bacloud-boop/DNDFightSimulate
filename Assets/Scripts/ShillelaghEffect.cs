using System;
public sealed class ShillelaghEffect
{
    public string Weapon {get;private set;}="";
    public int Ability {get;private set;}
    public long Expires {get;private set;}
    public bool Active=>Weapon!="";
    public static bool Eligible(string weapon)=>weapon=="短棒"||weapon=="长棍";
    public static int Sides(int level)=>level>=17?6:level>=11?12:level>=5?10:8;
    public static int Count(int level,bool critical=false)=>(level>=17?2:1)*(critical?2:1);
    public bool Begin(string weapon,int ability,long now)
    {
        if(!Eligible(weapon)||ability<0||ability>5||now<0||now>long.MaxValue-10)return false;
        Weapon=weapon;Ability=ability;Expires=now+10;return true;
    }
    public bool Applies(string weapon,long now)=>Active&&Weapon==weapon&&now<Expires;
    public bool Refresh(string held,bool melded,long now)
    {
        if(!Active||!melded&&Applies(held,now))return false;
        Clear();return true;
    }
    public void Clear(){Weapon="";Expires=0;Ability=0;}
}
