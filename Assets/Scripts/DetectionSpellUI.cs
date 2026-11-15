using UnityEngine;
public sealed partial class TrainingGame
{
    readonly DetectEvilGood detectEvilGood=new DetectEvilGood();
    string DetectEvilGoodError(bool self)
    {
        if(!self)return "侦测善恶只作用于自身。";
        return worldRounds<0||worldRounds>long.MaxValue-100?"世界时间超出此法术可记录范围。":"";
    }
    void ApplyDetectEvilGood()
    {
        EndConcentration("开始侦测善恶专注");detectEvilGood.Begin(worldRounds);concentrationSpell="侦测善恶";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+100;
        history.Insert(0,"侦测善恶：持续感知30尺内特定生物的位置，专注至多10分钟；不判定阵营，不赋予看见隐形或穿透黑暗的能力。场景暂无墙壁或圣居术区域。");
    }
    void DrawDetectionMap(Rect board)
    {
        if(!detectEvilGood.Detects(monsterProfile.TypeIndex,map.DistanceFeet,combat.Hp>0,worldRounds))return;
        Rect rect=BodyRect(board,map.Monster,map.MonsterSpaces);Color color=new Color(1,.75f,.15f,.95f);
        Fill(new Rect(rect.x,rect.y,rect.width,3),color);Fill(new Rect(rect.x,rect.yMax-3,rect.width,3),color);Fill(new Rect(rect.x,rect.y,3,rect.height),color);Fill(new Rect(rect.xMax-3,rect.y,3,rect.height),color);
    }
}
