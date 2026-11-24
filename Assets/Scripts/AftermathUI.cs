using System.Collections;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame {
 readonly CorpseClock playerCorpse=new CorpseClock(),monsterCorpse=new CorpseClock();
 bool hasMonsterLongRest;long lastMonsterLongRestStart;
 bool revivalWilling;string aftermathMessage="",waitMinutes="1";Vector2 aftermathScroll;
 void ObserveDeaths(){if(map.RescuePosition.HasValue)rescueCarrier.ObserveLife(worldRounds);ReconcileFallenRescue();playerCorpse.Observe(combat.PlayerDead,9,worldRounds);monsterCorpse.Observe(combat.Hp==0,monsterProfile.TypeIndex,worldRounds);if(playerCorpse.Dead||monsterCorpse.Dead||combat.MonsterDeparted)combat.Aftermath=true;EnsureAftermathTurns();SuspendSummonControlAftermath();}
 void AdvanceAftermathClock(long rounds){ObserveDeaths();worldRounds+=rounds;playerCorpse.Age(worldRounds);monsterCorpse.Age(worldRounds);if(rounds>1)combat.ClearShortDurationEffects();RefreshAidEffects();RefreshConcentration();RefreshWildShape();SyncTimedBuffs();UpdateStableRecovery();ObserveDeaths();}
 bool CanPlayerAftermathCast=>combat.Aftermath&&PlayerTurnAllowed&&combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&!combat.PlayerFeignDeath&&CanCastInCurrentForm&&playerResurrectionCaster.CanCast&&combat.AftermathMagicActionAvailable;
 bool KnowsRevival(string name){if(spellMetadata==null)spellMetadata=JsonUtility.FromJson<SpellMetadataSet>(Resources.Load<TextAsset>("Rules2024/Spells").text);var metadata=spellMetadata.spells.FirstOrDefault(x=>x.name==name);return metadata!=null&&EligibleCasters(metadata).Any();}
 string RevivalError(string name,bool player,bool external){ObserveDeaths();string absent=MissingBodyError(name,player?0:-1,external);if(absent!="")return absent;if(!combat.Aftermath)return "请在战后阶段使用。";if(!(external?externalResurrectionCaster:playerResurrectionCaster).CanCast)return "施法者受复生术疲惫影响，必须先完成长休。";if(!external&&!SpellVoiceAllowed(true,name))return "单位中心位于沉默范围，不能进行声音施法。";if(!external&&!ArmorSpellAllowed(name))return "未受训护甲禁止施法，或需要先收起武器腾出空手。";if(!external&&!CanPlayerAftermathCast)return "玩家当前不能施法。";if(!external&&!KnowsRevival(name))return "当前职业不能施放该法术。";if(!external&&!player&&map.BodyPresent(-1)&&map.DistanceFeet>5)return "玩家必须先接近尸体至5尺内。";if(name=="True Resurrection"&&!player&&monsterCorpse.Type==13&&!CanRestoreOriginalMonster)return "生前形态在原位置越界或与玩家占地重叠，禁止施法且不推进时间。";if(name!="Gentle Repose"&&!revivalWilling)return "请确认目标愿意复活。";if(name=="Reincarnate"){string placement=ReincarnationPlacementError(player?0:-1);if(placement!="")return placement;}return RevivalRules.Error(name,player?playerCorpse:monsterCorpse,worldRounds);}
 void DrawAftermath(){
  Surface(new Rect(350,110,540,745),paperTexture);if(Btn(new Rect(780,125,90,46),"关闭")){panel="";return;}
  GUILayout.BeginArea(new Rect(375,185,490,635));aftermathScroll=GUILayout.BeginScrollView(aftermathScroll);
  GUILayout.Label("战后施法与外部救援",heading);DrawAftermathPhaseControls();GUILayout.Label("外部施法者为固定同一人，不入场、可接触目标，法术位不限；其疲惫独立记录。复活后手动继续，不重置HP和职业次数。",small);
  ObserveDeaths();GUILayout.Label("玩家："+combat.PlayerLifeLabel+(playerCorpse.Dead?" · 有效死亡 "+playerCorpse.Age(worldRounds)*6+"秒":"")+"\n木桩："+(combat.Hp==0?"死亡 · 有效死亡 "+monsterCorpse.Age(worldRounds)*6+"秒":"存活"),small);
  if(steed!=null&&steed.Alive)GUILayout.Label("异界坐骑保留 · HP "+steed.Hp+" / "+steed.MaxHp+"；战后暂停操控，恢复战斗后再行动。",small);
  if(bestialSpirit.Active)GUILayout.Label("召唤物保留 · HP "+bestialSpirit.Unit.Hp+" / "+bestialSpirit.Unit.MaxHp+" · 最多剩余 "+System.Math.Max(0,bestialSpirit.Expires-worldRounds)*6+" 秒。战后暂停操控，时间照常流逝；恢复战斗后再行动。",small);
  if(GUILayout.Button("部署 / 操控救援搬运者",button)&&!rolling){panel="rescue";GUILayout.EndScrollView();GUILayout.EndArea();return;}
  if(map.Remains.Bodies.Count>0&&GUILayout.Button("旧躯体与遗留装备",button)&&!rolling){panel="remains";GUILayout.EndScrollView();GUILayout.EndArea();return;}
  DrawMissingBodyChoices();DrawPlayerWebEscape();DrawExternalCasterRest();
  revivalWilling=GUILayout.Toggle(revivalWilling,"确认目标灵魂愿意复活",label);
  foreach(string name in new[]{"Revivify","Raise Dead","Resurrection","Reincarnate","Gentle Repose","True Resurrection"}){
   var spell=DirectSpell.Find(name);GUILayout.Label(spell.Summary,small);if(name=="True Resurrection"&&monsterCorpse.Dead&&monsterCorpse.Type==13)GUILayout.Label("生前："+CreatureProfile.TypeNames[originalMonster.Type]+" · "+CreatureProfile.SizeNames[originalMonster.Size]+" · 原位置"+(CanRestoreOriginalMonster?"可容纳（绿色）":"不可容纳（红色），禁止施法"),small);
   foreach(bool external in new[]{false,true}){
    bool targetPlayer=external;string reason=RevivalError(name,targetPlayer,external);bool old=GUI.enabled;GUI.enabled=!rolling&&reason=="";
    if(GUILayout.Button((external?"外部施法者 → 玩家：":"玩家 → 木桩：")+spell.Chinese,button)){panel="";StartCoroutine(CastRevival(name,targetPlayer,external));}GUI.enabled=old;if(reason!="")GUILayout.Label(reason,small);
   }
  }
  GUILayout.Label("战后移动：拖动玩家棋子接近尸体，消耗与累计普通路程；额度不足可推进6秒刷新。",small);
  bool previous=GUI.enabled;GUI.enabled=combat.Aftermath&&!rolling;

  waitMinutes=IntegerField(waitMinutes,false);int minutes;GUI.enabled=combat.Aftermath&&!rolling&&int.TryParse(waitMinutes,out minutes)&&minutes>0&&!combat.NeedsDeathSave&&!rescueCarrier.NeedsDeathSave;
  if(GUILayout.Button("等待上述分钟数",button)){int.TryParse(waitMinutes,out minutes);StartCoroutine(WaitAftermathMinutes(minutes));}
  GUI.enabled=combat.Aftermath&&!rolling&&combat.Hp>0&&!combat.NeedsDeathSave&&!rescueCarrier.NeedsDeathSave;
  if(GUILayout.Button("木桩完成短休（1小时）",button)){StartCoroutine(RestMonsterAftermath(false));}
  GUI.enabled=combat.Aftermath&&!rolling&&combat.Hp>0&&!combat.NeedsDeathSave&&!rescueCarrier.NeedsDeathSave&&(!hasMonsterLongRest||worldRounds-lastMonsterLongRestStart>=14400);
  long monsterRestDuration=SpeciesPhysicalActive(false)?monsterSpecies.LongRestRounds:4800;
  if(GUILayout.Button("木桩完成长休（"+(monsterRestDuration/600)+"小时）",button)){StartCoroutine(RestMonsterAftermath(true));}
  GUI.enabled=combat.Aftermath&&!rolling&&combat.PlayerHp>0&&combat.Hp>0&&!combat.PlayerDead&&PlayerTurnAllowed&&RescueDepartureReady;
  if(GUILayout.Button("继续战斗（新回合）",button)){StartCoroutine(ResumeAftermath());panel="";}
  GUI.enabled=previous;GUILayout.Label(aftermathMessage,small);GUILayout.EndScrollView();GUILayout.EndArea();
 }
 IEnumerator CastRevival(string name,bool player,bool external){
  if(name=="Reincarnate"){yield return CastReincarnation(player?0:-1,external);yield break;}
  if(RevivalError(name,player,external)!="")yield break;long invisibilityToken=senses.Token(true,worldRounds);rolling=true;string chinese=DirectSpell.Find(name).Chinese;dice=chinese+" · 施法中";
  // Longer casting requires concentration; no interruptions exist in the paused aftermath scene.
  if(!external)PlayerCastWhileSubmerged(name);
  if(!external&&RevivalRules.Duration(name)>1)EndConcentration("开始长时间施法");
  yield return AdvanceRevivalTime(name,external);
  string error=RevivalRules.CompletionError(name,player?playerCorpse:monsterCorpse,worldRounds);if(error=="")error=MissingBodyError(name,player?0:-1,external);
  if(name=="True Resurrection"&&!player&&monsterCorpse.Type==13&&!CanRestoreOriginalMonster)error="原位置无法容纳生前形态，复生失败。";
  if(!external&&(!aftermathAdvanceCompleted||!PlayerCanSustainCast))error="施法者在施法期间失能，施法失败。";
  bool survivedRevival=true;bool taxesCaster=ResurrectionCasterState.RequiresExertion(name,player?playerCorpse:monsterCorpse,worldRounds);
  if(error==""){
   if(name=="Gentle Repose")(player?playerCorpse:monsterCorpse).Preserve(worldRounds);
   else {
    bool full=RevivalRules.FullHp(name);
    if(name=="True Resurrection"&&!player&&monsterCorpse.Type==13)RestoreOriginalMonster();
    if((name!="True Resurrection"||RestoreMissingBody(player?0:-1,external))&&combat.Revive(player,full?(player?combat.PlayerMaxHp:combat.MaxHp):1,RevivalRules.Ordeal(name))){if(name=="Resurrection"||name=="True Resurrection")regeneration.RestoreBody(player?0:-1);RecordResurrectionExertion(external,taxesCaster);(player?playerCorpse:monsterCorpse).Clear();survivedRevival=RestoreBreathingAfterRevival(player);}
    if(name=="True Resurrection"){combat.ClearPoison(player);if(mark.Active&&mark.Hex&&mark.Applies(player))EndConcentration("完全复生术解除诅咒");}
    if(RevivalRules.Ordeal(name))combat.ClearPoison(player);stableRecoveryAt=-1;ObserveDeaths();}
   history.Insert(0,(external?"外部救援：":"战后施法：")+chinese+"对"+(player?"玩家":"木桩")+"生效，耗时 "+RevivalRules.Duration(name)*6+" 秒。"+(RevivalRules.Ordeal(name)?"复活者d20检定减4，每次完成长休减轻1。":""));dice=chinese+(survivedRevival?" · 已完成":" · 复活后窒息再次死亡");if(!survivedRevival)history.Insert(0,"复活后的致死窒息仍在，目标立即再次死亡；本次施法耗时已经推进。");
  }else {history.Insert(0,chinese+"失败："+error);dice=chinese+" · 失败";}if(!external)BreakOrdinaryInvisibility(true,"施法",invisibilityToken);rolling=false;
 }
 IEnumerator AftermathTick(){yield return EndAftermathTurn();}
 IEnumerator ResumeAftermath(){
  if(!PlayerTurnAllowed){aftermathMessage="请先结束救援者回合。";yield break;}
  if(!RescueDepartureReady||rescueCarrier.Load.Loaded){aftermathMessage="请先让救援者恢复清醒并放下搬运对象。";yield break;}
  if(combat.MonsterDeparted||combat.PlayerHp<=0||combat.PlayerDead||combat.Hp<=0)yield break;
  yield return AdvanceAftermathRounds(1);
  if(combat.MonsterDeparted||combat.PlayerHp<=0||combat.PlayerDead||combat.Hp<=0||!RescueDepartureReady){aftermathMessage="回合结算后仍有伤亡，保留战后现场。";yield break;}
  map.RemoveRescue();rescueCarrier.Reset();rescueDropMode=false;combat.Aftermath=false;ExitAftermathTurns();combat.StartMonsterTurn(false);map.StartMonsterMovementTurn(combat.MonsterSpeed);tacticalMovement=false;outcomeRecorded=false;history.Insert(0,"继续战斗：新玩家回合已开始，保留HP、职业消耗和持续法术。");
 }
}
