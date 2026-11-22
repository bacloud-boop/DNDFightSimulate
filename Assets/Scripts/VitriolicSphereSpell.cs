using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    bool TryPlanMonsterVitriolicSphere(out Vector2Int center)
    {
        center=Vector2Int.zero;if(map.IsContained(false))return false;
        int best=0,nearest=int.MaxValue;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){
            var p=new Vector2Int(x,y);int distance=map.DistanceToCell(false,p);if(distance>150)continue;
            var targets=IceStormTargets(-1,p);if(targets.Contains(-1)||!acidArrows.CanAddMany(targets))continue;
            int count=targets.Count(id=>id!=BattleMap.RescueId&&!Feigning(id)&&!NeutralUndead(id));
            if(count>best||count==best&&count>0&&distance<nearest){best=count;nearest=distance;center=p;}
        }
        return best>0;
    }
    string VitriolicSphereError()
    {
        Vector2Int center;if(!VisionPoint(out center))return "浓酸球中心须位于地图内。";
        if(map.DistanceToCell(true,center)>150)return "中心超出150尺。";
        if(map.IsContained(true)&&!BattleMap.Overlap(center,1,map.Player,map.MonsterSpaces))return "吞咽内部施法不能选择体外中心。";
        return acidArrows.CanAddMany(IceStormTargets(0,center))?"":"残留效果编号或回合计数超出范围。";
    }
    void DrawVitriolicSphereChoices()
    {
        GUILayout.Label("中心列／行：150尺内一点，20尺半径球状范围。影响所有生物，包括自身、盟友、水面与潜水单位，不要求可见。",small);
        visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);
        Vector2Int p;if(VisionPoint(out p))GUILayout.Label("覆盖："+string.Join("、",IceStormTargets(0,p).Select(TransitActorName).ToArray()),small);
        GUILayout.Label("敏捷豁免失败："+VitriolicSphereRules.InitialDice(directSlotLevel)+"d4强酸，目标下个回合末另掷5d4；成功仅初始伤害减半，无残留。每升环初始增加2d4，残留不变。无需专注，多次残留独立结算。",small);
    }
    IEnumerator ResolveVitriolicSphere(bool player,int caster,int slot,Vector2Int center)
    {
        int count=VitriolicSphereRules.InitialDice(slot),source=player?0:-1;var targets=IceStormTargets(source,center);
        if(count==0||!acidArrows.CanAddMany(targets))yield break;
        var failed=new HashSet<int>();long dc=player?8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]):MonsterSpellDc;
        outcomeDeferDepth++;try{
            foreach(int actor in targets)yield return RollMappedAbilitySave(actor,source,dc,failed,"浓酸球",1,center);
            int sum=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=Random.Range(1,5);sum+=rolls[i];}
            dice="浓酸球 · 共享初始强酸";yield return PresentRoll(4,rolls,count+"d4 = "+sum);
            var reduced=new RayDamage();yield return ReduceRayDamage(player,sum,"强酸","浓酸球初始伤害",reduced);
            int self=0;foreach(int actor in targets){
                bool saved=!failed.Contains(actor);int amount=ApplyMappedDamagePacket(actor,source,VitriolicSphereRules.Packet(reduced.Amount,saved));if(actor==0)self=amount;
                if(!saved&&AcidArrowTargetAvailable(actor)){var entry=acidArrows.AddDelayed(source,actor,VitriolicSphereRules.LaterDice(slot),"浓酸球");if(entry!=null)history.Insert(0,"浓酸球残留 #"+entry.Id+"："+TransitActorName(actor)+"下个回合末另掷5d4强酸。");}
                history.Insert(0,"浓酸球："+TransitActorName(actor)+(saved?"豁免成功，初始半伤，无残留":"豁免失败")+"，结算 "+amount+" 点强酸伤害。");
            }
            yield return MaintainConcentration(self);RefreshUndeadFollowers();RefreshBestialSpirit();ReconcileFallenRescue();ClearUnavailableAcidArrows();
            dice="浓酸球 · "+targets.Count+"名目标已结算";
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
    void DrawVitriolicSpherePreview(Rect board)
    {
        Vector2Int center;if(panel!="spells"||selectedSpellName!="Vitriolic Sphere"||!VisionPoint(out center))return;
        if(map.IsContained(true)){Fill(BodyRect(board,map.Monster,map.MonsterSpaces),new Color(.5f,.85f,.2f,.35f));return;}
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(IceStormFields.Covers(center.x,center.y,x,y))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.5f,.85f,.2f,.35f));
    }
}
