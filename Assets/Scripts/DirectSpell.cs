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
        Summary=chinese+"｜"+name+"\n"+effect+(name=="Find Steed"?"\n非专注，即时召唤；坐骑无固定持续时间。":name=="Faerie Fire"||name=="Dancing Lights"?"\n专注，至多1分钟。":name=="Heat Metal"?"\n专注，至多1分钟。":name=="Heroism"?"\n专注，至多1分钟。":name=="Cloud of Daggers"?"\n专注，至多1分钟。":name=="Web"?"\n专注，至多1小时。":name=="Stoneskin"?"\n专注，至多1小时。":name=="Water Breathing"?"\n非专注，持续24小时。":name=="Spike Growth"?"\n专注，至多10分钟。":name=="Grease"?"\n非专注，持续1分钟。":name=="Entangle"?"\n专注，至多1分钟。":name=="Enlarge/Reduce"?"\n专注，至多1分钟。":name=="Ray of Enfeeblement"?"\n专注，至多1分钟。":name=="Protection from Poison"?"\n非专注，持续1小时。":name=="Jump"?"\n非专注，持续1分钟。":name=="Blindness/Deafness"?"\n非专注，持续至多1分钟；目标回合末重复体质豁免。":name=="Silence"||name=="Detect Evil and Good"?"\n专注，至多10分钟。":name=="Mage Armor"?"\n非专注，持续8小时。":name=="Shillelagh"?"\n非专注，持续1分钟。":name=="Mind Spike"?"\n专注，至多1小时。":name=="Polymorph"?"\n专注，至多1小时。":name=="Invisibility"?"\n专注，至多1小时。":name=="Blur"||name=="Greater Invisibility"?"\n专注，至多1分钟。":name=="See Invisibility"||name=="True Seeing"?"\n非专注，持续1小时。":name=="Gentle Repose"?"\n非专注，持续10天。":name=="Mirror Image"?"\n非专注，持续1分钟。":name=="Death Ward"?"\n非专注，持续8小时。":name=="Fog Cloud"?"\n专注，至多1小时。":name=="Expeditious Retreat"?"\n专注，至多10分钟。":name=="Darkness"?"\n专注，至多10分钟。":name=="Light"||name=="Feign Death"?"\n非专注，持续1小时。":name=="Darkvision"||name=="Daylight"?"\n无需专注。":name=="Guidance"?"\n专注，至多1分钟。":name=="Enhance Ability"?"\n专注，至多1小时。":name=="Aid"?"\n非专注，持续8小时。":name=="Hex"||name=="Hunter's Mark"?"\n专注，持续时间由环阶决定。":name=="Bane"||name=="Bless"?"\n专注，至多1分钟。":name=="Shield of Faith"?"\n专注，至多10分钟。":name=="Blade Ward"?"\n专注，至多1分钟。":name=="Protection from Energy"?"\n专注，至多1小时。":control?"\n专注，至多1分钟。":name=="Divine Favor"||name=="Magic Weapon"||name=="Longstrider"||name=="Barkskin"||name=="Armor of Agathys"?"\n无需专注。":"\n非专注，即时。");
    }
    public int EmanationRadius=>Name=="Arms of Hadar"?10:Name=="Thunderclap"||Name=="Word of Radiance"?5:0;
    public int BurstRadius=>Name=="Mass Cure Wounds"?30:Name=="Fireball"?20:Name=="Shatter"?10:Name=="Circle of Death"?60:0;
    public bool AllowsCreatureType(int type)=>Name!="Hold Person"||type==9;
    public int DiceCount(int totalLevel,int slotLevel) => Name=="Scorching Ray"?1+Math.Max(2,slotLevel):Name=="Magic Missile"?2+Math.Max(1,slotLevel):Healing?(BaseDice>0?BaseDice+Math.Max(0,slotLevel-Level)*UpcastDice:2*Math.Max(1,slotLevel)):Stable?0:BaseDice>0?BaseDice+Math.Max(0,slotLevel-Level)*UpcastDice:AcidSplash.DiceCount(totalLevel);
    public int EffectiveRange(int totalLevel) => Stable?Range*(1<<(AcidSplash.DiceCount(totalLevel)-1)):Range;
    public static readonly DirectSpell[] Available={
        new DirectSpell("Heat Metal","灼热金属","1动作 · 60尺 · 声音、姿势。一个可见制造金属物件的手持或穿戴者受到2d8火焰，每升环+1d8。后续自己的回合可用附赠动作再次造成伤害，物件仍须在60尺内。受伤者体质豁免失败须放下能放下的物件；未放下则攻击与属性检定劣势至施法者下回合开始。收起武器不视为接触。",2,60,8,baseDice:2,upcastDice:1),
        new DirectSpell("Heroism","英雄气概","1动作 · 接触 · 声音、姿势。自愿生物在专注期间免疫恐慌，且各自回合开始获得等于本次施法调整值的临时生命，不叠加、不恢复实际HP。每升环增加1名目标。专注至多1分钟；结束后已有临时生命保留。",1,5,0),
        new DirectSpell("Command","命令术","1动作 · 60尺 · 声音。选择可见生物，各自感知豁免；失败者下回合执行接近、丢弃、逃跑、趴下或站住，同次施法使用同一命令。每升一环增加1名目标。接近仅普通移动后结束；逃跑仅普通移动及不消耗次数的疾走，不自动施法或消耗职业／种族次数。",1,60,0),
        new DirectSpell("Destructive Wave","湮灭波","1动作 · 自身30尺光环 · 声音。自选范围内生物，体质豁免失败承受5d6雷鸣和5d6光耀或死灵（施法时选择）并倒地；成功只受减半伤害。每类伤害分别结算抗性／免疫，升环不增加伤害。",5,0,6,save:2,halfSave:true),
        new DirectSpell("Chain Lightning","连锁闪电","1动作 · 150尺 · 声音、姿势。首目标必须可见，另选至多3名距其30尺内的不同生物；分别敏捷豁免，失败10d8闪电，成功减半。每升环只增加一个次级目标。物件目标等待环境系统。",6,150,8,save:1,damageType:"闪电",baseDice:10,halfSave:true),
        new DirectSpell("Mass Heal","群体医疗术","1动作 · 60尺 · 声音、姿势。在可见的任意数量生物间分配至多700治疗。仅实际恢复至少1HP的目标解除目盲、耳聋与中毒；满血或恢复生命受阻时不解除。",9,60,0),
        new DirectSpell("Heal","医疗术","1动作 · 60尺 · 声音、姿势。一个可见生物恢复70生命，同时解除目盲、耳聋与中毒；每升一环治疗增加10。不加属性调整值，不复活死者。",6,60,0),
        new DirectSpell("Find Steed","寻获坐骑","1动作 · 30尺 · 声音、姿势。召唤大型天界／妖精／邪魔坐骑，AC=10+环阶、HP=5+10×环阶、步行60尺。无需专注，重施替换旧坐骑，归零或施法者死亡时消失。上／下马消耗半速移动；骑乘时双方按完整坐骑占地判定，体型不再合法时正常变化并强制下马。",2,30,8),
        new DirectSpell("Stoneskin","石肤术","动作 · 接触 · 声音、姿势 · 专注1小时。一个自愿生物获得钝击、穿刺和挥砍抗性。",4,5,0),
        new DirectSpell("Water Breathing","水下呼吸","动作 · 30尺 · 声音、姿势 · 24小时。自愿目标获得水下呼吸能力，保留原呼吸方式。",3,30,0),
        new DirectSpell("Sleep","睡眠术","动作 · 60尺 · 声音、姿势 · 专注1分钟。范围内所选目标首次感知豁免失败后失能，再次失败则昏迷；受伤或被摇醒时解除。",1,60,0,control:true),
        new DirectSpell("Spike Growth","荆棘丛生","1动作 · 150尺 · 声音、姿势 · 专注至多10分钟。20尺半径地面区域成为困难地形，水面与潜水均受影响；每实际经过5尺分别受到2d4穿刺伤害。空中跳跃与传送不计算地面路程，出现或静止不受伤。地面伪装成自然环境；施法时看不见区域者须用搜索动作，以感知（察觉或求生）检定对施法DC识别危险。",2,150,0),
        new DirectSpell("Grease","油腻术","1动作 · 60尺 · 声音、姿势 · 非专注1分钟。指定10尺方形地面形成不易燃油脂和困难地形，仅陆地生效。油脂出现时、进入区域及回合末敏捷豁免失败倒地；无每回合一次限制。玩家移动中倒地停在触发格，仅扣已走路程；木桩若移动足够则花半速起身继续追击。",1,60,0),
        new DirectSpell("Entangle","纠缠术","1动作 · 90尺 · 声音、姿势 · 专注至多1分钟。指定20尺方形地面形成困难地形，水域也生效。初始区域内除施法者外的生物力量豁免，失败束缚至法术结束；可用动作进行力量（运动）检定对施法DC，成功脱困。后来进入区域不会再次束缚。完整占地进入困难地形才增加移动消耗；已束缚目标离开区域不会自动解除。",1,90,0),
        new DirectSpell("Enlarge/Reduce","变巨／缩小术","1动作 · 30尺 · 声音、姿势 · 专注1分钟。可见目标体型增减一级，不自愿目标体质豁免成功无效。变巨使力量检定与力量豁免优势，武器或徒手命中额外1d4；缩小使这些检定劣势，武器或徒手命中减1d4，不将伤害降至1以下。体型边界或占地不合法则施法前阻止。装备重量不变，超载按搬运规则限制移动。物件环境效果暂缓。",2,30,0),
        new DirectSpell("Ray of Enfeeblement","衰弱射线","1动作 · 60尺 · 声音、姿势 · 专注至多1分钟。体质豁免成功：下次攻击劣势，至施法者下回合开始。失败：力量类D20检定劣势，伤害掷骰减1d8，回合末重复体质豁免成功结束。整次命中合并伤害只抵扣一次，由玩家分类分配；分类最低0，抗性与免疫前结算，暴击不翻倍减值骰。",2,60,0),
        new DirectSpell("Protection from Poison","防护毒素","1动作 · 接触（5尺） · 声音、姿势。解除目标中毒状态，持续1小时获得毒素抗性，对避免或结束中毒状态的豁免具有优势；不会免疫中毒，重复抗性不叠加。",2,5,0),
        new DirectSpell("Jump","跳跃术","附赠动作 · 接触（5尺） · 声音、姿势。自愿目标每回合一次，可花10尺移动力跳至30尺内合法落点。平面跳跃可越过生物及水域；普通跳跃仍引发借机，战术跳跃继承免借机，两种移动池共用次数。潜水须先切换水面，拖行须先放下或改为肩扛。",1,5,0,bonus:true),
        new DirectSpell("Lesser Restoration","次级复原术","附赠动作 · 接触（5尺） · 声音、姿势。解除目标一种状态：目盲、耳聋、麻痹或中毒。清除该状态全部施法来源（含暂被压制的旧施法），保留其他状态。持续区域条件仍按所在位置重新判定；不恢复HP。",2,5,0,bonus:true),
        new DirectSpell("Blindness/Deafness","目盲／耳聋","1动作 · 120尺 · 声音。可见生物体质豁免失败，施法者选择使其目盲或耳聋。目标每回合结束重复豁免，成功结束本次施法；每升一环增加一个目标。非专注至多1分钟；同名效果最高DC优先、同DC取最新，所有施法独立计时与豁免。",2,120,0),
        new DirectSpell("Silence","沉默术","1动作 · 120尺 · 声音、姿势 · 专注至多10分钟。指定点20尺半径内不能产生声音，声音不能穿过。按单位中心禁止声音施法；完整占地在内时失聪且免疫雷鸣伤害。固定范围，不随生物移动。",2,120,0),
        new DirectSpell("Arcane Vigor","活力奥法","附赠动作 · 自身 · 声音、姿势。消耗并投掷1～2颗未使用的生命骰，恢复骰值之和加一次施法属性调整值的HP，不加体质调整值。每升一环可多用1颗生命骰；多职业可自行分配，和短休共用同一资源。",2,0,0,bonus:true),
        new DirectSpell("Thorn Whip","荆棘之鞭","1动作 · 30尺 · 声音、姿势。近战法术攻击，命中造成1d6穿刺伤害，总等级5/11/17时增至2d6/3d6/4d6。大型或更小的目标可向施法者拉近至多10尺；施法前选择距离，遇阻停下，不消耗移动力或触发借机攻击。",0,30,6,attack:true,damageType:"穿刺",melee:true),
        new DirectSpell("Detect Evil and Good","侦测善恶","1动作 · 自身 · 声音、姿势 · 专注至多10分钟。感知30尺内异怪、天界、元素、妖精、邪魔或亡灵的位置，不判断阵营、不赋予视觉。圣居术区域与遮挡材质按环境系统暂缓，现有无墙壁训练场以金色边框提示符合条件的存活木桩。",1,0,0),
        new DirectSpell("Mage Armor","法师护甲","1动作 · 接触（5尺） · 声音、姿势 · 非专注8小时。自愿且未穿甲的生物获得13+敏捷调整值的基础AC计算方式，不与其他基础AC公式相加；穿上护甲时结束，持盾不结束。",1,5,0),
        new DirectSpell("Shillelagh","橡棍术","附赠动作 · 自身 · 声音、姿势 · 非专注1分钟。附魔持有的短棒或长棍，伤害骰变为1d8，总等级5/11/17时为1d10/1d12/2d6；可改用施法属性作命中与伤害，并选择原类型或力场伤害。再次施法替换旧效果；切换武器或装备融入形态视为放手并结束，不立即攻击。",0,0,0,bonus:true),
        new DirectSpell("Mind Spike","心灵尖刺","1动作 · 120尺 · 姿势。可见生物感知豁免，失败受到3d8心灵伤害，成功减半；每升一环+1d8。失败时在同一位面内持续知晓其位置，目标不能对施法者隐藏或从隐形获益；专注至多1小时。当前无隐藏操作或位面切换，位置继续在地图显示。",2,120,8,save:3,damageType:"心灵",baseDice:3,upcastDice:1,halfSave:true),
        new DirectSpell("Harm","伤害术","1动作 · 60尺 · 声音、姿势。可见生物体质豁免，失败受到14d6死灵伤害，并按实际承受的死灵伤害削减生命上限，最低1；成功仅伤害减半。不因升环增加伤害，完成长休恢复被削减的上限。",6,60,6,save:2,damageType:"死灵",baseDice:14,halfSave:true),
        new DirectSpell("Sorcerous Burst","术法迸发","1动作 · 120尺 · 声音、姿势。远程法术攻击，命中1d8所选类型伤害，总等级5/11/17时为2/3/4d8。每个8可追加d8，追加总数最多为施法调整值；项目暴击裁定：基础骰、每次追加骰及追加上限均翻倍。",0,120,8,attack:true),
        new DirectSpell("Polymorph","变形术","1动作 · 60尺 · 声音、姿势 · 专注至多1小时。可见生物感知豁免失败后变为CR不高于目标CR或等级的野兽。保留实际生命、生命骰和生物类型，获得野兽生命值对应临时生命。临时生命耗尽时结束。当前开放10种已有攻击数据的地面形态，更多野兽待接入。",4,60,0),
        new DirectSpell("Remove Curse","移除诅咒","1动作 · 接触（5尺） · 声音、姿势。解除目标现有诅咒，当前接入脆弱诅咒；不移除猎人印记。物件诅咒、同调与其他未建模诅咒等待对应系统。",3,5,0),
        new DirectSpell("Mass Cure Wounds","群体疗伤术","1动作 · 60尺内可见点 · 声音、姿势。在该点30尺半径内选择至多6名生物，各恢复5d8+施法属性调整值生命；每升一环+1d8。目标本身不要求可见，不能复活死者。",5,60,8,healing:true,baseDice:5,upcastDice:1),
        new DirectSpell("Invisibility","隐形术","1动作 · 接触（5尺） · 声音、姿势。目标隐形；攻击检定、造成伤害或施法后立即结束该目标隐形。每升一环增加一个目标，其他目标独立保留。",2,5,4),
        new DirectSpell("Blur","朦胧术","1动作 · 自身 · 声音。对你的攻击具有劣势；能用盲视或真实视觉感知你的攻击者不受影响。",2,0,4),
        new DirectSpell("Greater Invisibility","高等隐形术","1动作 · 接触（5尺） · 声音、姿势。目标隐形；攻击、伤害或施法不会自动结束本法术。已知位置仍显示棋子。",4,5,4),
        new DirectSpell("See Invisibility","识破隐形","1动作 · 自身 · 声音、姿势。能看见隐形生物和物件，仍受光照与云雾遮挡；以太位面观察保留说明，当前无位面实体。",2,0,4),
        new DirectSpell("True Seeing","真知术","1动作 · 接触（5尺） · 声音、姿势。自愿目标获得120尺真实视觉，可识破隐形、魔法黑暗、朦胧与镜像；实体云雾仍遮挡。当前未建模的变形原貌、以太位面效果保留说明。",6,5,4),
        new DirectSpell("Resurrection","复生术","战后或外部救援 · 1小时 · 接触 · 声音、姿势。死亡100年内、非老死且死亡时非亡灵的自愿目标以满HP复活，解除毒素，恢复身体；复活者D20检定减4，每次长休减轻1。若目标死亡满365天，施法者直到完成长休前不能施法，且所有D20检定劣势。",7,5,0),
        new DirectSpell("True Resurrection","完全复生术","战后或外部救援 · 1小时 · 接触 · 声音、姿势。死亡200年以内、非老死的自愿目标满HP复活，解除毒素、魔法疾病及诅咒；亡灵恢复完整生前形态，原位置必须容纳其占地。",9,5,0),
        new DirectSpell("Revivify","回生术","战后或外部救援 · 1动作 · 接触 · 声音、姿势。死亡不超过1分钟的自愿目标复活为1HP，不修复缺失器官，也不能复活老死者。计入施法6秒。",3,5,0),
        new DirectSpell("Raise Dead","死者复活","战后或外部救援 · 1小时 · 接触 · 声音、姿势。死亡不超过10天、死亡时非亡灵且尸体完整的自愿目标复活为1HP，并解除毒素。d20检定减4，每次完成长休减轻1。",5,5,0),
        new DirectSpell("Gentle Repose","遗体防腐","战后或外部施法 · 1动作 · 接触 · 声音、姿势。尸体10天内不会腐败或变成亡灵，这段时间不计入复活法术的死亡时限。",2,5,0),
        new DirectSpell("Ice Knife","冰刃","1动作 · 60尺 · 姿势。远程法术攻击命中1d10穿刺；无论命中与否，目标及其5尺内生物敏捷豁免，失败2d6寒冷、成功0。升环每环仅爆炸伤害+1d6，暴击仅翻倍穿刺骰。",1,60,6,baseDice:2,upcastDice:1),
        new DirectSpell("Fireball","火球术","1动作 · 150尺 · 声音、姿势。指定点20尺半径内所有生物敏捷豁免，失败8d6火焰，成功减半；每升一环+1d6。点燃物件等环境效果暂缓。",3,150,6,save:1,damageType:"火焰",baseDice:8,upcastDice:1,halfSave:true),
        new DirectSpell("Shatter","粉碎音波","1动作 · 60尺 · 声音、姿势。指定点10尺半径内所有生物体质豁免，失败3d8雷鸣，成功减半；构装生物此豁免劣势。每升一环+1d8；物件破坏暂缓。",2,60,8,save:2,damageType:"雷鸣",baseDice:3,upcastDice:1,halfSave:true),
        new DirectSpell("Circle of Death","死亡法阵","1动作 · 150尺 · 声音、姿势。指定点60尺半径内所有生物体质豁免，失败8d8死灵，成功减半；每升一环+2d8。",6,150,8,save:2,damageType:"死灵",baseDice:8,upcastDice:2,halfSave:true),
        new DirectSpell("Arms of Hadar","哈达之臂","1动作 · 自身10尺光环 · 声音、姿势。力量豁免失败受2d6死灵并无法进行反应，至目标下回合开始；成功伤害减半。每升一环+1d6。",1,0,6,save:0,damageType:"死灵",baseDice:2,upcastDice:1,halfSave:true),
        new DirectSpell("Hold Monster","怪物定身术","1动作 · 90尺 · 声音、姿势。可见生物感知豁免失败则麻痹，目标每个回合结束再次豁免。五环1目标，每升一环增加1目标；2024条文不排除亡灵。",5,90,0,save:3,control:true),
        new DirectSpell("Eldritch Blast","魔能爆","1动作 · 120尺 · 声音、姿势。每束独立远程法术攻击，命中1d10力场。总等级5/11/17时变为2/3/4束，分别选择目标。",0,120,10,attack:true),
        new DirectSpell("Ray of Sickness","致病射线","1动作 · 60尺 · 声音、姿势。远程法术攻击，命中2d8毒素并中毒至施法者下一回合结束，无额外豁免；每升一环伤害+1d8。",1,60,8,attack:true,damageType:"毒素",baseDice:2,upcastDice:1),
        new DirectSpell("Mirror Image","镜像术","1动作 · 自身 · 声音、姿势。三个替身；攻击命中时每个剩余替身各掷1d6，任一骰值至少3则一个替身承受攻击并消失。盲视或无法看见本体的攻击者不受影响。",2,0,6),
        new DirectSpell("Death Ward","防死结界","1动作 · 接触（5尺） · 声音、姿势。首次将降至0生命时改为1生命（包括伤害或临时生命上限效果结束）；或抵消一次不造成伤害的直接死亡效果，然后法术结束。不会阻止已经倒地后的死亡豁免失败。",4,5,4),
        new DirectSpell("Misty Step","迷踪步","附赠动作 · 自身 · 声音。传送至30尺内可见且未被占据的地面位置，不消耗移动力、不引发借机攻击；仍须容纳完整占地。",2,0,4,bonus:true),
        new DirectSpell("Reincarnate","转生术","1小时 · 接触死亡不超过10天的类人生物 · 声音、姿势。投1d10确定新种族，填写种族选择，在旧遗体旁5尺内生成新身体；沿用体重并回满新HP，旧体与装备保留。个别种族特性仍按待接入状态处理。",5,5,0),
        new DirectSpell("Burning Hands","燃烧之手","1魔法动作 · 自身15尺锥形 · 声音、姿势。范围内每名生物进行敏捷豁免，失败受到3d6火焰伤害，成功减半向下取整。每升1环增加1d6；共享伤害骰，不附加属性调整值，不进行攻击检定。环境可燃物点燃保留说明，暂缓环境结算。",1,15,6,save:1,damageType:"火焰",baseDice:3,upcastDice:1,halfSave:true),
        new DirectSpell("Cloud of Daggers","匕首之云","1魔法动作 · 60尺 · 声音、姿势。创造5尺立方匕首区域，专注至多1分钟。出现、进入、回合末或区域传送入体时造成4d4挥砍，每名生物每回合最多一次；每升环+2d4。后续自己的回合可用魔法动作将区域传送30尺，只结算落点；包括自身与盟友，无豁免或攻击检定。",2,60,4,damageType:"挥砍",baseDice:4,upcastDice:2),
        new DirectSpell("Web","蛛网术","1魔法动作 · 60尺 · 声音、姿势。20尺立方区域在平面地图仅陆地格铺网，形成困难地形及轻度遮蔽，专注至多1小时。每回合首次进入或自己回合开始时敏捷豁免，失败在蛛网内束缚；动作力量（运动）对施法DC脱困。蛛网遇火1回合烧毁，火中回合开始受到2d4火焰伤害。",2,60,0),
        new DirectSpell("Color Spray","七彩喷射","1动作 · 自身15尺锥形 · 声音、姿势。范围内每名生物进行体质豁免，失败后目盲至施法者下个回合结束；不造成伤害，无需专注，无后续重复豁免。包括自身与盟友，使用格心范围判定。升环不增强效果。",1,0,0),
        new DirectSpell("Starry Wisp","点点星芒","1动作 · 60尺 · 声音、姿势。远程法术攻击命中造成1d8光耀，角色总等级5／11／17时为2／3／4d8。命中者发出10尺微光，至施法者下个回合结束不能获得隐形收益；不主动终止原隐形法术。物件攻击等待环境物件受损系统。",0,60,8,attack:true,damageType:"光耀"),
        new DirectSpell("Feign Death","假死术","1魔法动作 · 接触 · 声音、姿势。自愿生物外观与检测生死时视为已死，实际仍存活；目盲、失能、速度0，除心灵外所有伤害抗性，中毒状态免疫。木桩忽略假死目标，无其他敌人时跳过行动；战后可搬运。非专注1小时，不获得麻痹、昏迷或自动暴击效果。",3,5,0),
        new DirectSpell("Light","光亮术","1魔法动作 · 接触（5尺） · 声音。触碰大型或更小、未由其他角色穿戴或携带的物件。本阶段支持场内武器、护甲与盾牌，光源随装备移动：20尺明亮，外围再20尺微光。颜色可选；不透明遮盖阻光，再次施放结束原效果。装备融入期间暂时阻光，复原后恢复剩余效果。",0,5,0),
        new DirectSpell("Dancing Lights","舞光术","1魔法动作 · 120尺 · 声音、姿势。创造1～4个光球、火把或灯笼光影，或将四光点合成一个中型人形；每个光源提供10尺微光。附赠动作可各移动至多60尺，仍须在施法者120尺内；多个独立光点各须有20尺内邻点，单个可独立存在。光影不占格，不作为生物。",0,120,0),
        new DirectSpell("Faerie Fire","妖火","动作 · 60尺 · 声音。20尺立方内所有生物敏捷豁免，失败者发出10尺微光、不能获得隐形收益；看见被勾勒目标的攻击者具有优势。离开初始区域不结束效果。环境物件效果暂缓。",1,60,0),
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
        new DirectSpell("Vicious Mockery","恶言相加","1动作 · 60尺 · 声音。选择能看见或听见的生物，目标感知豁免失败受到1d6心灵伤害，且目标在其下一回合结束前的下一次攻击具有劣势。总等级5/11/17时增加1d6。",0,60,6,save:3,damageType:"心灵"),
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
