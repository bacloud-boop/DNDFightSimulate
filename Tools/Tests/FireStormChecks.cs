using System;
using System.Collections.Generic;
using System.Linq;
using B=FireStormRules.Block;
class FireStormChecks
{
    static void C(bool value,string message){if(!value)throw new Exception(message);}
    static bool Range(int x,int y)=>true;
    static void Main()
    {
        C(FireStormRules.Valid(new[]{new B(0,0)},10,Range),"one standalone cube is permitted");
        C(FireStormRules.Valid(new[]{new B(0,0),new B(2,0),new B(6,6),new B(8,6)},10,Range),"two separated edge-connected groups are legal");
        C(!FireStormRules.Valid(new[]{new B(0,0),new B(6,6)},10,Range),"two isolated cubes are illegal even though each would be legal alone");
        C(!FireStormRules.Adjacent(new B(0,0),new B(2,2))&&FireStormRules.Adjacent(new B(0,0),new B(2,1)),"corner-only contact is not an edge; partial shared edges are valid");
        C(!FireStormRules.Valid(new[]{new B(0,0),new B(0,0)},10,Range)&&!FireStormRules.Valid(new[]{new B(9,0)},10,Range),"duplicates and clipped cubes rejected");
        C(!FireStormRules.Valid(new[]{new B(0,0)},10,(x,y)=>x==0),"all four cells must be within casting range");
        var ten=Enumerable.Range(0,10).Select(i=>new B(i*2,0)).ToList();C(FireStormRules.Valid(ten,20,Range),"ten full cubes may form a chain");ten.Add(new B(0,2));C(!FireStormRules.Valid(ten,20,Range),"eleventh cube rejected");
        var overlapping=new[]{new B(0,0),new B(2,0),new B(1,0),new B(3,0)};C(FireStormRules.Valid(overlapping,10,Range)&&FireStormRules.Covers(overlapping,2,0),"overlapping connected regions form one union");
        C(FireStormRules.Covers(new[]{new B(2,2)},1,1,2)&&!FireStormRules.Covers(new[]{new B(2,2)},0,0,2),"large creature is covered if any occupied grid center is inside");
        C(!FireStormRules.Adjacent(new B(int.MinValue,0),new B(int.MaxValue,0)),"extreme adjacency arithmetic cannot overflow");
        var enemies=new[]{new FireStormRules.Body(0,0,0,1),new FireStormRules.Body(1,9,9,1)};var caster=new FireStormRules.Body(-1,5,5,1);
        var plan=FireStormRules.Choose(10,caster,enemies,Range);C(FireStormRules.Valid(plan,10,Range)&&plan.Count<=10&&!FireStormRules.Covers(plan,5,5)&&FireStormRules.Covers(plan,0,0)&&FireStormRules.Covers(plan,9,9),"monster planner can cover separated enemies with legal groups without hitting itself");
        C(FireStormRules.Choose(10,caster,new FireStormRules.Body[0],Range).Count==0&&FireStormRules.Choose(10,caster,enemies,(x,y)=>false).Count==0,"empty or unreachable targets produce no plan for fallback");
        var spell=DirectSpell.Find("Fire Storm");for(int slot=7;slot<=9;slot++)C(spell.DiceCount(1,slot)==7&&spell.Sides==10&&spell.HalfSave&&spell.SaveAbility==1&&!spell.Attack,"seven d10 fire damage does not increase with slots or class level");
        var cycle=new MonsterSpellSequence();C(cycle.Configure(4,1,false,new[]{new MonsterSpellEntry("Fire Storm",7)})&&!cycle.Configure(4,1,false,new[]{new MonsterSpellEntry("Fire Storm",6)})&&cycle.Next.Slot==7,"monster minimum slot validation is atomic");
        var state=new CombatState();state.PlayerUnderwater=true;C(state.AdjustTypedDamage(true,17/2,5)==4,"successful save rounds down before underwater fire resistance");state.DamageImmunity=(player,type)=>type==5;C(state.AdjustTypedDamage(true,17,5)==0,"fire immunity prevents damage");
        Console.WriteLine("PASS: standalone/separated cubes, edge adjacency, invalid drafts, union coverage, multi-cell targets, safe monster planning, slots and save/resistance order");
    }
}
