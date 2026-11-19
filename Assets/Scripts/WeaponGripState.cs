using System;
public sealed class WeaponGripState
{
    public bool Stowed {get;private set;}
    int turn=-1;long world=-1;bool freeUsed,attackCredit;
    void ObserveTurn(int round,long now){if(turn==round&&world==now)return;turn=round;world=now;freeUsed=false;attackCredit=false;}
    public bool TrySetStowed(bool stowed,int round,long now,Func<bool> useAction,bool preparation=false)
    {
        if(stowed==Stowed)return true;
        ObserveTurn(round,now);
        if(!preparation){
            if(attackCredit)attackCredit=false;
            else if(!freeUsed)freeUsed=true;
            else if(useAction==null||!useAction())return false;
        }
        Stowed=stowed;return true;
    }
    public void DropHeld(){Stowed=true;attackCredit=false;}
    public bool TryInteract(int round,long now,Func<bool> useAction){ObserveTurn(round,now);if(!freeUsed){freeUsed=true;return true;}return useAction!=null&&useAction();}
    public void BeginAttack(int round,long now)
    {
        ObserveTurn(round,now);
        // Drawing for this Attack action uses its equip/unequip allowance.
        attackCredit=!Stowed;Stowed=false;
    }
    public void ClearAttackCredit(){attackCredit=false;}
    public void Reset(){Stowed=false;turn=-1;world=-1;freeUsed=attackCredit=false;}
}
