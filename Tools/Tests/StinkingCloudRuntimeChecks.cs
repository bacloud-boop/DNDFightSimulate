using System;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly HashSet<int> undeadIds=new HashSet<int>();bool feigning;
    object Undead(Unit unit)=>unit!=null&&undeadIds.Contains(unit.Id)?(object)unit:null;
    bool Feigning(int actor)=>feigning;
    static TrainingGame Cloud(){var g=new TrainingGame();g.map.Player=new Vector2Int(3,0);g.InitializeStinkingCloud();g.CastStinkingCloud(new Vector2Int(3,0),0);return g;}
    public static void CheckStinking()
    {
        var g=Cloud();C(g.startStatuses.Count==0&&!g.combat.PlayerPoisoned,"creation has no immediate poison save");g.QueueStinkingStart(0);Run(g,g.DrainGreaseEntries());C(g.combat.PlayerPoisoned&&!g.combat.ActionAvailable&&!g.combat.MagicActionAvailable&&!g.combat.BonusActionAvailable,"failed Constitution save causes poison with action and bonus restrictions");C(g.combat.PlayerMovementReady&&g.combat.PlayerCanAct&&g.combat.TryUseReaction(true),"poisoned creature may move and react, without becoming incapacitated");C(!g.combat.TryUseAction()&&!g.combat.TryUseMagicAction()&&!g.combat.TryUseBonusAction(),"resource spending cannot bypass poison gates");g.combat.ClearPoison(true);C(!g.Nauseated(0)&&g.combat.ActionAvailable&&g.combat.BonusActionAvailable,"restoration clears poison and restores unspent action budgets");
        g=Cloud();g.QueueStinkingStart(0);Run(g,g.DrainGreaseEntries());g.EndStinkingCloud();C(g.combat.PlayerPoisoned&&!g.combat.ActionAvailable&&!g.stinkingCloud.Active(0),"ending concentration does not end already applied current-turn poison");g.terrainTurn++;C(!g.combat.PlayerPoisoned&&g.combat.ActionAvailable,"condition expires once current turn ends");
        g=Cloud();g.map.PlayerAquatic.Submerged=true;g.QueueStinkingStart(0);C(g.startStatuses.Count==0&&!g.StinkingBlocks(true,0,0,40,0),"diving avoids new saves and gas obscurement");g.map.PlayerAquatic.Submerged=false;C(g.StinkingBlocks(true,0,0,40,0),"surface sight crossing cloud is blocked");g.map.Water.Add(g.map.Player);g.QueueStinkingStart(0);Run(g,g.DrainGreaseEntries());C(g.Nauseated(0),"water surface still affected");g.map.PlayerAquatic.Submerged=true;C(g.Nauseated(0),"diving after exposure does not cure existing poison");
        g=Cloud();g.QueueStinkingStart(-1);Run(g,g.DrainGreaseEntries());C(!g.combat.MonsterActionAvailable&&!g.combat.MonsterBonusActionAvailable&&g.combat.MonsterCanMove&&g.combat.MonsterCanAct,"monster loses actions without losing movement or reactions");g.combat.ClearPoison(false);C(g.combat.MonsterActionAvailable,"monster poison removal preserves unspent action");
        g=Cloud();g.combat.PoisonConditionSaveAdvantage=p=>true;UnityEngine.Random.Dice.Enqueue(1);UnityEngine.Random.Dice.Enqueue(20);g.QueueStinkingStart(0);Run(g,g.DrainGreaseEntries());C(!g.Nauseated(0)&&UnityEngine.Random.Dice.Count==0,"poison-specific save advantage is forwarded through actual mapped save runtime");
        g=Cloud();g.map.Units.Add(new Unit{Id=7,X=3,Y=0});g.undeadIds.Add(7);g.QueueStinkingStart(7);Run(g,g.DrainGreaseEntries());C(!g.Nauseated(7),"imported undead retain Poisoned immunity");
        g=Cloud();g.QueueStinkingStart(0);g.QueueStinkingStart(0);C(g.startStatuses.Count==1,"one save at each actual start, no duplicate notifications");g.EndStinkingCloud();C(g.startStatuses.Count==0,"ending spell clears pending field saves");
        g=Cloud();g.QueueStinkingStart(0);long old=g.stinkingCloud.Revision;g.CastStinkingCloud(new Vector2Int(4,0),0);Run(g,g.ResolveStinkingSave(0,old));C(!g.Nauseated(0),"old queued casting cannot poison after replacement");g.worldRounds=10;C(!g.stinkingCloud.Active(10)&&!g.StinkingBlocks(true,0,0,40,0),"one-minute field expires");
        g=Cloud();g.QueueStinkingStart(0);Run(g,g.DrainGreaseEntries());g.worldRounds++;C(!g.Nauseated(0),"world-time advances cannot leave stale current-turn poison behind");
        Console.WriteLine("PASS: production Stinking Cloud start saves, poison-only action restrictions, movement/reactions, restoration, concentration separation, surface/diving, poison advantage, immunity and expiry");
    }
}
class StinkingChecksMain {static void Main()=>TrainingGame.CheckStinking();}
