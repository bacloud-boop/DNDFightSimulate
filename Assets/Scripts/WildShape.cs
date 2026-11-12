using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class TrainingGame
{
    readonly DruidFeatures druid=new DruidFeatures();
    readonly HashSet<string> knownBeastForms=new HashSet<string>{"Rat","Riding Horse","Spider","Wolf"};
    BeastRecord wildForm;
    string selectedBeastForm="Wolf",wildShapeMessage="",knownToReplace="";
    int knownFormsLevel=-1;
    bool knownFormReplacement;
    long wildShapeEndsAt;
    Vector2 druidScroll;
    Vector2Int wildNormalOffset;
    Vector2Int FormReferenceCell => IsWildShaped?map.Player+wildNormalOffset:map.Player;
    bool IsWildShaped => wildForm!=null;
    bool CanCastInCurrentForm => !IsWildShaped||classes[11]>=18;
    int EffectivePlayerAbility(int index) => IsWildShaped&&index<3?wildForm.abilities[index]:attributes[index];
    BeastRecord FindBeast(string name)
    {
        if(beastRecords==null)beastRecords=JsonUtility.FromJson<BeastRecords>(Resources.Load<TextAsset>("Rules2024/Beasts").text);
        foreach(var beast in beastRecords.entries)if(beast.name==name)return beast;return null;
    }
    static int BeastSpaces(BeastRecord beast) => beast.size=="Large"?2:beast.size=="Huge"?3:beast.size=="Gargantuan"?4:1;
    void ReconcileKnownForms()
    {
        if(knownFormsLevel==classes[11])return;
        knownFormsLevel=classes[11];int limit=DruidFeatures.KnownFormLimit(classes[11]);
        foreach(var name in new List<string>(knownBeastForms)){var beast=FindBeast(name);if(beast==null||!beast.EligibleForBaseWildShape(classes[11])||knownBeastForms.Count>limit)knownBeastForms.Remove(name);}
        foreach(var attack in BeastAttack.Supported)if(knownBeastForms.Count<limit){var beast=FindBeast(attack.Form);if(beast!=null&&beast.EligibleForBaseWildShape(classes[11]))knownBeastForms.Add(beast.name);}
    }
    void RefreshWildShape()
    {
        if(!IsWildShaped)return;
        if(combat.PlayerHp==0||combat.PlayerDead||combat.PlayerParalyzed)EndWildShape("失能或死亡");
        else if(worldRounds>=wildShapeEndsAt)EndWildShape("持续时间结束");
    }
    void EndWildShape(string reason)
    {
        if(!IsWildShaped)return;
        string name=wildForm.name;var reference=FormReferenceCell;wildForm=null;
        map.TryResizePlayerAt(1,reference);
        map.SetPlayerSpeed(30);combat.SetPlayerArmorOverride(null);fighter.CancelAttackAction();actionWeapon=null;
        history.Insert(0,"荒野形态结束（"+reason+"）："+name+"，恢复原形与融入的武器；临时生命按通常规则保留。");
    }
    string WildShapeInvalidReason(BeastRecord form)
    {
        if(classes[11]<2)return "德鲁伊2级解锁荒野形态。";
        if(form==null||!form.EligibleForBaseWildShape(classes[11]))return "形态超出当前基础荒野形态限制，或属于暂缓的飞行形态。";
        if(BeastAttack.Find(form.name)==null)return "该野兽完整战斗效果待接入，目前仅供查看资料。";
        if(!knownBeastForms.Contains(form.name))return "尚未学习此形态。";
        if(knownBeastForms.Count!=DruidFeatures.KnownFormLimit(classes[11]))return "请先选满当前等级的已知形态数量。";
        if(page!="battle"||!map.Locked)return "部署完成后可变身。";
        if(rolling||spellTargeting||draggingToken>=0||!combat.PlayerCanAct)return "当前不能变身。";
        if(fighter.AttacksRemaining>0)return "请先完成当前攻击动作，不能在连击中更换攻击形式。";
        if(!combat.BonusActionAvailable)return "本回合附赠动作已使用。";
        if(druid.Remaining(classes[11])==0)return "荒野形态次数已用完。";
        Vector2Int anchor;if(!map.FindPlayerResizePlacement(BeastSpaces(form),out anchor,FormReferenceCell))return "周围空间不足：不能变身，不消耗附赠动作或次数。";
        return "";
    }
    void TryWildShape(BeastRecord form)
    {
        if(WildShapeInvalidReason(form)!="")return;
        var reference=FormReferenceCell;Vector2Int anchor;if(!druid.TryBeginWildShape(classes[11],combat,map,BeastSpaces(form),out anchor,reference))return;
        wildNormalOffset=reference-anchor;wildForm=form;wildShapeEndsAt=worldRounds+DruidFeatures.DurationRounds(classes[11]);
        combat.SetPlayerArmorOverride(form.armor);map.SetPlayerSpeed(form.walk);combat.GrantPlayerTemporaryHp(classes[11]);
        history.Insert(0,"荒野形态："+BeastAttack.Find(form.name).Chinese+"，消耗附赠动作与1次使用；获得"+classes[11]+"临时生命（不叠加），速度"+form.walk+"尺。武器融入形态。");
        panel="";dice="荒野形态 · "+BeastAttack.Find(form.name).Chinese;wildShapeMessage="";
    }
    void DrawDruidPanel()
    {
        ReconcileKnownForms();
        Surface(new Rect(350,110,480,745),paperTexture);
        if(Btn(new Rect(720,125,90,46),"关闭")){panel="";return;}
        GUILayout.BeginArea(new Rect(375,185,430,635));druidScroll=GUILayout.BeginScrollView(druidScroll);
        GUILayout.Label("德鲁伊 · 荒野形态",heading);
        GUILayout.Label("次数 "+druid.Remaining(classes[11])+" / "+DruidFeatures.Maximum(classes[11])+" · 短休恢复1次，长休恢复全部。",label);
        GUILayout.Label("保留原生命与心智属性；使用野兽力量、敏捷、体质、护甲与步行速度。武器统一融入。当前平面地图没有攀爬、游泳、洞穴、黑暗、跳跃或技能检定场景。月亮结社等额外特性待接入。",small);
        if(IsWildShaped)GUILayout.Label("力量 "+EffectivePlayerAbility(0)+" · 敏捷 "+EffectivePlayerAbility(1)+" · 体质 "+EffectivePlayerAbility(2),label);
        GUILayout.Label(IsWildShaped?"当前："+wildForm.name+" · 剩余约"+Math.Max(0,wildShapeEndsAt-worldRounds)+"回合":"当前：原形",label);
        if(IsWildShaped){bool previous=GUI.enabled;GUI.enabled=page=="battle"&&map.Locked&&!rolling&&combat.PlayerCanAct&&combat.BonusActionAvailable&&fighter.AttacksRemaining==0;
            if(GUILayout.Button("恢复原形（附赠动作，不消耗次数）",button)&&combat.TryUseBonusAction())EndWildShape("主动恢复");GUI.enabled=previous;}
        GUILayout.Label("已知形态 "+knownBeastForms.Count+" / "+DruidFeatures.KnownFormLimit(classes[11])+"。首次战斗操作前可配置；长休后可替换1种。",small);
        foreach(var attack in BeastAttack.Supported){
            var beast=FindBeast(attack.Form);if(beast==null||!beast.EligibleForBaseWildShape(classes[11]))continue;
            bool known=knownBeastForms.Contains(beast.name);bool previous=GUI.enabled;
            if(!combat.BattleStarted){
                bool next=GUILayout.Toggle(known,"学习 "+attack.Chinese+" · "+beast.name,label);
                if(next&&!known&&knownBeastForms.Count<DruidFeatures.KnownFormLimit(classes[11]))knownBeastForms.Add(beast.name);
                if(!next&&known)knownBeastForms.Remove(beast.name);
                known=knownBeastForms.Contains(beast.name);
            }else if(OutsideCombat&&knownFormReplacement){
                if(GUILayout.Button((known?"替换掉 ":"换成 ")+attack.Chinese,button)){
                    if(known)knownToReplace=beast.name;
                    else if(knownToReplace!=""&&knownBeastForms.Remove(knownToReplace)){knownBeastForms.Add(beast.name);knownFormReplacement=false;knownToReplace="";}
                }
            }
            GUI.enabled=previous&&known;
            if(GUILayout.Button((selectedBeastForm==beast.name?"✓ ":"")+attack.Chinese+" · 护甲"+beast.armor+" · 速度"+beast.walk+"尺",button))selectedBeastForm=beast.name;
            GUI.enabled=previous;
        }
        var selected=FindBeast(selectedBeastForm);string reason=WildShapeInvalidReason(selected);
        GUILayout.Label(reason==""?"占地合法，可消耗附赠动作确认变身。":reason,small);
        bool enabled=GUI.enabled;GUI.enabled=reason=="";
        if(GUILayout.Button("确认变身",button))TryWildShape(selected);
        GUI.enabled=enabled;GUILayout.Label(wildShapeMessage,small);
        if(GUILayout.Button("查看全部63种野兽资料（其他形态待接入）",button))panel="beasts";
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
    void DrawWildShapePreview(Rect board)
    {
        if(panel!="druid")return;
        var form=FindBeast(selectedBeastForm);if(form==null)return;
        Vector2Int anchor;bool valid=map.FindPlayerResizePlacement(BeastSpaces(form),out anchor,FormReferenceCell);
        if(valid)Fill(BodyRect(board,anchor,BeastSpaces(form)),new Color(.3f,.75f,.3f,.35f));
    }
    void NaturalAttack()
    {
        if(!IsWildShaped||!CanUseFeature||map.DistanceFeet>5)return;
        if(fighter.AttacksRemaining==0){if(!combat.TryUseAction())return;fighter.BeginAttackAction(classes[0]);}
        fighter.ConsumeAttack();StartCoroutine(RollNaturalAttack());
    }
    IEnumerator RollNaturalAttack(bool opportunity=false)
    {
        var attack=BeastAttack.Find(wildForm.name);rolling=true;int blessing=combat.PlayerBlessed?UnityEngine.Random.Range(1,5):0,bane=combat.PlayerBaned?UnityEngine.Random.Range(1,5):0,hitBonus=attack.HitBonus+blessing-bane;
        bool advantage=fighter.ConsumeAdvantage(classes[0])||combat.MonsterParalyzed||(combat.MonsterProne&&map.DistanceFeet<=5);
        bool disadvantage=combat.ConsumeMockery(true);bool rollHigh=advantage&&!disadvantage,rollLow=disadvantage&&!advantage;
        advantage=combat.ConsumeGuidingLight(false)||advantage;rollHigh=advantage&&!disadvantage;rollLow=disadvantage&&!advantage;
        int first=UnityEngine.Random.Range(1,21),second=rollHigh||rollLow?UnityEngine.Random.Range(1,21):0;
        int value=rollHigh?Math.Max(first,second):rollLow?Math.Min(first,second):first;
        dice=attack.Chinese+" · "+attack.AttackName+(opportunity?"借机攻击":"");
        yield return PresentRoll(20,second>0?new[]{first,second}:new[]{first},value+" + 野兽命中/祝福 "+hitBonus+" / 护甲 "+combat.MonsterArmorClass);
        if(blessing>0)yield return PresentRoll(4,new[]{blessing},"祝福术：本次自然攻击 +"+blessing);
        if(bane>0)yield return PresentRoll(4,new[]{bane},"灾祸术：本次自然攻击 -"+bane);
        if(value==1||(value!=20&&value+hitBonus<combat.MonsterArmorClass)){history.Insert(0,dice+"，未命中。");fighter.RecordMiss(classes[0]);rolling=false;yield break;}
        bool critical=value==20||(combat.MonsterParalyzed&&map.DistanceFeet<=5);int damage=attack.Fixed;
        int count=attack.Count*(critical?2:1);int[] values=new int[count];for(int i=0;i<count;i++){values[i]=UnityEngine.Random.Range(1,attack.Sides+1);damage+=values[i];}
        if(count>0)yield return PresentRoll(attack.Sides,values,"野兽伤害骰 + 固定 "+attack.Fixed+(critical?"（暴击，伤害骰翻倍）":""));
        int extra=attack.ExtraCount*(critical?2:1);if(extra>0){int[] poison=new int[extra];for(int i=0;i<extra;i++){poison[i]=UnityEngine.Random.Range(1,attack.ExtraSides+1);damage+=poison[i];}yield return PresentRoll(attack.ExtraSides,poison,"额外毒素伤害骰");}
        damage=Math.Max(0,damage);var markDamage=new ExtraMarkDamage();yield return RollMarkDamage(false,critical,markDamage);damage+=markDamage.Value;combat.ApplyDamage(damage);
        if(attack.KnockProne&&combat.Hp>0&&monsterProfile.SizeIndex<=2){combat.MonsterProne=true;history.Insert(0,"狼啮咬使中型或更小目标倒地。");}
        history.Insert(0,attack.Chinese+attack.AttackName+"："+damage+"点"+attack.DamageType+(extra>0?"及毒素":"")+"伤害"+(critical?"（暴击）":"")+"。");
        dice=attack.Chinese+attack.AttackName+" · "+damage;CheckBattleOutcome();rolling=false;
    }
}
