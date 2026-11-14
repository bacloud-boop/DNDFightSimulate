using System.Collections;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame {
 readonly CorpseClock playerCorpse=new CorpseClock(),monsterCorpse=new CorpseClock();
 bool hasMonsterLongRest;long lastMonsterLongRestStart;
 bool revivalWilling;string aftermathMessage="",waitMinutes="1";Vector2 aftermathScroll;
 void ObserveDeaths(){playerCorpse.Observe(combat.PlayerDead,9,worldRounds);monsterCorpse.Observe(combat.Hp==0,monsterProfile.TypeIndex,worldRounds);if(playerCorpse.Dead||monsterCorpse.Dead)combat.Aftermath=true;}
 void AdvanceAftermathTime(long rounds){ObserveDeaths();worldRounds+=rounds;playerCorpse.Age(worldRounds);monsterCorpse.Age(worldRounds);if(rounds>1)combat.ClearShortDurationEffects();RefreshAidEffects();RefreshConcentration();RefreshWildShape();SyncTimedBuffs();UpdateStableRecovery();ObserveDeaths();}
 bool CanPlayerAftermathCast=>combat.Aftermath&&combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&CanCastInCurrentForm;
 bool KnowsRevival(string name){if(spellMetadata==null)spellMetadata=JsonUtility.FromJson<SpellMetadataSet>(Resources.Load<TextAsset>("Rules2024/Spells").text);var metadata=spellMetadata.spells.FirstOrDefault(x=>x.name==name);return metadata!=null&&EligibleCasters(metadata).Any();}
 string RevivalError(string name,bool player,bool external){ObserveDeaths();if(!combat.Aftermath)return "请在战后阶段使用。";if(!external&&!CanPlayerAftermathCast)return "玩家当前不能施法。";if(!external&&!KnowsRevival(name))return "当前职业不能施放该法术。";if(!external&&!player&&map.DistanceFeet>5)return "玩家必须先接近尸体至5尺内。";if(name!="Gentle Repose"&&!revivalWilling)return "请确认目标愿意复活。";return RevivalRules.Error(name,player?playerCorpse:monsterCorpse,worldRounds);}
 void DrawAftermath(){
  Surface(new Rect(350,110,540,745),paperTexture);if(Btn(new Rect(780,125,90,46),"关闭")){panel="";return;}
  GUILayout.BeginArea(new Rect(375,185,490,635));aftermathScroll=GUILayout.BeginScrollView(aftermathScroll);
  GUILayout.Label("战后施法与外部救援",heading);GUILayout.Label("外部救援仅针对玩家，不入场、视为合法且可接触目标，法术位不限。复活后手动继续，不重置HP和职业次数。",small);
  ObserveDeaths();GUILayout.Label("玩家："+combat.PlayerLifeLabel+(playerCorpse.Dead?" · 有效死亡 "+playerCorpse.Age(worldRounds)*6+"秒":"")+"\n木桩："+(combat.Hp==0?"死亡 · 有效死亡 "+monsterCorpse.Age(worldRounds)*6+"秒":"存活"),small);
  revivalWilling=GUILayout.Toggle(revivalWilling,"确认目标灵魂愿意复活",label);
  foreach(string name in new[]{"Revivify","Raise Dead","Gentle Repose"}){
   var spell=DirectSpell.Find(name);GUILayout.Label(spell.Summary,small);
   foreach(bool external in new[]{false,true}){
    bool targetPlayer=external;string reason=RevivalError(name,targetPlayer,external);bool old=GUI.enabled;GUI.enabled=!rolling&&reason=="";
    if(GUILayout.Button((external?"外部施法者 → 玩家：":"玩家 → 木桩：")+spell.Chinese,button)){panel="";StartCoroutine(CastRevival(name,targetPlayer,external));}GUI.enabled=old;if(reason!="")GUILayout.Label(reason,small);
   }
  }
  GUILayout.Label("战后移动：拖动玩家棋子接近尸体，消耗与累计普通路程；额度不足可推进6秒刷新。",small);
  bool previous=GUI.enabled;GUI.enabled=combat.Aftermath&&!rolling;
  if(GUILayout.Button("推进6秒 / 倒地死亡豁免",button)){panel="";StartCoroutine(AftermathTick());}
  waitMinutes=IntegerField(waitMinutes,false);int minutes;GUI.enabled=combat.Aftermath&&!rolling&&int.TryParse(waitMinutes,out minutes)&&minutes>0&&!combat.NeedsDeathSave;
  if(GUILayout.Button("等待上述分钟数",button)){int.TryParse(waitMinutes,out minutes);AdvanceAftermathTime((long)minutes*10);aftermathMessage="已推进 "+minutes+" 分钟。";}
  GUI.enabled=combat.Aftermath&&!rolling&&combat.Hp>0&&!combat.NeedsDeathSave&&(!hasMonsterLongRest||worldRounds-lastMonsterLongRestStart>=14400);
  if(GUILayout.Button("木桩完成长休（8小时）",button)){lastMonsterLongRestStart=worldRounds;hasMonsterLongRest=true;AdvanceAftermathTime(4800);if(combat.Hp>0){combat.HealMonster(int.MaxValue);combat.CompleteLongRest(false);}aftermathMessage="木桩长休完成；复活减值减轻1（每24小时至多一次）。";}
  GUI.enabled=combat.Aftermath&&!rolling&&combat.PlayerHp>0&&combat.Hp>0&&!combat.PlayerDead;
  if(GUILayout.Button("继续战斗（新回合）",button)){ResumeAftermath();panel="";}
  GUI.enabled=previous;GUILayout.Label(aftermathMessage,small);GUILayout.EndScrollView();GUILayout.EndArea();
 }
 IEnumerator CastRevival(string name,bool player,bool external){
  if(RevivalError(name,player,external)!="")yield break;rolling=true;string chinese=DirectSpell.Find(name).Chinese;dice=chinese+" · 施法中";
  // Longer casting requires concentration; no interruptions exist in the paused aftermath scene.
  if(RevivalRules.Duration(name)>1)EndConcentration("开始长时间施法");
  AdvanceAftermathTime(RevivalRules.Duration(name));yield return null;
  string error=RevivalRules.Error(name,player?playerCorpse:monsterCorpse,worldRounds);if(name=="Revivify"||name=="Raise Dead"){
   // Eligibility was checked against completion time before advancing. Recheck current age without adding casting twice.
   var corpse=player?playerCorpse:monsterCorpse;error=!corpse.Dead?"目标已不再死亡。":name=="Raise Dead"&&corpse.Type==13?"亡灵目标不合法。":corpse.Age(worldRounds)>(name=="Revivify"?10:144000)?"已超过死亡时限。":"";
  }
  if(!external&&(combat.PlayerHp<=0||combat.PlayerDead||combat.PlayerParalyzed))error="施法者在施法期间失能，施法失败。";
  if(error==""){
   if(name=="Gentle Repose")(player?playerCorpse:monsterCorpse).Preserve(worldRounds);
   else {combat.Revive(player,1,name=="Raise Dead");if(name=="Raise Dead")combat.ClearPoison(player);stableRecoveryAt=-1;ObserveDeaths();}
   history.Insert(0,(external?"外部救援：":"战后施法：")+chinese+"对"+(player?"玩家":"木桩")+"生效，耗时 "+RevivalRules.Duration(name)*6+" 秒。"+(name=="Raise Dead"?"复活者d20检定减4，每次完成长休减轻1。":""));dice=chinese+" · 已完成";
  }else {history.Insert(0,chinese+"失败："+error);dice=chinese+" · 失败";}rolling=false;
 }
 IEnumerator AftermathTick(){rolling=true;AdvanceAftermathTime(1);combat.EndRound();combat.ExpirePlayerTurnEffects();fighter.EndRound();lightAttacksThisTurn.Clear();map.RefreshMovement();yield return PlayerDeathSave();rolling=false;}
 void ResumeAftermath(){if(combat.PlayerHp<=0||combat.PlayerDead||combat.Hp<=0)return;AdvanceAftermathTime(1);if(combat.PlayerHp<=0||combat.PlayerDead||combat.Hp<=0){aftermathMessage="时间推进后有人倒地或死亡，继续保持战后阶段。";return;}combat.Aftermath=false;combat.StartMonsterTurn();combat.EndRound();combat.ExpirePlayerTurnEffects();fighter.EndRound();lightAttacksThisTurn.Clear();actionWeapon=null;map.RefreshMovement();map.StartMonsterMovementTurn(combat.MonsterSpeed);tacticalMovement=false;outcomeRecorded=false;history.Insert(0,"继续战斗：新回合已开始，仅刷新回合资源，保留HP、职业消耗和持续法术。");}
}
