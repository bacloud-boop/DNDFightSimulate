using System;
using System.Collections.Generic;
using System.Linq;
public sealed class SkillGrant
{
    public string Id,Title,Group;public int[] Options;
    public SkillGrant(string id,string title,string group,int[] options){Id=id;Title=title;Group=group;Options=options;}
}
public static class SkillCreationRules
{
    public static int[] Range(int start,int count)=>Enumerable.Range(start,count).ToArray();
    public static string Name(int id)=>id<0||id>=55?"未选择":id<18?SkillCheckRules.Names[id]:SkillSourceTables.ToolNames[id-18];
    public static string Signature(PlayerClasses c)=>c.InitialClass+":"+string.Join(",",Enumerable.Range(0,12).Select(i=>c[i].ToString()).ToArray());
    public static List<SkillGrant> BaseGrants(PlayerClasses c,int background)
    {
        var list=new List<SkillGrant>();if(background<0||background>=16)return list;
        for(int i=0;i<2;i++)list.Add(new SkillGrant("bg-s"+i,"背景固定技能","bgSkills",new[]{SkillSourceTables.BackgroundSkills[background][i]}));
        list.Add(new SkillGrant("bg-tool","背景工具","bgTool",SkillSourceTables.BackgroundTools[background]));
        string feat=SkillSourceTables.BackgroundFeats[background];
        int[] choices=feat=="Skilled"?Range(0,55):feat=="Musician"?Range(39,10):new[]{21,27,28,30,31,32,33,34};
        if(feat!="")for(int i=0;i<3;i++)list.Add(new SkillGrant("origin-"+i,"背景专长 "+feat+" · 选择"+(i+1),"origin",choices));
        for(int cls=0;cls<12;cls++)if(c[cls]>0){
            bool initial=cls==c.InitialClass;int count=initial?SkillSourceTables.ClassCounts[cls]:(cls==2||cls==5||cls==7)?1:0;
            for(int i=0;i<count;i++)list.Add(new SkillGrant("class"+cls+"-"+i,PlayerClasses.Names[cls]+"技能 "+(i+1),"class"+cls,SkillSourceTables.ClassOptions[cls]));
            if(cls==5)for(int i=0;i<(initial?3:1);i++)list.Add(new SkillGrant("bard-tool"+i,"吟游诗人乐器 "+(i+1),"bardTools",Range(39,10)));
            if(cls==7)list.Add(new SkillGrant("rogue-tool","游荡者盗贼工具","rogueTools",new[]{54}));
            if(initial&&cls==11)list.Add(new SkillGrant("druid-tool","德鲁伊草药工具","druidTools",new[]{51}));
            if(initial&&cls==8)list.Add(new SkillGrant("monk-tool","武僧工匠工具或乐器","monkTools",Range(18,17).Concat(Range(39,10)).ToArray()));
        }return list;
    }
    public static int Choice(SkillGrant grant,IDictionary<string,int> choices){int value;return grant.Options.Length==1?grant.Options[0]:choices.TryGetValue(grant.Id,out value)?value:-1;}
    public static int[] Preview(PlayerClasses c,int background,IDictionary<string,int> choices,out List<SkillGrant> replacements,out string error)
    {
        var tiers=new int[55];error=background<0?"请选择背景。":"";var seenGroups=new HashSet<string>();
        foreach(var grant in BaseGrants(c,background).OrderBy(g=>g.Options.Length==1?0:1)){
            int value=Choice(grant,choices);
            if(Array.IndexOf(grant.Options,value)<0){if(error=="")error="请完成所有职业、背景与专长熟练选择。";continue;}
            if(!seenGroups.Add(grant.Group+":"+value)){error="同一来源的多项选择不能重复。";continue;}
            if(tiers[value]>0&&grant.Options.Length>1){if(error=="")error="可选熟练不能与已有熟练重复，请在原列表内另选。";}else tiers[value]=1;
        }
        replacements=new List<SkillGrant>();
        return tiers;
    }
    public static List<SkillGrant> ExpertiseGrants(PlayerClasses c,int[] tiers)
    {
        var result=new List<SkillGrant>();int[] all=Range(0,18).Where(i=>tiers[i]>0).ToArray();
        int[] scholar=all.Where(i=>new[]{4,5,6,7,8,11}.Contains(i)).ToArray();bool hasScholar=c[1]>=2&&scholar.Length>0;
        if(hasScholar)result.Add(new SkillGrant("scholar","法师学者专精","expertise",scholar));
        int general=(c[7]>=1?2:0)+(c[7]>=6?2:0)+(c[5]>=2?2:0)+(c[5]>=9?2:0)+(c[2]>=2?1:0)+(c[2]>=9?2:0);
        general=Math.Min(general,Math.Max(0,all.Length-(hasScholar?1:0)));
        for(int i=0;i<general;i++)result.Add(new SkillGrant("expert"+i,"职业专精 "+(i+1),"expertise",all));return result;
    }
    public static bool TryBuild(PlayerClasses c,int background,IDictionary<string,int> choices,out int[] tiers,out string error)
    {
        List<SkillGrant> replacement;tiers=Preview(c,background,choices,out replacement,out error);if(error!="")return false;
        foreach(var grant in ExpertiseGrants(c,tiers)){
            int value=Choice(grant,choices);if(Array.IndexOf(grant.Options,value)<0||tiers[value]!=1){error="专精必须选择已熟练且尚未专精的技能。";return false;}tiers[value]=2;
        }return true;
    }
}
