using System;
using UnityEngine;
public static class WeaponVerification
{
    public static void Verify()
    {
        int[] sides={10,8,6};
        for(int i=0;i<sides.Length;i++) {
            var weapon=Weapon.Available[i];
            if(weapon.DiceSides!=sides[i]) throw new Exception("Weapon mapping failed");
            bool min=false,max=false;
            for(int n=0;n<10000;n++) { int d=weapon.RollDamage(); if(d<1||d>sides[i]) throw new Exception("Dice range failed"); min|=d==1;max|=d==sides[i]; }
            if(!min||!max) throw new Exception("Dice boundary failed");
        }
        var selected=Weapon.Available[0]; var attackWeapon=selected; selected=Weapon.Available[2];
        if(attackWeapon.DiceSides!=10||selected.DiceSides!=6) throw new Exception("Weapon snapshot failed");
        var combat=new CombatState(); combat.ApplyDamage(attackWeapon.DiceSides); combat.ApplyDamage(selected.DiceSides);
        if(combat.Hp!=984) throw new Exception("Weapon damage failed");
        combat.EndRound();if(combat.Hp!=1000) throw new Exception("Round reset failed");
        Debug.Log("WEAPON_CHECKS_PASSED");
    }
}
