using UnityEngine;
public sealed partial class TrainingGame
{
    string fortifyPlayerInput="120",fortifyMonsterInput="0";
    void DrawFortifyAllocation()
    {
        GUILayout.Label("分配120临时生命（整数；填0表示不选择该目标）",small);
        GUILayout.Label("玩家",label);fortifyPlayerInput=IntegerField(fortifyPlayerInput,false);
        GUILayout.Label("木桩",label);fortifyMonsterInput=IntegerField(fortifyMonsterInput,false);
    }
    string FortifyInvalidReason()
    {
        int player,monster;if(!int.TryParse(fortifyPlayerInput,out player)||!int.TryParse(fortifyMonsterInput,out monster)||player<0||monster<0||player>120||monster>120||player+monster!=120)return "两项必须为0～120的整数，合计恰好120。";
        if(monster>0&&(combat.Hp==0||map.DistanceFeet>60))return "木桩必须存活且位于60尺内。";
        return "";
    }
    void ApplyFortify()
    {
        int player=int.Parse(fortifyPlayerInput),monster=int.Parse(fortifyMonsterInput);
        if(player>0)combat.GrantPlayerTemporaryHp(player);if(monster>0)combat.GrantMonsterTemporaryHp(monster);
        history.Insert(0,"律令巩固分配：玩家"+player+"，木桩"+monster+"；当前临时生命：玩家"+combat.PlayerTemporaryHp+"，木桩"+combat.MonsterTemporaryHp+"（保留已有与新值中较高者，不叠加）。");dice="律令巩固 · 已生效";
    }
}
