using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    int skillBackground=-1,checkKind,checkSkill,checkAbility,checkTool,checkCircumstance;
    int checkAuxTool=-1;bool checkTracking,checkToolAvailable,skillDirty=true,monsterSkillsDirty;
    bool tacticalMindPending;int tacticalMindDecision;
    string checkDc="10",skillMessage="",skillPicker="",skillSignature="";
    int[] playerSkillTiers=new int[55],monsterSkillTiers=new int[18],monsterSkillDraft=new int[18];
    readonly Dictionary<string,int> skillChoices=new Dictionary<string,int>();
    Vector2 skillScroll,monsterSkillsScroll;
    bool SkillProfileReady=>!skillDirty&&skillSignature==SkillCreationRules.Signature(classes);
    void DrawGrant(SkillGrant grant)
    {
        int chosen=SkillCreationRules.Choice(grant,skillChoices);
        if(grant.Options.Length==1){GUILayout.Label(grant.Title+"："+SkillCreationRules.Name(chosen),small);return;}
        if(GUILayout.Button(grant.Title+"："+(chosen<0?"请选择":SkillCreationRules.Name(chosen)),button))skillPicker=skillPicker==grant.Id?"":grant.Id;
        if(skillPicker!=grant.Id)return;
        int index=Array.IndexOf(grant.Options,chosen);string[] names=grant.Options.Select(x=>(x<18?"技能 · ":"工具 · ")+SkillCreationRules.Name(x)).ToArray();
        int next=GUILayout.SelectionGrid(index,names,2,new GUIStyle(button){fontSize=15,fixedHeight=33});
        if(next!=index&&next>=0){skillChoices[grant.Id]=grant.Options[next];skillDirty=true;skillPicker="";}
    }
    void DrawSkillPanel()
    {
        Surface(new Rect(350,110,480,745),paperTexture);if(Btn(new Rect(720,125,90,46),"关闭")){panel="";return;}
        GUILayout.BeginArea(new Rect(375,185,430,635));skillScroll=GUILayout.BeginScrollView(skillScroll);
        GUILayout.Label("属性、技能与工具检定",heading);
        GUILayout.Label("手动检定消耗1个动作。自然1/20不自动失败/成功。",small);
        bool previous=GUI.enabled;GUI.enabled=!combat.BattleStarted;
        GUILayout.Label("角色创建 · 背景与熟练",heading);
        int background=GUILayout.SelectionGrid(skillBackground,SkillSourceTables.BackgroundNames,4,new GUIStyle(button){fontSize=15,fixedHeight=32});if(background!=skillBackground){skillBackground=background;skillDirty=true;}
        foreach(var grant in SkillCreationRules.BaseGrants(classes,skillBackground))DrawGrant(grant);
        List<SkillGrant> replacements;string previewError;int[] preview=SkillCreationRules.Preview(classes,skillBackground,skillChoices,out replacements,out previewError);
        GUILayout.Label("重复熟练不额外补选；可选项须在原列表内避开已有熟练。",small);
        foreach(var grant in SkillCreationRules.ExpertiseGrants(classes,preview))DrawGrant(grant);
        GUILayout.Label("背景在此提供技能、工具及熟习/巧匠/音乐家的熟练选择；属性加点、物品制造、装备和其他专长增益按各自接入状态处理。",small);
        if(GUILayout.Button("应用角色技能资料",button)){
            int[] built;string error;if(SkillCreationRules.TryBuild(classes,skillBackground,skillChoices,out built,out error)){playerSkillTiers=built;skillSignature=SkillCreationRules.Signature(classes);skillDirty=false;skillMessage="技能与工具熟练已应用。";}else skillMessage=error;
        }
        GUI.enabled=previous;GUILayout.Label(skillMessage,small);
        if(!SkillProfileReady)GUILayout.Label("完成角色创建并应用后才能检定；职业改变后需重新确认。",small);
        GUILayout.Space(12);GUILayout.Label("本次检定",heading);
        checkKind=GUILayout.SelectionGrid(checkKind,new[]{"纯属性","技能","工具"},3,new GUIStyle(button){fontSize=16,fixedHeight=36});
        if(checkKind==0)checkAbility=GUILayout.SelectionGrid(checkAbility,PlayerAttributes.Names,3,new GUIStyle(button){fontSize=16,fixedHeight=32});
        if(checkKind==1){checkSkill=GUILayout.SelectionGrid(checkSkill,SkillCheckRules.Names,3,new GUIStyle(button){fontSize=16,fixedHeight=32});
            if(GUILayout.Button("辅助工具："+(checkAuxTool<0?"无":SkillSourceTables.ToolNames[checkAuxTool]),button))skillPicker=skillPicker=="aux"?"":"aux";
            if(skillPicker=="aux"){var names=new[]{"无"}.Concat(SkillSourceTables.ToolNames).ToArray();int selected=GUILayout.SelectionGrid(checkAuxTool+1,names,2,new GUIStyle(button){fontSize=15,fixedHeight=31});checkAuxTool=selected-1;}
        }
        if(checkKind==2)checkTool=GUILayout.SelectionGrid(checkTool,SkillSourceTables.ToolNames,2,new GUIStyle(button){fontSize=15,fixedHeight=31});
        bool tool=checkKind==2||checkKind==1&&checkAuxTool>=0;
        if(tool)checkToolAvailable=GUILayout.Toggle(checkToolAvailable,"确认此处有可用工具、形态可操作且工具适用",label);
        checkCircumstance=GUILayout.SelectionGrid(checkCircumstance,new[]{"正常","情境优势","情境劣势"},3,new GUIStyle(button){fontSize=15,fixedHeight=34});
        bool canTrack=checkKind==1&&(checkSkill==12||checkSkill==13)&&mark.Active&&!mark.Hex;
        GUI.enabled=canTrack;checkTracking=GUILayout.Toggle(canTrack&&checkTracking,canTrack?"此次检定用于寻找标记目标（"+(mark.TargetPlayer?"玩家自身":"木桩")+"）":"当前没有适用的猎人印记寻找检定",label);GUI.enabled=previous;
        if(checkKind==1&&checkSkill==12)checkVisual=GUILayout.Toggle(checkVisual,"此察觉检定依赖视觉，观察木桩",label);
        GUILayout.Label("本次DC（整数）",label);checkDc=IntegerField(checkDc,true);
        int dc;bool validDc=int.TryParse(checkDc,out dc);
        bool canRoll=SkillProfileReady&&page=="battle"&&map.Locked&&!rolling&&!spellTargeting&&!reactionPending&&!shieldPending&&draggingToken<0&&combat.ActionAvailable&&validDc&&(!tool||checkToolAvailable);
        GUI.enabled=canRoll;if(GUILayout.Button("进行检定（消耗动作）",button)){panel="";StartCoroutine(RollSkillCheck(dc));}GUI.enabled=previous;
        if(!validDc)GUILayout.Label("DC必须为有效整数。",small);
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
    void DrawMonsterSkills()
    {
        Surface(new Rect(770,110,480,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"关闭")){panel="";return;}
        GUILayout.BeginArea(new Rect(790,185,440,635));monsterSkillsScroll=GUILayout.BeginScrollView(monsterSkillsScroll);
        GUILayout.Label("怪物技能资料",heading);GUILayout.Label("按资料卡录入，默认无熟练。本阶段不提供怪物手动检定。",small);
        bool previous=GUI.enabled;GUI.enabled=!combat.BattleStarted;
        for(int i=0;i<18;i++){GUILayout.Label(SkillCheckRules.Names[i],label);int next=GUILayout.SelectionGrid(monsterSkillDraft[i],new[]{"无熟练","熟练","专精"},3,new GUIStyle(button){fontSize=15,fixedHeight=32});if(next!=monsterSkillDraft[i]){monsterSkillDraft[i]=next;monsterSkillsDirty=true;}}
        if(GUILayout.Button("应用怪物技能资料",button)){Array.Copy(monsterSkillDraft,monsterSkillTiers,18);monsterSkillsDirty=false;skillMessage="怪物技能资料已应用。";}
        GUI.enabled=previous;if(monsterSkillsDirty)GUILayout.Label("修改尚未应用。",small);GUILayout.Label(skillMessage,small);GUILayout.EndScrollView();GUILayout.EndArea();
    }
    void DrawTacticalMindPrompt()
    {
        if(!tacticalMindPending)return;
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.9f));Surface(new Rect(465,260,670,350),paperTexture);
        GUI.Label(new Rect(495,285,610,50),"战术头脑 · 检定失败",heading);
        GUI.Label(new Rect(495,350,610,130),"是否为原检定增加1d10？转为成功才消耗1次回气；仍失败不消耗。此效果不治疗，也不消耗额外动作或附赠动作。",label);
        if(Btn(new Rect(495,525,285,55),"使用战术头脑"))tacticalMindDecision=1;
        if(Btn(new Rect(820,525,285,55),"放弃"))tacticalMindDecision=2;
    }
    IEnumerator RollSkillCheck(int dc)
    {
        if(!SkillProfileReady||!combat.TryUseAction())yield break;
        rolling=true;
        if(checkKind==1&&checkSkill==12&&checkVisual&&!PlayerSeesMonster){history.Insert(0,"视觉察觉自动失败：无法看见木桩，消耗1动作。");dice="视觉察觉 · 自动失败";rolling=false;yield break;}
        int kind=checkKind,skill=kind==1?checkSkill:-1,tool=kind==2?checkTool:kind==1?checkAuxTool:-1;
        int ability=kind==0?checkAbility:kind==1?SkillCheckRules.Abilities[skill]:SkillSourceTables.ToolAbilities[tool];
        int tier=skill>=0?playerSkillTiers[skill]:0;bool toolProf=tool>=0&&playerSkillTiers[18+tool]>0;
        int effectiveTier=Math.Max(tier,toolProf?1:0),score=EffectivePlayerAbility(ability);
        int modifier=SkillCheckRules.Modifier(score,effectiveTier,classes.ProficiencyBonus,skill>=0,classes[5]>=2);
        bool beastProficient=false;
        if(IsWildShaped&&skill>=0&&wildForm.skillModifiers!=null&&wildForm.skillModifiers.Length==18&&wildForm.skillModifiers[skill]>-999){
            int beast=wildForm.skillModifiers[skill],baseModifier=AcidSplash.Modifier(wildForm.abilities[ability]);beastProficient=beast>baseModifier;
            if(beastProficient){int beastTier=Math.Min(2,Math.Max(1,(beast-baseModifier)/2));modifier=Math.Max(modifier,Math.Max(beast,AcidSplash.Modifier(score)+classes.ProficiencyBonus*beastTier));}
        }
        bool advantage=checkBuff.Advantage(true,ability,worldRounds)||checkCircumstance==1||(skill>=0&&tier>0&&toolProf)||(checkTracking&&skill>=0&&(skill==12||skill==13)&&mark.Active&&!mark.Hex);
        bool disadvantage=(skill==12&&checkVisual&&Visibility(true,false)==1)||checkCircumstance==2||mark.CheckDisadvantage(true,ability),two=advantage!=disadvantage;
        int first=UnityEngine.Random.Range(1,21),second=two?UnityEngine.Random.Range(1,21):0;
        int raw=SkillCheckRules.SelectDie(first,second,advantage,disadvantage),effective=SkillCheckRules.EffectiveDie(raw,classes[7]>=7,effectiveTier>0||beastProficient?1:0);
        string name=kind==0?PlayerAttributes.Names[ability]:kind==1?SkillCheckRules.Names[skill]:SkillSourceTables.ToolNames[tool];
        int guidanceBonus=0;
        if(checkBuff.Guided(true,skill,worldRounds)){guidanceBonus=UnityEngine.Random.Range(1,5);yield return PresentRoll(4,new[]{guidanceBonus},"指引术：本次"+name+"检定 +"+guidanceBonus);modifier+=guidanceBonus;}
        int total=effective+modifier;bool success=SkillCheckRules.Success(effective,modifier,dc);
        dice=name+"检定 · "+(advantage&&disadvantage?"优势劣势抵消":advantage?"优势":disadvantage?"劣势":"正常");
        yield return PresentRoll(20,two?new[]{first,second}:new[]{first},"采用 "+raw+(effective!=raw?"，可靠才能按10":"")+" + 调整 "+modifier+" = "+total+" / DC "+dc+"，"+(success?"成功":"失败"));
        int tacticalBonus=0;
        if(!success&&classes[0]>=2&&fighter.WindRemaining(classes[0])>0){
            tacticalMindDecision=0;tacticalMindPending=true;while(tacticalMindDecision==0)yield return null;tacticalMindPending=false;
            if(tacticalMindDecision==1){int extra=UnityEngine.Random.Range(1,11);yield return PresentRoll(10,new[]{extra},"战术头脑：原总值 "+total+" + "+extra+" / DC "+dc);bool turned=fighter.TacticalMind(classes[0],total,dc,extra);total+=extra;tacticalBonus=extra;success=turned;history.Insert(0,"战术头脑："+(turned?"检定成功，消耗1次回气":"仍失败，不消耗回气")+"；不治疗、不消耗额外动作或附赠动作。");}
        }
        history.Insert(0,dice+"：D20="+raw+(effective!=raw?"（可靠才能10）":"")+" + "+modifier+" + 战术头脑 "+tacticalBonus+" = "+total+" / DC "+dc+"，"+(success?"成功":"失败")+"，消耗1动作。");rolling=false;
    }
}
