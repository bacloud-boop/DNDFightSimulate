using System.Collections.Generic;
using UnityEngine;

public sealed partial class BattleMap
{
    public readonly RemainsArchive Remains=new RemainsArchive();
    RetainedRemains pendingRemains;
    IEnumerable<RetainedRemains> AllRemains(){foreach(var body in Remains.Bodies)yield return body;if(pendingRemains!=null)yield return pendingRemains;}
    public int RemainsSpaces(RetainedRemains body)=>!OnScene(body.Id)||RescueCargoId==body.Id&&!RescueCargoDragging?0:body.Spaces;
    bool RemainsFootprintClear(Vector2Int anchor,int spaces,int exceptId=0)
    {
        foreach(var body in AllRemains())if(body.Id!=exceptId&&Overlap(anchor,spaces,new Vector2Int(body.X,body.Y),RemainsSpaces(body)))return false;
        return true;
    }
    bool CrossesRemains(Vector2Int from,Vector2Int to,int spaces,int exceptId=0)
    {
        foreach(var body in AllRemains())if(body.Id!=exceptId&&!Overlap(from,spaces,new Vector2Int(body.X,body.Y),RemainsSpaces(body))&&SegmentCrossesBody(from,to,spaces,new Vector2Int(body.X,body.Y),RemainsSpaces(body)))return true;
        return false;
    }
    void SetReincarnationBody(int actor,Vector2Int anchor,int spaces){if(actor==0){Player=anchor;PlayerSpaces=spaces;}else if(actor==-1){Monster=anchor;MonsterSpaces=spaces;}else{RescuePosition=anchor;RescueSpaces=spaces;}}
    public bool CanReincarnateAt(bool player,Vector2Int anchor,int spaces)=>CanReincarnateAt(player?0:-1,anchor,spaces);
    public bool CanReincarnateAt(int actor,Vector2Int anchor,int spaces)
    {
        Vector2Int old;int oldSpaces;if((actor!=0&&actor!=-1&&actor!=RescueId)||!CargoBody(actor,out old,out oldSpaces))return false;
        if(!Locked||!Remains.CanAdd||spaces<1||spaces>4||(actor==0||actor==-1)&&IsContained(actor==0)||RescueCargoId==actor||actor==0&&Riding||!Inside(anchor,spaces)||BodyDistance(old,oldSpaces,anchor,spaces)>5||Overlap(old,oldSpaces,anchor,spaces)||!SummonFootprintClear(anchor,spaces,actor==RescueId?RescueId:0))return false;
        if(actor!=0&&Overlap(anchor,spaces,Player,OccupiedSpaces(true))||actor!=-1&&Overlap(anchor,spaces,Monster,OccupiedSpaces(false)))return false;
        int recovery=actor==0?playerRecoverySpaces:actor==-1?monsterRecoverySpaces:0;
        pendingRemains=new RetainedRemains(Remains.NextId,actor,old.x,old.y,oldSpaces,"待保留躯体",new BodyWeight(),0,false);
        try{SetReincarnationBody(actor,anchor,spaces);if(actor==0)playerRecoverySpaces=0;else if(actor==-1)monsterRecoverySpaces=0;return RecoverySafe(Player,Monster);}
        finally{SetReincarnationBody(actor,old,oldSpaces);if(actor==0)playerRecoverySpaces=recovery;else if(actor==-1)monsterRecoverySpaces=recovery;pendingRemains=null;}
    }
    public RetainedRemains ReincarnateAt(bool player,Vector2Int anchor,int spaces,string name,BodyWeight former,int armorIndex,bool shield)=>ReincarnateAt(player?0:-1,anchor,spaces,name,former,armorIndex,shield);
    public RetainedRemains ReincarnateAt(int actor,Vector2Int anchor,int spaces,string name,BodyWeight former,int armorIndex,bool shield)
    {
        if(former==null||!former.Known||!CanReincarnateAt(actor,anchor,spaces))return null;Vector2Int old;int oldSpaces;CargoBody(actor,out old,out oldSpaces);
        var body=Remains.Add(actor,old.x,old.y,oldSpaces,name,former,armorIndex,shield);if(body==null)return null;
        SetReincarnationBody(actor,anchor,spaces);if(actor==0)playerRecoverySpaces=0;else if(actor==-1)monsterRecoverySpaces=0;
        SyncWaterPositions();MovementObserved?.Invoke(actor,old,anchor,oldSpaces,spaces,true);return body;
    }
}
