public sealed partial class TrainingGame
{
    readonly DeathWardEffect deathWard=new DeathWardEffect();
    void InitializeDeathWard(){combat.DeathWardGuard=player=>{
        if(!deathWard.Consume(player,worldRounds))return false;
        history.Insert(0,"防死结界触发："+(player?"玩家":"木桩")+"抵消本次降至0生命或无伤害直接死亡的效果，结界结束。");return true;
    };}
    void ApplyDeathWard(bool player){deathWard.Cast(player,worldRounds);dice="防死结界 · 已生效";history.Insert(0,(player?"玩家":"木桩")+"获得防死结界，持续8小时、无需专注；再次施放只刷新持续时间，不叠加次数。");}
}
