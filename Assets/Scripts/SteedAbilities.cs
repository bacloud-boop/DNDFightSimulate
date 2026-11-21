using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly Dictionary<int,int> steedFears=new Dictionary<int,int>();
    void EndSteedFearsAtPlayerTurnEnd(){var ended=new List<int>();foreach(var pair in steedFears)if(combat.Round>=pair.Value)ended.Add(pair.Key);foreach(int id in ended)steedFears.Remove(id);}
    bool SteedFear(int id)=>!HeroismImmune(id)&&steed!=null&&steed.Alive&&steedFears.ContainsKey(id);
    bool FearDisadvantage(bool player)=>FearDisadvantage(player?0:-1);
    bool FearDisadvantage(int id)
    {
        if(!SteedFear(id))return false;
        if(id==0||id==-1){var p=id==0?map.Player:map.Monster;return VisibilityBetween(id==0,p.x,p.y,id==0?map.PlayerSpaces:map.MonsterSpaces,steed.X,steed.Y,steed.Spaces)<2;}
        var unit=FindSummon(id);return unit!=null&&SpiritSees(unit,steed.Id,new Vector2Int(steed.X,steed.Y),steed.Spaces);
    }
    bool FearAllowsMovement(int id,Vector2Int cell)
    {
        if(!SteedFear(id))return true;
        Vector2Int from;int spaces;string name;if(!SummonTarget(id,out from,out spaces,out name))return false;
        var source=new Vector2Int(steed.X,steed.Y);int start=BattleMap.BodyDistance(from,spaces,source,steed.Spaces);
        int steps=Mathf.Max(Mathf.Abs(cell.x-from.x),Mathf.Abs(cell.y-from.y))*2;
        for(int n=1;n<=steps;n++){var p=new Vector2Int(Mathf.RoundToInt(Mathf.Lerp(from.x,cell.x,(float)n/steps)),Mathf.RoundToInt(Mathf.Lerp(from.y,cell.y,(float)n/steps)));if(BattleMap.BodyDistance(p,spaces,source,steed.Spaces)<start)return false;}
        return true;
    }
    void DrawSteedSpecial(SummonedUnit unit)
    {
        if(!IsSteed(unit))return;
        string name=steedRules.Kind==SteedKind.Celestial?"治疗之触":steedRules.Kind==SteedKind.Fey?"妖精步": "凶恶凝视";
        GUILayout.Label(name+" · 附赠动作 · "+(steedRules.SpecialAvailable?"长休次数1":"已使用"),small);
        if(steedRules.Kind==SteedKind.Fey){GUILayout.Label("传送落点左上格列／行（60尺）",small);steedTeleportColumn=IntegerField(steedTeleportColumn,false);steedTeleportRow=IntegerField(steedTeleportRow,false);}
        bool enabled=GUI.enabled;GUI.enabled=!summonActing&&unit.BonusAvailable&&steedRules.SpecialAvailable;
        if(GUILayout.Button("使用"+name,button))StartCoroutine(ResolveSteedSpecial(unit));GUI.enabled=enabled;
    }
    IEnumerator ResolveSteedSpecial(SummonedUnit unit)
    {
        if(!IsSteed(unit)||!unit.Alive||!unit.TurnActive||!unit.BonusAvailable||!steedRules.SpecialAvailable)yield break;
        Vector2Int cell;int spaces;string target;
        if(steedRules.Kind==SteedKind.Fey){int x,y;if(!int.TryParse(steedTeleportColumn,out x)||!int.TryParse(steedTeleportRow,out y)||!map.CanTeleportSummon(unit.Id,new Vector2Int(x-1,y-1),60)){dice="妖精步：目标须为60尺内可容纳完整占地的空位。";yield break;}
            if(!steedRules.TryUseSpecial(()=>unit.TryUseBonus()))yield break;
            map.TeleportSummon(unit.Id,new Vector2Int(x-1,y-1),60);history.Insert(0,"妖精步：坐骑与骑手共同传送，不引发借机攻击。");if(PendingTerrainCount>0)yield return DrainGreaseEntries();yield break;}
        int targetId=summonTarget;
        if(!SummonTarget(targetId,out cell,out spaces,out target)||BattleMap.BodyDistance(new Vector2Int(unit.X,unit.Y),unit.Spaces,cell,spaces)>(steedRules.Kind==SteedKind.Celestial?5:60)){dice="坐骑能力：目标不合法或超出距离。";yield break;}
        if(steedRules.Kind==SteedKind.Fiend&&!SpiritSees(unit,targetId,cell,spaces)){dice="凶恶凝视必须能看见目标。";yield break;}
        if(!steedRules.TryUseSpecial(()=>unit.TryUseBonus()))yield break;
        bool previous=summonActing,previousRolling=rolling;summonActing=true;rolling=true;
        try{if(steedRules.Kind==SteedKind.Celestial){int[] rolls={Random.Range(1,9),Random.Range(1,9)};int amount=rolls[0]+rolls[1]+steedRules.Slot;
                yield return PresentRoll(8,rolls,"治疗之触：2d8 + "+steedRules.Slot+" = "+amount);
                int healed=targetId==0?combat.HealPlayer(amount):targetId==-1?combat.HealMonster(amount):FindSummon(targetId).Heal(amount);history.Insert(0,"治疗之触："+target+"实际恢复 "+healed+" HP。");}
            else {if(HeroismImmune(targetId)){history.Insert(0,"凶恶凝视："+target+"受英雄气概保护，免疫恐慌。");yield break;}int dc=8+SummonAttackBonus(unit);bool failed;
                if(targetId==0||targetId==-1){bool player=targetId==0;var roll=RollArmorSave(player,3,avoidsOrEndsFrightened:true);int modifier=player?PlayerSavingThrowModifier(3):MonsterSavingThrowModifier(3);var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,false,adjustment,3);modifier+=adjustment.Value;
                    yield return PresentRoll(20,roll.Dice,"凶恶凝视 · "+target+"感知豁免 "+roll.Value+" + "+modifier+" / DC "+dc);failed=(long)roll.Value+modifier<dc;}
                else {var save=new SummonSave();yield return RollSummonSave(FindSummon(targetId),3,dc,new Vector2Int(unit.X,unit.Y),save);failed=save.Failed;}
                if(failed)steedFears[targetId]=combat.Round+1;history.Insert(0,"凶恶凝视："+target+(failed?"恐慌至玩家下个回合结束；看见坐骑时攻击与属性检定劣势，不能自愿靠近。":"豁免成功。"));}
        }finally{summonActing=previous;rolling=previousRolling;}
    }
}
