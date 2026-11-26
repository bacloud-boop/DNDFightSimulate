public sealed class FlameBladeEffect
{
    public const int Duration=100;
    public int Slot {get;private set;}
    public int Caster {get;private set;}
    public long Expires {get;private set;}=-1;
    public bool Held {get;private set;}
    public static bool ValidTime(long now)=>now>=0&&now<=long.MaxValue-Duration;
    public static int Dice(int slot,bool critical=false)=>slot>=2&&slot<=9?(slot+1)*(critical?2:1):0;
    // A two-handed ordinary weapon can be held in one hand between attacks.
    public static bool FreeHand(bool ordinaryHeld,bool shield,bool bladeHeld)=>(ordinaryHeld?1:0)+(shield?1:0)+(bladeHeld?1:0)<2;
    public static bool CanDraw(bool shield,bool bladeHeld)=>!(shield&&bladeHeld);
    public static bool CanAttack(bool twoHanded,bool shield,bool bladeHeld)=>twoHanded?!shield&&!bladeHeld:CanDraw(shield,bladeHeld);
    public static bool CanReload(bool shield,bool bladeHeld)=>!shield&&!bladeHeld;
    public bool Active(long now)=>now>=0&&now>=Expires-Duration&&now<Expires;
    public bool Begin(int slot,int caster,long now){if(Dice(slot)==0||!ValidTime(now))return false;Slot=slot;Caster=caster;Expires=now+Duration;Held=true;return true;}
    public void LetGo(){Held=false;}
    public bool Evoke(long now,bool freeHand,bool beast){if(!Active(now)||Held||!freeHand||beast)return false;Held=true;return true;}
    public void Clear(){Held=false;Slot=0;Expires=-1;}
}
