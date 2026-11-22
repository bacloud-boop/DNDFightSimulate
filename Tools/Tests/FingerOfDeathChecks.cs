using System;
using UnityEngine;
class FingerOfDeathChecks
{
    static void C(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        C(FingerOfDeathRaising.Damage(7,false)==37&&FingerOfDeathRaising.Damage(56,false)==86&&FingerOfDeathRaising.Damage(7,true)==18,"7d8+30 and successful save rounds down");
        C(FingerOfDeathRaising.Damage(int.MaxValue,false)==int.MaxValue,"damage helper cannot overflow into healing");
        var queue=new FingerOfDeathRaising();var corpse=new CorpseClock();
        corpse.Observe(false,9,0);C(!queue.Schedule(-1,corpse),"0HP without actual death does not qualify");
        corpse.Observe(true,13,0);C(!queue.Schedule(-1,corpse),"non-humanoid cannot rise");
        corpse.Observe(false,9,0);corpse.Observe(true,9,0);
        C(queue.Schedule(-1,corpse)&&!queue.Schedule(-1,corpse),"one schedule per actual humanoid death");
        corpse.Age(500);C(queue.CasterStarts==0,"world time does not impersonate caster turn starts");
        var due=queue.StartTurn();C(due.Count==1&&due[0].Due==1&&FingerOfDeathRaising.SameCorpse(due[0],corpse)&&queue.StartTurn().Count==0,"next caster turn, once only");
        corpse.Observe(false,9,500);C(!FingerOfDeathRaising.SameCorpse(due[0],corpse),"revival cancels queued conversion");
        corpse.Observe(true,9,500);C(!FingerOfDeathRaising.SameCorpse(due[0],corpse),"same-time re-death is a different corpse generation");
        C(queue.Schedule(-1,corpse),"new death may be scheduled independently");corpse.Preserve(500);C(corpse.Preserved(501),"Gentle Repose can block conversion before due turn");
        queue.Clear();C(queue.CasterStarts==0&&queue.StartTurn().Count==0,"reset removes pending conversions");
        var control=new AnimateDeadControl();C(control.RegisterPermanent(1,0)&&control.Controlled(1,long.MaxValue),"permanent verbal control never reaches a 24h deadline");
        int paid=0;C(!control.Reassert(new[]{1},3,0,id=>true)&&!control.Command(new[]{1},1,0,id=>true,()=>{paid++;return true;})&&paid==0,"Animate Dead cannot renew or mentally command Finger zombies");
        var zombie=new AnimatedUndeadRules(false).Create(1);zombie.ControllerAllows=()=>control.Controlled(1,long.MaxValue);bool changed;
        C(zombie.ReceiveVerbalOrders(SummonCommand.AssistCombat,out changed)&&changed&&zombie.BeginTurn(1)&&zombie.Attack(),"permanent follower accepts free verbal orders and owns its action");
        C(zombie.ReceiveVerbalOrders(SummonCommand.AssistCombat,out changed)&&!changed,"continuing broad order does not need more speech");
        C(control.Register(new[]{2},3,0)&&!control.Controlled(2,14400)&&control.Reassert(new[]{2},3,14400,id=>true),"ordinary Animate Dead keeps 24-hour renewal rule");
        var map=new BattleMap();map.Place(false,new Vector2Int(1,0));C(map.Lock()&&map.BeginContainment(false),"swallowed corpse fixture");
        int releases=0;map.ContainmentEnded=host=>{C(host,"player was swallowing host");releases++;};
        var risen=new AnimatedUndeadRules(false).Create(4);C(!map.CanAnimateBody(risen,-1),"contained corpse cannot directly spawn an external zombie");
        var plans=map.MandatoryReleasePlans();C(plans.Count>0&&map.IsContained(false)&&releases==0,"release planning leaves corpse and equipment unchanged");
        C(map.ApplyMandatoryReleasePlan(plans[0])&&!map.IsContained(false)&&releases==1,"mandatory release commits and releases internal equipment exactly once");
        C(map.AnimateBody(risen,-1)&&!map.BodyPresent(-1)&&map.Summons.Count==1&&!map.AnimateBody(new AnimatedUndeadRules(false).Create(5),-1),"released corpse becomes one independent zombie and cannot be consumed twice");
        Console.WriteLine("PASS: Finger of Death damage, actual humanoid death, next-turn queue, revival generation, preservation, permanent verbal control and forced release before conversion");
    }
}
