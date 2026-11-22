using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly BreathChoice barrageChoice=new BreathChoice();
    Vector2 barrageScroll;
    readonly HashSet<int> barrageSelected=new HashSet<int>{-1},volleySelected=new HashSet<int>{-1};
    bool PlayerBarrageTemplate(out BreathTemplate template)
    {
        int n=map.PlayerSpaces;if(barrageChoice.Offset<0)barrageChoice.Offset=n;barrageChoice.Offset=Mathf.Clamp(barrageChoice.Offset,0,n*2);
        if(barrageChoice.AimX<0){barrageChoice.AimX=map.Player.x+n+12;barrageChoice.AimY=map.Player.y+n*.5;}
        return BreathTemplate.TryCreate(map.Player.x,map.Player.y,n,barrageChoice.Edge,barrageChoice.Offset,barrageChoice.AimX,barrageChoice.AimY,false,out template,barrageChoice.IncludeOrigin,ConjuredWeaponRules.ConeFeet);
    }
    bool ConjuredTargetVisible(bool player,int actor)
    {
        Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))return false;
        if(player)return actor==0?CanSeeOwnBody():actor==-1?PlayerSeesMonster:VisibilityAt(true,p,n)<2;
        return actor==-1?Visibility(false,false)<2:actor==0?MonsterSeesPlayer:VisibilityBetween(false,map.Monster.x,map.Monster.y,map.MonsterSpaces,p.x,p.y,n)<2;
    }
    List<int> VolleyCovered(int source,Vector2Int center)
    {
        var result=new List<int>();bool internalCast=map.IsContained(source==0);
        foreach(int actor in BreathActors()){
            Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))continue;
            if(internalCast){if(actor==source||actor==(source==0?-1:0))result.Add(actor);continue;}
            if((actor==0||actor==-1)&&map.IsContained(actor==0))continue;
            if(ConjuredWeaponRules.CylinderCell(center.x,center.y,p.x,p.y,n))result.Add(actor);
        }return result;
    }
    string ConjuredWeaponsError(bool volley)
    {
        if(!volley){BreathTemplate template;return PlayerBarrageTemplate(out template)?"":"请选择有效的60尺锥形源点与朝向。";}
        Vector2Int p;if(!VisionPoint(out p))return "万箭齐发中心须位于地图内。";
        if(map.DistanceToCell(true,p)>150)return "中心超出150尺。";
        return map.IsContained(true)&&!BattleMap.Overlap(p,1,map.Player,map.MonsterSpaces)?"吞咽内部施法不能选择体外中心。":"";
    }
    void DrawConjuredTargets(bool volley,List<int> covered)
    {
        var selection=volley?volleySelected:barrageSelected;
        GUILayout.Label("仅影响所选、范围内且当前可见的生物；可排除自身与盟友。",small);
        foreach(int actor in covered){
            bool visible=ConjuredTargetVisible(true,actor),old=GUI.enabled;GUI.enabled=old&&visible;
            bool selected=GUILayout.Toggle(selection.Contains(actor),TransitActorName(actor)+(visible?"":"（不可见，不受影响）"),label);
            if(selected)selection.Add(actor);else selection.Remove(actor);GUI.enabled=old;
        }
    }
    void DrawVolleyChoices()
    {
        GUILayout.Label("中心列／行：150尺内一点，40尺半径、20尺高圆柱；沿用平面地图，不记录高度。8d8力场，敏捷豁免成功减半，升环不增加伤害。",small);
        visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);
        Vector2Int p;if(VisionPoint(out p))DrawConjuredTargets(true,VolleyCovered(0,p));
    }
    bool TryPlanMonsterBarrage(out BreathTemplate template)
    {
        if(map.IsContained(false))return BreathTemplate.TryCreate(map.Monster.x,map.Monster.y,map.MonsterSpaces,4,0,map.Monster.x+map.MonsterSpaces+1,map.Monster.y,false,out template,false,60)&&ConjuredTargetVisible(false,0)&&!Feigning(0);
        var enemies=new List<BreathBody>();
        foreach(int actor in BreathActors())if((actor==0||actor>0)&&!Feigning(actor)&&!NeutralUndead(actor)&&ConjuredTargetVisible(false,actor)){
            Vector2Int p;int n;string name;if((actor!=0||!map.IsContained(true))&&HealingHandsTarget(actor,out p,out n,out name))enemies.Add(new BreathBody(actor,p.x,p.y,n));
        }
        return BreathWeaponRules.Choose(map.Size,map.Monster.x,map.Monster.y,map.MonsterSpaces,enemies,out template,false,ConjuredWeaponRules.ConeFeet,false);
    }
    HashSet<int> MonsterConjuredSelection()
    {
        var result=new HashSet<int>();foreach(int actor in BreathActors())if((actor==0||actor>0)&&!Feigning(actor)&&!NeutralUndead(actor))result.Add(actor);return result;
    }
    bool TryPlanMonsterVolley(out Vector2Int center)
    {
        center=map.Monster;if(map.IsContained(false))return ConjuredTargetVisible(false,0)&&!Feigning(0);
        int most=0,nearest=int.MaxValue;var selected=MonsterConjuredSelection();
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){
            var point=new Vector2Int(x,y);int distance=map.DistanceToCell(false,point);
            if(distance>150)continue;
            int count=ConjuredTargets(false,true,default(BreathTemplate),point,selected).Count;
            if(count>most||count==most&&count>0&&distance<nearest){most=count;nearest=distance;center=point;}
        }return most>0;
    }
    List<int> ConjuredTargets(bool player,bool volley,BreathTemplate template,Vector2Int center,ISet<int> selected)
        =>ConjuredWeaponRules.Select(volley?VolleyCovered(player?0:-1,center):BreathTargets(player?0:-1,template),selected,actor=>ConjuredTargetVisible(player,actor));
    IEnumerator ResolveConjuredWeapons(bool player,int caster,int slot,bool volley,BreathTemplate template,Vector2Int center,ISet<int> selected)
    {
        int count=ConjuredWeaponRules.DiceCount(volley,slot);if(count==0||!volley&&(!template.Valid||template.Line||template.LengthFeet!=60))yield break;
        int source=player?0:-1;string title=volley?"万箭齐发":"箭如雨下";
        var targets=ConjuredTargets(player,volley,template,center,selected);var failed=new HashSet<int>();
        long dc=player?8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]):MonsterSpellDc;
        bool previousPreview=burningPreview;var previousTemplate=burningTemplate;int previousSource=burningSource;
        if(!volley){burningPreview=true;burningTemplate=template;burningSource=source;}
        outcomeDeferDepth++;try{
            foreach(int actor in targets)yield return RollMappedAbilitySave(actor,source,dc,failed,title,1,volley?(Vector2Int?)center:null);
            int damage=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=Random.Range(1,9);damage+=rolls[i];}
            dice=title+" · 共享力场伤害";yield return PresentRoll(8,rolls,count+"d8 = "+damage+"，豁免成功 "+damage/2);
            var reduced=new RayDamage();yield return ReduceRayDamage(player,damage,"力场",title+"共享伤害",reduced);
            int playerDamage=0;foreach(int actor in targets){int amount=ApplyMappedAreaDamage(actor,source,failed.Contains(actor)?reduced.Amount:reduced.Amount/2,6,title,failed.Contains(actor));if(actor==0)playerDamage=amount;}
            yield return MaintainConcentration(playerDamage);ReconcileFallenRescue();RefreshBestialSpirit();NoticeCastingInterruption();
            dice=title+" · "+targets.Count+"名目标结算完成";history.Insert(0,title+"：仅影响所选可见目标；水面和潜水均受力场伤害，不产生实体武器、地形或环境伤害。");
        }finally{outcomeDeferDepth--;burningPreview=previousPreview;burningTemplate=previousTemplate;burningSource=previousSource;CheckBattleOutcome();}
    }
    void DrawVolleyPreview(Rect board)
    {
        Vector2Int center;if(panel!="spells"||selectedSpellName!="Conjure Volley"||!VisionPoint(out center))return;
        if(map.IsContained(true)){Fill(BodyRect(board,map.Monster,map.MonsterSpaces),new Color(.85f,.45f,.1f,.3f));return;}
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(ConjuredWeaponRules.CylinderCell(center.x,center.y,x,y))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.85f,.45f,.1f,.3f));
    }
}
