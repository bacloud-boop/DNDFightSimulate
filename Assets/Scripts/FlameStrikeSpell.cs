using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    string FlameStrikeError()
    {
        Vector2Int center;if(!VisionPoint(out center))return "焰击术中心须位于地图内。";
        if(map.DistanceToCell(true,center)>60)return "中心超出60尺。";
        return map.IsContained(true)&&!BattleMap.Overlap(center,1,map.Player,map.MonsterSpaces)?"吞咽内部施法不能选择体外中心。":"";
    }
    List<int> FlameStrikeTargets(int source,Vector2Int center)
    {
        var result=new List<int>();bool internalCast=map.IsContained(source==0);
        foreach(int actor in BreathActors()){
            Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))continue;
            if(internalCast){if(actor==source||actor==(source==0?-1:0))result.Add(actor);continue;}
            if((actor==0||actor==-1)&&map.IsContained(actor==0))continue;
            if(FlameStrikeRules.Covers(center.x,center.y,p.x,p.y,n))result.Add(actor);
        }return result;
    }
    void DrawFlameStrikeChoices()
    {
        GUILayout.Label("中心列／行：60尺内一点，10尺半径、40尺高圆柱；沿用平面地图。影响所有生物，包括自身与盟友、水面与潜水目标，不要求可见。",small);
        visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);
        Vector2Int p;if(VisionPoint(out p))GUILayout.Label("覆盖："+string.Join("、",FlameStrikeTargets(0,p).Select(TransitActorName).ToArray()),small);
        int count=FlameStrikeRules.DicePerType(directSlotLevel);GUILayout.Label(count+"d6火焰＋"+count+"d6光耀。敏捷豁免成功每类分别减半；升环两类各增加1d6，无需专注。",small);
    }
    IEnumerator ResolveFlameStrike(bool player,int caster,int slot,Vector2Int center)
    {
        int count=FlameStrikeRules.DicePerType(slot);if(count==0)yield break;
        int source=player?0:-1;bool internalCast=map.IsContained(player);var targets=FlameStrikeTargets(source,center);var failed=new HashSet<int>();
        long dc=player?8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]):MonsterSpellDc;
        bool previousFire=webAreaFire;outcomeDeferDepth++;try{
            foreach(int actor in targets)yield return RollMappedAbilitySave(actor,source,dc,failed,"焰击术",1,center);
            int fire=0,radiant=0;var a=new int[count];var b=new int[count];for(int i=0;i<count;i++){a[i]=Random.Range(1,7);fire+=a[i];b[i]=Random.Range(1,7);radiant+=b[i];}
            dice="焰击术 · 共享混合伤害";yield return PresentRoll(6,a,count+"d6火焰 = "+fire);yield return PresentRoll(6,b,count+"d6光耀 = "+radiant);
            var reduced=new RayDamage();yield return ReduceRayPacket(player,FlameStrikeRules.Packet(fire,radiant),true,"焰击术合并伤害",reduced);
            if(!internalCast)IgniteWebArea((x,y)=>FlameStrikeRules.Covers(center.x,center.y,x,y));webAreaFire=true;
            int self=0;foreach(int actor in targets){int amount=ApplyMappedDamagePacket(actor,source,IceStormRules.AfterSave(reduced.Raw,!failed.Contains(actor)));if(actor==0)self=amount;history.Insert(0,"焰击术："+TransitActorName(actor)+(failed.Contains(actor)?"豁免失败":"豁免成功，两类各减半")+"，结算 "+amount+" 点混合伤害。");}
            // Limit ignition to this area while applying damage, then allow a
            // reaction spell to ignite its own distinct area normally.
            webAreaFire=previousFire;
            yield return MaintainConcentration(self);SyncTimedBuffs();ReconcileFallenRescue();RefreshBestialSpirit();
            dice="焰击术 · "+targets.Count+"名目标已结算";
        }finally{webAreaFire=previousFire;outcomeDeferDepth--;CheckBattleOutcome();}
    }
    void DrawFlameStrikePreview(Rect board)
    {
        Vector2Int center;if(panel!="spells"||selectedSpellName!="Flame Strike"||!VisionPoint(out center))return;
        if(map.IsContained(true)){Fill(BodyRect(board,map.Monster,map.MonsterSpaces),new Color(1,.65f,.2f,.35f));return;}
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(FlameStrikeRules.Covers(center.x,center.y,x,y))Fill(CellRect(board,new Vector2Int(x,y)),new Color(1,.65f,.2f,.35f));
    }
}
