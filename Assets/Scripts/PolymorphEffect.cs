using System;

// Retained creature identity and base HP stay outside this temporary stat layer.
public sealed class PolymorphEffect
{
    public bool Active {get;private set;}
    public bool TargetPlayer {get;private set;}
    public string Form {get;private set;}="";
    public long Expires {get;private set;}
    int[] abilities,saves,skills;
    public int Armor {get;private set;}
    public int Speed {get;private set;}
    public int Darkvision {get;private set;}
    public int Blindsight {get;private set;}
    public int Spaces {get;private set;}
    // Missing CR is distinct from CR 0. Do not infer a CR from a summon slot.
    public static int TargetChallengeLimit(int? challengeEighths,int classLevels)
    {
        if(classLevels>0&&classLevels<=20)return classLevels*8;
        return challengeEighths.HasValue&&challengeEighths.Value>=0&&challengeEighths.Value<=240?challengeEighths.Value:-1;
    }
    public static bool AllowedChallenge(int beastEighths,int targetEighths)=>beastEighths>=0&&targetEighths>=0&&beastEighths<=targetEighths;
    public void Begin(bool player,string form,long now,int armor,int speed,int darkvision,int blindsight,int spaces,int[] scores,int[] saveModifiers,int[] skillModifiers)
    {
        if(string.IsNullOrEmpty(form)||now<0||now>long.MaxValue-600||armor<0||speed<0||darkvision<0||blindsight<0||spaces<1||spaces>4||scores==null||scores.Length!=6||saveModifiers==null||saveModifiers.Length!=6||skillModifiers==null||skillModifiers.Length!=18)throw new ArgumentException("Invalid polymorph form");
        var nextScores=(int[])scores.Clone();var nextSaves=(int[])saveModifiers.Clone();var nextSkills=(int[])skillModifiers.Clone();
        for(int i=0;i<6;i++)if(nextScores[i]<1||nextScores[i]>30)throw new ArgumentException("Invalid beast ability");
        TargetPlayer=player;Form=form;Expires=now+600;Armor=armor;Speed=speed;Darkvision=darkvision;Blindsight=blindsight;Spaces=spaces;
        abilities=nextScores;saves=nextSaves;skills=nextSkills;Active=true;
    }
    public int Ability(int index)=>abilities[index];
    public int Save(int index)=>saves[index];
    public int Skill(int index,int ability)=>skills[index]>-999?skills[index]:(int)Math.Floor((abilities[ability]-10)/2.0);
    public bool ShouldEnd(long now,int temporaryHp,bool dead)=>Active&&(now>=Expires||temporaryHp<=0||dead);
    public void Clear(){Active=false;Form="";}
}
