using System;
class AftermathTurnChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var phase=new AftermathTurns();var player=new CombatState{Aftermath=true};var rescue=new RescueCarrier();long world=12;
        C(player.TryUseAftermathMagicAction()&&player.ActionsRemaining==0,"entering aftermath retains already spent actions");phase.Enter();
        C(phase.PlayerTurn&&!phase.RescueTurn&&!rescue.TurnActive,"deployed rescuer receives no early turn");
        C(!phase.Finish(true)&&phase.RescueTurn&&world==12,"ending the player turn switches actor without advancing time");player.AftermathPlayerTurn=phase.PlayerTurn;
        phase.Enter();C(phase.RescueTurn,"repeated scene observation cannot reset the active phase");
        C(!player.TryUseObjectAction()&&!player.TryUseAftermathBonusAction()&&!player.AftermathMagicActionAvailable,"player actions and bonus actions are locked during rescue");
        C(rescue.BeginTurn(player.Round)&&rescue.UseAction()&&!rescue.UseAction()&&rescue.Move(5),"rescuer refreshes exactly once at its own turn start");
        C(!rescue.BeginTurn(player.Round),"reopening the rescue panel cannot refresh a spent turn");rescue.EndTurn();
        C(phase.Finish(true)&&phase.PlayerTurn,"rescue ending completes a shared round");world++;player.AftermathPlayerTurn=true;player.EndRound();
        C(world==13&&player.AftermathMagicActionAvailable&&player.ActionsRemaining==1&&!rescue.TurnActive&&!rescue.UseAction()&&!rescue.Move(5),"six seconds advances once; player refreshes while rescue remains inactive");
        C(!phase.Finish(true)&&rescue.BeginTurn(player.Round)&&rescue.ActionsRemaining==1&&rescue.MovementSpent==0,"the next rescue phase refreshes its resources without restoring player actions again");
        rescue.EndTurn();C(phase.Finish(false)&&phase.PlayerTurn,"rescuer removal cannot trap the phase controller");
        phase.Exit();C(!phase.Active&&phase.PlayerTurn&&!phase.Finish(true),"reset or resuming combat removes aftermath phase state");phase.Enter();C(phase.Finish(false),"without a deployed rescuer one player turn completes the round");
        var web=new WebEffect();web.Begin(0,0,65535,12,world);C(web.TrySave(0,1,world)&&!web.TrySave(0,1,world)&&web.TrySave(0,2,world),"distinct player/rescue phases support once-per-turn environmental triggers");
        var clock=new CorpseClock();clock.Observe(true,9,world);phase.Enter();phase.Finish(true);C(clock.Age(world)==0,"switching to rescue consumes no corpse time");phase.Finish(true);world++;C(clock.Age(world)==1,"corpse time advances only after both phases");
        rescue.TakeDamage(1000);C(rescue.Dead&&!rescue.BeginTurn(player.Round+1),"dead rescuers cannot gain action resources");C(rescue.Revive(1,false)&&!rescue.TurnActive&&!rescue.UseAction(),"revival does not grant a second or out-of-order turn");
        Console.WriteLine("PASS: ordered aftermath actors, one shared clock tick, exact resource refresh, locked off-turn actions, reset, revival and environmental turn identity");
    }
}
