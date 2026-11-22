using System;
using System.Collections.Generic;
public enum WeaponLocation {Stored,Carried,Ground,Remains,Follower}
public sealed class WeaponInventory
{
 public sealed class Item
 {
  public GroundWeaponState Ground {get;internal set;}
  public string Key {get;internal set;}public int WeightOunces {get;internal set;}
  public WeaponLocation Location {get;internal set;}public int X {get;internal set;}public int Y {get;internal set;}
  public int RemainsId {get;internal set;}public int FollowerId {get;internal set;}
 }
 readonly Dictionary<string,Item> items=new Dictionary<string,Item>();readonly HashSet<string> preparation=new HashSet<string>();
 public bool Locked {get;private set;}
 public IEnumerable<Item> Items=>items.Values;
 public long CarriedOunces{get{long total=0;foreach(var item in items.Values)if(item.Location==WeaponLocation.Carried)total+=item.WeightOunces;return total;}}
 public WeaponInventory(IEnumerable<string> names,string initial){if(names==null)throw new ArgumentNullException(nameof(names));foreach(string name in names){string key=WeaponWeights.Key(name);int weight=WeaponWeights.Ounces(name);if(!items.ContainsKey(key))items.Add(key,new Item{Key=key,WeightOunces=weight,Ground=new GroundWeaponState(0)});}var first=Find(initial);if(first==null)throw new ArgumentException("Initial weapon must be in catalog");first.Location=WeaponLocation.Carried;SavePreparation();}
 public Item Find(string name){Item item;return name!=null&&items.TryGetValue(WeaponWeights.Key(name),out item)?item:null;}
 public bool Carried(string name){var item=Find(name);return item!=null&&item.Location==WeaponLocation.Carried;}
 public bool TrySetCarried(string name,bool carry,Func<long,bool> fits){var item=Find(name);if(Locked||item==null||item.Location==WeaponLocation.Ground)return false;bool current=item.Location==WeaponLocation.Carried;if(current==carry)return true;long candidate=CarriedOunces+(carry?item.WeightOunces:-item.WeightOunces);if(fits==null||!fits(candidate))return false;item.Location=carry?WeaponLocation.Carried:WeaponLocation.Stored;return true;}
 void SavePreparation(){preparation.Clear();foreach(var item in items.Values)if(item.Location==WeaponLocation.Carried)preparation.Add(item.Key);}
 public void BeginBattle(){if(Locked)return;SavePreparation();Locked=true;}
 public bool Drop(string name,int x,int y,int spaces=1,bool water=false,int? container=null){var item=Find(name);if(!Locked||item==null||item.Location!=WeaponLocation.Carried||x<0||y<0)return false;if(!item.Ground.Drop(x,y,spaces,water,container))return false;item.Location=WeaponLocation.Ground;item.X=x;item.Y=y;return true;}
 // The controller checks map reach, hands and turn availability before invoking this atomic transfer.
 public bool PickUp(string name,Func<long,bool> fits,Func<bool> pay,bool submerged=false,int? container=null){var item=Find(name);if(item==null||item.FollowerId!=0||!item.Ground.AccessibleFrom(container)||item.Location!=WeaponLocation.Ground||item.Ground.Submerged&&!submerged||fits==null||pay==null||!fits(CarriedOunces+item.WeightOunces)||!pay())return false;item.Location=WeaponLocation.Carried;item.Ground.Reset();return true;}
 public bool LeaveOnRemains(int id){if(id>=-1||id==int.MinValue||!Locked)return false;foreach(var item in items.Values)if(item.Location==WeaponLocation.Carried){item.Location=WeaponLocation.Remains;item.RemainsId=id;}return true;}
 public long RemainsOunces(int id){long total=0;foreach(var item in items.Values)if(item.Location==WeaponLocation.Remains&&item.RemainsId==id)total+=item.WeightOunces;return total;}
 public bool PickUpFromRemains(string name,int id,Func<long,bool> fits,Func<bool> pay){var item=Find(name);if(item==null||item.Location!=WeaponLocation.Remains||item.RemainsId!=id||fits==null||pay==null||!fits(CarriedOunces+item.WeightOunces)||!pay())return false;item.Location=WeaponLocation.Carried;item.RemainsId=0;return true;}
 public void GiveToFollower(string name,int actor){var item=Find(name);if(item==null||actor<1)return;item.FollowerId=actor;if(item.Location!=WeaponLocation.Ground){item.Location=WeaponLocation.Follower;item.RemainsId=0;}}
 public bool FollowerDrop(string name,int actor,int x,int y,bool water){var item=Find(name);if(item==null||item.FollowerId!=actor||item.Location!=WeaponLocation.Follower||!item.Ground.Drop(x,y,1,water))return false;item.Location=WeaponLocation.Ground;return true;}
 public bool FollowerPickUp(string name,int actor,Func<long,bool> fits,Func<bool> pay,bool submerged){var item=Find(name);if(item==null||item.FollowerId!=actor||item.Location!=WeaponLocation.Ground||!item.Ground.AccessibleFrom(null)||item.Ground.Submerged&&!submerged||!fits(item.WeightOunces)||!pay())return false;item.Ground.Reset();item.Location=WeaponLocation.Follower;return true;}
 public void FollowerRemains(int actor,int remains){foreach(var item in items.Values)if(item.FollowerId==actor){item.FollowerId=0;if(item.Location==WeaponLocation.Follower){item.Location=WeaponLocation.Remains;item.RemainsId=remains;}}}
 public void ResetBattle(){foreach(var item in items.Values){item.Location=preparation.Contains(item.Key)?WeaponLocation.Carried:WeaponLocation.Stored;item.X=item.Y=item.RemainsId=item.FollowerId=0;item.Ground.Reset();}Locked=false;}
}
