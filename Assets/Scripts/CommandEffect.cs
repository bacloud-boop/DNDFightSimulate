using System;
using System.Collections.Generic;

public enum CommandWord {Approach,Drop,Flee,Grovel,Halt}
// Each casting waits for the target's next turn. Simultaneous copies don't stack.
public sealed class CommandEffect
{
    public static readonly string[] Names={"接近","丢弃","逃跑","趴下","站住"};
    public sealed class Order {public int Caster,Target;public long Dc,Sequence;public CommandWord Word;}
    readonly List<Order> orders=new List<Order>();long sequence;
    public bool Add(int caster,int target,long dc,CommandWord word)
    {
        if((int)word<0||(int)word>=Names.Length)return false;
        orders.Add(new Order{Caster=caster,Target=target,Dc=dc,Word=word,Sequence=++sequence});return true;
    }
    public Order Next(int target)
    {
        Order best=null;foreach(var order in orders)if(order.Target==target&&(best==null||order.Dc>best.Dc||order.Dc==best.Dc&&order.Sequence>best.Sequence))best=order;
        return best;
    }
    public Order Take(int target){var result=Next(target);orders.RemoveAll(o=>o.Target==target);return result;}
    public void Remove(int target){orders.RemoveAll(o=>o.Target==target);}
    public void Clear(){orders.Clear();sequence=0;}
    public static int TargetLimit(int slot)=>Math.Max(1,Math.Min(9,slot));
}
