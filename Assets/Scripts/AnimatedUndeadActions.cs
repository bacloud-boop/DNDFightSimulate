using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    bool IssueUndeadOrder(SummonedUnit unit,SummonCommand command)
    {
        if(unit==null||rolling||reactionPending||summonTurns.Active||!PlayerTurnAllowed||combat.PlayerHp<=0||combat.PlayerDead||combat.PlayerParalyzed||combat.PlayerFeignDeath)return false;
        int order=command.Kind==SummonCommandKind.AssistCombat?1:command.Kind==SummonCommandKind.Dodge?2:0;
        if(!undeadControl.Command(new[]{unit.Id},order,worldRounds,id=>BattleMap.BodyDistance(map.Player,map.PlayerSpaces,new Vector2Int(unit.X,unit.Y),1)<=60,()=>combat.Aftermath?combat.TryUseAftermathBonusAction():combat.TryUseBonusAction()))return false;
        bool changed;unit.ReceiveVerbalOrders(command,out changed);history.Insert(0,"附赠动作精神命令："+unit.Name+"，无需声音；旧指令跨回合保留。");return true;
    }
    bool UndeadLightReady(UndeadFollower f)=>f.Equipped!=null&&f.Equipped.Definition!=null&&f.Equipped.Definition.HasLight&&f.LightRound==combat.Round&&f.LastLightKey!=null&&f.LastLightKey!=f.Equipped.Definition.InventoryKey;
    int UndeadOpportunityReach(SummonedUnit unit){var f=Undead(unit);return f!=null&&f.Equipped!=null&&f.Equipped.Melee?f.Equipped.Reach:5;}
    int UndeadReach(UndeadFollower f)=>f.Equipped==null?5:f.Equipped.Reach;
    void DrawUndeadEquipment(UndeadFollower f)
    {
        var unit=f.Unit;GUILayout.Label(NeutralUndead(unit.Id)?"中立失控：不移动、不攻击、不接受指令。":f.Verbal?"永久服从口头指令 · 更改指令时需要开口":"受控 · 精神指令保留至更改或控制到期",small);
        if(f.ArmorIndex>0)GUILayout.Label("继承护甲未受训：力量／敏捷D20检定劣势；继承盾牌不提供受训AC加值。",small);
        GUILayout.Label("装备负重 "+WeightUnits.Format(f.Weight.GearOunces)+" · "+(f.Shield?"持盾；双手武器不可用":"未持盾"),small);
        bool old=GUI.enabled;GUI.enabled=old&&unit.ControllerReady&&unit.TurnActive&&!summonActing;
        if(GUILayout.Button(f.Equipped==null?"✓ 徒手／自然攻击":"改用徒手／自然攻击",button))if(f.Grip.TrySetStowed(true,combat.Round,worldRounds,unit.TryUseAction))f.Equipped=null;
        foreach(var item in f.Items){
            if(item.Ground.Dropped){
                bool near=item.Ground.WithinReach(unit.X,unit.Y,1)&&(!item.Ground.Submerged||unit.Aquatic.Submerged);
                bool before=GUI.enabled;GUI.enabled=before&&near&&f.Equipped==null;
                if(GUILayout.Button("拾回 "+item.Object.Name+(item.Ground.Submerged?"（水下，须潜水）":"（5尺内）"),button)){
                    Func<bool> pay=()=>f.Grip.TryInteract(combat.Round,worldRounds,unit.TryUseAction);Func<long,bool> fits=ounces=>CarryingRules.CanCarryOunces(f.Rules.Ability(0),2,f.Weight.GearOunces,ounces);
                    bool picked=item.Inventory!=null?carriedWeapons.FollowerPickUp(item.Inventory.Key,unit.Id,fits,pay,unit.Aquatic.Submerged):item.Ground.PickUp(item.Ground.Owner,unit.X,unit.Y,1,true,true,()=>fits(item.Ounces),pay,unit.Aquatic.Submerged);
                    if(picked){f.Equipped=item;f.Grip.TrySetStowed(false,combat.Round,worldRounds,()=>true,true);RefreshUndeadFollowers();}
                }GUI.enabled=before;continue;
            }
            bool allowed=!f.Shield||item.Definition==null||!item.Definition.RequiresTwoHands,enabled=GUI.enabled;GUI.enabled=enabled&&allowed;
            if(GUILayout.Button((f.Equipped==item?"✓ ":"")+item.Object.Name+(!allowed?"（持盾不可用）":""),button)&&f.Grip.TryInteract(combat.Round,worldRounds,unit.TryUseAction)){f.Equipped=item;f.Grip.TrySetStowed(false,combat.Round,worldRounds,()=>true,true);}
            GUI.enabled=enabled;
            if(item.Definition!=null&&item.Definition.InventoryKey=="长剑")foreach(var grip in Weapon.Available)if(grip.InventoryKey=="长剑"){
                bool gripEnabled=GUI.enabled;GUI.enabled=gripEnabled&&(!f.Shield||!grip.RequiresTwoHands);
                if(GUILayout.Button((item.Definition==grip?"✓ ":"")+grip.Name,button)&&f.Grip.TryInteract(combat.Round,worldRounds,unit.TryUseAction)){item.Definition=grip;f.Equipped=item;}
                GUI.enabled=gripEnabled;
            }
        }
        if(f.Equipped!=null&&GUILayout.Button("放下当前武器",button))DropUndeadItem(f,f.Equipped);
        if(UndeadLightReady(f)&&unit.BonusAvailable&&GUILayout.Button("轻型附赠攻击 · 不触发额外攻击",button))StartCoroutine(ResolveUndeadAttack(f,summonTarget,bonusAttack:true));
        GUI.enabled=old;
    }
    IEnumerator ResolveUndeadAttack(UndeadFollower f,int target,bool opportunity=false,CreatureCover.Body? targetAt=null,bool bonusAttack=false)
    {
        var unit=f.Unit;Vector2Int cell;int spaces;string name;
        if(!unit.Alive||!unit.ControllerReady||combat.Aftermath||combat.BattleFinished||target==unit.Id||!SummonTarget(target,out cell,out spaces,out name))yield break;
        var weapon=f.Equipped;bool melee=weapon==null||weapon.Melee;
        int distance=targetAt.HasValue?SummonGeometry.Distance(new CreatureCover.Body(unit.X,unit.Y,unit.Spaces),targetAt.Value):BattleMap.BodyDistance(new Vector2Int(unit.X,unit.Y),unit.Spaces,cell,spaces),reach=UndeadReach(f);
        if(opportunity&&!melee){weapon=null;melee=true;reach=5;}
        if(!opportunity&&distance>reach||f.Shield&&weapon!=null&&weapon.Definition!=null&&weapon.Definition.RequiresTwoHands)yield break;
        var definition=weapon==null?null:weapon.Definition;bool freeHand=definition!=null&&definition.RequiresTwoHands||!f.Shield;
        if(definition!=null&&!f.Ammo.CanFire(definition.Name,freeHand))yield break;
        if(!opportunity){if(bonusAttack){if(!UndeadLightReady(f)||!unit.TryUseBonus())yield break;}else if(unit.Orders.Kind!=SummonCommandKind.AssistCombat||!unit.Attack())yield break;}
        if(definition!=null)f.Ammo.Fire(definition.Name,freeHand);
        if(!opportunity&&!bonusAttack&&definition!=null&&definition.HasLight){f.LastLightKey=definition.InventoryKey;f.LightRound=combat.Round;}
        bool busy=summonActing;summonActing=true;
        try{
            bool sees=targetAt.HasValue?SpiritSeesAt(unit,target,unit.X,unit.Y,targetAt.Value.X,targetAt.Value.Y,spaces):SpiritSees(unit,target,cell,spaces),targetSees=TargetSeesSpirit(target,unit);
            if(targetAt.HasValue&&target==-1)targetSees=VisibilityBetween(false,targetAt.Value.X,targetAt.Value.Y,spaces,unit.X,unit.Y,unit.Spaces)<2;
            var other=FindSummon(target);bool helpless=target==0?combat.PlayerHp==0||combat.PlayerParalyzed:target==-1?combat.MonsterHelpless:other!=null&&other.SleepStage==2;
            bool prone=target==0?combat.PlayerProne:target==-1?combat.MonsterProne:other!=null&&other.Prone;
            bool restrained=target==0||target==-1?Entangled(target==0):other!=null&&other.Restrained;
            bool advantage=FaerieAdvantage(target,sees)||!targetSees||helpless||restrained||prone&&distance<=5;
            bool disadvantage=HeatDisadvantage(unit.Id)||!sees||unit.Prone||unit.Restrained||FearDisadvantage(unit.Id)||prone&&distance>5||f.ArmorIndex>0;
            if(!melee){disadvantage|=definition!=null&&definition.HasRangeDisadvantage(distance);disadvantage|=combat.MonsterCanAct&&BattleMap.BodyDistance(new Vector2Int(unit.X,unit.Y),1,map.Monster,map.MonsterSpaces)<=5&&VisibilityBetween(false,map.Monster.x,map.Monster.y,map.MonsterSpaces,unit.X,unit.Y,1)<2;}
            int type=weapon==null?0:weapon.Type,normal=definition==null?reach:definition.ReachFeet;
            disadvantage|=UnderwaterCombatRules.Disadvantage(unit.Aquatic.Submerged,true,melee,SummonSwimSpeed(unit),type==1);
            if(target==0)disadvantage|=senses.Blurred(worldRounds);if(other!=null)disadvantage|=other.Dodging&&other.EffectiveSpeed>0&&targetSees;
            int first=UnityEngine.Random.Range(1,21),second=advantage!=disadvantage?UnityEngine.Random.Range(1,21):0,value=second==0?first:advantage?Math.Max(first,second):Math.Min(first,second);
            int modifier=weapon==null?AcidSplash.Modifier(f.Rules.Ability(0)):UndeadWeaponModifier(f),bonus=2+modifier+unit.Breathing.D20Penalty;
            int cover=targetAt.HasValue?map.CreatureCoverAt(unit.Id,target,new CreatureCover.Body(unit.X,unit.Y,1),targetAt.Value):map.CreatureCoverBonus(unit.Id,target),armor=(target==0?combat.PlayerArmorClass:target==-1?combat.MonsterArmorClass:other.Armor)+cover;
            mirrorIntercepted=false;if(target==0&&value!=1&&(value==20||(long)value+bonus>=armor)){yield return ResolvePlayerHitDefense(value,(long)value+bonus,sees,false,cover);armor=combat.PlayerArmorClass+cover;}
            dice=unit.Name+" · "+(weapon==null?"徒手／猛击":weapon.Object.Name);yield return PresentRoll(20,second==0?new[]{first}:new[]{first,second},"采用 "+value+" + "+bonus+" / AC "+armor);
            if(mirrorIntercepted||value==1||value!=20&&(long)value+bonus<armor||UnderwaterCombatRules.AutoMiss(unit.Aquatic.Submerged,true,melee,distance,normal)){history.Insert(0,unit.Name+"未命中，伤害0。");yield break;}
            bool critical=value==20||helpless&&distance<=5;long[] raw=new long[13];
            if(weapon!=null&&weapon.Custom){var sides=new List<int>();var rolls=new List<int[]>();string details;weapon.Damage.Roll(out details,critical,(s,v,c)=>{sides.Add(s);rolls.Add(v);});raw=weapon.Damage.LastRawTypedDamage;for(int i=0;i<sides.Count;i++)yield return PresentRoll(sides[i],rolls[i],details);}
            else{
                int sides=definition!=null?definition.DiceSides:f.Rules.Skeleton?0:8,count=definition!=null?definition.DiceCount:f.Rules.Skeleton?0:1,damage=definition!=null?definition.FixedDamage:f.Rules.Skeleton?1:0;
                if(sides>0){var rolls=new int[count*(critical?2:1)];for(int i=0;i<rolls.Length;i++){rolls[i]=UnityEngine.Random.Range(1,sides+1);damage+=rolls[i];}yield return PresentRoll(sides,rolls,"伤害骰合计 "+damage+(critical?"（暴击骰翻倍）":""));}
                raw[type]=Math.Max(0,damage+(bonusAttack?Math.Min(0,modifier):modifier));
            }
            var reduced=new RayDamage{Raw=raw};
            if(target==0||target==-1){var resolved=combat.ResolveTypedPacket(target==0,reduced.Raw);int amount=PacketAmount(resolved);if(target==0){int frost=melee?AgathysRetaliation(true):0;combat.ApplyPlayerDamage(amount,critical,sourcePlayer:null,resolvedTypes:resolved,sourceActor:unit.Id);if(frost>0)unit.Damage(frost,4);yield return MaintainConcentration(amount);}else combat.ApplyDamage(amount,sourcePlayer:null,resolvedTypes:resolved,sourceActor:unit.Id);}
            else{var resolved=(long[])reduced.Raw.Clone();for(int i=0;i<resolved.Length;i++)if(other.ResistsType(i)||i==5&&other.Aquatic.Submerged)resolved[i]/=2;other.Damage(PacketAmount(resolved),resolvedTypes:resolved,critical:critical);}
            RefreshUndeadFollowers();yield return DrainUndeadSaves();CheckBattleOutcome();
        }finally{summonActing=busy;}
    }
}
