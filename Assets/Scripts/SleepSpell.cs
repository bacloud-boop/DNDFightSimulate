using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    int sleepDc;
    int sleepRepeatRound=-1;
    bool sleepTargetMonster=true,sleepTargetPlayer;
    readonly HashSet<int> sleepSelectedSummons=new HashSet<int>();
    bool SleepInArea(Vector2Int center,Vector2Int body,int spaces)
        =>BattleMap.BodyDistance(center,1,body,spaces)<=5;
    string SleepError()
    {
        if(worldRounds>long.MaxValue-10)return "世界时间超出范围。";
        Vector2Int center;
        if(!VisionPoint(out center)||map.DistanceToCell(true,center)>60)return "睡眠术中心必须在60尺内。";
        if(sleepTargetMonster&&(combat.Hp==0||map.IsContained(false)||map.IsCarried(false)||!SleepInArea(center,map.Monster,map.MonsterSpaces)))return "选中的木桩必须在范围内且存活。";
        if(sleepTargetPlayer&&!SleepInArea(center,map.Player,map.PlayerSpaces))return "玩家不在所选范围内。";
        foreach(int id in sleepSelectedSummons){var unit=FindSummon(id);if(unit==null||!unit.Alive||!SleepInArea(center,new Vector2Int(unit.X,unit.Y),unit.Spaces))return "选中的召唤物已离开范围或消失。";}
        return sleepTargetMonster||sleepTargetPlayer||sleepSelectedSummons.Count>0?"":"请至少选择一个范围内目标。";
    }
    void DrawSleepChoices()
    {
        sleepSelectedSummons.RemoveWhere(id=>FindSummon(id)==null);
        GUILayout.Label("选择5尺半径区域的中心列／行和目标。",small);
        visionColumn=IntegerField(visionColumn,false);
        visionRow=IntegerField(visionRow,false);
        sleepTargetMonster=GUILayout.Toggle(sleepTargetMonster,"木桩",label);
        sleepTargetPlayer=GUILayout.Toggle(sleepTargetPlayer,"玩家自身",label);
        foreach(var unit in map.Summons)if(unit.Alive){bool chosen=GUILayout.Toggle(sleepSelectedSummons.Contains(unit.Id),unit.Name,label);if(chosen)sleepSelectedSummons.Add(unit.Id);else sleepSelectedSummons.Remove(unit.Id);}
    }
    IEnumerator ResolveSleep(int caster)
    {
        bool targetMonster=sleepTargetMonster,targetPlayer=sleepTargetPlayer;
        int[] ids=sleepSelectedSummons.ToArray();
        sleepDc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        concentrationSpell="睡眠术";
        concentrationExpiresRound=int.MaxValue;
        concentrationExpiresWorld=worldRounds+10;
        if(targetMonster&&!AutomaticSleepSave(false)){
            var result=new SaveAdjustment();yield return RollSleepSave(result);
            if(result.Value<sleepDc){combat.MonsterSleepStage=1;sleepRepeatRound=combat.Round;}
        }
        foreach(int id in ids){
            var unit=FindSummon(id);if(unit==null||!unit.Alive||unit.NoSleep||unit.ExhaustionImmune)continue;
            var save=new SummonSave();yield return RollSummonSave(unit,3,sleepDc,new Vector2Int(unit.X,unit.Y),save);
            if(save.Failed){unit.SleepStage=1;unit.SleepRepeatRound=combat.Round;}
        }
        if(targetPlayer&&!AutomaticSleepSave(true)){
            var roll=RollArmorSave(true,3);var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(true,false,adjustment,3);
            int total=roll.Value+PlayerSavingThrowModifier(3)+adjustment.Value;
            yield return PresentRoll(20,roll.Dice,"玩家睡眠术感知豁免："+total+" / DC "+sleepDc);
            if(total<sleepDc){EndWildShape("睡眠术导致失能");EndConcentration("玩家自身失能，无法维持睡眠术");yield break;}
        }
        RefreshSleepConcentration();
    }
    IEnumerator RollSleepSave(SaveAdjustment result)
    {
        var roll=RollArmorSave(false,3);var adjustment=new SaveAdjustment();
        yield return ApplySaveAdjustments(false,false,adjustment,3);
        result.Value=roll.Value+MonsterSavingThrowModifier(3)+adjustment.Value;
        yield return PresentRoll(20,roll.Dice,"睡眠术 · 感知豁免："+result.Value+" / DC "+sleepDc);
    }
    void ClearSleepEffects(){combat.MonsterSleepStage=0;foreach(var unit in map.Summons)unit.SleepStage=0;}
    void RefreshSleepConcentration(){if(concentrationSpell=="睡眠术"&&combat.MonsterSleepStage==0&&!map.Summons.Any(u=>u.Alive&&u.SleepStage>0))EndConcentration("所有目标已摆脱睡眠术");}
    void WakeSleepMonster(){combat.MonsterSleepStage=0;RefreshSleepConcentration();}
    IEnumerator RepeatSleepSave()
    {
        if(combat.MonsterSleepStage!=1||combat.Round<sleepRepeatRound||combat.Hp==0)yield break;
        if(AutomaticSleepSave(false)){WakeSleepMonster();yield break;}
        var result=new SaveAdjustment();yield return RollSleepSave(result);
        if(result.Value>=sleepDc){WakeSleepMonster();yield break;}
        combat.MonsterSleepStage=2;combat.MonsterProne=true;DropSleepingMonsterWeapon();
        history.Insert(0,"木桩陷入昏迷，醒来后仍倒地。");
    }
    IEnumerator RepeatSummonSleep(SummonedUnit unit)
    {
        if(!unit.Alive||unit.SleepStage!=1||combat.Round<unit.SleepRepeatRound)yield break;
        if(unit.NoSleep||unit.ExhaustionImmune){unit.SleepStage=0;RefreshSleepConcentration();yield break;}
        var save=new SummonSave();yield return RollSummonSave(unit,3,sleepDc,new Vector2Int(unit.X,unit.Y),save);
        unit.SleepStage=save.Failed?2:0;if(save.Failed)unit.Prone=true;
        RefreshSleepConcentration();
    }
    void DrawSummonWakeOptions(SummonedUnit actor)
    {
        var origin=new Vector2Int(actor.X,actor.Y);
        if(combat.MonsterSleepStage>0&&BattleMap.BodyDistance(origin,actor.Spaces,map.Monster,map.MonsterSpaces)<=5&&GUILayout.Button("摇醒木桩（召唤物动作）",button)&&actor.TryUseAction())WakeSleepMonster();
        foreach(var target in map.Summons)
            if(target.Id!=actor.Id&&target.Alive&&target.SleepStage>0&&BattleMap.BodyDistance(origin,actor.Spaces,new Vector2Int(target.X,target.Y),target.Spaces)<=5&&GUILayout.Button("摇醒 "+target.Name+"（召唤物动作）",button)&&actor.TryUseAction()){target.SleepStage=0;RefreshSleepConcentration();}
    }
    void WakeMonster(){if(combat.MonsterSleepStage>0&&map.DistanceFeet<=5&&combat.TryUseAction())WakeSleepMonster();}
}
