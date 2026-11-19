using UnityEngine;
public sealed partial class TrainingGame
{
    int stoneskinChoice,stoneskinTarget=int.MinValue;
    bool stoneskinWilling;
    bool StoneskinResists(bool player,int type)
        =>type>=0&&type<=2&&concentrationSpell=="石肤术"&&stoneskinTarget==(player?0:-1)&&worldRounds<concentrationExpiresWorld;
    void DrawStoneskinChoices()
    {
        if(GUILayout.Button((stoneskinChoice==0?"✓ ":"")+"玩家自身",button))stoneskinChoice=0;
        if(GUILayout.Button((stoneskinChoice==-1?"✓ ":"")+"木桩",button))stoneskinChoice=-1;
        foreach(var unit in map.Summons)if(unit.Alive&&GUILayout.Button((stoneskinChoice==unit.Id?"✓ ":"")+unit.Name,button))stoneskinChoice=unit.Id;
        if(stoneskinChoice!=0)stoneskinWilling=GUILayout.Toggle(stoneskinWilling,"目标自愿接受",label);
    }
    string StoneskinError()
    {
        if(worldRounds>long.MaxValue-600)return "世界时间超出范围。";
        if(stoneskinChoice==0)return "";
        if(!stoneskinWilling)return "目标须自愿接受。";
        if(stoneskinChoice==-1)return combat.Hp>0&&!map.IsContained(false)&&!map.IsCarried(false)&&map.DistanceFeet<=5?"":"须触及5尺内存活的木桩。";
        var unit=FindSummon(stoneskinChoice);
        return unit!=null&&BattleMap.BodyDistance(map.Player,map.PlayerSpaces,new Vector2Int(unit.X,unit.Y),unit.Spaces)<=5?"":"须触及5尺内存活的召唤物。";
    }
    void ApplyStoneskin()
    {
        if(StoneskinError()!=""){history.Insert(0,"石肤术未生效，目标已不再合法。");return;}
        stoneskinTarget=stoneskinChoice;
        concentrationSpell="石肤术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+600;
        if(stoneskinTarget>0)FindSummon(stoneskinTarget).Stoneskin=true;
        history.Insert(0,"石肤术生效：钝击、穿刺、挥砍抗性，专注至多1小时。");
    }
    void ClearStoneskin(){stoneskinTarget=int.MinValue;foreach(var unit in map.Summons)unit.Stoneskin=false;}
}
