using UnityEngine;
public sealed partial class TrainingGame {
 int? opportunitySenseDistance;
 readonly SenseEffects senses=new SenseEffects();bool trueSeeingWilling;
 static bool IsSenseSpell(string name)=>name=="Blur"||name=="Greater Invisibility"||name=="See Invisibility"||name=="True Seeing";
 bool PerceivesWithSpecialSense(bool viewerPlayer,bool targetPlayer){if(viewerPlayer==targetPlayer)return senses.TrueSight(viewerPlayer,worldRounds)||(viewerPlayer&&PlayerBlindsight>0);return Visibility(viewerPlayer,targetPlayer)<2&&((senses.TrueSight(viewerPlayer,worldRounds)&&(opportunitySenseDistance??map.DistanceFeet)<=120)||(viewerPlayer&&PlayerBlindsight>0&&(opportunitySenseDistance??map.DistanceFeet)<=PlayerBlindsight));}
 bool BlurDisadvantage(bool attackerPlayer,bool targetPlayer)=>targetPlayer&&senses.Blurred(worldRounds)&&!PerceivesWithSpecialSense(attackerPlayer,targetPlayer);
 void ApplySenseSpell(DirectSpell spell,bool player){
  if(spell.Name=="Blur"||spell.Name=="Greater Invisibility"){
   EndConcentration("开始新的专注法术");concentrationSpell=spell.Chinese;concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;
   if(spell.Name=="Blur")senses.Blur(worldRounds);else senses.GreaterInvisible(player,worldRounds);
  }else if(spell.Name=="See Invisibility")senses.SeeInvisible(worldRounds);else senses.TrueSeeing(player,worldRounds);
  history.Insert(0,spell.Chinese+"对"+(player?"玩家":"木桩")+"生效。"+((spell.Name=="Blur"||spell.Name=="Greater Invisibility")?"专注至多1分钟。":"非专注1小时。"));dice=spell.Chinese+" · 已生效";
 }
}
