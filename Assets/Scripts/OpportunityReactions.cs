using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    bool tacticalMovement,reactionPending,reactionForPlayer;
    int reactionDecision;
    int reactionRange=5;
    void RequestPlayerMove(Vector2Int target,bool tactical)
    {
        if(combat.Aftermath){if(!rolling&&PendingTerrainCount==0&&panel==""&&PlayerTurnAllowed&&combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&!combat.PlayerFeignDeath&&map.CanMoveTo(target,tactical))StartCoroutine(ResolveMovement(true,target,tactical));return;}
        if(!combat.PlayerCanAct || rolling || reactionPending || panel!="" || !map.CanMoveTo(target,tactical))return;
        StartCoroutine(ResolveMovement(true,target,tactical));
    }
    // Entry point for future monster movement; deployment never invokes reactions.
    public void MoveMonsterWithReactions(Vector2Int target)
    {
        if(rolling || panel!="" || page!="battle" || !map.CanMoveMonsterTo(target))return;
        StartCoroutine(ResolveMovement(false,target,false));
    }
    IEnumerator ResolveMovement(bool movingPlayer,Vector2Int target,bool tactical,bool preserveBusy=false,bool jumping=false,bool greaseStep=false,int? prepaid=null,int? expectedSpaces=null,int? travelDistance=null)
    {
        if(!greaseStep&&!jumping&&GreasePathNeeded(movingPlayer,target)){yield return MoveThroughGrease(movingPlayer,target,tactical,preserveBusy);yield break;}
        Vector2Int from=movingPlayer?map.Player:map.Monster;
        if(from==target||jumping&&!CanUseJump(movingPlayer,target,tactical))yield break;
        bool proneAtStart=movingPlayer?combat.PlayerProne:combat.MonsterProne;combat.MarkBattleStarted();rolling=true;
        if(!movingPlayer){yield return ResolveMonsterMoveSummonReactions(from,target);if(combat.BattleFinished||combat.Hp==0){rolling=false;yield break;}}
        bool defenderPlayer=!movingPlayer;
        bool reactionAvailable=!combat.Aftermath&&(defenderPlayer?PlayerWeaponReactionReady&&!combat.PlayerReactionsBlocked&&combat.PlayerReactionAvailable&&combat.PlayerCanAct&&!combat.PlayerOpportunityBlocked:MonsterCanCastAttack&&!combat.MonsterReactionsBlocked&&combat.MonsterReactionAvailable&&combat.MonsterCanAct&&!combat.MonsterOpportunityBlocked);
        int reach=defenderPlayer?5:MonsterAttackReach;
        int opportunityPlayerSight,opportunityMonsterSight;
        if(movingPlayer&&OpportunitySight(movingPlayer,from,target,reach,out opportunityPlayerSight,out opportunityMonsterSight) && !tactical && !(movingPlayer&&PlayerInBeastForm&&BeastAttack.Find(PlayerBeast.name).Agile) && reactionAvailable && BattleMap.LeavesReach(from,target,movingPlayer?map.Monster:map.Player,reach,movingPlayer?map.PlayerSpaces:map.MonsterSpaces,movingPlayer?map.MonsterSpaces:map.PlayerSpaces)) {
            reactionForPlayer=defenderPlayer;reactionRange=reach;reactionDecision=0;reactionPending=true;
            dice="移动将离开 "+reach+" 尺范围，等待借机攻击选择。";
            while(reactionDecision==0)yield return null;
            reactionPending=false;
            if(reactionDecision==1 && combat.TryUseReaction(defenderPlayer)) {
                history.Insert(0,(defenderPlayer?"玩家":"木桩")+"消耗反应，在目标离开 "+reach+" 尺范围前借机攻击一次。");
                // At the reach boundary, the nearest occupied-cell distance equals the trigger reach (up to rounding).
                opportunitySenseDistance=reach;playerSightOverride=opportunityPlayerSight;monsterSightOverride=opportunityMonsterSight;
                SnapshotOpportunityCover(movingPlayer,from,target,reach);
                try {
                if(defenderPlayer) {
                    if(PlayerInBeastForm)yield return RollNaturalAttack(true);
                    else {var weapon=selectedWeapon;bool advantage=fighter.ConsumeAdvantage(classes[0]);
                    yield return Roll(weapon,weapon.AttackModifier(attributes),classes.ProficiencyBonus,weapon.UsesDexterity(attributes)?"敏捷修正":"力量修正",advantage,"玩家借机攻击",true);}
                } else yield return Counterattack(false);
                } finally {opportunitySenseDistance=null;playerSightOverride=monsterSightOverride=null;playerAttackCoverOverride=monsterAttackCoverOverride=null;}
            }else history.Insert(0,(defenderPlayer?"玩家":"木桩")+"放弃借机攻击，保留反应。");
            rolling=true;
            if(page!="battle" || !map.Locked || combat.BattleFinished || (movingPlayer&&!combat.PlayerCanAct) || (!movingPlayer&&combat.Hp==0)){rolling=false;yield break;}
        }
        if(expectedSpaces.HasValue&&((movingPlayer?map.PlayerSpaces:map.MonsterSpaces)!=expectedSpaces.Value||(movingPlayer?combat.PlayerProne:combat.MonsterProne)!=proneAtStart)){if(PendingTerrainCount>0)yield return DrainGreaseEntries();if(!preserveBusy)rolling=false;yield break;}
        if(!movingPlayer)yield return AnimateMonsterMovement(from,target);
        int paidMovement=prepaid.HasValue?prepaid.Value:jumping?10:movingPlayer?map.PlayerMovementCost(target):map.MonsterMovementCost(target);
        System.Func<bool> groundMove=()=>prepaid.HasValue?map.WithMovementCost(target,prepaid.Value,()=>movingPlayer?map.MovePlayer(target,tactical):map.MoveMonster(target),travelDistance):movingPlayer?map.MovePlayer(target,tactical):map.MoveMonster(target);
        bool moved=jumping?CanUseJump(movingPlayer,target,tactical)&&map.Jump(movingPlayer,target,tactical):greaseStep?map.WithCreatureTransitStep(movingPlayer?0:-1,groundMove):groundMove();
        if(moved) {
            if(jumping)jumpEffect.Use(movingPlayer,worldRounds,combat.Round);
            string mode=jumping?(tactical?"战术跳跃（免借机）":"跳跃"):tactical?"战术转进（不引发借机攻击）":"普通移动";
            movementMessage=mode+"完成"+(movingPlayer?"，剩余 "+map.MovementBudget(tactical)+" 尺。":"。");
            history.Insert(0,(movingPlayer?"玩家":"木桩")+mode+" "+BattleMap.FeetBetween(from,target)+" 尺（消耗 "+paidMovement+" 尺移动力）至("+(target.x+1)+","+(target.y+1)+")。");
        }
        if(PendingTerrainCount>0)yield return DrainGreaseEntries();
        if(!preserveBusy)rolling=false;logScroll=Vector2.zero;
    }
    void DrawReactionPrompt()
    {
        if(!reactionPending)return;
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.62f));
        Surface(new Rect(465,275,670,320),paperTexture);
        GUI.Label(new Rect(495,300,610,48),"借机攻击 · "+(reactionActorLabel!=""?reactionActorLabel:reactionForPlayer?"玩家":"木桩"),heading);
        GUI.Label(new Rect(495,365,610,100),"目标即将离开 "+reactionRange+" 尺范围。是否消耗 1 次反应进行一次攻击？\n放弃会保留反应，并继续本次移动。",label);
        if(Btn(new Rect(495,500,285,55),"使用反应攻击"))reactionDecision=1;
        if(Btn(new Rect(820,500,285,55),"放弃借机攻击"))reactionDecision=2;
    }
}
