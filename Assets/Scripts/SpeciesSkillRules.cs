using System.Collections.Generic;
public static class SpeciesSkillRules
{
    public static int[] Merge(int[] original,SpeciesProfile species,IEnumerable<int> expertise=null)
    {
        var result=(int[])original.Clone();if(species!=null)foreach(int id in species.Proficiencies())if(id>=0&&id<result.Length)result[id]=System.Math.Max(1,result[id]);
        if(expertise!=null)foreach(int id in expertise)if(id>=0&&id<result.Length&&result[id]>0)result[id]=2;return result;
    }
    public static bool TryBuild(PlayerClasses classes,int background,IDictionary<string,int> choices,SpeciesProfile species,out int[] baseline,out HashSet<int> expertise,out string error)
    {
        List<SkillGrant> unused;baseline=SkillCreationRules.Preview(classes,background,choices,out unused,out error);expertise=new HashSet<int>();if(error!="")return false;
        if(species!=null){foreach(int id in species.Proficiencies())if(baseline[id]>0){error="种族可选熟练与职业／背景重复，请在原列表内选择其他项目。";return false;}}
        var merged=Merge(baseline,species);
        foreach(var grant in SkillCreationRules.ExpertiseGrants(classes,merged)){
            int selected=SkillCreationRules.Choice(grant,choices);if(System.Array.IndexOf(grant.Options,selected)<0||merged[selected]!=1||!expertise.Add(selected)){error="专精须选择已熟练且尚未专精的技能。";return false;}
        }return true;
    }
}
