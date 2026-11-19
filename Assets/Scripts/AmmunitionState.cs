using System.Collections.Generic;
public sealed class AmmunitionState
{
    readonly HashSet<string> empty=new HashSet<string>();
    public static bool RequiresAmmo(string weapon)=>weapon=="长弓"||weapon=="短弓"||weapon=="轻弩"||weapon=="重弩"||weapon=="手弩"||weapon=="投石索"||weapon=="吹箭筒"||weapon=="火铳"||weapon=="手铳";
    public bool Loaded(string weapon)=>RequiresAmmo(weapon)&&!empty.Contains(weapon);
    public bool CanFire(string weapon,bool freeHand)=>!RequiresAmmo(weapon)||Loaded(weapon)||freeHand;
    // Reloading is part of the attack, not a separate action. Switching or stowing
    // a weapon never creates ammunition or restores its initial loaded state.
    public bool Fire(string weapon,bool freeHand)
    {
        if(!CanFire(weapon,freeHand))return false;
        if(RequiresAmmo(weapon))empty.Add(weapon);return true;
    }
    public void Reset(){empty.Clear();}
}
