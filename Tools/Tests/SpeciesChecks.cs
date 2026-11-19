using System;
using System.Collections.Generic;
class SpeciesChecks
{
 static void C(bool ok,string m){if(!ok)throw new Exception(m);}
 static void Main(){
  for(int i=0;i<10;i++){var p=new SpeciesProfile{Kind=(Species2024)i,ChosenSkill=i==3?12:0};for(int j=0;j<p.Lineages.Length;j++){p.Lineage=j;C(p.Valid(),"all species and lineages have legal defaults");}}
  var dwarf=new SpeciesProfile{Kind=Species2024.Dwarf};C(dwarf.Resist(9)&&!dwarf.Resist(5)&&dwarf.Darkvision==120&&dwarf.HpBonus(7)==7,"dwarf poison resistance, darkvision and per-level HP");
  var elf=new SpeciesProfile{Kind=Species2024.Elf,ChosenSkill=12,Lineage=2};C(elf.NoSleep&&elf.Speed==35&&elf.LongRestRounds==2400,"wood elf speed and four-hour trance");
  elf.Lineage=0;C(elf.Darkvision==120&&elf.SaveAdvantage(3,charm:true),"drow vision and elven fey ancestry");
  var gnome=new SpeciesProfile{Kind=Species2024.Gnome};C(gnome.BodySize==1&&gnome.SaveAdvantage(4)&&gnome.SaveAdvantage(3)&&gnome.SaveAdvantage(5)&&!gnome.SaveAdvantage(2),"gnomish cunning applies only to mental saves");
  var goliath=new SpeciesProfile{Kind=Species2024.Goliath};C(goliath.Speed==35&&goliath.BodySize==2&&goliath.CapacitySize(2)==3,"powerful build changes capacity rather than occupied size");
  C(goliath.ProficiencyUses(-10)==0&&goliath.ProficiencyUses(0)==0&&goliath.ProficiencyUses(int.MaxValue)==int.MaxValue,"manual monster proficiency uses clamp at zero without overflow");
  var h=SpeciesProfile.Human();h.OriginFeat=9;C(h.HpBonus(20)==40,"human Tough follows character level");
  h.OriginFeat=7;h.SkilledChoices=new[]{1,2,3};var baseSkills=new int[55];baseSkills[5]=1;
  var merged=SpeciesSkillRules.Merge(baseSkills,h,new[]{1});C(merged[0]==1&&merged[1]==2&&merged[3]==1&&merged[5]==1,"human skill and Skilled merge with separate class expertise");
  merged=SpeciesSkillRules.Merge(baseSkills,dwarf,new[]{1});C(merged[0]==0&&merged[1]==0&&merged[5]==1,"species swap removes only racial sources and unsupported expertise");
  var rogue=new PlayerClasses();var levels=new string[12];for(int i=0;i<12;i++)levels[i]=i==7?"1":"0";C(rogue.TryApply(levels,7),"rogue fixture");
  var picks=new Dictionary<string,int>{{"bg-tool",35},{"class7-0",1},{"class7-1",2},{"class7-2",3},{"class7-3",6},{"expert0",4},{"expert1",2}};
  var human=SpeciesProfile.Human();human.ChosenSkill=4;human.SkilledChoices=new[]{7,8,9};int[] baseline;HashSet<int> experts;string error;
  C(SpeciesSkillRules.TryBuild(rogue,14,picks,human,out baseline,out experts,out error),error);C(baseline[4]==0&&SpeciesSkillRules.Merge(baseline,human,experts)[4]==2,"expertise may use a separately granted racial proficiency");
  var swapped=SpeciesSkillRules.Merge(baseline,dwarf,experts);C(swapped[4]==0&&swapped[2]==2&&swapped[15]==1,"species replacement removes racial expertise prerequisite while retaining class/background sources");
  human.ChosenSkill=0;C(!SpeciesSkillRules.TryBuild(rogue,14,picks,human,out baseline,out experts,out error),"optional racial proficiency cannot duplicate background fixed grant");
  var copy=h.Copy();copy.SkilledChoices[0]=9;C(h.SkilledChoices[0]==1,"species choice arrays are independent");
  var c=new CombatState();c.ApplyPlayerDamage(3);c.SetSpeciesHpBonus(true,5,true);C(c.PlayerMaxHp==15&&c.PlayerHp==15,"configuration HP changes refill");
  c.KillMonster();c.SetSpeciesHpBonus(false,7,false);C(c.Hp==0&&c.MaxHp==1007,"original-form species restoration does not revive corpse early");
  var fatigue=new ResurrectionCasterState();fatigue.ApplyExertion();C(fatigue.CompleteLongRest(0,2400,elf.LongRestRounds)&&fatigue.CanCast,"completed elven trance clears caster exertion");
  var external=new ResurrectionCasterState();external.ApplyExertion();C(!external.CompleteLongRest(0,2400)&&external.Taxed,"external caster still requires eight hours");
  var abilities=new SpeciesAbilityState();
  C(!abilities.Use(Species2024.Orc,-1,0)&&abilities.RushSpent==0,"negative proficiency cannot consume a use or grant a negative pool");
  C(abilities.Use(Species2024.Orc,2,0)&&abilities.Use(Species2024.Orc,2,1)&&!abilities.Use(Species2024.Orc,2,2),"Rush stops at proficiency uses");
  C(abilities.Use(Species2024.Dwarf,2,3)&&abilities.StoneActive(102)&&!abilities.StoneActive(103),"Stonecunning expires after one hundred six-second rounds");
  C(!abilities.CanAutomaticallyUse(Species2024.Dwarf,2,4)&&abilities.CanUse(Species2024.Dwarf,2,4)&&abilities.StoneSpent==1,"automatic activation skips an active effect while manual renewal remains legal");
  abilities.ShortRest();C(abilities.RushSpent==0&&abilities.StoneSpent==1,"short rest refreshes Rush without refreshing Stonecunning uses");
  abilities.LongRest();C(abilities.StoneSpent==0&&abilities.RushSpent==0,"long rest refreshes both species use pools");
  C(!abilities.Use(Species2024.Dwarf,2,long.MaxValue-50),"duration overflow cannot consume a use");
  C(SpeciesTurnPolicy.ShouldAttempt(0,true,true,true,true)&&!SpeciesTurnPolicy.ShouldAttempt(0,false,true,true,true)&&SpeciesTurnPolicy.ShouldAttempt(1,false,true,true,true),"priority selects exactly one scheduling phase");
  C(!SpeciesTurnPolicy.ShouldAttempt(2,true,true,true,true)&&!SpeciesTurnPolicy.ShouldAttempt(1,false,false,true,true)&&!SpeciesTurnPolicy.ShouldAttempt(0,true,true,false,true),"disabled, spent bonus actions and beast form block automatic features");
  var turns=new CombatState();turns.StartMonsterTurn();C(turns.TryUseMonsterBonusAction()&&!turns.TryUseMonsterBonusAction(),"racial traits and bonus spells share the monster bonus action");
  turns.StartMonsterTurn();C(turns.MonsterBonusActionAvailable&&turns.TryUseMonsterBonusAction(),"only the next monster turn refreshes its bonus action");
  turns.MonsterParalyzed=true;turns.StartMonsterTurn();C(!turns.TryUseMonsterBonusAction(),"incapacitated monster cannot activate a species bonus action");
  var speciesCopy=dwarf.Copy();speciesCopy.BonusPriority=2;C(dwarf.BonusPriority==0&&speciesCopy.Valid(),"turn policy is independently copied with species data");
  var hands=new SpeciesAbilityState();C(hands.Remaining(Species2024.Aasimar,-2)==1&&hands.Use(Species2024.Aasimar,-2,0)&&!hands.Use(Species2024.Aasimar,2,1),"Healing Hands has one long-rest use independently of proficiency-based healing dice");
  hands.ShortRest();C(hands.HealingHandsSpent,"short rest does not refresh Healing Hands");hands.LongRest();C(!hands.HealingHandsSpent,"long rest refreshes Healing Hands");
  var budget=new CombatState();budget.StartMonsterTurn();C(budget.TryUseMonsterAction()&&!budget.TryUseMonsterAction()&&budget.TryUseMonsterBonusAction(),"monster racial Magic action, spells and custom attacks share one action while bonus action remains separate");
  C(budget.MonsterReactionAvailable,"spending an action does not consume reaction");budget.StartMonsterTurn();C(budget.MonsterActionAvailable&&budget.MonsterBonusActionAvailable,"monster turn refreshes both action pools");
  var surge=new CombatState();surge.AddSurgeAction();C(surge.TryUseMagicAction(),"ordinary action can use a Magic action");surge.Aftermath=true;C(!surge.TryUseAftermathMagicAction(),"Action Surge's restricted action cannot become a Magic action in aftermath");
  var outside=new CombatState();outside.KillMonster();outside.Aftermath=true;C(outside.TryUseAftermathMagicAction()&&!outside.TryUseAftermathMagicAction(),"living player may use one Magic action after combat even while the dummy is dead");
  Console.WriteLine("PASS: species choices, separate proficiencies, per-level HP, resistance, save advantage, capacity, corpse HP and elven trance");
 }
}
