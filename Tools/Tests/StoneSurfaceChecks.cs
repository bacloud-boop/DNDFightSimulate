using System;
using UnityEngine;
class StoneSurfaceChecks
{
 static void C(bool ok,string m){if(!ok)throw new Exception(m);}
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void Main(){var m=new BattleMap();C(m.StoneContact(V(0,0),1)&&m.SameStoneSurface(V(0,0),1,V(9,9),1),"all ordinary land is stone");for(int y=0;y<10;y++)m.SetWater(V(5,y),true);C(!m.StoneContact(V(5,0),1)&&!m.SameStoneSurface(V(0,0),1,V(9,9),1),"water is not stone and separates surfaces");C(m.StoneContact(V(4,0),2),"part of a large footprint can touch stone");C(!m.StoneContact(V(-1,0),1),"invalid footprint has no stone contact");Console.WriteLine("PASS: stone land, water exclusion, connected surfaces and multi-cell contact");}
}
