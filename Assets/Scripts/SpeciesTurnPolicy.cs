public static class SpeciesTurnPolicy
{
    public static readonly string[] PriorityNames={"种族特性优先","法术附赠动作优先","禁用自动使用"};
    public static bool Valid(int priority)=>priority>=0&&priority<PriorityNames.Length;
    public static bool ShouldAttempt(int priority,bool beforeSpell,bool bonusAvailable,bool physicallyActive,bool canAct)
        =>Valid(priority)&&priority!=2&&bonusAvailable&&physicallyActive&&canAct&&(priority==0)==beforeSpell;
}
