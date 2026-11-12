using System.Collections.Generic;
public sealed class AidEffects
{
    sealed class Entry {public int amount;public long end;}
    readonly List<Entry>[] effects={new List<Entry>(),new List<Entry>()};
    public void Add(bool player,int slot,long now){if(slot<2||slot>9)throw new System.ArgumentOutOfRangeException(nameof(slot));effects[player?1:0].Add(new Entry{amount=5*(slot-1),end=now+4800});}
    public int ActiveBonus(bool player,long now){var list=effects[player?1:0];list.RemoveAll(x=>now>=x.end);int best=0;foreach(var entry in list)if(entry.amount>best)best=entry.amount;return best;}
    public void Clear(){foreach(var list in effects)list.Clear();}
}
