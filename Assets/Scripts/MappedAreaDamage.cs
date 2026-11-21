using UnityEngine;
public sealed partial class TrainingGame
{
    int ApplyMappedAreaDamage(int target,int caster,int raw,int type,string context,bool? failed=null)
    {
        bool? source=caster==0?(bool?)true:caster==-1?false:(bool?)null;int amount=raw;
        if(target==0||target==-1){amount=combat.AdjustTypedDamage(target==0,amount,type);if(target==0)combat.ApplyPlayerDamage(amount,sourcePlayer:source);else combat.ApplyDamage(amount,source);}
        else if(target==BattleMap.RescueId){if(type==5)IgniteWebActor(target);if(rescueCarrier.ResistsType(type)||map.RescueAquatic.Submerged&&type==5)amount/=2;rescueCarrier.TakeDamage(amount);if(amount>0&&source.HasValue)BreakOrdinaryInvisibility(source.Value,"造成伤害");}
        else {var unit=FindSummon(target);if(unit==null||!unit.Alive)return 0;return ApplySummonSpellDamage(unit,amount,DamageTypes.Names[type],source,context);}
        history.Insert(0,context+"："+TransitActorName(target)+(failed.HasValue?(failed.Value?"豁免失败":"豁免成功"):"")+"，结算 "+amount+" "+DamageTypes.Names[type]+"伤害。");return amount;
    }
}
