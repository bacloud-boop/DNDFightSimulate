using System;
using UnityEngine;
class RetainedRemainsMapChecks
{
    static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
    static void C(bool ok,string m){if(!ok)throw new Exception(m);}
    static void Main()
    {
        var m=new BattleMap();m.Lock();C(m.CanReincarnateAt(true,V(1,0),1)&&m.Player==V(0,0)&&m.Remains.Bodies.Count==0,"preview is atomic and does not leave a phantom body");
        C(!m.CanReincarnateAt(true,V(0,0),1)&&!m.CanReincarnateAt(true,V(1,1),1),"new body cannot overlap the old body or exceed five-foot spacing");
        var body=m.ReincarnateAt(true,V(1,0),1,"旧躯体",new BodyWeight(150,10),0,false);
        C(body!=null&&body.X==0&&body.Y==0&&m.Player==V(1,0),"reincarnation separates live actor and independent remains");
        C(!m.CanMoveTo(V(0,0))&&!m.CanPlaceSummon(V(0,0),1),"grounded old body blocks placement without moving the new actor");
        C(m.DeployRescue(V(0,1),1,true)&&m.AttachRescueRemains(body.Id,true),"rescuer can attach an archived body for dragging");
        C(m.MoveRescue(V(0,2),cost=>true)&&body.X==0&&body.Y==1&&m.Player==V(1,0),"dragged old body follows rescuer while the living character stays put");
        C(m.DropRescueCorpse(V(0,1))&&m.AttachRescueRemains(body.Id,false)&&m.RemainsSpaces(body)==0,"shouldered archived body does not separately occupy floor");
        C(!m.RemoveRescue()&&m.MoveRescue(V(0,3),cost=>true)&&body.Y==3,"carried archived body cannot disappear with rescuer and follows movement");
        C(m.ForceReleaseRescueCargo()&&!m.RescueCargoId.HasValue&&m.RemainsSpaces(body)==1,"loss of consciousness can force-release archived cargo at a legal location");
        var cover=new BattleMap();cover.Place(false,V(0,4));cover.Lock();var obstacle=cover.Remains.Add(0,0,2,1,"旧躯体",new BodyWeight(),0,false);
        C(cover.CreatureCoverBonus(0,-1)==2,"grounded independent remains provide the existing precise half-cover rule");
        C(cover.DeployRescue(V(1,2),1,true)==false,"interior deployment remains forbidden");
        var rescue=new BattleMap();rescue.Lock();C(rescue.DeployRescue(V(0,4),1,true),"rescue body fixture");
        C(!rescue.CanAttachRescueRemains(BattleMap.RescueId),"rescuer cannot attach itself as its own cargo");
        C(rescue.CanReincarnateAt(BattleMap.RescueId,V(1,4),1)&&rescue.RescuePosition.Value==V(0,4),"rescue reincarnation preview preserves original position");
        var rescueOld=rescue.ReincarnateAt(BattleMap.RescueId,V(1,4),1,"救援者旧躯体",new BodyWeight(),0,false);
        C(rescueOld!=null&&rescueOld.Owner==BattleMap.RescueId&&rescueOld.Id!=BattleMap.RescueId&&rescue.RescuePosition.Value==V(1,4)&&rescueOld.X==0&&rescueOld.Y==4,"rescue identity remains current while its former body becomes a distinct object");
        Console.WriteLine("PASS: reincarnation geometry, atomic preview, independent occupancy, archived corpse cargo, forced release and cover");
    }
}
