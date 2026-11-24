using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly BanishmentEffect banishment=new BanishmentEffect();
    readonly HashSet<int> banishTargets=new HashSet<int>{-1};
    sealed class PlanarHome {public int Actor,Scene;public Vector2Int Cell;public bool Submerged;}
    readonly List<PlanarHome> planarHomes=new List<PlanarHome>();
    int nextDemiplane=1;bool endingBanishment;
    bool BanishLiving(int actor)=>actor==0?!combat.PlayerDead:actor==-1?combat.Hp>0:actor==BattleMap.RescueId?!rescueCarrier.Dead:FindSummon(actor)!=null;
    bool BanishIncapacitated(int actor)=>banishment.Entries.Any(e=>e.Actor==actor&&e.State==BanishmentEffect.Location.Demiplane);
    int BanishType(int actor)=>actor==0?9:actor==-1?monsterProfile.TypeIndex:actor==BattleMap.RescueId?9:FindSummon(actor)?.CreatureType??9;
    string BanishTargetError(int actor)
    {
        Vector2Int p;int n;string name;if(!BanishLiving(actor)||!map.SameScene(0,actor)||!HealingHandsTarget(actor,out p,out n,out name))return "目标不在同一场景，或已经死亡／消失。";
        if(BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n)>30)return "目标超出30尺。";
        if(actor==0&&!CanSeeOwnBody()||actor==-1&&!PlayerSeesMonster||actor>0&&VisibilityAt(true,p,n)>=2)return "放逐术必须能看见目标。";
        if(AcidArrowIsolated(0,actor))return "吞咽内外隔离，无法选择该目标。";
        if(bestialSpirit.Active&&actor==bestialSpirit.Unit.Id&&concentrationSpell=="召唤野兽")return "开始新的专注会使该野兽灵魄消失，不能选择它。";
        return "";
    }
    string BanishmentError(int slot)
    {
        if(!BanishmentEffect.ValidTime(worldRounds)||nextDemiplane==int.MaxValue)return "世界时间或场景编号超出范围。";
        if(banishment.Concentrating||planarHomes.Count>0)return "请先结束现存放逐术，再选择新目标。";
        if(banishTargets.Count<1||banishTargets.Count>BanishmentEffect.MaximumTargets(slot))return "请选择1～"+BanishmentEffect.MaximumTargets(slot)+"名目标。";
        foreach(int actor in banishTargets){string error=BanishTargetError(actor);if(error!="")return TransitActorName(actor)+"："+error;}return "";
    }
    void DrawBanishmentChoices()
    {
        GUILayout.Label("至多选择 "+BanishmentEffect.MaximumTargets(directSlotLevel)+"名可见生物；魅力豁免失败后进入无害半位面并失能。无需原生位面资料。",small);
        foreach(int actor in BreathActors())if(actor!=BattleMap.RescueId){bool chosen=GUILayout.Toggle(banishTargets.Contains(actor),TransitActorName(actor),label);if(chosen)banishTargets.Add(actor);else banishTargets.Remove(actor);}
        GUILayout.Label("提前结束专注则返回；持续满1分钟后，异怪、天界、元素、妖精及邪魔不返回。木桩被永久放逐时判定玩家胜利，木桩未死亡且不产生遗体。\n附着效果仍逐回合结算，所有目标均能正常呼吸。未被选中的体内随行者可以在半位面正常行动、脱困，并在法术结束时返回训练场。",small);
    }
    IEnumerator ResolveBanishment(int caster,int slot,List<int> chosen)
    {
        var failed=new HashSet<int>();long dc=8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        foreach(int actor in chosen)if(BanishTargetError(actor)=="")yield return RollMappedAbilitySave(actor,0,dc,failed,"放逐术",5);
        var targets=failed.Where(BanishLiving).Select(id=>new BanishmentEffect.Target(id,BanishType(id))).ToArray();
        if(targets.Length==0){history.Insert(0,"放逐术：所有有效目标均通过豁免，没有目标离场。");yield break;}
        if(!banishment.Begin(caster,slot,worldRounds,targets))yield break;
        int scene=nextDemiplane++;var travelers=new HashSet<int>(targets.Select(t=>t.Actor));
        if(map.HasContainedCreature){int host=map.IsContained(true)?-1:0,passenger=host==0?-1:0;if(travelers.Contains(host))travelers.Add(passenger);}
        if(map.Riding&&travelers.Contains(0)!=travelers.Contains(map.RiderMountId))map.SeparatePlanarRider();
        foreach(int actor in travelers){Vector2Int p;int n;if(!map.SceneBody(actor,out p,out n))continue;
            bool underwater=actor==0?map.PlayerAquatic.Submerged:actor==-1?map.MonsterAquatic.Submerged:FindSummon(actor)?.Aquatic.Submerged??false;
            planarHomes.Add(new PlanarHome{Actor=actor,Scene=map.SceneOf(actor),Cell=p,Submerged=underwater});map.SetActorScene(actor,scene);
        }
        concentrationSpell="放逐术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=banishment.Expires;
        SyncBanishmentFlags();map.ViewActorScene(0);ObserveWaterBreathing();SyncTimedBuffs();
        history.Insert(0,"放逐术："+string.Join("、",targets.Select(t=>TransitActorName(t.Actor)).ToArray())+"进入临时半位面并失能，至多1分钟；未被选中的随行者仍可行动。");
        RefreshConcentration();
    }
    void SyncBanishmentFlags()
    {
        combat.PlayerBanished=BanishIncapacitated(0);combat.MonsterBanished=BanishIncapacitated(-1);combat.MonsterDeparted=banishment.PermanentlyDeparted(-1);
        foreach(var unit in map.Summons)unit.Banished=BanishIncapacitated(unit.Id)||map.SceneOf(unit.Id)<0;
    }
    void EndBanishment()
    {
        if(endingBanishment||!banishment.Concentrating)return;endingBanishment=true;
        try{
            banishment.End(worldRounds,BanishLiving);
            bool playerLeaves=banishment.PermanentlyDeparted(0),monsterLeaves=banishment.PermanentlyDeparted(-1);
            if(map.HasContainedCreature&&playerLeaves!=monsterLeaves)map.SeparatePlanarContainment();
            foreach(var home in planarHomes){
                if(banishment.PermanentlyDeparted(home.Actor)){map.SetActorScene(home.Actor,-1);acidArrows.RemoveTarget(home.Actor);history.Insert(0,TransitActorName(home.Actor)+"被永久放逐离场，未死亡，也不生成遗体。");continue;}
                bool shared;if(map.RestoreSceneBody(home.Actor,home.Scene,home.Cell,out shared)){
                    map.RestoreSceneWater(home.Actor,home.Submerged);banishment.FinishReturn(home.Actor);if(shared)ObserveForcedSharedStop(home.Actor);
                    history.Insert(0,TransitActorName(home.Actor)+"从临时半位面返回原位置或最近空位。");
                }
            }
            foreach(var item in carriedWeapons.Items)if(item.Ground.Dropped&&item.Ground.Scene>0)item.Ground.Scene=item.Ground.ContainedBy.HasValue?map.SceneOf(item.Ground.ContainedBy.Value):0;
            if(monsterGroundWeapon.Dropped&&monsterGroundWeapon.Scene>0)monsterGroundWeapon.Scene=monsterGroundWeapon.ContainedBy.HasValue?map.SceneOf(monsterGroundWeapon.ContainedBy.Value):0;
            planarHomes.Clear();SyncBanishmentFlags();map.ViewActorScene(0);ObserveWaterBreathing();
        }finally{endingBanishment=false;}
    }
    void ResetBanishment()
    {
        banishment.Clear();planarHomes.Clear();nextDemiplane=1;map.ClearScenes();SyncBanishmentFlags();
    }
    void DrawPlanarStatus()
    {
        if(planarHomes.Count==0&&!combat.MonsterDeparted)return;
        string text=map.Scene==0?"训练场":map.Scene>0?"临时半位面 · 所有生物均可呼吸":"已离场";
        if(banishment.Concentrating)text+=" · 放逐术剩余 "+Math.Max(0,banishment.Expires-worldRounds)+"回合";
        if(combat.MonsterDeparted)text+=" · 木桩永久放逐，玩家胜利";
        GUI.Label(new Rect(590,80,430,30),text,centered);
    }
}
