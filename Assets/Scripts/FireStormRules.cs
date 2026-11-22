using System;
using System.Collections.Generic;
using System.Linq;
public static class FireStormRules
{
    public struct Block : IEquatable<Block>
    {
        public readonly int X,Y;
        public Block(int x,int y){X=x;Y=y;}
        public bool Equals(Block other)=>X==other.X&&Y==other.Y;
        public override bool Equals(object other)=>other is Block&&Equals((Block)other);
        public override int GetHashCode()=>X*397^Y;
        public bool Covers(int x,int y,int spaces=1)=>spaces>0&&(long)X<(long)x+spaces&&(long)X+2>x&&(long)Y<(long)y+spaces&&(long)Y+2>y;
    }
    public struct Body {public int Id,X,Y,Spaces;public Body(int id,int x,int y,int spaces){Id=id;X=x;Y=y;Spaces=spaces;}}
    public static bool Adjacent(Block a,Block b)=>
        (Math.Abs((long)a.X-b.X)==2&&Math.Abs((long)a.Y-b.Y)<2)||(Math.Abs((long)a.Y-b.Y)==2&&Math.Abs((long)a.X-b.X)<2);
    public static bool Valid(IReadOnlyList<Block> blocks,int size,Func<int,int,bool> inRange)
    {
        if(blocks==null||blocks.Count<1||blocks.Count>10||size<2||inRange==null)return false;
        var seen=new HashSet<Block>();foreach(var block in blocks){
            if(block.X<0||block.Y<0||block.X>size-2||block.Y>size-2||!seen.Add(block))return false;
            for(int y=0;y<2;y++)for(int x=0;x<2;x++)if(!inRange(block.X+x,block.Y+y))return false;
        }
        if(blocks.Count==1)return true;
        foreach(var block in blocks)if(!blocks.Any(other=>Adjacent(block,other)))return false;
        return true;
    }
    public static bool Covers(IEnumerable<Block> blocks,int x,int y,int spaces=1)=>blocks.Any(b=>b.Covers(x,y,spaces));
    // Bounded coverage search. Adjacent pairs allow separate groups without
    // adding an isolated block. Every returned plan obeys the same validator.
    public static List<Block> Choose(int size,Body caster,IReadOnlyList<Body> enemies,Func<int,int,bool> range)
    {
        var candidates=new List<Block>();var hits=new List<HashSet<int>>();
        if(size<2||size>20||enemies==null||range==null)return new List<Block>();
        for(int y=0;y<size-1;y++)for(int x=0;x<size-1;x++){
            var block=new Block(x,y);if(block.Covers(caster.X,caster.Y,caster.Spaces)||!Valid(new[]{block},size,range))continue;
            candidates.Add(block);hits.Add(new HashSet<int>(enemies.Where(e=>block.Covers(e.X,e.Y,e.Spaces)).Select(e=>e.Id)));
        }
        var moves=new List<int[]>();for(int i=0;i<candidates.Count;i++){
            moves.Add(new[]{i});for(int j=0;j<i;j++)if(Adjacent(candidates[i],candidates[j]))moves.Add(new[]{j,i});
        }
        var best=new List<Block>();int bestCount=0;
        foreach(int seed in Enumerable.Range(0,candidates.Count).Where(i=>hits[i].Count>0).OrderByDescending(i=>hits[i].Count).ThenBy(i=>(long)(candidates[i].X-caster.X)*(candidates[i].X-caster.X)+(long)(candidates[i].Y-caster.Y)*(candidates[i].Y-caster.Y)).Take(16)){
            var selected=new HashSet<int>{seed};var covered=new HashSet<int>(hits[seed]);
            while(selected.Count<10){int[] next=null;int gain=0,cost=1;
                foreach(var move in moves){var add=move.Where(i=>!selected.Contains(i)).ToArray();if(add.Length==0||selected.Count+add.Length>10)continue;
                    if(add.Length==1&&!selected.Any(i=>Adjacent(candidates[i],candidates[add[0]])))continue;
                    int extra=add.SelectMany(i=>hits[i]).Distinct().Count(id=>!covered.Contains(id));
                    if(extra>0&&(next==null||extra*cost>gain*add.Length||extra*cost==gain*add.Length&&extra>gain)){next=add;gain=extra;cost=add.Length;}
                }
                if(next==null)break;foreach(int i in next){selected.Add(i);covered.UnionWith(hits[i]);}
            }
            foreach(int isolated in selected.ToArray())if(!selected.Any(i=>Adjacent(candidates[i],candidates[isolated]))&&selected.Count>1){
                int neighbor=Enumerable.Range(0,candidates.Count).Where(i=>!selected.Contains(i)&&Adjacent(candidates[i],candidates[isolated])).DefaultIfEmpty(-1).First();
                if(selected.Count<10&&neighbor>=0)selected.Add(neighbor);else selected.Remove(isolated);
            }
            covered=new HashSet<int>(selected.SelectMany(i=>hits[i]));
            var result=selected.OrderBy(i=>i).Select(i=>candidates[i]).ToList();
            if(covered.Count>bestCount&&Valid(result,size,range)){best=result;bestCount=covered.Count;}
        }
        return best;
    }
}
