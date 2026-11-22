using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    bool monsterMoving;
    Vector2 monsterVisualCell;
    IEnumerator ChasePlayer(int? reachOverride=null)
    {
        int reach=reachOverride??MonsterTurnReach;
        if(!combat.MonsterCanMove)yield break;
        if(monsterGroundWeapon.Dropped&&!MonsterPolymorphed&&combat.MonsterCanAct){yield return RecoverMonsterWeapon();if(monsterGroundWeapon.Dropped)yield break;}
        if(combat.MonsterProne){int stand=combat.MonsterSpeed/2;if(combat.MonsterSpeed<=0||map.MonsterMovementRemaining<stand)yield break;map.SpendMonsterMovement(stand);combat.MonsterProne=false;SyncTimedBuffs();}
        if(!map.HasSharedPosition(-1)&&map.NearestEnemyInReach(!combat.PlayerDead,reach)>=0)yield break;
        Vector2Int from=map.Monster,best=from;int initialSpaces=map.MonsterSpaces;
        int bestDistance,bestCost=0;map.NearestEnemyFrom(from,map.MonsterSpaces,!combat.PlayerDead,int.MaxValue,out bestDistance);
        bool foundInRange=false,bestJump=false;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++) {
            var cell=new Vector2Int(x,y);
            bool walking=map.CanMoveMonsterTo(cell),leaping=CanUseJump(false,cell);
            if(!walking&&!leaping)continue;
            bool useJump=leaping&&(!walking||map.MonsterMovementCost(cell)>10);
            int cost=useJump?10:map.MonsterMovementCost(cell);
            if(cost>map.MonsterMovementRemaining)continue;
            int distance;map.NearestEnemyFrom(cell,map.MonsterSpaces,!combat.PlayerDead,int.MaxValue,out distance);
            bool inRange=distance<=reach;
            if(inRange && (!foundInRange || cost<bestCost || (cost==bestCost && distance<bestDistance)) ||
                !inRange && !foundInRange && (distance<bestDistance || (distance==bestDistance && cost<bestCost))) {
                best=cell;bestDistance=distance;bestCost=cost;foundInRange=inRange;bestJump=useJump;
            }
        }
        if(best!=from) {
            dice="木桩向最近敌人移动 "+bestCost+" 尺";
            yield return ResolveMovement(false,best,false,true,bestJump);
            if((bestJump||map.Monster!=best||map.MonsterSpaces!=initialSpaces)&&(map.Monster!=from||map.MonsterSpaces!=initialSpaces)&&(!combat.MonsterProne||combat.MonsterSpeed>0&&map.MonsterMovementRemaining>=combat.MonsterSpeed/2)&&!combat.BattleFinished&&combat.MonsterCanMove)yield return ChasePlayer(reach);
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
