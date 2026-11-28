using System;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    object label=new object();bool freedom;bool MovementFreedom(int actor)=>freedom;void SyncTimedBuffs(){}
    static TrainingGame Guardian(int alignment=4,int slot=3,HashSet<int> excluded=null){var g=new TrainingGame();g.playerAlignment=alignment;g.map.Monster=new Vector2Int(3,0);g.InitializeGuardians();g.CastGuardians(0,slot,excluded??new HashSet<int>());return g;}
    public static void CheckGuardians()
    {
        for(int a=0;a<9;a++){var g=Guardian(a);C(g.guardians.Type==(a>=6?8:11)&&g.concentrationExpiresWorld==100,"nine alignments choose correct damage and ten-minute duration");}
        var x=Guardian();C(x.startStatuses.Count==1&&x.combat.MonsterSpeed==15,"creation immediately queues damage and halves speed");Run(x,x.DrainGreaseEntries());C(x.combat.Hp==976&&!x.GuardianEndAvailable(-1),"three d8 damage and creation consumes same-turn save allowance");x.QueueGuardianEnd(-1);C(x.startStatuses.Count==0,"no duplicate at end of same turn");
        x.terrainTurn++;x.QueueGuardianEnd(-1);Run(x,x.DrainGreaseEntries());C(x.combat.Hp==952,"new actor turn can trigger again");C(!x.GuardianAffected(0),"caster always excluded");
        x=Guardian(8,5,new HashSet<int>{-1});C(x.combat.MonsterSpeed==30&&x.startStatuses.Count==0,"excluded creatures receive neither damage nor speed reduction");
        x=Guardian(6,5);UnityEngine.Random.D20=20;x.combat.DamageResistance=(p,t)=>t==8;Run(x,x.DrainGreaseEntries());C(x.combat.Hp==990,"five d8 necrotic, save half then resistance half");UnityEngine.Random.D20=1;
        x=Guardian();x.startStatuses.Clear();x.map.Monster=new Vector2Int(8,0);var old=x.map.Player;x.map.Player=new Vector2Int(5,0);x.ObserveGuardianMovement(0,old,x.map.Player,1,1,false);C(x.startStatuses.Count==0,"already claimed creature cannot repeat save after aura moves on same turn");x.terrainTurn++;x.map.Player=new Vector2Int(0,0);x.ObserveGuardianMovement(0,new Vector2Int(5,0),x.map.Player,1,1,false);x.map.Player=new Vector2Int(5,0);x.ObserveGuardianMovement(0,new Vector2Int(0,0),x.map.Player,1,1,false);C(x.startStatuses.Count==1,"moving aura across target queues new-turn trigger");
        x=Guardian();x.startStatuses.Clear();x.terrainTurn++;x.map.Monster=new Vector2Int(3,0);x.ObserveGuardianMovement(-1,new Vector2Int(8,0),x.map.Monster,1,1,false);C(x.startStatuses.Count==1,"creature entering aura triggers save");x.map.MonsterScene=1;C(!x.GuardianAffected(-1)&&x.combat.MonsterSpeed==30,"different planes isolate damage and speed");
        x=Guardian();x.map.Monster=new Vector2Int(6,0);x.map.MonsterSpaces=4;C(!x.GuardianAffected(-1),"distant target footprint excluded");x.map.Monster=new Vector2Int(3,0);C(x.GuardianAffected(-1),"nearest occupied target cell defines reach");x.freedom=true;C(x.combat.MonsterSpeed==30&&x.GuardianAffected(-1),"Freedom of Movement prevents slow but not spell damage");
        x=Guardian();x.map.Monster=new Vector2Int(8,0);x.map.StartMonsterMovementTurn(30);x.map.SpendMonsterMovement(10);x.map.Monster=new Vector2Int(3,0);C(x.map.MonsterMovementRemaining==5,"entering aura lowers total movement allowance while preserving ten feet already spent");x.map.SpendMonsterMovement(5);C(x.map.MonsterMovementRemaining==0,"halved movement exhausted");x.map.Monster=new Vector2Int(8,0);C(x.map.MonsterMovementRemaining==15,"leaving via relocation restores normal allowance, without refunding spent movement");x.map.GrantMonsterDash(30);C(x.map.MonsterMovementRemaining==45,"dash tracks live speed budget");x.map.Monster=new Vector2Int(3,0);C(x.map.MonsterMovementRemaining==15,"dash also respects halved live speed");
        x=Guardian();long revision=x.guardians.Revision;x.EndGuardians();Run(x,x.ResolveGuardianSave(-1,revision,Vector2Int.zero,1));C(x.combat.Hp==1000&&x.combat.MonsterSpeed==30&&x.startStatuses.Count==0,"ending concentration clears queues and live speed modifier");x=Guardian();x.worldRounds=100;C(!x.GuardianAffected(-1)&&x.combat.MonsterSpeed==30,"exact expiry removes aura effects");
        C(!new SpiritGuardiansEffect().Begin(3,9,10,null,0)&&!new SpiritGuardiansEffect().Begin(3,4,10,null,long.MaxValue-99),"invalid alignment and overflowing duration rejected");
        Console.WriteLine("PASS: production Spirit Guardians nine alignments, creation damage, exclusions, per-turn saves, movement triggers, mitigation, speed budgets, planes and cleanup");
    }
}
class GuardiansChecksMain {static void Main()=>TrainingGame.CheckGuardians();}
