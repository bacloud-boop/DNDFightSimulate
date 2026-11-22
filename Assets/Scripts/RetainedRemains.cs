using System;
using System.Collections.Generic;

// An old body is an independent object; its former creature keeps its own actor ID.
public sealed class RetainedRemains
{
    public int Id {get;}
    public int Owner {get;}
    public int X {get;internal set;}
    public int Y {get;internal set;}
    public int Spaces {get;}
    public string Name {get;}
    public BodyWeight Weight {get;}
    public int ArmorIndex {get;}
    public bool Shield {get;}
    public int CreatureType {get;set;}=9;public int BodySize {get;set;}=2;
    public bool Bones {get;set;}
    public bool NonWeaponGearRecovered {get;private set;}
    public RetainedRemains(int id,int owner,int x,int y,int spaces,string name,BodyWeight weight,int armorIndex,bool shield)
    {
        if(id>=-1||id==int.MinValue||(owner!=0&&owner!=-1&&owner!=int.MinValue)||x<0||y<0||spaces<1||spaces>4||string.IsNullOrEmpty(name)||weight==null||!weight.Known||armorIndex<0)throw new ArgumentException("Invalid retained body");
        Id=id;Owner=owner;X=x;Y=y;Spaces=spaces;Name=name;Weight=weight.Copy();ArmorIndex=armorIndex;Shield=shield;
    }
    public long NonWeaponGearOunces=>Weight.GearOunces-Weight.WeaponOunces;
    public bool RecoverGear(BodyWeight recipient,Func<long,bool> fits,Func<bool> pay)
    {
        long ounces=NonWeaponGearOunces;
        if(NonWeaponGearRecovered||ounces<1||recipient==null||ReferenceEquals(recipient,Weight)||recipient.StoredGearOunces>long.MaxValue-ounces||recipient.GearOunces>long.MaxValue-ounces||fits==null||pay==null||!fits(ounces)||!pay())return false;
        recipient.SetStoredGear(recipient.StoredGearOunces+ounces);
        Weight.TryApply(Weight.BodyPounds.Value.ToString(),"0");Weight.SetArmorWeight(0);Weight.SetStoredGear(0);NonWeaponGearRecovered=true;return true;
    }
}
public sealed class RemainsArchive
{
    readonly List<RetainedRemains> bodies=new List<RetainedRemains>();
    int nextId=-2;
    public int NextId=>nextId;
    public IReadOnlyList<RetainedRemains> Bodies=>bodies.AsReadOnly();
    public RetainedRemains Find(int id)=>bodies.Find(b=>b.Id==id);
    public bool CanAdd=>nextId>int.MinValue;
    public RetainedRemains Add(int owner,int x,int y,int spaces,string name,BodyWeight weight,int armorIndex,bool shield)
    {
        if(!CanAdd)return null;
        var body=new RetainedRemains(nextId,owner,x,y,spaces,name,weight,armorIndex,shield);nextId--;bodies.Add(body);return body;
    }
    public void Clear(){bodies.Clear();nextId=-2;}
}
