public static class MonsterSpellCatalog
{
    static readonly DirectSpell Acid=new DirectSpell("Acid Splash","酸液飞溅","60尺内一点5尺半径球状范围；敏捷豁免失败受到1d6强酸，随施法等级强化。",0,60,6,save:1,damageType:"强酸");
    public static DirectSpell Find(string name)=>name=="Acid Splash"?Acid:DirectSpell.Find(name);
    public static bool IsCone(DirectSpell spell)=>spell!=null&&spell.Name=="Burning Hands";
    public static bool IsArea(DirectSpell spell)=>spell!=null&&(spell.Name=="Acid Splash"||spell.Name=="Fireball"||spell.Name=="Shatter"||spell.Name=="Circle of Death"||spell.Name=="Thunderclap"||spell.Name=="Word of Radiance");
    public static int Radius(DirectSpell spell)=>spell.Name=="Acid Splash"?5:spell.EmanationRadius>0?spell.EmanationRadius:spell.BurstRadius;
}
