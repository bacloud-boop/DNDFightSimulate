using UnityEngine;
public sealed partial class TrainingGame
{
    int healTarget;
    void DrawHealChoices()
    {
        if(GUILayout.Button((healTarget==0?"✓ ":"")+"目标：玩家",button))healTarget=0;
        if(GUILayout.Button((healTarget==-1?"✓ ":"")+"目标：木桩",button))healTarget=-1;
        foreach(var unit in map.SceneSummons)if(unit.Alive&&GUILayout.Button((healTarget==unit.Id?"✓ ":"")+"目标："+unit.Name,button))healTarget=unit.Id;
        GUILayout.Label("固定恢复 "+HealRules.Amount(directSlotLevel)+" HP，不加施法调整值。解除目盲、耳聋和中毒；不解除麻痹、恐慌或睡眠昏迷。",small);
    }
    string HealSpellError()=>FixedHealingTargetError(healTarget);
    string FixedHealingTargetError(int targetId)
    {
        Vector2Int cell;int spaces;string name;if(!SummonTarget(targetId,out cell,out spaces,out name))return "请选择存活或倒地但未死亡的目标。";
        if(BattleMap.BodyDistance(map.Player,map.PlayerSpaces,cell,spaces)>60)return "目标超出60尺。";
        if(targetId==-1&&!PlayerSeesMonster)return "该治疗法术必须能看见目标，不能直接选取未被识破的隐形木桩。";
        if(targetId==0)return CanSeeOwnBody()?"":"该治疗法术必须能看见目标，自身也不例外。";
        return VisibilityAt(true,cell,spaces)<2?"":"该治疗法术必须能看见目标。";
    }
    void ApplyHealSpell(int slot)
    {
        if(HealSpellError()!=""){history.Insert(0,"医疗术目标已失效。");return;}
        int actual;
        if(healTarget==0||healTarget==-1)actual=HealRules.Apply(combat,sensory,healTarget==0,slot,worldRounds);
        else {actual=FindSummon(healTarget).Heal(HealRules.Amount(slot));EndAuxiliaryCondition(healTarget,0,worldRounds);SyncFeignDeath();}
        if(healTarget==0)ApplySteedLifeBond(actual,6);
        history.Insert(0,"医疗术："+(healTarget==0?"玩家":healTarget==-1?"木桩":FindSummon(healTarget).Name)+"实际恢复 "+actual+" HP，解除目盲、耳聋和中毒的全部施法来源。持续区域造成的状态仍按位置判定。");
        dice="医疗术 · 固定治疗 "+HealRules.Amount(slot)+" · 实际恢复 "+actual;
        RefreshConcentration();
    }
}
