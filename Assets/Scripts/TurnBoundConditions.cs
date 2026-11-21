using System.Collections.Generic;
// Short conditions keyed by caster, target and condition. They do not replace
// unrelated spells' conditions or enter a repeating-save queue.
public sealed class TurnBoundConditions
{
    sealed class Effect {public int Caster,Target,Condition;public long Begins,EndWorld,EndTurn;}
    readonly List<Effect> effects=new List<Effect>();
    readonly Dictionary<int,long> ended=new Dictionary<int,long>();
    long Turns(int caster)=>ended.ContainsKey(caster)?ended[caster]:0;
    public static bool ValidTime(long now)=>now>=0&&now<=long.MaxValue-2;
    bool Active(Effect e,long now)=>now>=e.Begins&&now<e.EndWorld&&Turns(e.Caster)<e.EndTurn;
    public bool Add(int caster,int target,int condition,long now)
    {
        if(!ValidTime(now)||condition<0||condition>3||Turns(caster)>long.MaxValue-2)return false;
        effects.RemoveAll(e=>e.Caster==caster&&e.Target==target&&e.Condition==condition);
        effects.Add(new Effect{Caster=caster,Target=target,Condition=condition,Begins=now,EndWorld=now+2,EndTurn=Turns(caster)+2});return true;
    }
    public bool Has(int target,int condition,long now)=>effects.Exists(e=>e.Target==target&&e.Condition==condition&&Active(e,now));
    public int End(int target,int condition,long now)=>effects.RemoveAll(e=>e.Target==target&&e.Condition==condition&&Active(e,now));
    public void EndTurn(int caster){long turns=Turns(caster);if(turns<long.MaxValue)ended[caster]=turns+1;effects.RemoveAll(e=>e.Caster==caster&&Turns(caster)>=e.EndTurn);}
    public void Refresh(long now){effects.RemoveAll(e=>now>=e.EndWorld||Turns(e.Caster)>=e.EndTurn);}
    public void Clear(){effects.Clear();ended.Clear();}
}
