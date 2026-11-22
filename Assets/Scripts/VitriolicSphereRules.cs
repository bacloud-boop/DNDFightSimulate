using System;
public static class VitriolicSphereRules
{
    public static int InitialDice(int slot)=>slot>=4&&slot<=9?10+2*(slot-4):0;
    public static int LaterDice(int slot)=>InitialDice(slot)>0?5:0;
    public static long[] Packet(int rolled,bool saved){var result=new long[13];result[3]=Math.Max(0,rolled)/(saved?2:1);return result;}
}
