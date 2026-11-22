using System;
using System.Collections.Generic;
using UnityEngine;
class DissonantWhispersChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
    static List<int> Plan(BattleMap map,int actor,Vector2Int source,int sourceSpaces,int speed,Func<int,bool> safe)
    {
        Vector2Int from;int n;C(map.MovementBody(actor,out from,out n),"actor exists");int w=map.Size;
        return DissonantWhispersRules.SafeRoute(w,from.y*w+from.x,speed,(a,b)=>map.CommandStepCost(actor,V(a%w,a/w),V(b%w,b/w)),i=>BattleMap.BodyDistance(V(i%w,i/w),n,source,sourceSpaces),safe,i=>map.CommandCanEnd(actor,V(i%w,i/w)));
    }
    static void Main()
    {
        var spell=DirectSpell.Find("Dissonant Whispers");C(spell!=null&&spell.Level==1&&spell.Range==60&&spell.SaveAbility==3&&spell.HalfSave&&!spell.Attack&&!spell.Bonus&&spell.DamageType=="心灵","2024 spell uses one Magic action and Wisdom, never an attack or Frightened");
        for(int slot=1;slot<=9;slot++)C(DissonantWhispersRules.DiceCount(slot)==slot+2&&spell.DiceCount(1,slot)==slot+2,"3d6 initial damage, +1d6 per higher slot");
        C(DissonantWhispersRules.DiceCount(0)==0&&DissonantWhispersRules.DiceCount(10)==0&&DissonantWhispersRules.AfterSave(19,true)==9,"invalid slots rejected and successful save floors half damage");
        var sequence=new MonsterSpellSequence();C(sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Dissonant Whispers",1)})&&!sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Dissonant Whispers",0)}),"monster rotation validates first-level minimum");
        var combat=new CombatState();C(combat.TryUseMagicAction()&&!combat.MagicActionAvailable&&combat.BonusActionAvailable,"cast spends one action, no bonus action");
        C(combat.TryUseCompelledReaction(false)&&!combat.TryUseCompelledReaction(false)&&combat.MonsterActionAvailable,"compelled retreat spends exactly one reaction, no monster action");
        combat.StartMonsterTurn(false);C(combat.TryUseCompelledReaction(false),"reaction refreshes at target next turn");
        combat.BlockReactions(true);C(!combat.TryUseCompelledReaction(true),"reaction blocking prevents retreat");
        var helpless=new CombatState();helpless.ApplyPlayerDamage(helpless.PlayerMaxHp);C(!helpless.PlayerDead&&!helpless.TryUseCompelledReaction(true),"a surviving unconscious target cannot take a reaction");
        var budget=new ReactionMovementBudget();C(budget.Spend(7,30)&&budget.Spend(7,30)&&budget.Remaining(30)==16&&!budget.Spend(17,30),"reaction movement tracks cumulative five/seven-foot route costs independently");
        C(budget.Remaining(10)==0&&!budget.Spend(1,10)&&budget.Remaining(int.MinValue)==0,"a speed reduction never grants extra movement or overflows");
        var map=new BattleMap();map.Place(true,V(3,3));map.Place(false,V(1,3));map.Lock();map.SpendPlayerMovement(30);map.GrantTacticalMovement();map.StartMonsterMovementTurn(0);
        int observed=0;map.MovementObserved=(a,b,c,n,m,landing)=>{C(!landing,"reaction movement is traversed ground, not teleportation");observed++;};
        var original=map.Player;var roundStart=map.RoundStartPlayer;int spent=map.MovementSpent,tactical=map.TacticalMovementRemaining;
        var route=Plan(map,0,map.Monster,1,30,i=>i%10!=5);C(route.Count>0&&map.Player==original&&observed==0,"path preview is pure");
        int travelled=0;var previous=original;foreach(int i in route){var p=V(i%10,i/10);C(p.x!=5,"dangerous full-column barrier cannot be crossed even to reach farther safe cells");travelled+=map.CommandStepCost(0,previous,p);C(map.MoveReactionStep(0,p),"reaction step can move despite exhausted ordinary movement");previous=p;}
        C(travelled<=30&&map.Player.x<5&&map.MovementRemaining==0&&map.MovementSpent==spent&&map.TacticalMovementRemaining==tactical&&map.RoundStartPlayer==roundStart&&observed==route.Count,"reaction traversal preserves all turn budgets and emits each real terrain entry once");
        C(Plan(map,0,map.Monster,1,30,i=>false).Count==0,"no safe route leaves target in place");
        C(!map.CanReactionStep(0,V(int.MaxValue,int.MinValue))&&!map.MoveReactionStep(0,V(9,9)),"cannot teleport or overflow from invalid coordinates");
        map.CreatureIncapacitated=id=>id==0;C(!map.MoveReactionStep(0,V(map.Player.x,map.Player.y+1)),"incapacitation between reaction and commit stops movement");
        var monster=new BattleMap();monster.Place(false,V(4,4));monster.Lock();monster.StartMonsterMovementTurn(0);C(monster.MoveReactionStep(-1,V(5,4))&&monster.MonsterMovementRemaining==0,"monster retreat does not grant or spend turn movement");
        var summons=new BattleMap();summons.Lock();var unit=new SummonedUnit(7,"spirit",20,10,30,2,1);C(summons.AddSummon(unit,V(4,4)),"summon fixture");int starts=0;unit.TurnStarted=()=>starts++;
        C(!unit.TurnActive&&unit.MovementRemaining==0&&summons.MoveReactionStep(7,V(5,4))&&!unit.TurnActive&&!unit.ActionAvailable&&unit.MovementRemaining==0&&starts==0,"off-turn summon movement creates no turn, action, reaction refresh or movement pool");
        var mounted=new BattleMap();mounted.Lock();var steed=new SummonedUnit(8,"mount",30,12,60,3,1);C(mounted.AddSummon(steed,V(1,0))&&mounted.MountSummon(8),"mounted fixture");
        int events=0;mounted.MovementObserved=(a,b,c,n,m,l)=>events++;var old=mounted.Player;Vector2Int dismount=V(0,0);C(mounted.CanReactionDismountAt(dismount),"legal rider landing");
        C(mounted.PreviewReactionDismount(dismount,()=>!mounted.Riding&&mounted.Player==dismount)&&mounted.Riding&&mounted.Player==old&&events==0,"dismount route preview restores rider, mount and events");
        C(mounted.ReactionDismountAt(dismount)&&!mounted.Riding&&steed.X==1&&steed.Y==0&&events==1,"reaction dismount moves only rider, not mount");
        Console.WriteLine("PASS: Dissonant Whispers 2024 damage, safe weighted routes, danger barriers, target reaction spending, off-turn independent movement, pure previews, incapacity and mounted dismount");
    }
}
