using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly MonsterSpellSequence monsterSpells=new MonsterSpellSequence();
    readonly List<MonsterSpellEntry> monsterSpellDraft=new List<MonsterSpellEntry>();
    int monsterSpellAbilityDraft=4,monsterSpellPick,monsterSpellSlot=1;
    string monsterCasterLevelDraft="1",monsterSpellMessage="";
    bool monsterCounterspellDraft;
    bool monsterSpellsOriginal;
    Vector2 monsterSpellScroll;
    void OpenMonsterSpells(bool original=false)
    {
        monsterSpellsOriginal=original;
        monsterSpellDraft.Clear();
        if(original){foreach(var entry in originalDraft.Spells)monsterSpellDraft.Add(entry);monsterSpellAbilityDraft=originalDraft.SpellAbility;monsterCasterLevelDraft=originalDraft.SpellLevel.ToString();monsterCounterspellDraft=originalDraft.CanCounterspell;}
        else {foreach(var entry in monsterSpells.Entries)monsterSpellDraft.Add(entry);monsterSpellAbilityDraft=monsterSpells.Ability;monsterCasterLevelDraft=monsterSpells.CasterLevel.ToString();monsterCounterspellDraft=monsterSpells.Counterspell;}
        monsterSpellMessage="";OpenPanel("monsterSpells");
    }
    void DrawMonsterSpells()
    {
        Surface(new Rect(770,110,480,745),paperTexture);
        if(Btn(new Rect(1140,125,90,46),"关闭")){panel=monsterSpellsOriginal?"originalMonster":"";return;}
        GUILayout.BeginArea(new Rect(790,185,440,640));monsterSpellScroll=GUILayout.BeginScrollView(monsterSpellScroll);
        GUILayout.Label(monsterSpellsOriginal?"生前形态 · 法术草稿":"怪物法术",heading);
        GUILayout.Label("按列表循环，每回合取一项；无合法施法则用原有自定义攻击。附赠动作治疗后仍可攻击。范围法术避开自身并优先覆盖更多敌人；法术位暂不限。",small);
        bool previous=GUI.enabled;GUI.enabled=!combat.BattleStarted&&!rolling;
        GUILayout.Label("施法属性",label);monsterSpellAbilityDraft=GUILayout.SelectionGrid(monsterSpellAbilityDraft,PlayerAttributes.Names,3,button);
        GUILayout.Label("施法等级（1～20，仅用于戏法强化）",small);monsterCasterLevelDraft=IntegerField(monsterCasterLevelDraft,false);
        int previewAbility=monsterAttributes[monsterSpellAbilityDraft],previewProficiency=combat.MonsterProficiencyBonus;
        if(monsterSpellsOriginal){if(!int.TryParse(originalAbilityInputs[monsterSpellAbilityDraft],out previewAbility))previewAbility=originalDraft.Abilities[monsterSpellAbilityDraft];if(!int.TryParse(originalStatInputs[2],out previewProficiency))previewProficiency=originalDraft.Proficiency;}
        GUILayout.Label("法术命中 "+MonsterSpellSequence.AttackBonus(previewProficiency,previewAbility)+" · DC "+MonsterSpellSequence.SaveDc(previewProficiency,previewAbility),small);
        monsterCounterspellDraft=GUILayout.Toggle(monsterCounterspellDraft,"具备法术反制：满足条件时自动消耗反应",label);
        for(int i=0;i<monsterSpellDraft.Count;i++){
            var entry=monsterSpellDraft[i];var definition=MonsterSpellCatalog.Find(entry.Name);
            GUILayout.Label((i+1)+". "+definition.Chinese+" · "+(entry.Slot==0?"戏法":entry.Slot+"环"),small);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("上移",button)&&i>0){monsterSpellDraft[i]=monsterSpellDraft[i-1];monsterSpellDraft[i-1]=entry;}
            if(GUILayout.Button("删除",button)){monsterSpellDraft.RemoveAt(i);GUILayout.EndHorizontal();break;}
            GUILayout.EndHorizontal();
        }
        var names=new string[MonsterSpellSequence.Supported.Length];for(int i=0;i<names.Length;i++)names[i]=MonsterSpellCatalog.Find(MonsterSpellSequence.Supported[i]).Chinese;
        monsterSpellPick=GUILayout.SelectionGrid(monsterSpellPick,names,2,new GUIStyle(button){fontSize=17,fixedHeight=40});
        string chosen=MonsterSpellSequence.Supported[monsterSpellPick];int minimum=MonsterSpellSequence.BaseLevel(chosen);
        monsterSpellSlot=minimum==0?0:Mathf.Clamp(monsterSpellSlot,minimum,9);
        if(minimum>0){GUILayout.Label("施法环阶",small);string[] slots=new string[10-minimum];for(int i=0;i<slots.Length;i++)slots[i]=(minimum+i).ToString();monsterSpellSlot=minimum+GUILayout.SelectionGrid(monsterSpellSlot-minimum,slots,5,button);}
        if(GUILayout.Button("添加到循环列表",button))monsterSpellDraft.Add(new MonsterSpellEntry(chosen,monsterSpellSlot));
        if(GUILayout.Button("确认更改",button)){
            int level;var target=monsterSpellsOriginal?new MonsterSpellSequence():monsterSpells;
            if(!int.TryParse(monsterCasterLevelDraft,out level)||!target.Configure(monsterSpellAbilityDraft,level,monsterCounterspellDraft,monsterSpellDraft))monsterSpellMessage="施法等级须为1～20整数，法术环阶须合法。";
            else if(monsterSpellsOriginal){originalDraft.SpellAbility=target.Ability;originalDraft.SpellLevel=target.CasterLevel;originalDraft.CanCounterspell=target.Counterspell;originalDraft.Spells=monsterSpellDraft.ToArray();monsterSpellMessage="已写入生前草稿，请返回生前资料面板应用保存。";}
            else monsterSpellMessage="已保存，循环从第一项开始。";
        }
        GUI.enabled=previous;GUILayout.Label(monsterSpellMessage,small);GUILayout.EndScrollView();GUILayout.EndArea();
    }
}
