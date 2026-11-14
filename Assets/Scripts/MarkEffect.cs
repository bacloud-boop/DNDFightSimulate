public sealed class MarkEffect
{
    public bool Active {get;private set;}
    public bool Hex {get;private set;}
    public bool TargetPlayer {get;private set;}
    public int Ability {get;private set;}
    public void Cast(bool hex,bool player,int ability){if(ability<0||ability>5)throw new System.ArgumentOutOfRangeException(nameof(ability));Active=true;Hex=hex;TargetPlayer=player;Ability=ability;}
    public bool Applies(bool player)=>Active&&player==TargetPlayer;
    public bool CheckDisadvantage(bool player,int ability)=>Applies(player)&&Hex&&ability==Ability;
    public int DamageType=>Hex?8:6;
    public static long Duration(bool hex,int slot)=>slot>=5?14400:slot>=3?4800:hex&&slot==2?2400:600;
    public bool RemoveCurse(bool player){if(!Active||!Hex||TargetPlayer!=player)return false;Clear();return true;}
    public void Clear(){Active=false;}
}
