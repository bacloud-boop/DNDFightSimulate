using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace UnityEngine
{
    public static class Mathf {public static int Max(int a,int b)=>Math.Max(a,b);}
    public static class GUILayout
    {
        public static void Label(string s,object style){}public static bool Toggle(bool b,string s,object style)=>b;
        public static void BeginHorizontal(){}public static void EndHorizontal(){}public static bool Button(string s,object style)=>false;
    }
}
// Execute the production priority coroutine against real CombatState and spell
// sequence models; only animations, target planning and individual actions are stubbed.
public sealed partial class TrainingGame
{
    sealed class SpeciesStub {public int BonusPriority=0;}
    sealed class MapStub {public bool Locked=true;public int Victim;public int NearestEnemyInReach(bool alive,int reach)=>Victim;}
    sealed class MonsterSpellTurn {public bool Attempted,Cast,Bonus;}
    CombatState combat=new CombatState();SpeciesStub monsterSpecies=new SpeciesStub();MapStub map=new MapStub();
    MonsterSpellSequence monsterSpells=new MonsterSpellSequence();bool rolling;string page="battle";
    object small=new object(),heading=new object(),label=new object(),button=new object();
    int MonsterAttackReach=>5;bool MonsterSpellReady=true;
    bool speciesAction,speciesBonus,granted,drop,invalidSpell,killOnMove;
    int attacks,breaths,spells,raceActions,raceBonuses;readonly List<string> log=new List<string>();
    bool MonsterCanCastAttack=>!drop;
    bool BreathReady(int actor)=>false;
    bool DragonBreathReady(int actor)=>granted&&combat.MonsterCanAct&&combat.MonsterActionAvailable;
    bool TryPlanDragonBreath(int actor,out BreathTemplate plan)=>BreathTemplate.TryCreate(0,0,1,0,1,5,0,false,out plan);
    IEnumerator ChasePlayer(int? reach=null,bool needsWeapon=true){log.Add("move:"+needsWeapon);if(needsWeapon)drop=false;if(killOnMove)combat.KillMonster();yield break;}
    IEnumerator TryMonsterSpeciesMagic(bool before,bool unified){if(speciesAction&&combat.TryUseMonsterAction()){raceActions++;log.Add("species action");}yield break;}
    IEnumerator TryMonsterSpeciesBonus(bool before,bool unified){if(speciesBonus&&combat.TryUseMonsterBonusAction()){raceBonuses++;log.Add("species bonus");}yield break;}
    IEnumerator TryMonsterBreath(bool before,bool unified){yield break;}
    IEnumerator TryMonsterTurnSpell(MonsterSpellTurn result){var entry=monsterSpells.Take();if(entry==null)yield break;result.Attempted=true;if(invalidSpell)yield break;var s=DirectSpell.Find(entry.Name);if(s.Bonus?combat.TryUseMonsterBonusAction():combat.TryUseMonsterAction()){spells++;result.Cast=true;result.Bonus=s.Bonus;log.Add("spell");}yield break;}
    IEnumerator UseDragonBreath(int actor,BreathTemplate plan,bool automatic){if(combat.TryUseMonsterAction()){breaths++;log.Add("granted breath");}yield break;}
    object FindSummon(int id)=>new object();
    IEnumerator ResolveMonsterSpiritAttack(object unit){attacks++;log.Add("summon attacked");yield break;}
    IEnumerator Counterattack(bool endTurn,bool priorityAction){if(endTurn||!priorityAction)throw new Exception("priority attack accidentally advances the round or uses reaction mode");attacks++;log.Add("custom attack");yield break;}
    static void Run(IEnumerator routine){var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){var top=stack.Peek();if(!top.MoveNext()){stack.Pop();continue;}var nested=top.Current as IEnumerator;if(nested!=null)stack.Push(nested);}}
    static void C(bool value,string message){if(!value)throw new Exception(message);}
    static TrainingGame New(string first="Fire Bolt",string second="Sacred Flame")
    {
        var g=new TrainingGame();g.monsterSpells.Configure(4,1,false,new[]{new MonsterSpellEntry(first,MonsterSpellSequence.BaseLevel(first)),new MonsterSpellEntry(second,MonsterSpellSequence.BaseLevel(second))});return g;
    }
    public static void Main()
    {
        var g=New();g.granted=true;Run(g.RunMonsterPriorityTurn());C(g.spells==1&&g.breaths==0&&g.attacks==0&&g.monsterSpells.Cursor==1&&g.rolling,"default spell priority spends the only action once");
        g=New();g.granted=true;g.monsterActions.Configure(new[]{2,1,0,3},new[]{true,true,true,true});Run(g.RunMonsterPriorityTurn());C(g.breaths==1&&g.spells==0&&g.attacks==0&&g.monsterSpells.Cursor==0,"breath-first order preserves the unattempted spell cursor and prevents extra attacks");
        g=New("Healing Word");g.granted=true;g.speciesBonus=true;g.monsterActions.Configure(new[]{2,1,0,3},new[]{true,true,true,true});Run(g.RunMonsterPriorityTurn());C(g.breaths==1&&g.spells==1&&g.raceBonuses==0,"a bonus-action spell remains legal after an action breath; lower bonus ability cannot double spend");
        g=New();g.drop=true;g.granted=true;g.monsterActions.Configure(new[]{3,2,1,0},new[]{true,true,true,true});Run(g.RunMonsterPriorityTurn());C(g.attacks==1&&g.breaths==0&&g.spells==0&&!g.drop&&g.log.Contains("move:True"),"custom attack priority recovers its dropped weapon before readiness check and consumes its action");
        g=New();g.invalidSpell=true;g.granted=true;Run(g.RunMonsterPriorityTurn());C(g.spells==0&&g.monsterSpells.Cursor==1&&g.breaths==1&&g.attacks==0,"invalid spell advances exactly once and falls through to granted breath without spending an action");
        g=New();g.monsterActions.SetEnabled(1,false);g.monsterActions.SetEnabled(2,false);g.speciesBonus=true;Run(g.RunMonsterPriorityTurn());C(g.raceBonuses==1&&g.attacks==1&&g.monsterSpells.Cursor==0,"disabled spell category does not advance sequence; species bonus and custom action coexist");
        g=New();g.speciesAction=true;g.speciesBonus=true;Run(g.RunMonsterPriorityTurn());C(g.raceActions==1&&g.raceBonuses==1&&g.spells==0&&g.attacks==0,"species group shares the same action and bonus budgets");
        g=New();for(int i=0;i<4;i++)g.monsterActions.SetEnabled(i,false);Run(g.RunMonsterPriorityTurn());C(g.combat.MonsterActionAvailable&&g.combat.MonsterBonusActionAvailable&&g.log.Count==0,"disabled categories cause no movement or action expenditure");
        g=New();g.killOnMove=true;g.granted=true;Run(g.RunMonsterPriorityTurn());C(g.attacks==0&&g.breaths==0&&g.combat.Hp==0,"death while moving prevents all later action categories");
        g=New();g.map.Victim=4;g.monsterActions.SetEnabled(1,false);Run(g.RunMonsterPriorityTurn());C(g.attacks==1&&g.log.Contains("summon attacked"),"custom attack still targets nearest summon instead of always attacking player");
        Console.WriteLine("PASS: production monster priority coroutine, action/bonus isolation, ordered fallback, spell cursor retention, dropped weapon recovery, target selection, category disable and movement death");
    }
}
