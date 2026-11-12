using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class TrainingGame
{
    SpellMetadataSet spellMetadata;
    string spellSearch="",selectedSpellName="";
    Vector2 spellLibraryScroll,directSpellScroll;
    bool onlyClassSpells=true;
    readonly HashSet<string> learnedUtilitySpells=new HashSet<string>();
    static bool IsLearningOnly(string name) => name=="Alarm"||name=="Goodberry"||name=="Find Familiar"||name=="Teleport"||name=="Plane Shift"||name=="Gate";
    int directCastingClass=1,directSlotLevel=1;
    bool directTargetPlayer,directBothTargets;
    static readonly string[] EnglishClassNames={"Fighter","Wizard","Ranger","Sorcerer","Warlock","Bard","Cleric","Rogue","Monk","Paladin","Barbarian","Druid"};
    IEnumerable<int> EligibleCasters(SpellMetadata spell)
    {
        for(int i=0;i<EnglishClassNames.Length;i++)if(classes[i]>0&&(Array.IndexOf(spell.classes,EnglishClassNames[i])>=0||(i==11&&classes[11]>=2&&spell.name=="Find Familiar")))yield return i;
    }
    int SpellAbilityIndex(int classIndex) => classIndex==1?4:classIndex==2||classIndex==6||classIndex==11?3:5;
    static string SchoolChinese(string school)
    {
        switch(school){case "Abjuration":return "防护";case "Conjuration":return "咒法";case "Divination":return "预言";case "Enchantment":return "惑控";case "Evocation":return "塑能";case "Illusion":return "幻术";case "Necromancy":return "死灵";case "Transmutation":return "变化";default:return school;}
    }
    void DrawSpellPanel()
    {
        if(spellMetadata==null)spellMetadata=JsonUtility.FromJson<SpellMetadataSet>(Resources.Load<TextAsset>("Rules2024/Spells").text);
        if(selectedSpellName=="Acid Splash"){DrawAcidSpellPanel();return;}
        if(selectedSpellName!=""){DrawDirectSpellPanel();return;}
        Surface(new Rect(350,110,480,745),paperTexture);
        if(Btn(new Rect(720,125,90,46),"关闭")){panel="";return;}
        GUILayout.BeginArea(new Rect(375,185,430,635));
        GUILayout.Label("免费法术集 · "+spellMetadata.count+"项",heading);
        if(IsConcentrating&&GUILayout.Button("结束专注："+concentrationSpell,button))EndConcentration("玩家主动结束");
        GUILayout.Label("法术位资源暂不扣除；学习数量限制暂不启用。未接入条目不能施法。飞行相关暂缓。",small);
        spellSearch=GUILayout.TextField(spellSearch,fieldStyle);
        onlyClassSpells=GUILayout.Toggle(onlyClassSpells,"仅显示当前职业的法术",label);
        spellLibraryScroll=GUILayout.BeginScrollView(spellLibraryScroll);
        foreach(var spell in spellMetadata.spells){
            bool eligible=System.Linq.Enumerable.Any(EligibleCasters(spell));
            if(onlyClassSpells&&!eligible)continue;
            var direct=DirectSpell.Find(spell.name);
            string chinese=direct!=null?direct.Chinese:spell.name=="Acid Splash"?"酸液飞溅":spell.name=="Alarm"?"警报术":spell.name=="Goodberry"?"神莓术":spell.name=="Find Familiar"?"召唤魔宠":spell.name=="Teleport"?"传送术":spell.name=="Plane Shift"?"异界传送":spell.name=="Gate"?"异界之门":"";
            if(spellSearch!=""&&(spell.name+chinese).IndexOf(spellSearch,StringComparison.OrdinalIgnoreCase)<0)continue;
            bool available=direct!=null||spell.name=="Acid Splash";
            string status=available?"战斗结算已接入":IsLearningOnly(spell.name)?((spell.name=="Alarm"?alarmLearned:learnedUtilitySpells.Contains(spell.name))?"已学习 · 战斗禁用":"可学习 · 战斗禁用"):"待接入";
            string text=(chinese==""?spell.name:chinese+" · "+spell.name)+"\n"+(spell.level==0?"戏法":spell.level+"环")+" · "+SchoolChinese(spell.school)+" · "+status;
            string tooltip=direct!=null?direct.Summary:spell.name=="Acid Splash"?AcidSplash.Summary:spell.name=="Alarm"?AlarmSpell.Summary:spell.name+"\n施法时间："+spell.castingTime+"\n距离："+spell.range+"\n成分："+string.Join("、",spell.components)+"\n持续："+spell.duration+"\n"+(IsLearningOnly(spell.name)?"仅开放学习，不能施放或带入预先生成的效果。":"效果待接入。");
            bool previous=GUI.enabled;GUI.enabled=eligible&&(available||(IsLearningOnly(spell.name)&&!combat.BattleStarted));
            if(GUILayout.Button(new GUIContent(text,tooltip),new GUIStyle(button){fixedHeight=76,fontSize=17})){
                if(IsLearningOnly(spell.name)){learnedUtilitySpells.Add(spell.name);if(spell.name=="Alarm")alarmLearned=true;}
                else {selectedSpellName=spell.name;directSlotLevel=Math.Max(1,spell.level);directTargetPlayer=direct!=null&&(direct.Healing||direct.Stable);directBothTargets=false;}
            }
            GUI.enabled=previous;GUILayout.Space(6);
        }
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
    SpellMetadata SelectedSpellMetadata()
    {
        foreach(var spell in spellMetadata.spells)if(spell.name==selectedSpellName)return spell;
        return null;
    }
    void DrawDirectSpellPanel()
    {
        var definition=DirectSpell.Find(selectedSpellName);var metadata=SelectedSpellMetadata();
        if(definition==null||metadata==null){selectedSpellName="";return;}
        Surface(new Rect(350,110,480,745),paperTexture);
        if(Btn(new Rect(720,125,90,46),"返回")){selectedSpellName="";return;}
        GUILayout.BeginArea(new Rect(375,185,430,635));
        directSpellScroll=GUILayout.BeginScrollView(directSpellScroll);
        GUILayout.Label(definition.Chinese,heading);GUILayout.Label(definition.Summary,label);
        var casters=System.Linq.Enumerable.ToArray(EligibleCasters(metadata));
        if(Array.IndexOf(casters,directCastingClass)<0&&casters.Length>0)directCastingClass=casters[0];
        foreach(int caster in casters)if(GUILayout.Button((caster==directCastingClass?"✓ ":"")+PlayerClasses.Names[caster]+" · "+PlayerAttributes.Names[SpellAbilityIndex(caster)],button))directCastingClass=caster;
        int modifier=AcidSplash.Modifier(attributes[SpellAbilityIndex(directCastingClass)]);
        GUILayout.Label("施法调整值 "+modifier+" · 法术命中 +"+(classes.ProficiencyBonus+modifier)+" · 豁免DC "+(8+classes.ProficiencyBonus+modifier),small);
        if(definition.Level>0){GUILayout.Label("施法环阶（法术位资源暂不扣除）",small);directSlotLevel=GUILayout.SelectionGrid(directSlotLevel-definition.Level,System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(System.Linq.Enumerable.Range(definition.Level,10-definition.Level),x=>x.ToString())),Math.Min(9,10-definition.Level),new GUIStyle(button){fontSize=16,fixedHeight=34})+definition.Level;}
        if(GUILayout.Button((directTargetPlayer?"✓ ":"")+"目标：玩家（自身）",button))directTargetPlayer=true;
        if(GUILayout.Button((!directTargetPlayer?"✓ ":"")+"目标：木桩 · "+monsterProfile.Label,button))directTargetPlayer=false;
        if(definition.Control&&directSlotLevel>=3)directBothTargets=GUILayout.Toggle(directBothTargets,"同时选中玩家与木桩（升环额外目标）",label);
        else directBothTargets=false;
        string reason=DirectSpellInvalidReason(definition,casters.Length>0);
        GUILayout.Label(reason==""?"距离合法 · 消耗"+(definition.Bonus?"附赠动作":"动作"):reason,small);
        bool previous=GUI.enabled;GUI.enabled=reason=="";
        if(GUILayout.Button("确认施法",button)){panel="";StartCoroutine(ResolveDirectSpell(definition,directTargetPlayer,directCastingClass,directSlotLevel));}
        GUI.enabled=previous;GUILayout.EndScrollView();GUILayout.EndArea();
    }
    string DirectSpellInvalidReason(DirectSpell spell,bool eligible)
    {
        if(!eligible)return "当前职业不可施放。";
        if(page!="battle"||!map.Locked)return "部署完成后可施法。";
        if(rolling||spellTargeting||draggingToken>=0)return "请等待当前操作结束。";
        if(!combat.PlayerCanAct)return "玩家当前不能行动。";
        if(!CanCastInCurrentForm)return "荒野形态不能施法（德鲁伊18级解锁兽形施法）。";
        if(spell.Bonus?!combat.BonusActionAvailable:!combat.MagicActionAvailable)return "没有可用的"+(spell.Bonus?"附赠动作":"施法动作")+"。";
        int distance=directTargetPlayer?0:map.DistanceFeet;
        if(directBothTargets)distance=map.DistanceFeet;
        if(spell.Control&&(!directTargetPlayer||directBothTargets)&&!monsterProfile.IsHumanoid)return "目标类型不合法：木桩不是类人生物。";
        if(distance>spell.EffectiveRange(classes.TotalLevel))return "超出射程：目标 "+distance+" 尺。";
        if(directTargetPlayer?combat.PlayerDead:combat.Hp==0)return "死亡目标不能接受此法术。";
        if(spell.Stable&&(!directTargetPlayer||combat.PlayerHp!=0))return "维生术需要生命为0且未死亡的目标。";
        return "";
    }
    IEnumerator ResolveDirectSpell(DirectSpell spell,bool targetPlayer,int caster,int slotLevel)
    {
        if(!(spell.Bonus?combat.TryUseBonusAction():combat.TryUseMagicAction()))yield break;
        rolling=true;logScroll=Vector2.zero;
        int modifier=AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),bonus=classes.ProficiencyBonus+modifier;
        string target=targetPlayer?"玩家":"木桩";bool critical=false,saveSucceeded=false;
        if(spell.Name=="Power Word Kill"&&(targetPlayer?combat.PlayerHp:combat.Hp)<=100){if(targetPlayer)combat.KillPlayer();else combat.ApplyDamage(combat.Hp);history.Insert(0,"律令死亡："+target+"生命不超过100，直接死亡。");CheckBattleOutcome();rolling=false;yield break;}
        if(spell.Control){yield return ResolveHoldPerson(spell,targetPlayer,caster,directBothTargets);rolling=false;yield break;}
        if(spell.Stable){combat.StabilizePlayer();history.Insert(0,"维生术："+target+"稳定。");rolling=false;yield break;}
        if(spell.Attack){
            // A hostile creature within 5 feet imposes ranged-attack disadvantage.
            bool disadvantage=combat.MonsterCanAct&&map.DistanceFeet<=5;
            bool advantage=fighter.ConsumeAdvantage(classes[0])||(!targetPlayer&&(combat.MonsterParalyzed||(combat.MonsterProne&&map.DistanceFeet<=5)));
            disadvantage=disadvantage||(!targetPlayer&&combat.MonsterProne&&map.DistanceFeet>5);
            bool rollHigh=advantage&&!disadvantage,rollLow=disadvantage&&!advantage;
            int first=UnityEngine.Random.Range(1,21),second=rollHigh||rollLow?UnityEngine.Random.Range(1,21):0;
            int value=rollHigh?Math.Max(first,second):rollLow?Math.Min(first,second):first,armor=targetPlayer?combat.PlayerArmorClass:combat.MonsterArmorClass;
            critical=value==20;dice=spell.Chinese+" · 远程法术攻击";
            yield return PresentRoll(20,second>0?new[]{first,second}:new[]{first},(rollHigh?"优势，采用 ":rollLow?"邻近敌人：劣势，采用 ":"采用 ")+value+" + "+bonus+" / 护甲 "+armor);
            if(value==1||(!critical&&value+bonus<armor)){history.Insert(0,spell.Chinese+"对"+target+"：D20="+value+" + "+bonus+"，未命中。");fighter.RecordMiss(classes[0]);rolling=false;yield break;}
        }
        if(spell.Attack&&!targetPlayer&&combat.MonsterParalyzed&&map.DistanceFeet<=5)critical=true;
        if(spell.SaveAbility>=0){
            int targetModifier=AcidSplash.Modifier((targetPlayer?attributes:monsterAttributes)[spell.SaveAbility]);
            if(targetPlayer)targetModifier=PlayerSavingThrowModifier(spell.SaveAbility);
            int value=UnityEngine.Random.Range(1,21),dc=8+bonus;bool automatic=(spell.SaveAbility==0||spell.SaveAbility==1)&&(targetPlayer?(combat.PlayerHp==0||combat.PlayerParalyzed):combat.MonsterParalyzed);
            dice=spell.Chinese+" · "+target+"豁免";
            if(!automatic)yield return PresentRoll(20,new[]{value},value+" + "+targetModifier+" / DC "+dc);
            saveSucceeded=!automatic&&value+targetModifier>=dc;
            if(saveSucceeded&&!spell.HalfSave){history.Insert(0,spell.Chinese+"："+target+"豁免成功，伤害0。");rolling=false;yield break;}
        }
        int count=spell.DiceCount(classes.TotalLevel,slotLevel)*(critical?2:1),amount=0;
        int[] rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=UnityEngine.Random.Range(1,spell.Sides+1);amount+=rolls[i];}
        if(spell.Healing)amount=Math.Max(0,amount+modifier);
        if(saveSucceeded&&spell.HalfSave)amount/=2;
        dice=spell.Chinese+(critical?" · 暴击":"")+" · "+count+"d"+spell.Sides;
        yield return PresentRoll(spell.Sides,rolls,"骰值 ["+string.Join("+",rolls)+"]"+(spell.Healing?" + 施法属性 "+modifier:"")+(saveSucceeded&&spell.HalfSave?"，豁免成功减半向下取整":"")+" = "+amount);
        if(spell.Healing)amount=targetPlayer?combat.HealPlayer(amount):combat.HealMonster(amount);
        else if(targetPlayer){combat.ApplyPlayerDamage(amount,critical);yield return MaintainConcentration(amount);}else combat.ApplyDamage(amount);
        history.Insert(0,spell.Chinese+"对"+target+(spell.Healing?"实际治疗 ":"造成"+spell.DamageType+"伤害 ")+amount+"。");
        dice=spell.Chinese+" · "+amount;CheckBattleOutcome();rolling=false;
    }
}
