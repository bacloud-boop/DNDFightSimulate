using System;
using System.Collections.Generic;
public sealed class CordonArrowsEffect
{
    public sealed class Arrow {public readonly int X,Y;public bool Spent {get;internal set;}public Arrow(int x,int y){X=x;Y=y;}}
    public sealed class Field
    {
        public long Id,Dc,Begins,Expires;public int Scene,Source,Slot;public bool Bolts;
        public Arrow[] Arrows;public readonly HashSet<int> Ignored=new HashSet<int>();
        internal readonly Dictionary<int,long> Moves=new Dictionary<int,long>(),Ends=new Dictionary<int,long>();
        public int Remaining {get{int n=0;foreach(var arrow in Arrows)if(!arrow.Spent)n++;return n;}}
        public bool Active(long now)=>now>=Begins&&now<Expires&&Remaining>0;
    }
    readonly List<Field> fields=new List<Field>();long sequence;
    public IReadOnlyList<Field> Fields=>fields.AsReadOnly();
    public static int Limit(int slot)=>slot>=2&&slot<=9?4+2*(slot-2):0;
    public bool CanAdd(long now)=>now>=0&&now<=long.MaxValue-4800&&sequence<long.MaxValue;
    public Field Add(IEnumerable<Arrow> planted,int slot,long dc,int source,int scene,IEnumerable<int> ignored,long now,bool bolts=false)
    {
        var arrows=new List<Arrow>();foreach(var arrow in planted)arrows.Add(new Arrow(arrow.X,arrow.Y));if(!CanAdd(now)||arrows.Count<1||arrows.Count>Limit(slot))return null;
        var field=new Field{Id=++sequence,Dc=dc,Source=source,Scene=scene,Slot=slot,Begins=now,Expires=now+4800,Arrows=arrows.ToArray(),Bolts=bolts};
        foreach(int actor in ignored)field.Ignored.Add(actor);field.Ignored.Add(source);fields.Add(field);return field;
    }
    // Match the map's rounded nearest occupied-cell distance, including multi-cell bodies.
    public static bool InRange(Arrow arrow,int x,int y,int spaces)
    {
        if(spaces<1)return false;long dx=arrow.X<x?(long)x-arrow.X:arrow.X>=(long)x+spaces?(long)arrow.X-x-spaces+1:0;
        long dy=arrow.Y<y?(long)y-arrow.Y:arrow.Y>=(long)y+spaces?(long)arrow.Y-y-spaces+1:0;
        return Math.Round(Math.Sqrt((double)dx*dx+(double)dy*dy)*5)<=30;
    }
    public static int[] Available(Field field,int x,int y,int spaces)
    {
        var indices=new List<int>();for(int i=0;i<field.Arrows.Length;i++)if(!field.Arrows[i].Spent&&InRange(field.Arrows[i],x,y,spaces))indices.Add(i);return indices.ToArray();
    }
    public bool Covers(Field field,int actor,int x,int y,int spaces,int scene,long now)=>field.Scene==scene&&!field.Ignored.Contains(actor)&&field.Active(now)&&Available(field,x,y,spaces).Length>0;
    public Field Strongest(int actor,int x,int y,int spaces,int scene,long now)
    {
        Field best=null;foreach(var field in fields)if(Covers(field,actor,x,y,spaces,scene,now)&&(best==null||field.Dc>best.Dc||field.Dc==best.Dc&&field.Id>best.Id))best=field;return best;
    }
    Field Claim(int actor,int x,int y,int spaces,int scene,long turn,long now,bool end)
    {
        if(turn<0)return null;var best=Strongest(actor,x,y,spaces,scene,now);bool ready=false;
        // Suppressed casts still observe the creature's first movement/end on
        // this turn, but only the strongest cast spends an arrow.
        foreach(var field in fields)if(Covers(field,actor,x,y,spaces,scene,now)){
            var ledger=end?field.Ends:field.Moves;long old;bool fresh=!ledger.TryGetValue(actor,out old)||old!=turn;ledger[actor]=turn;if(field==best)ready=fresh;
        }
        return ready?best:null;
    }
    public Field ClaimMovement(int actor,int x,int y,int spaces,int scene,long turn,long now)=>Claim(actor,x,y,spaces,scene,turn,now,false);
    public Field ClaimEnd(int actor,int x,int y,int spaces,int scene,long turn,long now)=>Claim(actor,x,y,spaces,scene,turn,now,true);
    public bool EndAvailable(int actor,int x,int y,int spaces,int scene,long turn,long now){var field=Strongest(actor,x,y,spaces,scene,now);long previous;return field!=null&&(!field.Ends.TryGetValue(actor,out previous)||previous!=turn);}
    public static bool Consume(Field field,int index){if(index<0||index>=field.Arrows.Length||field.Arrows[index].Spent)return false;field.Arrows[index].Spent=true;return true;}
    public void Refresh(long now){fields.RemoveAll(f=>now>=f.Expires||f.Remaining==0);}
    public void Clear(){fields.Clear();sequence=0;}
}
