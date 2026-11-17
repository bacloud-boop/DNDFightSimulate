using System;
public static class CarryingRules
{
    // Half-pound internal units preserve the Tiny 7.5*Strength boundary exactly.
    // User-facing body and equipment inputs can remain whole pounds.
    public static long CapacityHalfPounds(int strength,int size)
    {
        if(strength<1||strength>30||size<0||size>5)throw new ArgumentOutOfRangeException();
        int multiplier=size==0?15:size<=2?30:size==3?60:size==4?120:240;
        return (long)strength*multiplier;
    }
    public static bool CanCarry(int strength,int size,long equipmentPounds,long bodyPounds)
    {
        if(equipmentPounds<0||bodyPounds<0)return false;
        long limit=CapacityHalfPounds(strength,size)/2;return equipmentPounds<=limit&&bodyPounds<=limit-equipmentPounds;
    }
    public static bool CanDrag(int strength,int size,long bodyPounds)
        =>bodyPounds>=0&&bodyPounds<=CapacityHalfPounds(strength,size);
    public static int DragSpeed(int strength,int size,long bodyPounds,int currentSpeed)
    {
        if(currentSpeed<0||!CanDrag(strength,size,bodyPounds))return 0;
        return (long)bodyPounds*2>CapacityHalfPounds(strength,size)?Math.Min(5,currentSpeed):currentSpeed;
    }
}
