using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace UnityEngine
{
    public struct Vector2Int {public int x,y;public Vector2Int(int a,int b){x=a;y=b;}}
    public static class Random {public static int Range(int min,int max)=>max-1;}
    public static class GUI {public static bool enabled=true;}
    public static class GUILayout {public static void Label(string text,object style){}public static bool Button(string text,object style)=>false;}
}
public static class BattleMap {public const int RescueId=int.MinValue;public static int BodyDistance(UnityEngine.Vector2Int a,int n,UnityEngine.Vector2Int b,int m)=>Math.Abs(a.x-b.x)*5;}
public sealed partial class TrainingGame
{
    sealed class MapStub {public bool Locked=true;public UnityEngine.Vector2Int Player=new UnityEngine.Vector2Int();public int PlayerSpaces=1;}
    sealed class CombatStub
    {
        public int PlayerHp=10,PlayerMaxHp=100,Hp=100,Actions=1,Restricted;public bool PlayerDead=false,Resist,Immune,HealingBlocked;
        public bool PlayerCanAct=>PlayerHp>0&&!PlayerDead&&Hp>0;public bool MagicActionAvailable=>PlayerCanAct&&Actions>Restricted;
        public bool TryUseMagicAction(){if(!MagicActionAvailable)return false;Actions--;return true;}
        public long[] ResolveTypedPacket(bool player,long[] raw){var result=(long[])raw.Clone();if(Immune)result[8]=0;else if(Resist)result[8]/=2;return result;}
        public int HealPlayer(int value){if(PlayerDead||HealingBlocked)return 0;int before=PlayerHp;PlayerHp=Math.Min(PlayerMaxHp,PlayerHp+value);return PlayerHp-before;}
    }
    sealed class TurnStub {public bool Active=false;}sealed class SpellAttackResult {public bool Hit,Critical,Intercepted;}
    sealed class UnitStub {public bool Resistant=false;public bool ResistsType(int type)=>Resistant;}
    sealed class RayDamage {public long[] Raw;}sealed class ExtraMarkDamage {public int Raw=0;}sealed class MarkStub {public int DamageType=8;}
    readonly MapStub map=new MapStub();readonly CombatStub combat=new CombatStub();readonly TurnStub summonTurns=new TurnStub();readonly MarkStub mark=new MarkStub();readonly UnitStub unit=new UnitStub();
    string page="battle",panel="spells",concentrationSpell="",dice;long worldRounds,concentrationExpiresWorld;int concentrationExpiresRound,directSlotLevel=3,draggingToken=-1,outcomeDeferDepth;
    bool rolling,spellTargeting=false,reactionPending=false,PlayerTurnAllowed=true,hit=true,critical,intercepted,meleeUsed,isolated=false,killSelf;
    int distance=1,seenSlotDamage,seenCaster=-1,lifeBond,damageEvents;object button=new object(),small=new object();readonly List<string> history=new List<string>();
    bool AcidArrowTargetAvailable(int target)=>target==0?!combat.PlayerDead:target==-1?combat.Hp>0:true;
    bool SummonTarget(int target,out UnityEngine.Vector2Int p,out int n,out string name){p=new UnityEngine.Vector2Int(target==0?0:distance,0);n=1;name=target.ToString();return true;}
    bool AcidArrowIsolated(int source,int target)=>isolated;List<int> BreathActors()=>new List<int>{0,-1,7};string TransitActorName(int id)=>id.ToString();UnitStub FindSummon(int id)=>unit;
    IEnumerator RollMappedSpellAttack(int source,int target,int caster,string title,SpellAttackResult result,bool melee=false){meleeUsed=melee;seenCaster=caster;result.Hit=hit;result.Critical=critical;result.Intercepted=intercepted;yield break;}
    IEnumerator PresentRoll(int sides,int[] rolls,string text){seenSlotDamage=rolls.Sum();yield break;}
    IEnumerator RollMarkDamage(bool player,bool crit,ExtraMarkDamage result){yield break;}
    IEnumerator ReduceRayPacket(bool player,long[] raw,bool rolled,string label,RayDamage result){result.Raw=raw;yield break;}
    int ApplyMappedDamagePacket(int target,int source,long[] raw,bool crit){damageEvents++;var resolved=combat.ResolveTypedPacket(target==0,raw);if(target==0){combat.PlayerHp=killSelf?0:Math.Max(0,combat.PlayerHp-(int)resolved[8]);}return (int)resolved[8];}
    sealed class HitRetaliation {}
    HitRetaliation CaptureHitRetaliation(int attacker,bool melee,int? distance=null)=>new HitRetaliation();
    IEnumerator ResolveHitRetaliation(int attacker,HitRetaliation result){yield break;}
    IEnumerator MaintainRetaliationConcentration(HitRetaliation result){yield break;}
    int AgathysRetaliation(bool melee)=>0;int ApplyAgathysRetaliation(int raw,bool player)=>0;
    void RefreshConcentration(){if(combat.PlayerHp<=0){vampiricTouch.Clear();concentrationSpell="";}}
    void ApplySteedLifeBond(int actual,int level){lifeBond+=actual;}
    IEnumerator MaintainConcentration(int damage){yield break;}void RefreshUndeadFollowers(){}void RefreshBestialSpirit(){}void ReconcileFallenRescue(){}void CheckBattleOutcome(){}
    void StartCoroutine(IEnumerator routine)=>Run(routine);
    static void Run(IEnumerator routine){var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){var top=stack.Peek();if(!top.MoveNext()){stack.Pop();continue;}if(top.Current is IEnumerator)stack.Push((IEnumerator)top.Current);}}
    static void C(bool value,string text){if(!value)throw new Exception(text);}
    static TrainingGame Started(int slot=3){var g=new TrainingGame();Run(g.CastVampiricTouch(4,slot,-1));return g;}
    public static void Main()
    {
        for(int slot=3;slot<=9;slot++)C(VampiricTouchEffect.DiceCount(slot)==slot&&VampiricTouchEffect.DiceCount(slot,true)==slot*2,"upcast and critical dice");
        C(VampiricTouchEffect.DiceCount(2)==0&&VampiricTouchEffect.DiceCount(10)==0&&VampiricTouchEffect.Healing(19)==9&&VampiricTouchEffect.Healing(-1)==0,"invalid slot and healing floor");
        var effect=new VampiricTouchEffect();C(!effect.Begin(3,1,long.MaxValue-9)&&effect.Begin(5,4,10)&&effect.Active(19)&&!effect.Active(20),"one minute independent world timer and overflow guard");effect.Clear();C(!effect.Active(10),"concentration end clears attack availability");
        var g=Started();C(g.meleeUsed&&g.seenSlotDamage==18&&g.combat.PlayerHp==19&&g.lifeBond==9&&g.VampiricTouchActive&&g.concentrationExpiresWorld==10,"production initial hit is melee, heals half necrotic, triggers steed life bond and starts concentration");
        g.worldRounds=3;Run(g.RepeatVampiricTouch(7));C(g.combat.Actions==0&&g.seenCaster==4&&g.seenSlotDamage==18&&g.vampiricTouch.Expires==10&&g.outcomeDeferDepth==0&&!g.rolling,"repeat pays a Magic action, uses original caster/slot and cannot extend duration");
        Run(g.RepeatVampiricTouch(-1));C(g.damageEvents==2,"no extra attack when the action is spent");
        g=new TrainingGame();g.hit=false;Run(g.CastVampiricTouch(1,3,-1));C(g.VampiricTouchActive&&g.damageEvents==0&&g.combat.PlayerHp==10,"initial miss does not end ongoing spell or heal");
        g=new TrainingGame();g.critical=true;g.combat.Resist=true;Run(g.CastVampiricTouch(1,4,-1));C(g.seenSlotDamage==48&&g.combat.PlayerHp==22,"critical dice double before necrotic resistance and half-healing");
        g=new TrainingGame();g.hit=false;g.intercepted=true;Run(g.CastVampiricTouch(1,3,0));C(g.damageEvents==0&&g.combat.PlayerHp==10,"mirror interception cannot cause damage or healing");
        g=Started(5);g.unit.Resistant=true;Run(g.RepeatVampiricTouch(7));C(g.combat.PlayerHp==32&&g.panel==""&&g.concentrationExpiresRound==int.MaxValue,"summon necrotic resistance halves healing, repeat closes panel and expiry uses world time");
        g=new TrainingGame();g.combat.PlayerHp=98;Run(g.CastVampiricTouch(1,3,-1));C(g.combat.PlayerHp==100&&g.lifeBond==2,"healing clamps to max HP and bond uses actual healing only");
        g=new TrainingGame();g.combat.Immune=true;Run(g.CastVampiricTouch(1,3,-1));C(g.combat.PlayerHp==10,"necrotic immunity prevents all healing");
        g=Started();g.worldRounds=10;Run(g.RepeatVampiricTouch(-1));C(g.damageEvents==1&&g.combat.Actions==1,"expired effect cannot consume action or attack");
        g=Started();g.combat.Restricted=1;Run(g.RepeatVampiricTouch(-1));C(g.damageEvents==1&&g.combat.Actions==1,"restricted Action Surge cannot fund a Magic action");
        g=Started();g.distance=2;Run(g.RepeatVampiricTouch(-1));C(g.damageEvents==1&&g.combat.Actions==1,"out-of-reach target does not spend action");g.distance=1;g.isolated=true;Run(g.RepeatVampiricTouch(-1));C(g.damageEvents==1,"swallow boundary blocks touch");
        g=new TrainingGame();g.killSelf=true;Run(g.CastVampiricTouch(1,3,0));C(g.combat.PlayerHp==9&&!g.VampiricTouchActive,"becoming incapacitated ends concentration before immediate self healing restores consciousness");
        g=new TrainingGame();g.combat.HealingBlocked=true;Run(g.CastVampiricTouch(1,3,-1));C(g.damageEvents==1&&g.combat.PlayerHp==10&&g.lifeBond==0,"blocked healing does not prevent attack damage and cannot trigger bond healing");
        Console.WriteLine("PASS: production Vampiric Touch initial and repeated melee attacks, caster/slot snapshot, missed first cast, critical/resistance/immunity healing, resource isolation, range, expiry and incapacitation");
    }
}
