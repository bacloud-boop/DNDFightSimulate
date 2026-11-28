using UnityEngine;

public sealed partial class TrainingGame
{
    readonly BestialSpiritSession bestialSpirit=new BestialSpiritSession();
    readonly SummonTurnQueue summonTurns=new SummonTurnQueue();
    int nextSummonId=1;
    void SuspendSummonControlAftermath()
    {
        if(!combat.Aftermath||summonActing)return;
        // Close the phase only; retain HP, orders, concentration and expiry.
        summonTurns.Cancel();if(steed!=null)steed.EndTurn();if(panel=="summons")panel="";
    }
    string BestialSpiritCastError(int caster,int slot,bool water,Vector2Int cell)
    {
        if(page!="battle"||!map.Locked)return "请先完成部署。";
        if(rolling||spellTargeting||reactionPending||draggingToken>=0)return "请等待当前操作结束。";
        if(!combat.PlayerCanAct||!combat.MagicActionAvailable)return "玩家没有可用的施法动作。";
        if(!playerResurrectionCaster.CanCast)return "复生术疲惫：完成长休前不能施法。";
        if(!CanCastInCurrentForm)return "当前形态不能施法。";
        if(!ArmorSpellAllowed("Summon Beast"))return "未受训护甲禁止施法，或需先收起武器腾出空手。";
        if((caster!=2&&caster!=11)||classes[caster]<1)return "召唤野兽需要游侠或德鲁伊职业。";
        if(slot<2||slot>9)return "施法环阶须为2至9环。";
        if(worldRounds<0||worldRounds>long.MaxValue-BestialSpiritSession.DurationRounds||nextSummonId==int.MaxValue)return "模拟时间或单位编号超出范围，请重置运行。";
        if(!map.Inside(cell,1)||map.DistanceToCell(true,cell)>90)return "落位须在地图内且距玩家不超过90尺。";
        if(VisibilityAt(true,cell,1)>=2)return "必须能看见召唤位置。";
        var rules=new BestialSpiritRules(slot,classes.ProficiencyBonus+AcidSplash.Modifier(attributes[3]));
        var unit=water?rules.CreateWaterUnit(nextSummonId):rules.CreateLandUnit(nextSummonId);
        return map.CanAddSummon(unit,cell)?"":"该位置被占据，或会阻挡现有形态恢复。";
    }
    bool CastBestialSpirit(int caster,int slot,bool water,Vector2Int cell)
    {
        string error=BestialSpiritCastError(caster,slot,water,cell);if(error!=""){dice=error;return false;}
        var rules=new BestialSpiritRules(slot,classes.ProficiencyBonus+AcidSplash.Modifier(attributes[3]));
        var unit=water?rules.CreateWaterUnit(nextSummonId):rules.CreateLandUnit(nextSummonId);
        if(!combat.TryUseMagicAction())return false;
        long invisibilityToken=senses.Token(true,worldRounds);
        EndConcentration("开始召唤野兽专注");
        // No yield occurs between validation and commit; dismissal only frees space.
        if(!map.AddSummon(unit,cell)||!bestialSpirit.Begin(unit,rules,worldRounds)){
            map.RemoveSummon(unit.Id);history.Insert(0,"召唤野兽落位失败，动作已消耗。");return false;
        }
        nextSummonId++;
        concentrationSpell="召唤野兽";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=bestialSpirit.Expires;
        PlayerCastWhileSubmerged("Summon Beast");BreakOrdinaryInvisibility(true,"施法",invisibilityToken);
        history.Insert(0,"召唤野兽："+unit.Name+"出现在("+(cell.x+1)+","+(cell.y+1)+")，HP "+unit.MaxHp+"、护甲 "+unit.Armor+"，专注至多1小时。尚未收到口头指令。");
        dice="召唤野兽 · "+unit.Name;ObserveWaterBreathing();return true;
    }
    void DismissBestialSpirit()
    {
        var unit=bestialSpirit.Dismiss();if(unit==null)return;
        map.RemoveSummon(unit.Id);summonTurns.Reconcile();
        history.Insert(0,unit.Name+"消失，不留下遗体。");
    }
    void RefreshBestialSpirit()
    {
        // The creature disappearing at 0 HP does not itself say that the caster
        // loses concentration. It grants no replacement creature or free recast.
        if(bestialSpirit.Active&&!bestialSpirit.Unit.Alive&&combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&concentrationSpell=="召唤野兽"&&worldRounds<bestialSpirit.Expires){DismissBestialSpirit();history.Insert(0,"灵魄已消失，原专注可主动结束或等待到期；重新召唤仍需重新施法。");return;}
        if(!bestialSpirit.ShouldDismiss(worldRounds,combat.PlayerHp<=0||combat.PlayerDead||combat.PlayerParalyzed||combat.ConditionIncapacitated(true),concentrationSpell=="召唤野兽"))return;
        if(concentrationSpell=="召唤野兽")EndConcentration("灵魄归零、持续时间结束或施法者失能");
        else DismissBestialSpirit();
    }
}
