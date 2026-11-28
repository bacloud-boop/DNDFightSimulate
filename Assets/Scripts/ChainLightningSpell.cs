using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    int chainFirst=-1;
    readonly HashSet<int> chainSecondary=new HashSet<int>();
    List<int> LivingSpellTargets(){var ids=new List<int>();Vector2Int p;int n;string name;if(SummonTarget(0,out p,out n,out name))ids.Add(0);if(SummonTarget(-1,out p,out n,out name))ids.Add(-1);foreach(var unit in map.SceneSummons)if(unit.Alive)ids.Add(unit.Id);return ids;}
    int SpellTargetDistance(int a,int b){Vector2Int p,q;int n,m;string name;return SummonTarget(a,out p,out n,out name)&&SummonTarget(b,out q,out m,out name)?BattleMap.BodyDistance(p,n,q,m):int.MaxValue;}
    bool LivingSpellTarget(int id){Vector2Int p;int n;string name;return SummonTarget(id,out p,out n,out name);}
    bool PlayerSeesSpellTarget(int id){Vector2Int p;int n;string name;if(!SummonTarget(id,out p,out n,out name))return false;return id==0?CanSeeOwnBody():id==-1?PlayerSeesMonster:VisibilityAt(true,p,n)<2;}
    string SpellTargetName(int id){Vector2Int p;int n;string name;return SummonTarget(id,out p,out n,out name)?name:"已失效目标 #"+id;}
    void DrawChainLightningChoices()
    {
        GUILayout.Label("选择150尺内可见的首目标；再选至多 "+ChainLightningRules.SecondaryLimit(directSlotLevel)+" 个距首目标30尺内的不同生物。次级目标无需可见。",small);
        foreach(int id in LivingSpellTargets()){
            if(GUILayout.Button((chainFirst==id?"✓ ":"")+"首目标："+SpellTargetName(id),button)){chainFirst=id;chainSecondary.Remove(id);}
        }
        foreach(int id in LivingSpellTargets())if(id!=chainFirst){bool selected=GUILayout.Toggle(chainSecondary.Contains(id),"次级："+SpellTargetName(id)+" · 距首目标 "+SpellTargetDistance(chainFirst,id)+"尺",label);if(selected)chainSecondary.Add(id);else chainSecondary.Remove(id);}
        foreach(int id in new List<int>(chainSecondary))if(!LivingSpellTarget(id)&&GUILayout.Button("移除失效目标 #"+id,button))chainSecondary.Remove(id);
        GUILayout.Label("同一目标只承受一道闪电；所有目标分别进行敏捷豁免，共享10d8伤害骰，成功伤害减半。每升环只增加一个次级目标，不增加伤害骰。",small);
    }
    string ChainLightningError(int slot)=>ChainLightningRules.Valid(chainFirst,chainSecondary,slot,LivingSpellTarget,PlayerSeesSpellTarget,SpellTargetDistance)?"":"首目标须在150尺内且可见；次级目标须存活、互不重复、距首目标不超过30尺，数量不能超过环阶限制。";
    sealed class MappedSave {public bool Failed;}
    IEnumerator RollMappedSpellSave(int id,int ability,int dc,int sourceId,string title,MappedSave result)
    {
        if(id!=0&&id!=-1){var unit=FindSummon(id);if(unit==null)yield break;var save=new SummonSave();yield return RollSummonSave(unit,ability,dc,map.Player,save,sourceId);result.Failed=save.Failed;yield break;}
        bool player=id==0,automatic=(ability==0||ability==1)&&(player?combat.PlayerHp==0||combat.PlayerParalyzed:combat.MonsterHelpless);
        var roll=RollArmorSave(player,ability);int modifier=player?PlayerSavingThrowModifier(ability):MonsterSavingThrowModifier(ability);
        var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,automatic,adjustment,ability,ignoreCover:true);modifier+=adjustment.Value;
        int cover=ability==1?map.CreatureCoverBonus(sourceId,id):0;modifier+=cover;
        if(!automatic)yield return PresentRoll(20,roll.Dice,title+" · "+SpellTargetName(id)+PlayerAttributes.Names[ability]+"豁免："+roll.Value+" + "+modifier+" / DC "+dc+(cover>0?"（半掩护+2）":""));
        result.Failed=automatic||(long)roll.Value+modifier<dc;
    }
    IEnumerator ResolveChainLightning(int caster,int slot)
    {
        if(ChainLightningError(slot)!="")yield break;
        int first=chainFirst;var targets=new List<int>{first};var rest=new List<int>(chainSecondary);rest.Sort();targets.AddRange(rest);
        var failed=new HashSet<int>();int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        outcomeDeferDepth++;try{
            foreach(int id in targets){var result=new MappedSave();yield return RollMappedSpellSave(id,1,dc,id==first?0:first,"连锁闪电",result);if(result.Failed)failed.Add(id);}
            var rolls=new int[10];int damage=0;for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,9);damage+=rolls[i];}
            yield return PresentRoll(8,rolls,"连锁闪电共享10d8：失败 "+damage+" 闪电，成功 "+damage/2);
            var ray=new RayDamage();yield return ReduceRayDamage(true,damage,"闪电","连锁闪电共享伤害",ray);damage=ray.Amount;
            int playerDamage=0;
            foreach(int id in targets){string name=SpellTargetName(id);int raw=failed.Contains(id)?damage:damage/2;
                if(id==0||id==-1){int adjusted=combat.AdjustIncomingTypedDamage(id==0,raw,7);if(id==0){playerDamage=adjusted;combat.ApplyPlayerDamage(adjusted,sourcePlayer:true);}else combat.ApplyDamage(adjusted);history.Insert(0,"连锁闪电："+name+"受到 "+adjusted+" 闪电伤害。");}
                else {var unit=FindSummon(id);if(unit!=null)ApplySummonSpellDamage(unit,raw,"闪电");}
            }
            if(playerDamage>0)yield return MaintainConcentration(playerDamage);RefreshBestialSpirit();dice="连锁闪电 · "+targets.Count+"名目标结算完成";
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
}
