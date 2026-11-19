using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed partial class TrainingGame
{
    readonly SpeciesSpellUses playerSpeciesSpellUses=new SpeciesSpellUses();
    bool useSpeciesFreeCast=true,highElfCantripReplacement;
    SpeciesSpellGrant PlayerSpeciesSpell(string name)=>SpeciesPhysicalActive(true)?SpeciesSpellRules.Find(playerSpecies,classes.TotalLevel,name):null;
    string CasterLabel(int caster)=>caster==SpeciesSpellRules.Caster?playerSpecies.Name+" · 血统法术":PlayerClasses.Names[caster];
    string SpeciesSpellCastError(string name,int caster,int slot)
    {
        if(caster!=SpeciesSpellRules.Caster)return "";
        var grant=PlayerSpeciesSpell(name);if(grant==null)return "当前种族／血统或等级未授予该法术。";
        if(!playerSpeciesSpellUses.CanCast(grant,classes.ProficiencyBonus,grant.SpellLevel==0?0:slot,useSpeciesFreeCast))return "种族免费施法须使用法术基础环阶，且尚有本次长休的次数。";
        return "";
    }
    void ConsumeSpeciesSpell(string name,int caster)
    {
        if(caster!=SpeciesSpellRules.Caster)return;
        var grant=PlayerSpeciesSpell(name);if(grant==null||grant.SpellLevel==0)return;
        if(useSpeciesFreeCast){playerSpeciesSpellUses.Consume(grant,classes.ProficiencyBonus);history.Insert(0,"血统免费施法："+name+"，本次长休剩余 "+playerSpeciesSpellUses.Remaining(grant,classes.ProficiencyBonus)+" 次。");}
        else history.Insert(0,"血统法术：本次使用法术位施放（法术位资源暂不限），保留种族免费次数。");
    }
    void DrawSpeciesSpellResource(DirectSpell spell)
    {
        if(directCastingClass!=SpeciesSpellRules.Caster)return;
        var grant=PlayerSpeciesSpell(spell.Name);if(grant==null||grant.SpellLevel==0)return;
        long remaining=playerSpeciesSpellUses.Remaining(grant,classes.ProficiencyBonus);
        int next=GUILayout.SelectionGrid(useSpeciesFreeCast?0:1,new[]{"种族免费次数 · 剩 "+remaining,"使用法术位（暂不限）"},1,new GUIStyle(button){fontSize=15,fixedHeight=32});
        useSpeciesFreeCast=next==0;if(useSpeciesFreeCast)directSlotLevel=grant.SpellLevel;
        GUILayout.Label("免费次数在消耗施法行动时扣除；被反制仍消耗次数。种族免费施法不能升环。",small);
    }
    SpellMetadata[] WizardCantripOptions()
    {
        if(spellMetadata==null)spellMetadata=JsonUtility.FromJson<SpellMetadataSet>(Resources.Load<TextAsset>("Rules2024/Spells").text);
        return spellMetadata.spells.Where(s=>s.level==0&&Array.IndexOf(s.classes,"Wizard")>=0&&SpeciesSpellRules.ValidHighElfCantrip(s.name)).OrderBy(s=>s.name).ToArray();
    }
    void DrawHighElfCantrip(SpeciesProfile p)
    {
        if(p.Kind!=Species2024.Elf||p.Lineage!=1)return;
        var options=WizardCantripOptions();var names=options.Select(s=>(string.IsNullOrEmpty(s.chineseName)?s.name:s.chineseName)+(DirectSpell.Find(s.name)==null&&s.name!="Acid Splash"?"（效果待接入）":"")).ToArray();
        GUILayout.Label("高等精灵 · 法师戏法",small);int selected=Array.FindIndex(options,s=>s.name==p.HighElfCantrip);int next=GUILayout.SelectionGrid(selected,names,2,new GUIStyle(button){fontSize=14,fixedHeight=32});if(next>=0)p.HighElfCantrip=options[next].name;
    }
    void DrawSpeciesSpellSummary()
    {
        GUILayout.Space(10);GUILayout.Label("种族法术",heading);
        var grants=SpeciesSpellRules.Grants(playerSpecies,classes.TotalLevel).ToArray();
        if(grants.Length==0){GUILayout.Label("当前种族未授予法术。",small);return;}
        foreach(var grant in grants){var spell=DirectSpell.Find(grant.Name);GUILayout.Label((spell==null?grant.Name:spell.Chinese)+(spell==null&&grant.Name!="Acid Splash"?" · 效果待接入":" · 请从法术书选择种族施法来源")+(grant.SpellLevel>0?" · 免费剩余 "+playerSpeciesSpellUses.Remaining(grant,classes.ProficiencyBonus):""),small);}
        if(playerSpecies.Kind==Species2024.Elf&&playerSpecies.Lineage==1){
            bool old=GUI.enabled;GUI.enabled=!rolling&&(!combat.BattleStarted||highElfCantripReplacement)&&!PlayerInBeastForm;
            var draft=playerSpecies.Copy();DrawHighElfCantrip(draft);
            if(draft.HighElfCantrip!=playerSpecies.HighElfCantrip){playerSpecies.HighElfCantrip=draft.HighElfCantrip;highElfCantripReplacement=false;history.Insert(0,"高等精灵已将血统戏法更换为 "+draft.HighElfCantrip+"；再次更换须完成长休。");}
            GUI.enabled=old;
        }
    }
}
