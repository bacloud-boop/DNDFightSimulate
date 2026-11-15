using System;
class HarmChecks
{
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Main(){
  var c=new CombatState();c.ApplyPlayerDamage(8);
  Check(c.ReduceMaxHp(true,4)==4&&c.PlayerMaxHp==6&&c.PlayerHp==2,"spell maximum reduction does not heal");
  Check(c.ReduceMaxHp(true,20)==5&&c.PlayerMaxHp==1&&c.PlayerHp==1,"maximum floor and current hp clamp");
  Check(c.ReduceMaxHp(true,10)==0&&c.PlayerMaxHpReduction==9,"cannot reduce maximum below one");
  c.ReduceMaxHp(false,10);c.CompleteLongRest(true);
  Check(c.PlayerMaxHp==10&&c.MaxHp==990,"only the resting actor recovers reduced maximum");
  c.ResetBattle();Check(c.PlayerMaxHp==10&&c.PlayerMaxHpReduction==0,"reset clears spell reduction");
  c.GrantMonsterTemporaryHp(20);c.ApplyDamage(5);c.ReduceMaxHp(false,5);
  Check(c.MaxHp==995&&c.Hp==995&&c.MonsterTemporaryHp==15,"temporary hp absorbs damage but not maximum reduction");
  c.UpdateMonsterMaxHp(int.MaxValue);Check(c.ReduceMaxHp(false,7)==7&&c.MaxHp==int.MaxValue-12,"integer maximum stays exact");
  bool bad=false;try{c.ReduceMaxHp(false,-1);}catch(ArgumentOutOfRangeException){bad=true;}Check(bad,"negative reduction rejected");
  Check(DirectSpell.Find("Harm").DiceCount(1,6)==14&&DirectSpell.Find("Harm").DiceCount(20,9)==14,"no invented upcast damage");
  Console.WriteLine("PASS: harm maximum reduction, no healing, floor, temporary HP and reset");
 }
}
