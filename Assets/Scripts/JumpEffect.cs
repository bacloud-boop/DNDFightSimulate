public sealed class JumpEffect
{
    readonly long[] expires={-1,-1},usedTurn={-1,-1},begins={0,0};
    public bool Grant(bool player,long now){if(now<0||now>long.MaxValue-10)return false;expires[player?0:1]=now+10;begins[player?0:1]=now;return true;}
    public bool Active(bool player,long now)=>now>=begins[player?0:1]&&now>=0&&now<expires[player?0:1];
    public bool Available(bool player,long now,int round)=>Active(player,now)&&usedTurn[player?0:1]!=round;
    public bool Use(bool player,long now,int round){if(!Available(player,now,round))return false;usedTurn[player?0:1]=round;return true;}
    public void Clear(){expires[0]=expires[1]=usedTurn[0]=usedTurn[1]=-1;}
    public static bool EnvironmentAllowed(bool submerged,bool dragging,bool loadFits)=>!submerged&&!dragging&&loadFits;
}
