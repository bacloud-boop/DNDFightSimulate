using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    bool monsterMoving;
    Vector2 monsterVisualCell;
    IEnumerator ChasePlayer(int? reachOverride=null)
    {
        int reach=reachOverride??MonsterTurnReach;
        if(!combat.MonsterCanAct||map.DistanceFeet<=reach)yield break;
        Vector2Int from=map.Monster,best=from;
        int bestDistance=map.DistanceFeet,bestCost=0;
        bool foundInRange=false,bestJump=false;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++) {
            var cell=new Vector2Int(x,y);
            bool walking=map.CanMoveMonsterTo(cell),leaping=CanUseJump(false,cell);if(!walking&&!leaping)continue;
            bool useJump=leaping&&(!walking||map.MonsterMovementCost(cell)>10);int cost=useJump?10:map.MonsterMovementCost(cell);
            if(cost>map.MonsterMovementRemaining)continue;
            int distance=BattleMap.BodyDistance(cell,map.MonsterSpaces,map.Player,map.PlayerSpaces);
            bool inRange=distance<=reach;
            if(inRange && (!foundInRange || cost<bestCost || (cost==bestCost && distance<bestDistance)) ||
                !inRange && !foundInRange && (distance<bestDistance || (distance==bestDistance && cost<bestCost))) {
                best=cell;bestDistance=distance;bestCost=cost;foundInRange=inRange;bestJump=useJump;
            }
        }
        if(best!=from) {
            dice="木桩向玩家移动 "+bestCost+" 尺";
            yield return ResolveMovement(false,best,false,true,bestJump);
            if(bestJump&&map.Monster!=from&&!combat.BattleFinished&&combat.MonsterCanAct)yield return ChasePlayer(reach);
        }
    }
    IEnumerator AnimateMonsterMovement(Vector2Int from,Vector2Int to)
    {
        monsterMoving=true;float elapsed=0,duration=fastAnimation?.35f:.8f;
        while(elapsed<duration) {
            elapsed+=Time.unscaledDeltaTime;
            monsterVisualCell=Vector2.Lerp(from,to,Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/duration)));
            yield return null;
        }
        monsterMoving=false;
    }
}
