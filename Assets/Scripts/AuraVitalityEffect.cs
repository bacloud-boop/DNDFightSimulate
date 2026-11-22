// The runtime resolves the creation heal immediately. Start-of-turn claims
// reserve that turn's optional heal, including when the caster chooses to skip.
public sealed class AuraVitalityEffect
{
    public const long Duration=10;
    public const int RadiusFeet=30,HealingDice=2,HealingSides=6;
    long begins=-1,lastStart=-1;
    public long Expires {get;private set;}
    public long Revision {get;private set;}
    public bool Begin(long now,long turn)
    {
        if(now<0||now>long.MaxValue-Duration||turn<0)return false;
        begins=now;Expires=now+Duration;lastStart=turn;Revision++;return true;
    }
    public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
    public bool ClaimStart(long turn,long now)
    {
        if(!Active(now)||turn<=lastStart)return false;
        lastStart=turn;return true;
    }
    public void Clear(){begins=lastStart=-1;Expires=0;Revision++;}
}
