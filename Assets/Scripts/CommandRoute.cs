using System;
using System.Collections.Generic;
// Weighted shortest paths on the current board. Planning never moves a real unit.
public static class CommandRoute
{
    public static List<int> Plan(int width,int start,int budget,bool flee,Func<int,int,int> stepCost,Func<int,int> distance,Func<int,bool> canEnd=null)
    {
        var path=new List<int>();if(width<1||start<0||start>=width*width||budget<=0||!flee&&distance(start)<=5)return path;
        int count=width*width;var costs=new long[count];var previous=new int[count];var done=new bool[count];
        for(int i=0;i<count;i++){costs[i]=long.MaxValue;previous[i]=-1;}costs[start]=0;
        for(int pass=0;pass<count;pass++){
            int at=-1;for(int i=0;i<count;i++)if(!done[i]&&costs[i]!=long.MaxValue&&(at<0||costs[i]<costs[at]))at=i;
            if(at<0||flee&&costs[at]>budget)break;done[at]=true;
            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){
                if(dx==0&&dy==0)continue;int x=at%width+dx,y=at/width+dy;if(x<0||y<0||x>=width||y>=width)continue;
                int next=y*width+x,cost=stepCost(at,next);if(cost<1||cost==int.MaxValue)continue;
                long total=costs[at]+cost;if(total<costs[next]){costs[next]=total;previous[next]=at;}
            }
        }
        int best=start;
        for(int i=0;i<count;i++){
            if(costs[i]==long.MaxValue||flee&&costs[i]>budget||canEnd!=null&&!canEnd(i))continue;
            int d=distance(i),old=distance(best);
            if(flee?(d>old||d==old&&costs[i]<costs[best]):
                (d<=5?(old>5||costs[i]<costs[best]):old>5&&(d<old||d==old&&costs[i]<costs[best])))best=i;
        }
        while(best!=start&&best>=0){path.Add(best);best=previous[best];}path.Reverse();
        long spent=0;int from=start,keep=0,index=0;foreach(int to in path){spent+=stepCost(from,to);if(spent>budget)break;index++;if(canEnd==null||canEnd(to))keep=index;from=to;}
        if(keep<path.Count)path.RemoveRange(keep,path.Count-keep);return path;
    }
}
