using System.Collections.Generic;

// Sources are permanent until explicitly removed. Suppressed castings remain
// attached so that removing a stronger source restores the next valid one.
public sealed class ContinualFlameEffect
{
    public sealed class Source
    {
        public int ObjectId {get;private set;}
        public int Slot {get;private set;}
        public int Group {get;private set;}
        internal Source(int objectId,int slot,int group){ObjectId=objectId;Slot=slot;Group=group;}
    }
    readonly List<Source> sources=new List<Source>();
    long nextGroup=5;
    public bool Cast(int objectId,int slot,out Source source)
    {
        source=null;
        if(objectId<1||slot<2||slot>9||nextGroup>int.MaxValue)return false;
        source=new Source(objectId,slot,(int)nextGroup);
        sources.Add(source);nextGroup++;return true;
    }
    public Source Effective(int objectId)
    {
        Source best=null;
        foreach(var source in sources)if(source.ObjectId==objectId&&(best==null||source.Slot>best.Slot||source.Slot==best.Slot&&source.Group>best.Group))best=source;
        return best;
    }
    public Source[] All=>sources.ToArray();
    public int[] Objects
    {
        get{
            var seen=new HashSet<int>();var objects=new List<int>();
            foreach(var source in sources)if(seen.Add(source.ObjectId))objects.Add(source.ObjectId);
            return objects.ToArray();
        }
    }
    public bool Remove(int group)
    {
        for(int i=0;i<sources.Count;i++)if(sources[i].Group==group){sources.RemoveAt(i);return true;}
        return false;
    }
    // The runtime clears its Vision groups before resetting this allocator.
    public void Clear(){sources.Clear();nextGroup=5;}
}
