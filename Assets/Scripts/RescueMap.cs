using System;
using UnityEngine;
public sealed partial class BattleMap
{
    public const int RescueId=int.MinValue;
    public Vector2Int? RescuePosition {get;private set;}
    public int RescueSpaces {get;private set;}=1;
    public bool CanDeployRescue(Vector2Int cell,int spaces,bool edgeOnly)
    {
        if(!Locked||RescuePosition.HasValue||!CanPlaceSummon(cell,spaces))return false;
        return !edgeOnly||cell.x==0||cell.y==0||cell.x+spaces==Size||cell.y+spaces==Size;
    }
    public bool DeployRescue(Vector2Int cell,int spaces,bool edgeOnly)
    {
        if(!CanDeployRescue(cell,spaces,edgeOnly))return false;RescuePosition=cell;RescueSpaces=spaces;MovementObserved?.Invoke(RescueId,new Vector2Int(-100,-100),cell,spaces,spaces,true);return true;
    }
    public bool CanMoveRescue(Vector2Int cell)
    {
        if(!Locked||!RescuePosition.HasValue||cell==RescuePosition.Value||!RescueDestinationClear(cell))return false;
        var from=RescuePosition.Value;
        return CreaturePathClear(RescueId,from,cell,RescueSpaces);
    }
    public bool MoveRescue(Vector2Int cell,Func<int,bool> spendMovement)
    {
        if(!CanMoveRescue(cell)||spendMovement==null)return false;
        if(!spendMovement(RescueMovementCost(cell)))return false;
        var from=RescuePosition.Value;RescuePosition=cell;SyncRescueCargo();SyncWaterPositions();MovementObserved?.Invoke(RescueId,from,cell,RescueSpaces,RescueSpaces,false);return true;
    }
    public bool RemoveRescue(bool reset=false){if(RescueCargoId.HasValue&&!reset)return false;if(RescueCargoPlayer.HasValue){Player=Vector2Int.zero;Monster=new Vector2Int(Size-MonsterSpaces,Size-MonsterSpaces);}RescuePosition=null;RescueSpaces=1;RescueCargoId=null;RescueCargoDragging=false;RescueAquatic.TrySetSubmerged(false,0,1);return true;}
}
