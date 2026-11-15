using System;
class MonsterAreaChecks
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static int Feet(int ax,int ay,int bx,int by)=>(int)Math.Round(Math.Sqrt((ax-bx)*(ax-bx)+(ay-by)*(ay-by))*5);
    static void Main()
    {
        int x,y;
        Check(MonsterAreaPlanner.Choose(10,60,5,(a,b)=>Feet(0,0,a,b),(a,b)=>(Feet(a,b,8,8)<=5?1:0)+(Feet(a,b,9,8)<=5?1:0),out x,out y),"safe centre exists");
        Check(Feet(x,y,8,8)<=5&&Feet(x,y,9,8)<=5&&Feet(0,0,x,y)>5,"max coverage avoids caster");
        Check(!MonsterAreaPlanner.Choose(10,150,60,(a,b)=>Feet(4,4,a,b),(a,b)=>1,out x,out y)&&x==-1&&y==-1,"no safe centre falls back");
        Check(!MonsterAreaPlanner.Choose(10,5,5,(a,b)=>Feet(0,0,a,b),(a,b)=>1,out x,out y),"range includes no safe point");
        Check(!MonsterAreaPlanner.Choose(10,150,5,(a,b)=>Feet(0,0,a,b),(a,b)=>0,out x,out y),"no enemies is not a cast");
        Check(MonsterAreaPlanner.Choose(10,30,0,(a,b)=>Feet(5,5,a,b),(a,b)=>(a==3&&b==5||a==7&&b==5)?1:0,out x,out y)&&x==3&&y==5,"deterministic equal coverage");
        Check(MonsterAreaPlanner.Choose(10,30,0,(a,b)=>Feet(5,5,a,b),(a,b)=>(a==3&&b==5||a==6&&b==5)?1:0,out x,out y)&&x==6&&y==5,"closer centre breaks coverage tie");
        Check(!MonsterAreaPlanner.Choose(0,30,5,(a,b)=>10,(a,b)=>1,out x,out y),"invalid size");
        // Full 2x2 caster footprint: a candidate touching its far cell is unsafe,
        // even when it is more than five feet from the top-left anchor.
        Check(MonsterAreaPlanner.Choose(4,30,5,(a,b)=>Feet(Math.Min(1,a),Math.Min(1,b),a,b),(a,b)=>a==2&&b==1?3:a==3&&b==1?1:0,out x,out y)&&x==3&&y==1,"full footprint excludes unsafe high coverage");
        Console.WriteLine("PASS: safe maximum coverage, range, empty targets, deterministic ties, large footprints and fallback");
    }
}
