using UnityEngine;
public sealed partial class TrainingGame
{
    readonly AidEffects aidEffects=new AidEffects();
    void ApplyAid(bool player,int slot,bool both)
    {
        aidEffects.Add(player,slot,worldRounds);if(both)aidEffects.Add(!player,slot,worldRounds);
        RefreshAidEffects();history.Insert(0,"援助术："+(both?"双方":player?"玩家":"木桩")+"当前HP与上限增加至多"+(5*(slot-1))+"，同名取较强效果，不重复增加；持续8小时。");dice="援助术 · 已生效";
    }
    void RefreshAidEffects()
    {
        int player=aidEffects.ActiveBonus(true,worldRounds),monster=aidEffects.ActiveBonus(false,worldRounds);
        bool changed=player!=combat.PlayerAidBonus||monster!=combat.MonsterAidBonus;
        if(!changed)return;
        int oldPlayer=combat.PlayerAidBonus,oldMonster=combat.MonsterAidBonus;
        combat.SetAidBonus(true,player);combat.SetAidBonus(false,monster);
        if(player<oldPlayer||monster<oldMonster){history.Insert(0,"援助术效果到期/减弱：恢复对应HP上限并扣回实际增加的当前HP，最低0。");RefreshConcentration();RefreshWildShape();CheckBattleOutcome();}
    }
}
