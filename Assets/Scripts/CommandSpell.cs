using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly CommandEffect commandEffect=new CommandEffect();
    readonly HashSet<int> commandTargets=new HashSet<int>{-1};
    int commandChoice,monsterCommandChoice,commandActor=int.MaxValue;
    sealed class CommandResolution {public bool Consumed;}
    bool CommandMoving(int actor)=>commandActor==actor;
    bool CommandBody(int actor,out Vector2Int cell,out int spaces)
    {
        string name;if(HealingHandsTarget(actor,out cell,out spaces,out name))return true;
        // The instruction still points toward/away from a caster who has since died.
        if(actor==0){cell=map.Player;spaces=map.PlayerSpaces;return true;}
        if(actor==-1){cell=map.Monster;spaces=map.MonsterSpaces;return true;}return false;
    }
    bool CommandTargetAllowed(int caster,int target)
    {
        Vector2Int from,to;int n,m;string name;
        if(!HealingHandsTarget(target,out to,out m,out name)||!CommandBody(caster,out from,out n))return false;
        if(BattleMap.BodyDistance(from,n,to,m)>60)return false;
        if((caster==0||caster==-1)&&map.IsContained(caster==0)&&target!=caster&&target!=(caster==0?-1:0))return false;
        if((target==0||target==-1)&&map.IsContained(target==0)&&caster!=target&&caster!=(target==0?-1:0))return false;
        if(target==caster)return !ActorBlinded(caster);
        return caster==0?VisibilityAt(true,to,m)<2:MonsterCanSeeTarget(target);
    }
    void DrawCommandChoices()
    {
        commandTargets.RemoveWhere(id=>{Vector2Int p;int n;string name;return !HealingHandsTarget(id,out p,out n,out name);});
        commandChoice=GUILayout.SelectionGrid(commandChoice,CommandEffect.Names,3,button);
        GUILayout.Label("选择至多 "+CommandEffect.TargetLimit(directSlotLevel)+" 名60尺内可见生物；各自感知豁免。下个回合执行，不会立刻触发。",small);
        foreach(int id in BreathActors()){
            Vector2Int p;int n;string name;if(!HealingHandsTarget(id,out p,out n,out name))continue;
            bool chosen=GUILayout.Toggle(commandTargets.Contains(id),name+(CommandTargetAllowed(0,id)?"":"（不在合法范围）"),label);
            if(chosen)commandTargets.Add(id);else commandTargets.Remove(id);
        }
        GUILayout.Label("接近：仅普通移动，随后结束回合。逃跑：普通移动与不消耗次数的疾走，不自动施法或消耗职业／种族次数。丢弃、趴下、站住均跳过剩余回合；移动正常触发借机和地形。",small);
    }
    string CommandError(int slot)
    {
        if(commandTargets.Count<1||commandTargets.Count>CommandEffect.TargetLimit(slot))return "请选择1～"+CommandEffect.TargetLimit(slot)+"名不同目标。";
        foreach(int id in commandTargets)if(!CommandTargetAllowed(0,id))return TransitActorName(id)+"必须存活、在60尺内且可见。";
        return "";
    }
    List<int> MonsterCommandTargets(int slot)
    {
        var result=new List<int>();if(!Feigning(0)&&CommandTargetAllowed(-1,0))result.Add(0);
        foreach(var unit in map.Summons)if(unit.Alive&&!Feigning(unit.Id)&&CommandTargetAllowed(-1,unit.Id))result.Add(unit.Id);
        result.Sort((a,b)=>{Vector2Int pa,pb;int na,nb;CommandBody(a,out pa,out na);CommandBody(b,out pb,out nb);int d=BattleMap.BodyDistance(map.Monster,map.MonsterSpaces,pa,na).CompareTo(BattleMap.BodyDistance(map.Monster,map.MonsterSpaces,pb,nb));return d!=0?d:a.CompareTo(b);});
        int limit=CommandEffect.TargetLimit(slot);if(result.Count>limit)result.RemoveRange(limit,result.Count-limit);return result;
    }
    IEnumerator ResolveCommandCast(int caster,int castingClass,CommandWord word,IEnumerable<int> selected)
    {
        var targets=new List<int>(selected);var failed=new HashSet<int>();
        long dc=caster==0?8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(castingClass)]):MonsterSpellDc;
        foreach(int target in targets)if(CommandTargetAllowed(caster,target))yield return RollMappedAbilitySave(target,caster,dc,failed,"命令术 · "+CommandEffect.Names[(int)word],3);
        foreach(int target in targets){if(failed.Contains(target))commandEffect.Add(caster,target,dc,word);history.Insert(0,"命令术："+TransitActorName(target)+(failed.Contains(target)?"将在下个回合执行“"+CommandEffect.Names[(int)word]+"”。":"未受命令影响。"));}
    }
    bool CommandCanMove(int actor)
    {
        if(actor==0)return combat.PlayerMovementReady&&!Entangled(true)&&!map.IsContained(true)&&!map.IsCarried(true);
        if(actor==-1)return combat.Hp>0&&!combat.MonsterParalyzed&&(!combat.MonsterFeignDeath||MovementFreedom(-1))&&combat.MonsterSleepStage<2&&!Entangled(false)&&!map.IsContained(false)&&!map.IsCarried(false);
        if(actor==BattleMap.RescueId)return rescueCarrier.MovementReady&&!rescueCarrier.Restrained;
        var u=FindSummon(actor);return u!=null&&u.Alive&&(!u.FeignDeath||MovementFreedom(actor))&&u.SleepStage!=2&&!u.Restrained;
    }
    int CommandBudget(int actor)=>actor==0?map.MovementRemaining:actor==-1?map.MonsterMovementRemaining:actor==BattleMap.RescueId?rescueCarrier.MovementRemaining:FindSummon(actor)?.MovementRemaining??0;
    void CommandStand(int actor)
    {
        if(!ActorProne(actor))return;
        if(actor==0){int cost=map.PlayerSpeed/2;if(map.PlayerSpeed>0&&map.SpendPlayerMovement(cost))combat.PlayerProne=false;}
        else if(actor==-1){int cost=combat.MonsterSpeed/2;if(combat.MonsterSpeed>0&&map.MonsterMovementRemaining>=cost){map.SpendMonsterMovement(cost);combat.MonsterProne=false;}}
        else if(actor==BattleMap.RescueId)rescueCarrier.Stand();else FindSummon(actor)?.Stand();
        SyncTimedBuffs();
    }
    void CommandDash(int actor)
    {
        if(actor==0){if((combat.Aftermath?combat.TryUseObjectAction():combat.TryUseAction()))map.Dash();if(expeditiousRetreat&&(combat.Aftermath?combat.TryUseAftermathBonusAction():combat.TryUseBonusAction()))map.Dash();}
        else if(actor==-1){if(combat.Aftermath?combat.MonsterSleepStage==0:combat.TryUseMonsterAction())map.GrantMonsterDash(combat.MonsterSpeed);}
        else if(actor==BattleMap.RescueId)rescueCarrier.Dash();else FindSummon(actor)?.Dash();
    }
    void CommandDrop(int actor)
    {
        if(actor==BattleMap.RescueId&&rescueCarrier.Load.Loaded&&map.ForceReleaseRescueCargo()){rescueCarrier.Load.Clear();rescueDropMode=false;}
        if(actor==-1)DropSleepingMonsterWeapon();
        if(actor==0&&!PlayerInBeastForm&&!weaponGrip.Stowed){carriedWeapons.BeginBattle();if(carriedWeapons.Drop(selectedWeapon.Name,map.Player.x,map.Player.y,map.PlayerSpaces,WeaponFallsInWater(map.Player,map.PlayerSpaces),WeaponContainer(true))){weaponGrip.DropHeld();RefreshShillelagh();}}
        SyncWeaponInventory();
    }
    IEnumerator FollowCommand(int actor,CommandResolution result)
    {
        var order=commandEffect.Take(actor);if(order==null)yield break;result.Consumed=true;
        history.Insert(0,TransitActorName(actor)+"回合开始：执行命令术“"+CommandEffect.Names[(int)order.Word]+"”。");
        int previousActor=commandActor;bool busy=rolling;commandActor=actor;rolling=true;
        try{
            if(order.Word==CommandWord.Drop){CommandDrop(actor);yield break;}
            if(order.Word==CommandWord.Grovel){SetActorProne(actor);SyncTimedBuffs();RefreshSteed();if(mountFallPending){mountFallPending=false;yield return ResolveMountedFall();rolling=true;}yield break;}
            if(order.Word==CommandWord.Halt||!CommandCanMove(actor))yield break;
            Vector2Int origin,source;int spaces,sourceSpaces;if(!CommandBody(actor,out origin,out spaces)||!CommandBody(order.Caster,out source,out sourceSpaces)||actor==order.Caster)yield break;
            if(order.Word==CommandWord.Approach&&BattleMap.BodyDistance(origin,spaces,source,sourceSpaces)<=5)yield break;
            // A rider uses their own movement to dismount, never commandeers the mount.
            if(actor==0&&map.Riding){int cost=map.PlayerSpeed/2;if(map.PlayerSpeed<=0||map.MovementRemaining<cost||!map.DismountRider())yield break;map.SpendPlayerMovement(cost);if(PendingTerrainCount>0)yield return DrainGreaseEntries();}
            CommandStand(actor);if(order.Word==CommandWord.Flee)CommandDash(actor);
            if(!CommandCanMove(actor)||!CommandBody(actor,out origin,out spaces))yield break;
            int width=map.Size;
            var route=CommandRoute.Plan(width,origin.y*width+origin.x,CommandBudget(actor),order.Word==CommandWord.Flee,
                (a,b)=>map.CommandStepCost(actor,new Vector2Int(a%width,a/width),new Vector2Int(b%width,b/width)),
                c=>BattleMap.BodyDistance(new Vector2Int(c%width,c/width),spaces,source,sourceSpaces),
                c=>map.CommandCanEnd(actor,new Vector2Int(c%width,c/width)));
            foreach(int step in route){
                if(!CommandCanMove(actor))break;var cell=new Vector2Int(step%width,step/width);bool prone=ActorProne(actor);
                Vector2Int before;int beforeSpaces;if(!CommandBody(actor,out before,out beforeSpaces))break;int cost=map.CommandStepCost(actor,before,cell);if(cost<1||cost==int.MaxValue||cost>CommandBudget(actor))break;
                if(actor==0||actor==-1)yield return ResolveMovement(actor==0,cell,false,true,false,true,cost,spaces);
                else if(actor==BattleMap.RescueId){map.WithCreatureTransitStep(actor,()=>map.WithMovementCost(cell,cost,()=>map.MoveRescue(cell,rescueCarrier.Move)));if(PendingTerrainCount>0)yield return DrainGreaseEntries();}
                else yield return MoveSpiritWithReactions(FindSummon(actor),cell,true,cost);
                if(actor==-1&&!prone&&ActorProne(actor))CommandStand(actor);
                Vector2Int actual;int currentSpaces;if(!CommandBody(actor,out actual,out currentSpaces)||actual!=cell||currentSpaces!=spaces||!prone&&ActorProne(actor))break;
                if(order.Word==CommandWord.Approach&&BattleMap.BodyDistance(actual,spaces,source,sourceSpaces)<=5)break;
            }
        }finally{ObserveForcedSharedStop(actor);commandActor=previousActor;rolling=busy;history.Insert(0,TransitActorName(actor)+"命令回合结束，不进行额外攻击或施法。");}
    }
    IEnumerator FollowPlayerCommand()
    {
        RefreshSteed();
        if(ControlledSteed&&commandEffect.Next(steed.Id)!=null){var mounted=new CommandResolution();yield return FollowCommand(steed.Id,mounted);if(steed.Alive)yield return FinishSummonPassiveEffects(steed);steed.EndTurn();}
        var result=new CommandResolution();yield return FollowCommand(0,result);if(!result.Consumed)yield break;
        if(combat.Aftermath){yield return EndAftermathTurn();yield break;}
        if(combat.BattleFinished)yield break;
        fighter.CancelAttackAction();actionWeapon=null;FinishWaterBreathingTurn(true);ApplySharedTurnEnd(0);combat.ExpirePlayerTurnEffects();
        if(!combat.PlayerDead)yield return EndPlayerTurnWithSensorySaves();
    }
}
