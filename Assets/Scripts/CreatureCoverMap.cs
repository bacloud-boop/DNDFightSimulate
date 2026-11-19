using System.Collections.Generic;
using UnityEngine;

public sealed partial class BattleMap
{
    bool CoverBody(int id,out CreatureCover.Body body)
    {
        if(id==0){body=new CreatureCover.Body(Player.x,Player.y,PlayerSpaces);return !IsContained(true)&&!IsCarried(true);}
        if(id==-1){body=new CreatureCover.Body(Monster.x,Monster.y,MonsterSpaces);return !IsContained(false)&&!IsCarried(false);}
        if(id==RescueId&&RescuePosition.HasValue){var p=RescuePosition.Value;body=new CreatureCover.Body(p.x,p.y,RescueSpaces);return true;}
        foreach(var unit in summons)if(unit.Id==id&&unit.Alive){body=new CreatureCover.Body(unit.X,unit.Y,unit.Spaces);return true;}
        var old=Remains.Find(id);if(old!=null&&RemainsSpaces(old)>0){body=new CreatureCover.Body(old.X,old.Y,old.Spaces);return true;}
        body=default(CreatureCover.Body);return false;
    }
    List<CreatureCover.Body> CoverObstacles(int sourceId,int targetId)
    {
        var bodies=new List<CreatureCover.Body>();CreatureCover.Body body;
        // Primary bodies persist after death. Grounded and dragged corpses keep
        // their full footprint; CoverBody excludes bodies lifted by a carrier.
        if(sourceId!=0&&targetId!=0&&sourceId!=riderMountId&&targetId!=riderMountId&&CoverBody(0,out body))bodies.Add(body);
        if(sourceId!=-1&&targetId!=-1&&CoverBody(-1,out body))bodies.Add(body);
        if(sourceId!=RescueId&&targetId!=RescueId&&CoverBody(RescueId,out body))bodies.Add(body);
        foreach(var unit in summons)if(unit.Alive&&unit.Id!=sourceId&&unit.Id!=targetId&&!(unit.Id==riderMountId&&(sourceId==0||targetId==0)))bodies.Add(new CreatureCover.Body(unit.X,unit.Y,unit.Spaces));
        foreach(var old in AllRemains())if(old.Id!=sourceId&&old.Id!=targetId&&RemainsSpaces(old)>0)bodies.Add(new CreatureCover.Body(old.X,old.Y,old.Spaces));
        return bodies;
    }
    public int CreatureCoverBonus(int sourceId,int targetId)
    {
        CreatureCover.Body source,target;
        if(sourceId==targetId||!CoverBody(sourceId,out source)||!CoverBody(targetId,out target))return 0;
        return CreatureCover.Bonus(source,target,CoverObstacles(sourceId,targetId));
    }
    public int CreatureCoverAt(int sourceId,int targetId,CreatureCover.Body source,CreatureCover.Body target)
        =>sourceId==targetId?0:CreatureCover.Bonus(source,target,CoverObstacles(sourceId,targetId));
    public int CreatureCoverFromPoint(Vector2Int point,int targetId,int originSpaces=1)
    {
        CreatureCover.Body target;if(!CoverBody(targetId,out target))return 0;
        // A zero-size body represents the effect's origin, not a creature.
        return CreatureCover.Bonus(new CreatureCover.Body(point.x+originSpaces*.5,point.y+originSpaces*.5,0),target,CoverObstacles(int.MaxValue,targetId));
    }
}
