using System;
class CreatureCoverChecks
{
 static CreatureCover.Body B(double x,double y,double size=1)=>new CreatureCover.Body(x,y,size);
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Main(){
  Check(CreatureCover.Bonus(B(0,0),B(4,0),new CreatureCover.Body[0])==0,"empty battlefield has no cover");
  Check(CreatureCover.Bonus(B(0,0),B(4,0),new[]{B(2,0)})==2,"aligned intervening creature blocks half the corners");
  Check(CreatureCover.Bonus(B(0,0),B(4,0),new[]{B(2,1)})==0,"edge touching is not an obstruction");
  Check(CreatureCover.Bonus(B(0,0),B(4,0),new[]{B(1,0),B(2,0),B(3,0)})==2,"multiple cover creatures do not stack");
  Check(CreatureCover.Bonus(B(0,0),B(4,0),new[]{B(6,0)})==0,"creature behind target is ignored");
  Check(CreatureCover.Bonus(B(0,0),B(0,4),new[]{B(0,2)})==2,"vertical geometry is symmetric");
  Check(CreatureCover.Bonus(B(0,0,2),B(6,0),new[]{B(3,0)})==0,"large attacker can use a clear origin corner");
  Check(CreatureCover.Bonus(B(.5,.5,0),B(4,0),new[]{B(2,0)})==2,"area origin uses point rather than caster");
  Check(CreatureCover.Bonus(B(.5,.5,0),B(4,0),new[]{B(0,0)})==0,"body containing effect origin does not shield its outward blast");
  Console.WriteLine("PASS: four-corner creature cover, favourable origins, tangent boundaries, area origins and non-stacking");
 }
}
