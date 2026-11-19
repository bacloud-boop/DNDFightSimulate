public sealed partial class TrainingGame
{
    readonly MageArmorEffect mageArmor=new MageArmorEffect();
    bool MageArmorTargetWearing(bool player)=>player?playerArmor.WearingArmor&&!PlayerArmorMelded:monsterArmor.WearingArmor&&!MonsterPolymorphed;
    string MageArmorError(bool player)=>MageArmorTargetWearing(player)?"法师护甲只能作用于未穿着护甲的目标。":!player&&!energyTargetWilling?"请确认木桩自愿接受法师护甲。":worldRounds>long.MaxValue-4800?"模拟时间超出范围。":"";
    void ApplyMageArmor(bool player){mageArmor.Grant(player,worldRounds);SyncArmorEquipment();history.Insert(0,"法师护甲："+(player?"玩家":"木桩")+"获得13+敏捷调整值的基础AC计算方式，持续8小时，不与其他基础AC公式相加；持盾本身不结束效果。");}
}
