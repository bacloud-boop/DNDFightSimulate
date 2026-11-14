using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    readonly bool[] missileTargetsPlayer=new bool[11];
    void DrawMissileTargets(int count,string kind="飞弹",string rules="每枚飞弹独立1d4+1；所有目标在施法前确定。")
    {
        GUILayout.Label(rules,small);
        for(int i=0;i<count;i++){
            if(GUILayout.Button(kind+" "+(i+1)+" → "+(missileTargetsPlayer[i]?"玩家（自身）":"木桩")+" · 点击切换",button))missileTargetsPlayer[i]=!missileTargetsPlayer[i];
        }
    }
    string MissileTargetError(int count)
    {
        for(int i=0;i<count;i++)if(!missileTargetsPlayer[i]){
            if(selectedSpellName=="Magic Missile"&&!PlayerSeesMonster)return "魔法飞弹需要看见所选目标。";
            if(combat.Hp==0)return "不能指定死亡的木桩。";
            if(map.DistanceFeet>120)return "部分目标超过120尺，不能施放。";
        }
        return "";
    }
    IEnumerator ResolveMagicMissile(int count)
    {
        var targets=new bool[count];var rolls=new int[count];
        for(int i=0;i<count;i++){targets[i]=missileTargetsPlayer[i];rolls[i]=Random.Range(1,5);}
        if(System.Array.Exists(targets,x=>x))yield return OfferShield(false);
        for(int i=0;i<count;i++){
            dice="魔法飞弹 "+(i+1)+" / "+count+" → "+(targets[i]?"玩家":"木桩");
            yield return PresentRoll(4,new[]{rolls[i]},"本枚 "+rolls[i]+" + 1 = "+(rolls[i]+1)+" 力场伤害；投骰结束后同时结算"+(targets[i]&&combat.ShieldActive?"（护盾术：本枚伤害0）":""));
        }
        int playerDamage=0,monsterDamage=0;
        for(int i=0;i<count;i++){
            int damage=targets[i]&&combat.ShieldActive?0:rolls[i]+1;
            if(targets[i]){combat.ApplyPlayerDamage(damage);playerDamage+=damage;}
            else {combat.ApplyDamage(damage);monsterDamage+=damage;}
            history.Insert(0,"魔法飞弹 "+(i+1)+" → "+(targets[i]?"玩家":"木桩")+"：1d4="+rolls[i]+" + 1，"+damage+"力场伤害。");
        }
        // Each dart is a separate source for concentration; HP is settled before these saves.
        for(int i=0;i<count;i++)if(targets[i]&&!combat.ShieldActive)yield return MaintainConcentration(rolls[i]+1);
        dice="魔法飞弹：玩家 "+playerDamage+"，木桩 "+monsterDamage+" 力场伤害";
        CheckBattleOutcome();
    }
}
