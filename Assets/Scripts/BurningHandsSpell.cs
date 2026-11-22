using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly BreathChoice burningChoice=new BreathChoice();Vector2 burningScroll;
    bool burningPreview;BreathTemplate burningTemplate;int burningSource;
    bool PlayerBurningTemplate(out BreathTemplate template)
    {
        int spaces=map.PlayerSpaces;if(burningChoice.Offset<0)burningChoice.Offset=spaces;burningChoice.Offset=Mathf.Clamp(burningChoice.Offset,0,spaces*2);
        if(burningChoice.AimX<0){burningChoice.AimX=Mathf.Min(map.Size-1,map.Player.x+spaces+1)+.5;burningChoice.AimY=map.Player.y+.5;}
        return BreathTemplate.TryCreate(map.Player.x,map.Player.y,spaces,burningChoice.Edge,burningChoice.Offset,burningChoice.AimX,burningChoice.AimY,false,out template,burningChoice.IncludeOrigin);
    }
    string BurningHandsError(){BreathTemplate template;return PlayerBurningTemplate(out template)?"":"请选择有效的源点与朝外锥形方向。";}
    bool TryPlanMonsterBurningHands(out BreathTemplate template)
    {
        var enemies=new List<BreathBody>();foreach(int id in BreathActors())if((id==0||id>0)&&!Feigning(id)&&!NeutralUndead(id)){Vector2Int cell;int spaces;string name;if((id!=0||!map.IsContained(true))&&HealingHandsTarget(id,out cell,out spaces,out name))enemies.Add(new BreathBody(id,cell.x,cell.y,spaces));}
        return BreathWeaponRules.Choose(map.Size,map.Monster.x,map.Monster.y,map.MonsterSpaces,enemies,out template,false);
    }
    IEnumerator ResolveBurningHands(bool player,int casterClass,int slot,BreathTemplate template)
    {
        int source=player?0:-1;var spell=DirectSpell.Find("Burning Hands");var targets=BreathTargets(source,template);var failed=new HashSet<int>();
        long dc=player?8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(casterClass)]):MonsterSpellDc;
        bool previousPreview=burningPreview;var previousTemplate=burningTemplate;int previousSource=burningSource;
        burningPreview=true;burningTemplate=template;burningSource=source;outcomeDeferDepth++;bool oldWebFire=webAreaFire;webAreaFire=true;IgniteWebArea(template.Cell);
        try{
            foreach(int target in targets)yield return RollMappedDexteritySave(target,source,dc,failed,"燃烧之手");
            int count=spell.DiceCount(player?classes.TotalLevel:monsterSpells.CasterLevel,slot),damage=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=Random.Range(1,7);damage+=rolls[i];}
            dice="燃烧之手 · 共享火焰伤害";yield return PresentRoll(6,rolls,count+"d6：失败 "+damage+"，成功 "+damage/2+" 火焰");
            var reduced=new RayDamage();yield return ReduceRayDamage(player,damage,"火焰","燃烧之手共享伤害",reduced);damage=reduced.Amount;
            int playerDamage=0;foreach(int target in targets){int amount=ApplyMappedAreaDamage(target,source,failed.Contains(target)?damage:damage/2,5,"燃烧之手",failed.Contains(target));if(target==0)playerDamage=amount;}
            if(playerDamage>0)yield return MaintainConcentration(playerDamage);
            ReconcileFallenRescue();RefreshBestialSpirit();dice="燃烧之手 · "+targets.Count+"名目标结算完成";
            history.Insert(0,"燃烧之手：15尺锥形，"+count+"d6共享火焰伤害；环境可燃物点燃保留说明，暂缓环境结算。");
        }finally{webAreaFire=oldWebFire;outcomeDeferDepth--;burningPreview=previousPreview;burningTemplate=previousTemplate;burningSource=previousSource;CheckBattleOutcome();}
    }
}
