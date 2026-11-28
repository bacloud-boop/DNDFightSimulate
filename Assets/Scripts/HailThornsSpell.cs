using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class HailHit {public int Slot;public long Dc;public Vector2Int Origin;public int Spaces;public bool Before;public readonly List<int> Targets=new List<int>();}
    bool hailPromptPending,hailBefore;int hailChoice,hailSlot=1;
    bool HailReady=>classes[2]>0&&terrainActor==0&&!summonTurns.Active&&combat.PlayerCanAct&&combat.BonusActionAvailable&&playerResurrectionCaster.CanCast&&CanCastInCurrentForm&&ArmorSpellAllowed("Hail of Thorns")&&SpellVoiceAllowed(true,"Hail of Thorns");
    void DrawHailPrompt()
    {
        Surface(new Rect(420,190,760,520),paperTexture);GUILayout.BeginArea(new Rect(450,220,700,460));GUILayout.Label("远程武器命中 · 荆棘之雨",heading);
        GUILayout.Label("消耗附赠动作；目标和周围5尺内所有生物（包括玩家和盟友）进行敏捷豁免，每环1d10穿刺，成功半伤。原攻击暴击不翻倍这组法术骰。无需专注。",small);
        var slots=new string[9];for(int i=0;i<9;i++)slots[i]=(i+1).ToString();hailSlot=GUILayout.SelectionGrid(hailSlot-1,slots,9,button)+1;
        hailBefore=GUILayout.SelectionGrid(hailBefore?1:0,new[]{"先原武器伤害，再荆棘之雨","先荆棘之雨，再原武器伤害"},1,button)==1;
        if(GUILayout.Button("施放（附赠动作）",button))hailChoice=1;if(GUILayout.Button("跳过，继续原攻击",button))hailChoice=2;GUILayout.EndArea();
    }
    IEnumerator OfferHailThorns(int target,HailHit result)
    {
        if(!HailReady)yield break;
        hailChoice=0;hailPromptPending=true;try{while(hailChoice==0)yield return null;}finally{hailPromptPending=false;}
        if(hailChoice!=1||!HailReady||!combat.TryUseBonusAction())yield break;
        Vector2Int origin;int spaces;string name;if(!HealingHandsTarget(target,out origin,out spaces,out name))yield break;
        int slot=hailSlot;bool before=hailBefore;spellCastSequence++;PlayerCastWhileSubmerged("Hail of Thorns");
        var stopped=new SpellInterruption();yield return TryInterruptSpell(true,"Hail of Thorns",stopped);
        if(stopped.Cancelled){history.Insert(0,"荆棘之雨被反制，附赠动作已消耗，原武器命中继续。");yield break;}
        result.Slot=slot;result.Dc=8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(2)]);result.Origin=origin;result.Spaces=spaces;result.Before=before;
        bool inside=map.IsContained(true)||map.IsContained(false);
        foreach(int actor in BreathActors()){
            Vector2Int p;int n;if(map.SceneOf(actor)!=map.SceneOf(target)||!HealingHandsTarget(actor,out p,out n,out name))continue;
            if(inside){if(actor==0||actor==-1)result.Targets.Add(actor);continue;}
            if((actor==0||actor==-1)&&map.IsContained(actor==0))continue;
            if(BattleMap.BodyDistance(origin,spaces,p,n)<=5)result.Targets.Add(actor);
        }
        history.Insert(0,"荆棘之雨已施放："+(before?"先法术伤害，再武器伤害":"先武器伤害，再法术伤害")+"；命中处与范围目标已记录。");
    }
    IEnumerator ResolveHailThorns(HailHit hit)
    {
        if(hit.Slot<1)yield break;
        int sum=0;var rolls=new int[hit.Slot];for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,11);sum+=rolls[i];}
        yield return PresentRoll(10,rolls,"荆棘之雨："+hit.Slot+"d10 = "+sum+" 穿刺；不因原攻击暴击翻倍");
        var reduced=new RayDamage();yield return ReduceRayDamage(true,sum,"穿刺","荆棘之雨",reduced);
        int self=0;outcomeDeferDepth++;
        try{foreach(int actor in hit.Targets){Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))continue;
            var failed=new HashSet<int>();yield return RollMappedAbilitySave(actor,0,hit.Dc,failed,"荆棘之雨",1,hit.Origin,hit.Spaces);
            int amount=failed.Contains(actor)?reduced.Amount:reduced.Amount/2;int actual=ApplyMappedAreaDamage(actor,0,amount,1,"荆棘之雨");if(actor==0)self=actual;
        }yield return MaintainConcentration(self);RefreshBestialSpirit();ReconcileFallenRescue();}finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
}
