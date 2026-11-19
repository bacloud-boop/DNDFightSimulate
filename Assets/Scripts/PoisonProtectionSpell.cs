public sealed partial class TrainingGame
{
    readonly PoisonProtectionEffect poisonProtection=new PoisonProtectionEffect();
    void InitializePoisonProtection(){combat.DamageResistance=(player,type)=>(type==9&&poisonProtection.Active(player,worldRounds))||StoneskinResists(player,type);combat.PoisonConditionSaveAdvantage=player=>poisonProtection.Active(player,worldRounds);}
    string PoisonProtectionError(bool player)
    {
        if(player?combat.PlayerDead:combat.Hp==0)return "死亡目标不能接受防护毒素。";
        if(!player&&map.DistanceFeet>5)return "防护毒素需要接触：请接近至5尺。";
        return worldRounds<0||worldRounds>long.MaxValue-600?"世界时间超出可记录范围。":"";
    }
    void ApplyPoisonProtection(bool player)
    {
        combat.ClearPoison(player);poisonProtection.Grant(player,worldRounds);
        history.Insert(0,"防护毒素："+(player?"玩家":"木桩")+"解除中毒，1小时内获得毒素抗性，并对避免或结束中毒状态的豁免具有优势。不会免疫后续无豁免附加的中毒，也不与其他同类型抗性重复减半。");
    }
}
