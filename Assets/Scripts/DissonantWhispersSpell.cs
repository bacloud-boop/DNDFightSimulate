using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    int whispersTarget=-1;
    bool WhispersTargetAllowed(int caster,int target)
    {
        Vector2Int from,to;int n,m;string name;
        if(!CommandBody(caster,out from,out n)||!HealingHandsTarget(target,out to,out m,out name)||BattleMap.BodyDistance(from,n,to,m)>60)return false;
        if((caster==0||caster==-1)&&map.IsContained(caster==0)&&target!=caster&&target!=(caster==0?-1:0))return false;
        if((target==0||target==-1)&&map.IsContained(target==0)&&caster!=target&&caster!=(target==0?-1:0))return false;
        return ConjuredTargetVisible(caster==0,target);
    }
    string DissonantWhispersError()=>WhispersTargetAllowed(0,whispersTarget)?"":"请选择60尺内仍存活且可见的生物。";
    void DrawDissonantWhispersChoices()
    {
        foreach(int actor in BreathActors())if(GUILayout.Button((whispersTarget==actor?"✓ ":"")+TransitActorName(actor)+(WhispersTargetAllowed(0,actor)?"":"（当前不可选）"),button))whispersTarget=actor;
        GUILayout.Label("感知豁免失败受到"+DissonantWhispersRules.DiceCount(directSlotLevel)+"d6心灵，并立即消耗可用反应撤离；成功半伤且不移动。安全路线中尽量远离，没有安全路线则停留。借机照常，不消耗普通／战术移动、动作或附赠动作，不会疾走或传送。",small);
    }
    bool WhispersSafeCell(int actor,Vector2Int p,int n)
    {
        if(grease.Current(p.x,p.y,n,worldRounds)!=null||spikeGrowth.Intersects(p.x,p.y,n,worldRounds)||cloudDaggers.Intersects(p.x,p.y,n,worldRounds)||web.Intersects(p.x,p.y,n,worldRounds)||entangle.Intersects(p.x,p.y,n,worldRounds))return false;
        if(flamingSphere.Active(worldRounds)&&BattleMap.BodyDistance(p,n,SphereCell,1)<=5)return false;
        int water=map.WaterCells(p,n);var unit=actor>0?FindSummon(actor):null;
        bool underwater=(actor==0?map.PlayerAquatic.Submerged:actor==-1?map.MonsterAquatic.Submerged:actor==BattleMap.RescueId?map.RescueAquatic.Submerged:unit!=null&&unit.Aquatic.Submerged)&&water==n*n;
        BeastRecord form=actor==0?(PlayerInBeastForm?PlayerBeast:null):actor==-1&&MonsterPolymorphed?PolymorphBeast:null;
        bool amphibious=form!=null&&form.actionNames!=null&&Array.IndexOf(form.actionNames,"Amphibious")>=0;
        bool onlyWater=form!=null&&form.actionNames!=null&&Array.IndexOf(form.actionNames,"Water Breathing")>=0;
        bool air=unit!=null?unit.BreathesAir:!onlyWater||amphibious;
        bool wet=unit!=null?unit.BreathesWater||worldRounds<unit.WaterBreathingExpires:actor==BattleMap.RescueId?false:onlyWater||amphibious||WaterBreathingActive(actor==0);
        return underwater?wet:water>0?air||wet:air;
    }
    int WhispersSpeed(int actor)=>actor==0?map.PlayerSpeed:actor==-1?combat.MonsterSpeed:actor==BattleMap.RescueId?rescueCarrier.EffectiveSpeed:FindSummon(actor)?.EffectiveSpeed??0;
    List<int> WhispersRoute(int actor,int source,int budget)
    {
        Vector2Int from,origin;int n,m;
        if(actor==source||!CommandBody(actor,out from,out n)||!CommandBody(source,out origin,out m))return new List<int>();
        int width=map.Size;
        return DissonantWhispersRules.SafeRoute(width,from.y*width+from.x,budget,
            (a,b)=>map.CommandStepCost(actor,new Vector2Int(a%width,a/width),new Vector2Int(b%width,b/width)),
            c=>BattleMap.BodyDistance(new Vector2Int(c%width,c/width),n,origin,m),
            c=>WhispersSafeCell(actor,new Vector2Int(c%width,c/width),n),c=>map.CommandCanEnd(actor,new Vector2Int(c%width,c/width)));
    }
    bool WhispersUseReaction(int actor)=>actor==0||actor==-1?combat.TryUseCompelledReaction(actor==0):actor==BattleMap.RescueId?rescueCarrier.TryUseReaction():FindSummon(actor)?.UseReaction()??false;
    bool WhispersDismount(int actor,int source,ReactionMovementBudget budget)
    {
        if(actor!=0||!map.Riding)return true;
        int speed=WhispersSpeed(actor),cost=speed/2;if(speed<=0||cost>budget.Remaining(speed))return false;
        Vector2Int origin;int sourceSpaces;if(!CommandBody(source,out origin,out sourceSpaces))return false;
        Vector2Int best=map.Player;int far=-1;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){
            var p=new Vector2Int(x,y);if(!map.CanReactionDismountAt(p)||!WhispersSafeCell(actor,p,map.RiderBodySpaces))continue;
            int distance=map.PreviewReactionDismount(p,()=>{var route=WhispersRoute(actor,source,budget.Remaining(speed)-cost);var last=route.Count==0?p:new Vector2Int(route[route.Count-1]%map.Size,route[route.Count-1]/map.Size);return BattleMap.BodyDistance(last,map.RiderBodySpaces,origin,sourceSpaces);});
            if(distance>far){far=distance;best=p;}
        }
        if(far<0||!map.ReactionDismountAt(best))return false;
        if(cost>0)budget.Spend(cost,speed);history.Insert(0,"不谐低语：骑手使用自身反应移动额度的一半下马，未驱使坐骑移动。");return true;
    }
    IEnumerator RetreatFromWhispers(int source,int actor)
    {
        int previousActor=commandActor;bool busy=rolling;commandActor=actor;rolling=true;
        try{
            if(!WhispersUseReaction(actor)){history.Insert(0,"不谐低语：目标无可用反应或已经失能，只结算伤害。");yield break;}
            history.Insert(0,"不谐低语："+TransitActorName(actor)+"消耗反应，沿安全路线远离施法者。");
            var budget=new ReactionMovementBudget();if(!CommandCanMove(actor)||actor==source||!WhispersDismount(actor,source,budget))yield break;
            if(PendingTerrainCount>0)yield return DrainGreaseEntries();
            var route=WhispersRoute(actor,source,budget.Remaining(WhispersSpeed(actor)));
            if(route.Count==0){history.Insert(0,"不谐低语：没有能更远离施法者的安全路线，留在原处。");yield break;}
            foreach(int step in route){
                Vector2Int from;int n;if(!CommandCanMove(actor)||!CommandBody(actor,out from,out n))break;
                var cell=new Vector2Int(step%map.Size,step/map.Size);int cost=map.CommandStepCost(actor,from,cell);
                if(!WhispersSafeCell(actor,cell,n)||cost==int.MaxValue||cost<1||cost>budget.Remaining(WhispersSpeed(actor)))break;
                bool prone=ActorProne(actor);
                Func<bool> guard=()=>CommandCanMove(actor)&&WhispersSafeCell(actor,cell,n)&&cost<=budget.Remaining(WhispersSpeed(actor));
                if(actor==0||actor==-1)yield return ResolveMovement(actor==0,cell,false,true,false,true,cost,n,reactionMove:true,movementGuard:guard);
                else if(actor==BattleMap.RescueId){map.MoveReactionStep(actor,cell);if(PendingTerrainCount>0)yield return DrainGreaseEntries();}
                else yield return MoveSpiritWithReactions(FindSummon(actor),cell,true,cost,reactionMove:true,movementGuard:guard);
                Vector2Int actual;int spaces;if(!CommandBody(actor,out actual,out spaces)||actual!=cell)break;
                budget.Spend(cost,int.MaxValue);if(spaces!=n||!prone&&ActorProne(actor))break;
            }
            history.Insert(0,"不谐低语：反应移动结束，消耗独立额度 "+budget.Spent+"尺；本回合普通移动与行动资源不变。");
        }finally{ObserveForcedSharedStop(actor);commandActor=previousActor;rolling=busy;}
    }
    IEnumerator ResolveDissonantWhispers(int source,int caster,int slot,int target)
    {
        int count=DissonantWhispersRules.DiceCount(slot);if(count==0||!WhispersTargetAllowed(source,target))yield break;
        var failed=new HashSet<int>();long dc=source==0?8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]):MonsterSpellDc;
        outcomeDeferDepth++;try{
            yield return RollMappedAbilitySave(target,source,dc,failed,"不谐低语",3);
            int sum=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=UnityEngine.Random.Range(1,7);sum+=rolls[i];}
            dice="不谐低语 · "+count+"d6心灵";yield return PresentRoll(6,rolls,"失败 "+sum+"，成功 "+sum/2);
            var reduced=new RayDamage();yield return ReduceRayDamage(source==0,sum,"心灵","不谐低语",reduced);
            var packet=new long[13];packet[10]=DissonantWhispersRules.AfterSave(reduced.Amount,!failed.Contains(target));
            int amount=ApplyMappedDamagePacket(target,source,packet);history.Insert(0,"不谐低语："+TransitActorName(target)+(failed.Contains(target)?"感知豁免失败":"感知豁免成功，伤害减半，不撤离")+"，结算 "+amount+" 心灵。");
            yield return MaintainConcentration(target==0?amount:0);RefreshUndeadFollowers();RefreshBestialSpirit();ReconcileFallenRescue();
            if(failed.Contains(target))yield return RetreatFromWhispers(source,target);
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
}
