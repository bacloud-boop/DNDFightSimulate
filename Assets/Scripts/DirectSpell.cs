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
        Summary=chinese+"｜"+name+"\n"+effect+(control?"\n专注，至多1分钟。":"\n非专注，即时。");
    }
    public int DiceCount(int totalLevel,int slotLevel) => Name=="Magic Missile"?2+Math.Max(1,slotLevel):Healing?2*Math.Max(1,slotLevel):Stable?0:BaseDice>0?BaseDice+Math.Max(0,slotLevel-Level)*UpcastDice:AcidSplash.DiceCount(totalLevel);
    public int EffectiveRange(int totalLevel) => Stable?Range*(1<<(AcidSplash.DiceCount(totalLevel)-1)):Range;
    public static readonly DirectSpell[] Available={
        new DirectSpell("Cure Wounds","疗伤术","1动作 · 接触（5尺） · 声音、姿势。治疗2d8+施法属性调整值，每提升1环增加2d8。",1,5,8,healing:true),
        new DirectSpell("Healing Word","治愈真言","附赠动作 · 60尺 · 声音。治疗2d4+施法属性调整值，每提升1环增加2d4。",1,60,4,bonus:true,healing:true),
        new DirectSpell("Spare the Dying","维生术","1动作 · 15尺 · 声音、姿势。稳定生命为0且未死亡的生物。总等级5/11/17时射程为30/60/120尺。",0,15,0,stable:true),
        new DirectSpell("Fire Bolt","火焰箭","1动作 · 120尺 · 声音、姿势。远程法术攻击，命中造成1d10火焰伤害。总等级5/11/17时增加1d10；自然20伤害骰翻倍。",0,120,10,attack:true,damageType:"火焰"),
        new DirectSpell("Sacred Flame","圣火术","1动作 · 60尺 · 声音、姿势。敏捷豁免失败受1d8光耀伤害，成功无伤害。总等级5/11/17时增加1d8；豁免不受半身或四分之三掩护影响。",0,60,8,save:1,damageType:"光耀"),
        new DirectSpell("Hold Person","人类定身术","1动作 · 60尺 · 声音、姿势。仅类人生物：感知豁免失败则麻痹，每个目标回合结束可再次豁免。二环1目标，提升环阶可增加目标。",2,60,0,save:3,control:true),
        new DirectSpell("Inflict Wounds","致伤术","1动作 · 接触（5尺） · 声音、姿势。体质豁免失败受2d10死灵伤害，成功减半向下取整。每提升1环增加1d10。",1,5,10,save:2,damageType:"死灵",baseDice:2,upcastDice:1,halfSave:true),
        new DirectSpell("Power Word Kill","律令死亡","1动作 · 60尺 · 声音。当前生命不超过100的目标直接死亡，否则受到12d12心灵伤害。无需命中或豁免。",9,60,12,damageType:"心灵",baseDice:12),
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
