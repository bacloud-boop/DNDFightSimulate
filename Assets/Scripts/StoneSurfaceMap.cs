using System.Collections.Generic;
using UnityEngine;
public sealed partial class BattleMap
{
    public bool StoneContact(Vector2Int body,int spaces)
    {
        if(!Inside(body,spaces))return false;
        for(int y=body.y;y<body.y+spaces;y++)for(int x=body.x;x<body.x+spaces;x++)if(!IsWater(new Vector2Int(x,y)))return true;
        return false;
    }
    // Land cells share a continuous stone surface only when joined by a land edge.
    public bool SameStoneSurface(Vector2Int a,int aSpaces,Vector2Int b,int bSpaces)
    {
        if(!StoneContact(a,aSpaces)||!StoneContact(b,bSpaces))return false;
        var seen=new bool[Size,Size];var queue=new Queue<Vector2Int>();
        for(int y=a.y;y<a.y+aSpaces;y++)for(int x=a.x;x<a.x+aSpaces;x++){var p=new Vector2Int(x,y);if(!IsWater(p)){seen[x,y]=true;queue.Enqueue(p);}}
        int[] dx={1,-1,0,0},dy={0,0,1,-1};
        while(queue.Count>0){var p=queue.Dequeue();if(p.x>=b.x&&p.y>=b.y&&p.x<b.x+bSpaces&&p.y<b.y+bSpaces)return true;
            for(int i=0;i<4;i++){var next=new Vector2Int(p.x+dx[i],p.y+dy[i]);if(Inside(next,1)&&!seen[next.x,next.y]&&!IsWater(next)){seen[next.x,next.y]=true;queue.Enqueue(next);}}}
        return false;
    }
}
