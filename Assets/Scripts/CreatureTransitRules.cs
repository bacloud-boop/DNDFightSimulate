using System;
public static class CreatureTransitRules
{
    public static bool CanPass(int moverSize,int otherSize,bool allied,bool incapacitated,bool nimble)
        =>allied||incapacitated||otherSize==0||Math.Abs(moverSize-otherSize)>=2||nimble&&otherSize>moverSize;
    public static bool SharedTurnProne(int moverSize,int otherSize)=>moverSize!=0&&moverSize<=otherSize;
}
