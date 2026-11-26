using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly SearingSmiteEffects searingSmite=new SearingSmiteEffects();
    sealed class SmiteHitResult {public int Fire,Radiant;}
    bool searingPromptPending;int searingPromptChoice,searingSlot=1,smiteKind;
    string OnHitSmiteName=>smiteKind==1?"Divine Smite":"Searing Smite";
    bool SearingSmiteReady=>classes[9]>0&&terrainActor==0&&!summonTurns.Active&&combat.PlayerCanAct&&combat.BonusActionAvailable&&CanCastInCurrentForm&&ArmorSpellAllowed(OnHitSmiteName)&&SpellVoiceAllowed(true,OnHitSmiteName)&&(smiteKind==1||searingSmite.CanAdd(worldRounds));
    void DrawSearingSmiteInfo()
    {
        GUILayout.Label("仅在自身回合使用近战武器或徒手／自然攻击命中后，立即选择是否消耗附赠动作施放。不能直接从法术书施放。初始伤害并入该次命中，持续火焰每目标回合开始先伤害、后体质豁免，成功结束。",small);
    }
    void DrawSearingSmitePrompt()
    {
        Surface(new Rect(440,170,720,545),paperTexture);GUILayout.BeginArea(new Rect(470,200,660,485));
        GUILayout.Label("近战命中 · 选择斩击法术",heading);
        smiteKind=GUILayout.SelectionGrid(smiteKind,new[]{"炽焰斩","神圣斩"},2,button);
        GUILayout.Label(smiteKind==1?"神圣斩：一环2d8光耀，每升环＋1d8；目标为邪魔或亡灵另＋1d8，暴击翻倍所有附加骰。即时，不留下持续伤害。":"炽焰斩：初始每环1d6火焰（暴击翻倍），后续每目标回合开始每环1d6火焰，体质豁免成功结束。",label);
        searingSlot=GUILayout.SelectionGrid(searingSlot-1,new[]{"1","2","3","4","5","6","7","8","9"},9,button)+1;
        if(GUILayout.Button("施放所选法术（附赠动作）",button))searingPromptChoice=1;
        if(GUILayout.Button("跳过，继续原攻击",button))searingPromptChoice=2;
        GUILayout.EndArea();
    }
    IEnumerator OfferSearingSmite(int target,bool critical,SmiteHitResult result)
    {
        if(!SearingSmiteReady)yield break;
        searingPromptChoice=0;searingPromptPending=true;
        try{while(searingPromptChoice==0)yield return null;}finally{searingPromptPending=false;}
        if(searingPromptChoice!=1||!SearingSmiteReady||!combat.TryUseBonusAction())yield break;
        int slot=searingSlot,kind=smiteKind;string spell=OnHitSmiteName;spellCastSequence++;PlayerCastWhileSubmerged(spell);
        var stopped=new SpellInterruption();yield return TryInterruptSpell(true,spell,stopped);
        if(stopped.Cancelled){history.Insert(0,(kind==1?"神圣斩":"炽焰斩")+"被反制，附赠动作已消耗；原武器命中继续结算。");yield break;}
        if(kind==1){
            int divineCount=DivineSmiteRules.Dice(slot,BanishType(target),critical),divineSum=0;var divineRolls=new int[divineCount];
            for(int i=0;i<divineCount;i++){divineRolls[i]=UnityEngine.Random.Range(1,9);divineSum+=divineRolls[i];}
            yield return PresentRoll(8,divineRolls,"神圣斩："+divineCount+"d8 = "+divineSum+" 光耀，合并至本次命中；邪魔／亡灵额外骰也受暴击翻倍");result.Radiant=divineSum;yield break;
        }
        long dc=8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(9)]);
        if(searingSmite.Add(target,slot,dc,worldRounds)==null)yield break;
        int count=SearingSmiteEffects.Dice(slot,critical);var rolls=new int[count];int sum=0;
        for(int i=0;i<count;i++){rolls[i]=UnityEngine.Random.Range(1,7);sum+=rolls[i];}
        yield return PresentRoll(6,rolls,"炽焰斩初始："+count+"d6 = "+sum+" 火焰，合并至本次命中；后续伤害不继承暴击");result.Fire=sum;
    }
    IEnumerator ResolveSearingStart(int target)
    {
        var active=searingSmite.Strongest(target,worldRounds);if(active==null||!BanishLiving(target))yield break;
        int sum=0;var rolls=new int[active.Dice];for(int i=0;i<rolls.Length;i++){rolls[i]=UnityEngine.Random.Range(1,7);sum+=rolls[i];}
        yield return PresentRoll(6,rolls,"炽焰斩持续："+active.Dice+"d6 = "+sum+" 火焰；同名效果仅最强者造成伤害");
        var reduced=new RayDamage();yield return ReduceRayDamage(true,sum,"火焰","炽焰斩持续伤害",reduced);
        var raw=new long[13];raw[5]=reduced.Amount;outcomeDeferDepth++;
        try{
            int amount=ApplyMappedDamagePacket(target,0,raw);yield return MaintainConcentration(target==0?amount:0);
            if(!BanishLiving(target))yield break;
            foreach(var entry in searingSmite.Pending(target,worldRounds)){
                var failed=new HashSet<int>();yield return RollMappedAbilitySave(target,0,entry.Dc,failed,"炽焰斩 #"+entry.Id,2);
                if(!failed.Contains(target)){searingSmite.End(entry.Id);history.Insert(0,"炽焰斩 #"+entry.Id+"：体质豁免成功，该次施法结束。");}
            }
            RefreshBestialSpirit();ReconcileFallenRescue();
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
}
