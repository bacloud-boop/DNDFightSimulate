using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    bool summonActing,summonEndRequested;
    int summonTarget=-1;
    SummonedUnit FindSummon(int id){foreach(var unit in map.Summons)if(unit.Id==id&&unit.Alive)return unit;return null;}
    bool SummonTarget(int id,out Vector2Int cell,out int spaces,out string name)
    {
        cell=Vector2Int.zero;spaces=1;name="";
        if(id==0&&!combat.PlayerDead&&!map.IsContained(true)&&!map.IsCarried(true)){cell=map.Player;spaces=map.PlayerSpaces;name="玩家";return true;}
        if(id==-1&&combat.Hp>0&&!map.IsContained(false)&&!map.IsCarried(false)){cell=map.Monster;spaces=map.MonsterSpaces;name="木桩";return true;}
        var unit=FindSummon(id);if(unit==null)return false;cell=new Vector2Int(unit.X,unit.Y);spaces=unit.Spaces;name=unit.Name;return true;
    }
    bool SpiritSees(SummonedUnit unit,int id,Vector2Int target,int targetSpaces)
        =>SpiritSeesAt(unit,id,unit.X,unit.Y,target.x,target.y,targetSpaces);
    bool SpiritSeesAt(SummonedUnit unit,int id,double fromX,double fromY,double targetX,double targetY,int targetSpaces)
    {
        if(ActorBlinded(unit.Id)||unit.SleepStage==2)return false;
        bool invisible=(id==0||id==-1)&&InvisibleWithFaerie(id==0);
        if(invisible)return false;
        for(int dy=0;dy<unit.Spaces;dy++)for(int dx=0;dx<unit.Spaces;dx++)
        for(int y=0;y<targetSpaces;y++)for(int x=0;x<targetSpaces;x++)
            if(vision.Quality((fromX+dx)*5,(fromY+dy)*5,(targetX+x)*5,(targetY+y)*5,IsSteed(unit)?0:BestialSpiritRules.Darkvision,worldRounds,0,false)<2)return true;
        return false;
    }
    bool TargetSeesSpirit(int target,SummonedUnit attacker)
    {
        if(target==0||target==-1){var origin=target==0?map.Player:map.Monster;return VisibilityBetween(target==0,origin.x,origin.y,target==0?map.PlayerSpaces:map.MonsterSpaces,attacker.X,attacker.Y,attacker.Spaces)<2;}
        var unit=FindSummon(target);return unit!=null&&SpiritSees(unit,attacker.Id,new Vector2Int(attacker.X,attacker.Y),attacker.Spaces);
    }
    bool SpiritPackTactics(SummonedUnit attacker,int targetId,Vector2Int target,int spaces,CreatureCover.Body? targetAt=null)
    {
        var body=targetAt??new CreatureCover.Body(target.x,target.y,spaces);
        if(targetId!=0&&combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&!combat.PlayerFeignDeath&&SummonGeometry.Distance(new CreatureCover.Body(map.Player.x,map.Player.y,map.PlayerSpaces),body)<=5)return true;
        foreach(var ally in map.Summons)if(ally.Id!=attacker.Id&&ally.Id!=targetId&&ally.Alive&&!ally.FeignDeath&&ally.SleepStage==0&&SummonGeometry.Distance(new CreatureCover.Body(ally.X,ally.Y,ally.Spaces),body)<=5)return true;
        return false;
    }
    IEnumerator ResolveSpiritAttack(SummonedUnit unit,int targetId,bool opportunity=false,CreatureCover.Body? targetAt=null)
    {
        Vector2Int cell;int spaces;string target;
        if(page!="battle"||combat.Aftermath||combat.BattleFinished||unit==null||!unit.Alive||(unit!=bestialSpirit.Unit&&unit!=steed)||(IsSteed(unit)&&ControlledSteed)||targetId==unit.Id||!SummonTarget(targetId,out cell,out spaces,out target))yield break;
        if(!opportunity&&BattleMap.BodyDistance(new Vector2Int(unit.X,unit.Y),unit.Spaces,cell,spaces)>5)yield break;
        if(!opportunity&&(!unit.TurnActive||unit.Orders.Kind!=SummonCommandKind.AssistCombat||!unit.Attack()))yield break;
        bool previousBusy=summonActing;summonActing=true;
        try{
            bool sees=targetAt.HasValue?SpiritSeesAt(unit,targetId,unit.X,unit.Y,targetAt.Value.X,targetAt.Value.Y,spaces):SpiritSees(unit,targetId,cell,spaces),targetSees=TargetSeesSpirit(targetId,unit);
            if(targetAt.HasValue&&targetId==-1)targetSees=VisibilityBetween(false,targetAt.Value.X,targetAt.Value.Y,spaces,unit.X,unit.Y,unit.Spaces)<2;
            bool advantage=FaerieAdvantage(targetId,sees)||(FindSummon(targetId)!=null&&FindSummon(targetId).Prone)||targetId==0&&combat.PlayerProne||targetId==-1&&combat.MonsterProne||(targetId==0||targetId==-1?Entangled(targetId==0):FindSummon(targetId)!=null&&FindSummon(targetId).Restrained)||(!IsSteed(unit)&&SpiritPackTactics(unit,targetId,cell,spaces,targetAt))||!targetSees||(targetId==0&&(combat.PlayerHp==0||combat.PlayerParalyzed))||(targetId==-1&&combat.MonsterHelpless);
            bool disadvantage=FearDisadvantage(unit.Id)||unit.Prone||unit.Restrained||!sees||UnderwaterCombatRules.Disadvantage(unit.Aquatic.Submerged,true,true,unit.SwimSpeed,false);
            if(targetId==0)disadvantage|=senses.Blurred(worldRounds);
            var other=FindSummon(targetId);if(other!=null&&other.Dodging&&other.Speed>other.Breathing.SpeedPenalty&&targetSees)disadvantage=true;
            int first=Random.Range(1,21),second=advantage!=disadvantage?Random.Range(1,21):0;
            int value=second==0?first:advantage?Mathf.Max(first,second):Mathf.Min(first,second);
            int bonus=SummonAttackBonus(unit)+unit.Breathing.D20Penalty;
            int cover=targetAt.HasValue?map.CreatureCoverAt(unit.Id,targetId,new CreatureCover.Body(unit.X,unit.Y,unit.Spaces),targetAt.Value):map.CreatureCoverBonus(unit.Id,targetId),armor=(targetId==0?combat.PlayerArmorClass:targetId==-1?combat.MonsterArmorClass:other.Armor)+cover;
            mirrorIntercepted=false;
            if(targetId==0&&value!=1&&(value==20||(long)value+bonus>=armor)){yield return ResolvePlayerHitDefense(value,(long)value+bonus,sees,false,cover);armor=combat.PlayerArmorClass+cover;}
            yield return PresentRoll(20,second==0?new[]{first}:new[]{first,second},unit.Name+" → "+target+"：采用 "+value+" + "+bonus+" / 护甲 "+armor+(cover>0?"（半掩护+2）":""));
            bool hit=value!=1&&(value==20||(long)value+bonus>=armor);
            if(!hit||mirrorIntercepted){history.Insert(0,unit.Name+"攻击"+target+"："+(mirrorIntercepted?"镜像拦截":"未命中")+"，伤害0。");yield break;}
            bool critical=value==20||(other!=null&&other.SleepStage==2)||(targetId==0&&(combat.PlayerHp==0||combat.PlayerParalyzed))||(targetId==-1&&combat.MonsterHelpless);
            int[] rolls=new int[critical?2:1];int damage=SummonDamageBonus(unit),type=SummonDamageType(unit);string damageName=DamageTypes.Names[type];
            for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,9);damage+=rolls[i];}
            yield return PresentRoll(8,rolls,(IsSteed(unit)?"异界猛击":"撕裂")+(critical?"暴击":"")+"：骰值 + "+SummonDamageBonus(unit)+" = "+damage+" "+damageName);
            if(targetId==0){int frost=AgathysRetaliation(true);damage=combat.AdjustTypedDamage(true,damage,type);combat.ApplyPlayerDamage(damage,critical,sourcePlayer:null);unit.Damage(frost,4);if(frost>0)BreakOrdinaryInvisibility(true,"造成伤害");yield return MaintainConcentration(damage);}
            else if(targetId==-1){damage=combat.AdjustTypedDamage(false,damage,type);combat.ApplyDamage(damage,sourcePlayer:null);}
            else other.Damage(damage,type);
            history.Insert(0,unit.Name+"对"+target+"造成 "+damage+" "+damageName+"伤害。");RefreshBestialSpirit();CheckBattleOutcome();
        }finally{summonActing=previousBusy;}
    }
    IEnumerator CompletePlayerTurnWithSummons()
    {
        rolling=true;
        try{
            EndSteedFearsAtPlayerTurnEnd();
            if(ControlledSteed&&steed.TurnActive){
                yield return RepeatSummonSleep(steed);if(steed!=null&&steed.Alive)yield return ResolveGreaseSave(steed.Id,ActorGrease(steed.Id),"坐骑回合末油脂");
                if(steed!=null&&steed.Alive){ObserveWaterBreathing();if(steed.Breathing.EndTurn(combat.Round,worldRounds+1)&&steed.Breathing.Fatal)steed.Dismiss();}
            }
            int continuing=steed!=null&&steed.TurnActive&&!ControlledSteed?steed.Id:0;
            var followers=new System.Collections.Generic.List<SummonedUnit>();
            foreach(var follower in map.Summons){if(follower==steed&&ControlledSteed){follower.EndTurn();continue;}followers.Add(follower);}
            web.ExpireMissingSummonOwners(id=>FindSummon(id)!=null);SyncTimedBuffs();summonTurns.Begin(followers,combat.Round,combat.Aftermath,continuing);
            while(summonTurns.Active&&!combat.Aftermath&&!combat.BattleFinished){
                var unit=summonTurns.Current;if(PendingTerrainCount>0)yield return DrainGreaseEntries();summonEndRequested=false;
                if(unit.FeignDeath||unit.SleepStage==2)summonEndRequested=true;
                else if(unit.Orders.Kind!=SummonCommandKind.AssistCombat){unit.Dodge();history.Insert(0,unit.Name+"本回合闪避并尝试避险。");Vector2Int retreat;if(map.ChooseSummonRetreat(unit.Id,out retreat))yield return MoveSpiritWithReactions(unit,retreat);summonEndRequested=true;}
                else {panel="summons";while(!summonEndRequested&&unit.Alive&&!combat.Aftermath&&!combat.BattleFinished)yield return null;}
                if(unit.Alive)yield return RepeatSummonSleep(unit);
                if(unit.Alive)yield return ResolveGreaseSave(unit.Id,ActorGrease(unit.Id),"召唤物回合末油脂");
                if(unit.Alive){ObserveWaterBreathing();if(unit.Breathing.EndTurn(combat.Round,worldRounds+1)&&unit.Breathing.Fatal)unit.Dismiss();}
                if(summonTurns.Current==unit)summonTurns.EndCurrent();else summonTurns.Reconcile();
            }
        }finally{summonTurns.Cancel();panel="";summonActing=false;}
        RefreshBestialSpirit();
        if(!combat.Aftermath&&!combat.BattleFinished)yield return Counterattack();else rolling=false;
    }
}
