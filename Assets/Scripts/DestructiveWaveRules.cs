using System;
public static class DestructiveWaveRules
{
    public static long[] Packet(int thunder,int other,bool necrotic)
    {
        if(thunder<0||other<0)throw new ArgumentOutOfRangeException();
        var raw=new long[DamageTypes.Names.Length];raw[12]=thunder;raw[necrotic?8:11]=other;return raw;
    }
    public static long[] AfterSave(long[] raw,bool succeeded)
    {
        if(raw==null||raw.Length!=DamageTypes.Names.Length)throw new ArgumentException();
        var copy=(long[])raw.Clone();for(int i=0;i<copy.Length;i++){copy[i]=Math.Max(0,copy[i]);if(succeeded)copy[i]/=2;}return copy;
    }
}
