using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine { public static class Random { public static int Range(int min,int max)=>max-1; } }
public sealed partial class TrainingGame
{
    sealed class Unit {public int Id,X,Y,Spaces=1;public bool Alive=true,Neutral;}
    sealed class MapStub {
        public Vector2Int Player=new Vector2Int(0,0);public int PlayerSpaces=1,PlayerScene,AllyScene;public bool Contained;
        public readonly List<Unit> Units=new List<Unit>();
        public bool SameScene(int a,int b)=>PlayerScene==AllyScene;
        public bool IsContained(bool player)=>player&&Contained;
        public bool SceneBody(int id,out Vector2Int p,out int n){p=Player;n=PlayerSpaces;if(id==0)return true;var u=Units.Find(x=>x.Id==id);if(u==null)return false;p=new Vector2Int(u.X,u.Y);n=u.Spaces;return true;}
    }
    readonly MapStub map=new MapStub();readonly CombatState combat=new CombatState();
    readonly List<string> history=new List<string>();readonly List<int> presented=new List<int>();
    string concentrationSpell="";long worldRounds,concentrationExpiresWorld;int concentrationExpiresRound;
    Unit FindSummon(int actor)=>map.Units.Find(u=>u.Id==actor);
    bool NeutralUndead(int actor)=>FindSummon(actor)!=null&&FindSummon(actor).Neutral;
    string TransitActorName(int actor)=>actor.ToString();
    IEnumerator PresentRoll(int sides,int[] values,string label){C(sides==4,"aura must roll d4");presented.Add(values.Length);yield break;}
    static void C(bool ok,string reason){if(!ok)throw new Exception(reason);}
    static void Run(IEnumerator routine){while(routine.MoveNext())if(routine.Current is IEnumerator)Run((IEnumerator)routine.Current);}
    static TrainingGame New(){var g=new TrainingGame();g.CastCrusaderMantle();g.map.Units.Add(new Unit{Id=7,X=6});return g;}
    static long[] Hit(TrainingGame g,int actor,bool crit=false){var raw=new long[13];raw[1]=5;Run(g.AddCrusaderMantleDamage(actor,crit,raw));return raw;}
    public static void CheckMantle()
    {
        var g=New();C(g.concentrationSpell=="十字军披风"&&g.concentrationExpiresWorld==10&&g.concentrationExpiresRound==int.MaxValue,"one-minute concentration begins");
        C(Hit(g,0)[11]==4&&g.presented[0]==1,"caster normal hit adds exactly one d4");
        C(Hit(g,7,true)[11]==8&&g.presented[1]==2,"ally at exactly thirty feet receives doubled dice on a critical");
        C(Hit(g,-1)[11]==0&&Hit(g,99)[11]==0,"enemy and missing actors never receive the aura");
        g.map.Units[0].X=7;C(Hit(g,7)[11]==0,"thirty-five feet excluded");g.map.Units[0].X=4;g.map.Units[0].Y=4;C(Hit(g,7)[11]==4,"diagonal uses map geometry");g.map.Units[0].X=5;g.map.Units[0].Y=5;C(Hit(g,7)[11]==0,"diagonal outside radius excluded");
        g.map.Units[0].Y=0;g.map.PlayerSpaces=2;g.map.Units[0].X=7;C(Hit(g,7)[11]==4,"mounted or large caster measures from complete footprint");
        g.map.Contained=true;C(Hit(g,0)[11]==4&&Hit(g,7)[11]==0,"swallowed caster retains self benefit but not external aura");g.map.Contained=false;
        g.map.AllyScene=1;C(Hit(g,7)[11]==0,"different plane excluded");g.map.PlayerScene=1;C(Hit(g,7)[11]==4,"same-plane allies benefit even away from original map");
        g.map.Units[0].Neutral=true;C(Hit(g,7)[11]==0,"expired undead control removes ally benefit");g.map.Units[0].Neutral=false;g.map.Units[0].Alive=false;C(Hit(g,7)[11]==0,"dead allies excluded");
        g=New();var raw=Hit(g,0);raw[11]+=3;g.combat.DamageResistance=(p,t)=>t==11;var resolved=g.combat.ResolveTypedPacket(false,raw);C(resolved[1]==5&&resolved[11]==3,"radiant damage merges with other radiant before resistance rounds once");
        g.combat.DamageImmunity=(p,t)=>t==11;C(g.combat.ResolveTypedPacket(false,raw)[11]==0,"radiant immunity applies to complete hit");
        g=New();g.combat.PlayerParalyzed=true;C(Hit(g,7)[11]==0,"incapacitation stops aura immediately");g.combat.PlayerParalyzed=false;g.combat.PlayerBanished=true;C(Hit(g,0)[11]==0,"banishment incapacitation prevents benefits");g.combat.PlayerBanished=false;
        g.worldRounds=9;C(Hit(g,0)[11]==4,"active before expiry");g.worldRounds=10;C(Hit(g,0)[11]==0,"expires at ten rounds");g.worldRounds=0;g.concentrationSpell="其他专注";C(Hit(g,0)[11]==0,"ending or replacing concentration clears all benefits without stale flags");
        Console.WriteLine("PASS: production Crusader's Mantle caster/allies, exact radius/diagonals/multicell, critical d4s, planes, swallowed isolation, expired control, typed packet merging, immunity and concentration expiry");
    }
}
class CrusaderMantleChecksMain { static void Main()=>TrainingGame.CheckMantle(); }
