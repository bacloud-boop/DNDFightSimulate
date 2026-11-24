using System;
using System.Linq;
using UnityEngine;
class BanishmentMapChecks
{
    static void C(bool v,string text){if(!v)throw new Exception(text);}
    static void Main()
    {
        var map=new BattleMap();map.Lock();var original=map.Monster;
        map.SetActorScene(-1,1);C(map.BodyPresent(-1)&&!map.OnScene(-1)&&map.OccupiedSpaces(false)==0&&map.DistanceFeet==int.MaxValue,"away body remains existent but has no training-field footprint or distance");
        var blocker=new SummonedUnit(7,"blocker",20,10,30,2,1);C(map.AddSummon(blocker,original),"the original position becomes available to another creature");
        map.ViewActorScene(-1);C(map.Scene==1&&map.OnScene(-1)&&!map.OnScene(0)&&!map.SceneSummons.Any(),"temporary scene shows only its own participants");
        C(map.NearestEnemyInReach(true,int.MaxValue)<0&&map.CreatureCoverBonus(0,-1)==0,"other planes provide neither targets nor cover");
        bool shared;C(map.RestoreSceneBody(-1,0,original,out shared)&&!shared&&map.Monster!=original&&map.SameScene(0,-1),"return picks nearest legal unoccupied footprint without moving its blocker");
        map.ViewScene(0);C(map.OnScene(-1)&&map.OnScene(7)&&map.BodyPresent(-1),"return does not duplicate or consume bodies");
        map.SetActorScene(-1,-1);C(map.BodyPresent(-1)&&!map.OnScene(-1)&&!map.SameScene(-1,-1),"permanent departure is distinct from death and any active scene");
        map.ClearScenes();C(map.Scene==0&&map.OnScene(-1),"reset restores original scene membership");
        var state=new CombatState();state.MonsterBanished=true;C(state.Hp==1000&&!state.MonsterCanAct&&!state.MonsterCanMove&&!state.TryUseReaction(false),"banishment incapacitates without inflicting damage");
        state.MonsterDeparted=true;C(state.BattleFinished&&!state.PlayerCanAct&&state.Hp==1000,"permanent departure ends the battle without killing the dummy");state.ResetBattle();C(!state.MonsterBanished&&!state.MonsterDeparted&&!state.BattleFinished,"battle reset clears both temporary and permanent flags");
        var unit=new SummonedUnit(8,"away",20,10,30,2,1);unit.Banished=true;int starts=0;unit.TurnStarted=()=>starts++;C(unit.BeginTurn(1)&&starts==1&&!unit.ActionAvailable&&!unit.UseReaction()&&!unit.TryUseBonus()&&unit.MovementRemaining==0&&unit.Alive,"away summons keep passive turn identity but cannot act or move");
        var grease=new GreaseFields();grease.Add(0,0,15,10,0,1);C(!grease.Cell(0,0,0)&&grease.Cell(0,0,0,1)&&grease.Current(0,0,1,0,1)!=null,"nonconcentration grease belongs to its originating plane");
        var ice=new IceStormFields();ice.Add(0,3,3,10,(x,y)=>true,0,1);C(!ice.Cell(3,3,0)&&ice.Cell(3,3,0,1),"independent ice terrain cannot cross planes");
        var ground=new GroundWeaponState(0){SceneProvider=()=>1};C(ground.Drop(2,2,1)&&ground.Scene==1,"dropped equipment records its actual scene");
        Console.WriteLine("PASS: planar map membership, body identity, occupancy, cover, nearest return, permanent non-death victory, passive incapacitated turns, terrain and ground-equipment scene isolation");
    }
}
