using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace UnityEngine
{
    public struct Vector2 {}public struct Rect {public Rect(float a,float b,float c,float d){}}public struct Color {public Color(float a,float b,float c,float d){}}
    public static class GUILayout
    {
        public static void BeginArea(Rect r){}public static void EndArea(){}public static void Label(string s,object style){}
        public static Vector2 BeginScrollView(Vector2 p)=>p;public static void EndScrollView(){}public static bool Button(string s,object style)=>false;
    }
}
public sealed class SummonedUnit {public int Hp=10;public bool Alive=>Hp>0;public int SleepStage,SleepRepeatRound;}
public static class BattleMap {public const int RescueId=int.MinValue;}
public sealed partial class TrainingGame
{
    sealed class RoundStub {public int Round=1;}sealed class RescueStub {public bool Dead=false;}
    RoundStub combat=new RoundStub();RescueStub rescueCarrier=new RescueStub();
    readonly AcidArrowResidue acidArrows=new AcidArrowResidue();object heading=new object(),button=new object(),paperTexture=new object();
    bool cloud,sphere,greased,killOnResidue;SummonedUnit active;
    readonly List<string> log=new List<string>();int choices;
    string TransitActorName(int actor)=>actor.ToString();void Fill(UnityEngine.Rect r,UnityEngine.Color c){}void Surface(UnityEngine.Rect r,object t){}
    object ActorGrease(int actor)=>greased?new object():null;bool DaggersEndAvailable(int actor)=>cloud;bool SphereEndAvailable(int actor)=>sphere;
    IEnumerator RepeatSummonSleep(SummonedUnit unit){log.Add("sleep");unit.SleepStage=0;yield break;}
    IEnumerator ResolveGreaseSave(int actor,object field,string text){log.Add("grease");yield break;}
    IEnumerator EndDaggersTurn(int actor){log.Add("daggers");yield break;}
    IEnumerator EndFlamingSphereTurn(int actor){log.Add("sphere");yield break;}
    IEnumerator ResolveAcidArrowResidue(AcidArrowResidue.Entry e){if(!acidArrows.Take(e.Id))throw new Exception("duplicate residue claim");log.Add("acid:"+e.Id);if(killOnResidue&&active!=null)active.Hp=0;yield break;}
    static void Run(TrainingGame g,IEnumerator routine)
    {
        var stack=new Stack<IEnumerator>();stack.Push(routine);
        while(stack.Count>0){var top=stack.Peek();if(!top.MoveNext()){stack.Pop();continue;}var nested=top.Current as IEnumerator;if(nested!=null){stack.Push(nested);continue;}
            if(g.unitEndOrderPending){g.choices++;int chosen=Array.FindIndex(g.unitEndOrderLabels,s=>s.Contains("#2"));if(chosen<0)chosen=g.unitEndOrderLabels.Length-1;g.unitEndOrderChoice=chosen;}
        }
    }
    static void C(bool v,string m){if(!v)throw new Exception(m);}
    public static void Main()
    {
        var g=new TrainingGame();var unit=new SummonedUnit();g.active=unit;var one=g.acidArrows.Add(0,7,2);var two=g.acidArrows.Add(-1,7,5);
        Run(g,g.FinishUnitEndEffects(7,unit));C(g.log.Count==0&&g.acidArrows.Pending(7).Count==0,"production end handler does not run future residue in the cast turn");
        g.acidArrows.StartTurn(7);g.cloud=g.sphere=g.greased=true;unit.SleepStage=1;unit.SleepRepeatRound=1;Run(g,g.FinishUnitEndEffects(7,unit));
        C(g.log.Count==6&&g.log[0]=="acid:"+two.Id&&g.log[1]=="acid:"+one.Id&&g.log.Distinct().Count()==6&&g.choices==5,"production end handler dispatches independently selected residue IDs among sleep, grease, daggers and sphere");
        C(g.acidArrows.Pending(7).Count==0&&!g.unitEndOrderPending,"all selected residues are claimed once and modal state closes");
        g.cloud=g.sphere=g.greased=false;g.log.Clear();Run(g,g.FinishUnitEndEffects(7,unit));C(g.log.Count==0,"repeated end callback cannot duplicate consumed damage");
        g=new TrainingGame();g.active=new SummonedUnit();g.killOnResidue=true;g.acidArrows.Add(0,7,2);g.acidArrows.Add(-1,7,2);g.acidArrows.StartTurn(7);Run(g,g.FinishUnitEndEffects(7,g.active));C(g.log.Count==1&&!g.active.Alive,"death from the selected residue stops remaining end effects immediately");
        g=new TrainingGame();g.acidArrows.Add(0,BattleMap.RescueId,2);g.acidArrows.StartTurn(BattleMap.RescueId);Run(g,g.FinishUnitEndEffects(BattleMap.RescueId));C(g.log.Single()=="acid:1","shared end handler handles independent auxiliary actor IDs without a summon object");
        Console.WriteLine("PASS: production turn-end residue dispatch, no premature damage, selectable mixed-effect order, independent one-time claims, modal cleanup and stop on target death");
    }
}
