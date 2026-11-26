using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    string CrusaderMantleError()=>worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";
    void CastCrusaderMantle()
    {
        concentrationSpell="十字军披风";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;
        history.Insert(0,"十字军披风：30尺光环内自己与盟友的武器／徒手命中额外1d4光耀；专注至多1分钟，升环不增强。");
    }
    bool CrusaderMantleBenefits(int actor)
    {
        if(concentrationSpell!="十字军披风"||worldRounds>=concentrationExpiresWorld||combat.PlayerHp<=0||combat.PlayerDead||combat.PlayerParalyzed||combat.PlayerFeignDeath||combat.PlayerBanished)return false;
        if(actor==0)return true;
        if(actor<=0||NeutralUndead(actor)||!map.SameScene(0,actor)||map.IsContained(true))return false;
        var unit=FindSummon(actor);if(unit==null||!unit.Alive)return false;
        Vector2Int from,to;int a,b;
        return map.SceneBody(0,out from,out a)&&map.SceneBody(actor,out to,out b)&&BattleMap.BodyDistance(from,a,to,b)<=30;
    }
    IEnumerator AddCrusaderMantleDamage(int actor,bool critical,long[] raw)
    {
        if(!CrusaderMantleBenefits(actor))yield break;
        var rolls=new int[critical?2:1];int sum=0;for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,5);sum+=rolls[i];}
        yield return PresentRoll(4,rolls,"十字军披风：额外 "+sum+" 光耀"+(critical?"（暴击骰翻倍）":""));
        raw[11]+=sum;history.Insert(0,"十字军披风："+TransitActorName(actor)+"本次命中额外 "+sum+" 光耀，合并后结算伤害减免。");
    }
}
