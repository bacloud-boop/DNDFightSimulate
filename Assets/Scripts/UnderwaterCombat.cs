public sealed partial class TrainingGame
{
    int PlayerSwimSpeed=>PlayerInBeastForm?PlayerBeast.swim:0;
    int MonsterSwimSpeed=>MonsterPolymorphed?PolymorphBeast.swim:0;
    bool PlayerUnderwaterWeaponDisadvantage(Weapon weapon)=>UnderwaterCombatRules.Disadvantage(map.PlayerAquatic.Submerged,true,weapon.IsMelee,PlayerSwimSpeed,weapon.DamageType=="穿刺");
    bool MonsterUnderwaterDisadvantage()
    {
        bool piercing=false;
        if(MonsterPolymorphed)piercing=BeastAttack.Find(polymorph.Form).DamageType=="穿刺";
        else {for(int i=0;i<monster.Counts.Length;i++)if(monster.Counts[i]>0&&monster.Types[i]==DamageTypes.Index("穿刺"))piercing=true;if(monster.Bonus>0&&monster.Types[5]==DamageTypes.Index("穿刺"))piercing=true;}
        return UnderwaterCombatRules.Disadvantage(map.MonsterAquatic.Submerged,MonsterPolymorphed||combat.MonsterAttackIsWeapon,MonsterMeleeAttack,MonsterSwimSpeed,piercing);
    }
}
