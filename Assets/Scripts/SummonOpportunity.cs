using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class SummonOpportunity {public int Id;public double At;public CreatureCover.Body Target;}
    List<SummonOpportunity> opportunityChoices;
    int opportunityChoice=-1;
    string reactionActorLabel="";
    CreatureCover.Body MovingBodyAt(Vector2Int from,Vector2Int to,int spaces,double at)=>new CreatureCover.Body(from.x+(to.x-from.x)*at,from.y+(to.y-from.y)*at,spaces);
    void DrawSummonOpportunityOrder()
    {
        if(opportunityChoices==null)return;
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.7f));Surface(new Rect(450,220,700,500),paperTexture);
        GUILayout.BeginArea(new Rect(480,250,640,440));GUILayout.Label("同时触发借机攻击 · 选择先处理者",heading);
        for(int i=0;i<opportunityChoices.Count;i++){int id=opportunityChoices[i].Id;var unit=FindSummon(id);if(GUILayout.Button(id==0?"玩家":unit==null?"已消失的召唤物":unit.Name,button))opportunityChoice=i;}
        GUILayout.EndArea();
    }
    IEnumerator ResolveMonsterMoveSummonReactions(Vector2Int from,Vector2Int to)
    {
        var candidates=new List<SummonOpportunity>();int ps,ms;
        if(PlayerWeaponReactionReady&&combat.PlayerCanAct&&combat.PlayerReactionAvailable&&!combat.PlayerReactionsBlocked&&!combat.PlayerOpportunityBlocked&&BattleMap.LeavesReach(from,to,map.Player,5,map.MonsterSpaces,map.PlayerSpaces)&&OpportunitySight(false,from,to,5,out ps,out ms)){
            double at=VisionState.ReachExit(from.x,from.y,to.x,to.y,map.MonsterSpaces,map.Player.x,map.Player.y,map.PlayerSpaces,5);
            candidates.Add(new SummonOpportunity{Id=0,At=at,Target=MovingBodyAt(from,to,map.MonsterSpaces,at)});
        }
        foreach(var unit in map.Summons)if(unit.Alive&&(!IsSteed(unit)||!ControlledSteed)&&unit.ReactionAvailable&&BattleMap.LeavesReach(from,to,new Vector2Int(unit.X,unit.Y),5,map.MonsterSpaces,unit.Spaces)){
            double at=VisionState.ReachExit(from.x,from.y,to.x,to.y,map.MonsterSpaces,unit.X,unit.Y,unit.Spaces,5);
            var target=MovingBodyAt(from,to,map.MonsterSpaces,at);
            if(at>=0&&SpiritSeesAt(unit,-1,unit.X,unit.Y,target.X,target.Y,map.MonsterSpaces))candidates.Add(new SummonOpportunity{Id=unit.Id,At=at,Target=target});
        }
        candidates.Sort((a,b)=>a.At.CompareTo(b.At));
        while(candidates.Count>0&&combat.Hp>0&&!combat.BattleFinished){
            int chosen=0;var simultaneous=candidates.FindAll(x=>Math.Abs(x.At-candidates[0].At)<1e-7);
            if(simultaneous.Count>1){opportunityChoices=simultaneous;opportunityChoice=-1;while(opportunityChoice<0)yield return null;chosen=candidates.IndexOf(simultaneous[opportunityChoice]);opportunityChoices=null;}
            var candidate=candidates[chosen];candidates.RemoveAt(chosen);var unit=FindSummon(candidate.Id);
            if(candidate.Id!=0&&(unit==null||!unit.ReactionAvailable))continue;
            reactionActorLabel=candidate.Id==0?"玩家":unit.Name;reactionForPlayer=candidate.Id==0;reactionRange=5;reactionDecision=0;reactionPending=true;
            while(reactionDecision==0)yield return null;reactionPending=false;reactionActorLabel="";
            if(reactionDecision!=1)continue;
            if(candidate.Id==0){
                if(!combat.TryUseReaction(true))continue;
                OpportunitySight(false,from,to,5,out ps,out ms);playerSightOverride=ps;monsterSightOverride=ms;opportunitySenseDistance=5;SnapshotOpportunityCover(false,from,to,5);
                try{if(PlayerInBeastForm)yield return RollNaturalAttack(true);else{var weapon=selectedWeapon;yield return Roll(weapon,weapon.AttackModifier(attributes),classes.ProficiencyBonus,weapon.UsesDexterity(attributes)?"敏捷修正":"力量修正",fighter.ConsumeAdvantage(classes[0]),"玩家借机攻击",true);}}
                finally{playerSightOverride=monsterSightOverride=null;opportunitySenseDistance=null;playerAttackCoverOverride=monsterAttackCoverOverride=null;rolling=true;}
            }else if(unit.UseReaction())yield return ResolveSpiritAttack(unit,-1,true,candidate.Target);
        }
    }
    IEnumerator MoveSpiritWithReactions(SummonedUnit unit,Vector2Int target,bool greaseStep=false,int? prepaid=null,int? travelDistance=null)
    {
        if(page!="battle"||combat.Aftermath||combat.BattleFinished||unit==null||!unit.Alive)yield break;
        System.Func<bool> validate=()=>prepaid.HasValue?map.WithMovementCost(target,prepaid.Value,()=>map.CanMoveSummon(unit.Id,target)):map.CanMoveSummon(unit.Id,target);
        if(!(greaseStep?map.WithCreatureTransitStep(unit.Id,validate):validate()))yield break;
        if(!greaseStep&&(grease.Any(worldRounds)||spikeGrowth.Active(worldRounds)||map.CreaturePathNeedsSteps(unit.Id,new Vector2Int(unit.X,unit.Y),target,unit.Spaces))){yield return MoveSpiritThroughGrease(unit,target);yield break;}
        bool previous=summonActing;summonActing=true;
        try{
            var from=new Vector2Int(unit.X,unit.Y);
            if(!unit.Disengaged&&MonsterCanCastAttack&&combat.MonsterCanAct&&combat.MonsterReactionAvailable&&!combat.MonsterReactionsBlocked&&!combat.MonsterOpportunityBlocked&&BattleMap.LeavesReach(from,target,map.Monster,MonsterAttackReach,unit.Spaces,map.MonsterSpaces)){
                double at=VisionState.ReachExit(from.x,from.y,target.x,target.y,unit.Spaces,map.Monster.x,map.Monster.y,map.MonsterSpaces,MonsterAttackReach);
                var body=MovingBodyAt(from,target,unit.Spaces,at);
                if(at>=0&&VisibilityBetween(false,map.Monster.x,map.Monster.y,map.MonsterSpaces,body.X,body.Y,unit.Spaces)<2){
                    reactionActorLabel="木桩 → "+unit.Name;reactionForPlayer=false;reactionRange=MonsterAttackReach;reactionDecision=0;reactionPending=true;
                    while(reactionDecision==0)yield return null;reactionPending=false;reactionActorLabel="";
                    if(reactionDecision==1&&combat.TryUseReaction(false))yield return ResolveMonsterSpiritAttack(unit,body);
                }
            }
            if(!unit.Alive||combat.BattleFinished||combat.Aftermath)yield break;
            System.Func<bool> move=()=>prepaid.HasValue?map.WithMovementCost(target,prepaid.Value,()=>map.MoveSummon(unit.Id,target),travelDistance):map.MoveSummon(unit.Id,target);
            if(greaseStep?map.WithCreatureTransitStep(unit.Id,move):move()){history.Insert(0,unit.Name+"移动至("+(target.x+1)+","+(target.y+1)+")，继续原指令，无需开口。");ObserveWaterBreathing();}
            if(PendingTerrainCount>0)yield return DrainGreaseEntries();
        }finally{summonActing=previous;reactionActorLabel="";}
    }
}
