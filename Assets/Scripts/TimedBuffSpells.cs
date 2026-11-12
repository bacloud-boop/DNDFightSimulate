using UnityEngine;
public sealed partial class TrainingGame
{
    readonly TimedSpellBuffs timedBuffs=new TimedSpellBuffs();
    static bool IsTimedBuff(string name)=>name=="Divine Favor"||name=="Magic Weapon"||name=="Longstrider"||name=="Barkskin";
    void SyncTimedBuffs(){combat.PlayerArmorFloor=timedBuffs.ArmorFloor(true,worldRounds);combat.MonsterArmorFloor=timedBuffs.ArmorFloor(false,worldRounds);combat.MonsterSpeedBonus=timedBuffs.SpeedBonus(false,worldRounds);map.SetPlayerSpeed(Mathf.Max(0,(IsWildShaped?wildForm.walk:30)+timedBuffs.SpeedBonus(true,worldRounds)-(combat.PlayerSlowed?10:0)));}
    void ApplyTimedBuff(DirectSpell spell,bool player,int slot)
    {
        if(spell.Name=="Divine Favor"){timedBuffs.DivineFavor(worldRounds);history.Insert(0,"神恩：武器命中额外1d4光耀，持续1分钟，无需专注。");}
        else if(spell.Name=="Magic Weapon"){timedBuffs.MagicWeapon(selectedWeapon.Name,slot,worldRounds);history.Insert(0,"魔化武器："+selectedWeapon.Name+" 命中/伤害 +"+timedBuffs.WeaponBonus(selectedWeapon.Name,worldRounds)+"，持续1小时；原先的魔化武器效果结束。");}
        else if(spell.Name=="Barkskin"){timedBuffs.Barkskin(player,worldRounds);history.Insert(0,"树肤术："+(player?"玩家":"木桩")+"的最终AC至少17，持续1小时，无需专注。");}
        else {timedBuffs.Longstrider(player,worldRounds);if(directBothTargets)timedBuffs.Longstrider(!player,worldRounds);history.Insert(0,"大步奔行："+(directBothTargets?"双方":player?"玩家":"木桩")+"速度+10尺，持续1小时，不叠加。");}
        SyncTimedBuffs();dice=spell.Chinese+" · 效果已生效";
    }
}
