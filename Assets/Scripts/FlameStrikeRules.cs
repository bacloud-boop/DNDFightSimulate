using System;
public static class FlameStrikeRules
{
    public static int DicePerType(int slot)=>slot>=5&&slot<=9?slot:0;
    public static bool Covers(int cx,int cy,int x,int y,int spaces=1)
    {
        if(spaces<1||spaces>4)return false;
        long dx=Math.Max(0,Math.Max((long)cx-x-spaces+1,(long)x-cx)),dy=Math.Max(0,Math.Max((long)cy-y-spaces+1,(long)y-cy));
        return dx<=2&&dy<=2&&dx*dx+dy*dy<=4;
    }
    public static long[] Packet(int fire,int radiant){var raw=new long[13];raw[5]=Math.Max(0,fire);raw[11]=Math.Max(0,radiant);return raw;}
}
