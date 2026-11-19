using System.Collections.Generic;

// One follower phase follows the caster's turn. UI selection must not restart it.
public sealed class SummonTurnQueue
{
    readonly List<SummonedUnit> roster=new List<SummonedUnit>();
    long lastSequence=-1;
    int index;
    public SummonedUnit Current {get;private set;}
    public bool Active=>Current!=null;
    public bool Begin(IReadOnlyList<SummonedUnit> units,long sequence,bool aftermath=false)
    {
        if(aftermath||Active||units==null||sequence<=lastSequence)return false;
        var next=new List<SummonedUnit>();var ids=new HashSet<int>();
        foreach(var unit in units){
            if(unit==null||!unit.Alive)continue;
            if(unit.TurnActive||!ids.Add(unit.Id))return false;
            next.Add(unit);
        }
        roster.Clear();roster.AddRange(next);index=0;lastSequence=sequence;
        Advance();return true;
    }
    void Advance()
    {
        Current=null;
        while(index<roster.Count){var unit=roster[index++];if(unit.BeginTurn(lastSequence)){Current=unit;return;}}
        roster.Clear();
    }
    public bool EndCurrent()
    {
        if(Current==null)return false;
        Current.EndTurn();Advance();return true;
    }
    public void Reconcile(){if(Current!=null&&!Current.Alive){Current.EndTurn();Advance();}}
    public void Cancel(){if(Current!=null)Current.EndTurn();Current=null;roster.Clear();}
    public void Reset(){Cancel();lastSequence=-1;}
}
