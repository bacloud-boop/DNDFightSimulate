using System;
public sealed class BodyWeight
{
    public int? BodyPounds {get;private set;}
    public int EquipmentPounds {get;private set;}
    public int ArmorPounds {get;private set;}
    public int WeaponOunces {get;private set;}
    public long StoredGearOunces {get;private set;}
    public long GearOunces=>((long)EquipmentPounds+ArmorPounds)*16+WeaponOunces+StoredGearOunces;
    public void SetStoredGear(long ounces){if(ounces<0||ounces>long.MaxValue-((long)EquipmentPounds+ArmorPounds)*16-WeaponOunces)throw new ArgumentOutOfRangeException(nameof(ounces));StoredGearOunces=ounces;}
    public BodyWeight Copy(){var result=new BodyWeight(BodyPounds,EquipmentPounds);result.SetArmorWeight(ArmorPounds);result.SetWeaponWeight(WeaponOunces);result.SetStoredGear(StoredGearOunces);return result;}
    public long TotalOunces=>Known?(long)BodyPounds.Value*16+GearOunces:-1;
    // Legacy whole-pound consumers round up; exact encumbrance uses the ounce properties.
    public long GearPounds=>WeightUnits.WholePoundsCeiling(GearOunces);
    public void SetWeaponWeight(int ounces){if(ounces<0||StoredGearOunces>long.MaxValue-((long)EquipmentPounds+ArmorPounds)*16-ounces)throw new ArgumentOutOfRangeException(nameof(ounces));WeaponOunces=ounces;}
    public void SetArmorWeight(int pounds){if(pounds<0||StoredGearOunces>long.MaxValue-((long)EquipmentPounds+pounds)*16-WeaponOunces)throw new ArgumentOutOfRangeException(nameof(pounds));ArmorPounds=pounds;}
    public bool Known=>BodyPounds.HasValue;
    public long TotalPounds=>Known?(long)BodyPounds.Value+GearPounds:-1;
    public BodyWeight(int? bodyPounds=150,int equipmentPounds=0)
    {
        if(bodyPounds.HasValue&&bodyPounds.Value<1||equipmentPounds<0)throw new ArgumentOutOfRangeException();
        BodyPounds=bodyPounds;EquipmentPounds=equipmentPounds;
    }
    public bool TryApply(string body,string equipment)
    {
        int b,e;if(!int.TryParse(body,out b)||b<1||!int.TryParse(equipment,out e)||e<0||StoredGearOunces>long.MaxValue-((long)e+ArmorPounds)*16-WeaponOunces)return false;
        BodyPounds=b;EquipmentPounds=e;return true;
    }
}
