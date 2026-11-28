using UnityEngine;
public sealed partial class BattleMap
{
    public int JumpMovementCost(bool player,Vector2Int cell){var from=player?Player:Monster;int n=player?PlayerSpaces:MonsterSpaces;if(cell.x<0||cell.y<0||cell.x>Size-n||cell.y>Size-n||FeetBetween(from,cell)>30)return int.MaxValue;return 10+WindMovementCost(player?0:-1,from,cell,n,10);}
    public bool CanJump(bool player,Vector2Int cell,bool tactical=false)=>Locked&&VoluntaryMovementAllowed(player?0:-1,cell)&&(!player||!Riding)&&!ActorRestrained(player)&&!IsContained(player)&&cell!=(player?Player:Monster)&&CanPlace(player,cell)&&RecoverySafe(player?cell:Player,player?Monster:cell)&&FeetBetween(player?Player:Monster,cell)<=30&&(player?MovementBudget(tactical):MonsterMovementRemaining)>=JumpMovementCost(player,cell);
    public bool Jump(bool player,Vector2Int cell,bool tactical=false){if(!CanJump(player,cell,tactical))return false;var from=player?Player:Monster;int spaces=player?PlayerSpaces:MonsterSpaces,cost=JumpMovementCost(player,cell);if(player){if(tactical)TacticalMovementRemaining-=cost;else SpendPlayerMovement(cost);Player=cell;}else{SpendMonsterMovement(cost);Monster=cell;}SyncContainedPosition();ApplyWaterWalkFall(player?0:-1,from,cell,spaces,true);MovementObserved?.Invoke(player?0:-1,from,cell,spaces,spaces,true);return true;}
}
