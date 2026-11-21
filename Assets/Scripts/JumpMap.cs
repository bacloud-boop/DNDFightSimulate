using UnityEngine;
public sealed partial class BattleMap
{
    public bool CanJump(bool player,Vector2Int cell,bool tactical=false)=>Locked&&VoluntaryMovementAllowed(player?0:-1,cell)&&(!player||!Riding)&&!ActorRestrained(player)&&!IsContained(player)&&cell!=(player?Player:Monster)&&CanPlace(player,cell)&&RecoverySafe(player?cell:Player,player?Monster:cell)&&FeetBetween(player?Player:Monster,cell)<=30&&(player?MovementBudget(tactical):MonsterMovementRemaining)>=10;
    public bool Jump(bool player,Vector2Int cell,bool tactical=false){if(!CanJump(player,cell,tactical))return false;var from=player?Player:Monster;int spaces=player?PlayerSpaces:MonsterSpaces;if(player){if(tactical)TacticalMovementRemaining-=10;else SpendPlayerMovement(10);Player=cell;}else{SpendMonsterMovement(10);Monster=cell;}SyncContainedPosition();ApplyWaterWalkFall(player?0:-1,from,cell,spaces,true);MovementObserved?.Invoke(player?0:-1,from,cell,spaces,spaces,true);return true;}
}
