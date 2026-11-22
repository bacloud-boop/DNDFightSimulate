using System;
using System.Collections.Generic;
public enum MonsterActionKind {Species,SpellCycle,DragonBreath,CustomAttack}
public sealed class MonsterActionPolicy
{
    public static readonly string[] Names={"种族主动特性","法术循环","龙息术喷吐","自定义攻击"};
    int[] order={0,1,2,3};bool[] enabled={true,true,true,true};
    public IReadOnlyList<int> Order=>Array.AsReadOnly(order);
    public bool Enabled(int kind)=>kind>=0&&kind<4&&enabled[kind];
    public bool Configure(IReadOnlyList<int> priorities,IReadOnlyList<bool> allowed)
    {
        if(priorities==null||allowed==null||priorities.Count!=4||allowed.Count!=4)return false;
        var seen=new HashSet<int>();var copy=new int[4];var flags=new bool[4];
        for(int i=0;i<4;i++){if(priorities[i]<0||priorities[i]>3||!seen.Add(priorities[i]))return false;copy[i]=priorities[i];flags[i]=allowed[i];}
        order=copy;enabled=flags;return true;
    }
    public bool Move(int at,int offset){int next=at+offset;if(at<0||at>3||next<0||next>3)return false;int old=order[at];order[at]=order[next];order[next]=old;return true;}
    public void SetEnabled(int kind,bool value){if(kind>=0&&kind<4)enabled[kind]=value;}
    public MonsterActionPolicy Copy(){var copy=new MonsterActionPolicy();copy.Configure(order,enabled);return copy;}
}
