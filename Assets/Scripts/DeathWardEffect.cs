public sealed class DeathWardEffect
{
    readonly long[] ends={-1,-1};
    public void Cast(bool player,long now){ends[player?0:1]=now+4800;}
    public bool Active(bool player,long now)=>now<ends[player?0:1];
    public bool Consume(bool player,long now){int index=player?0:1;if(!Active(player,now))return false;ends[index]=-1;return true;}
    public void Clear(){ends[0]=ends[1]=-1;}
}
