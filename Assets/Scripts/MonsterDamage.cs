using System;
using System.Text;
public sealed class MonsterDamage
{
    public static readonly int[] Sides = {4,6,8,10,12};
    public readonly int[] Counts = new int[5];
    public int Bonus;
    public int Roll(out string details)
    {
        long total=Bonus; var text=new StringBuilder();
        for(int i=0;i<5;i++) {
            long subtotal=0;
            for(int n=0;n<Counts[i];n++) subtotal+=UnityEngine.Random.Range(1,Sides[i]+1);
            total+=subtotal;
            if(Counts[i]>0) text.Append(Counts[i]+"d"+Sides[i]+"="+subtotal+"，");
        }
        int damage=(int)Math.Max(0L,Math.Min(int.MaxValue,total));
        details=text+"固定加值 "+Bonus+"，总计 "+total+"，伤害 "+damage;
        return damage;
    }
}
