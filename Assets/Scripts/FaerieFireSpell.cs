using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class TrainingGame
{
    const int FaerieLightGroup=1;
    readonly FaerieFireEffect faerieFire=new FaerieFireEffect();
    int faerieColor,activeFaerieColor;
    bool FaerieOutlined(int actor)=>faerieFire.Outlined(actor,worldRounds);
    bool InvisibleWithFaerie(bool player)=>senses.Invisible(player,worldRounds)&&!FaerieOutlined(player?0:-1)&&!starryWisp.Suppresses(player?0:-1,worldRounds);
    bool FaerieAdvantage(int target,bool sees)=>faerieFire.AttackAdvantage(target,sees,worldRounds);
    string FaerieFireError(){Vector2Int anchor;if(!VisionPoint(out anchor))return "妖火立方区域左上格须位于地图内。";return map.DistanceToCell(true,anchor)>60?"妖火区域起点超出60尺。":worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";}
    void DrawFaerieFireChoices(){GUILayout.Label("20尺立方（平面4×4格），输入左上格列／行。区域内所有生物（包括自身及召唤物）进行敏捷豁免。离开初始区域不会结束勾勒。",small);visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);faerieColor=GUILayout.SelectionGrid(faerieColor,new[]{"蓝光","绿光","紫光"},3,button);}
    void EndFaerieFire(){faerieFire.Clear();vision.ClearLightGroup(FaerieLightGroup);}
    void RefreshFaerieFire()
    {
        if(!faerieFire.Active(worldRounds)){vision.ClearLightGroup(FaerieLightGroup);return;}
        var points=new List<double[]>();
        if(FaerieOutlined(0)&&!map.IsContained(true))AddFaeriePoints(points,map.IsCarried(true)?map.RescuePosition.Value:map.Player,map.IsCarried(true)?map.RescueSpaces:map.PlayerSpaces);
        if(FaerieOutlined(-1)&&!map.IsContained(false))AddFaeriePoints(points,map.IsCarried(false)?map.RescuePosition.Value:map.Monster,map.IsCarried(false)?map.RescueSpaces:map.MonsterSpaces);
        foreach(var unit in map.Summons)if(FaerieOutlined(unit.Id))AddFaeriePoints(points,new Vector2Int(unit.X,unit.Y),unit.Spaces);
        if(map.RescuePosition.HasValue&&FaerieOutlined(BattleMap.RescueId))AddFaeriePoints(points,map.RescuePosition.Value,map.RescueSpaces);
        if(!vision.SetDimLightGroup(FaerieLightGroup,points,10,faerieFire.Slot,faerieFire.Expires,worldRounds)){
            EndFaerieFire();if(concentrationSpell=="妖火")EndConcentration("妖火的光照与黑暗术重叠，低环光照法术被驱散");
        }
    }
    static void AddFaeriePoints(List<double[]> points,Vector2Int body,int spaces){for(int y=0;y<spaces;y++)for(int x=0;x<spaces;x++)points.Add(new[]{(body.x+x)*5.0,(body.y+y)*5.0});}
    IEnumerator ResolveFaerieFire(Vector2Int anchor,int caster,int slot)
    {
        if(!faerieFire.Begin(slot,worldRounds))yield break;
        activeFaerieColor=faerieColor;
        concentrationSpell="妖火";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=faerieFire.Expires;
        int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        var actors=new List<int>();if(!combat.PlayerDead&&!map.IsContained(true)&&!map.IsCarried(true)&&FaerieFireEffect.Intersects(anchor.x,anchor.y,map.Player.x,map.Player.y,map.PlayerSpaces))actors.Add(0);
        if(combat.Hp>0&&!map.IsContained(false)&&!map.IsCarried(false)&&FaerieFireEffect.Intersects(anchor.x,anchor.y,map.Monster.x,map.Monster.y,map.MonsterSpaces))actors.Add(-1);
        foreach(var unit in map.Summons)if(unit.Alive&&FaerieFireEffect.Intersects(anchor.x,anchor.y,unit.X,unit.Y,unit.Spaces))actors.Add(unit.Id);
        if(map.RescuePosition.HasValue&&!rescueCarrier.Dead&&FaerieFireEffect.Intersects(anchor.x,anchor.y,map.RescuePosition.Value.x,map.RescuePosition.Value.y,map.RescueSpaces))actors.Add(BattleMap.RescueId);
        foreach(int actor in actors){
            if(!faerieFire.Active(worldRounds))break;
            bool failed;string name;
            if(actor==0||actor==-1){bool player=actor==0;name=player?"玩家":"木桩";bool automatic=player?combat.PlayerHp==0||combat.PlayerParalyzed:combat.MonsterHelpless;var roll=RollArmorSave(player,1);int modifier=player?PlayerSavingThrowModifier(1):MonsterSavingThrowModifier(1);var adjust=new SaveAdjustment();yield return ApplySaveAdjustments(player,automatic,adjust,1,anchor);modifier+=adjust.Value;
                if(!automatic)yield return PresentRoll(20,roll.Dice,"妖火 · "+name+"敏捷豁免："+roll.Value+" + "+modifier+" / DC "+dc);failed=automatic||(long)roll.Value+modifier<dc;
            }else if(actor==BattleMap.RescueId){name="救援者";int raw=Random.Range(1,21);failed=rescueCarrier.Hp==0||rescueCarrier.Dead||(long)raw+rescueCarrier.DexteritySaveModifier<dc;if(rescueCarrier.Hp>0&&!rescueCarrier.Dead)yield return PresentRoll(20,new[]{raw},"妖火 · 救援者敏捷豁免："+raw+" + "+rescueCarrier.DexteritySaveModifier+" / DC "+dc);
            }else {var unit=FindSummon(actor);if(unit==null)continue;name=unit.Name;var save=new SummonSave();yield return RollSummonSave(unit,1,dc,anchor,save);failed=save.Failed;}
            if(failed)faerieFire.Bind(actor,worldRounds);
            history.Insert(0,"妖火："+name+(failed?"豁免失败，被光芒勾勒，不能获得隐形收益。":"豁免成功，未被勾勒。"));RefreshFaerieFire();
        }
        dice="妖火 · 敏捷豁免已结算";history.Insert(0,"妖火持续至多1分钟；被勾勒目标发出10尺微光，能看见目标的攻击者具有优势；环境物件勾勒保留说明，暂缓环境结算。");
    }
    void DrawFaerieFireMap(Rect board)
    {
        Vector2Int anchor=Vector2Int.zero;if(panel=="spells"&&selectedSpellName=="Faerie Fire"&&VisionPoint(out anchor))for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(FaerieFireEffect.Intersects(anchor.x,anchor.y,x,y,1))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.3f,.7f,.9f,.35f));
        Color glow=activeFaerieColor==0?new Color(.2f,.5f,1,.3f):activeFaerieColor==1?new Color(.2f,1,.4f,.3f):new Color(.8f,.3f,1,.3f);
        if(FaerieOutlined(0)&&!map.IsContained(true)&&!map.IsCarried(true))Fill(BodyRect(board,map.Player,map.PlayerSpaces),glow);
        if(FaerieOutlined(-1)&&!map.IsContained(false)&&!map.IsCarried(false))Fill(BodyRect(board,map.Monster,map.MonsterSpaces),glow);
        foreach(var unit in map.Summons)if(FaerieOutlined(unit.Id))Fill(BodyRect(board,new Vector2Int(unit.X,unit.Y),unit.Spaces),glow);
    }
}
