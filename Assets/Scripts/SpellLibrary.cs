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
    bool trueStrikeRadiant=true;
    int energyChoice=2;bool energyTargetWilling;
    static readonly int[] EnergyTypes={3,4,5,7,12};
    readonly HashSet<string> learnedUtilitySpells=new HashSet<string>();
    static bool IsLearningOnly(string name) => name=="Alarm"||name=="Goodberry"||name=="Find Familiar"||name=="Teleport"||name=="Plane Shift"||name=="Gate"||name=="Spider Climb"||name=="Control Water";
    static bool AftermathLightSpell(string name)=>name=="Prayer of Healing"||name=="Fire Shield"||name=="Flame Blade"||name=="Regenerate"||name=="Dragon’s Breath"||name=="Light"||name=="Dancing Lights"||name=="Feign Death"||name=="Water Walk"||name=="Freedom of Movement"||name=="Aura of Vitality"||name=="Beacon of Hope"||name=="Pass without Trace"||name=="Continual Flame"||name=="Produce Flame"||name=="Animate Dead";
    int directCastingClass=1,directSlotLevel=1;
    bool directTargetPlayer,directBothTargets;
    static readonly string[] EnglishClassNames={"Fighter","Wizard","Ranger","Sorcerer","Warlock","Bard","Cleric","Rogue","Monk","Paladin","Barbarian","Druid"};
    IEnumerable<int> EligibleCasters(SpellMetadata spell)
    {
        for(int i=0;i<EnglishClassNames.Length;i++)if(classes[i]>0&&(Array.IndexOf(spell.classes,EnglishClassNames[i])>=0||(i==11&&classes[11]>=2&&spell.name=="Find Familiar")))yield return i;
        if(PlayerSpeciesSpell(spell.name)!=null)yield return SpeciesSpellRules.Caster;
    }
    int SpellAbilityIndex(int classIndex) => classIndex==SpeciesSpellRules.Caster?SpeciesSpellRules.Ability(playerSpecies):classIndex==1?4:classIndex==2||classIndex==6||classIndex==11?3:5;
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
        GUILayout.Label("2024法术集 · "+spellMetadata.count+"项",heading);DrawAnimateChannelControls();DrawRegenerateChannel();DrawPrayerChannel();
        if(witchBolt.Active(worldRounds)&&concentrationSpell=="巫术箭"&&GUILayout.Button("巫术箭放电（附赠动作）",button))selectedSpellName="Witch Bolt";
        if(VampiricTouchActive&&GUILayout.Button("使用现存吸血之触（魔法动作）",button))selectedSpellName="Vampiric Touch";
        if(IsConcentrating&&GUILayout.Button("结束专注："+concentrationSpell,button))EndConcentration("玩家主动结束");
        if(flameBlade.Active(worldRounds)&&GUILayout.Button("使用／重召焰刃",button))selectedSpellName="Flame Blade";
        if(flamingSphere.Active(worldRounds)&&GUILayout.Button("滚动炽焰法球（附赠动作）",button))OpenFlamingSphereControl();
        if(heatMetal.Active(worldRounds)&&GUILayout.Button("再次加热灼热金属（附赠动作）",button))panel="heatMetal";
        if(callLightning.Active(worldRounds)&&GUILayout.Button("控制现存召雷术（魔法动作）",button))selectedSpellName="Call Lightning";
        if(animalPack.Active(worldRounds)&&GUILayout.Button("控制现存兽群（本回合共30尺）",button))OpenAnimalPackControl();
        if(cloudDaggers.Active(worldRounds)&&GUILayout.Button("传送现存匕首之云（魔法动作）",button))OpenCloudDaggersControl();
        if(dancingLights.Active(worldRounds)&&GUILayout.Button("移动现存舞光（附赠动作）",button))OpenDancingLightsControl();
        GUILayout.Label("法术位资源暂不扣除；学习数量限制暂不启用。未接入条目不能施法。飞行相关暂缓。",small);
        spellSearch=GUILayout.TextField(spellSearch,fieldStyle);
        onlyClassSpells=GUILayout.Toggle(onlyClassSpells,"仅显示当前职业与种族的法术",label);
        spellLibraryScroll=GUILayout.BeginScrollView(spellLibraryScroll);
        foreach(var spell in spellMetadata.spells){
            bool eligible=System.Linq.Enumerable.Any(EligibleCasters(spell));
            if(onlyClassSpells&&!eligible)continue;
            var direct=DirectSpell.Find(spell.name);
            string chinese=!string.IsNullOrEmpty(spell.chineseName)?spell.chineseName:direct!=null?direct.Chinese:spell.name=="Acid Splash"?"酸液飞溅":spell.name=="Alarm"?"警报术":spell.name=="Goodberry"?"神莓术":spell.name=="Find Familiar"?"召唤魔宠":spell.name=="Teleport"?"传送术":spell.name=="Plane Shift"?"异界传送":spell.name=="Gate"?"异界之门":"";
            if(spellSearch!=""&&(spell.name+chinese).IndexOf(spellSearch,StringComparison.OrdinalIgnoreCase)<0)continue;
            bool available=direct!=null||spell.name=="Acid Splash";
            bool reactionSpell=spell.name=="Shield"||spell.name=="Counterspell"||spell.name=="Hellish Rebuke";
            string status=reactionSpell?"反应触发时选择施放":available?(spell.name=="Reincarnate"?"复生与新躯体已接入 · 部分种族特性待接入":spell.name=="Polymorph"?"战斗结算已接入 · 10种地面形态":"战斗结算已接入"):IsLearningOnly(spell.name)?((spell.name=="Alarm"?alarmLearned:learnedUtilitySpells.Contains(spell.name))?"已学习 · 战斗禁用":"可学习 · 战斗禁用"):"待接入";
            string text=(chinese==""?spell.name:chinese+" · "+spell.name)+"\n"+(spell.level==0?"戏法":spell.level+"环")+" · "+SchoolChinese(spell.school)+" · "+status;
            string tooltip=spell.name=="Hellish Rebuke"?"炼狱叱喝｜Hellish Rebuke\n一环塑能 · 反应 · 声音、姿势\n受到60尺内可见生物的伤害后可消耗反应。伤害来源敏捷豁免，失败2d10火焰，成功减半；每升环+1d10。守护之链转移不触发。":spell.name=="Counterspell"?CounterspellRules.Summary:reactionSpell?"护盾术｜Shield\n一环防护 · 反应 · 声音、姿势\n被攻击命中或成为魔法飞弹目标时选择施放。AC+5，免疫魔法飞弹，至下一玩家回合开始。选择前隐藏命中骰值与总值。":direct!=null?direct.Summary:spell.name=="Acid Splash"?AcidSplash.Summary:spell.name=="Alarm"?AlarmSpell.Summary:spell.name+"\n施法时间："+spell.castingTime+"\n距离："+spell.range+"\n成分："+string.Join("、",spell.components)+"\n持续："+spell.duration+"\n"+(IsLearningOnly(spell.name)?"仅开放学习，不能施放或带入预先生成的效果。":"效果待接入。");
            bool previous=GUI.enabled;GUI.enabled=eligible&&(available||(IsLearningOnly(spell.name)&&!combat.BattleStarted));
            if(GUILayout.Button(new GUIContent(text,tooltip),new GUIStyle(button){fixedHeight=76,fontSize=17})){
                if(IsLearningOnly(spell.name)){learnedUtilitySpells.Add(spell.name);if(spell.name=="Alarm")alarmLearned=true;}
                else {selectedSpellName=spell.name;directSlotLevel=Math.Max(1,spell.level);directTargetPlayer=direct!=null&&(direct.Name=="Protection from Poison"||direct.Name=="Jump"||direct.Name=="Lesser Restoration"||direct.Name=="Arcane Vigor"||direct.Name=="Detect Evil and Good"||direct.Name=="Mage Armor"||direct.Name=="Shillelagh"||direct.Healing||direct.Stable||IsSenseSpell(direct.Name)||direct.Name=="Mirror Image"||direct.Name=="Death Ward"||direct.Name=="Darkvision"||direct.Name=="Guidance"||direct.Name=="Enhance Ability"||direct.Name=="False Life"||IsTimedBuff(direct.Name)||direct.Name=="Protection from Energy"||direct.Name=="Blade Ward"||direct.Name=="Shield of Faith"||direct.Name=="Aid"||direct.Name=="Armor of Agathys"||direct.Name=="Bless");directBothTargets=false;trueSeeingWilling=false;energyTargetWilling=false;visionWilling=false;}
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
        if(RevivalRules.Supported(selectedSpellName)){selectedSpellName="";panel="aftermath";DrawAftermath();return;}
        if(selectedSpellName=="Dancing Lights"){DrawDancingLightsPanel(false);return;}
        if(selectedSpellName=="Gust of Wind"){DrawGustWindPanel();return;}
        if(selectedSpellName=="Conjure Barrage"){DrawBarragePanel();return;}
        if(selectedSpellName=="Lightning Bolt"){DrawLightningBoltPanel();return;}
        if(selectedSpellName=="Cone of Cold"||selectedSpellName=="Burning Hands"||selectedSpellName=="Color Spray"){DrawBurningHandsPanel();return;}
        if(selectedSpellName=="Light"){DrawLightSpellPanel();return;}
        var definition=DirectSpell.Find(selectedSpellName);var metadata=SelectedSpellMetadata();
        if(definition==null||metadata==null){selectedSpellName="";return;}
        Surface(new Rect(350,110,480,745),paperTexture);
        if(Btn(new Rect(720,125,90,46),"返回")){selectedSpellName="";return;}
        GUILayout.BeginArea(new Rect(375,185,430,635));
        directSpellScroll=GUILayout.BeginScrollView(directSpellScroll);
        GUILayout.Label(definition.Chinese,heading);GUILayout.Label(definition.Summary,label);
        if(map.PlayerAquatic.Submerged&&!WaterBreathingAllowed(true)&&Array.IndexOf(metadata.components,"声音")>=0)GUILayout.Label("水下声音施法会立即结束屏息并呛水，本回合末开始累积窒息力竭。",small);
        var casters=System.Linq.Enumerable.ToArray(EligibleCasters(metadata));
        if(Array.IndexOf(casters,directCastingClass)<0&&casters.Length>0)directCastingClass=casters[0];
        foreach(int caster in casters)if(GUILayout.Button((caster==directCastingClass?"✓ ":"")+CasterLabel(caster)+" · "+PlayerAttributes.Names[SpellAbilityIndex(caster)],button))directCastingClass=caster;
        int modifier=AcidSplash.Modifier(attributes[SpellAbilityIndex(directCastingClass)]);
        GUILayout.Label("施法调整值 "+modifier+" · 法术命中 +"+(classes.ProficiencyBonus+modifier+PlayerAquaticD20Penalty)+" · 豁免DC "+(8+classes.ProficiencyBonus+modifier),small);
        DrawSpeciesSpellResource(definition);
        if(definition.Level>0){bool slotEnabled=GUI.enabled;GUI.enabled=slotEnabled&&!(directCastingClass==SpeciesSpellRules.Caster&&useSpeciesFreeCast);GUILayout.Label("施法环阶（法术位资源暂不扣除）",small);directSlotLevel=GUILayout.SelectionGrid(directSlotLevel-definition.Level,System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(System.Linq.Enumerable.Range(definition.Level,10-definition.Level),x=>x.ToString())),Math.Min(9,10-definition.Level),new GUIStyle(button){fontSize=16,fixedHeight=34})+definition.Level;GUI.enabled=slotEnabled;}
        if(definition.Name=="Fire Storm")DrawFireStormChoices();
        else if(definition.Name=="Animate Dead")DrawAnimateDeadChoices();
        else if(definition.Name=="Flame Strike")DrawFlameStrikeChoices();
        else if(definition.Name=="Conjure Volley")DrawVolleyChoices();
        else if(definition.Name=="Prayer of Healing")DrawPrayerChoices();
        else if(definition.Name==LaughterEffect.Name)DrawLaughterChoices();
        else if(definition.Name=="Moonbeam")DrawMoonbeamChoices();
        else if(definition.Name=="Resistance")DrawResistanceChoices();
        else if(definition.Name=="Stinking Cloud")DrawStinkingCloudChoices();
        else if(definition.Name=="Spirit Guardians")DrawGuardianChoices();
        else if(definition.Name=="Sleet Storm")DrawSleetStormChoices();
        else if(definition.Name=="Call Lightning")DrawCallLightningChoices();
        else if(definition.Name=="Hail of Thorns")GUILayout.Label("仅在自己的回合远程武器命中后立即触发，选择施法环阶及武器／法术伤害先后；范围包含玩家与盟友。不可从法术书直接施放。",small);
        else if(definition.Name=="Conjure Animals")DrawConjureAnimalsChoices();
        else if(definition.Name=="Elemental Weapon")DrawElementalWeaponChoices();
        else if(definition.Name=="Crusader's Mantle")GUILayout.Label("30尺光环随玩家移动，自己与范围内受控盟友的武器或徒手命中额外1d4光耀；暴击2d4。兽形自然攻击可受益，法术攻击不受益。专注至多1分钟；升环不增强。吞咽隔离内外，其他位面不受益。",small);
        else if(definition.Name=="Fire Shield")DrawFireShieldChoices();
        else if(definition.Name=="Flame Blade")DrawFlameBladeChoices();
        else if(definition.Name=="Chromatic Orb")DrawChromaticOrbChoices();
        else if(definition.Name=="Spiritual Weapon")DrawSpiritualWeaponChoices();
        else if(definition.Name=="Blinding Smite")GUILayout.Label("自身回合近战命中后选择致盲斩；三环3d8光耀，每升环＋1d8。目标直接目盲，回合末体质豁免成功结束；无需专注。不能直接从法术书施放。",small);
        else if(definition.Name=="Shining Smite")GUILayout.Label("自身回合近战命中后选择闪耀斩；二环2d6光耀，每升环＋1d6。专注至多1分钟，目标5尺明亮光照、被攻击有优势、不能受益于隐形。不能直接从法术书施放。",small);
        else if(definition.Name=="Wrathful Smite")GUILayout.Label("自身回合近战命中后选择激愤斩；每环1d6暗蚀，感知豁免失败恐慌，每回合末重复豁免成功结束。不能直接从法术书施放。",small);
        else if(definition.Name=="Thunderous Smite")GUILayout.Label("在自身回合近战命中后选择雷鸣斩；额外2d6雷鸣，每升环＋1d6。力量豁免失败推离10尺并倒地，成功不减伤。不能直接从法术书施放。",small);
        else if(definition.Name=="Divine Smite")GUILayout.Label("在自身回合近战命中后，从斩击弹窗选择神圣斩；一环2d8光耀，每升环＋1d8，邪魔／亡灵另＋1d8。不能直接从法术书施放。",small);
        else if(definition.Name=="Searing Smite")DrawSearingSmiteInfo();
        else if(definition.Name=="Witch Bolt")DrawWitchBoltChoices();
        else if(definition.Name=="Regenerate")DrawRegenerateChoices();
        else if(definition.Name=="Banishment")DrawBanishmentChoices();
        else if(definition.Name=="Vampiric Touch")DrawVampiricTouchChoices();
        else if(definition.Name=="Vitriolic Sphere")DrawVitriolicSphereChoices();
        else if(definition.Name=="Ice Storm")DrawIceStormChoices();
        else if(definition.Name=="Produce Flame")DrawProduceFlameChoices();
        else if(definition.Name=="Continual Flame")DrawContinualFlameChoices();
        else if(definition.Name=="Pass without Trace")DrawPassTraceChoices();
        else if(definition.Name=="Beacon of Hope")DrawBeaconHopeChoices();
        else if(definition.Name=="Aura of Vitality")DrawAuraVitalityChoices();
        else if(definition.Name=="Freedom of Movement")DrawFreedomMovementChoices();
        else if(definition.Name=="Water Walk")DrawWaterWalkChoices();
        else if(definition.Name=="Warding Bond")DrawWardingBondChoices();
        else if(definition.Name=="Flaming Sphere")DrawFlamingSphereChoices();
        else if(definition.Name=="Heat Metal")DrawHeatMetalChoices();
        else if(definition.Name=="Heroism")DrawHeroismChoices();
        else if(definition.Name=="Melf’s Acid Arrow")DrawAcidArrowChoices();
        else if(definition.Name=="Dragon’s Breath")DrawDragonBreathChoices();
        else if(definition.Name=="Dissonant Whispers")DrawDissonantWhispersChoices();
        else if(definition.Name=="Command")DrawCommandChoices();
        else if(definition.Name=="Starry Wisp")DrawStarryChoices();
        else if(definition.Name=="Feign Death")DrawFeignDeathChoices();
        else if(definition.Name=="Faerie Fire")DrawFaerieFireChoices();
        else if(definition.Name=="Silence")DrawSilenceChoices();
        else if(IsPointBurst(definition))DrawPointBurstChoices(definition);
        else if(definition.Name=="Power Word Fortify")DrawFortifyAllocation();
        else if(IsSimpleEmanation(definition.Name))DrawEmanationTargets(definition);
        else if(definition.Name=="Misty Step"||definition.Name=="Expeditious Retreat")DrawMobilityChoices(definition);
        else if(definition.Name=="Darkness"||definition.Name=="Daylight"||definition.Name=="Fog Cloud")DrawVisionSpellChoices(definition);
        else if(definition.Name=="True Strike"){GUILayout.Label("武器："+selectedWeapon.Name+" · "+selectedWeapon.DamageDice+" · "+selectedWeapon.RangeText,small);trueStrikeRadiant=GUILayout.Toggle(trueStrikeRadiant,"将武器基础伤害改为光耀（否则为"+selectedWeapon.DamageType+"）",label);}
        else if(definition.Name=="Thunderwave")GUILayout.Label("确认后选择15尺立方方向并预览范围。",small);
        else if(definition.Name=="Eldritch Blast")DrawMissileTargets(definition.DiceCount(classes.TotalLevel,1),"魔能束","每束分别选择目标，独立攻击并造成1d10力场伤害。");
        else if(definition.Name=="Scorching Ray")DrawMissileTargets(definition.DiceCount(classes.TotalLevel,directSlotLevel),"射线","每道射线单独攻击与伤害；先分配所有目标。");
        else if(definition.Name=="Magic Missile")DrawMissileTargets(definition.DiceCount(classes.TotalLevel,directSlotLevel));
        else if(definition.Name=="Destructive Wave")DrawDestructiveWaveChoices();
        else if(definition.Name=="Chain Lightning")DrawChainLightningChoices();
        else if(definition.Name=="Mass Heal")DrawMassHealChoices();
        else if(definition.Name=="Heal")DrawHealChoices();
        else if(definition.Name=="Find Steed")DrawFindSteedChoices();
        else if(definition.Name=="Stoneskin")DrawStoneskinChoices();
        else if(definition.Name=="Cloud of Daggers")DrawCloudDaggersChoices();
        else if(definition.Name=="Web")DrawWebChoices();
        else if(definition.Name=="Water Breathing")DrawWaterBreathingChoices();
        else if(definition.Name=="Sleep")DrawSleepChoices();
        else if(definition.Name=="Spike Growth")GUILayout.Label("指定地面区域中心，所有生物均受影响。",small);
        else if(definition.Name=="Grease")GUILayout.Label("指定10尺方形地面区域，初始影响其中所有生物。",small);
        else if(definition.Name=="Entangle")GUILayout.Label("指定地面方形区域，初始影响除施法者外的范围内生物。",small);
        else {
        if(GUILayout.Button((directTargetPlayer?"✓ ":"")+"目标：玩家（自身）",button))directTargetPlayer=true;
        if(GUILayout.Button((!directTargetPlayer?"✓ ":"")+"目标：木桩 · "+monsterProfile.Label,button))directTargetPlayer=false;
        }
        if(definition.Name=="Bane"||definition.Name=="Bless"||definition.Name=="Aid"||definition.Name=="Mass Healing Word"||(definition.Name=="Jump"&&directSlotLevel>=2)||(definition.Name=="Blindness/Deafness"&&directSlotLevel>=3)||((definition.Control&&definition.Name!="Sleep"&&directSlotLevel>definition.Level)||(definition.Name=="Enhance Ability"&&directSlotLevel>=3))||(definition.Name=="Longstrider"&&directSlotLevel>=2)||(definition.Name=="Invisibility"&&directSlotLevel>=3))directBothTargets=GUILayout.Toggle(directBothTargets,definition.Name=="Bane"||definition.Name=="Aid"||definition.Name=="Bless"?"同时选择双方（至多3名目标）":definition.Name=="Mass Healing Word"?"同时选择双方（至多6名目标）":"同时选中玩家与木桩（升环额外目标）",label);
        else directBothTargets=false;
        if(definition.Name=="Darkvision")DrawVisionSpellChoices(definition);
        if(definition.Name=="True Seeing"&&!directTargetPlayer)trueSeeingWilling=GUILayout.Toggle(trueSeeingWilling,"确认木桩自愿接受真知术",label);
        if(definition.Name=="Spike Growth")DrawSpikeGrowthChoices();
        if(definition.Name=="Grease")DrawGreaseChoices();
        if(definition.Name=="Entangle")DrawEntangleChoices();
        if(definition.Name=="Enlarge/Reduce")DrawSizeChangeChoices();
        if(definition.Name=="Polymorph")DrawPolymorphChoices();
        if(definition.Name=="Sorcerous Burst")DrawSorcerousBurstChoices();
        if(definition.Name=="Shillelagh")DrawShillelaghOptions();
        if(definition.Name=="Thorn Whip")DrawThornWhipChoices();
        if(definition.Name=="Arcane Vigor")DrawArcaneVigorChoices();
        if(definition.Name=="Blindness/Deafness")DrawBlindnessDeafnessChoices();
        if(definition.Name=="Lesser Restoration")DrawLesserRestorationChoices();
        if(definition.Name=="Jump")DrawJumpChoices();
        if(definition.Name=="Mage Armor"&&!directTargetPlayer)energyTargetWilling=GUILayout.Toggle(energyTargetWilling,"确认木桩自愿接受法师护甲",label);
        DrawCheckBuffChoices(definition);
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
        if(!eligible)return "当前职业与种族不可施放。";
        if(spell.Name=="Hail of Thorns")return "仅在自己的回合远程武器命中后立即触发。";
        if(spell.Name=="Searing Smite"||spell.Name=="Divine Smite"||spell.Name=="Thunderous Smite"||spell.Name=="Wrathful Smite"||spell.Name=="Shining Smite"||spell.Name=="Blinding Smite")return "仅在自身回合的近战命中后立即触发，不能直接施放。";
        if(banishment.Concentrating&&map.SceneOf(0)>0&&(SelectedSpellMetadata()?.concentration==true||spell.Name=="Prayer of Healing")&&spell.Name!="Banishment")return "请先结束放逐术专注，再选择返回场景中的新法术目标。";
        string speciesError=SpeciesSpellCastError(spell.Name,directCastingClass,directSlotLevel);if(speciesError!="")return speciesError;
        if(page!="battle"||!map.Locked)return "部署完成后可施法。";
        if(rolling||spellTargeting||draggingToken>=0)return "请等待当前操作结束。";
        if(RevivalRules.Supported(spell.Name))return RevivalError(spell.Name,directTargetPlayer,false);
        bool aftermathLight=combat.Aftermath&&AftermathLightSpell(spell.Name);
        if(combat.Aftermath&&!PlayerTurnAllowed)return "请等待救援者结束回合。";
        if(aftermathLight?combat.PlayerHp<=0||combat.PlayerDead||combat.PlayerParalyzed||combat.PlayerFeignDeath:!combat.PlayerCanAct)return "玩家当前不能行动。";
        if(!playerResurrectionCaster.CanCast)return "复生术疲惫：完成长休前不能施法。";
        if(!CanCastInCurrentForm)return PlayerPolymorphed?"变形术形态不能施法。":"荒野形态不能施法（德鲁伊18级解锁兽形施法）。";
        if(!SpellVoiceAllowed(true,spell.Name))return "单位中心位于沉默范围，不能进行声音施法。";
        if(MockeryTargetError(spell,directTargetPlayer)!="")return MockeryTargetError(spell,directTargetPlayer);
        if(!ArmorSpellAllowed(spell.Name))return PlayerArmorUntrained?"未受训护甲使玩家不能施法。":"双手被武器／盾牌／焰刃占用，请先空出一只手执行姿势成分。";
        if(spell.Name=="True Strike"&&(weaponGrip.Stowed||WeaponHandError(selectedWeapon)!=""))return "克敌机先需要可使用的已持握武器。";
        if(spell.Bonus?!combat.BonusActionAvailable:!(aftermathLight?combat.AftermathMagicActionAvailable:combat.MagicActionAvailable))return "没有可用的"+(spell.Bonus?"附赠动作":"施法动作")+"。";
        if(spell.Name=="Shillelagh")return !directTargetPlayer?"橡棍术只作用于自身持有的武器。":ShillelaghError();
        if(spell.Name=="Mage Armor"&&MageArmorError(directTargetPlayer)!="")return MageArmorError(directTargetPlayer);
        if(spell.Name=="Detect Evil and Good"&&DetectEvilGoodError(directTargetPlayer)!="")return DetectEvilGoodError(directTargetPlayer);
        if(spell.Name=="Arcane Vigor")return ArcaneVigorError(directTargetPlayer,directSlotLevel);
        if(spell.Name=="Faerie Fire")return FaerieFireError();
        if(spell.Name=="Dancing Lights")return DancingLightsError();
        if(spell.Name=="Cloud of Daggers")return CloudDaggersError();
        if(spell.Name=="Web")return WebError();
        if(spell.Name=="Conjure Barrage"||spell.Name=="Conjure Volley")return ConjuredWeaponsError(spell.Name=="Conjure Volley");
        if(spell.Name=="Cone of Cold")return ConeOfColdError();
        if(spell.Name=="Finger of Death")return FingerOfDeathError(directTargetPlayer);
        if(spell.Name=="Fire Storm")return FireStormError();
        if(spell.Name=="Animate Dead")return AnimateDeadError();
        if(spell.Name=="Flame Strike")return FlameStrikeError();
        if(spell.Name=="Prayer of Healing")return PrayerError(directSlotLevel);
        if(spell.Name==LaughterEffect.Name)return LaughterError(directSlotLevel);
        if(spell.Name=="Moonbeam")return MoonbeamError();
        if(spell.Name=="Resistance")return ResistanceError();
        if(spell.Name=="Gust of Wind")return GustWindError();
        if(spell.Name=="Stinking Cloud")return StinkingCloudError();
        if(spell.Name=="Spirit Guardians")return GuardianError();
        if(spell.Name=="Sleet Storm")return SleetStormError();
        if(spell.Name=="Call Lightning")return CallLightningError();
        if(spell.Name=="Conjure Animals")return ConjureAnimalsError();
        if(spell.Name=="Elemental Weapon")return ElementalWeaponError(elementalObject);
        if(spell.Name=="Crusader's Mantle")return CrusaderMantleError();
        if(spell.Name=="Fire Shield")return FireShieldError();
        if(spell.Name=="Flame Blade")return FlameBladeError();
        if(spell.Name=="Chromatic Orb")return ChromaticOrbError();
        if(spell.Name=="Spiritual Weapon")return SpiritualWeaponError();
        if(spell.Name=="Witch Bolt")return WitchBoltError();
        if(spell.Name=="Regenerate")return RegenerateError();
        if(spell.Name=="Banishment")return BanishmentError(directSlotLevel);
        if(spell.Name=="Vampiric Touch")return VampiricTouchError();
        if(spell.Name=="Vitriolic Sphere")return VitriolicSphereError();
        if(spell.Name=="Ice Storm")return IceStormError();
        if(spell.Name=="Produce Flame")return ProduceFlameError();
        if(spell.Name=="Continual Flame")return ContinualFlameError(continualObject);
        if(spell.Name=="Pass without Trace")return PassTraceError();
        if(spell.Name=="Beacon of Hope")return BeaconHopeError();
        if(spell.Name=="Aura of Vitality")return AuraVitalityError();
        if(spell.Name=="Freedom of Movement")return FreedomMovementError(directSlotLevel);
        if(spell.Name=="Water Walk")return WaterWalkError();
        if(spell.Name=="Warding Bond")return WardingBondError();
        if(spell.Name=="Flaming Sphere")return FlamingSphereError();
        if(spell.Name=="Heat Metal")return HeatObjectError(heatObject,true);
        if(spell.Name=="Heroism")return HeroismError(directSlotLevel);
        if(spell.Name=="Melf’s Acid Arrow")return AcidArrowError();
        if(spell.Name=="Dragon’s Breath")return DragonBreathError();
        if(spell.Name=="Dissonant Whispers")return DissonantWhispersError();
        if(spell.Name=="Command")return CommandError(directSlotLevel);
        if(spell.Name=="Color Spray")return ColorSprayError();
        if(spell.Name=="Burning Hands")return BurningHandsError();
        if(spell.Name=="Lightning Bolt")return LightningBoltError();
        if(spell.Name=="Light")return LightSpellError();
        if(spell.Name=="Starry Wisp")return StarryWispError();
        if(spell.Name=="Feign Death")return FeignDeathError();
        if(spell.Name=="Silence")return SilenceError();
        if(spell.Name=="Destructive Wave")return DestructiveWaveError();
        if(spell.Name=="Chain Lightning")return ChainLightningError(directSlotLevel);
        if(spell.Name=="Mass Heal")return MassHealError();
        if(spell.Name=="Heal")return HealSpellError();
        if(spell.Name=="Find Steed")return FindSteedError(directSlotLevel);
        if(spell.Name=="Stoneskin")return StoneskinError();
        if(spell.Name=="Sleep")return SleepError();
        if(spell.Name=="Water Breathing")return WaterBreathingError(directTargetPlayer,directBothTargets);
        if(spell.Name=="Spike Growth")return SpikeGrowthError();
        if(spell.Name=="Grease")return GreaseError();
        if(spell.Name=="Entangle")return EntangleError();
        if(spell.Name=="Enlarge/Reduce")return SizeChangeError(directTargetPlayer);
        if(spell.Name=="Ray of Enfeeblement")return EnfeeblementError(directTargetPlayer);
        if(spell.Name=="Protection from Poison")return PoisonProtectionError(directTargetPlayer);
        if(spell.Name=="Jump")return JumpSpellError(directTargetPlayer,directSlotLevel,directBothTargets);
        if(spell.Name=="Blindness/Deafness")return BlindnessDeafnessError(directTargetPlayer,directSlotLevel,directBothTargets);
        if(spell.Name=="Lesser Restoration")return LesserRestorationError(directTargetPlayer,lesserRestorationCondition);
        if((spell.Name=="Blur"||spell.Name=="See Invisibility")&&!directTargetPlayer)return "此法术只能作用于自身。";
        if(spell.Name=="True Seeing"&&!directTargetPlayer&&!trueSeeingWilling)return "真知术需要自愿目标。";
        if(spell.Name=="Polymorph"){string error=PolymorphError();if(error!="")return error;}
        if(IsPointBurst(spell))return PointBurstError(spell);
        if(spell.Name=="Mirror Image"&&!directTargetPlayer)return "镜像术只能作用于自身。";
        if(spell.Name=="Armor of Agathys"&&!directTargetPlayer)return "黯冰狱铠只能作用于自身。";
        if(SpellRequiresSight(spell.Name)&&(!directTargetPlayer||directBothTargets)&&!PlayerSeesMonster&&spell.Name!="Magic Missile"&&spell.Name!="Power Word Fortify")return "此法术需要看见所选目标。";
        if(spell.Name=="Misty Step")return MistyStepError();
        if(spell.Name=="Expeditious Retreat")return "";
        if(IsVisionSpell(spell.Name))return VisionSpellError(spell);
        if(spell.Name=="Power Word Fortify")return FortifyInvalidReason();
        if(spell.Name=="Blade Ward"&&!directTargetPlayer)return "剑刃防护只能作用于自身。";
        if(spell.Name=="Guidance"&&!directTargetPlayer&&!energyTargetWilling)return "指引术需要自愿目标，请先确认。";
        if(spell.Name=="Barkskin"&&!directTargetPlayer&&!energyTargetWilling)return "树肤术需要自愿目标，请先确认。";
        if(spell.Name=="Protection from Energy"&&!directTargetPlayer&&!energyTargetWilling)return "此法术需要自愿目标，请确认木桩是否接受。";
        if((spell.Name=="Divine Favor"||spell.Name=="Magic Weapon")&&!directTargetPlayer)return "此法术当前仅可用于自身或玩家装备的武器。";
        if(spell.Name=="Magic Weapon"){if(PlayerInBeastForm)return "武器已融入兽形，当前不能触碰。";string e=ElementalWeaponError(PlayerWeaponObject(selectedWeapon));if(e!="")return e;}
        if(spell.Name=="True Strike"){if(IsWildShaped)return "武器已融入兽形，当前无法使用。";if(!selectedWeapon.CanReach(map.DistanceFeet))return "目标超出当前武器射程。";return "";}
        if(spell.Name=="Thunderwave"||IsSimpleEmanation(spell.Name))return "";
        if(spell.Name=="Magic Missile"||spell.Name=="Scorching Ray"||spell.Name=="Eldritch Blast")return MissileTargetError(spell.DiceCount(classes.TotalLevel,directSlotLevel));
        if(spell.Name=="False Life"&&!directTargetPlayer)return "虚假生命只能以自身为目标。";
        int distance=directTargetPlayer?0:map.DistanceFeet;
        if(directBothTargets)distance=map.DistanceFeet;
        if(spell.Control&&(!directTargetPlayer||directBothTargets)&&!spell.AllowsCreatureType(monsterProfile.TypeIndex))return "目标类型不合法：木桩不是类人生物。";
        if(distance>spell.EffectiveRange(classes.TotalLevel))return "超出射程：目标 "+distance+" 尺。";
        if(directTargetPlayer?combat.PlayerDead:combat.Hp==0)return "死亡目标不能接受此法术。";
        if(spell.Stable&&(!directTargetPlayer||combat.PlayerHp!=0))return "维生术需要生命为0且未死亡的目标。";
        return "";
    }
    int spellCastSequence;
    IEnumerator ResolveDirectSpell(DirectSpell spell,bool targetPlayer,int caster,int slotLevel,bool embedded=false){
        long token=senses.Token(true,worldRounds);int sequence=spellCastSequence;
        yield return ResolveDirectSpellCore(spell,targetPlayer,caster,slotLevel,embedded);
        if(PendingTerrainCount>0){bool restoreBusy=embedded||rolling;rolling=true;yield return DrainGreaseEntries();rolling=restoreBusy;}
        if(!embedded&&sequence!=spellCastSequence)BreakOrdinaryInvisibility(true,"施法",token);
    }
    IEnumerator ResolveDirectSpellCore(DirectSpell spell,bool targetPlayer,int caster,int slotLevel,bool embedded)
    {
        if(spell.Name=="Hail of Thorns"||spell.Name=="Searing Smite"||spell.Name=="Divine Smite"||spell.Name=="Thunderous Smite"||spell.Name=="Wrathful Smite"||spell.Name=="Shining Smite"||spell.Name=="Blinding Smite")yield break;
        if(RevivalRules.Supported(spell.Name)){yield return CastRevival(spell.Name,targetPlayer,false);yield break;}
        if(SpeciesSpellCastError(spell.Name,caster,slotLevel)!="")yield break;
        Vector2Int entangleAnchor;VisionPoint(out entangleAnchor);
        int sizeDelta=sizeEnlarge?1:-1;bool sizeConsent=sizeWilling;var chosenSizeAnchor=sizeAnchor;
        bool blindnessBoth=directBothTargets,blindnessPlayer=blindnessPlayerMode==0,blindnessMonster=blindnessMonsterMode==0;
        int restorationCondition=lesserRestorationCondition;bool jumpBoth=directBothTargets;
        if(!SpellVoiceAllowed(true,spell.Name))yield break;
        if(MockeryTargetError(spell,targetPlayer)!="")yield break;
        if(!ArmorSpellAllowed(spell.Name))yield break;
        if(spell.Name=="True Strike"&&(weaponGrip.Stowed||WeaponHandError(selectedWeapon)!=""))yield break;
        if(spell.Name=="Mage Armor"&&MageArmorError(targetPlayer)!="")yield break;
        if(spell.Name=="Detect Evil and Good"&&DetectEvilGoodError(targetPlayer)!="")yield break;
        if(spell.Name=="Arcane Vigor"&&ArcaneVigorError(targetPlayer,slotLevel)!="")yield break;
        if(spell.Name=="Faerie Fire"&&FaerieFireError()!="")yield break;
        if(spell.Name=="Dancing Lights"&&DancingLightsError()!="")yield break;
        bool conjuredVolley=spell.Name=="Conjure Volley";var conjuredSelected=new HashSet<int>(conjuredVolley?volleySelected:barrageSelected);
        BreathTemplate coldAim=default(BreathTemplate);if(spell.Name=="Cone of Cold"&&!PlayerBurningTemplate(out coldAim,60))yield break;
        BreathTemplate barrageAim=default(BreathTemplate);if(spell.Name=="Conjure Barrage"&&!PlayerBarrageTemplate(out barrageAim))yield break;
        if((spell.Name=="Conjure Barrage"||conjuredVolley)&&ConjuredWeaponsError(conjuredVolley)!="")yield break;
        BreathTemplate lightningAim=default(BreathTemplate);if(spell.Name=="Lightning Bolt"&&!PlayerLightningTemplate(out lightningAim))yield break;
        BreathTemplate burningAim=default(BreathTemplate);if((spell.Name=="Burning Hands"||spell.Name=="Color Spray")&&!PlayerBurningTemplate(out burningAim))yield break;
        if(spell.Name=="Color Spray"&&ColorSprayError()!="")yield break;
        if(spell.Name=="Web"&&WebError()!="")yield break;
        if(spell.Name=="Cloud of Daggers"&&CloudDaggersError()!="")yield break;
        if(spell.Name=="Finger of Death"&&FingerOfDeathError(targetPlayer)!="")yield break;
        var stormSelection=new List<FireStormRules.Block>(fireStormBlocks);if(spell.Name=="Fire Storm"&&FireStormError()!="")yield break;
        if(spell.Name=="Animate Dead"&&AnimateDeadError()!="")yield break;
        if(spell.Name=="Flame Strike"&&FlameStrikeError()!="")yield break;
        var prayerSelected=new List<int>(prayerTargets);if(spell.Name=="Prayer of Healing"&&PrayerError(slotLevel)!="")yield break;
        var laughterSelected=new List<int>(laughterTargets);if(spell.Name==LaughterEffect.Name&&LaughterError(slotLevel)!="")yield break;
        if(spell.Name=="Moonbeam"&&MoonbeamError()!="")yield break;
        int resistanceSelected=resistanceTarget,resistanceSelectedType=ResistanceTypes[resistanceType];if(spell.Name=="Resistance"&&ResistanceError()!="")yield break;
        BreathTemplate gustAim;PlayerGustTemplate(out gustAim);int gustEdge=gustChoice.Edge,gustOffset=gustChoice.Offset;if(spell.Name=="Gust of Wind"&&GustWindError()!="")yield break;
        if(spell.Name=="Stinking Cloud"&&StinkingCloudError()!="")yield break;
        int guardAppearance=guardianAppearance;var guardExcluded=new HashSet<int>(guardianExclusions);if(spell.Name=="Spirit Guardians"&&GuardianError()!="")yield break;
        if(spell.Name=="Sleet Storm"&&SleetStormError()!="")yield break;
        Vector2Int lightningCloud;CloudPoint(out lightningCloud);if(spell.Name=="Call Lightning"&&CallLightningError()!="")yield break;
        int packChosenAppearance=packAppearance;if(spell.Name=="Conjure Animals"&&ConjureAnimalsError()!="")yield break;
        int elementalSelected=elementalObject,elementalType=EnergyTypes[elementalEnergy];
        if(spell.Name=="Elemental Weapon"&&ElementalWeaponError(elementalSelected)!="")yield break;
        if(spell.Name=="Magic Weapon"&&(PlayerInBeastForm||ElementalWeaponError(PlayerWeaponObject(selectedWeapon))!=""))yield break;
        if(spell.Name=="Crusader's Mantle"&&CrusaderMantleError()!="")yield break;
        bool fireShieldSelected=fireShieldCold;if(spell.Name=="Fire Shield"&&FireShieldError()!="")yield break;
        if(spell.Name=="Flame Blade"&&FlameBladeError()!="")yield break;
        int orbSelected=orbTarget,orbSelectedType=ChromaticOrbRules.Types[orbType];if(spell.Name=="Chromatic Orb"&&ChromaticOrbError()!="")yield break;
        Vector2Int spiritualPoint;SpiritualInput(out spiritualPoint);int spiritualSelected=spiritualTarget,spiritualSelectedHost=SpiritualCreationHost(spiritualPoint);bool spiritualSelectedAttack=spiritualAttack;if(spell.Name=="Spiritual Weapon"&&SpiritualWeaponError()!="")yield break;
        int witchSelected=witchTarget;if(spell.Name=="Witch Bolt"&&WitchBoltError()!="")yield break;
        int regenerateSelected=regenerateTarget;if(spell.Name=="Regenerate"&&RegenerateError()!="")yield break;
        var banishSelected=new List<int>(banishTargets);if(spell.Name=="Banishment"&&BanishmentError(slotLevel)!="")yield break;
        int vampiricSelected=vampiricTarget;if(spell.Name=="Vampiric Touch"&&VampiricTouchError()!="")yield break;
        if(spell.Name=="Vitriolic Sphere"&&VitriolicSphereError()!="")yield break;
        if(spell.Name=="Ice Storm"&&IceStormError()!="")yield break;
        if(spell.Name=="Produce Flame"&&ProduceFlameError()!="")yield break;
        int continualSelected=continualObject;if(spell.Name=="Continual Flame"&&ContinualFlameError(continualSelected)!="")yield break;
        var traceSelected=new List<int>(passTraceTargets);if(spell.Name=="Pass without Trace"&&PassTraceError()!="")yield break;
        var beaconSelected=new List<int>(beaconTargets);if(spell.Name=="Beacon of Hope"&&BeaconHopeError()!="")yield break;
        if(spell.Name=="Aura of Vitality"&&AuraVitalityError()!="")yield break;
        var freedomSelected=new List<int>(freedomTargets);if(spell.Name=="Freedom of Movement"&&FreedomMovementError(slotLevel)!="")yield break;
        var walkSelected=new List<int>(waterWalkTargets);if(spell.Name=="Water Walk"&&WaterWalkError()!="")yield break;
        int bondedSelection=bondTarget;if(spell.Name=="Warding Bond"&&WardingBondError()!="")yield break;
        if(spell.Name=="Flaming Sphere"&&FlamingSphereError()!="")yield break;
        int heatedSelection=heatObject;if(spell.Name=="Heat Metal"&&HeatObjectError(heatedSelection,true)!="")yield break;
        var heroismSelected=new System.Collections.Generic.List<int>(heroismTargets);if(spell.Name=="Heroism"&&HeroismError(slotLevel)!="")yield break;
        int acidSelected=acidArrowTarget;if(spell.Name=="Melf’s Acid Arrow"&&AcidArrowError()!="")yield break;
        int dragonSelected=dragonBreathTarget,dragonTypeSelected=DragonBreathTypes[dragonBreathType];if(spell.Name=="Dragon’s Breath"&&DragonBreathError()!="")yield break;
        int whispersSelected=whispersTarget;if(spell.Name=="Dissonant Whispers"&&DissonantWhispersError()!="")yield break;
        var commandSelected=new System.Collections.Generic.List<int>(commandTargets);var chosenCommand=(CommandWord)commandChoice;if(spell.Name=="Command"&&CommandError(slotLevel)!="")yield break;
        int starrySelected=starryTarget;if(spell.Name=="Starry Wisp"&&StarryWispError()!="")yield break;
        int feignSelected=feignTarget;if(spell.Name=="Feign Death"&&FeignDeathError()!="")yield break;
        int lightTarget=lightObjectId,lightR=0,lightG=0,lightB=0;if(spell.Name=="Light"&&(LightSpellError()!=""||!LightColors(out lightR,out lightG,out lightB)))yield break;
        if(spell.Name=="Silence"&&SilenceError()!="")yield break;
        if(spell.Name=="Water Breathing"&&WaterBreathingError(targetPlayer,directBothTargets)!="")yield break;
        if(spell.Name=="Destructive Wave"&&DestructiveWaveError()!="")yield break;
        if(spell.Name=="Chain Lightning"&&ChainLightningError(slotLevel)!="")yield break;
        if(spell.Name=="Mass Heal"&&MassHealError()!="")yield break;
        if(spell.Name=="Heal"&&HealSpellError()!="")yield break;
        if(spell.Name=="Find Steed"&&FindSteedError(slotLevel)!="")yield break;
        if(spell.Name=="Stoneskin"&&StoneskinError()!="")yield break;
        if(spell.Name=="Sleep"&&SleepError()!="")yield break;
        if(spell.Name=="Spike Growth"&&SpikeGrowthError()!="")yield break;
        if(spell.Name=="Grease"&&GreaseError()!="")yield break;
        if(spell.Name=="Entangle"&&EntangleError()!="")yield break;
        if(spell.Name=="Enlarge/Reduce"&&SizeChangeError(targetPlayer)!="")yield break;
        if(spell.Name=="Ray of Enfeeblement"&&EnfeeblementError(targetPlayer)!="")yield break;
        if(spell.Name=="Protection from Poison"&&PoisonProtectionError(targetPlayer)!="")yield break;
        if(spell.Name=="Jump"&&JumpSpellError(targetPlayer,slotLevel,jumpBoth)!="")yield break;
        if(spell.Name=="Blindness/Deafness"&&BlindnessDeafnessError(targetPlayer,slotLevel,blindnessBoth)!="")yield break;
        if(spell.Name=="Lesser Restoration"&&LesserRestorationError(targetPlayer,restorationCondition)!="")yield break;
        if(spell.Name=="Polymorph"&&PolymorphError()!="")yield break;
        if(spell.Name=="Shillelagh"&&(!targetPlayer||ShillelaghError()!=""))yield break;
        if(!embedded&&!(spell.Bonus?(combat.Aftermath?combat.TryUseAftermathBonusAction():combat.TryUseBonusAction()):AftermathLightSpell(spell.Name)?combat.TryUseAftermathMagicAction():combat.TryUseMagicAction()))yield break;
        if(!embedded){spellCastSequence++;ConsumeSpeciesSpell(spell.Name,caster);PlayerCastWhileSubmerged(spell.Name);}
        rolling=true;logScroll=Vector2.zero;
        if(spell.Name=="Produce Flame")EndProduceFlame();
        if(spell.Name=="Light")EndLightSpell("再次施放光亮术");
        if(spell.Name=="Prayer of Healing"||spell.Name==LaughterEffect.Name||spell.Name=="Moonbeam"||spell.Name=="Resistance"||spell.Name=="Gust of Wind"||spell.Name=="Stinking Cloud"||spell.Name=="Spirit Guardians"||spell.Name=="Sleet Storm"||spell.Name=="Call Lightning"||spell.Name=="Conjure Animals"||spell.Name=="Elemental Weapon"||spell.Name=="Crusader's Mantle"||spell.Name=="Flame Blade"||spell.Name=="Spiritual Weapon"||spell.Name=="Witch Bolt"||spell.Name=="Banishment"||spell.Name=="Vampiric Touch"||spell.Name=="Dragon’s Breath"||spell.Name=="Pass without Trace"||spell.Name=="Beacon of Hope"||spell.Name=="Aura of Vitality"||spell.Name=="Flaming Sphere"||spell.Name=="Heat Metal"||spell.Name=="Heroism"||spell.Name=="Cloud of Daggers"||spell.Name=="Web"||spell.Name=="Dancing Lights"||spell.Name=="Faerie Fire"||spell.Name=="Stoneskin"||spell.Name=="Sleep"||spell.Name=="Spike Growth"||spell.Name=="Entangle"||spell.Name=="Enlarge/Reduce"||spell.Name=="Ray of Enfeeblement")EndConcentration("开始新的专注法术");
        var interruption=new SpellInterruption();yield return TryInterruptSpell(true,spell.Name,interruption);
        if(interruption.Cancelled){if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Mind Spike")EndConcentration("开始新的专注法术");
        int modifier=AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),bonus=classes.ProficiencyBonus+modifier;
        string damageType=spell.Name=="Sorcerous Burst"?BurstTypes[burstType]:spell.DamageType;
        string target=targetPlayer?"玩家":"木桩";bool critical=false,saveSucceeded=false;HitRetaliation selfHitRetaliation=null;
        if(spell.Name=="Shillelagh"){ApplyShillelagh(caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Mage Armor"){ApplyMageArmor(targetPlayer);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Detect Evil and Good"){ApplyDetectEvilGood();if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Jump"){if(jumpBoth){ApplyJumpSpell(true);ApplyJumpSpell(false);}else ApplyJumpSpell(targetPlayer);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Destructive Wave"){yield return ResolveDestructiveWave(caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Chain Lightning"){yield return ResolveChainLightning(caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Mass Heal"){ApplyMassHeal();if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Heal"){ApplyHealSpell(slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Find Steed"){ApplyFindSteed(caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Faerie Fire"){yield return ResolveFaerieFire(entangleAnchor,caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Dancing Lights"){ApplyDancingLights();if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Cloud of Daggers"){yield return CastCloudDaggers(entangleAnchor,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Web"){ApplyWeb(entangleAnchor,caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Conjure Barrage"||conjuredVolley){yield return ResolveConjuredWeapons(true,caster,slotLevel,conjuredVolley,barrageAim,entangleAnchor,conjuredSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Cone of Cold"){yield return ResolveConeOfCold(true,caster,slotLevel,coldAim);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Finger of Death"){yield return ResolveFingerOfDeath(targetPlayer,caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Fire Storm"){yield return ResolveFireStorm(true,caster,stormSelection);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Animate Dead"){BeginAnimateDead(caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Flame Strike"){yield return ResolveFlameStrike(true,caster,slotLevel,entangleAnchor);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Prayer of Healing"){BeginPrayer(prayerSelected,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name==LaughterEffect.Name){yield return CastLaughter(laughterSelected,caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Moonbeam"){yield return CastMoonbeam(entangleAnchor,caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Resistance"){CastResistance(resistanceSelected,resistanceSelectedType);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Gust of Wind"){yield return CastGustWind(gustAim,gustEdge,gustOffset,caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Stinking Cloud"){CastStinkingCloud(entangleAnchor,caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Spirit Guardians"){CastGuardians(caster,slotLevel,guardExcluded,guardAppearance);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Sleet Storm"){CastSleetStorm(entangleAnchor,caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Call Lightning"){yield return CastCallLightning(lightningCloud,entangleAnchor,caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Conjure Animals"){CastConjureAnimals(entangleAnchor,caster,slotLevel,packChosenAppearance);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Elemental Weapon"){CastElementalWeapon(elementalSelected,slotLevel,elementalType);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Crusader's Mantle"){CastCrusaderMantle();if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Fire Shield"){CastFireShield(slotLevel,fireShieldSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Flame Blade"){CastFlameBlade(caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Chromatic Orb"){yield return ResolveChromaticOrb(caster,slotLevel,orbSelected,orbSelectedType);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Spiritual Weapon"){yield return CastSpiritualWeapon(spiritualPoint,caster,slotLevel,spiritualSelected,spiritualSelectedAttack,spiritualSelectedHost);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Witch Bolt"){yield return CastWitchBolt(caster,slotLevel,witchSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Regenerate"){BeginRegenerate(regenerateSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Banishment"){yield return ResolveBanishment(caster,slotLevel,banishSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Vampiric Touch"){yield return CastVampiricTouch(caster,slotLevel,vampiricSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Vitriolic Sphere"){yield return ResolveVitriolicSphere(true,caster,slotLevel,entangleAnchor);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Ice Storm"){yield return ResolveIceStorm(true,caster,slotLevel,entangleAnchor);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Produce Flame"){CastProduceFlame(caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Continual Flame"){CastContinualFlame(continualSelected,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Pass without Trace"){CastPassTrace(traceSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Beacon of Hope"){CastBeaconHope(beaconSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Aura of Vitality"){yield return CastAuraVitality();if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Freedom of Movement"){CastFreedomMovement(freedomSelected,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Water Walk"){CastWaterWalk(walkSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Warding Bond"){CastWardingBond(bondedSelection);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Flaming Sphere"){CastFlamingSphere(entangleAnchor,caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Heat Metal"){yield return CastHeatMetal(heatedSelection,caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Heroism"){ApplyHeroism(caster,slotLevel,heroismSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Melf’s Acid Arrow"){yield return ResolveAcidArrow(0,caster,slotLevel,acidSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Dragon’s Breath"){CastDragonBreath(caster,dragonSelected,slotLevel,dragonTypeSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Dissonant Whispers"){yield return ResolveDissonantWhispers(0,caster,slotLevel,whispersSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Command"){yield return ResolveCommandCast(0,caster,chosenCommand,commandSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Color Spray"){yield return ResolveColorSpray(true,caster,burningAim);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Lightning Bolt"){yield return ResolveLightningBolt(true,caster,slotLevel,lightningAim);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Burning Hands"){yield return ResolveBurningHands(true,caster,slotLevel,burningAim);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Starry Wisp"){yield return ResolvePlayerStarryWisp(caster,starrySelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Feign Death"){ApplyFeignDeath(feignSelected);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Light"){ApplyLightSpell(lightTarget,lightR,lightG,lightB);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Stoneskin"){ApplyStoneskin();if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Water Breathing"){ApplyWaterBreathing(targetPlayer,directBothTargets);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Sleep"){yield return ResolveSleep(caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Spike Growth"){ResolveSpikeGrowth(entangleAnchor,caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Grease"){yield return ResolveGrease(entangleAnchor,caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Entangle"){yield return ResolveEntangle(entangleAnchor,caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Enlarge/Reduce"){yield return ResolveSizeChange(targetPlayer,caster,sizeDelta,sizeConsent,chosenSizeAnchor);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Ray of Enfeeblement"){yield return ResolveEnfeeblement(targetPlayer,caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Protection from Poison"){ApplyPoisonProtection(targetPlayer);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Silence"){ApplySilence();if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Lesser Restoration"){ApplyLesserRestoration(targetPlayer,restorationCondition);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Blindness/Deafness"){yield return ResolveBlindnessDeafness(targetPlayer,caster,slotLevel,blindnessBoth,blindnessPlayer,blindnessMonster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Arcane Vigor"){yield return ResolveArcaneVigor(caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Polymorph"){yield return ResolvePolymorph(targetPlayer,caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Remove Curse"){bool removed=mark.RemoveCurse(targetPlayer);if(removed)EndConcentration("移除诅咒解除脆弱诅咒");history.Insert(0,"移除诅咒："+(removed?"已解除目标的脆弱诅咒。":"目标当前没有已接入的诅咒效果；物件诅咒与同调系统暂缓。"));if(!embedded)rolling=false;yield break;}
        if(IsSenseSpell(spell.Name)){ApplySenseSpell(spell,targetPlayer);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Ice Knife"){yield return ResolveIceKnife(targetPlayer,caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(IsPointBurst(spell)){yield return ResolvePointBurst(spell,caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Mirror Image"){mirrorImage.Cast(worldRounds);history.Insert(0,"镜像术：三个替身，持续1分钟，无需专注。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Death Ward"){ApplyDeathWard(targetPlayer);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Misty Step"||spell.Name=="Expeditious Retreat"){ApplyMobilitySpell(spell);if(!embedded)rolling=false;yield break;}
        if(IsVisionSpell(spell.Name)){ApplyVisionSpell(spell,targetPlayer,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="True Strike"){yield return Roll(selectedWeapon,modifier,classes.ProficiencyBonus,"施法属性修正",fighter.ConsumeAdvantage(classes[0]),"克敌机先",trueStrike:true,radiantWeapon:trueStrikeRadiant);if(!embedded)rolling=false;yield break;}
        if(IsSimpleEmanation(spell.Name)){yield return ResolveSimpleEmanation(spell,caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Guidance"||spell.Name=="Enhance Ability"){ApplyCheckBuff(spell,targetPlayer);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Magic Missile"){yield return ResolveMagicMissile(spell.DiceCount(classes.TotalLevel,slotLevel));if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Bane"){yield return ResolveBane(targetPlayer,caster,directBothTargets);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Hex"||spell.Name=="Hunter's Mark"){EndConcentration("开始新的专注法术");bool hex=spell.Name=="Hex";mark.Cast(hex,targetPlayer,hexAbility);concentrationSpell=spell.Chinese;concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+MarkEffect.Duration(hex,slotLevel);history.Insert(0,spell.Chinese+"标记"+target+"，攻击检定命中额外1d6"+(hex?"死灵；所选属性检定劣势（不影响豁免）":"力场；寻找目标的察觉/求生优势")+"。玩家技能入口已接入对应属性劣势与寻找目标优势；场景探索结果由手动DC检定表示。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Bless"){EndConcentration("开始新的专注法术");concentrationSpell="祝福术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;if(targetPlayer||directBothTargets)combat.PlayerBlessed=true;if(!targetPlayer||directBothTargets)combat.MonsterBlessed=true;history.Insert(0,"祝福术：选定目标攻击与豁免每次+1d4，专注至多1分钟。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Armor of Agathys"){agathys.Cast(slotLevel,worldRounds);combat.GrantPlayerTemporaryHp(slotLevel*5);history.Insert(0,"黯冰狱铠：获得"+(slotLevel*5)+"临时生命，近战命中反击"+(slotLevel*5)+"寒冷，至多1小时，临时生命耗尽时结束。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Power Word Fortify"){ApplyFortify();if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Aid"){ApplyAid(targetPlayer,slotLevel,directBothTargets);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Shield of Faith"){EndConcentration("开始新的专注法术");concentrationSpell="虔诚护盾";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+100;if(targetPlayer)combat.PlayerArmorBonus=2;else combat.MonsterArmorBonus=2;history.Insert(0,target+"护甲等级+2，专注至多10分钟。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Blade Ward"){EndConcentration("开始新的专注法术");concentrationSpell="剑刃防护";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;combat.PlayerBladeWard=true;history.Insert(0,"剑刃防护：攻击玩家的检定减去1d4，专注至多1分钟。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Protection from Energy"){EndConcentration("开始新的专注法术");concentrationSpell="防护能量";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+600;if(targetPlayer)combat.PlayerEnergyResistance=EnergyTypes[energyChoice];else combat.MonsterEnergyResistance=EnergyTypes[energyChoice];history.Insert(0,target+"获得"+DamageTypes.Names[EnergyTypes[energyChoice]]+"抗性，专注至多1小时。");if(!embedded)rolling=false;yield break;}
        if(IsTimedBuff(spell.Name)){ApplyTimedBuff(spell,targetPlayer,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Eldritch Blast"){yield return ResolveEldritchBlast(caster);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Scorching Ray"){yield return ResolveScorchingRays(caster,slotLevel);if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Power Word Kill"&&(targetPlayer?combat.PlayerHp:combat.Hp)<=100){if(targetPlayer)combat.KillPlayer();else combat.KillMonster();history.Insert(0,"律令死亡："+target+((targetPlayer?combat.PlayerDead:combat.Hp==0)?"生命不超过100，直接死亡。":"防死结界抵消本次直接死亡，生命不变。"));CheckBattleOutcome();if(!embedded)rolling=false;yield break;}
        if(spell.Control){yield return ResolveHoldPerson(spell,targetPlayer,caster,directBothTargets);if(!embedded)rolling=false;yield break;}
        if(spell.Stable){combat.StabilizePlayer();history.Insert(0,"维生术："+target+"稳定。");if(!embedded)rolling=false;yield break;}
        if(spell.Attack){
            bonus+=PlayerAquaticD20Penalty;
            // A hostile creature within 5 feet imposes ranged-attack disadvantage.
            bool disadvantage=BlurDisadvantage(true,targetPlayer)||Visibility(true,targetPlayer)==2||!spell.Melee&&combat.MonsterCanAct&&MonsterSeesPlayer&&map.DistanceFeet<=5;
            bool advantage=fighter.ConsumeAdvantage(classes[0])||FaerieAdvantage(targetPlayer?0:-1,Visibility(true,targetPlayer)<2)||Visibility(targetPlayer,true)==2||(!targetPlayer&&(combat.MonsterHelpless||(combat.MonsterProne&&map.DistanceFeet<=5)));
            advantage=Entangled(targetPlayer)|combat.ConsumeGuidingLight(targetPlayer)||advantage;
            disadvantage=ResurrectionD20Disadvantage(true)|FearDisadvantage(true)|Entangled(true)|RayAttackDisadvantage(true,SpellAbilityIndex(caster))||combat.PlayerPoisoned||combat.PlayerProne||disadvantage||(!targetPlayer&&combat.MonsterProne&&map.DistanceFeet>5);
            bool rollHigh=advantage&&!disadvantage,rollLow=disadvantage&&!advantage;
            int first=UnityEngine.Random.Range(1,21),second=rollHigh||rollLow?UnityEngine.Random.Range(1,21):0;
            int value=rollHigh?Math.Max(first,second):rollLow?Math.Min(first,second):first,armor=AttackArmor(true,targetPlayer);
            LogAttackCover(true,targetPlayer);
            int blessing=combat.PlayerBlessed?UnityEngine.Random.Range(1,5):0,bane=combat.PlayerBaned?UnityEngine.Random.Range(1,5):0;bonus+=blessing-bane;
            BreakOrdinaryInvisibility(true,"进行攻击检定");
            int wardPenalty=targetPlayer&&combat.PlayerBladeWard?UnityEngine.Random.Range(1,5):0;bonus-=wardPenalty;
            mirrorIntercepted=false;
            if(targetPlayer&&value!=1&&(value==20||value+bonus>=armor)){yield return ResolvePlayerHitDefense(value,value+bonus,true,PerceivesWithSpecialSense(true,true),AttackCover(true,true));armor=AttackArmor(true,true);}
            critical=value==20;dice=spell.Chinese+(spell.Melee?" · 近战法术攻击":" · 远程法术攻击");
            yield return PresentRoll(20,second>0?new[]{first,second}:new[]{first},(rollHigh?"优势，采用 ":rollLow?"劣势，采用 ":"采用 ")+value+" + "+bonus+" / 护甲 "+armor);
            if(blessing>0)yield return PresentRoll(4,new[]{blessing},"祝福术：本次攻击 +"+blessing);
            if(bane>0)yield return PresentRoll(4,new[]{bane},"灾祸术：本次攻击 -"+bane);
            if(wardPenalty>0)yield return PresentRoll(4,new[]{wardPenalty},"剑刃防护减值 "+wardPenalty);
            if(mirrorIntercepted){dice="镜像术拦截 · 本体伤害0";if(!embedded)rolling=false;yield break;}
            if(value==1||(!critical&&value+bonus<armor)){history.Insert(0,spell.Chinese+"对"+target+"：D20="+value+" + "+bonus+"，未命中。");fighter.RecordMiss(classes[0]);if(!embedded)rolling=false;yield break;}
        }
        if(spell.Attack&&targetPlayer&&spell.Melee)selfHitRetaliation=CaptureHitRetaliation(0,true);
        if(spell.Attack&&!targetPlayer&&combat.MonsterHelpless&&map.DistanceFeet<=5)critical=true;
        if(spell.SaveAbility>=0){
            int targetModifier=AcidSplash.Modifier((targetPlayer?attributes:monsterAttributes)[spell.SaveAbility]);
            if(targetPlayer)targetModifier=PlayerSavingThrowModifier(spell.SaveAbility);
            var armorSave=RollArmorSave(targetPlayer,spell.SaveAbility);int value=armorSave.Value,dc=8+bonus;bool automatic=(spell.SaveAbility==0||spell.SaveAbility==1)&&(targetPlayer?(combat.PlayerHp==0||combat.PlayerParalyzed):combat.MonsterHelpless);
            automatic=automatic||(spell.Name=="Blight"&&!targetPlayer&&monsterProfile.TypeIndex==12);
            dice=spell.Chinese+" · "+target+"豁免";
            var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(targetPlayer,automatic,adjustment,spell.SaveAbility,ignoreCover:spell.Name=="Sacred Flame");targetModifier+=adjustment.Value;
            if(!automatic)yield return PresentRoll(20,armorSave.Dice,value+" + "+targetModifier+" / DC "+dc);
            saveSucceeded=!automatic&&value+targetModifier>=dc;
            if(saveSucceeded&&!spell.HalfSave){history.Insert(0,spell.Chinese+"："+target+"豁免成功，伤害0。");if(!embedded)rolling=false;yield break;}
        }
        if(spell.Name=="Mind Spike"&&!saveSucceeded){mindSpike.Cast(targetPlayer,worldRounds);concentrationSpell=spell.Chinese;concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+600;history.Insert(0,"心灵尖刺：持续知晓"+(targetPlayer?"玩家":"木桩")+"位置，目标的隐形对施法者失去效果；黑暗与云雾仍会遮挡视觉。");}
        int damageSides=spell.Name=="Toll the Dead"&&(targetPlayer?combat.PlayerHp<combat.PlayerMaxHp:combat.Hp<combat.MaxHp)?12:spell.Sides;
        int count=spell.DiceCount(classes.TotalLevel,slotLevel)*(critical?2:1),amount=0;
        int[] rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=UnityEngine.Random.Range(1,damageSides+1);amount+=rolls[i];}
        if(spell.Healing)amount=Math.Max(0,amount+modifier);
        if(spell.Name=="False Life")amount+=4+5*(slotLevel-1);

        dice=spell.Chinese+(critical?" · 暴击":"")+" · "+count+"d"+damageSides;
        yield return PresentRoll(damageSides,rolls,"骰值 ["+string.Join("+",rolls)+"]"+(spell.Healing?" + 施法属性 "+modifier:spell.Name=="False Life"?" + 固定加值 "+(4+5*(slotLevel-1)):"")+(saveSucceeded&&spell.HalfSave?"，豁免成功减半向下取整":"")+" = "+amount);
        if(spell.Name=="Sorcerous Burst"){var burst=new BurstDamage();yield return RollSorcerousBurstExtra(rolls,modifier,critical,burst);amount+=burst.Extra;}
        if(spell.Name=="False Life"){combat.GrantPlayerTemporaryHp(amount);history.Insert(0,"虚假生命：掷出临时生命 "+amount+"，当前保留 "+combat.PlayerTemporaryHp+"（不叠加）。");if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Mass Healing Word"&&directBothTargets){int playerHeal=combat.HealPlayer(RollHealingAmount(0,amount,(long)count*damageSides+modifier)),monsterHeal=combat.HealMonster(RollHealingAmount(-1,amount,(long)count*damageSides+modifier));ApplySteedLifeBond(playerHeal,spell.Level);history.Insert(0,"群体治愈真言：玩家实际恢复 "+playerHeal+"，木桩实际恢复 "+monsterHeal+"；使用同组治疗骰结果。");dice="群体治愈真言 · 已结算";if(!embedded)rolling=false;yield break;}
        if(spell.Name=="Ray of Sickness"){combat.ApplyPoison(targetPlayer);history.Insert(0,target+"中毒，至施法者下一回合结束；攻击与属性检定具有劣势。");}
        if(spell.Name=="Mind Sliver"){combat.ApplySliver(targetPlayer);history.Insert(0,target+"下一次豁免减1d4，至玩家下一回合结束失效。");}
        if(!spell.Healing){
            var raw=new long[DamageTypes.Names.Length];raw[DamageTypes.Index(damageType)]=amount;
            if(spell.Attack){var extra=new ExtraMarkDamage();yield return RollMarkDamage(targetPlayer,critical,extra);if(extra.Raw>0)raw[mark.DamageType]+=extra.Raw;}
            var reduced=new RayDamage();yield return ReduceRayPacket(true,raw,true,spell.Chinese,reduced);
            if(saveSucceeded&&spell.HalfSave)for(int i=0;i<reduced.Raw.Length;i++)reduced.Raw[i]/=2;
            amount=ResolveRayPacket(targetPlayer,reduced.Raw);
        }
        if(spell.Healing){amount=RollHealingAmount(targetPlayer?0:-1,amount,(long)count*damageSides+modifier);amount=targetPlayer?combat.HealPlayer(amount):combat.HealMonster(amount);if(targetPlayer)ApplySteedLifeBond(amount,spell.Level);}
        else if(targetPlayer){combat.ApplyPlayerDamage(amount,critical,sourcePlayer:true);if(selfHitRetaliation!=null)yield return ResolveHitRetaliation(0,selfHitRetaliation);yield return MaintainConcentration(amount);if(selfHitRetaliation!=null)yield return MaintainRetaliationConcentration(selfHitRetaliation);}else combat.ApplyDamage(amount);
        if(spell.Name=="Harm"&&!saveSucceeded){int reduced=combat.ReduceMaxHp(targetPlayer,amount);history.Insert(0,"伤害术："+target+"生命上限减少 "+reduced+"，上限最低1；不恢复生命。");}
        if(spell.Name=="Guiding Bolt"){combat.ApplyGuidingLight(targetPlayer);history.Insert(0,"对"+target+"的下一次攻击具有优势，最迟在施法者下一回合结束失效。");}
        if(spell.Name=="Ray of Frost"){combat.ApplyFrostSlow(targetPlayer);history.Insert(0,target+"速度降低10尺，至玩家下一回合开始。");}
        if(spell.Name=="Shocking Grasp"){combat.BlockOpportunity(targetPlayer);history.Insert(0,target+"不能借机攻击，至其下一回合开始；其他反应不受影响。");}
        if(spell.Name=="Thorn Whip")ApplyThornWhipPull(targetPlayer);
        if(spell.Name=="Vicious Mockery"){combat.ApplyMockery(targetPlayer);history.Insert(0,target+"下次攻击具有劣势，最迟在其下一回合结束时失效。");}
        if(spell.Name=="Chill Touch"){combat.BlockHealing(targetPlayer);history.Insert(0,target+"不能恢复生命，持续至玩家下一回合结束。");}
        history.Insert(0,spell.Chinese+"对"+target+(spell.Healing?"实际治疗 ":"造成"+damageType+"伤害 ")+amount+"。");
        dice=spell.Chinese+" · "+amount;CheckBattleOutcome();if(!embedded)rolling=false;
    }
}
