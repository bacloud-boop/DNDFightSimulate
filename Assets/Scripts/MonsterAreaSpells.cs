using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly List<int> monsterAreaPreviewTargets=new List<int>();
    bool monsterAreaPreview,monsterAreaChoosing,monsterVolleyPreview,monsterAcidSpherePreview;
    Vector2Int monsterAreaCenter;
    int monsterAreaRadius,monsterAreaOriginSpaces=1;
    List<int> MonsterAreaTargets(Vector2Int center,int radius,int spaces=1,bool sight=false,bool planning=false)
    {
        var targets=new List<int>();
        if((!planning||!Feigning(0))&&!combat.PlayerDead&&!map.IsContained(true)&&BattleMap.BodyDistance(center,spaces,map.Player,map.PlayerSpaces)<=radius&&(!sight||MonsterSeesPlayer))targets.Add(0);
        foreach(var unit in map.SceneSummons)if(unit.Alive&&(!planning||!Feigning(unit.Id)&&!NeutralUndead(unit.Id))&&BattleMap.BodyDistance(center,spaces,new Vector2Int(unit.X,unit.Y),unit.Spaces)<=radius&&(!sight||MonsterCanSeeTarget(unit.Id)))targets.Add(unit.Id);
        return targets;
    }
    bool TryPlanMonsterArea(DirectSpell spell,out Vector2Int center)
    {
        if(spell.Name=="Vitriolic Sphere")return TryPlanMonsterVitriolicSphere(out center);
        if(spell.Name=="Conjure Volley")return TryPlanMonsterVolley(out center);
        if((spell.Name=="Flame Strike"||spell.Name=="Ice Storm")&&map.IsContained(false)){center=Vector2Int.zero;return false;}
        int x,y,radius=MonsterSpellCatalog.Radius(spell);
        if(spell.EmanationRadius>0){center=map.Monster;return MonsterAreaTargets(center,radius,map.MonsterSpaces,spell.Name=="Word of Radiance",true).Count>0;}
        bool found=MonsterAreaPlanner.Choose(map.Size,spell.Range,radius,(column,row)=>map.DistanceToCell(false,new Vector2Int(column,row)),(column,row)=>MonsterAreaTargets(new Vector2Int(column,row),radius,planning:true).Count,out x,out y);
        center=new Vector2Int(x,y);return found;
    }
    void ShowMonsterArea(DirectSpell spell,Vector2Int center)
    {
        monsterAcidSpherePreview=spell.Name=="Vitriolic Sphere";monsterVolleyPreview=spell.Name=="Conjure Volley";monsterAreaCenter=center;monsterAreaRadius=MonsterSpellCatalog.Radius(spell);monsterAreaOriginSpaces=spell.EmanationRadius>0?map.MonsterSpaces:1;monsterAreaPreview=true;
        monsterAreaPreviewTargets.Clear();monsterAreaPreviewTargets.AddRange(spell.Name=="Vitriolic Sphere"?IceStormTargets(-1,center).FindAll(id=>id!=-1):monsterVolleyPreview?ConjuredTargets(false,true,default(BreathTemplate),center,MonsterConjuredSelection()):MonsterAreaTargets(center,monsterAreaRadius,monsterAreaOriginSpaces,spell.Name=="Word of Radiance"));
        history.Insert(0,"木桩范围施法中心：第"+(center.x+1)+"列，第"+(center.y+1)+"行；半径 "+monsterAreaRadius+"尺，覆盖 "+monsterAreaPreviewTargets.Count+"名敌人"+(monsterVolleyPreview?"，仅选择敌人受伤，不将自身列为目标。":"，避开自身。"));
    }
    void DrawMonsterArea(Rect board)
    {
        if(!monsterAreaPreview)return;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(!BattleMap.Overlap(map.Monster,map.MonsterSpaces,new Vector2Int(x,y),1)&&(monsterAcidSpherePreview?IceStormFields.Covers(monsterAreaCenter.x,monsterAreaCenter.y,x,y):monsterVolleyPreview?ConjuredWeaponRules.CylinderCell(monsterAreaCenter.x,monsterAreaCenter.y,x,y):BattleMap.BodyDistance(monsterAreaCenter,monsterAreaOriginSpaces,new Vector2Int(x,y),1)<=monsterAreaRadius))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.9f,.18f,.08f,.32f));
    }
    IEnumerator ResolveMonsterAreaSpell(DirectSpell spell,int slot,Vector2Int center)
    {
        bool old=webAreaFire;try{if(spell.DamageType=="火焰"){IgniteWebArea((x,y)=>BattleMap.BodyDistance(center,1,new Vector2Int(x,y),1)<=MonsterSpellCatalog.Radius(spell));webAreaFire=true;}yield return ResolveMonsterAreaSpellCore(spell,slot,center);}finally{webAreaFire=old;}
    }
    IEnumerator ResolveMonsterAreaSpellCore(DirectSpell spell,int slot,Vector2Int center)
    {
        int radius=MonsterSpellCatalog.Radius(spell);var targets=MonsterAreaTargets(center,radius,spell.EmanationRadius>0?map.MonsterSpaces:1,spell.Name=="Word of Radiance");var failed=new HashSet<int>();
        foreach(int id in targets){
            bool player=id==0;var unit=player?null:FindSummon(id);
            bool automatic=(spell.SaveAbility==0||spell.SaveAbility==1)&&(player?combat.PlayerHp==0||combat.PlayerParalyzed:unit.SleepStage==2);
            ArmorSaveRoll save;int modifier;
            if(player){
                save=RollArmorSave(true,spell.SaveAbility);
                modifier=PlayerSavingThrowModifier(spell.SaveAbility);
                var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(true,automatic,adjustment,spell.SaveAbility,center);modifier+=adjustment.Value;
            }else{
                save=RollSummonSavingThrow(unit,spell.SaveAbility);
                modifier=SummonSaveModifier(unit,spell.SaveAbility)+unit.Breathing.D20Penalty;
                if(spell.SaveAbility==1)modifier+=map.CreatureCoverFromPoint(center,id);
            }
            dice="木桩"+spell.Chinese+" · "+(player?"玩家":unit.Name)+PlayerAttributes.Names[spell.SaveAbility]+"豁免";
            if(!automatic)yield return PresentRoll(20,save.Dice,"采用 "+save.Value+" + "+modifier+" / DC "+MonsterSpellDc);
            if(automatic||(long)save.Value+modifier<MonsterSpellDc)failed.Add(id);
        }
        int count=spell.DiceCount(monsterSpells.CasterLevel,slot),damage=0;var rolls=new int[count];
        for(int i=0;i<count;i++){rolls[i]=Random.Range(1,spell.Sides+1);damage+=rolls[i];}
        dice="木桩"+spell.Chinese+" · 共享范围伤害";yield return PresentRoll(spell.Sides,rolls,"失败 "+damage+"；成功 "+(spell.HalfSave?damage/2:0)+" "+spell.DamageType);
        var rayDamage=new RayDamage();yield return ReduceRayDamage(false,damage,spell.DamageType,"木桩范围法术共享伤害",rayDamage);damage=rayDamage.Amount;
        // Resolve simultaneous area damage before concentration can dismiss units.
        int playerDamage=0;
        foreach(int id in targets){
            int raw=failed.Contains(id)?damage:spell.HalfSave?damage/2:0;
            if(id==0){playerDamage=combat.AdjustIncomingTypedDamage(true,raw,DamageTypes.Index(spell.DamageType));combat.ApplyPlayerDamage(playerDamage);history.Insert(0,"木桩"+spell.Chinese+"对玩家造成 "+playerDamage+" "+spell.DamageType+"伤害。");}
            else {var unit=FindSummon(id);if(unit==null||!unit.Alive)continue;int amount=SilenceUnitImmune(unit,spell.DamageType)?0:raw;unit.Damage(amount,DamageTypes.Index(spell.DamageType));amount=unit.LastDamageReceived;history.Insert(0,"木桩"+spell.Chinese+"对"+unit.Name+"造成 "+amount+" "+spell.DamageType+"伤害。");}
        }
        if(targets.Contains(0))yield return MaintainConcentration(playerDamage);
    }
}
