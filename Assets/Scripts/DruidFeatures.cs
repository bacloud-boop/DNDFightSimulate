public sealed class DruidFeatures
{
    public int WildShapesUsed {get;private set;}
    public static int Maximum(int level) => level<2?0:level>=17?4:level>=6?3:2;
    public static int KnownFormLimit(int level) => level<2?0:level>=8?8:level>=4?6:4;
    public int Remaining(int level) => System.Math.Max(0,Maximum(level)-WildShapesUsed);
    public bool TryUse(int level){if(Remaining(level)<=0)return false;WildShapesUsed++;return true;}
    public bool TryBeginWildShape(int level,CombatState combat,BattleMap map,int spaces,out UnityEngine.Vector2Int anchor,UnityEngine.Vector2Int? referenceCell=null)
    {
        anchor=map.Player;
        if(!map.Locked||!combat.PlayerCanAct||!combat.BonusActionAvailable||Remaining(level)<=0||!map.FindPlayerResizePlacement(spaces,out anchor,referenceCell))return false;
        if(!combat.TryUseBonusAction())return false;
        TryUse(level);map.TryResizePlayerAt(spaces,anchor);return true;
    }
    public void ShortRest(){WildShapesUsed=System.Math.Max(0,WildShapesUsed-1);}
    public void LongRest(){WildShapesUsed=0;}
    public static long DurationRounds(int level) => (long)level*300;
}
