using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly IceStormFields iceStorm=new IceStormFields();
    string IceStormError()
    {
        Vector2Int center;if(!VisionPoint(out center))return "冰雹暴中心须位于地图内。";
        if(map.DistanceToCell(true,center)>300)return "中心超出300尺。";
        if(map.IsContained(true)&&!BattleMap.Overlap(center,1,map.Player,map.MonsterSpaces))return "被吞咽时只能在内部施法，不能选择体外中心。";
        return iceStorm.CanAdd(0,worldRounds)?"":"世界时间超出范围。";
    }
    void DrawIceStormChoices()
    {
        GUILayout.Label("中心列／行，半径20尺、圆柱高40尺。沿用平面地图，不计算高度；伤害影响陆地、水面和潜水单位。只有陆地留下困难地形，至施法者下个回合结束；不需要专注。",small);
        visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);
        Vector2Int p;if(VisionPoint(out p))GUILayout.Label("覆盖："+string.Join("、",IceStormTargets(0,p).Select(TransitActorName).ToArray()),small);
        GUILayout.Label(IceStormRules.BludgeoningDice(directSlotLevel)+"d10钝击＋4d6寒冷，敏捷豁免成功时每类伤害各减半；升环仅增加钝击。",small);
    }
    List<int> IceStormTargets(int caster,Vector2Int center)
    {
        var result=new List<int>();bool internalCast=(caster==0||caster==-1)&&map.IsContained(caster==0);
        foreach(int actor in BreathActors()){
            Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))continue;
            if(internalCast){if(actor==caster||actor==(caster==0?-1:0))result.Add(actor);continue;}
            if((actor==0||actor==-1)&&map.IsContained(actor==0))continue;
            if(IceStormFields.Covers(center.x,center.y,p.x,p.y,n))result.Add(actor);
        }
        return result;
    }
    int ApplyMappedDamagePacket(int target,int caster,long[] raw,bool critical=false)
    {
        if(target==0||target==-1){if(raw.Length>5&&raw[5]>0)combat.FireExposure?.Invoke(target==0);var resolved=combat.ResolveTypedPacket(target==0,raw);int amount=PacketAmount(resolved);
            if(target==0)combat.ApplyPlayerDamage(amount,critical,sourcePlayer:caster==0,resolvedTypes:resolved,sourceActor:caster);
            else combat.ApplyDamage(amount,sourcePlayer:caster==0,resolvedTypes:resolved,sourceActor:caster);
            return amount;
        }
        if(target==BattleMap.RescueId){int received=rescueCarrier.TakeDamagePacket(raw);if(received>0)BreakOrdinaryInvisibility(caster==0,"造成伤害");return received;}
        var unit=FindSummon(target);if(unit==null)return 0;
        var packet=(long[])raw.Clone();for(int i=0;i<packet.Length;i++){if(i==12&&SilenceUnitImmune(unit,"雷鸣"))packet[i]=0;else if(unit.ResistsType(i)||i==5&&unit.Aquatic.Submerged)packet[i]/=2;}
        int total=PacketAmount(packet);unit.Damage(total,type:raw.Length>5&&raw[5]>0?5:-1,resolvedTypes:packet,critical:critical);if(total>0)BreakOrdinaryInvisibility(caster==0,"造成伤害");return total;
    }
    static int PacketAmount(long[] packet){long total=0;foreach(long v in packet)total=System.Math.Min(int.MaxValue,total+System.Math.Max(0,v));return (int)total;}
    IEnumerator ResolveIceStorm(bool player,int caster,int slot,Vector2Int center)
    {
        int count=IceStormRules.BludgeoningDice(slot),source=player?0:-1;if(count==0||!iceStorm.CanAdd(source,worldRounds))yield break;
        bool internalCast=map.IsContained(player);var targets=IceStormTargets(source,center);var failed=new HashSet<int>();
        long dc=player?8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]):MonsterSpellDc;
        outcomeDeferDepth++;try{
            foreach(int actor in targets)yield return RollMappedAbilitySave(actor,source,dc,failed,"冰雹暴",1,center);
            int blunt=0,cold=0;var a=new int[count];var b=new int[4];for(int i=0;i<a.Length;i++){a[i]=Random.Range(1,11);blunt+=a[i];}for(int i=0;i<b.Length;i++){b[i]=Random.Range(1,7);cold+=b[i];}
            dice="冰雹暴 · 共享混合伤害";yield return PresentRoll(10,a,count+"d10钝击 = "+blunt);yield return PresentRoll(6,b,"4d6寒冷 = "+cold);
            var reduced=new RayDamage();yield return ReduceRayPacket(player,IceStormRules.Packet(blunt,cold),true,"冰雹暴合并伤害",reduced);
            int playerDamage=0;foreach(int actor in targets){int amount=ApplyMappedDamagePacket(actor,source,IceStormRules.AfterSave(reduced.Raw,!failed.Contains(actor)));if(actor==0)playerDamage=amount;history.Insert(0,"冰雹暴："+TransitActorName(actor)+(failed.Contains(actor)?"豁免失败":"豁免成功，两类伤害减半")+"，结算 "+amount+" 点混合伤害。");}
            if(!internalCast)iceStorm.Add(source,center.x,center.y,map.Size,(x,y)=>!map.IsWater(new Vector2Int(x,y)),worldRounds,map.Scene);
            SyncTimedBuffs();if(playerDamage>0)yield return MaintainConcentration(playerDamage);
            ReconcileFallenRescue();RefreshBestialSpirit();dice="冰雹暴 · "+targets.Count+"名目标已结算";
            history.Insert(0,internalCast?"冰雹暴受吞咽内外隔离限制，不在体外留下地形。":"冰雹暴：陆地困难地形持续至施法者下个回合结束，水域不留下困难地形。");
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
    void DrawIceStormArea(Rect board)
    {
        Vector2Int center=Vector2Int.zero;bool preview=panel=="spells"&&selectedSpellName=="Ice Storm"&&VisionPoint(out center);
        if(preview&&map.IsContained(true)){Fill(BodyRect(board,map.Monster,map.MonsterSpaces),new Color(.65f,.85f,1,.28f));preview=false;}
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){
            if(iceStorm.Cell(x,y,worldRounds,map.Scene))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.4f,.65f,.85f,.28f));
            if(preview&&IceStormFields.Covers(center.x,center.y,x,y))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.65f,.85f,1,.28f));
        }
    }
}
