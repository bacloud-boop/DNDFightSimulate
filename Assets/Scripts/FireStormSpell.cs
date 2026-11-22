using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly List<FireStormRules.Block> fireStormBlocks=new List<FireStormRules.Block>();
    List<FireStormRules.Block> activeFireStorm;
    string fireStormColumn="1",fireStormRow="1",fireStormMessage="";
    string FireStormError()
    {
        if(!FireStormRules.Valid(fireStormBlocks,map.Size,(x,y)=>map.DistanceToCell(true,new Vector2Int(x,y))<=150))return "须放置1～10块完整2×2格区域；多块时每块至少与另一块共边，每格在150尺内。";
        if(map.IsContained(true)&&fireStormBlocks.Any(b=>!b.Covers(map.Player.x,map.Player.y,map.MonsterSpaces)))return "吞咽内部施法的区域须与容纳者占地重叠，不能作用于体外。";
        return "";
    }
    List<int> FireStormTargets(int source,IReadOnlyList<FireStormRules.Block> blocks)
    {
        var targets=new List<int>();bool inside=map.IsContained(source==0);
        foreach(int actor in BreathActors()){
            Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))continue;
            if(inside){if(actor==source||actor==(source==0?-1:0))targets.Add(actor);continue;}
            if((actor==0||actor==-1)&&map.IsContained(actor==0))continue;
            if(FireStormRules.Covers(blocks,p.x,p.y,n))targets.Add(actor);
        }return targets;
    }
    void DrawFireStormChoices()
    {
        GUILayout.Label("每块10尺立方区域投影为2×2格，输入左上格列／行。可单独放1块，多块时每块至少与另一块共边，允许分成多个组；至多10块。",small);
        fireStormColumn=IntegerField(fireStormColumn,false);fireStormRow=IntegerField(fireStormRow,false);
        if(GUILayout.Button("添加2×2格区域",button)){
            int x,y;if(int.TryParse(fireStormColumn,out x)&&int.TryParse(fireStormRow,out y)&&x>=1&&y>=1&&x<map.Size&&y<map.Size&&fireStormBlocks.Count<10){var b=new FireStormRules.Block(x-1,y-1);if(!fireStormBlocks.Contains(b)){fireStormBlocks.Add(b);fireStormMessage="";}else fireStormMessage="该区域已添加。";}else fireStormMessage="左上格须能容纳完整2×2区域，最多10块。";
        }
        for(int i=0;i<fireStormBlocks.Count;i++)if(GUILayout.Button("移除区域 "+(i+1)+" · 列"+(fireStormBlocks[i].X+1)+"／行"+(fireStormBlocks[i].Y+1),button)){fireStormBlocks.RemoveAt(i);break;}
        if(GUILayout.Button("清空火焰区域",button))fireStormBlocks.Clear();
        GUILayout.Label("7d10共享火焰，敏捷豁免成功减半；升环不增强。不留困难地形，水面和潜水均受影响；每名生物只结算一次，包括自身和盟友。",small);
        GUILayout.Label("覆盖："+string.Join("、",FireStormTargets(0,fireStormBlocks).Select(TransitActorName).ToArray()),small);GUILayout.Label(fireStormMessage,small);
    }
    bool TryPlanMonsterFireStorm(out List<FireStormRules.Block> plan)
    {
        plan=new List<FireStormRules.Block>();if(map.IsContained(false))return false;
        var enemies=new List<FireStormRules.Body>();foreach(int actor in BreathActors())if((actor==0||actor>0)&&!Feigning(actor)&&!NeutralUndead(actor)){
            Vector2Int p;int n;string name;if((actor!=0||!map.IsContained(true))&&HealingHandsTarget(actor,out p,out n,out name))enemies.Add(new FireStormRules.Body(actor,p.x,p.y,n));
        }
        plan=FireStormRules.Choose(map.Size,new FireStormRules.Body(-1,map.Monster.x,map.Monster.y,map.MonsterSpaces),enemies,(x,y)=>map.DistanceToCell(false,new Vector2Int(x,y))<=150);return plan.Count>0;
    }
    Vector2Int FireStormSaveOrigin(int actor,IReadOnlyList<FireStormRules.Block> blocks)
    {
        Vector2Int p;int n;string name;HealingHandsTarget(actor,out p,out n,out name);var result=blocks[0];int best=int.MaxValue;
        foreach(var block in blocks)if(block.Covers(p.x,p.y,n)){int cover=map.CreatureCoverFromPoint(new Vector2Int(block.X,block.Y),actor,2);if(cover<best){best=cover;result=block;}}
        return new Vector2Int(result.X,result.Y);
    }
    IEnumerator ResolveFireStorm(bool player,int caster,IReadOnlyList<FireStormRules.Block> blocks)
    {
        int source=player?0:-1;var targets=FireStormTargets(source,blocks);var failed=new HashSet<int>();
        long dc=player?8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]):MonsterSpellDc;
        var previous=activeFireStorm;bool oldFire=webAreaFire;activeFireStorm=new List<FireStormRules.Block>(blocks);outcomeDeferDepth++;
        try{
            foreach(int actor in targets)yield return RollMappedAbilitySave(actor,source,dc,failed,"火焰风暴",1,FireStormSaveOrigin(actor,blocks),2);
            int damage=0;var rolls=new int[7];for(int i=0;i<rolls.Length;i++){rolls[i]=UnityEngine.Random.Range(1,11);damage+=rolls[i];}
            dice="火焰风暴 · 共享7d10火焰";yield return PresentRoll(10,rolls,"失败 "+damage+"，成功 "+damage/2);
            var reduced=new RayDamage();yield return ReduceRayDamage(player,damage,"火焰","火焰风暴共享伤害",reduced);
            if(!map.IsContained(player))IgniteWebArea((x,y)=>FireStormRules.Covers(blocks,x,y));webAreaFire=true;
            int self=0;foreach(int actor in targets){var raw=new long[13];raw[5]=failed.Contains(actor)?reduced.Amount:reduced.Amount/2;int amount=ApplyMappedDamagePacket(actor,source,raw);if(actor==0)self=amount;history.Insert(0,"火焰风暴："+TransitActorName(actor)+(failed.Contains(actor)?"豁免失败":"豁免成功，伤害减半")+"，结算 "+amount+" 火焰。");}
            webAreaFire=oldFire;yield return MaintainConcentration(self);SyncTimedBuffs();ReconcileFallenRescue();RefreshUndeadFollowers();RefreshBestialSpirit();dice="火焰风暴 · "+targets.Count+"名目标结算完成";
        }finally{webAreaFire=oldFire;activeFireStorm=previous;outcomeDeferDepth--;CheckBattleOutcome();}
    }
    void DrawFireStormPreview(Rect board)
    {
        var blocks=activeFireStorm??(panel=="spells"&&selectedSpellName=="Fire Storm"?fireStormBlocks:null);if(blocks==null)return;
        if(activeFireStorm==null&&map.IsContained(true)){Fill(BodyRect(board,map.Monster,map.MonsterSpaces),new Color(1,.2f,.05f,.35f));return;}
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(FireStormRules.Covers(blocks,x,y))Fill(CellRect(board,new Vector2Int(x,y)),new Color(1,.2f,.05f,.35f));
    }
}
