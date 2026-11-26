using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly TurnBoundConditions turnConditions=new TurnBoundConditions();
    bool ActorBlinded(int actor)=>SmiteBlinded(actor)||turnConditions.Has(actor,0,worldRounds)||feignDeath.Blind(actor,worldRounds)||((actor==0||actor==-1)&&sensory.Blinded(actor==0,worldRounds));
    int EndAuxiliaryCondition(int actor,int condition,long now)
    {
        int ended=turnConditions.End(actor,condition,now);if(condition==0)ended+=EndBlindingSmite(actor,now);if(condition==0&&feignDeath.EndBlind(actor,now))ended++;return ended;
    }
    string ColorSprayError()=>!TurnBoundConditions.ValidTime(worldRounds)?"世界时间超出可用范围。":BurningHandsError();
    IEnumerator ResolveColorSpray(bool player,int casterClass,BreathTemplate template)
    {
        int source=player?0:-1;var targets=BreathTargets(source,template);var failed=new HashSet<int>();
        long dc=player?8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(casterClass)]):MonsterSpellDc;
        bool oldPreview=burningPreview;var oldTemplate=burningTemplate;int oldSource=burningSource;
        burningPreview=true;burningTemplate=template;burningSource=source;
        try{
            foreach(int target in targets)yield return RollMappedAbilitySave(target,source,dc,failed,"七彩喷射",2);
            foreach(int target in targets){if(failed.Contains(target))turnConditions.Add(source,target,0,worldRounds);history.Insert(0,"七彩喷射："+TransitActorName(target)+(failed.Contains(target)?"体质豁免失败，目盲至施法者下个回合结束。":"体质豁免成功。"));}
            dice="七彩喷射 · "+failed.Count+" / "+targets.Count+" 名生物目盲";
        }finally{burningPreview=oldPreview;burningTemplate=oldTemplate;burningSource=oldSource;}
    }
}
