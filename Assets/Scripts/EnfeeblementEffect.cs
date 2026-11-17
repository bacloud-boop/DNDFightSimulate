using System;
// One concentration spell. The successful-save branch expires at the caster's next turn.
public sealed class EnfeeblementEffect
{
    bool present,targetPlayer,failed,nextAttack;
    long begins,expires;
    int casterRound;
    public long Dc {get;private set;}
    public bool CanCast(long now)=>now>=0&&now<=long.MaxValue-10;
    public void Cast(bool player,bool failedSave,long dc,long now,int round)
    {
        if(!CanCast(now))throw new ArgumentOutOfRangeException(nameof(now));
        present=true;targetPlayer=player;failed=failedSave;nextAttack=!failedSave;
        Dc=dc;begins=now;expires=now+10;casterRound=round;
    }
    public bool Active(bool player,long now,int round)=>present&&targetPlayer==player&&now>=begins&&now<expires&&(failed||nextAttack&&round<=casterRound);
    public bool Weakened(bool player,long now,int round)=>failed&&Active(player,now,round);
    public bool StrengthDisadvantage(bool player,int ability,long now,int round)=>ability==0&&Weakened(player,now,round);
    public bool ConsumeAttack(bool player,int ability,long now,int round)
    {
        if(!Active(player,now,round))return false;
        if(failed)return ability==0;
        nextAttack=false;return true;
    }
    public void Clear(){present=failed=nextAttack=false;Dc=0;}
}
