using System;
public sealed class MirrorImageEffect {
 public int Remaining {get;private set;}long end=-1;
 public void Cast(long now){Remaining=3;end=now+10;}
 public int Count(long now)=>now<end?Remaining:0;
 public bool Intercept(long now,int[] rolls){int count=Count(now);if(count==0)return false;if(rolls.Length!=count)throw new ArgumentException("One die per duplicate");bool blocked=false;foreach(int roll in rolls){if(roll<1||roll>6)throw new ArgumentOutOfRangeException();blocked|=roll>=3;}if(blocked)Remaining--;return blocked;}
 public void Clear(){Remaining=0;end=-1;}
}
