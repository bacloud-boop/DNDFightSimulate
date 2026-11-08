using System;
using System.Text;
public sealed class MonsterDamage
{
    public static readonly int[] Sides = {4,6,8,10,12};
    public readonly int[] Counts = new int[5];
    public int Bonus;
    public int Roll(out string details, bool critical = false, Action<int,int[],int> observe = null)
    {
        long total=Bonus; var text=new StringBuilder();
        for(int i=0;i<5;i++) {
            long subtotal=0;
            int count=Counts[i]*(critical?2:1);
            int[] preview=new int[Math.Min(count,2)];
            for(int n=0;n<count;n++) {
                int value=UnityEngine.Random.Range(1,Sides[i]+1);subtotal+=value;
                if(n<preview.Length)preview[n]=value;
            }
            if(count>0)observe?.Invoke(Sides[i],preview,count);
            total+=subtotal;
            if(count>0) text.Append(count+"d"+Sides[i]+"="+subtotal+"，");
        }
        int damage=(int)Math.Max(0L,Math.Min(int.MaxValue,total));
        details=text+"固定加值 "+Bonus+"，总计 "+total+"，伤害 "+damage;
        return damage;
    }
}
