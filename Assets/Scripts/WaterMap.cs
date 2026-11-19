using UnityEngine;

public sealed partial class BattleMap
{
    readonly bool[,,] water=new bool[3,20,20];
    public readonly AquaticState PlayerAquatic=new AquaticState(),MonsterAquatic=new AquaticState(),RescueAquatic=new AquaticState();
    public bool SwitchWaterState(bool player,bool submerged)
    {
        if(!Locked||IsContained(player)||player&&Riding)return false;
        var cell=player?Player:Monster;int spaces=player?PlayerSpaces:MonsterSpaces;
        return (player?PlayerAquatic:MonsterAquatic).TrySwitch(submerged,WaterCells(cell,spaces),spaces*spaces,cost=>{
            if(player)return SpendPlayerMovement(cost);
            if(MonsterMovementRemaining<cost)return false;
            SpendMonsterMovement(cost);return true;
        });
    }
    void SyncWaterPositions()
    {
        if(RescuePosition.HasValue)RescueAquatic.ObservePosition(WaterCells(RescuePosition.Value,RescueSpaces),RescueSpaces*RescueSpaces);
        if(IsCarried(true))PlayerAquatic.TrySetSubmerged(RescueAquatic.Submerged,WaterCells(RescuePosition.Value,RescueSpaces),RescueSpaces*RescueSpaces);else PlayerAquatic.ObservePosition(WaterCells(Player,PlayerSpaces),PlayerSpaces*PlayerSpaces);
        if(IsCarried(false))MonsterAquatic.TrySetSubmerged(RescueAquatic.Submerged,WaterCells(RescuePosition.Value,RescueSpaces),RescueSpaces*RescueSpaces);else MonsterAquatic.ObservePosition(WaterCells(Monster,MonsterSpaces),MonsterSpaces*MonsterSpaces);
        foreach(var unit in summons)unit.Aquatic.ObservePosition(WaterCells(new Vector2Int(unit.X,unit.Y),unit.Spaces),unit.Spaces*unit.Spaces);SyncRiderWater();
    }
    public bool IsWater(Vector2Int cell)=>Inside(cell,1)&&water[Selected,cell.x,cell.y];
    public bool SetWater(Vector2Int cell,bool value){if(Locked||!Inside(cell,1))return false;water[Selected,cell.x,cell.y]=value;return true;}
    public bool ClearWater(){if(Locked)return false;for(int y=0;y<Size;y++)for(int x=0;x<Size;x++)water[Selected,x,y]=false;return true;}
    public int WaterCells(Vector2Int anchor,int spaces)
    {
        if(spaces<1||spaces>4||!Inside(anchor,spaces))return -1;
        int count=0;for(int y=anchor.y;y<anchor.y+spaces;y++)for(int x=anchor.x;x<anchor.x+spaces;x++)if(water[Selected,x,y])count++;
        return count;
    }
}
