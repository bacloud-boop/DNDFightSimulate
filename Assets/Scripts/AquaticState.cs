// Geometry and breathing only. The turn controller pays any cost of changing state.
public sealed class AquaticState
{
    public const int SwitchCost=5;
    public bool Submerged {get;private set;}
    public static bool FullyInWater(int waterCells,int occupiedCells)=>occupiedCells>0&&waterCells==occupiedCells;
    public bool TrySetSubmerged(bool value,int waterCells,int occupiedCells)
    {
        if(value&&!FullyInWater(waterCells,occupiedCells))return false;
        Submerged=value;return true;
    }
    public bool TrySwitch(bool value,int waterCells,int occupiedCells,System.Func<int,bool> spendMovement)
    {
        if(value==Submerged||spendMovement==null||(value&&!FullyInWater(waterCells,occupiedCells)))return false;
        if(!spendMovement(SwitchCost))return false;
        Submerged=value;return true;
    }
    public void ObservePosition(int waterCells,int occupiedCells)
    {
        if(!FullyInWater(waterCells,occupiedCells))Submerged=false;
    }
    public bool CanBreathe(bool air,bool water,int waterCells,bool needsBreath=true)
    {
        if(!needsBreath)return true;
        if(Submerged)return water;
        return waterCells>0?air||water:air;
    }
}
