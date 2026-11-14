using UnityEngine;
public sealed partial class TrainingGame {
 int? opportunitySenseDistance;
 readonly SenseEffects senses=new SenseEffects();bool trueSeeingWilling;
 void InitializeInvisibilityEvents(){combat.DamageDealt=source=>BreakOrdinaryInvisibility(source,"造成伤害");}
 void BreakOrdinaryInvisibility(bool player,string reason,long? token=null){if(!senses.BreakNormal(player,worldRounds,token??senses.Token(player,worldRounds)))return;history.Insert(0,(player?"玩家":"木桩")+"因"+reason+"结束自身隐形术，其他目标不受影响。");if(!senses.AnyNormal(worldRounds)&&concentrationSpell=="隐形术")EndConcentration("所有隐形目标均已显形");}
 static bool IsSenseSpell(string name)=>name=="Invisibility"||name=="Blur"||name=="Greater Invisibility"||name=="See Invisibility"||name=="True Seeing";
 bool PerceivesWithSpecialSense(bool viewerPlayer,bool targetPlayer){if(viewerPlayer==targetPlayer)return senses.TrueSight(viewerPlayer,worldRounds)||(viewerPlayer&&PlayerBlindsight>0);return Visibility(viewerPlayer,targetPlayer)<2&&((senses.TrueSight(viewerPlayer,worldRounds)&&(opportunitySenseDistance??map.DistanceFeet)<=120)||(viewerPlayer&&PlayerBlindsight>0&&(opportunitySenseDistance??map.DistanceFeet)<=PlayerBlindsight));}
 bool BlurDisadvantage(bool attackerPlayer,bool targetPlayer)=>targetPlayer&&senses.Blurred(worldRounds)&&!PerceivesWithSpecialSense(attackerPlayer,targetPlayer);
 void ApplySenseSpell(DirectSpell spell,bool player){
  if(spell.Name=="Blur"||spell.Name=="Greater Invisibility"||spell.Name=="Invisibility"){
   EndConcentration("开始新的专注法术");concentrationSpell=spell.Chinese;concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+(spell.Name=="Invisibility"?600:10);
   if(spell.Name=="Blur")senses.Blur(worldRounds);else if(spell.Name=="Invisibility"){if(player||directBothTargets)senses.NormalInvisibility(true,worldRounds);if(!player||directBothTargets)senses.NormalInvisibility(false,worldRounds);}else senses.GreaterInvisible(player,worldRounds);
  }else if(spell.Name=="See Invisibility")senses.SeeInvisible(worldRounds);else senses.TrueSeeing(player,worldRounds);
  history.Insert(0,spell.Chinese+"对"+(spell.Name=="Invisibility"&&directBothTargets?"玩家与木桩":player?"玩家":"木桩")+"生效。"+(spell.Name=="Invisibility"?"专注至多1小时；攻击、伤害或施法仅结束相应目标隐形。":(spell.Name=="Blur"||spell.Name=="Greater Invisibility")?"专注至多1分钟。":"非专注1小时。"));dice=spell.Chinese+" · 已生效";
 }
}
