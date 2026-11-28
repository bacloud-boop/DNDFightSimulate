using System;
// A fixed route's existing rounded distance is conserved when it is paid in sections.
public sealed class RouteCostMeter
{
 readonly int distance,total;readonly bool crawling;
 int walked,swum,rough,wind,paid;
 public RouteCostMeter(int roundedDistance,int totalSegmentWeight,bool crawl){if(roundedDistance<0||totalSegmentWeight<1)throw new ArgumentException();distance=roundedDistance;total=totalSegmentWeight;crawling=crawl;}
 int Scale(int weight)=>(int)Math.Round((float)distance*weight/total,MidpointRounding.ToEven);
 public int Next(int weight,bool swimming,bool difficult,bool againstWind=false)
 {
  if(weight<1||weight>total-walked)throw new ArgumentOutOfRangeException(nameof(weight));
  walked+=weight;if(swimming)swum+=weight;if(difficult)rough+=weight;if(againstWind)wind+=weight;
  int next=Scale(walked)*(crawling?2:1)+Scale(swum)+Scale(rough)+Scale(wind),cost=next-paid;paid=next;return cost;
 }
 public int Paid=>paid;
 public int Travelled=>Scale(walked);
}
