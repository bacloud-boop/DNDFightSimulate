using System;
using System.Linq;
class WeaponInventoryChecks
{
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Main(){
  foreach(var weapon in Weapon.Available)Check(weapon.WeightOunces>=0,"all existing weapons have known official weights");
  var bag=new WeaponInventory(Weapon.Available.Select(w=>w.Name),"长剑（双手）");
  Check(bag.Items.Count()==Weapon.Available.Length-1&&bag.CarriedOunces==48,"one physical longsword and only initial weapon carried");
  Check(object.ReferenceEquals(bag.Find("长剑（单手）"),bag.Find("长剑（双手）")),"grip variants refer to same object");
  Check(bag.TrySetCarried("飞镖",true,w=>w<=52)&&bag.CarriedOunces==52,"quarter-pound dart remains exact four ounces");
  Check(!bag.TrySetCarried("巨锤",true,w=>w<=52)&&bag.CarriedOunces==52,"overloaded preparation rejects atomically");
  bag.BeginBattle();Check(!bag.TrySetCarried("匕首",true,w=>true),"off-map storage cannot be accessed in battle");
  Check(bag.Drop("长剑（双手）",2,3)&&!bag.Carried("长剑（单手）")&&!bag.Drop("长剑（单手）",2,3)&&bag.CarriedOunces==4,"dropping removes both grip modes without creating duplicate items");
  int paid=0;Check(!bag.PickUp("长剑（单手）",w=>w<=51,()=>{paid++;return true;})&&paid==0,"overweight pickup consumes no interaction");
  Check(!bag.PickUp("长剑（单手）",w=>true,()=>{paid++;return false;})&&!bag.Carried("长剑（双手）"),"failed action callback leaves ground item intact");
  Check(bag.PickUp("长剑（单手）",w=>w==52,()=>true)&&bag.Carried("长剑（双手）")&&!bag.PickUp("长剑（双手）",w=>true,()=>true),"one successful pickup restores unique physical item");
  bag.Drop("飞镖",4,5);bag.ResetBattle();Check(!bag.Locked&&bag.CarriedOunces==52&&bag.Carried("飞镖"),"reset restores saved preparation instead of duplicating ground loot");
  Check(WeightUnits.Format(52)=="3磅4盎司"&&WeightUnits.Format(4)=="0磅4盎司","integer-only readable weights");
  Check(CarryingRules.CapacityOunces(3,0)==360&&CarryingRules.CanCarryOunces(3,0,356,4)&&!CarryingRules.CanCarryOunces(3,0,356,5),"tiny carrying boundary retains half-pound precision with quarter-pound items");
  Check(!CarryingRules.CanCarryOunces(10,2,long.MaxValue,long.MaxValue),"huge ounce values cannot overflow into a valid load");
  var body=new BodyWeight(22);body.SetWeaponWeight(4);var carry=new CorpseCarryingState();
  Check(body.TotalOunces==356&&carry.PickUpOunces(0,true,body,3,0,4,false,()=>true)&&carry.WeightOunces==356,"corpse pickup uses exact ounces at tiny carrying boundary");
  Check(carry.AllowedSpeedOunces(3,0,4,30)==30&&!carry.FitsFormOunces(3,0,5,false),"one ounce over combined carry limit is rejected");
  Console.WriteLine("PASS: unique inventory, longsword grip identity, storage lock, atomic drop/pickup, loadout reset and exact-ounce encumbrance");
 }
}
