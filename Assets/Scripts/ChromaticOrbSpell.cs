using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    int orbTarget=-1,orbType,orbBounceTarget;
    bool orbBouncePending,orbBounceReady,orbToEnvironment;
    readonly List<int> orbBounceOptions=new List<int>();
    Vector2 orbBounceScroll;
    bool OrbTargetAllowed(int target,Vector2Int origin,int spaces,int range,HashSet<int> used=null)
    {
        Vector2Int p;int n;string name;
        return target!=BattleMap.RescueId&&(used==null||!used.Contains(target))&&map.SameScene(0,target)&&!AcidArrowIsolated(0,target)&&SummonTarget(target,out p,out n,out name)&&BattleMap.BodyDistance(origin,spaces,p,n)<=range;
    }
    string ChromaticOrbError()=>OrbTargetAllowed(orbTarget,map.Player,map.PlayerSpaces,90)?"":"首目标须在90尺内、仍在同一位面且不隔着吞咽边界。";
    void DrawChromaticOrbChoices()
    {
        orbType=GUILayout.SelectionGrid(orbType,new[]{"强酸","寒冷","火焰","闪电","毒素","雷鸣"},3,button);
        foreach(int actor in BreathActors())if(actor!=BattleMap.RescueId&&GUILayout.Button((orbTarget==actor?"✓ ":"")+TransitActorName(actor)+(OrbTargetAllowed(actor,map.Player,map.PlayerSpaces,90)?"":"（当前不可选）"),button))orbTarget=actor;
        GUILayout.Label("每次命中造成"+ChromaticOrbRules.Dice(directSlotLevel)+"d8所选伤害，不加施法调整值。该次法术d8出现重复骰值时，选择前一目标30尺内的新目标；最多弹射等同环阶次数，每个生物每次施法仅能成为目标一次。可选“射向环境”直接结束，不结算环境伤害。",small);
    }
    void DrawOrbBounce()
    {
        Surface(new Rect(440,140,720,620),paperTexture);GUILayout.BeginArea(new Rect(470,170,660,560));
        GUILayout.Label("繁彩球 · 选择弹射目标",heading);GUILayout.Label("本次d8出现相同骰值。以下目标距离前一目标不超过30尺，且本次施法尚未被选中。无需额外动作、施法或资源，伤害类型沿用初始选择。",small);
        orbBounceScroll=GUILayout.BeginScrollView(orbBounceScroll);
        foreach(int actor in orbBounceOptions)if(GUILayout.Button(TransitActorName(actor),button)){orbBounceTarget=actor;orbToEnvironment=false;orbBounceReady=true;}
        if(GUILayout.Button("射向环境 · 结束本次弹射链",button)){orbToEnvironment=true;orbBounceReady=true;}
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
    IEnumerator ResolveChromaticOrb(int caster,int slot,int target,int type)
    {
        if(ChromaticOrbRules.Dice(slot)==0||Array.IndexOf(ChromaticOrbRules.Types,type)<0||!OrbTargetAllowed(target,map.Player,map.PlayerSpaces,90))yield break;
        var used=new HashSet<int>();Vector2Int origin=map.Player;int spaces=map.PlayerSpaces,previous=0,leaps=0;
        outcomeDeferDepth++;
        try{while(true){
            if(!PlayerCanSustainCast||combat.PlayerBanished||!OrbTargetAllowed(target,origin,spaces,used.Count==0?90:30,used))yield break;
            Vector2Int impact;int impactSpaces;string name;if(!SummonTarget(target,out impact,out impactSpaces,out name))yield break;
            used.Add(target);var hit=new SpellAttackResult();
            if(used.Count==1)yield return RollMappedSpellAttack(0,target,caster,"繁彩球",hit);
            else yield return RollMappedSpellAttack(0,target,caster,"繁彩球 · 弹射"+leaps,hit,false,origin,spaces,previous);
            if(!hit.Hit){history.Insert(0,"繁彩球："+(hit.Intercepted?"镜像拦截":"未命中")+"，伤害0，弹射链结束。");yield break;}
            int count=ChromaticOrbRules.Dice(slot,hit.Critical),sum=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=UnityEngine.Random.Range(1,9);sum+=rolls[i];}
            bool duplicate=ChromaticOrbRules.Duplicates(rolls);
            yield return PresentRoll(8,rolls,"繁彩球："+count+"d8 = "+sum+" "+DamageTypes.Names[type]+(duplicate?"；出现重复骰值":"；没有重复骰值"));
            var raw=new long[13];raw[type]=sum;
            if(target==0||target==-1){var extra=new ExtraMarkDamage();yield return RollMarkDamage(target==0,hit.Critical,extra);if(extra.Raw>0)raw[mark.DamageType]+=extra.Raw;}
            var reduced=new RayDamage();yield return ReduceRayPacket(true,raw,true,"繁彩球",reduced);
            int amount=ApplyMappedDamagePacket(target,0,reduced.Raw,hit.Critical);history.Insert(0,"繁彩球对"+name+"造成 "+amount+" 点结算伤害。");
            yield return MaintainConcentration(target==0?amount:0);RefreshUndeadFollowers();RefreshBestialSpirit();ReconcileFallenRescue();
            if(!PlayerCanSustainCast||combat.PlayerBanished||!ChromaticOrbRules.CanLeap(slot,leaps,rolls))yield break;
            orbBounceOptions.Clear();foreach(int actor in BreathActors())if(OrbTargetAllowed(actor,impact,impactSpaces,30,used))orbBounceOptions.Add(actor);
            if(orbBounceOptions.Count==0){history.Insert(0,"繁彩球没有合法生物可继续弹射，射向环境结束；不结算环境伤害。");yield break;}
            orbBounceReady=false;orbToEnvironment=false;orbBouncePending=true;
            try{while(!orbBounceReady)yield return null;}finally{orbBouncePending=false;}
            if(orbToEnvironment){history.Insert(0,"繁彩球射向环境，本次施法结束；不结算环境伤害或继续弹射。");yield break;}
            previous=target;origin=impact;spaces=impactSpaces;target=orbBounceTarget;leaps++;
        }}finally{orbBouncePending=false;orbBounceOptions.Clear();outcomeDeferDepth--;CheckBattleOutcome();}
    }
}
