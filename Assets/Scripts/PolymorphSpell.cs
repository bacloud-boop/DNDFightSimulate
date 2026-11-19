using System;
using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    readonly PolymorphEffect polymorph=new PolymorphEffect();
    bool PlayerPolymorphed=>polymorph.Active&&polymorph.TargetPlayer;
    bool PlayerInBeastForm=>PlayerPolymorphed||IsWildShaped;
    BeastRecord PlayerBeast=>PlayerPolymorphed?PolymorphBeast:wildForm;
    bool MonsterPolymorphed=>polymorph.Active&&!polymorph.TargetPlayer;
    string polymorphForm="Rat";
    Vector2Int polymorphAnchor;
    bool polymorphAnchorChosen,polymorphTargetDraft,polymorphFailSave;
    BeastRecord PolymorphBeast=>polymorph.Active?FindBeast(polymorph.Form):null;
    int EffectiveMonsterAbility(int ability)=>MonsterPolymorphed?polymorph.Ability(ability):monsterAttributes[ability];
    int MonsterSavingThrowModifier(int ability)=>MonsterPolymorphed?polymorph.Save(ability):AcidSplash.Modifier(monsterAttributes[ability]);
    int MonsterAttackReach=>MonsterPolymorphed?5:combat.MonsterAttackRange;
    bool MonsterMeleeAttack=>MonsterPolymorphed||combat.MonsterAttackIsMelee;
    MonsterDamage CurrentMonsterDamage()
    {
        if(!MonsterPolymorphed)return monster;
        var attack=BeastAttack.Find(polymorph.Form);var packet=new MonsterDamage();
        packet.Bonus=attack.Fixed;packet.Types[5]=DamageTypes.Index(attack.DamageType);
        for(int i=0;i<MonsterDamage.Sides.Length;i++){
            if(MonsterDamage.Sides[i]==attack.Sides){packet.Counts[i]+=attack.Count;packet.Types[i]=DamageTypes.Index(attack.DamageType);}
            if(MonsterDamage.Sides[i]==attack.ExtraSides){packet.Counts[i]+=attack.ExtraCount;packet.Types[i]=DamageTypes.Index("毒素");}
        }
        return packet;
    }
    string PolymorphError()
    {
        var form=FindBeast(polymorphForm);
        if(form==null||form.fly>0||BeastAttack.Find(form.name)==null)return "此野兽战斗动作尚未接入或属于暂缓的飞行形态。";
        if((directTargetPlayer||polymorphFailSave)&&!CanKeepCorpseInForm(directTargetPlayer,form))return "新形态无法继续搬运当前遗体，请先放下。";
        int target=PolymorphEffect.TargetChallengeLimit(directTargetPlayer?(int?)null:ChallengeRatings.Eighths(monsterProfile.ChallengeIndex),directTargetPlayer?classes.TotalLevel:0);
        if(!PolymorphEffect.AllowedChallenge((int)Math.Round(form.challenge*8),target))return "野兽CR高于目标CR或职业总等级。";
        if(polymorph.Active)return "请先结束现有变形术。";
        if(map.HasContainedCreature)return "请先完成被吞目标的放出流程。";
        if(!polymorphAnchorChosen||polymorphTargetDraft!=directTargetPlayer)return "请选择合法变形占地。";
        int original=directTargetPlayer?map.RiderBodySpaces:map.MonsterSpaces;
        return map.CanTransformAt(directTargetPlayer,BeastSpaces(form),polymorphAnchor,original)?"":"该占地无法容纳形态及原形恢复。";
    }
    void DrawPolymorphChoices()
    {
        if(polymorphTargetDraft!=directTargetPlayer){polymorphTargetDraft=directTargetPlayer;polymorphAnchorChosen=false;}
        GUILayout.Label("当前开放已有完整攻击数据的10种地面形态；其他野兽仍在接入。",small);
        GUILayout.Label("无CR且无职业等级的召唤物暂不能成为变形术目标；不将其视为CR 0。",small);
        polymorphFailSave=GUILayout.Toggle(polymorphFailSave,"目标自愿放弃本次豁免",label);
        foreach(var attack in BeastAttack.Supported){var form=FindBeast(attack.Form);if(form==null||form.fly>0)continue;
            if(GUILayout.Button((polymorphForm==form.name?"✓ ":"")+attack.Chinese+" · CR "+form.challengeLabel+" · 临时生命 "+form.hp,button)){polymorphForm=form.name;polymorphAnchorChosen=false;}
        }
        var selected=FindBeast(polymorphForm);if(selected==null)return;DrawBeastWeight(selected);
        var body=directTargetPlayer?map.Player:map.Monster;int bodySpaces=directTargetPlayer?map.RiderBodySpaces:map.MonsterSpaces;
        GUILayout.Label("变形占地（与当前占地重叠）",small);
        for(int y=body.y-BeastSpaces(selected)+1;y<body.y+bodySpaces;y++)for(int x=body.x-BeastSpaces(selected)+1;x<body.x+bodySpaces;x++){
            var cell=new Vector2Int(x,y);if(!map.CanTransformAt(directTargetPlayer,BeastSpaces(selected),cell,bodySpaces))continue;
            if(GUILayout.Button((polymorphAnchorChosen&&polymorphAnchor==cell?"✓ ":"")+"左上格 "+(x+1)+","+(y+1),button)){polymorphAnchor=cell;polymorphAnchorChosen=true;}
        }
    }
    IEnumerator ResolvePolymorph(bool player,int caster)
    {
        var form=FindBeast(polymorphForm);int original=player?map.RiderBodySpaces:map.MonsterSpaces;
        int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        EndConcentration("开始新的专注法术");
        var d20=RollD20Test(player,ability:3);int value=d20.Value,modifier=player?PlayerSavingThrowModifier(3):AcidSplash.Modifier(EffectiveMonsterAbility(3));
        if(!polymorphFailSave){var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,false,adjustment);modifier+=adjustment.Value;
        yield return PresentRoll(20,d20.Dice,"变形术 · 感知豁免 "+value+" + "+modifier+" / DC "+dc);}
        else history.Insert(0,"变形术：目标自愿放弃感知豁免。");
        if(!polymorphFailSave&&value+modifier>=dc){history.Insert(0,"变形术：目标豁免成功。");yield break;}
        if(!map.TransformAt(player,BeastSpaces(form),polymorphAnchor,original)){history.Insert(0,"变形术：占地失效，未替换形态。");yield break;}
        polymorph.Begin(player,form.name,worldRounds,form.armor,form.walk,form.darkvision,form.blindsight,BeastSpaces(form),form.abilities,form.saves,form.skillModifiers);
        if(player)RefreshShillelagh();
        concentrationSpell="变形术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+600;
        fighter.CancelAttackAction();actionWeapon=null;
        if(player)combat.SetPlayerArmorOverride(form.armor);else combat.MonsterFormArmor= form.armor;
        yield return OfferTemporaryHp(player,form.hp,"变形术");SyncTimedBuffs();
        history.Insert(0,"变形术："+(player?"玩家":"木桩")+"变为"+BeastAttack.Find(form.name).Chinese+"；保留生命、生命骰与生物类型，替换六属性与野兽数据，装备融入，不能施法或使用职业特性。");
        RefreshPolymorph();
    }
    void DrawPolymorphPreview(Rect board){if(panel!="spells"||selectedSpellName!="Polymorph"||!polymorphAnchorChosen)return;var form=FindBeast(polymorphForm);if(form!=null)Fill(BodyRect(board,polymorphAnchor,BeastSpaces(form)),PolymorphError()==""?new Color(.25f,.7f,.3f,.4f):new Color(.8f,.1f,.1f,.4f));}
    void RefreshPolymorph()
    {
        if(!polymorph.Active)return;
        bool player=polymorph.TargetPlayer;
        if(polymorph.ShouldEnd(worldRounds,player?combat.PlayerTemporaryHp:combat.MonsterTemporaryHp,player?combat.PlayerDead:combat.Hp==0))EndConcentration("变形持续时间结束、死亡或临时生命耗尽");
    }
    void EndPolymorph()
    {
        if(!polymorph.Active)return;
        bool player=polymorph.TargetPlayer;
        if(!map.RestoreForm(player)){history.Insert(0,"变形术复原占地暂不可用，等待放出流程。");return;}
        polymorph.Clear();
        if(player){combat.SetPlayerArmorOverride(IsWildShaped?(int?)wildForm.armor:null);wildNormalOffset=Vector2Int.zero;}
        else combat.MonsterFormArmor=null;
        fighter.CancelAttackAction();actionWeapon=null;SyncTimedBuffs();
        history.Insert(0,"变形术结束，恢复仍有效的底层形态；已有临时生命按通常规则保留。");
    }
}
