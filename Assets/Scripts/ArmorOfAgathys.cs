using UnityEngine;
public sealed partial class TrainingGame
{
    readonly AgathysEffect agathys=new AgathysEffect();
    int AgathysRetaliation(bool melee)=>agathys.Retaliation(worldRounds,combat.PlayerTemporaryHp,melee);
    int ApplyAgathysRetaliation(int damage,bool attackerPlayer)
    {
        if(damage<=0)return 0;
        int result=combat.AdjustTypedDamage(attackerPlayer,damage,4);
        if(attackerPlayer)combat.ApplyPlayerDamage(result,sourcePlayer:true);else combat.ApplyDamage(result);
        history.Insert(0,"黯冰狱铠：近战攻击命中触发，对"+(attackerPlayer?"施法者自身":"木桩")+"造成"+result+"寒冷伤害（原值"+damage+"），不需要反应，不进行攻击或豁免检定。");return result;
    }
}
