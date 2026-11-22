using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    List<SummonedUnit> SummonsInBurst(Vector2Int center,int radius)
    {
        var found=new List<SummonedUnit>();foreach(var unit in map.Summons)if(unit.Alive&&BattleMap.BodyDistance(center,1,new Vector2Int(unit.X,unit.Y),unit.Spaces)<=radius)found.Add(unit);return found;
    }
    sealed class SummonSave {public bool Failed;}
    ArmorSaveRoll RollSummonSavingThrow(SummonedUnit unit,int ability,bool advantage=false,bool disadvantage=false)
    {
        advantage|=ability==1&&unit.Dodging&&unit.EffectiveSpeed>0||ability==3&&BeaconHope(unit.Id);
        disadvantage|=ability==1&&unit.Restrained;
        return RollSavingThrowDice(advantage,disadvantage);
    }
    IEnumerator RollSummonSave(SummonedUnit unit,int ability,int dc,Vector2Int center,SummonSave result,int? sourceId=null)
    {
        if(unit.SleepStage==2&&(ability==0||ability==1)){result.Failed=true;yield break;}
        int modifier=BondBonus(unit.Id)+AcidSplash.Modifier(SummonAbility(unit,ability))+unit.Breathing.D20Penalty;
        if(ability==1)modifier+=sourceId.HasValue?map.CreatureCoverBonus(sourceId.Value,unit.Id):map.CreatureCoverFromPoint(center,unit.Id);
        var roll=RollSummonSavingThrow(unit,ability);
        yield return PresentRoll(20,roll.Dice,unit.Name+" · "+PlayerAttributes.Names[ability]+"豁免："+roll.Value+" + "+modifier+" / DC "+dc);
        result.Failed=roll.Value+modifier<dc;
    }
    int ApplySummonSpellDamage(SummonedUnit unit,int raw,string type,bool? sourcePlayer=true,string context="")
    {
        int damage=SilenceUnitImmune(unit,type)?0:unit.Aquatic.Submerged&&type=="火焰"&&!unit.ResistsType(DamageTypes.Index(type))?raw/2:raw;
        int actual=unit.Damage(damage,DamageTypes.Index(type));if(damage>0&&sourcePlayer.HasValue)BreakOrdinaryInvisibility(sourcePlayer.Value,"造成伤害");
        history.Insert(0,(context==""?"":context+"：")+unit.Name+"承受 "+damage+" "+type+"伤害（扣除HP "+actual+"）。");return actual;
    }
}
