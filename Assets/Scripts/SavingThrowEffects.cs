using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class SaveAdjustment {public int Value;}
    IEnumerator ApplySaveAdjustments(bool player,bool automatic,SaveAdjustment result)
    {
        result.Value+=combat.RevivalPenalty(player);
        bool sliver=combat.ConsumeSliver(player);
        if(automatic){if(sliver)history.Insert(0,"自动失败的豁免消耗心灵之楔，不再投减值骰。");yield break;}
        if(player?combat.PlayerBlessed:combat.MonsterBlessed){int value=Random.Range(1,5);yield return PresentRoll(4,new[]{value},"祝福术：本次豁免 +"+value);result.Value+=value;}
        if(player?combat.PlayerBaned:combat.MonsterBaned){int value=Random.Range(1,5);yield return PresentRoll(4,new[]{value},"灾祸术：本次豁免 -"+value);result.Value-=value;}
        if(sliver){int value=Random.Range(1,5);yield return PresentRoll(4,new[]{value},"心灵之楔：本次豁免 -"+value+"，减值已消耗");result.Value-=value;}
    }
}
