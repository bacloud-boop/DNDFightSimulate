using System;
public static class SkillCheckRules
{
    public static readonly string[] Names={"运动","特技","巧手","隐匿","奥秘","历史","调查","自然","宗教","驯兽","洞悉","医药","察觉","求生","欺瞒","威吓","表演","游说"};
    public static readonly int[] Abilities={0,1,1,1,4,4,4,4,4,3,3,3,3,3,5,5,5,5};
    public static int Modifier(int score,int tier,int proficiency,bool skill,bool jackOfAllTrades)
    {
        if(tier<0||tier>2)throw new ArgumentOutOfRangeException(nameof(tier));
        return (int)Math.Floor((score-10)/2.0)+(tier>0?tier*proficiency:skill&&jackOfAllTrades?proficiency/2:0);
    }
    public static int SelectDie(int first,int second,bool advantage,bool disadvantage)
    {
        if(first<1||first>20)throw new ArgumentOutOfRangeException(nameof(first));
        if(advantage==disadvantage)return first;
        if(second<1||second>20)throw new ArgumentOutOfRangeException(nameof(second));
        return advantage?Math.Max(first,second):Math.Min(first,second);
    }
    public static int EffectiveDie(int die,bool reliableTalent,int tier)=>reliableTalent&&tier>0?Math.Max(10,die):die;
    public static bool Success(int die,int modifier,int dc)=> (long)die+modifier>=dc;
}
