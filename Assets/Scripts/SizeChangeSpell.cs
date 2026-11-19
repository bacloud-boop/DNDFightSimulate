using System;
using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly SizeChangeEffect sizeChange=new SizeChangeEffect();
    bool sizeEnlarge=true,sizeWilling,sizeAnchorChosen,sizeTargetDraft,sizeRestorePending;
    Vector2Int sizeAnchor;
    int BaseCreatureSize(bool player)=>player?(PlayerInBeastForm?BeastSizeIndex(PlayerBeast):2):(MonsterPolymorphed?BeastSizeIndex(PolymorphBeast):monsterProfile.SizeIndex);
    int ProspectiveSize(bool player)=>player?(IsWildShaped?BeastSizeIndex(wildForm):2):monsterProfile.SizeIndex;
    int EffectiveCreatureSize(bool player)=>sizeChange.Applies(player)?SizeChangeEffect.ChangedSize(BaseCreatureSize(player),sizeChange.Delta):BaseCreatureSize(player);
    bool SizeStrengthAdvantage(bool player,int ability)=>ability==0&&!sizeRestorePending&&sizeChange.Applies(player)&&sizeChange.Delta>0;
    bool SizeStrengthDisadvantage(bool player,int ability)=>ability==0&&!sizeRestorePending&&sizeChange.Applies(player)&&sizeChange.Delta<0;
    int SizeRecovery(bool player,int baseSize,int delta)=>Math.Max(CreatureProfile.SpacesForSize(baseSize),player?CreatureProfile.SpacesForSize(2+delta):1);
    bool FindSizePlacement(bool player,int spaces,int recovery,out Vector2Int result,Vector2Int? preferred=null)
    {
        var body=player?map.Player:map.Monster;int old=player?map.PlayerSpaces:map.MonsterSpaces;double best=double.MaxValue;result=body;
        for(int y=body.y-spaces+1;y<body.y+old;y++)for(int x=body.x-spaces+1;x<body.x+old;x++){
            var cell=new Vector2Int(x,y);if(!map.CanTransformAt(player,spaces,cell,recovery))continue;
            double dx=x+spaces*.5-body.x-old*.5,dy=y+spaces*.5-body.y-old*.5,d=dx*dx+dy*dy;
            if(preferred.HasValue&&cell==preferred.Value)d=-1;
            if(d<best){best=d;result=cell;}
        }return best<double.MaxValue;
    }
    string SizeChangeError(bool player)
    {
        if(sizeRestorePending)return "请先完成体型恢复。";
        if(player?combat.PlayerDead:combat.Hp==0)return "不能对死亡目标施放体型变化。";
        if(player?!CanSeeOwnBody():!PlayerSeesMonster)return "必须能看见目标。";
        if(!player&&map.DistanceFeet>30)return "目标超出30尺射程。";
        if(worldRounds<0||worldRounds>long.MaxValue-10)return "世界时间超出范围。";
        int delta=sizeEnlarge?1:-1,baseSize=ProspectiveSize(player),changed=SizeChangeEffect.ChangedSize(baseSize,delta);
        if(changed<0)return "体型已达边界，不能再次变巨／缩小；不消耗动作。";
        if(!sizeAnchorChosen||sizeTargetDraft!=player)return "请选择合法体型占地。";
        return map.CanTransformAt(player,CreatureProfile.SpacesForSize(changed),sizeAnchor,SizeRecovery(player,baseSize,delta))?"":"目标与恢复原形所需占地不合法。";
    }
    void DrawSizeChangeChoices()
    {
        if(sizeTargetDraft!=directTargetPlayer){sizeTargetDraft=directTargetPlayer;sizeAnchorChosen=false;}
        bool next=GUILayout.SelectionGrid(sizeEnlarge?0:1,new[]{"变巨","缩小"},2,button)==0;if(next!=sizeEnlarge){sizeEnlarge=next;sizeAnchorChosen=false;}
        sizeWilling=GUILayout.Toggle(sizeWilling,"目标自愿接受，不进行体质豁免",label);
        int baseSize=ProspectiveSize(directTargetPlayer),delta=sizeEnlarge?1:-1,changed=SizeChangeEffect.ChangedSize(baseSize,delta);
        if(changed<0){GUILayout.Label("已达体型边界，禁止施法且不消耗动作。",small);return;}
        GUILayout.Label(CreatureProfile.SizeNames[baseSize]+" → "+CreatureProfile.SizeNames[changed]+"；选择与原占地重叠的落位。装备重量不变，临时超载限制移动。",small);
        var body=directTargetPlayer?map.Player:map.Monster;int old=directTargetPlayer?map.PlayerSpaces:map.MonsterSpaces,spaces=CreatureProfile.SpacesForSize(changed);
        for(int y=body.y-spaces+1;y<body.y+old;y++)for(int x=body.x-spaces+1;x<body.x+old;x++){
            var cell=new Vector2Int(x,y);if(!map.CanTransformAt(directTargetPlayer,spaces,cell,SizeRecovery(directTargetPlayer,baseSize,delta)))continue;
            if(GUILayout.Button((sizeAnchorChosen&&sizeAnchor==cell?"✓ ":"")+"左上格 "+(x+1)+","+(y+1),button)){sizeAnchor=cell;sizeAnchorChosen=true;}
        }
    }
    void DrawSizeChangePreview(Rect board){if(panel!="spells"||selectedSpellName!="Enlarge/Reduce"||!sizeAnchorChosen)return;int changed=SizeChangeEffect.ChangedSize(ProspectiveSize(directTargetPlayer),sizeEnlarge?1:-1);if(changed>=0)Fill(BodyRect(board,sizeAnchor,CreatureProfile.SpacesForSize(changed)),SizeChangeError(directTargetPlayer)==""?new Color(.3f,.7f,.3f,.4f):new Color(.8f,.1f,.1f,.4f));}
    IEnumerator ResolveSizeChange(bool player,int caster,int delta,bool willing,Vector2Int anchor)
    {
        if(!willing){var roll=RollArmorSave(player,2);long dc=8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);int modifier=player?PlayerSavingThrowModifier(2):MonsterSavingThrowModifier(2);var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,false,adjustment);modifier+=adjustment.Value;
            yield return PresentRoll(20,roll.Dice,"变巨／缩小 · 体质豁免 "+roll.Value+" + "+modifier+" / DC "+dc);if((long)roll.Value+modifier>=dc){history.Insert(0,"变巨／缩小术：目标豁免成功，法术无效。");yield break;}}
        int baseSize=ProspectiveSize(player),changed=SizeChangeEffect.ChangedSize(baseSize,delta);if(changed<0)yield break;
        if(!map.CanTransformAt(player,CreatureProfile.SpacesForSize(changed),anchor,SizeRecovery(player,baseSize,delta))&&!FindSizePlacement(player,CreatureProfile.SpacesForSize(changed),SizeRecovery(player,baseSize,delta),out anchor))yield break;
        var reference=FormReferenceCell;if(!map.TransformAt(player,CreatureProfile.SpacesForSize(changed),anchor,SizeRecovery(player,baseSize,delta))){history.Insert(0,"体型占地在结算时失效，未改变体型。");yield break;}
        sizeChange.Begin(player,delta,worldRounds);if(player&&IsWildShaped)wildNormalOffset=reference-map.Player;
        concentrationSpell="变巨／缩小术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;fighter.CancelAttackAction();actionWeapon=null;SyncTimedBuffs();
        history.Insert(0,(player?"玩家":"木桩")+"体型变为"+CreatureProfile.SizeNames[changed]+"；力量检定与力量豁免"+(delta>0?"优势，武器或徒手命中额外1d4。":"劣势，武器或徒手命中减1d4，不能将伤害降至1以下。"));
    }
    void EndSizeChange()
    {
        if(!sizeChange.Present)return;
        bool player=sizeChange.Player;int spaces=CreatureProfile.SpacesForSize(BaseCreatureSize(player));Vector2Int anchor;
        var reference=FormReferenceCell;
        if(!FindSizePlacement(player,spaces,player&&IsWildShaped?1:spaces,out anchor)||!map.TransformAt(player,spaces,anchor,player&&IsWildShaped?1:spaces)){sizeRestorePending=true;return;}
        sizeChange.Clear();sizeRestorePending=false;if(player&&IsWildShaped)wildNormalOffset=reference-map.Player;
        map.TrySetRecoverySize(player,player&&IsWildShaped?1:0);history.Insert(0,"变巨／缩小术结束，恢复当前底层形态体型。");
    }
    bool FindSizedWildPlacement(BeastRecord form,out Vector2Int anchor)
    {
        anchor=map.Player;if(!sizeChange.Applies(true))return map.FindPlayerResizePlacement(BeastSpaces(form),out anchor,FormReferenceCell);
        int changed=SizeChangeEffect.ChangedSize(BeastSizeIndex(form),sizeChange.Delta);return changed>=0&&FindSizePlacement(true,CreatureProfile.SpacesForSize(changed),SizeRecovery(true,BeastSizeIndex(form),sizeChange.Delta),out anchor);
    }
    bool BeginSizedWildShape(BeastRecord form,out Vector2Int anchor,Vector2Int reference)
    {
        if(!sizeChange.Applies(true))return druid.TryBeginWildShape(classes[11],combat,map,BeastSpaces(form),out anchor,reference);
        if(!FindSizedWildPlacement(form,out anchor)||!combat.TryUseBonusAction())return false;
        druid.TryUse(classes[11]);return map.TransformAt(true,CreatureProfile.SpacesForSize(SizeChangeEffect.ChangedSize(BeastSizeIndex(form),sizeChange.Delta)),anchor,Math.Max(BeastSpaces(form),CreatureProfile.SpacesForSize(2+sizeChange.Delta)));
    }
    void RestoreSizedPlayerForm(Vector2Int reference)
    {
        if(!sizeChange.Applies(true)){map.TryResizePlayerAt(1,reference);return;}
        Vector2Int anchor;int spaces=CreatureProfile.SpacesForSize(2+sizeChange.Delta);
        if(FindSizePlacement(true,spaces,1,out anchor,reference))map.TransformAt(true,spaces,anchor,1);
    }
    int SizeLoadSpeed(bool player,int speed)
    {
        if(Entangled(player))return 0;
        if(!sizeChange.Applies(player))return speed;
        var load=player?playerCorpseLoad:monsterCorpseLoad;long gear=player?PlayerEquipmentWeight:MonsterEquipmentWeight;
        return CarryingRules.DragSpeedOunces(player?EffectivePlayerAbility(0):EffectiveMonsterAbility(0),EffectiveCreatureSize(player),gear+(load.Loaded?load.WeightOunces:0),speed);
    }
    IEnumerator ApplySizeWeaponDamage(bool player,long[] raw,int type,bool critical,RayDamage result)
    {
        result.Raw=(long[])raw.Clone();if(sizeRestorePending||!sizeChange.Applies(player))yield break;
        int[] rolls=new int[sizeChange.Delta>0&&critical?2:1];int amount=0;for(int i=0;i<rolls.Length;i++){rolls[i]=UnityEngine.Random.Range(1,5);amount+=rolls[i];}
        yield return PresentRoll(4,rolls,(sizeChange.Delta>0?"变巨：额外":"缩小：减少")+amount+"武器伤害"+(sizeChange.Delta<0?"，不将原有正伤害降至1以下":" · "+DamageTypes.Names[type]));
        if(sizeChange.Delta>0)result.Raw[type]+=amount;
        else {var reduced=new DamageReductionResult();yield return AllocateDamageReduction(raw,amount,"缩小术武器伤害",reduced,1);result.Raw=reduced.Raw;}
    }
}
