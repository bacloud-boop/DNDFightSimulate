using UnityEngine;
public sealed partial class BattleMap
{
    public bool CanJump(bool player,Vector2Int cell)=>Locked&&!IsContained(player)&&cell!=(player?Player:Monster)&&CanPlace(player,cell)&&RecoverySafe(player?cell:Player,player?Monster:cell)&&FeetBetween(player?Player:Monster,cell)<=30&&(player?MovementRemaining:MonsterMovementRemaining)>=10;
    public bool Jump(bool player,Vector2Int cell){if(!CanJump(player,cell))return false;if(player){SpendPlayerMovement(10);Player=cell;}else{SpendMonsterMovement(10);Monster=cell;}SyncContainedPosition();return true;}
}
