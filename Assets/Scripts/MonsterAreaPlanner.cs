using System;
public static class MonsterAreaPlanner
{
    // Callbacks use the same footprint distances as the battle map.
    // Equal coverage prefers the nearer centre, then row/column order.
    public static bool Choose(int size,int range,int radius,Func<int,int,int> casterDistance,Func<int,int,int> enemies,out int column,out int row)
    {
        column=row=-1;if(size<1||range<0||radius<0||casterDistance==null||enemies==null)return false;
        int bestCount=0,bestDistance=int.MaxValue;
        for(int y=0;y<size;y++)for(int x=0;x<size;x++){
            int distance=casterDistance(x,y);if(distance<=radius||distance>range)continue;
            int count=enemies(x,y);
            if(count>bestCount||count==bestCount&&count>0&&distance<bestDistance){bestCount=count;bestDistance=distance;column=x;row=y;}
        }
        return bestCount>0;
    }
}
