using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly EntangleEffect entangle=new EntangleEffect();
    bool Entangled(bool player)=>entangle.Restrained(player?0:-1,worldRounds);
    void InitializeEntangle(){map.DifficultTerrain=FullDifficultTerrain;map.RestrainedActor=Entangled;}
    void SyncEntangleActors(){combat.MonsterRestrained=Entangled(false);foreach(var unit in map.Summons)unit.Restrained=entangle.Restrained(unit.Id,worldRounds);}
    void EndEntangle(){entangle.Clear();SyncEntangleActors();}
    string EntangleError(){Vector2Int cell;if(!VisionPoint(out cell))return "纠缠区域左上格必须位于地图内。";if(map.DistanceToCell(true,cell)>90)return "纠缠区域起点超出90尺。";return worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";}
    void DrawEntangleChoices(){GUILayout.Label("20尺方形（4×4格），输入左上格列与行。水域同样生效；完整占地进入才增加地形移动消耗。初始区域内除施法者外的生物进行力量豁免，后进入者不额外束缚。",small);visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);}
    void DrawEntangleArea(Rect board)
    {
        Vector2Int preview=Vector2Int.zero;bool choosing=panel=="spells"&&selectedSpellName=="Entangle"&&VisionPoint(out preview);
        if(!choosing&&!entangle.Active(worldRounds))return;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){
            if(entangle.FullFootprint(x,y,1,worldRounds))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.1f,.5f,.12f,.32f));
            if(choosing&&x>=preview.x&&x<preview.x+4&&y>=preview.y&&y<preview.y+4)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.4f,.7f,.1f,.35f));
        }
    }
    IEnumerator ResolveEntangle(Vector2Int anchor,int caster)
    {
        int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        entangle.Begin(anchor.x,anchor.y,dc,worldRounds);concentrationSpell="纠缠术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;
        if(combat.Hp>0&&entangle.Intersects(map.Monster.x,map.Monster.y,map.MonsterSpaces,worldRounds)){
            bool automatic=combat.MonsterParalyzed||combat.MonsterSleepStage==2;var roll=RollArmorSave(false,0);int modifier=MonsterSavingThrowModifier(0);var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(false,automatic,adjustment,0);modifier+=adjustment.Value;
            if(!automatic)yield return PresentRoll(20,roll.Dice,"纠缠术 · 木桩力量豁免："+roll.Value+" + "+modifier+" / DC "+dc);
            if(automatic||(long)roll.Value+modifier<dc){entangle.Bind(-1,worldRounds);history.Insert(0,"木桩被纠缠术束缚，需消耗动作进行力量（运动）检定脱困。");}else history.Insert(0,"木桩通过力量豁免，未被束缚；困难地形仍生效。");
        }
        foreach(var unit in map.Summons)if(unit.Alive&&entangle.Intersects(unit.X,unit.Y,unit.Spaces,worldRounds)){
            var save=new SummonSave();yield return RollSummonSave(unit,0,dc,anchor,save);if(save.Failed)entangle.Bind(unit.Id,worldRounds);
        }
        SyncEntangleActors();SyncTimedBuffs();history.Insert(0,"纠缠术：20尺方形困难地形持续至多1分钟；初始豁免排除施法者。脱困、离开区域或区域内没有束缚目标，都不会提前移除地形。");
    }
    IEnumerator EscapeMonsterEntangle()
    {
        int tier=MonsterPolymorphed?0:monsterSkillTiers[0];long modifier=AcidSplash.Modifier(EffectiveMonsterAbility(0))+(long)tier*combat.MonsterProficiencyBonus+combat.MonsterRevivalPenalty+monsterBreathing.D20Penalty;
        if(MonsterPolymorphed)modifier=polymorph.Skill(0,0)+combat.MonsterRevivalPenalty+monsterBreathing.D20Penalty;
        bool advantage=SizeStrengthAdvantage(false,0)||checkBuff.Advantage(false,0,worldRounds),disadvantage=SizeStrengthDisadvantage(false,0)||RayStrengthDisadvantage(false,0)||ArmorD20Disadvantage(false,0)||combat.MonsterPoisoned||mark.CheckDisadvantage(false,0);
        int first=Random.Range(1,21),second=advantage!=disadvantage?Random.Range(1,21):0,value=second==0?first:advantage?Mathf.Max(first,second):Mathf.Min(first,second);
        yield return PresentRoll(20,second==0?new[]{first}:new[]{first,second},"木桩脱困 · 力量（运动）：采用 "+value+" + "+modifier+" / DC "+entangle.Dc);
        if((long)value+modifier>=entangle.Dc){entangle.Release(-1);SyncEntangleActors();SyncTimedBuffs();map.StartMonsterMovementTurn(combat.MonsterSpeed);history.Insert(0,"木桩脱困成功，动作已消耗；可继续移动，本回合不再攻击。");}
        else history.Insert(0,"木桩脱困失败，动作已消耗，继续束缚；本回合不攻击。");
    }
    IEnumerator EscapeSpiritEntangle(SummonedUnit unit)
    {
        if(unit==null||!entangle.Restrained(unit.Id,worldRounds)||!unit.TryUseAction())yield break;
        summonActing=true;try{
            int value=Random.Range(1,21),modifier=AcidSplash.Modifier(bestialSpirit.Rules.Ability(0))+unit.Breathing.D20Penalty;
            yield return PresentRoll(20,new[]{value},unit.Name+"脱困 · 力量（运动）："+value+" + "+modifier+" / DC "+entangle.Dc);
            if((long)value+modifier>=entangle.Dc){entangle.Release(unit.Id);SyncEntangleActors();history.Insert(0,unit.Name+"脱困成功，已消耗其自身动作。");}else history.Insert(0,unit.Name+"脱困失败，已消耗其自身动作。");
        }finally{summonActing=false;}
    }
}
