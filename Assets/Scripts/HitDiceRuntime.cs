using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly HitDicePool monsterHitDice=new HitDicePool(),rescueHitDice=new HitDicePool();
    readonly Dictionary<int,HitDicePool> summonHitDice=new Dictionary<int,HitDicePool>();
    readonly HashSet<int> prayerUsed=new HashSet<int>();
    readonly Dictionary<int,long> restDamageVersions=new Dictionary<int,long>();
    long RestDamageVersion(int actor){long n;return restDamageVersions.TryGetValue(actor,out n)?n:0;}
    void NoteRestDamage(int actor,int amount){if(amount>0)restDamageVersions[actor]=unchecked(RestDamageVersion(actor)+1);}
    readonly Dictionary<int,long> summonLastLongRest=new Dictionary<int,long>();
    sealed class HitDiceDraft {public string Count="1";public int Face=2;}
    readonly Dictionary<string,HitDiceDraft> summonHitDiceDrafts=new Dictionary<string,HitDiceDraft>();
    string monsterHitDiceInput="1",rescueHitDiceInput="1",originalHitDiceInput="1";
    int monsterHitDieFace=2,rescueHitDieFace=2;
    HitDicePool ActorHitDice(int actor)
    {
        if(actor==-1)return monsterHitDice;if(actor==BattleMap.RescueId)return rescueHitDice;
        if(actor<=0||FindSummon(actor)==null)return null;HitDicePool pool;
        if(!summonHitDice.TryGetValue(actor,out pool)){pool=new HitDicePool();if(Undead(FindSummon(actor))!=null)pool.Configure(2,8);summonHitDice.Add(actor,pool);}return pool;
    }
    void DrawHitDiceFields(ref string count,ref int face)
    {
        GUILayout.Label("生命骰数量（非负整数；不改变HP上限）",small);count=IntegerField(count,false);
        face=GUILayout.SelectionGrid(face,new[]{"d4","d6","d8","d10","d12","d20"},3,button);
    }
    bool ValidHitDiceInput(string input,int face,out int count)=>int.TryParse(input,out count)&&face>=0&&face<HitDicePool.Faces.Length&&count>=0;
    HitDiceDraft SummonHitDiceDraft(string name){HitDiceDraft draft;if(!summonHitDiceDrafts.TryGetValue(name,out draft)){draft=new HitDiceDraft{Count=name=="Animate Dead"?"2":"1"};summonHitDiceDrafts.Add(name,draft);}return draft;}
    void DrawSummonHitDiceChoices(string name){var draft=SummonHitDiceDraft(name);DrawHitDiceFields(ref draft.Count,ref draft.Face);int n;if(!ValidHitDiceInput(draft.Count,draft.Face,out n))GUILayout.Label("生命骰配置无效，请填写非负整数。",small);}
    bool SummonHitDiceValid(string name){var draft=SummonHitDiceDraft(name);int n;return ValidHitDiceInput(draft.Count,draft.Face,out n);}
    void ConfigureSummonHitDice(int actor,string name){var draft=SummonHitDiceDraft(name);int n;if(ValidHitDiceInput(draft.Count,draft.Face,out n))ActorHitDice(actor)?.Configure(n,HitDicePool.Faces[draft.Face]);}
    bool HitDiceActorAlive(int actor)=>actor==0?!combat.PlayerDead:actor==-1?combat.Hp>0:actor==BattleMap.RescueId?!rescueCarrier.Dead:FindSummon(actor)?.Alive==true;
    int HitDiceConstitution(int actor)=>actor==0?EffectivePlayerAbility(2):actor==-1?EffectiveMonsterAbility(2):actor==BattleMap.RescueId?rescueCarrier.Ability(2):SummonAbility(FindSummon(actor),2);
    void GrantShortRest(int actor)
    {
        if(actor==0){fighter.ShortRest();druid.ShortRest();playerSpeciesAbilities.ShortRest();shortRestReady=true;RefreshWildShape();}
        else {ActorHitDice(actor)?.ShortRest();if(actor==-1)monsterSpeciesAbilities.ShortRest();else if(actor==BattleMap.RescueId)rescueCarrier.SpeciesAbilities.ShortRest();}
    }
    void FinishActorLongRest(int actor){prayerUsed.Remove(actor);ActorHitDice(actor)?.LongRest();}
    void ResetNonPlayerHitDice(){prayerChannel=null;prayerUsed.Clear();monsterHitDice.LongRest();rescueHitDice.LongRest();summonHitDice.Clear();summonLastLongRest.Clear();restDamageVersions.Clear();}
    void DrawSummonRestControls(int actor)
    {
        var unit=FindSummon(actor);if(unit==null||!unit.Alive)return;bool old=GUI.enabled;GUI.enabled=old&&OutsideCombat&&combat.Aftermath;
        if(GUILayout.Button(TransitActorName(actor)+"短休（1小时）",button))StartCoroutine(RestSummonAftermath(actor,false));
        long last;GUI.enabled=GUI.enabled&&(!summonLastLongRest.TryGetValue(actor,out last)||worldRounds-last>=14400);
        if(GUILayout.Button(TransitActorName(actor)+"长休（8小时）",button))StartCoroutine(RestSummonAftermath(actor,true));GUI.enabled=old;
    }
    IEnumerator RestSummonAftermath(int actor,bool longRest)
    {
        var unit=FindSummon(actor);if(unit==null||!unit.Alive||!combat.Aftermath||!OutsideCombat)yield break;long last,start=worldRounds,damageVersion=RestDamageVersion(actor);if(longRest&&summonLastLongRest.TryGetValue(actor,out last)&&start-last<14400)yield break;
        yield return AdvanceAftermathRounds(longRest?4800:600,()=>FindSummon(actor)==unit&&unit.Alive&&RestDamageVersion(actor)==damageVersion);
        if(!aftermathAdvanceCompleted||FindSummon(actor)!=unit||!unit.Alive||RestDamageVersion(actor)!=damageVersion)yield break;
        if(longRest){summonLastLongRest[actor]=start;FinishActorLongRest(actor);unit.Heal(int.MaxValue);if(unit==steed)steedRules.LongRest();}else GrantShortRest(actor);
        history.Insert(0,unit.Name+(longRest?"完成长休，恢复HP及生命骰。":"获得短休收益，可逐颗使用生命骰。"));
    }
    void DrawActorRestDice(int actor)
    {
        var pool=ActorHitDice(actor);if(pool==null)return;
        GUILayout.Label(TransitActorName(actor)+" · 生命骰 "+pool.Remaining+" / "+pool.Count+" d"+pool.Sides,small);
        bool old=GUI.enabled;GUI.enabled=old&&!rolling&&pool.RestReady&&HitDiceActorAlive(actor)&&OutsideCombat;
        if(GUILayout.Button("消耗1颗生命骰恢复HP（免费）",button))StartCoroutine(SpendActorRestDie(actor));GUI.enabled=old;
    }
    IEnumerator SpendActorRestDie(int actor)
    {
        var pool=ActorHitDice(actor);if(pool==null||!HitDiceActorAlive(actor)||!pool.Spend())yield break;
        bool busy=rolling;rolling=true;try{
            int die=UnityEngine.Random.Range(1,pool.Sides+1),modifier=AcidSplash.Modifier(HitDiceConstitution(actor)),amount=HitDicePool.Healing(die,modifier);
            dice=TransitActorName(actor)+" · 短休生命骰";yield return PresentRoll(pool.Sides,new[]{die},die+" + 体质 "+modifier+" = "+amount+"（最低1）");
            if(!HitDiceActorAlive(actor))yield break;
            int actual=HealRegenerationTarget(actor,RollHealingAmount(actor,amount,HitDicePool.Healing(pool.Sides,modifier)));
            history.Insert(0,TransitActorName(actor)+"消耗1颗d"+pool.Sides+"生命骰，实际恢复"+actual+"HP；剩余"+pool.Remaining+"。");ObserveDeaths();
        }finally{rolling=busy;}
    }
}
