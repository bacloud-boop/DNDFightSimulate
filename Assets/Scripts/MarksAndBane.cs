using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly MarkEffect mark=new MarkEffect();int hexAbility;
    sealed class ExtraMarkDamage {public int Value;}
    IEnumerator RollMarkDamage(bool targetPlayer,bool critical,ExtraMarkDamage result)
    {
        if(!mark.Applies(targetPlayer))yield break;
        int[] rolls=new int[critical?2:1];int amount=0;for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,7);amount+=rolls[i];}
        result.Value=combat.AdjustTypedDamage(targetPlayer,amount,mark.DamageType);
        yield return PresentRoll(6,rolls,(mark.Hex?"脆弱诅咒":"猎人印记")+"额外 "+result.Value+" "+DamageTypes.Names[mark.DamageType]+(critical?"（暴击：2d6）":""));
    }
    IEnumerator ResolveBane(bool player,int caster,bool both)
    {
        EndConcentration("开始新的专注法术");concentrationSpell="灾祸术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;
        int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);bool[] targets={both||!player,both||player};
        for(int i=0;i<2;i++)if(targets[i]){
            int die=Random.Range(1,21),modifier=i==0?AcidSplash.Modifier(monsterAttributes[5]):PlayerSavingThrowModifier(5);var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(i==1,false,adjustment);modifier+=adjustment.Value;
            dice="灾祸术 · "+(i==0?"木桩":"玩家")+"魅力豁免";yield return PresentRoll(20,new[]{die},die+" + "+modifier+" / DC "+dc);
            if(die+modifier<dc){if(i==0)combat.MonsterBaned=true;else combat.PlayerBaned=true;history.Insert(0,(i==0?"木桩":"玩家")+"灾祸术豁免失败，攻击与豁免每次减1d4。");}
            else history.Insert(0,(i==0?"木桩":"玩家")+"灾祸术豁免成功。");
        }
    }
}
