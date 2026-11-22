using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    readonly SpeciesAbilityState playerSpeciesAbilities=new SpeciesAbilityState(),monsterSpeciesAbilities=new SpeciesAbilityState();
    Vector2 speciesActionScroll;
    SpeciesAbilityState SpeciesAbilities(bool player)=>player?playerSpeciesAbilities:monsterSpeciesAbilities;
    int SpeciesProficiency(bool player)=>player?classes.ProficiencyBonus:combat.MonsterProficiencyBonus;
    bool SpeciesStoneContact(bool player)
        =>!map.IsContained(player)&&!map.IsCarried(player)&&(!player||!map.Riding)&&map.StoneContact(player?map.Player:map.Monster,player?map.PlayerSpaces:map.MonsterSpaces);
    bool CanUseSpeciesBonus(bool player)
    {
        if(!map.Locked||!SpeciesPhysicalActive(player)||!(player?combat.PlayerCanAct:combat.MonsterCanAct)||!(player?combat.BonusActionAvailable:combat.MonsterBonusActionAvailable))return false;
        var kind=Species(player).Kind;
        if(kind!=Species2024.Orc&&kind!=Species2024.Dwarf)return false;
        return SpeciesAbilities(player).CanUse(kind,SpeciesProficiency(player),worldRounds);
    }
    IEnumerator UseSpeciesBonus(bool player)
    {
        if(!CanUseSpeciesBonus(player))yield break;
        if(!(player?combat.TryUseBonusAction():combat.TryUseMonsterBonusAction()))yield break;
        var kind=Species(player).Kind;int proficiency=SpeciesProficiency(player);
        if(!SpeciesAbilities(player).Use(kind,proficiency,worldRounds))yield break;
        string who=player?"玩家":"木桩";
        if(kind==Species2024.Orc){
            if(player)map.Dash();else map.GrantMonsterDash(combat.MonsterSpeed);
            // Existing project temporary-HP grants retain the greater pool.
            if(player)combat.GrantPlayerTemporaryHp(Mathf.Max(0,proficiency));else combat.GrantMonsterTemporaryHp(Mathf.Max(0,proficiency));
            dice=who+" · 肾上腺素激增";
            history.Insert(0,who+"消耗附赠动作疾走，增加当前速度的普通移动额度，获得 "+Mathf.Max(0,proficiency)+" 临时生命（不叠加，保留较高值）；剩余 "+SpeciesAbilities(player).Remaining(kind,proficiency)+" 次。");
        }else {
            dice=who+" · 石中精妙";
            history.Insert(0,who+"消耗附赠动作获得60尺震感，持续10分钟；须接触相连石质表面，水域不传递。震感定位不等于看见，仍保留不可见攻击影响；剩余 "+SpeciesAbilities(player).Remaining(kind,proficiency)+" 次。");
        }
        yield break;
    }
    IEnumerator TryMonsterSpeciesBonus(bool beforeSpell,bool unified=false)
    {
        if(unified?!(combat.MonsterBonusActionAvailable&&SpeciesPhysicalActive(false)&&combat.MonsterCanAct):!SpeciesTurnPolicy.ShouldAttempt(monsterSpecies.BonusPriority,beforeSpell,combat.MonsterBonusActionAvailable,SpeciesPhysicalActive(false),combat.MonsterCanAct))yield break;
        if(!monsterSpeciesAbilities.CanAutomaticallyUse(monsterSpecies.Kind,SpeciesProficiency(false),worldRounds))yield break;
        yield return UseSpeciesBonus(false);
    }
    bool SpeciesTremorDetects(bool viewerPlayer,bool targetPlayer)
    {
        if(viewerPlayer==targetPlayer||!SpeciesPhysicalActive(viewerPlayer)||Species(viewerPlayer).Kind!=Species2024.Dwarf||!SpeciesAbilities(viewerPlayer).StoneActive(worldRounds)||!SpeciesStoneContact(viewerPlayer)||!SpeciesStoneContact(targetPlayer))return false;
        if(targetPlayer?combat.PlayerDead:combat.Hp==0)return false;
        return map.DistanceFeet<=60&&map.SameStoneSurface(viewerPlayer?map.Player:map.Monster,viewerPlayer?map.PlayerSpaces:map.MonsterSpaces,targetPlayer?map.Player:map.Monster,targetPlayer?map.PlayerSpaces:map.MonsterSpaces);
    }
    string SpeciesTremorStatus(bool player)
    {
        if(!SpeciesPhysicalActive(player)||Species(player).Kind!=Species2024.Dwarf||!SpeciesAbilities(player).StoneActive(worldRounds))return "";
        return "\n震感："+(!SpeciesStoneContact(player)?"未接触石面":SpeciesTremorDetects(player,!player)?"已定位对方":"范围内未定位对方");
    }
    void DrawSpeciesSenseMap(Rect board)
    {
        if(SpeciesTremorDetects(true,false)){var rect=BodyRect(board,map.Monster,map.MonsterSpaces);GUI.Label(new Rect(rect.x,rect.y-19,120,19),"震感 · 木桩",new GUIStyle(small){normal={textColor=Color.yellow}});}
        if(SpeciesTremorDetects(false,true)){var rect=BodyRect(board,map.Player,map.PlayerSpaces);GUI.Label(new Rect(rect.x,rect.y-19,120,19),"木桩震感 · 玩家",new GUIStyle(small){normal={textColor=Color.yellow}});}
    }
    void DrawSpeciesActions()
    {
        if(playerSpecies.Kind==Species2024.Dragonborn){DrawBreathPanel(0);return;}
        Surface(new Rect(350,110,480,745),paperTexture);if(Btn(new Rect(720,125,90,46),"关闭")){panel="";return;}
        GUILayout.BeginArea(new Rect(375,185,430,630));speciesActionScroll=GUILayout.BeginScrollView(speciesActionScroll);
        GUILayout.Label("种族特性 · "+playerSpecies.Name,heading);
        var kind=playerSpecies.Kind;int proficiency=SpeciesProficiency(true);
        if(kind==Species2024.Orc||kind==Species2024.Dwarf){
            GUILayout.Label(kind==Species2024.Orc?"肾上腺素激增：附赠动作疾走，并获得等于熟练加值的临时生命；短休或长休恢复次数。":"石中精妙：附赠动作获得60尺震感，持续10分钟；须接触石质表面，长休恢复次数。震感不视为视觉。",label);
            GUILayout.Label("剩余 "+playerSpeciesAbilities.Remaining(kind,proficiency)+" / "+Mathf.Max(0,proficiency)+SpeciesTremorStatus(true),small);
            bool old=GUI.enabled;GUI.enabled=page=="battle"&&!rolling&&!spellTargeting&&!reactionPending&&draggingToken<0&&CanUseSpeciesBonus(true);
            if(GUILayout.Button(kind==Species2024.Orc?"使用肾上腺素激增（附赠动作）":"使用石中精妙（附赠动作）",button)){panel="";StartCoroutine(UseSpeciesBonus(true));}
            GUI.enabled=old;
            if(!SpeciesPhysicalActive(true))GUILayout.Label("当前兽形不具有原种族的身体特性。",small);
        }else if(kind==Species2024.Aasimar)DrawHealingHandsChoices(0);
        else GUILayout.Label("当前种族的主动特性仍在接入；已完成的被动效果照常结算。",label);
        DrawSpeciesSpellSummary();
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
}
