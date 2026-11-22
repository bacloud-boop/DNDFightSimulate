using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    bool animateRenew;readonly HashSet<int> animateTargets=new HashSet<int>(),animateBones=new HashSet<int>();
    sealed class AnimateChannel {public int Slot,Caster;public AnimateDeadCasting Clock;public int PaidRound=>Clock.PaidRound;public long Finish=>Clock.Finish;public bool Renew;public int[] Targets;public HashSet<int> Bones;}
    AnimateChannel animateChannel;
    bool AnimateBodyInfo(int actor,out Vector2Int p,out int n,out string name)
    {
        p=Vector2Int.zero;n=1;name="";if(!map.BodyPresent(actor))return false;
        if(actor==0&&combat.PlayerDead){p=map.Player;n=map.PlayerSpaces;name="玩家遗体";return true;}
        if(actor==-1&&combat.Hp==0){p=map.Monster;n=map.MonsterSpaces;name="木桩遗体";return true;}
        if(actor==BattleMap.RescueId&&rescueCarrier.Dead&&map.RescuePosition.HasValue){p=map.RescuePosition.Value;n=map.RescueSpaces;name="救援者遗体";return true;}
        var body=map.Remains.Find(actor);if(body==null)return false;p=new Vector2Int(body.X,body.Y);n=body.Spaces;name=body.Name;return true;
    }
    BodyWeight AnimateWeight(int actor)=>actor==0||actor==-1||actor==BattleMap.RescueId?CurrentBodyWeight(actor):map.Remains.Find(actor)?.Weight;
    IEnumerable<int> AnimateBodies(){foreach(int actor in new[]{0,-1,BattleMap.RescueId}){Vector2Int p;int n;string name;if(AnimateBodyInfo(actor,out p,out n,out name))yield return actor;}foreach(var body in map.Remains.Bodies)if(map.BodyPresent(body.Id))yield return body.Id;}
    string AnimatePlanError(bool renewal,IEnumerable<int> targets,int slot)
    {
        var chosen=targets.ToArray();int limit=renewal?AnimateDeadControl.RenewalLimit(slot):AnimateDeadControl.CreationLimit(slot);
        if(limit==0||chosen.Length<1||chosen.Length>limit||chosen.Distinct().Count()!=chosen.Length)return "请选择1～"+limit+"个不同目标。";
        if(!AnimateDeadControl.ValidTime(worldRounds+10)||nextSummonId>int.MaxValue-chosen.Length)return "时间或单位编号超出范围。";
        foreach(int actor in chosen){
            if(renewal){var unit=FindSummon(actor);var entry=undeadControl.Find(actor);if(unit==null||entry==null||entry.Destroyed||BattleMap.BodyDistance(map.Player,map.PlayerSpaces,new Vector2Int(unit.X,unit.Y),unit.Spaces)>10)return "续控目标须为10尺内由本法术创造、尚未被摧毁的亡灵。";continue;}
            Vector2Int p;int n;string name;if(!AnimateBodyInfo(actor,out p,out n,out name)||n!=1)return "需要小型或中型类人生物的遗体／骨堆。";
            int type=actor==0?playerCorpse.Type:actor==-1?monsterCorpse.Type:actor==BattleMap.RescueId?rescueCarrier.Corpse.Type:map.Remains.Find(actor).CreatureType;
            int size=actor==0?EffectiveCreatureSize(true):actor==-1?EffectiveCreatureSize(false):actor==BattleMap.RescueId?rescueCarrier.Size:map.Remains.Find(actor).BodySize;
            if(type!=9||size<1||size>2)return "需要小型或中型类人生物的遗体／骨堆。";
            if(actor==0&&playerCorpse.Preserved(worldRounds)||actor==-1&&monsterCorpse.Preserved(worldRounds)||actor==BattleMap.RescueId&&rescueCarrier.Corpse.Preserved(worldRounds))return "遗体防腐尚未结束，不能转化为亡灵。";
            if(BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n)>10)return "每具遗体须在10尺内。";
            if(map.IsContained(true)||map.RescueCargoId==actor||(actor==0||actor==-1)&&map.IsContained(actor==0))return "请先释放吞咽／搬运中的遗体，施法者不能从体内作用于外界。";
            var weight=AnimateWeight(actor);if(weight==null||!weight.Known)return "请先填写遗体重量。";
            if(!map.CanAnimateBody(new AnimatedUndeadRules(false).Create(nextSummonId),actor))return "亡灵无法在原遗体位置合法落位。";
        }return "";
    }
    string AnimateDeadError()=>animateChannel!=null?"操纵死尸正在持续施法，请使用继续或取消按钮。":AnimatePlanError(animateRenew,animateTargets,directSlotLevel);
    void DrawAnimateDeadChoices()
    {
        GUILayout.Label("持续施法1分钟：每个自身回合须花魔法动作继续并维持专注；未继续便结束施法。完成后亡灵无需专注。遗体／续控目标均须在10尺内。",small);
        bool next=GUILayout.Toggle(animateRenew,"续控／恢复中立亡灵（不创造新单位）",label);if(next!=animateRenew){animateRenew=next;animateTargets.Clear();}
        GUILayout.Label("本次最多 "+(animateRenew?AnimateDeadControl.RenewalLimit(directSlotLevel):AnimateDeadControl.CreationLimit(directSlotLevel))+"个目标",small);
        if(animateRenew){foreach(var follower in undeadFollowers.Values)if(follower.Unit.Alive)DrawAnimateTarget(follower.Unit.Id,follower.Unit.Name+(NeutralUndead(follower.Unit.Id)?"（中立失控）":"（受控）"),false);}
        else foreach(int actor in AnimateBodies()){Vector2Int p;int n;string name;AnimateBodyInfo(actor,out p,out n,out name);DrawAnimateTarget(actor,name+" · "+WeightUnits.Format(AnimateWeight(actor).TotalOunces),true);}
        DrawAnimateChannelControls();DrawUndeadMentalCommands();
    }
    void DrawAnimateTarget(int actor,string name,bool material)
    {
        bool chosen=GUILayout.Toggle(animateTargets.Contains(actor),name,label);if(chosen)animateTargets.Add(actor);else animateTargets.Remove(actor);
        if(material&&chosen){bool bones=GUILayout.Toggle(animateBones.Contains(actor),"资料：此目标为骨堆（骷髅）；关闭表示完整遗体（僵尸）",small);if(bones)animateBones.Add(actor);else animateBones.Remove(actor);}
    }
    void BeginAnimateDead(int caster,int slot)
    {
        if(AnimatePlanError(animateRenew,animateTargets,slot)!="")return;
        var channel=new AnimateChannel{Caster=caster,Slot=slot,Renew=animateRenew,Targets=animateTargets.ToArray(),Bones=new HashSet<int>(animateBones),Clock=new AnimateDeadCasting(combat.Round,worldRounds)};
        EndConcentration("开始操纵死尸的长时间施法");animateChannel=channel;concentrationSpell="操纵死尸（施法中）";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=channel.Finish+1;
        history.Insert(0,"开始操纵死尸：已花本回合魔法动作；之后每回合继续施法，满1分钟结算。未继续、失能或专注中断均失败。");
    }
    void CancelAnimateDead(){animateChannel=null;}
    void EndAnimateCastingTurn(){if(animateChannel!=null&&!animateChannel.Clock.EndTurn(combat.Round))EndConcentration("本回合没有花魔法动作继续操纵死尸");}
    void DrawAnimateChannelControls()
    {
        if(animateChannel==null)return;
        GUILayout.Label("操纵死尸施法中 · 剩余 "+Math.Max(0,animateChannel.Finish-worldRounds)*6+"秒 · "+(animateChannel.PaidRound==combat.Round?"本回合已继续":"本回合尚未继续"),label);
        bool old=GUI.enabled;GUI.enabled=old&&!rolling&&animateChannel.PaidRound!=combat.Round;
        if(GUILayout.Button("继续操纵死尸（魔法动作）",button))StartCoroutine(ContinueAnimateDead());GUI.enabled=old;
        if(GUILayout.Button("取消持续施法",button))EndConcentration("主动取消操纵死尸");
    }
    IEnumerator ContinueAnimateDead()
    {
        if(animateChannel==null||rolling||animateChannel.PaidRound==combat.Round||!PlayerTurnAllowed||!PlayerCanSustainCast||!SpellVoiceAllowed(true,"Animate Dead")||!ArmorSpellAllowed("Animate Dead"))yield break;
        if(!animateChannel.Clock.CanContinue(combat.Round,worldRounds)||!(combat.Aftermath?combat.TryUseAftermathMagicAction():combat.TryUseMagicAction()))yield break;
        var channel=animateChannel;channel.Clock.Continue(combat.Round,worldRounds);rolling=true;PlayerCastWhileSubmerged("Animate Dead");
        try{var stopped=new SpellInterruption();yield return TryInterruptSpell(true,"Animate Dead",stopped);if(stopped.Cancelled)EndConcentration("操纵死尸被法术反制中断");}
        finally{rolling=false;}
    }
    void RefreshAnimateDead()
    {
        RefreshUndeadFollowers();if(animateChannel==null||rolling||worldRounds<animateChannel.Finish)return;
        var channel=animateChannel;animateChannel=null;EndConcentration("操纵死尸施法结束");
        if(!channel.Clock.Ready(worldRounds)){history.Insert(0,"操纵死尸失败：未连续完成10次魔法动作，未创造亡灵或延长控制。");return;}
        string error=PlayerCanSustainCast?AnimatePlanError(channel.Renew,channel.Targets,channel.Slot):"施法者失能，施法失败。";
        if(error!=""){history.Insert(0,"操纵死尸失败："+error);return;}
        if(channel.Renew){undeadControl.Reassert(channel.Targets,channel.Slot,worldRounds,id=>true);history.Insert(0,"操纵死尸：所选亡灵的控制恢复／延长至24小时后，HP不变，失控前旧指令不恢复。");return;}
        foreach(int actor in channel.Targets){
            var rules=new AnimatedUndeadRules(channel.Bones.Contains(actor));var unit=rules.Create(nextSummonId);
            var old=map.Remains.Find(actor);var armor=actor==0?playerArmor:actor==-1?monsterArmor:null;
            var follower=new UndeadFollower{Unit=unit,Source=actor,Owner=old!=null?old.Owner:actor,Rules=rules,Weight=AnimateWeight(actor).Copy(),Ammo=(old!=null?old.Owner:actor)==0?ammunition:new AmmunitionState(),ArmorIndex=old!=null?(old.NonWeaponGearRecovered?0:old.ArmorIndex):armor!=null&&!armor.Custom?armor.ArmorIndex:0,Shield=old!=null?old.Shield&&!old.NonWeaponGearRecovered:armor!=null&&armor.Shield};
            SaveUndeadEquipmentReset(actor);follower.Items.AddRange(CollectUndeadItems(actor));
            if(!map.AnimateBody(unit,actor)){history.Insert(0,"操纵死尸落位失败，遗体未消耗。");continue;}
            undeadControl.Register(new[]{unit.Id},channel.Slot,worldRounds);undeadFollowers.Add(unit.Id,follower);nextSummonId++;
            unit.ControllerAllows=()=>undeadControl.Controlled(unit.Id,worldRounds)||CommandMoving(unit.Id);unit.LethalGuard=(damage,types,critical)=>UndeadFortitude(follower,damage,types,critical);
            unit.BaseArmorOverride=()=>Math.Max(rules.Armor,follower.ArmorIndex>0?ArmorDefinition.All[follower.ArmorIndex].ArmorClass(rules.Ability(1)):rules.Armor);
            AdoptUndeadEquipment(follower);ObserveGreaseMovement(unit.Id,new Vector2Int(-100,-100),new Vector2Int(unit.X,unit.Y),0,unit.Spaces,true);history.Insert(0,"操纵死尸："+unit.Name+"从原遗体位置起身。原角色保持死亡、原遗体已消耗，装备沿用原物件；尚无精神指令。");
        }RefreshUndeadFollowers();
    }
    void DrawUndeadMentalCommands()
    {
        var controlled=undeadFollowers.Values.Where(f=>f.Unit.Alive&&undeadControl.Controlled(f.Unit.Id,worldRounds)&&BattleMap.BodyDistance(map.Player,map.PlayerSpaces,new Vector2Int(f.Unit.X,f.Unit.Y),1)<=60).ToArray();
        if(controlled.Length==0)return;
        GUILayout.Label("精神指令：60尺内受控亡灵共用1个附赠动作接收同一笼统指令；无需开口，不打断屏息。旧指令持续有效。",small);
        foreach(int order in new[]{1,2}){bool old=GUI.enabled;GUI.enabled=old&&!rolling&&PlayerTurnAllowed&&combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&!combat.PlayerFeignDeath;
            if(GUILayout.Button(order==1?"命令全部受控亡灵：协助战斗（附赠动作）":"命令全部受控亡灵：闪避（附赠动作）",button)&&undeadControl.Command(controlled.Select(f=>f.Unit.Id),order,worldRounds,id=>true,()=>combat.Aftermath?combat.TryUseAftermathBonusAction():combat.TryUseBonusAction()))foreach(var f in controlled){bool changed;f.Unit.ReceiveVerbalOrders(order==1?SummonCommand.AssistCombat:SummonCommand.Dodge,out changed);}GUI.enabled=old;
        }
    }
}
