using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly HeroismEffect heroism=new HeroismEffect();
    readonly HashSet<int> heroismTargets=new HashSet<int>{0};
    bool heroismWilling,heroismOrderPending,heroismFirst;int heroismOrderChoice;
    sealed class HeroismEntry {public int Actor;public long Revision;}
    readonly Queue<HeroismEntry> heroismEntries=new Queue<HeroismEntry>();
    bool HeroismImmune(int actor)=>heroism.Active(actor,worldRounds);
    void EndHeroism(){heroism.Clear();heroismEntries.Clear();}
    string HeroismError(int slot)
    {
        if(!heroismWilling)return "请确认所有所选目标自愿接受英雄气概。";
        if(worldRounds<0||worldRounds>long.MaxValue-10)return "世界时间超出范围。";
        if(heroismTargets.Count<1||heroismTargets.Count>slot)return "请选择1～"+slot+"名不同目标。";
        foreach(int id in heroismTargets)if(!HealingHandsCanTouch(0,id))return TransitActorName(id)+"必须存活且处于5尺可接触范围。";
        return "";
    }
    void DrawHeroismChoices()
    {
        heroismTargets.RemoveWhere(id=>{Vector2Int p;int n;string name;return !HealingHandsTarget(id,out p,out n,out name);});
        foreach(int id in BreathActors()){
            Vector2Int p;int n;string name;if(!HealingHandsTarget(id,out p,out n,out name))continue;
            bool chosen=GUILayout.Toggle(heroismTargets.Contains(id),name,label);if(chosen)heroismTargets.Add(id);else heroismTargets.Remove(id);
        }
        heroismWilling=GUILayout.Toggle(heroismWilling,"确认所有目标自愿接受法术",label);
        GUILayout.Label("接触时获得恐慌免疫；临时生命从目标下次回合开始获得，等于本次施法调整值，不相加、不治疗或复活。法术结束停止继续授予，已有临时生命保留。",small);
    }
    void ApplyHeroism(int caster,int slot,IEnumerable<int> selected)
    {
        var legal=new List<int>();foreach(int id in selected)if(HealingHandsCanTouch(0,id))legal.Add(id);
        if(!heroism.Begin(legal,AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),slot,worldRounds,terrainTurn))return;
        concentrationSpell="英雄气概";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;
        history.Insert(0,"英雄气概：所选自愿目标免疫恐慌；各自回合开始获得 "+heroism.Amount+" 临时生命，专注至多1分钟。施法当下不立即授予临时生命。");
    }
    void QueueHeroismStart(int actor)
    {
        if(heroism.Claim(actor,terrainTurn,worldRounds))heroismEntries.Enqueue(new HeroismEntry{Actor=actor,Revision=heroism.Revision});
    }
    IEnumerator ChooseHeroismStartOrder()
    {
        heroismFirst=true;if(heroismEntries.Count==0||greaseEntries.Count+spikeEntries.Count+webEntries.Count+daggerEntries.Count==0)yield break;
        heroismOrderChoice=0;heroismOrderPending=true;while(heroismOrderChoice==0)yield return null;heroismOrderPending=false;heroismFirst=heroismOrderChoice==1;
    }
    void DrawHeroismStartOrder()
    {
        if(!heroismOrderPending)return;Fill(new Rect(0,0,1600,900),new Color(0,0,0,.92f));Surface(new Rect(465,245,670,380),paperTexture);
        GUI.Label(new Rect(495,270,610,60),"回合开始 · 选择效果先后",heading);
        if(GUI.Button(new Rect(495,365,610,65),"先获得英雄气概临时生命",button))heroismOrderChoice=1;
        if(GUI.Button(new Rect(495,455,610,65),"先结算地形／区域效果",button))heroismOrderChoice=2;
    }
    int ActorTemporaryHp(int actor)=>actor==0?combat.PlayerTemporaryHp:actor==-1?combat.MonsterTemporaryHp:actor==BattleMap.RescueId?rescueCarrier.TemporaryHp:FindSummon(actor)?.TemporaryHp??0;
    void ChooseActorTemporaryHp(int actor,int amount,bool takeNew)
    {
        if(actor==0||actor==-1)combat.ChooseTemporaryHp(actor==0,amount,takeNew);
        else if(actor==BattleMap.RescueId)rescueCarrier.ChooseTemporaryHp(amount,takeNew);else FindSummon(actor)?.ChooseTemporaryHp(amount,takeNew);
    }
    IEnumerator ResolveHeroismStart(HeroismEntry entry)
    {
        if(entry.Revision!=heroism.Revision||!HeroismImmune(entry.Actor))yield break;
        Vector2Int p;int n;string name;if(!HealingHandsTarget(entry.Actor,out p,out n,out name))yield break;
        if(heroism.Amount<=0)yield break;
        yield return OfferActorTemporaryHp(entry.Actor,heroism.Amount,"英雄气概 · 回合开始");RefreshPolymorph();
    }
}
