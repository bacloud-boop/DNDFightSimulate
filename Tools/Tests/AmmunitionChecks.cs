using System;
class AmmunitionChecks {
 static void Check(bool v){if(!v)throw new Exception("Ammo regression");}
 static void Main(){var ammo=new AmmunitionState();Check(ammo.Loaded("手弩"));Check(ammo.Fire("手弩",false));Check(!ammo.Loaded("手弩")&&!ammo.CanFire("手弩",false));Check(ammo.Loaded("手铳"));Check(!ammo.Fire("手弩",false));Check(ammo.Fire("手弩",true)&&!ammo.Loaded("手弩"));Check(ammo.Fire("短剑",false));Check(!AmmunitionState.RequiresAmmo("飞镖"));ammo.Reset();Check(ammo.Loaded("手弩"));Console.WriteLine("PASS: initial load, independent weapons, no-hand rejection, reload during attack and reset");}
}
