public static class EquipmentLoadRules
{
    public static bool Allowed(int strength,int size,long otherEquipment,int armorWeight)
    {
        if(strength<1||strength>30||size<0||size>5||otherEquipment<0||armorWeight<0)return false;
        long limit=CarryingRules.CapacityHalfPounds(strength,size)/2;
        return armorWeight<=limit&&otherEquipment<=limit-armorWeight;
    }
}
