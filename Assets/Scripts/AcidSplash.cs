using UnityEngine;
public static class AcidSplash
{
    public const string Summary="酸液飞溅｜Acid Splash\n塑能 戏法（术士、法师）\n施法时间：1 动作\n施法距离：60 尺\n法术成分：V（声音）、S（姿势）\n持续时间：立即 · 非专注\n在射程内一点创造酸液泡，在 5 尺半径球状范围爆发。范围内目标敏捷豁免失败受到强酸伤害，成功无伤害；本模拟不包含玩家自身。\n伤害：1d6；总等级 5 / 11 / 17 时为 2d6 / 3d6 / 4d6。";
    public static int DiceCount(int totalLevel) => totalLevel>=17?4:totalLevel>=11?3:totalLevel>=5?2:1;
    public static int Modifier(int score) => (int)System.Math.Floor((score-10)/2.0);
}
