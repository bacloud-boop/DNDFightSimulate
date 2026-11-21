using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly FeignDeathEffect feignDeath=new FeignDeathEffect();int feignTarget;bool feignWilling;
    bool FeignLiving(int actor)=>actor<=0||FindSummon(actor)?.Alive==true;
    bool Feigning(int actor)=>feignDeath.Active(actor,worldRounds);
    void SyncFeignDeath()
    {
        combat.PlayerFeignDeath=Feigning(0);combat.MonsterFeignDeath=Feigning(-1);rescueCarrier.FeignDeath=Feigning(BattleMap.RescueId);
        foreach(var unit in map.Summons){unit.SetFeignDeath(Feigning(unit.Id),feignDeath.Blind(unit.Id,worldRounds));}
    }
    string FeignDeathError()
    {
        if(!feignWilling)return "请确认目标自愿接受假死术。";
        if(worldRounds<0||worldRounds>long.MaxValue-600)return "世界时间超出可用范围。";
        Vector2Int cell;int spaces;string name;if(!FeignLiving(feignTarget)||!HealingHandsTarget(feignTarget,out cell,out spaces,out name))return "请选择仍存活的现身体，遗体不能接受假死术。";
        return HealingHandsCanTouch(0,feignTarget)?"":"假死术需要接触：目标不在5尺可触及范围。";
    }
    void DrawFeignDeathChoices()
    {
        var ids=new List<int>{0,-1};if(map.RescuePosition.HasValue)ids.Add(BattleMap.RescueId);foreach(var unit in map.Summons)if(unit.Alive)ids.Add(unit.Id);
        foreach(int id in ids){Vector2Int p;int n;string name;if(HealingHandsTarget(id,out p,out n,out name)&&GUILayout.Button((feignTarget==id?"✓ ":"")+name,button))feignTarget=id;}
        feignWilling=GUILayout.Toggle(feignWilling,"确认目标自愿接受假死术",label);
        GUILayout.Label("目盲、失能、速度0，但不等于麻痹或昏迷。除心灵外全部伤害抗性，中毒状态免疫。木桩忽略假死者；真实死亡与复活判断不变。",small);
    }
    void ApplyFeignDeath(int target)
    {
        Vector2Int p;int n;string name;if(!FeignLiving(target)||!HealingHandsTarget(target,out p,out n,out name)||!HealingHandsCanTouch(0,target)||!feignDeath.Cast(target,worldRounds))return;
        SyncFeignDeath();if(target==0){fighter.CancelAttackAction();actionWeapon=null;RefreshConcentration();RefreshWildShape();}
        SyncTimedBuffs();ReconcileFallenRescue();dice="假死术 · "+name;
        history.Insert(0,"假死术："+name+"自愿进入非专注1小时假死，目盲、失能、速度0；除心灵外伤害抗性，中毒状态免疫。仍存活，不触发死亡记录、胜负或复活入口；战后可按现有规则搬运。");
    }
    void DrawFeignStatus(Rect board)
    {
        foreach(int id in BreathActors())if(Feigning(id)){Vector2Int p;int n;string name;if(HealingHandsTarget(id,out p,out n,out name)){var r=BodyRect(board,p,n);GUI.Label(new Rect(r.x,r.y-18,180,20),name+" · 假死（仍存活）",small);}}
    }
}
