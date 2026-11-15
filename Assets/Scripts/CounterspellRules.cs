public static class CounterspellRules
{
    public const string Summary="法术反制｜Counterspell\n三环防护 · 反应 · 姿势 · 60尺\n看见范围内生物正在施放带成分的法术时可使用。施法者进行体质豁免，失败则本次法术无效，施法所用动作、附赠动作或反应仍消耗；法术位不消耗。普通法术攻击能力不会自动触发。";
    public static bool CanTrigger(int distance,bool seesCaster,bool components,bool canAct,bool reaction)
        =>distance>=0&&distance<=60&&seesCaster&&components&&canAct&&reaction;
    public static bool Interrupted(int roll,int modifier,long dc)=>roll>=1&&roll<=20&&(long)roll+modifier<dc;
}
