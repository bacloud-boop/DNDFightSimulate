using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class TrainingGame
{
    bool breathResolving;BreathTemplate activeBreath;int activeBreathCaster;
    SpeciesProfile BreathSpecies(int caster)=>caster==BattleMap.RescueId?rescueCarrier.Species:Species(caster==0);
    SpeciesAbilityState BreathState(int caster)=>caster==BattleMap.RescueId?rescueCarrier.SpeciesAbilities:SpeciesAbilities(caster==0);
    int BreathProficiency(int caster)=>caster==BattleMap.RescueId?rescueCarrier.ProficiencyBonus:SpeciesProficiency(caster==0);
    int BreathLevel(int caster)=>caster==BattleMap.RescueId?rescueCarrier.Species.Level:SpeciesLevel(caster==0);
    int BreathConstitution(int caster)=>caster==BattleMap.RescueId?rescueCarrier.Ability(2):caster==0?EffectivePlayerAbility(2):EffectiveMonsterAbility(2);
    bool BreathReady(int caster)
    {
        if(page!="battle"||!map.Locked||BreathSpecies(caster).Kind!=Species2024.Dragonborn||!BreathState(caster).CanUse(Species2024.Dragonborn,BreathProficiency(caster),worldRounds))return false;
        if(caster==BattleMap.RescueId)return map.RescuePosition.HasValue&&rescueCarrier.Conscious&&rescueCarrier.TurnActive&&rescueCarrier.ActionsRemaining>0;
        if(!SpeciesPhysicalActive(caster==0))return false;
        if(caster==-1)return combat.MonsterCanAct&&combat.MonsterActionAvailable;
        bool canAct=combat.Aftermath?combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&!combat.PlayerFeignDeath:combat.PlayerCanAct;
        return canAct&&(fighter.AttacksRemaining>0?!fighter.BreathUsedThisAttackAction:combat.ActionsRemaining>0);
    }
    List<int> BreathActors()
    {
        var ids=new List<int>();if(!combat.PlayerDead)ids.Add(0);if(combat.Hp>0)ids.Add(-1);if(map.RescuePosition.HasValue&&!rescueCarrier.Dead)ids.Add(BattleMap.RescueId);foreach(var unit in map.Summons)if(unit.Alive)ids.Add(unit.Id);return ids;
    }
    List<int> BreathTargets(int caster,BreathTemplate template)
    {
        var ids=new List<int>();
        foreach(int id in BreathActors()){
            if(id==caster){Vector2Int origin;int n;string label;if(HealingHandsTarget(caster,out origin,out n,out label)&&template.Intersects(origin.x,origin.y,n))ids.Add(id);continue;}
            // A swallowed creature has total cover from outside the container.
            if((id==0||id==-1)&&map.IsContained(id==0))continue;
            if((caster==0||caster==-1)&&map.IsContained(caster==0)){if(id==(caster==0?-1:0))ids.Add(id);continue;}
            Vector2Int cell;int spaces;string name;if(HealingHandsTarget(id,out cell,out spaces,out name)&&template.Intersects(cell.x,cell.y,spaces))ids.Add(id);
        }return ids;
    }
    IEnumerator RollMappedDexteritySave(int target,int caster,long dc,HashSet<int> failed,string context)
    {
        bool automatic=false;long modifier;int value;int[] shown;
        if(target==0||target==-1){
            bool player=target==0;automatic=player?combat.PlayerHp==0||combat.PlayerParalyzed:combat.MonsterHelpless;
            var roll=RollArmorSave(player,1);shown=roll.Dice;value=roll.Value;
            modifier=player?PlayerSavingThrowModifier(1):MonsterSavingThrowModifier(1);
            var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,automatic,adjustment,1,ignoreCover:true);modifier+=adjustment.Value;
        }else if(target==BattleMap.RescueId){automatic=!rescueCarrier.Conscious;value=Random.Range(1,21);shown=new[]{value};modifier=rescueCarrier.DexteritySaveModifier;}
        else {
            var unit=FindSummon(target);if(unit==null)yield break;automatic=unit.SleepStage==2;
            modifier=AcidSplash.Modifier(SummonAbility(unit,1))+unit.Breathing.D20Penalty;
            bool advantage=unit.Dodging&&unit.EffectiveSpeed>0,disadvantage=unit.Restrained;
            int first=Random.Range(1,21),second=advantage!=disadvantage?Random.Range(1,21):0;value=second==0?first:advantage?Mathf.Max(first,second):Mathf.Min(first,second);shown=second==0?new[]{first}:new[]{first,second};
        }
        int cover=target==caster?0:map.CreatureCoverBonus(caster,target);modifier+=cover;
        dice=context+" · "+TransitActorName(target)+"敏捷豁免";
        if(!automatic)yield return PresentRoll(20,shown,"采用 "+value+" + "+modifier+" / DC "+dc+(cover>0?"（半掩护+2）":""));
        if(automatic||(long)value+modifier<dc)failed.Add(target);
    }
    IEnumerator UseBreath(int caster,BreathTemplate template,bool automatic=false)
    {
        if(!template.Valid||!BreathReady(caster)||!automatic&&(rolling||spellTargeting||reactionPending||draggingToken>=0))yield break;
        var targets=BreathTargets(caster,template);
        if(caster==0){
            if(fighter.AttacksRemaining==0){if(!(combat.Aftermath?combat.TryUseObjectAction():combat.TryUseAction()))yield break;actionWeapon=selectedWeapon;fighter.BeginAttackAction(classes[0]);}
            if(!fighter.ReplaceAttackWithBreath())yield break;
        }else if(caster==-1){if(!combat.TryUseMonsterAction())yield break;}
        else if(!rescueCarrier.UseAction())yield break;
        if(!BreathState(caster).Use(Species2024.Dragonborn,BreathProficiency(caster),worldRounds))yield break;
        bool busy=rolling;string saved=panel;panel="";rolling=true;breathResolving=true;activeBreath=template;activeBreathCaster=caster;
        int type=BreathWeaponRules.DamageType(BreathSpecies(caster).Lineage),count=BreathWeaponRules.DiceCount(BreathLevel(caster));long dc=BreathWeaponRules.SaveDc(BreathConstitution(caster),BreathProficiency(caster));
        outcomeDeferDepth++;try{
            history.Insert(0,TransitActorName(caster)+"以龙裔吐息替换攻击动作的一次攻击："+(template.Line?"30尺×5尺线形":"15尺锥形")+"，"+count+"d10 "+DamageTypes.Names[type]+"，敏捷DC "+dc+"。不是施法或攻击检定。"+(caster==0?"本动作剩余攻击 "+fighter.AttacksRemaining+"次。":""));
            var failed=new HashSet<int>();foreach(int id in targets)yield return RollMappedDexteritySave(id,caster,dc,failed,"龙裔吐息");
            int damage=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=Random.Range(1,11);damage+=rolls[i];}
            dice="龙裔吐息 · 共享伤害";yield return PresentRoll(10,rolls,"失败 "+damage+" / 成功 "+damage/2+" "+DamageTypes.Names[type]);
            if(caster==0||caster==-1){var ray=new RayDamage();yield return ReduceRayDamage(caster==0,damage,DamageTypes.Names[type],"龙裔吐息共享伤害",ray);damage=ray.Amount;}
            int playerDamage=0;
            foreach(int id in targets){
                int amount=failed.Contains(id)?damage:damage/2;
                int applied=ApplyMappedAreaDamage(id,caster,amount,type,"龙裔吐息",failed.Contains(id));if(id==0)playerDamage=applied;
            }
            if(playerDamage>0)yield return MaintainConcentration(playerDamage);
            RefreshBestialSpirit();ReconcileFallenRescue();dice="龙裔吐息 · "+targets.Count+"名目标结算完成";
        }finally{outcomeDeferDepth--;breathResolving=false;panel=automatic?"":saved;rolling=busy;CheckBattleOutcome();}
    }
    bool MonsterBreathPreferred=>monsterSpecies.BonusPriority==0&&BreathReady(-1);
    IEnumerator TryMonsterBreath(bool beforeSpell)
    {
        if(!SpeciesTurnPolicy.ShouldAttempt(monsterSpecies.BonusPriority,beforeSpell,combat.MonsterActionAvailable,SpeciesPhysicalActive(false),combat.MonsterCanAct)||!BreathReady(-1))yield break;
        var enemies=new List<BreathBody>();foreach(int id in BreathActors())if((id==0||id>0)&&!Feigning(id)){Vector2Int cell;int spaces;string name;if((id!=0||!map.IsContained(true))&&HealingHandsTarget(id,out cell,out spaces,out name))enemies.Add(new BreathBody(id,cell.x,cell.y,spaces));}
        BreathTemplate plan;
        if(map.IsContained(false)){if(combat.PlayerDead||!BreathTemplate.TryCreate(map.Monster.x,map.Monster.y,map.MonsterSpaces,0,map.MonsterSpaces,map.Size, map.Monster.y,false,out plan))yield break;}
        else if(!BreathWeaponRules.Choose(map.Size,map.Monster.x,map.Monster.y,map.MonsterSpaces,enemies,out plan))yield break;
        yield return UseBreath(-1,plan,true);
    }
    void DrawBreathArea(Rect board,BreathTemplate template,int caster)
    {
        if(!template.Valid)return;
        bool internalSource=(caster==0||caster==-1)&&map.IsContained(caster==0);
        if(!internalSource)for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(template.Cell(x,y))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.95f,.32f,.08f,.36f));
        if(internalSource){Vector2Int cell;int spaces;string name;if(HealingHandsTarget(caster,out cell,out spaces,out name))Fill(BodyRect(board,cell,spaces),new Color(.95f,.32f,.08f,.36f));}
        float size=board.width/map.Size;Fill(new Rect(board.x+(float)template.X*size-3,board.y+(float)template.Y*size-3,6,6),Color.yellow);
    }
}
