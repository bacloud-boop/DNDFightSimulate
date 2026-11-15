using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly List<int> monsterAreaPreviewTargets=new List<int>();
    bool monsterAreaPreview,monsterAreaChoosing;
    Vector2Int monsterAreaCenter;
    int monsterAreaRadius,monsterAreaOriginSpaces=1;
    List<int> MonsterAreaTargets(Vector2Int center,int radius,int spaces=1,bool sight=false)
    {
        var targets=new List<int>();
        if(!combat.PlayerDead&&!map.IsContained(true)&&BattleMap.BodyDistance(center,spaces,map.Player,map.PlayerSpaces)<=radius&&(!sight||MonsterSeesPlayer))targets.Add(0);
        return targets;
    }
    bool TryPlanMonsterArea(DirectSpell spell,out Vector2Int center)
    {
        int x,y,radius=MonsterSpellCatalog.Radius(spell);
        if(spell.EmanationRadius>0){center=map.Monster;return MonsterAreaTargets(center,radius,map.MonsterSpaces,spell.Name=="Word of Radiance").Count>0;}
        bool found=MonsterAreaPlanner.Choose(map.Size,spell.Range,radius,(column,row)=>map.DistanceToCell(false,new Vector2Int(column,row)),(column,row)=>MonsterAreaTargets(new Vector2Int(column,row),radius).Count,out x,out y);
        center=new Vector2Int(x,y);return found;
    }
    void ShowMonsterArea(DirectSpell spell,Vector2Int center)
    {
        monsterAreaCenter=center;monsterAreaRadius=MonsterSpellCatalog.Radius(spell);monsterAreaOriginSpaces=spell.EmanationRadius>0?map.MonsterSpaces:1;monsterAreaPreview=true;
        monsterAreaPreviewTargets.Clear();monsterAreaPreviewTargets.AddRange(MonsterAreaTargets(center,monsterAreaRadius,monsterAreaOriginSpaces,spell.Name=="Word of Radiance"));
        history.Insert(0,"木桩范围施法中心：第"+(center.x+1)+"列，第"+(center.y+1)+"行；半径 "+monsterAreaRadius+"尺，覆盖 "+monsterAreaPreviewTargets.Count+"名敌人，避开自身。");
    }
    void DrawMonsterArea(Rect board)
    {
        if(!monsterAreaPreview)return;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(!BattleMap.Overlap(map.Monster,map.MonsterSpaces,new Vector2Int(x,y),1)&&BattleMap.BodyDistance(monsterAreaCenter,monsterAreaOriginSpaces,new Vector2Int(x,y),1)<=monsterAreaRadius)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.9f,.18f,.08f,.32f));
    }
    IEnumerator ResolveMonsterAreaSpell(DirectSpell spell,int slot,Vector2Int center)
    {
        int radius=MonsterSpellCatalog.Radius(spell);var targets=MonsterAreaTargets(center,radius,spell.EmanationRadius>0?map.MonsterSpaces:1,spell.Name=="Word of Radiance");var failed=new HashSet<int>();
        foreach(int id in targets){
            bool player=id==0;
            bool automatic=player&&spell.SaveAbility==1&&(combat.PlayerHp==0||combat.PlayerParalyzed);
            int first=Random.Range(1,21),second=0,value=first;
            int modifier=PlayerSavingThrowModifier(spell.SaveAbility);
            if(player){
                modifier=PlayerSavingThrowModifier(spell.SaveAbility);
                var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(true,automatic,adjustment);modifier+=adjustment.Value;
            }
            dice="木桩"+spell.Chinese+" · "+"玩家"+PlayerAttributes.Names[spell.SaveAbility]+"豁免";
            if(!automatic)yield return PresentRoll(20,second==0?new[]{first}:new[]{first,second},"采用 "+value+" + "+modifier+" / DC "+MonsterSpellDc);
            if(automatic||(long)value+modifier<MonsterSpellDc)failed.Add(id);
        }
        int count=spell.DiceCount(monsterSpells.CasterLevel,slot),damage=0;var rolls=new int[count];
        for(int i=0;i<count;i++){rolls[i]=Random.Range(1,spell.Sides+1);damage+=rolls[i];}
        dice="木桩"+spell.Chinese+" · 共享范围伤害";yield return PresentRoll(spell.Sides,rolls,"失败 "+damage+"；成功 "+(spell.HalfSave?damage/2:0)+" "+spell.DamageType);
        // Resolve simultaneous area damage before concentration can dismiss units.
        int playerDamage=0;
        foreach(int id in targets){
            int raw=failed.Contains(id)?damage:spell.HalfSave?damage/2:0;
            if(id==0){playerDamage=combat.AdjustTypedDamage(true,raw,DamageTypes.Index(spell.DamageType));combat.ApplyPlayerDamage(playerDamage);history.Insert(0,"木桩"+spell.Chinese+"对玩家造成 "+playerDamage+" "+spell.DamageType+"伤害。");}
        }
        if(targets.Contains(0))yield return MaintainConcentration(playerDamage);
    }
}
