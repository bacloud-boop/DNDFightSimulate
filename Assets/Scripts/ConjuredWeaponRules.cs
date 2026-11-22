using System;
using System.Collections.Generic;
public static class ConjuredWeaponRules
{
    public const int ConeFeet=60,VolleyRadius=40;
    public static int DiceCount(bool volley,int slot)=>slot<(volley?5:3)||slot>9?0:volley?8:slot+2;
    public static bool CylinderCell(int cx,int cy,int x,int y,int spaces=1)
    {
        if(spaces<1||spaces>4)return false;
        long dx=Math.Max(0,Math.Max((long)cx-x-spaces+1,(long)x-cx)),dy=Math.Max(0,Math.Max((long)cy-y-spaces+1,(long)y-cy));
        return dx<=8&&dy<=8&&dx*dx+dy*dy<=64;
    }
    public static List<int> Select(IEnumerable<int> covered,ISet<int> selected,Func<int,bool> visible)
    {
        var result=new List<int>();var seen=new HashSet<int>();
        foreach(int id in covered)if(seen.Add(id)&&selected.Contains(id)&&visible(id))result.Add(id);
        return result;
    }
}
