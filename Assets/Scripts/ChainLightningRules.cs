using System;
using System.Collections.Generic;
public static class ChainLightningRules
{
    public static int SecondaryLimit(int slot){if(slot<6||slot>9)throw new ArgumentOutOfRangeException(nameof(slot));return slot-3;}
    public static bool Valid(int first,IEnumerable<int> secondary,int slot,Func<int,bool> alive,Func<int,bool> visible,Func<int,int,int> distance)
    {
        if(slot<6||slot>9||secondary==null||alive==null||visible==null||distance==null||!alive(first)||!visible(first)||distance(0,first)>150)return false;
        var ids=new HashSet<int>{first};int count=0;
        foreach(int id in secondary)if(!ids.Add(id)||!alive(id)||distance(first,id)>30||++count>SecondaryLimit(slot))return false;
        return true;
    }
}
