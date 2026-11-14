using System;

// Hand-authored mechanical effects; descriptions are project summaries, not copied rule text.
public sealed class DirectSpell
{
    public readonly string Name,Chinese,Effect,Summary,DamageType;
    public readonly int Level,Range,Sides,SaveAbility,BaseDice,UpcastDice;
    public readonly bool Bonus,Attack,Healing,Stable,HalfSave,Control,Melee;
    public DirectSpell(string name,string chinese,string effect,int level,int range,int sides,bool bonus=false,bool attack=false,bool healing=false,bool stable=false,int save=-1,string damageType="",int baseDice=0,int upcastDice=0,bool halfSave=false,bool control=false,bool melee=false)
    {
        Name=name;Chinese=chinese;Effect=effect;DamageType=damageType;BaseDice=baseDice;UpcastDice=upcastDice;HalfSave=halfSave;Control=control;Melee=melee;Level=level;Range=range;Sides=sides;Bonus=bonus;Attack=attack;Healing=healing;Stable=stable;SaveAbility=save;
        Summary=chinese+"｜"+name+"\n"+effect+(name=="Death Ward"?"\n非专注，持续8小时。":name=="Fog Cloud"?"\n专注，至多1小时。":name=="Expeditious Retreat"?"\n专注，至多10分钟。":name=="Darkness"?"\n专注，至多10分钟。":name=="Darkvision"||name=="Daylight"?"\n无需专注。":name=="Guidance"?"\n专注，至多1分钟。":name=="Enhance Ability"?"\n专注，至多1小时。":name=="Aid"?"\n非专注，持续8小时。":name=="Hex"||name=="Hunter's Mark"?"\n专注，持续时间由环阶决定。":name=="Bane"||name=="Bless"?"\n专注，至多1分钟。":name=="Shield of Faith"?"\n专注，至多10分钟。":name=="Blade Ward"?"\n专注，至多1分钟。":name=="Protection from Energy"?"\n专注，至多1小时。":control?"\n专注，至多1分钟。":name=="Divine Favor"||name=="Magic Weapon"||name=="Longstrider"||name=="Barkskin"||name=="Armor of Agathys"?"\n无需专注。":"\n非专注，即时。");
    }
    public int DiceCount(int totalLevel,int slotLevel) => Name=="Scorching Ray"?1+Math.Max(2,slotLevel):Name=="Magic Missile"?2+Math.Max(1,slotLevel):Healing?(BaseDice>0?BaseDice+Math.Max(0,slotLevel-Level)*UpcastDice:2*Math.Max(1,slotLevel)):Stable?0:BaseDice>0?BaseDice+Math.Max(0,slotLevel-Level)*UpcastDice:AcidSplash.DiceCount(totalLevel);
    public int EffectiveRange(int totalLevel) => Stable?Range*(1<<(AcidSplash.DiceCount(totalLevel)-1)):Range;
    public static readonly DirectSpell[] Available={
        new DirectSpell("Death Ward","防死结界","1动作 · 接触（5尺） · 声音、姿势。首次将降至0生命时改为1生命（包括伤害或临时生命上限效果结束）；或抵消一次不造成伤害的直接死亡效果，然后法术结束。不会阻止已经倒地后的死亡豁免失败。",4,5,4),
        new DirectSpell("Misty Step","迷踪步","附赠动作 · 自身 · 声音。传送至30尺内可见且未被占据的地面位置，不消耗移动力、不引发借机攻击；仍须容纳完整占地。",2,0,4,bonus:true),
        new DirectSpell("Expeditious Retreat","脚底抹油","附赠动作 · 自身 · 声音、姿势。立即疾走一次，持续期间可使用附赠动作疾走。疾走增加相当于当前速度的本回合移动距离。",1,0,4,bonus:true),
        new DirectSpell("Fog Cloud","云雾术","1动作 · 120尺 · 声音、姿势。指定点产生20尺半径重度遮蔽云雾，每升1环半径+20尺。普通视觉与黑暗视觉不能穿透；强风环境消散暂缓。",1,120,4),
        new DirectSpell("Darkvision","黑暗视觉","1动作 · 接触（5尺） · 声音、姿势。自愿生物获得150尺黑暗视觉，持续8小时；不能看穿黑暗术。",2,5,4),
        new DirectSpell("Darkness","黑暗术","1动作 · 60尺 · 声音。固定点产生15尺半径魔法黑暗，普通黑暗视觉无法穿透，非魔法光不能照亮；与二环以下光照法术重叠时驱散后者。物件承载与遮盖的环境用法暂缓。",2,60,4),
        new DirectSpell("Daylight","昼明术","1动作 · 60尺 · 声音、姿势。固定点产生60尺半径明亮阳光，外围再60尺微光，持续1小时；驱散重叠的三环以下黑暗法术。物件承载与遮盖暂缓。",3,60,4),
        new DirectSpell("True Strike","克敌机先","1施法动作 · 自身 · 姿势。用当前武器攻击一次，命中与伤害改用施法属性；可选武器原伤害或光耀。总等级5/11/17额外1d6/2d6/3d6光耀，暴击伤害骰翻倍，不触发额外攻击。",0,0,6),
        new DirectSpell("Guidance","指引术","1动作 · 接触（5尺） · 声音、姿势。自愿生物所选一项技能的每次属性检定加1d4，使用后不消失。",0,5,4),
        new DirectSpell("Enhance Ability","强化属性","1动作 · 接触（5尺） · 声音、姿势。目标所选力量/敏捷/智力/感知/魅力属性的检定具有优势；每升一环增加一目标，各目标可选不同属性。",2,5,4),
        new DirectSpell("Cure Wounds","疗伤术","1动作 · 接触（5尺） · 声音、姿势。治疗2d8+施法属性调整值，每提升1环增加2d8。",1,5,8,healing:true),
        new DirectSpell("Healing Word","治愈真言","附赠动作 · 60尺 · 声音。治疗2d4+施法属性调整值，每提升1环增加2d4。",1,60,4,bonus:true,healing:true),
        new DirectSpell("Spare the Dying","维生术","1动作 · 15尺 · 声音、姿势。稳定生命为0且未死亡的生物。总等级5/11/17时射程为30/60/120尺。",0,15,0,stable:true),
        new DirectSpell("Fire Bolt","火焰箭","1动作 · 120尺 · 声音、姿势。远程法术攻击，命中造成1d10火焰伤害。总等级5/11/17时增加1d10；自然20伤害骰翻倍。",0,120,10,attack:true,damageType:"火焰"),
        new DirectSpell("Sacred Flame","圣火术","1动作 · 60尺 · 声音、姿势。敏捷豁免失败受1d8光耀伤害，成功无伤害。总等级5/11/17时增加1d8；豁免不受半身或四分之三掩护影响。",0,60,8,save:1,damageType:"光耀"),
        new DirectSpell("Hold Person","人类定身术","1动作 · 60尺 · 声音、姿势。仅类人生物：感知豁免失败则麻痹，每个目标回合结束可再次豁免。二环1目标，提升环阶可增加目标。",2,60,0,save:3,control:true),
        new DirectSpell("Inflict Wounds","致伤术","1动作 · 接触（5尺） · 声音、姿势。体质豁免失败受2d10死灵伤害，成功减半向下取整。每提升1环增加1d10。",1,5,10,save:2,damageType:"死灵",baseDice:2,upcastDice:1,halfSave:true),
        new DirectSpell("Power Word Kill","律令死亡","1动作 · 60尺 · 声音。当前生命不超过100的目标直接死亡，否则受到12d12心灵伤害。无需命中或豁免。",9,60,12,damageType:"心灵",baseDice:12),
        new DirectSpell("Bane","灾祸术","1动作 · 30尺 · 声音、姿势。至多3名生物魅力豁免失败后，攻击与豁免每次减1d4；专注1分钟。",1,30,4),
        new DirectSpell("Hex","脆弱诅咒","附赠动作 · 90尺 · 声音、姿势。攻击检定命中目标额外1d6死灵伤害；选择一项属性，使目标该属性检定具有劣势（非豁免）。专注1小时；二环4小时、三四环8小时、五环以上24小时。",1,90,6,bonus:true),
        new DirectSpell("Hunter's Mark","猎人印记","附赠动作 · 90尺 · 声音。攻击检定命中目标额外1d6力场伤害，寻找目标的察觉/求生具有优势。专注1小时；三四环8小时、五环以上24小时。",1,90,6,bonus:true),
        new DirectSpell("Mind Sliver","心灵之楔","1动作 · 60尺 · 声音。智力豁免失败受1d6心灵伤害，并在施法者下一回合结束前的下一次豁免减1d4；伤害随总等级强化。",0,60,6,save:4,damageType:"心灵"),
        new DirectSpell("Bless","祝福术","1动作 · 30尺 · 声音、姿势。至多3名生物每次攻击与豁免加1d4，专注至多1分钟；升环可增加目标。",1,30,4),
        new DirectSpell("Armor of Agathys","黯冰狱铠","附赠动作 · 自身 · 声音、姿势。获得5临时生命，被近战攻击命中时对攻击者造成5寒冷伤害；每升环两者各+5。持续1小时，无临时生命时提前结束，无需专注。",1,0,4,bonus:true),
        new DirectSpell("Mass Healing Word","群体治愈真言","附赠动作 · 60尺 · 声音。至多6名可见目标共享2d4+施法属性调整值的治疗；每升1环增加1d4。",3,60,4,bonus:true,healing:true,baseDice:2,upcastDice:1),
        new DirectSpell("Power Word Fortify","律令巩固","1动作 · 60尺 · 声音。为至多6名可见生物分配共计120点临时生命值；不叠加已有临时生命，不视为治疗。",7,60,4),
        new DirectSpell("Aid","援助术","1动作 · 30尺 · 声音、姿势。至多3名目标的当前HP与上限各+5，持续8小时；每升1环额外+5，不叠加同名效果。到期扣回增加的当前HP，最低0。",2,30,4),
        new DirectSpell("Shield of Faith","虔诚护盾","附赠动作 · 60尺 · 声音、姿势。目标护甲等级+2，专注，至多10分钟。",1,60,4,bonus:true),
        new DirectSpell("Barkskin","树肤术","附赠动作 · 接触（5尺） · 声音、姿势。自愿目标的最终AC至少为17，持续1小时，无需专注。",2,5,4,bonus:true),
        new DirectSpell("Blade Ward","剑刃防护","1动作 · 自身 · 声音、姿势。持续期间，任何生物对你的每次攻击检定都减去独立1d4。专注，至多1分钟。",0,0,4),
        new DirectSpell("Protection from Energy","防护能量","1动作 · 接触（5尺） · 声音、姿势。自愿生物获得一种能量抗性：强酸、寒冷、火焰、闪电或雷鸣。专注，至多1小时。",3,5,4),
        new DirectSpell("Divine Favor","神恩","附赠动作 · 自身 · 声音、姿势。持续1分钟，武器攻击命中额外1d4光耀伤害；无需专注。",1,0,4,bonus:true),
        new DirectSpell("Magic Weapon","魔化武器","附赠动作 · 接触 · 声音、姿势。当前非魔法武器命中与伤害+1，持续1小时；三至五环+2，六环以上+3。再施放时原效果结束，无需专注。",2,0,4,bonus:true),
        new DirectSpell("Longstrider","大步奔行","1动作 · 接触（5尺） · 声音、姿势。目标速度+10尺，持续1小时，无需专注；每升1环可额外指定一个目标。",1,5,4),
        new DirectSpell("Blight","枯萎术","1动作 · 30尺 · 声音、姿势。体质豁免失败8d8死灵伤害，成功减半；植物生物自动失败。每升1环增加1d8；非生物植物的环境用法暂缓。",4,30,8,save:2,damageType:"死灵",baseDice:8,upcastDice:1,halfSave:true),
        new DirectSpell("Guiding Bolt","光导箭","1动作 · 120尺 · 声音、姿势。远程法术攻击，命中4d6光耀伤害；至施法者下一回合结束，对目标的下一次攻击具有优势。每升1环增加1d6。",1,120,6,attack:true,damageType:"光耀",baseDice:4,upcastDice:1),
        new DirectSpell("Scorching Ray","灼热射线","1动作 · 120尺 · 声音、姿势。三道射线分别选择目标并进行远程法术攻击，每道命中2d6火焰伤害；每升1环增加一道。",2,120,6,attack:true,damageType:"火焰",baseDice:2),
        new DirectSpell("Thunderclap","鸣雷破","1动作 · 自身5尺光环 · 姿势。范围内生物体质豁免失败受1d6雷鸣伤害，随总等级强化；巨大声响可传至100尺。",0,0,6,save:2,damageType:"雷鸣"),
        new DirectSpell("Word of Radiance","光耀祷词","1动作 · 自身5尺光环 · 声音。选择范围内可见生物，体质豁免失败受1d6光耀伤害，随总等级强化。",0,0,6,save:2,damageType:"光耀"),
        new DirectSpell("Thunderwave","雷鸣波","1动作 · 自身15尺立方 · 声音、姿势。体质豁免失败受2d8雷鸣并被推离10尺，成功伤害减半；每升1环增加1d8。",1,0,8,save:2,damageType:"雷鸣",baseDice:2,upcastDice:1,halfSave:true),
        new DirectSpell("Ray of Frost","冷冻射线","1动作 · 60尺 · 声音、姿势。远程法术攻击，命中1d8寒冷伤害，目标速度降低10尺至施法者下一回合开始；伤害随总等级强化。",0,60,8,attack:true,damageType:"寒冷"),
        new DirectSpell("Shocking Grasp","电爪","1动作 · 接触（5尺） · 声音、姿势。近战法术攻击，命中1d8闪电伤害，目标至其下一回合开始不能借机攻击；伤害随总等级强化。",0,5,8,attack:true,damageType:"闪电",melee:true),
        new DirectSpell("Magic Missile","魔法飞弹","1动作 · 120尺 · 声音、姿势。三枚飞弹自动命中，分别选择目标，每枚独立1d4+1力场伤害并同时结算；每升1环增加一枚，不进行攻击或豁免检定。",1,120,4,damageType:"力场",baseDice:3),
        new DirectSpell("False Life","虚假生命","1动作 · 自身 · 声音、姿势。获得2d4+4临时生命，每升1环增加5点；临时生命不叠加，不计为治疗。",1,0,4,baseDice:2),
        new DirectSpell("Vicious Mockery","恶言相加","1动作 · 60尺 · 声音。感知豁免失败受到1d6心灵伤害，且目标在其下一回合结束前的下一次攻击具有劣势。总等级5/11/17时增加1d6。",0,60,6,save:3,damageType:"心灵"),
        new DirectSpell("Chill Touch","颤栗之触","1动作 · 接触（5尺） · 声音、姿势。近战法术攻击，命中造成1d10死灵伤害，目标直至施法者下一回合结束不能恢复生命。总等级5/11/17时增加1d10。",0,5,10,attack:true,damageType:"死灵",melee:true),
        new DirectSpell("Toll the Dead","亡者丧钟","1动作 · 60尺 · 声音、姿势。感知豁免失败受到1d8死灵伤害；目标已损失生命时改用1d12。总等级5/11/17时增加一枚伤害骰。",0,60,8,save:3,damageType:"死灵"),
        new DirectSpell("Poison Spray","毒气喷溅","1动作 · 30尺 · 声音、姿势。远程法术攻击，命中造成1d12毒素伤害。总等级5/11/17时增加1d12；自然20伤害骰翻倍。",0,30,12,attack:true,damageType:"毒素")
    };
    public static DirectSpell Find(string name) {foreach(var spell in Available)if(spell.Name==name)return spell;return null;}
}
[Serializable] public sealed class SpellMetadata
{
    public string name,chineseName,school,castingTime,range,duration,source,implementation;
    public string[] classes,components;
    public int level;
}
[Serializable] public sealed class SpellMetadataSet {public SpellMetadata[] spells;public int count;}
