public sealed class PoisonProtectionEffect
{
    readonly long[] begins={0,0},expires={-1,-1};
    public bool Grant(bool player,long now){if(now<0||now>long.MaxValue-600)return false;begins[player?0:1]=now;expires[player?0:1]=now+600;return true;}
    public bool Active(bool player,long now)=>now>=begins[player?0:1]&&now<expires[player?0:1];
    public bool SaveAdvantage(bool player,bool avoidsOrEndsPoisoned,long now)=>avoidsOrEndsPoisoned&&Active(player,now);
    public void Clear(){expires[0]=expires[1]=-1;}
    public static int SelectDie(int first,int second,bool advantage,bool disadvantage)=>advantage==disadvantage?first:advantage?System.Math.Max(first,second):System.Math.Min(first,second);
}
