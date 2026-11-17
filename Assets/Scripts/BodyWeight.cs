using System;
public sealed class BodyWeight
{
    public int? BodyPounds {get;private set;}
    public int EquipmentPounds {get;private set;}
    public int ArmorPounds {get;private set;}
    public long GearPounds=>(long)EquipmentPounds+ArmorPounds;
    public void SetArmorWeight(int pounds){if(pounds<0)throw new ArgumentOutOfRangeException(nameof(pounds));ArmorPounds=pounds;}
    public bool Known=>BodyPounds.HasValue;
    public long TotalPounds=>Known?(long)BodyPounds.Value+GearPounds:-1;
    public BodyWeight(int? bodyPounds=150,int equipmentPounds=0)
    {
        if(bodyPounds.HasValue&&bodyPounds.Value<1||equipmentPounds<0)throw new ArgumentOutOfRangeException();
        BodyPounds=bodyPounds;EquipmentPounds=equipmentPounds;
    }
    public bool TryApply(string body,string equipment)
    {
        int b,e;if(!int.TryParse(body,out b)||b<1||!int.TryParse(equipment,out e)||e<0)return false;
        BodyPounds=b;EquipmentPounds=e;return true;
    }
}
