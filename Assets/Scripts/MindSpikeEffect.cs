public sealed class MindSpikeEffect
{
    public bool Active {get;private set;}
    public bool TargetPlayer {get;private set;}
    public long Expires {get;private set;}
    public void Cast(bool player,long now){Active=true;TargetPlayer=player;Expires=now+600;}
    public bool Tracks(bool targetPlayer,long now)=>Active&&TargetPlayer==targetPlayer&&now<Expires;
    public bool IgnoresInvisibility(bool viewerPlayer,bool targetPlayer,long now)=>viewerPlayer&&Tracks(targetPlayer,now);
    public void Clear(){Active=false;}
}
