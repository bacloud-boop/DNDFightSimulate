using System;
// A rest resource, independent of maximum HP and form changes.
public sealed class HitDicePool
{
    public static readonly int[] Faces={4,6,8,10,12,20};
    public int Count {get;private set;}=1;
    public int Sides {get;private set;}=8;
    public int Spent {get;private set;}
    public bool RestReady {get;private set;}
    public int Remaining=>Math.Max(0,Count-Spent);
    public static bool Valid(int count,int sides)=>count>=0&&Array.IndexOf(Faces,sides)>=0;
    public bool Configure(int count,int sides){if(!Valid(count,sides))return false;Count=count;Sides=sides;return true;}
    public void ShortRest(){RestReady=true;}
    public void LongRest(){Spent=0;RestReady=false;}
    public void CloseRest(){RestReady=false;}
    public bool Spend(){if(!RestReady||Remaining==0)return false;Spent++;return true;}
    public static int Healing(int die,int constitutionModifier)=>(int)Math.Max(1,Math.Min(int.MaxValue,(long)die+constitutionModifier));
}
