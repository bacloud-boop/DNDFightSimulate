using System;
using System.Collections.Generic;
public enum EquipmentObjectKind {Weapon,Armor,Shield}
public enum EquipmentObjectPlace {Equipped,Remains,Carried,Unavailable}
public sealed class EquipmentObjectRecord
{
    public int Id {get;internal set;}
    public int Owner {get;internal set;}
    public EquipmentObjectKind Kind {get;internal set;}
    public string Key {get;internal set;}
    public string Name {get;internal set;}
    public string InventoryKey {get;internal set;}
    public int Definition {get;internal set;}
    public int Size {get;internal set;}
    public bool Covered {get;internal set;}
    public bool Metal {get;internal set;}
    public EquipmentObjectPlace Place {get;internal set;}
    public int RemainsId {get;internal set;}
    public int Carrier {get;internal set;}public int? Follower {get;set;}
}
// Identity remains stable through corpse storage and recovery; preparation options survive a reset.
public sealed class EquipmentObjectCatalog
{
    struct Options {public int Size;public bool Covered,Metal;}
    public static bool DefaultMetal(EquipmentObjectKind kind,string name,int definition)
    {
        if(kind==EquipmentObjectKind.Shield)return true;if(kind==EquipmentObjectKind.Armor)return definition>=5;
        return name!="短棒"&&name!="巨棒"&&name!="长棍"&&name!="鞭"&&name!="长弓"&&name!="短弓"&&name!="投石索";
    }
    readonly Dictionary<string,Options> preparation=new Dictionary<string,Options>();
    readonly Dictionary<string,EquipmentObjectRecord> weapons=new Dictionary<string,EquipmentObjectRecord>(),slots=new Dictionary<string,EquipmentObjectRecord>();
    readonly List<EquipmentObjectRecord> objects=new List<EquipmentObjectRecord>();int nextId=1;
    public IReadOnlyList<EquipmentObjectRecord> Objects=>objects.ToArray();
    public EquipmentObjectRecord Find(int id){foreach(var item in objects)if(item.Id==id)return item;return null;}
    EquipmentObjectRecord Create(int owner,EquipmentObjectKind kind,string key,string name,int definition,int defaultSize)
    {
        Options option;bool configured=preparation.TryGetValue(key,out option);
        var record=new EquipmentObjectRecord{Id=nextId++,Owner=owner,Kind=kind,Key=key,Name=name,Definition=definition,Size=configured?option.Size:Math.Max(0,Math.Min(5,defaultSize)),Covered=configured&&option.Covered,Metal=configured?option.Metal:DefaultMetal(kind,name,definition),Place=EquipmentObjectPlace.Equipped};objects.Add(record);return record;
    }
    void DefaultSize(EquipmentObjectRecord record,int size,bool preparing){if(preparing&&!preparation.ContainsKey(record.Key))record.Size=Math.Max(0,Math.Min(5,size));}
    public EquipmentObjectRecord Weapon(int owner,string weaponKey,string name,int defaultSize,bool preparing)
    {
        string key=owner+":weapon:"+weaponKey;EquipmentObjectRecord record;if(!weapons.TryGetValue(key,out record)){record=Create(owner,EquipmentObjectKind.Weapon,key,name,0,defaultSize);record.InventoryKey=weaponKey;weapons.Add(key,record);}DefaultSize(record,defaultSize,preparing);return record;
    }
    public EquipmentObjectRecord Slot(int owner,EquipmentObjectKind kind,int definition,string name,bool present,int defaultSize,bool preparing)
    {
        if(kind==EquipmentObjectKind.Weapon)throw new ArgumentException("Use Weapon for inventory objects");
        string slot=owner+":"+kind;EquipmentObjectRecord record;
        if(slots.TryGetValue(slot,out record)&&(!present||record.Definition!=definition)){record.Place=EquipmentObjectPlace.Unavailable;slots.Remove(slot);record=null;}
        if(!present)return null;
        if(record==null){record=Create(owner,kind,slot+":"+definition,name,definition,defaultSize);slots[slot]=record;}
        DefaultSize(record,defaultSize,preparing);return record;
    }
    public bool Configure(int id,int size,bool covered,bool? metal=null)
    {
        var record=Find(id);if(record==null||size<0||size>5)return false;record.Size=size;record.Covered=covered;if(metal.HasValue)record.Metal=metal.Value;preparation[record.Key]=new Options{Size=size,Covered=covered,Metal=record.Metal};return true;
    }
    public bool Cover(int id,bool covered,Func<bool> interact)
    {
        var record=Find(id);if(record==null||record.Covered==covered||interact==null||!interact())return false;record.Covered=covered;return true;
    }
    public void Archive(int actor,int remains)
    {
        foreach(var item in objects)if(item.Kind!=EquipmentObjectKind.Weapon&&(item.Place==EquipmentObjectPlace.Equipped&&item.Owner==actor||item.Place==EquipmentObjectPlace.Carried&&item.Carrier==actor)){
            item.Place=EquipmentObjectPlace.Remains;item.RemainsId=remains;
            string slot=item.Owner+":"+item.Kind;EquipmentObjectRecord current;if(slots.TryGetValue(slot,out current)&&current==item)slots.Remove(slot);
        }
    }
    public void Recover(int remains,int actor)
    {
        foreach(var item in objects)if(item.Place==EquipmentObjectPlace.Remains&&item.RemainsId==remains){item.Place=EquipmentObjectPlace.Carried;item.Carrier=actor;item.RemainsId=0;item.Covered=true;}
    }
    public void ResetBattle(){objects.Clear();weapons.Clear();slots.Clear();nextId=1;}
}
