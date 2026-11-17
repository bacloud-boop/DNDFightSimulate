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
    public static long CapacityOunces(int strength,int size)=>CapacityHalfPounds(strength,size)*8;
    public static bool CanCarryOunces(int strength,int size,long equipmentOunces,long bodyOunces){if(equipmentOunces<0||bodyOunces<0)return false;long cap=CapacityOunces(strength,size);return equipmentOunces<=cap&&bodyOunces<=cap-equipmentOunces;}
    public static bool CanDragOunces(int strength,int size,long bodyOunces)=>bodyOunces>=0&&bodyOunces<=CapacityOunces(strength,size)*2;
    public static int DragSpeedOunces(int strength,int size,long loadOunces,int speed){if(speed<0||!CanDragOunces(strength,size,loadOunces))return 0;return loadOunces>CapacityOunces(strength,size)?Math.Min(5,speed):speed;}
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
