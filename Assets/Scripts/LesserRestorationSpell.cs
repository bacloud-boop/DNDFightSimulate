using UnityEngine;
public sealed partial class TrainingGame
{
    static readonly string[] RestorationNames={"目盲","耳聋","麻痹","中毒"};
    int lesserRestorationCondition;
    void DrawLesserRestorationChoices()
    {
        GUILayout.Label("解除一种状态（包括全部同类施法来源）",small);
        lesserRestorationCondition=GUILayout.SelectionGrid(lesserRestorationCondition,RestorationNames,2,button);
        GUILayout.Label("只影响所选状态，其他状态保留；没有匹配来源时仍消耗附赠动作。沉默术区域持续造成的失聪不会因此永久消失。",small);
    }
    string LesserRestorationError(bool player,int condition)
    {
        if(!RestorationRules.Valid(condition))return "请选择有效状态。";
        if(player?combat.PlayerDead:combat.Hp==0)return "死亡目标不能接受次级复原术。";
        return !player&&map.DistanceFeet>5?"次级复原术需要接触：请接近至5尺。":"";
    }
    void ApplyLesserRestoration(bool player,int condition)
    {
        int ended=RestorationRules.EndCondition(combat,sensory,player,condition,worldRounds);
        history.Insert(0,"次级复原术："+(player?"玩家":"木桩")+"的"+RestorationNames[condition]+"，清除 "+ended+" 项来源；其他状态保留。");
        if(condition==1&&SilenceWhole(player))history.Insert(0,"目标仍完整位于沉默术范围内，区域持续造成失聪；沉默术本身与雷鸣免疫不被解除。");
        RefreshConcentration();
    }
}
