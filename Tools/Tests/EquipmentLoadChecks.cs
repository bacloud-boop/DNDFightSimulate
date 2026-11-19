using System;
class EquipmentLoadChecks {
 static void C(bool v){if(!v)throw new Exception("Load regression");}
 static void Main(){C(!EquipmentLoadRules.Allowed(3,2,0,65));C(EquipmentLoadRules.Allowed(3,2,0,45));C(!EquipmentLoadRules.Allowed(3,2,1,45));C(EquipmentLoadRules.Allowed(3,3,0,71));C(!EquipmentLoadRules.Allowed(3,0,23,0));C(EquipmentLoadRules.Allowed(3,0,22,0));C(!EquipmentLoadRules.Allowed(20,2,long.MaxValue,71));C(!EquipmentLoadRules.Allowed(3,-1,0,0));Console.WriteLine("PASS: exact carrying boundary, armor plus other gear, size changes, tiny rounding and overflow");}
}
