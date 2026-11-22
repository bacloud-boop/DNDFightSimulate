using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class HellishDamage {public int Source,Amount;}
    readonly Queue<HellishDamage> hellishDamage=new Queue<HellishDamage>();
    bool hellishPending,drainingHellish;int hellishTarget,hellishDecision,hellishCaster=4,hellishSlot=1;
    bool CanHellishCast=>combat.CanPlayerDamageReact&&CanCastInCurrentForm&&ArmorSpellAllowed(HellishRebukeRules.Name);
    bool HellishTarget(int actor,out int distance,out bool sees)
    {
        distance=0;sees=false;Vector2Int p;int n;string name;
        if(!HealingHandsTarget(actor,out p,out n,out name))return false;
        if(actor==-1&&map.IsContained(false)||map.IsContained(true)&&actor!=0&&actor!=-1)return false;
        distance=BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n);
        sees=actor==0?CanSeeOwnBody():actor==-1?PlayerSeesMonster:VisibilityAt(true,p,n)<2;
        return true;
    }
    int[] HellishCasters()=>EligibleCasters(SpellInfo(HellishRebukeRules.Name)).ToArray();
    void QueueHellishDamage(bool player,int? source,int amount)
    {
        if(!player||amount<=0||!source.HasValue||page!="battle"||!map.Locked||combat.PlayerHp<=0||combat.PlayerDead||!combat.PlayerReactionAvailable||HellishCasters().Length==0)return;
        hellishDamage.Enqueue(new HellishDamage{Source=source.Value,Amount=amount});
    }
    void ResetHellishRebuke(){hellishDamage.Clear();hellishPending=false;}
    IEnumerator DrainHellishRebukes()
    {
        if(drainingHellish)yield break;
        drainingHellish=true;bool busy=rolling;string oldPanel=panel;rolling=true;panel="";
        try{
            while(hellishDamage.Count>0){
                var incoming=hellishDamage.Dequeue();int target=incoming.Source,distance;bool sees;
                // Settle transformations ended by this damage before checking
                // whether the surviving character can cast a reaction spell.
                RefreshPolymorph();RefreshWildShape();RefreshConcentration();SyncTimedBuffs();
                if(!HellishTarget(target,out distance,out sees)||!HellishRebukeRules.CanTrigger(incoming.Amount,target,distance,sees,CanHellishCast))continue;
                int[] casters=HellishCasters();if(casters.Length==0)continue;
                if(System.Array.IndexOf(casters,hellishCaster)<0)hellishCaster=casters[0];
                hellishTarget=target;hellishSlot=Mathf.Clamp(hellishSlot,1,9);hellishDecision=0;hellishPending=true;
                while(hellishDecision==0)yield return null;hellishPending=false;
                if(hellishDecision!=1){history.Insert(0,"玩家放弃炼狱叱喝，保留反应。");continue;}
                if(!CanHellishCast||SpeciesSpellCastError(HellishRebukeRules.Name,hellishCaster,hellishSlot)!=""||!HellishTarget(target,out distance,out sees)||distance>60||!sees||!combat.TryUseDamageReaction())continue;
                int caster=hellishCaster,slot=hellishSlot;
                ConsumeSpeciesSpell(HellishRebukeRules.Name,caster);PlayerCastWhileSubmerged(HellishRebukeRules.Name);
                var interruption=new SpellInterruption();yield return TryInterruptSpell(true,HellishRebukeRules.Name,interruption);
                BreakOrdinaryInvisibility(true,"施放炼狱叱喝");
                if(interruption.Cancelled)continue;
                if(!HellishTarget(target,out distance,out sees))continue;
                long dc=8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
                var failed=new HashSet<int>();yield return RollMappedDexteritySave(target,0,dc,failed,"炼狱叱喝");
                int count=HellishRebukeRules.DiceCount(slot),raw=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=Random.Range(1,11);raw+=rolls[i];}
                dice="炼狱叱喝 · 火焰伤害";yield return PresentRoll(10,rolls,count+"d10 = "+raw+"，豁免成功减半");
                var reduced=new RayDamage();yield return ReduceRayDamage(true,raw,"火焰","炼狱叱喝",reduced);
                int actual=ApplyMappedAreaDamage(target,0,failed.Contains(target)?reduced.Amount:reduced.Amount/2,5,"炼狱叱喝",failed.Contains(target));
                if(target==0)yield return MaintainConcentration(actual);
                ReconcileFallenRescue();RefreshBestialSpirit();CheckBattleOutcome();
            }
        }finally{hellishPending=false;drainingHellish=false;rolling=busy;panel=oldPanel;}
    }
    void DrawHellishRebukePrompt()
    {
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.94f));Surface(new Rect(430,140,740,650),paperTexture);
        GUILayout.BeginArea(new Rect(460,165,680,600));GUILayout.Label("炼狱叱喝 · 反应施法",heading);
        GUILayout.Label(TransitActorName(hellishTarget)+"对玩家造成伤害。是否消耗反应，使伤害来源进行敏捷豁免？成功伤害减半。法术位资源暂不限；守护之链转移不触发。",label);
        foreach(int caster in HellishCasters())if(GUILayout.Button((hellishCaster==caster?"✓ ":"")+CasterLabel(caster),button))hellishCaster=caster;
        if(hellishCaster==SpeciesSpellRules.Caster){var grant=PlayerSpeciesSpell(HellishRebukeRules.Name);if(grant!=null){useSpeciesFreeCast=GUILayout.Toggle(useSpeciesFreeCast,"使用血统免费次数（剩余 "+playerSpeciesSpellUses.Remaining(grant,classes.ProficiencyBonus)+"）",label);if(useSpeciesFreeCast)hellishSlot=grant.SpellLevel;}}
        bool previous=GUI.enabled;GUI.enabled=previous&&!(hellishCaster==SpeciesSpellRules.Caster&&useSpeciesFreeCast);
        hellishSlot=GUILayout.SelectionGrid(hellishSlot-1,new[]{"1","2","3","4","5","6","7","8","9"},9,button)+1;GUI.enabled=previous;
        GUILayout.Label(HellishRebukeRules.DiceCount(hellishSlot)+"d10火焰 · 敏捷DC "+(8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(hellishCaster)])),small);
        string error=SpeciesSpellCastError(HellishRebukeRules.Name,hellishCaster,hellishSlot);if(error!="")GUILayout.Label(error,small);
        GUI.enabled=previous&&error=="";if(GUILayout.Button("施放炼狱叱喝 · 消耗反应",button))hellishDecision=1;
        GUI.enabled=previous;if(GUILayout.Button("放弃，保留反应",button))hellishDecision=2;
        GUILayout.EndArea();
    }
}
