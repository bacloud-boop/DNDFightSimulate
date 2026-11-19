using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class SaveAdjustment {public int Value;}
    IEnumerator ApplySaveAdjustments(bool player,bool automatic,SaveAdjustment result,int ability=-1,Vector2Int? origin=null,bool ignoreCover=false,int originSpaces=1)
    {
        result.Value+=combat.RevivalPenalty(player)+(player?playerBreathing.D20Penalty:monsterBreathing.D20Penalty);
        bool sliver=combat.ConsumeSliver(player);
        if(automatic){if(sliver)history.Insert(0,"自动失败的豁免消耗心灵之楔，不再投减值骰。");yield break;}
        if(ability==1&&!ignoreCover){int cover=DexterityCover(player,origin,originSpaces);result.Value+=cover;if(cover>0)history.Insert(0,(player?"玩家":"木桩")+"获得生物半掩护，敏捷豁免+2（不叠加）。");}
        if(player?combat.PlayerBlessed:combat.MonsterBlessed){int value=Random.Range(1,5);yield return PresentRoll(4,new[]{value},"祝福术：本次豁免 +"+value);result.Value+=value;}
        if(player?combat.PlayerBaned:combat.MonsterBaned){int value=Random.Range(1,5);yield return PresentRoll(4,new[]{value},"灾祸术：本次豁免 -"+value);result.Value-=value;}
        if(sliver){int value=Random.Range(1,5);yield return PresentRoll(4,new[]{value},"心灵之楔：本次豁免 -"+value+"，减值已消耗");result.Value-=value;}
    }
}
