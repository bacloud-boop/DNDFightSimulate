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
    int energyChoice=2;bool energyTargetWilling;
    static readonly int[] EnergyTypes={3,4,5,7,12};
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
        GUILayout.Label("2024法术集 · "+spellMetadata.count+"项",heading);
        if(IsConcentrating&&GUILayout.Button("结束专注："+concentrationSpell,button))EndConcentration("玩家主动结束");
        GUILayout.Label("法术位资源暂不扣除；学习数量限制暂不启用。未接入条目不能施法。飞行相关暂缓。",small);
        spellSearch=GUILayout.TextField(spellSearch,fieldStyle);
        onlyClassSpells=GUILayout.Toggle(onlyClassSpells,"仅显示当前职业的法术",label);
        spellLibraryScroll=GUILayout.BeginScrollView(spellLibraryScroll);
        foreach(var spell in spellMetadata.spells){
            bool eligible=System.Linq.Enumerable.Any(EligibleCasters(spell));
            if(onlyClassSpells&&!eligible)continue;
            var direct=DirectSpell.Find(spell.name);
            string chinese=!string.IsNullOrEmpty(spell.chineseName)?spell.chineseName:direct!=null?direct.Chinese:spell.name=="Acid Splash"?"酸液飞溅":spell.name=="Alarm"?"警报术":spell.name=="Goodberry"?"神莓术":spell.name=="Find Familiar"?"召唤魔宠":spell.name=="Teleport"?"传送术":spell.name=="Plane Shift"?"异界传送":spell.name=="Gate"?"异界之门":"";
            if(spellSearch!=""&&(spell.name+chinese).IndexOf(spellSearch,StringComparison.OrdinalIgnoreCase)<0)continue;
            bool available=direct!=null||spell.name=="Acid Splash";
            bool reactionSpell=spell.name=="Shield";
            string status=reactionSpell?"反应触发时选择施放":available?"战斗结算已接入":IsLearningOnly(spell.name)?((spell.name=="Alarm"?alarmLearned:learnedUtilitySpells.Contains(spell.name))?"已学习 · 战斗禁用":"可学习 · 战斗禁用"):"待接入";
            string text=(chinese==""?spell.name:chinese+" · "+spell.name)+"\n"+(spell.level==0?"戏法":spell.level+"环")+" · "+SchoolChinese(spell.school)+" · "+status;
            string tooltip=reactionSpell?"护盾术｜Shield\n一环防护 · 反应 · 声音、姿势\n被攻击命中或成为魔法飞弹目标时选择施放。AC+5，免疫魔法飞弹，至下一玩家回合开始。选择前隐藏命中骰值与总值。":direct!=null?direct.Summary:spell.name=="Acid Splash"?AcidSplash.Summary:spell.name=="Alarm"?AlarmSpell.Summary:spell.name+"\n施法时间："+spell.castingTime+"\n距离："+spell.range+"\n成分："+string.Join("、",spell.components)+"\n持续："+spell.duration+"\n"+(IsLearningOnly(spell.name)?"仅开放学习，不能施放或带入预先生成的效果。":"效果待接入。");
            bool previous=GUI.enabled;GUI.enabled=eligible&&(available||(IsLearningOnly(spell.name)&&!combat.BattleStarted));
            if(GUILayout.Button(new GUIContent(text,tooltip),new GUIStyle(button){fixedHeight=76,fontSize=17})){
                if(IsLearningOnly(spell.name)){learnedUtilitySpells.Add(spell.name);if(spell.name=="Alarm")alarmLearned=true;}
                else {selectedSpellName=spell.name;directSlotLevel=Math.Max(1,spell.level);directTargetPlayer=direct!=null&&(direct.Healing||direct.Stable||direct.Name=="False Life"||IsTimedBuff(direct.Name)||direct.Name=="Protection from Energy"||direct.Name=="Blade Ward"||direct.Name=="Shield of Faith"||direct.Name=="Aid"||direct.Name=="Armor of Agathys"||direct.Name=="Bless");directBothTargets=false;energyTargetWilling=false;}
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
        if(definition.Name=="Power Word Fortify")DrawFortifyAllocation();
        else if(IsSimpleEmanation(definition.Name))DrawEmanationTargets(definition);
        else if(definition.Name=="Thunderwave")GUILayout.Label("确认后选择15尺立方方向并预览范围。",small);
        else if(definition.Name=="Scorching Ray")DrawMissileTargets(definition.DiceCount(classes.TotalLevel,directSlotLevel),"射线","每道射线单独攻击与伤害；先分配所有目标。");
        else if(definition.Name=="Magic Missile")DrawMissileTargets(definition.DiceCount(classes.TotalLevel,directSlotLevel));
        else {
        if(GUILayout.Button((directTargetPlayer?"✓ ":"")+"目标：玩家（自身）",button))directTargetPlayer=true;
        if(GUILayout.Button((!directTargetPlayer?"✓ ":"")+"目标：木桩 · "+monsterProfile.Label,button))directTargetPlayer=false;
        }
        if(definition.Name=="Bane"||definition.Name=="Bless"||definition.Name=="Aid"||definition.Name=="Mass Healing Word"||(definition.Control&&directSlotLevel>=3)||(definition.Name=="Longstrider"&&directSlotLevel>=2))directBothTargets=GUILayout.Toggle(directBothTargets,definition.Name=="Bane"||definition.Name=="Aid"||definition.Name=="Bless"?"同时选择双方（至多3名目标）":definition.Name=="Mass Healing Word"?"同时选择双方（至多6名目标）":"同时选中玩家与木桩（升环额外目标）",label);
        else directBothTargets=false;
        if(definition.Name=="Hex")hexAbility=GUILayout.SelectionGrid(hexAbility,PlayerAttributes.Names,3,new GUIStyle(button){fontSize=16,fixedHeight=34});
        if(definition.Name=="Barkskin"&&!directTargetPlayer)energyTargetWilling=GUILayout.Toggle(energyTargetWilling,"确认木桩自愿接受树肤术",label);
        if(definition.Name=="Protection from Energy"){energyChoice=GUILayout.SelectionGrid(energyChoice,new[]{"强酸","寒冷","火焰","闪电","雷鸣"},3,new GUIStyle(button){fontSize=16,fixedHeight=34});if(!directTargetPlayer)energyTargetWilling=GUILayout.Toggle(energyTargetWilling,"确认木桩自愿接受该法术",label);}
        string reason=DirectSpellInvalidReason(definition,casters.Length>0);
        GUILayout.Label(reason==""?"距离合法 · 消耗"+(definition.Bonus?"附赠动作":"动作"):reason,small);
        bool previous=GUI.enabled;GUI.enabled=reason=="";
        if(GUILayout.Button("确认施法",button)){panel="";if(definition.Name=="Thunderwave")BeginThunderwave(directCastingClass,directSlotLevel);else StartCoroutine(ResolveDirectSpell(definition,directTargetPlayer,directCastingClass,directSlotLevel));}
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
        if(spell.Name=="Armor of Agathys"&&!directTargetPlayer)return "黯冰狱铠只能作用于自身。";
        if(spell.Name=="Power Word Fortify")return FortifyInvalidReason();
        if(spell.Name=="Blade Ward"&&!directTargetPlayer)return "剑刃防护只能作用于自身。";
        if(spell.Name=="Barkskin"&&!directTargetPlayer&&!energyTargetWilling)return "树肤术需要自愿目标，请先确认。";
        if(spell.Name=="Protection from Energy"&&!directTargetPlayer&&!energyTargetWilling)return "此法术需要自愿目标，请确认木桩是否接受。";
        if((spell.Name=="Divine Favor"||spell.Name=="Magic Weapon")&&!directTargetPlayer)return "此法术当前仅可用于自身或玩家装备的武器。";
        if(spell.Name=="Magic Weapon"&&IsWildShaped)return "武器已融入兽形，当前不能触碰。";
        if(spell.Name=="Thunderwave"||IsSimpleEmanation(spell.Name))return "";
        if(spell.Name=="Magic Missile"||spell.Name=="Scorching Ray")return MissileTargetError(spell.DiceCount(classes.TotalLevel,directSlotLevel));
        if(spell.Name=="False Life"&&!directTargetPlayer)return "虚假生命只能以自身为目标。";
        int distance=directTargetPlayer?0:map.DistanceFeet;
        if(directBothTargets)distance=map.DistanceFeet;
        if(spell.Control&&(!directTargetPlayer||directBothTargets)&&!monsterProfile.IsHumanoid)return "目标类型不合法：木桩不是类人生物。";
        if(distance>spell.EffectiveRange(classes.TotalLevel))return "超出射程：目标 "+distance+" 尺。";
        if(directTargetPlayer?combat.PlayerDead:combat.Hp==0)return "死亡目标不能接受此法术。";
        if(spell.Stable&&(!directTargetPlayer||combat.PlayerHp!=0))return "维生术需要生命为0且未死亡的目标。";
        return "";
    }
    IEnumerator ResolveDirectSpell(DirectSpell spell,bool targetPlayer,int caster,int slotLevel,bool embedded=false)
    {
        if(!embedded&&!(spell.Bonus?combat.TryUseBonusAction():combat.TryUseMagicAction()))yield break;
        rolling=true;logScroll=Vector2.zero;
        int modifier=AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),bonus=classes.ProficiencyBonus+modifier;
        string target=targetPlayer?"玩家":"木桩";bool critical=false,saveSucceeded=false;int selfFrostRetaliation=0;
        if(IsSimpleEmanation(spell.Name)){yield return ResolveSimpleEmanation(spell,caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Magic Missile"){yield return ResolveMagicMissile(spell.DiceCount(classes.TotalLevel,slotLevel));if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Bane"){yield return ResolveBane(targetPlayer,caster,directBothTargets);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Hex"||spell.Name=="Hunter's Mark"){EndConcentration("开始新的专注法术");bool hex=spell.Name=="Hex";mark.Cast(hex,targetPlayer,hexAbility);concentrationSpell=spell.Chinese;concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+MarkEffect.Duration(hex,slotLevel);history.Insert(0,spell.Chinese+"标记"+target+"，攻击检定命中额外1d6"+(hex?"死灵；所选属性检定劣势（不影响豁免）":"力场；寻找目标的察觉/求生优势")+"。当前无技能检定/追踪系统，非战斗部分保留说明。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Bless"){EndConcentration("开始新的专注法术");concentrationSpell="祝福术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;if(targetPlayer||directBothTargets)combat.PlayerBlessed=true;if(!targetPlayer||directBothTargets)combat.MonsterBlessed=true;history.Insert(0,"祝福术：选定目标攻击与豁免每次+1d4，专注至多1分钟。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Armor of Agathys"){agathys.Cast(slotLevel,worldRounds);combat.GrantPlayerTemporaryHp(slotLevel*5);history.Insert(0,"黯冰狱铠：获得"+(slotLevel*5)+"临时生命，近战命中反击"+(slotLevel*5)+"寒冷，至多1小时，临时生命耗尽时结束。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Power Word Fortify"){ApplyFortify();if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Aid"){ApplyAid(targetPlayer,slotLevel,directBothTargets);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Shield of Faith"){EndConcentration("开始新的专注法术");concentrationSpell="虔诚护盾";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+100;if(targetPlayer)combat.PlayerArmorBonus=2;else combat.MonsterArmorBonus=2;history.Insert(0,target+"护甲等级+2，专注至多10分钟。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Blade Ward"){EndConcentration("开始新的专注法术");concentrationSpell="剑刃防护";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;combat.PlayerBladeWard=true;history.Insert(0,"剑刃防护：攻击玩家的检定减去1d4，专注至多1分钟。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Protection from Energy"){EndConcentration("开始新的专注法术");concentrationSpell="防护能量";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+600;if(targetPlayer)combat.PlayerEnergyResistance=EnergyTypes[energyChoice];else combat.MonsterEnergyResistance=EnergyTypes[energyChoice];history.Insert(0,target+"获得"+DamageTypes.Names[EnergyTypes[energyChoice]]+"抗性，专注至多1小时。");if(!embedded)rolling=false;yield break;}
        if(IsTimedBuff(spell.Name)){ApplyTimedBuff(spell,targetPlayer,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Scorching Ray"){yield return ResolveScorchingRays(caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Power Word Kill"&&(targetPlayer?combat.PlayerHp:combat.Hp)<=100){if(targetPlayer)combat.KillPlayer();else combat.KillMonster();history.Insert(0,"律令死亡："+target+"生命不超过100，直接死亡。");CheckBattleOutcome();if(!embedded)rolling=false;yield break;}
        if(spell.Control){yield return ResolveHoldPerson(spell,targetPlayer,caster,directBothTargets);if(!embedded)rolling=false;yield break;}
        if(spell.Stable){combat.StabilizePlayer();history.Insert(0,"维生术："+target+"稳定。");if(!embedded)rolling=false;yield break;}
        if(spell.Attack){
            // A hostile creature within 5 feet imposes ranged-attack disadvantage.
            bool disadvantage=!spell.Melee&&combat.MonsterCanAct&&map.DistanceFeet<=5;
            bool advantage=fighter.ConsumeAdvantage(classes[0])||(!targetPlayer&&(combat.MonsterParalyzed||(combat.MonsterProne&&map.DistanceFeet<=5)));
            advantage=combat.ConsumeGuidingLight(targetPlayer)||advantage;
            disadvantage=combat.ConsumeMockery(true)||disadvantage||(!targetPlayer&&combat.MonsterProne&&map.DistanceFeet>5);
            bool rollHigh=advantage&&!disadvantage,rollLow=disadvantage&&!advantage;
            int first=UnityEngine.Random.Range(1,21),second=rollHigh||rollLow?UnityEngine.Random.Range(1,21):0;
            int value=rollHigh?Math.Max(first,second):rollLow?Math.Min(first,second):first,armor=targetPlayer?combat.PlayerArmorClass:combat.MonsterArmorClass;
            int blessing=combat.PlayerBlessed?UnityEngine.Random.Range(1,5):0,bane=combat.PlayerBaned?UnityEngine.Random.Range(1,5):0;bonus+=blessing-bane;
            int wardPenalty=targetPlayer&&combat.PlayerBladeWard?UnityEngine.Random.Range(1,5):0;bonus-=wardPenalty;
            if(targetPlayer&&value!=1&&(value==20||value+bonus>=armor)){yield return OfferShield(true);armor=combat.PlayerArmorClass;}
            critical=value==20;dice=spell.Chinese+(spell.Melee?" · 近战法术攻击":" · 远程法术攻击");
            yield return PresentRoll(20,second>0?new[]{first,second}:new[]{first},(rollHigh?"优势，采用 ":rollLow?"劣势，采用 ":"采用 ")+value+" + "+bonus+" / 护甲 "+armor);
            if(blessing>0)yield return PresentRoll(4,new[]{blessing},"祝福术：本次攻击 +"+blessing);
            if(bane>0)yield return PresentRoll(4,new[]{bane},"灾祸术：本次攻击 -"+bane);
            if(wardPenalty>0)yield return PresentRoll(4,new[]{wardPenalty},"剑刃防护减值 "+wardPenalty);
            if(value==1||(!critical&&value+bonus<armor)){history.Insert(0,spell.Chinese+"对"+target+"：D20="+value+" + "+bonus+"，未命中。");fighter.RecordMiss(classes[0]);if(!embedded)rolling=false;yield break;}
        }
        if(spell.Attack&&targetPlayer&&spell.Melee)selfFrostRetaliation=AgathysRetaliation(true);
        if(spell.Attack&&!targetPlayer&&combat.MonsterParalyzed&&map.DistanceFeet<=5)critical=true;
        if(spell.SaveAbility>=0){
            int targetModifier=AcidSplash.Modifier((targetPlayer?attributes:monsterAttributes)[spell.SaveAbility]);
            if(targetPlayer)targetModifier=PlayerSavingThrowModifier(spell.SaveAbility);
            int value=UnityEngine.Random.Range(1,21),dc=8+bonus;bool automatic=(spell.SaveAbility==0||spell.SaveAbility==1)&&(targetPlayer?(combat.PlayerHp==0||combat.PlayerParalyzed):combat.MonsterParalyzed);
            automatic=automatic||(spell.Name=="Blight"&&!targetPlayer&&monsterProfile.TypeIndex==12);
            dice=spell.Chinese+" · "+target+"豁免";
            var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(targetPlayer,automatic,adjustment);targetModifier+=adjustment.Value;
            if(!automatic)yield return PresentRoll(20,new[]{value},value+" + "+targetModifier+" / DC "+dc);
            saveSucceeded=!automatic&&value+targetModifier>=dc;
            if(saveSucceeded&&!spell.HalfSave){history.Insert(0,spell.Chinese+"："+target+"豁免成功，伤害0。");if(!embedded)rolling=false;yield break;}
        }
        int damageSides=spell.Name=="Toll the Dead"&&(targetPlayer?combat.PlayerHp<combat.PlayerMaxHp:combat.Hp<combat.MaxHp)?12:spell.Sides;
        int count=spell.DiceCount(classes.TotalLevel,slotLevel)*(critical?2:1),amount=0;
        int[] rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=UnityEngine.Random.Range(1,damageSides+1);amount+=rolls[i];}
        if(spell.Healing)amount=Math.Max(0,amount+modifier);
        if(spell.Name=="False Life")amount+=4+5*(slotLevel-1);
        if(saveSucceeded&&spell.HalfSave)amount/=2;
        dice=spell.Chinese+(critical?" · 暴击":"")+" · "+count+"d"+damageSides;
        yield return PresentRoll(damageSides,rolls,"骰值 ["+string.Join("+",rolls)+"]"+(spell.Healing?" + 施法属性 "+modifier:spell.Name=="False Life"?" + 固定加值 "+(4+5*(slotLevel-1)):"")+(saveSucceeded&&spell.HalfSave?"，豁免成功减半向下取整":"")+" = "+amount);
        if(spell.Name=="False Life"){combat.GrantPlayerTemporaryHp(amount);history.Insert(0,"虚假生命：掷出临时生命 "+amount+"，当前保留 "+combat.PlayerTemporaryHp+"（不叠加）。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Mass Healing Word"&&directBothTargets){int playerHeal=combat.HealPlayer(amount),monsterHeal=combat.HealMonster(amount);history.Insert(0,"群体治愈真言：玩家实际恢复 "+playerHeal+"，木桩实际恢复 "+monsterHeal+"；使用同组治疗骰结果。");dice="群体治愈真言 · 已结算";if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Mind Sliver"){combat.ApplySliver(targetPlayer);history.Insert(0,target+"下一次豁免减1d4，至玩家下一回合结束失效。");}
        if(!spell.Healing)amount=combat.AdjustTypedDamage(targetPlayer,amount,DamageTypes.Index(spell.DamageType));
        if(spell.Attack){var extra=new ExtraMarkDamage();yield return RollMarkDamage(targetPlayer,critical,extra);amount+=extra.Value;}
        if(spell.Healing)amount=targetPlayer?combat.HealPlayer(amount):combat.HealMonster(amount);
        else if(targetPlayer){combat.ApplyPlayerDamage(amount,critical);int frost=ApplyAgathysRetaliation(selfFrostRetaliation,true);yield return MaintainConcentration(amount);if(frost>0)yield return MaintainConcentration(frost);}else combat.ApplyDamage(amount);
        if(spell.Name=="Guiding Bolt"){combat.ApplyGuidingLight(targetPlayer);history.Insert(0,"对"+target+"的下一次攻击具有优势，最迟在施法者下一回合结束失效。");}
        if(spell.Name=="Ray of Frost"){combat.ApplyFrostSlow(targetPlayer);history.Insert(0,target+"速度降低10尺，至玩家下一回合开始。");}
        if(spell.Name=="Shocking Grasp"){combat.BlockOpportunity(targetPlayer);history.Insert(0,target+"不能借机攻击，至其下一回合开始；其他反应不受影响。");}
        if(spell.Name=="Vicious Mockery"){combat.ApplyMockery(targetPlayer);history.Insert(0,target+"下次攻击具有劣势，最迟在其下一回合结束时失效。");}
        if(spell.Name=="Chill Touch"){combat.BlockHealing(targetPlayer);history.Insert(0,target+"不能恢复生命，持续至玩家下一回合结束。");}
        history.Insert(0,spell.Chinese+"对"+target+(spell.Healing?"实际治疗 ":"造成"+spell.DamageType+"伤害 ")+amount+"。");
        dice=spell.Chinese+" · "+amount;CheckBattleOutcome();if(!embedded)rolling=false;
    }
}
