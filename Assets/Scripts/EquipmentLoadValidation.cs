public sealed partial class TrainingGame
{
    bool WeightConfigurationFits(WeightDraft draft,bool beast)
    {
        if(beast)return true;
        int other;if(!int.TryParse(draft.Equipment,out other)||other<0)return false;
        bool player=object.ReferenceEquals(draft,playerWeight);
        return EquipmentLoadRules.AllowedOunces(player?attributes[0]:monsterAttributes[0],Species(player).CapacitySize(player?NaturalPlayerSize:monsterProfile.SizeIndex),((long)other+(player?playerArmor:monsterArmor).Weight)*16+(player?carriedWeapons.CarriedOunces:combat.MonsterWeaponIsHeld?combat.MonsterWeaponOunces:0));
    }
    bool AttributeLoadFits(bool player,string[] inputs,int size,long? weaponOunces=null)
    {
        int strength;if(inputs==null||inputs.Length!=6||!int.TryParse(inputs[0],out strength)||strength<3||strength>20)return false;
        return EquipmentLoadRules.AllowedOunces(strength,Species(player).CapacitySize(size),((long)(player?playerWeight:monsterWeight).Weight.EquipmentPounds+(player?playerArmor:monsterArmor).Weight)*16+(weaponOunces??(player?carriedWeapons.CarriedOunces:combat.MonsterWeaponIsHeld?combat.MonsterWeaponOunces:0)));
    }
    bool ArmorDraftFits(bool player)
    {
        int index=player?playerArmorDraft:monsterArmorDraft;
        int weight=(!player&&monsterCustomDraft?0:ArmorDefinition.All[index].Weight)+((player?playerShieldDraft:monsterShieldDraft)?6:0);
        return EquipmentLoadRules.AllowedOunces(player?attributes[0]:monsterAttributes[0],Species(player).CapacitySize(player?NaturalPlayerSize:monsterProfile.SizeIndex),((long)(player?playerWeight:monsterWeight).Weight.EquipmentPounds+weight)*16+(player?carriedWeapons.CarriedOunces:combat.MonsterWeaponIsHeld?combat.MonsterWeaponOunces:0));
    }
}
