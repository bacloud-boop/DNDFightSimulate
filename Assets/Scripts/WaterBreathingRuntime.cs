using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class TrainingGame
{
    readonly BreathingState playerBreathing=new BreathingState(),monsterBreathing=new BreathingState();
    int PlayerAquaticD20Penalty=>combat.PlayerRevivalPenalty+playerBreathing.D20Penalty;
    readonly HashSet<int> selectedWaterBreathingSummons=new HashSet<int>();
    readonly long[] waterBreathingExpires=new long[2];
    bool WaterBreathingActive(bool player)=>worldRounds<waterBreathingExpires[player?0:1];
    bool waterBreathingPlayer=true,waterBreathingMonster,waterBreathingMonsterWilling;
    void ApplyWaterBreathing(bool player,bool both)
    {
        if(waterBreathingPlayer)waterBreathingExpires[0]=worldRounds+14400;
        if(waterBreathingMonster)waterBreathingExpires[1]=worldRounds+14400;
        foreach(var unit in map.Summons)
            if(selectedWaterBreathingSummons.Contains(unit.Id)&&WaterBreathingSummonInRange(unit))unit.WaterBreathingExpires=worldRounds+14400;
        ObserveWaterBreathing();
        history.Insert(0,"水下呼吸生效，持续24小时，保留原呼吸方式。");
    }
    void DrawWaterBreathingChoices()
    {
        selectedWaterBreathingSummons.RemoveWhere(id=>FindSummon(id)==null);
        waterBreathingPlayer=GUILayout.Toggle(waterBreathingPlayer,"玩家自身",label);
        waterBreathingMonster=GUILayout.Toggle(waterBreathingMonster,"木桩",label);
        if(waterBreathingMonster)waterBreathingMonsterWilling=GUILayout.Toggle(waterBreathingMonsterWilling,"木桩自愿",label);
        foreach(var unit in map.Summons)if(unit.Alive){bool chosen=GUILayout.Toggle(selectedWaterBreathingSummons.Contains(unit.Id),unit.Name+"（自愿）",label);if(chosen)selectedWaterBreathingSummons.Add(unit.Id);else selectedWaterBreathingSummons.Remove(unit.Id);}
    }
    bool WaterBreathingSummonInRange(SummonedUnit unit)
        =>unit.Alive&&BattleMap.BodyDistance(map.Player,map.PlayerSpaces,new Vector2Int(unit.X,unit.Y),unit.Spaces)<=30;
    string WaterBreathingError(bool player,bool both)
    {
        int count=(waterBreathingPlayer?1:0)+(waterBreathingMonster?1:0)+selectedWaterBreathingSummons.Count;
        if(count<1||count>10)return "请选择1至10个自愿目标。";
        if(worldRounds>long.MaxValue-14400)return "世界时间超出范围。";
        if(waterBreathingMonster&&(combat.Hp<=0||map.IsContained(false)||map.DistanceFeet>30||!waterBreathingMonsterWilling))return "木桩须存活、自愿且在30尺内。";
        foreach(int id in selectedWaterBreathingSummons){var unit=FindSummon(id);if(unit==null||!WaterBreathingSummonInRange(unit))return "所选召唤物必须存活且在30尺内。";}
        return "";
    }
    bool WaterBreathingAllowed(bool player)
    {
        BeastRecord form=player?(PlayerInBeastForm?PlayerBeast:null):(MonsterPolymorphed?PolymorphBeast:null);
        bool amphibious=form!=null&&form.actionNames!=null&&Array.IndexOf(form.actionNames,"Amphibious")>=0;
        bool onlyWater=form!=null&&form.actionNames!=null&&Array.IndexOf(form.actionNames,"Water Breathing")>=0;
        var aquatic=player?map.PlayerAquatic:map.MonsterAquatic;var cell=player?map.Player:map.Monster;int spaces=player?map.PlayerSpaces:map.MonsterSpaces;
        if(map.IsCarried(player)&&map.RescuePosition.HasValue)return map.RescueAquatic.CanBreathe(!onlyWater||amphibious,onlyWater||amphibious||WaterBreathingActive(player),map.WaterCells(map.RescuePosition.Value,map.RescueSpaces));
        return aquatic.CanBreathe(!onlyWater||amphibious,onlyWater||amphibious||WaterBreathingActive(player),map.WaterCells(cell,spaces));
    }
    void ObserveWaterBreathing()
    {
        if(page!="battle"||!map.Locked)return;
        playerBreathing.ExhaustionImmune=EffectiveExhaustionImmune(true);monsterBreathing.ExhaustionImmune=EffectiveExhaustionImmune(false);
        if(!combat.PlayerDead)playerBreathing.Observe(WaterBreathingAllowed(true),worldRounds,EffectivePlayerAbility(2));
        if(combat.Hp>0)monsterBreathing.Observe(WaterBreathingAllowed(false),worldRounds,EffectiveMonsterAbility(2));
        foreach(var unit in map.Summons)if(unit.Alive){
            int cells=map.WaterCells(new Vector2Int(unit.X,unit.Y),unit.Spaces);
            unit.Aquatic.ObservePosition(cells,unit.Spaces*unit.Spaces);unit.Breathing.ExhaustionImmune=unit.ExhaustionImmune;
            unit.Breathing.Observe(unit.Aquatic.CanBreathe(unit.BreathesAir,unit.BreathesWater||worldRounds<unit.WaterBreathingExpires,cells),worldRounds,SummonAbility(unit,2));
        }
    }
    bool RestoreBreathingAfterRevival(bool player)
    {
        var state=player?playerBreathing:monsterBreathing;state.ExhaustionImmune=EffectiveExhaustionImmune(player);
        if(state.ReconcileRevival(WaterBreathingAllowed(player)))return true;
        combat.ApplyFatalExhaustion(player,state.SuffocationExhaustion);
        history.Insert(0,(player?"玩家":"木桩")+"复活后仍无法呼吸，保留6级窒息力竭，立即再次死亡；必须先移至可呼吸环境。");
        return false;
    }
    void PlayerCastWhileSubmerged(string name)
    {
        if(spellMetadata==null)spellMetadata=JsonUtility.FromJson<SpellMetadataSet>(Resources.Load<TextAsset>("Rules2024/Spells").text);
        foreach(var spell in spellMetadata.spells)if(spell.name==name){
            if(Array.IndexOf(spell.components,"声音")<0)return;
            PlayerVoiceWhileSubmerged("声音施法");return;
        }
    }
    // Commands and verbal spell components have the same respiratory cost.
    // Invoke only after the requested operation has passed its legality checks.
    void PlayerVoiceWhileSubmerged(string reason)
    {
        ObserveWaterBreathing();
        if(!map.PlayerAquatic.Submerged||playerBreathing.CanBreathe||combat.PlayerDead)return;
        bool wasHolding=playerBreathing.BreathEndsAt>worldRounds;
        playerBreathing.Observe(false,worldRounds,EffectivePlayerAbility(2),true);
        if(wasHolding)history.Insert(0,"水下"+reason+"：本次操作照常生效，玩家立即结束屏息并呛水，从本回合末起结算窒息力竭。");
    }
    bool IssueSummonVerbalCommand(SummonedUnit unit,SummonCommand command)
    {
        if(unit==null||!unit.Alive||!combat.PlayerCanAct||PlayerInBeastForm||summonTurns.Active||rolling||reactionPending)return false;
        bool registered=false;foreach(var candidate in map.Summons)if(ReferenceEquals(candidate,unit)){registered=true;break;}
        if(!registered)return false;
        if(!unit.Orders.Equals(command)&&!SilenceHearCommand(unit)){history.Insert(0,"沉默术阻止口头指令传达，保留旧指令。");return false;}
        bool changed;if(!unit.ReceiveVerbalOrders(command,out changed))return false;
        if(!changed)return true; // Continuing an existing order is silent.
        combat.MarkBattleStarted();
        PlayerVoiceWhileSubmerged("口头指令");
        history.Insert(0,"玩家向"+unit.Name+"发出新口头指令，不消耗动作或附赠动作；指令跨回合保留，更改时才需重新开口。");
        return true;
    }
    void FinishWaterBreathingTurn(bool player)
    {
        if(player?combat.PlayerDead:combat.Hp==0)return;
        ObserveWaterBreathing();var state=player?playerBreathing:monsterBreathing;
        long now=worldRounds+(player?1:0);
        bool increased=state.EndTurn(combat.Round,now,()=>{
            if(!deathWard.Consume(player,now))return false;
            history.Insert(0,"防死结界抵消本次致死窒息力竭，"+(player?"玩家":"木桩")+"保持5级，结界消耗。");return true;
        });
        if(increased)history.Insert(0,(player?"玩家":"木桩")+"窒息力竭 "+state.SuffocationExhaustion+"级，d20减值 "+state.D20Penalty+"，速度减少 "+state.SpeedPenalty+"尺。");
        if(state.Fatal){if(player)worldRounds=now;combat.ApplyFatalExhaustion(player,state.SuffocationExhaustion);CheckBattleOutcome();}
        SyncTimedBuffs();
    }
}
