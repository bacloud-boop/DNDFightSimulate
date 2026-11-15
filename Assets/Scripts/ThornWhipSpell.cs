using UnityEngine;
public sealed partial class TrainingGame
{
    int thornWhipFeet=10;
    void DrawThornWhipChoices()
    {
        GUILayout.Label("命中后拉近距离（0表示不拉近；最多10尺）",small);
        thornWhipFeet=GUILayout.SelectionGrid(thornWhipFeet,new[]{"0","1","2","3","4","5","6","7","8","9","10"},6,new GUIStyle(button){fontSize=16,fixedHeight=32});
        GUILayout.Label("仅影响大型或更小目标；格子落点受5尺直移、7尺斜移与占地限制。",small);
    }
    void ApplyThornWhipPull(bool player)
    {
        int spaces=player?map.PlayerSpaces:map.MonsterSpaces;
        int moved=!player&&spaces<=2?map.ForceMoveToward(false,map.Player,map.PlayerSpaces,thornWhipFeet):0;
        history.Insert(0,"荆棘之鞭：向施法者拉近"+moved+"尺；不消耗移动力、不触发借机攻击、不追加碰撞伤害。");
    }
}
