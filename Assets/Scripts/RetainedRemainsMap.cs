using System.Collections.Generic;
using UnityEngine;

public sealed partial class BattleMap
{
    public readonly RemainsArchive Remains=new RemainsArchive();
    RetainedRemains pendingRemains;
    IEnumerable<RetainedRemains> AllRemains(){foreach(var body in Remains.Bodies)yield return body;if(pendingRemains!=null)yield return pendingRemains;}
    public int RemainsSpaces(RetainedRemains body)=>RescueCargoId==body.Id&&!RescueCargoDragging?0:body.Spaces;
    bool RemainsFootprintClear(Vector2Int anchor,int spaces,int exceptId=0)
    {
        foreach(var body in AllRemains())if(body.Id!=exceptId&&Overlap(anchor,spaces,new Vector2Int(body.X,body.Y),RemainsSpaces(body)))return false;
        return true;
    }
    bool CrossesRemains(Vector2Int from,Vector2Int to,int spaces,int exceptId=0)
    {
        foreach(var body in AllRemains())if(body.Id!=exceptId&&SegmentCrossesBody(from,to,spaces,new Vector2Int(body.X,body.Y),RemainsSpaces(body)))return true;
        return false;
    }
    public bool CanReincarnateAt(bool player,Vector2Int anchor,int spaces)
    {
        var old=player?Player:Monster;int oldSpaces=player?PlayerSpaces:MonsterSpaces;
        if(!Locked||!Remains.CanAdd||spaces<1||spaces>4||IsContained(player)||RescueCargoId==(player?0:-1)||player&&Riding||!Inside(anchor,spaces)||BodyDistance(old,oldSpaces,anchor,spaces)>5||Overlap(old,oldSpaces,anchor,spaces)||!SummonFootprintClear(anchor,spaces)||Overlap(anchor,spaces,player?Monster:Player,OccupiedSpaces(!player)))return false;
        int recovery=player?playerRecoverySpaces:monsterRecoverySpaces;
        pendingRemains=new RetainedRemains(Remains.NextId,player?0:-1,old.x,old.y,oldSpaces,"待保留躯体",new BodyWeight(),0,false);
        try{
            if(player){Player=anchor;PlayerSpaces=spaces;playerRecoverySpaces=0;}else{Monster=anchor;MonsterSpaces=spaces;monsterRecoverySpaces=0;}
            return RecoverySafe(Player,Monster);
        }finally{if(player){Player=old;PlayerSpaces=oldSpaces;playerRecoverySpaces=recovery;}else{Monster=old;MonsterSpaces=oldSpaces;monsterRecoverySpaces=recovery;}pendingRemains=null;}
    }
    public RetainedRemains ReincarnateAt(bool player,Vector2Int anchor,int spaces,string name,BodyWeight former,int armorIndex,bool shield)
    {
        if(former==null||!former.Known||!CanReincarnateAt(player,anchor,spaces))return null;
        var old=player?Player:Monster;int oldSpaces=player?PlayerSpaces:MonsterSpaces;
        var body=Remains.Add(player?0:-1,old.x,old.y,oldSpaces,name,former,armorIndex,shield);if(body==null)return null;
        if(player){Player=anchor;PlayerSpaces=spaces;playerRecoverySpaces=0;}else{Monster=anchor;MonsterSpaces=spaces;monsterRecoverySpaces=0;}
        SyncWaterPositions();MovementObserved?.Invoke(player?0:-1,old,anchor,oldSpaces,spaces,true);return body;
    }
}
