using System;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed partial class Unit{public Func<long[],long[]> IncomingDamage;public Func<int,bool> IntrinsicImmunity;public bool Resistant;public bool ResistsType(int type)=>Resistant;public long[] PrepareIncomingDamage(long[] raw)=>IncomingDamage==null?raw:IncomingDamage(raw);}
    sealed partial class Rescue{public Func<long[],long[]> IncomingDamage;public bool Resistant;public bool ResistsType(int type)=>Resistant;}
    object label=new object();bool SilenceUnitImmune(Unit u,string type)=>false;
    bool BanishLiving(int actor)=>actor==0?!combat.PlayerDead:actor==-1?combat.Hp>0:FindSummon(actor)!=null;
    bool HealingHandsCanTouch(int caster,int target){Vector2Int a,b;int n,m;string name;return HealingHandsTarget(caster,out a,out n,out name)&&HealingHandsTarget(target,out b,out m,out name)&&BattleMap.BodyDistance(a,n,b,m)<=5;}
    static TrainingGame Protected(int actor=0,int type=5){var g=new TrainingGame();g.InitializeResistance();g.CastResistance(actor,type);return g;}
    public static void CheckResistance()
    {
        var g=Protected();g.combat.DamageResistance=(p,t)=>t==5;C(g.combat.ResolveTypedPacket(true,new long[]{0,0,0,0,0,10})[5]==5&&g.resistanceRolls.Count==0,"pure preview cannot consume or roll protection");int actual=g.combat.AdjustIncomingTypedDamage(true,10,5);C(actual==3&&g.resistanceRolls.Count==1,"reduce ten by d4=4 then halve six to three");g.combat.ApplyPlayerDamage(actual);C(g.combat.PlayerHp==7,"actual HP uses defended damage");C(g.combat.AdjustIncomingTypedDamage(true,10,5)==5&&g.resistanceRolls.Count==1,"second damage in same turn does not roll again");g.terrainTurn++;C(g.combat.AdjustIncomingTypedDamage(true,10,5)==3&&g.resistanceRolls.Count==2,"another actor's global turn restores one benefit");
        g=Protected();g.combat.DamageImmunity=(p,t)=>t==5;C(g.combat.AdjustIncomingTypedDamage(true,20,5)==0&&g.resistanceRolls.Count==0&&g.resistanceSpell.Available(0,g.terrainTurn,0),"immunity never spends once-turn protection");g.combat.DamageImmunity=null;g.combat.DamageResistance=(p,t)=>t==5;C(g.combat.AdjustIncomingTypedDamage(true,1,5)==0&&g.resistanceRolls.Count==0,"damage already rounded to zero does not consume protection");
        g=Protected();g.combat.DamageVulnerability=(p,t)=>t==5;var raw=new long[13];raw[5]=10;raw[1]=7;var packet=g.combat.ResolveIncomingPacket(true,raw);C(packet[5]==12&&packet[1]==7&&raw[5]==10,"selected type only, reduction before vulnerability, caller packet unchanged");
        g=Protected(-1,1);C(g.combat.AdjustIncomingTypedDamage(false,3,1)==0&&g.resistanceRolls.Count==1,"reduction floors at zero on monster");C(g.combat.AdjustIncomingTypedDamage(true,20,1)==20,"one target only");
        g=Protected();g.map.Units.Add(new Unit{Id=9});var u=g.FindSummon(9);u.IncomingDamage=r=>g.ResistanceAdjusted(9,r);g.CastResistance(9,5);raw=new long[13];raw[5]=8;C(u.PrepareIncomingDamage(raw)[5]==4,"summon raw-packet callback uses same production effect");
        g=Protected(BattleMap.RescueId,0);raw=new long[13];raw[0]=8;C(g.rescueCarrier.IncomingDamage(raw)[0]==4,"rescue carrier callback uses same effect");
        g=Protected();g.combat.AdjustIncomingTypedDamage(true,8,5);C(g.resistanceRolls.Count==1,"one queued visual per actual benefit");g.resistanceSpell.Clear();Run(g,g.DrainResistanceRolls());C(g.resistanceRolls.Count==0&&!g.drainingResistance&&!g.rolling,"queued die remains visible after concentration ends and restores busy state");
        g=Protected();g.combat.AdjustIncomingTypedDamage(true,8,5);g.resistanceSpell.Clear();g.CastResistance(0,5);C(g.combat.AdjustIncomingTypedDamage(true,8,5)==8&&g.resistanceRolls.Count==1,"ending and recasting cannot bypass one benefit per creature per turn");g.terrainTurn++;C(g.combat.AdjustIncomingTypedDamage(true,8,5)==4,"later turn restores benefit after recast");
        g=Protected();g.worldRounds=10;C(g.combat.AdjustIncomingTypedDamage(true,10,5)==10,"one-minute expiry");C(!g.resistanceSpell.Begin(0,6,0)&&!g.resistanceSpell.Begin(0,10,0),"force and psychic excluded");
        g=Protected(-1);g.resistanceTarget=-1;C(g.ResistanceError()!="","nonwilling target rejected");g.map.Monster=new Vector2Int(1,0);g.resistanceWilling=true;C(g.ResistanceError()=="","adjacent willing monster accepted");
        Console.WriteLine("PASS: production Resistance target/type/once-turn damage reduction, immunity/zero, resistance/vulnerability order, summon/rescue wiring, visual queue and expiry");
    }
}
class ResistanceChecksMain{static void Main()=>TrainingGame.CheckResistance();}
