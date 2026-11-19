using UnityEngine;
public sealed partial class TrainingGame
{
    int? playerAttackCoverOverride,monsterAttackCoverOverride;
    int AttackCover(bool attackerPlayer,bool targetPlayer)
    {
        if(attackerPlayer==targetPlayer)return 0;
        int? snapshot=attackerPlayer?playerAttackCoverOverride:monsterAttackCoverOverride;
        return snapshot??map.CreatureCoverBonus(attackerPlayer?0:-1,targetPlayer?0:-1);
    }
    void SnapshotOpportunityCover(bool movingPlayer,Vector2Int from,Vector2Int to,int reach)
    {
        var defender=movingPlayer?map.Monster:map.Player;
        int moverSize=movingPlayer?map.PlayerSpaces:map.MonsterSpaces,defenderSize=movingPlayer?map.MonsterSpaces:map.PlayerSpaces;
        double t=VisionState.ReachExit(from.x,from.y,to.x,to.y,moverSize,defender.x,defender.y,defenderSize,reach);
        var mover=new CreatureCover.Body(from.x+(to.x-from.x)*t,from.y+(to.y-from.y)*t,moverSize);
        var attacker=new CreatureCover.Body(defender.x,defender.y,defenderSize);
        int cover=map.CreatureCoverAt(movingPlayer?-1:0,movingPlayer?0:-1,attacker,mover);
        if(movingPlayer)monsterAttackCoverOverride=cover;else playerAttackCoverOverride=cover;
    }
    int AttackArmor(bool attackerPlayer,bool targetPlayer)=>(targetPlayer?combat.PlayerArmorClass:combat.MonsterArmorClass)+AttackCover(attackerPlayer,targetPlayer);
    void LogAttackCover(bool attackerPlayer,bool targetPlayer){if(AttackCover(attackerPlayer,targetPlayer)>0)history.Insert(0,(targetPlayer?"玩家":"木桩")+"获得生物半掩护，本次攻击对抗的护甲等级+2（不叠加）。");}
    int DexterityCover(bool targetPlayer,Vector2Int? origin=null,int originSpaces=1)=>origin.HasValue?map.CreatureCoverFromPoint(origin.Value,targetPlayer?0:-1,originSpaces):AttackCover(true,targetPlayer);
}
