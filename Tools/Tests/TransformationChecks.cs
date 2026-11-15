using System;
using UnityEngine;

class TransformationChecks
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
    static void Main()
    {
        var map=new BattleMap();map.Place(false,V(4,4));map.Place(true,V(1,1));map.Lock();
        Check(map.MovePlayer(V(1,2)),"movement before transformation");
        int spent=map.MovementSpent,remaining=map.MovementRemaining;
        Check(!map.TransformAt(true,2,V(7,7),1),"form cannot teleport away from old body");
        Check(map.Player==V(1,2)&&map.PlayerSpaces==1,"invalid transform preserves state");
        Check(map.TransformAt(true,2,V(1,2),1),"player grows into legal overlapping footprint");
        Check(map.MovementSpent==spent&&map.MovementRemaining==remaining,"transform cannot refund movement");
        Check(map.RestoreForm(true)&&map.PlayerSpaces==1&&BattleMap.Overlap(map.Player,1,V(1,2),2),"player restoration stays inside transformed footprint");
        Check(!map.RestoreForm(true),"restoration clears its guard");
        Check(map.TransformAt(false,1,V(4,4),3),"monster can shrink with larger original reserved");
        Check(map.MoveMonster(V(5,4)),"monster moves while small");
        Check(map.RestoreForm(false)&&map.MonsterSpaces==3&&BattleMap.Overlap(map.Monster,3,V(5,4),1),"monster regrows near its current body");
        var combat=new CombatState();int hp=combat.PlayerHp,max=combat.PlayerMaxHp;
        combat.GrantPlayerTemporaryHp(20);combat.ChooseTemporaryHp(true,4,false);
        Check(combat.PlayerTemporaryHp==20,"keep old pool");
        combat.ChooseTemporaryHp(true,4,true);
        Check(combat.PlayerTemporaryHp==4&&combat.PlayerHp==hp&&combat.PlayerMaxHp==max,"explicit replacement may be lower without changing real HP");
        combat.ApplyPlayerDamage(6);Check(combat.PlayerTemporaryHp==0&&combat.PlayerHp==hp-2,"overflow damage reaches retained HP");
        combat.GrantMonsterTemporaryHp(20);combat.ChooseTemporaryHp(false,3,true);
        combat.ApplyDamage(5);Check(combat.MonsterTemporaryHp==0&&combat.Hp==combat.MaxHp-2,"monster replacement and overflow");
        combat.ChooseTemporaryHp(true,7,true);combat.ChooseTemporaryHp(true,30,false);
        Check(combat.PlayerTemporaryHp==7,"can retain smaller old pool too");
        bool rejected=false;try{combat.ChooseTemporaryHp(true,-1,true);}catch(ArgumentOutOfRangeException){rejected=true;}
        Check(rejected&&combat.PlayerTemporaryHp==7,"negative pool rejected atomically");
        combat.MonsterFormArmor=12;combat.MonsterArmorBonus=2;combat.MonsterFormSpeed=40;
        Check(combat.MonsterArmorClass==14&&combat.MonsterBaseArmorClass==10&&combat.MonsterSpeed==40,"temporary beast stats preserve monster base configuration and spell modifiers");
        combat.MonsterFormArmor=null;combat.MonsterFormSpeed=null;
        Check(combat.MonsterArmorClass==12&&combat.MonsterSpeed==30,"removing stat layer restores underlying armor and speed");
        var crawl=new BattleMap();crawl.Lock();crawl.PlayerCrawling=true;
        Check(crawl.MovePlayer(V(0,3))&&crawl.MovementRemaining==0,"crawling spends twice the normal movement distance");
        Console.WriteLine("PASS: atomic transformation, local restoration, movement retention, temporary HP choices and overflow");
    }
}
