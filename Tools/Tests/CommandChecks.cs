using System;
using System.Collections.Generic;
using UnityEngine;
class CommandChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
    static List<int> Route(BattleMap map,int actor,Vector2Int source,int spaces,int budget,bool flee)
    {
        Vector2Int start;int n;C(map.MovementBody(actor,out start,out n),"actor exists");int w=map.Size;
        return CommandRoute.Plan(w,start.y*w+start.x,budget,flee,(a,b)=>map.CommandStepCost(actor,V(a%w,a/w),V(b%w,b/w)),i=>BattleMap.BodyDistance(V(i%w,i/w),n,source,spaces),i=>map.CommandCanEnd(actor,V(i%w,i/w)));
    }
    static void Main()
    {
        var effects=new CommandEffect();C(effects.Add(0,-1,12,CommandWord.Flee)&&effects.Next(-1).Word==CommandWord.Flee,"casting records a future turn without executing now");
        effects.Add(3,-1,15,CommandWord.Grovel);effects.Add(0,-1,14,CommandWord.Drop);C(effects.Next(-1).Word==CommandWord.Grovel,"highest-DC same-name effect wins");
        effects.Add(0,-1,15,CommandWord.Approach);C(effects.Take(-1).Word==CommandWord.Approach&&effects.Take(-1)==null,"newest breaks tied DC and all copies are consumed once on next turn");
        effects.Add(-1,0,-100,CommandWord.Halt);effects.Remove(0);C(effects.Next(0)==null&&!effects.Add(0,0,10,(CommandWord)5),"death cleanup and invalid word rejection");
        C(CommandEffect.TargetLimit(1)==1&&CommandEffect.TargetLimit(9)==9,"one extra target per upcast slot");
        var sequence=new MonsterSpellSequence();C(sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Command",3,2)}),"monster can configure third-level Flee");
        C(sequence.Take().CommandMode==2&&sequence.Next.CommandMode==2&&MonsterSpellSequence.BaseLevel("Command")==1,"monster cycle preserves mode on copy and repetition");
        C(!sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Command",1,9)})&&sequence.Next.CommandMode==2,"invalid draft doesn't destroy saved cycle");
        var spell=DirectSpell.Find("Command");C(spell!=null&&spell.Level==1&&spell.Range==60&&!spell.Control&&!spell.Attack,"Command is dedicated nonconcentration logic, without charm/type/save-attack shortcuts");
        var map=new BattleMap();map.Place(false,V(8,0));map.Lock();int events=0;map.MovementObserved=(a,b,c,d,e,f)=>events++;
        var approach=Route(map,-1,map.Player,1,30,false);C(approach.Count==6&&approach[5]==2,"Approach uses only thirty feet, stops short when out of range, and does not auto-Dash");
        C(map.Monster==V(8,0)&&map.Player==V(0,0)&&map.MonsterMovementRemaining==30&&events==0,"route search cannot move units, spend resources or trigger terrain");
        map.StartMonsterMovementTurn(60);approach=Route(map,-1,map.Player,1,60,false);C(approach.Count==7&&approach[6]==1,"shortest approach stops at five feet without entering caster's cell");
        var flee=Route(map,0,map.Monster,1,30,true);C(flee.Count>0,"Flee finds an available route away from caster");int last=flee[flee.Count-1];C(BattleMap.BodyDistance(V(last%10,last/10),1,map.Monster,1)>map.DistanceFeet,"flee increases source distance rather than chasing nearest enemy");
        var wall=new BattleMap();wall.Place(false,V(5,0));wall.Lock();wall.CreatureSizeIndex=id=>2;
        for(int y=0;y<3;y++)C(wall.AddSummon(new SummonedUnit(y+1,"barrier",20,10,30,2,1),V(2,y)),"barrier fixture");
        var detour=Route(wall,-1,wall.Player,1,100,false);C(detour.Count>0,"approach finds a route around obstructing footprints");bool around=false;foreach(int step in detour)if(step/10>=3)around=true;C(around,"shortest-path search can initially move away to go around obstacles");
        var water=new BattleMap();water.SetWater(V(0,1),true);water.Lock();C(water.CommandStepCost(0,V(0,0),V(0,1))==10,"water and no swim speed double command movement cost");water.PlayerSwimmingSpeed=30;C(water.CommandStepCost(0,V(0,0),V(0,1))==5,"swim speed removes extra water cost");
        var allied=new BattleMap();allied.Lock();allied.CreatureSizeIndex=id=>2;allied.AddSummon(new SummonedUnit(1,"ally",20,10,30,2,1),V(1,0));
        C(allied.CommandStepCost(0,V(0,0),V(1,0))==5&&!allied.CommandCanEnd(0,V(1,0)),"allied transit preserves existing movement costs but cannot be a voluntary endpoint");
        var noEnd=CommandRoute.Plan(4,0,5,false,(a,b)=>Math.Abs(a-b)==1?5:int.MaxValue,i=>(3-i)*5,i=>i!=1);C(noEnd.Count==0,"partial approach never voluntarily stops in an occupied transit cell");
        var fighter=new FighterFeatures();var combat=new CombatState();combat.TryUseAction();C(fighter.ActionSurgeUsed==0&&fighter.SecondWindUsed==0&&!combat.ActionAvailable,"ordinary Dash spending leaves limited fighter resources untouched");
        Console.WriteLine("PASS: delayed orders, simultaneous copies, upcasting, monster command cycles, action budgets, weighted shortest routes, safe endpoints, water/ally costs and mutation-free planning");
    }
}
