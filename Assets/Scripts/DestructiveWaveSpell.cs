using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    bool destructiveNecrotic;
    readonly HashSet<int> destructiveTargets=new HashSet<int>{-1};
    void DrawDestructiveWaveChoices()
    {
        destructiveNecrotic=GUILayout.SelectionGrid(destructiveNecrotic?1:0,new[]{"光耀","死灵"},2,button)==1;
        GUILayout.Label("自身30尺光环内自选目标，不要求看见目标。体质豁免失败承受5d6雷鸣＋5d6所选类型并倒地；成功两类各减半，不倒地。升环不增加伤害。",small);
        foreach(int id in LivingSpellTargets()){
            bool chosen=GUILayout.Toggle(destructiveTargets.Contains(id),SpellTargetName(id)+" · 距离 "+SpellTargetDistance(0,id)+"尺",label);
            if(chosen)destructiveTargets.Add(id);else destructiveTargets.Remove(id);
        }
        foreach(int id in new List<int>(destructiveTargets))if(!LivingSpellTarget(id)&&GUILayout.Button("移除失效目标 #"+id,button))destructiveTargets.Remove(id);
    }
    string DestructiveWaveError(){foreach(int id in destructiveTargets)if(!LivingSpellTarget(id)||SpellTargetDistance(0,id)>30)return "所选目标必须存活或倒地未死亡，且处于自身30尺光环内。";return "";}
    void DrawDestructiveWavePreview(Rect board)
    {
        if(panel!="spells"||selectedSpellName!="Destructive Wave")return;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){var cell=new Vector2Int(x,y);if(map.DistanceToCell(true,cell)<=30)Fill(CellRect(board,cell),new Color(.8f,.48f,.16f,.3f));}
    }
    IEnumerator ResolveDestructiveWave(int caster)
    {
        if(DestructiveWaveError()!="")yield break;
        var targets=new List<int>(destructiveTargets);targets.Sort();var failed=new HashSet<int>();bool necrotic=destructiveNecrotic;
        int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        outcomeDeferDepth++;try{
            foreach(int id in targets){var save=new MappedSave();yield return RollMappedSpellSave(id,2,dc,0,"湮灭波",save);if(save.Failed)failed.Add(id);}
            if(targets.Count==0){history.Insert(0,"湮灭波：未选择目标，动作已消耗。");yield break;}
            int thunder=0,other=0;var a=new int[5];var b=new int[5];
            for(int i=0;i<5;i++){a[i]=Random.Range(1,7);b[i]=Random.Range(1,7);thunder+=a[i];other+=b[i];}
            yield return PresentRoll(6,a,"湮灭波 · 雷鸣5d6 = "+thunder);
            yield return PresentRoll(6,b,"湮灭波 · "+(necrotic?"死灵":"光耀")+"5d6 = "+other);
            var reduction=new RayDamage();yield return ReduceRayPacket(true,DestructiveWaveRules.Packet(thunder,other,necrotic),true,"湮灭波合并伤害",reduction);
            int playerDamage=0;
            foreach(int id in targets){string name=SpellTargetName(id);bool knocked=failed.Contains(id);var raw=DestructiveWaveRules.AfterSave(reduction.Raw,!knocked);int damage;
                if(id==0||id==-1){damage=ResolveRayPacket(id==0,raw);if(id==0){playerDamage=damage;combat.ApplyPlayerDamage(damage,sourcePlayer:true);if(knocked)combat.PlayerProne=true;}else{combat.ApplyDamage(damage);if(knocked)combat.MonsterProne=true;}}
                else {var unit=FindSummon(id);if(unit==null)continue;var immune=new bool[DamageTypes.Names.Length];immune[12]=SilenceUnitImmune(unit,"雷鸣");damage=DamageTypes.Total(DamageTypes.Resolve(raw,immunity:immune));unit.Damage(damage);if(knocked)unit.Prone=true;if(damage>0)BreakOrdinaryInvisibility(true,"造成伤害");}
                history.Insert(0,"湮灭波："+name+"承受 "+damage+" 点混合伤害"+(knocked?"，豁免失败并倒地。":"，豁免成功不倒地。"));
            }
            SyncTimedBuffs();if(playerDamage>0)yield return MaintainConcentration(playerDamage);RefreshBestialSpirit();dice="湮灭波 · "+targets.Count+"名目标已结算";
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
}
