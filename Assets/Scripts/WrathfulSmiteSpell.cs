using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly WrathfulSmiteEffects wrathfulSmite=new WrathfulSmiteEffects();
    bool WrathfulFear(int target)=>!HeroismImmune(target)&&wrathfulSmite.Active(target,worldRounds);
    bool ActorFrightened(int target)=>SteedFear(target)||WrathfulFear(target);
    bool WrathfulFearVisible(int target)=>WrathfulFear(target)&&map.SameScene(0,target)&&ConjuredTargetVisible(target==0,0);
    bool WrathfulMovementAllowed(int actor,Vector2Int cell)
    {
        if(!WrathfulFear(actor)||!map.SameScene(0,actor))return true;
        Vector2Int from,source;int n,m;
        if(!map.SceneBody(actor,out from,out n)||!map.SceneBody(0,out source,out m))return true;
        if(actor==0)n=map.PlayerSpaces;m=map.PlayerSpaces;
        int start=BattleMap.BodyDistance(from,n,source,m);
        foreach(var step in BattleMap.MovementAnchors(from,cell))if(BattleMap.BodyDistance(step,n,source,m)<start)return false;
        return true;
    }
    IEnumerator ResolveWrathfulSmiteHit(int target,SmiteHitResult hit)
    {
        if(!hit.Wrathful||!BanishLiving(target))yield break;
        if(HeroismImmune(target)){history.Insert(0,"激愤斩：目标免疫恐慌，额外暗蚀伤害仍结算。");yield break;}
        var failed=new HashSet<int>();yield return RollMappedAbilitySave(target,0,hit.WrathfulDc,failed,"激愤斩",3,avoidsFrightened:true);
        if(failed.Contains(target)){wrathfulSmite.Add(target,hit.WrathfulDc,worldRounds);history.Insert(0,"激愤斩：感知豁免失败，恐慌至多1分钟；看见施法者时攻击与属性检定劣势，不能自愿靠近。无需专注。");}
        else history.Insert(0,"激愤斩：感知豁免成功，不陷入恐慌，附加暗蚀伤害不减半。");
    }
    IEnumerator RepeatWrathfulSave(WrathfulSmiteEffects.Entry effect)
    {
        if(effect==null||!BanishLiving(effect.Target)||!wrathfulSmite.Pending(effect.Target,worldRounds).Contains(effect))yield break;
        var failed=new HashSet<int>();yield return RollMappedAbilitySave(effect.Target,0,effect.Dc,failed,"激愤斩 #"+effect.Id,3,avoidsFrightened:true);
        if(!failed.Contains(effect.Target)){wrathfulSmite.End(effect.Id);history.Insert(0,"激愤斩 #"+effect.Id+"：回合末感知豁免成功，结束该次恐慌。");}
    }
}
