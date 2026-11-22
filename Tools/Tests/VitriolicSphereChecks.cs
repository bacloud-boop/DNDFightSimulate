using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace UnityEngine
{
    public struct Vector2Int {public int x,y;public Vector2Int(int a,int b){x=a;y=b;}public static Vector2Int zero=>new Vector2Int();}
    public struct Rect {}public struct Color {public Color(float a,float b,float c,float d){}}
    public static class Random {public static int Range(int min,int max)=>max-1;}
    public static class GUILayout {public static void Label(string value,object style){}}
}
public static class BattleMap {public const int RescueId=int.MinValue;public static bool Overlap(UnityEngine.Vector2Int a,int n,UnityEngine.Vector2Int b,int m)=>false;}
public static class AcidSplash {public static int Modifier(int v)=>(int)Math.Floor((v-10)/2.0);}
public sealed partial class TrainingGame
{
    sealed class MapStub {public int Size=20,MonsterSpaces=1;public UnityEngine.Vector2Int Player=new UnityEngine.Vector2Int(),Monster=new UnityEngine.Vector2Int();public bool Contained=false;public bool IsContained(bool p)=>Contained;public int DistanceToCell(bool player,UnityEngine.Vector2Int p)=>p.x*5;}
    sealed class ClassStub {public int ProficiencyBonus=2;}sealed class RayDamage {public int Amount;}
    readonly MapStub map=new MapStub();readonly ClassStub classes=new ClassStub();readonly int[] attributes={10,10,10,10,10,10};readonly AcidArrowResidue acidArrows=new AcidArrowResidue();
    readonly List<int> actors=new List<int>{0,-1,7,BattleMap.RescueId};readonly HashSet<int> saved=new HashSet<int>{-1};readonly HashSet<int> dead=new HashSet<int>();
    readonly Dictionary<int,int> damage=new Dictionary<int,int>();readonly List<string> history=new List<string>();int outcomeDeferDepth,concentrationDamage,checks;long MonsterSpellDc=15;
    object small=new object();int directSlotLevel=4;string visionColumn,visionRow,panel="",selectedSpellName="",dice;
    bool VisionPoint(out UnityEngine.Vector2Int p){p=new UnityEngine.Vector2Int(3,3);return true;}
    string IntegerField(string value,bool flag)=>value;string TransitActorName(int actor)=>actor.ToString();int SpellAbilityIndex(int caster)=>4;
    List<int> IceStormTargets(int source,UnityEngine.Vector2Int p)=>actors.ToList();bool Feigning(int id)=>false;bool NeutralUndead(int id)=>false;
    bool AcidArrowTargetAvailable(int id)=>!dead.Contains(id);
    IEnumerator RollMappedAbilitySave(int actor,int source,long dc,HashSet<int> failed,string name,int ability,UnityEngine.Vector2Int p){if(!saved.Contains(actor))failed.Add(actor);yield break;}
    IEnumerator PresentRoll(int sides,int[] rolls,string label){yield break;}
    IEnumerator ReduceRayDamage(bool player,int raw,string type,string label,RayDamage result){result.Amount=raw;yield break;}
    int ApplyMappedDamagePacket(int actor,int source,long[] raw){int value=(int)raw[3];damage[actor]=value;return value;}
    IEnumerator MaintainConcentration(int amount){concentrationDamage=amount;yield break;}
    void RefreshUndeadFollowers(){}void RefreshBestialSpirit(){}void ReconcileFallenRescue(){}void ClearUnavailableAcidArrows()=>acidArrows.RemoveUnavailable(AcidArrowTargetAvailable);void CheckBattleOutcome(){checks++;}
    UnityEngine.Rect BodyRect(UnityEngine.Rect board,UnityEngine.Vector2Int p,int n)=>board;UnityEngine.Rect CellRect(UnityEngine.Rect board,UnityEngine.Vector2Int p)=>board;void Fill(UnityEngine.Rect r,UnityEngine.Color c){}
    static void Run(IEnumerator routine){var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){var top=stack.Peek();if(!top.MoveNext()){stack.Pop();continue;}if(top.Current is IEnumerator)stack.Push((IEnumerator)top.Current);}}
    static void C(bool value,string text){if(!value)throw new Exception(text);}
    public static void Main()
    {
        for(int slot=4;slot<=9;slot++)C(VitriolicSphereRules.InitialDice(slot)==10+2*(slot-4)&&VitriolicSphereRules.LaterDice(slot)==5,"upcasting changes initial dice only");
        C(VitriolicSphereRules.InitialDice(3)==0&&VitriolicSphereRules.LaterDice(10)==0&&VitriolicSphereRules.Packet(19,true)[3]==9,"invalid slots and save rounding");
        var g=new TrainingGame();g.acidArrows.StartTurn(0);Run(g.ResolveVitriolicSphere(true,0,4,new UnityEngine.Vector2Int(3,3)));
        C(g.damage.Count==4&&g.damage[0]==40&&g.damage[-1]==20&&g.damage[7]==40&&g.damage[BattleMap.RescueId]==40,"production AoE hits self, allies, auxiliary actors; each successful save halves shared initial damage");
        C(g.concentrationDamage==40&&g.outcomeDeferDepth==0&&g.checks==1,"self damage checks concentration and defers outcome through entire AoE");
        C(g.acidArrows.Pending(0).Count==0,"self-target waits until its NEXT turn, not the current end");
        foreach(int actor in g.actors)g.acidArrows.StartTurn(actor);
        C(g.acidArrows.Pending(-1).Count==0&&g.acidArrows.Pending(7).Single().Dice==5&&g.acidArrows.Pending(BattleMap.RescueId).Single().Title=="浓酸球","saved target has no residue; failed targets retain independent titled five-die entries");
        g.acidArrows.Add(0,7,2);Run(g.ResolveVitriolicSphere(false,0,9,new UnityEngine.Vector2Int()));g.acidArrows.StartTurn(7);
        C(g.damage[7]==80&&g.acidArrows.Pending(7).Count==3&&g.acidArrows.Pending(7).Sum(e=>e.Dice)==12,"upcast Sphere and Arrow residues coexist with independent clocks and dice");
        g=new TrainingGame();g.dead.Add(7);Run(g.ResolveVitriolicSphere(true,0,4,new UnityEngine.Vector2Int()));g.acidArrows.StartTurn(7);C(g.acidArrows.Pending(7).Count==0,"target killed by initial damage receives no future residue");
        g=new TrainingGame();Run(g.ResolveVitriolicSphere(true,0,3,new UnityEngine.Vector2Int()));C(g.damage.Count==0&&g.checks==0,"invalid slot cannot deal damage");
        var queue=new AcidArrowResidue();C(!queue.CanAddMany(null)&&queue.CanAddMany(new[]{0,-1,7}),"batch capacity preflight validates all targets");C(queue.AddDelayed(0,7,0,"sphere")==null&&queue.AddDelayed(0,7,5,"")==null,"invalid delayed damage is rejected");
        C(IceStormFields.Covers(4,4,8,4)&&!IceStormFields.Covers(4,4,8,5)&&IceStormFields.Covers(4,4,0,4,2),"same exact twenty-foot cell-center circle covers multi-cell bodies once");
        Console.WriteLine("PASS: production Vitriolic Sphere AoE, separate saves, shared damage, self concentration, failed-save-only residue, fixed delayed scaling, next-target-turn clocks and mixed-spell independence");
    }
}
