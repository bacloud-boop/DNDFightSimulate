public sealed class ProduceFlameEffect
{
    public const long Duration=100;
    long begins=-1;
    public long Expires {get;private set;}
    public int Caster {get;private set;}
    public bool Begin(int caster,long now){if(caster<0||now<0||now>long.MaxValue-Duration)return false;Caster=caster;begins=now;Expires=now+Duration;return true;}
    public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
    public static int DamageDice(int level)=>level>=17?4:level>=11?3:level>=5?2:1;
    public void Clear(){begins=-1;Expires=0;}
}
