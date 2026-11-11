using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    bool tacticalMovement,reactionPending,reactionForPlayer;
    int reactionDecision;
    int reactionRange=5;
    // A straight move can leave reach after starting inside it or passing through it.
    static bool LeavesReach(Vector2Int from,Vector2Int to,Vector2Int defender,int rangeFeet)
    {
        Vector2 a=from-defender,b=to-defender,d=b-a;
        float radius=rangeFeet/5f;float radiusSquared=radius*radius;
        if(b.sqrMagnitude<=radiusSquared || d.sqrMagnitude==0)return false;
        float t=Mathf.Clamp01(-Vector2.Dot(a,d)/d.sqrMagnitude);
        return a.sqrMagnitude<=radiusSquared || (a+t*d).sqrMagnitude<radiusSquared;
    }
    void RequestPlayerMove(Vector2Int target,bool tactical)
    {
        if(!combat.PlayerCanAct || rolling || reactionPending || panel!="" || !map.CanMoveTo(target,tactical))return;
        StartCoroutine(ResolveMovement(true,target,tactical));
    }
    // Entry point for future monster movement; deployment never invokes reactions.
    public void MoveMonsterWithReactions(Vector2Int target)
    {
        if(rolling || panel!="" || page!="battle" || !map.CanMoveMonsterTo(target))return;
        StartCoroutine(ResolveMovement(false,target,false));
    }
    IEnumerator ResolveMovement(bool movingPlayer,Vector2Int target,bool tactical,bool preserveBusy=false)
    {
        Vector2Int from=movingPlayer?map.Player:map.Monster;
        if(from==target)yield break;
        combat.MarkBattleStarted();rolling=true;
        bool defenderPlayer=!movingPlayer;
        bool reactionAvailable=defenderPlayer?combat.PlayerReactionAvailable:combat.MonsterReactionAvailable;
        int reach=defenderPlayer?5:combat.MonsterAttackRange;
        if(!tactical && reactionAvailable && LeavesReach(from,target,movingPlayer?map.Monster:map.Player,reach)) {
            reactionForPlayer=defenderPlayer;reactionRange=reach;reactionDecision=0;reactionPending=true;
            dice="移动将离开 "+reach+" 尺范围，等待借机攻击选择。";
            while(reactionDecision==0)yield return null;
            reactionPending=false;
            if(reactionDecision==1 && combat.TryUseReaction(defenderPlayer)) {
                history.Insert(0,(defenderPlayer?"玩家":"木桩")+"消耗反应，在目标离开 "+reach+" 尺范围前借机攻击一次。");
                if(defenderPlayer) {
                    var weapon=selectedWeapon;bool advantage=fighter.ConsumeAdvantage(classes[0]);
                    yield return Roll(weapon,weapon.AttackModifier(attributes),classes.ProficiencyBonus,weapon.UsesDexterity(attributes)?"敏捷修正":"力量修正",advantage,"玩家借机攻击",true);
                } else yield return Counterattack(false);
            }else history.Insert(0,(defenderPlayer?"玩家":"木桩")+"放弃借机攻击，保留反应。");
            rolling=true;
            if(page!="battle" || !map.Locked || combat.BattleFinished || (movingPlayer&&!combat.PlayerCanAct) || (!movingPlayer&&combat.Hp==0)){rolling=false;yield break;}
        }
        if(!movingPlayer)yield return AnimateMonsterMovement(from,target);
        bool moved=movingPlayer?map.MovePlayer(target,tactical):map.MoveMonster(target);
        if(moved) {
            string mode=tactical?"战术转进（不引发借机攻击）":"普通移动";
            movementMessage=mode+"完成"+(movingPlayer?"，剩余 "+map.MovementBudget(tactical)+" 尺。":"。");
            history.Insert(0,(movingPlayer?"玩家":"木桩")+mode+" "+BattleMap.FeetBetween(from,target)+" 尺至("+(target.x+1)+","+(target.y+1)+")。");
        }
        if(!preserveBusy)rolling=false;logScroll=Vector2.zero;
    }
    void DrawReactionPrompt()
    {
        if(!reactionPending)return;
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.62f));
        Surface(new Rect(465,275,670,320),paperTexture);
        GUI.Label(new Rect(495,300,610,48),"借机攻击 · "+(reactionForPlayer?"玩家":"木桩"),heading);
        GUI.Label(new Rect(495,365,610,100),"目标即将离开 "+reactionRange+" 尺范围。是否消耗 1 次反应进行一次攻击？\n放弃会保留反应，并继续本次移动。",label);
        if(Btn(new Rect(495,500,285,55),"使用反应攻击"))reactionDecision=1;
        if(Btn(new Rect(820,500,285,55),"放弃借机攻击"))reactionDecision=2;
    }
}
