using System;
using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class SpellInterruption {public bool Cancelled;}
    bool counterspellPending;int counterspellDecision,counterspellClass=1;
    string counterspellIncoming="";
    SpellMetadata SpellInfo(string name)
    {
        if(spellMetadata==null)spellMetadata=JsonUtility.FromJson<SpellMetadataSet>(Resources.Load<TextAsset>("Rules2024/Spells").text);
        foreach(var entry in spellMetadata.spells)if(entry.name==name)return entry;return null;
    }
    bool MonsterSpellReady=>combat.MonsterCanAct&&!MonsterPolymorphed&&!monsterArmor.Untrained(monsterArmorTraining);
    bool CanPlayerCounterspell=>combat.PlayerCanAct&&!combat.PlayerReactionsBlocked&&combat.PlayerReactionAvailable&&CanCastInCurrentForm&&ArmorSpellAllowed("Counterspell")&&(classes[1]>0||classes[3]>0||classes[4]>0);
    long MonsterSpellDc=>MonsterSpellSequence.SaveDc(combat.MonsterProficiencyBonus,EffectiveMonsterAbility(monsterSpells.Ability));
    IEnumerator TryInterruptSpell(bool casterPlayer,string name,SpellInterruption result)
    {
        var metadata=SpellInfo(name);
        bool hasComponents=metadata!=null&&metadata.components!=null&&metadata.components.Length>0;
        bool sees=casterPlayer?MonsterSeesPlayer:PlayerSeesMonster;
        bool eligible=casterPlayer?MonsterSpellReady&&monsterSpells.Counterspell&&!combat.MonsterReactionsBlocked&&combat.MonsterReactionAvailable:CanPlayerCounterspell;
        if(!CounterspellRules.CanTrigger(map.DistanceFeet,sees,hasComponents,true,eligible))yield break;
        bool counterPlayer=!casterPlayer;
        if(counterPlayer){
            if(classes[counterspellClass]==0||counterspellClass!=1&&counterspellClass!=3&&counterspellClass!=4)counterspellClass=classes[1]>0?1:classes[3]>0?3:4;
            counterspellIncoming=metadata.chineseName;counterspellDecision=0;counterspellPending=true;
            dice="木桩正在施放"+counterspellIncoming+" · 等待法术反制选择";
            while(counterspellDecision==0)yield return null;
            counterspellPending=false;if(counterspellDecision!=1)yield break;
        }
        if(!combat.TryUseReaction(counterPlayer))yield break;
        long dc=counterPlayer?8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(counterspellClass)]):MonsterSpellDc;
        history.Insert(0,(counterPlayer?"玩家":"木桩")+"消耗反应施放法术反制。");
        // Counterspell is itself a spell. Spent reactions naturally bound the chain.
        var reply=new SpellInterruption();yield return TryInterruptSpell(counterPlayer,"Counterspell",reply);
        BreakOrdinaryInvisibility(counterPlayer,"施放法术反制");
        if(reply.Cancelled){history.Insert(0,"法术反制本身被打断，原法术继续。");yield break;}
        int value=UnityEngine.Random.Range(1,21);
        int modifier=casterPlayer?PlayerSavingThrowModifier(2):MonsterSavingThrowModifier(2);
        var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(casterPlayer,false,adjustment,2);modifier+=adjustment.Value;
        dice="法术反制 · "+(casterPlayer?"玩家":"木桩")+"体质豁免";
        yield return PresentRoll(20,new[]{value},value+" + "+modifier+" / DC "+dc);
        result.Cancelled=CounterspellRules.Interrupted(value,modifier,dc);
        history.Insert(0,result.Cancelled?"体质豁免失败："+metadata.chineseName+"被打断，施法所用动作／附赠动作／反应仍消耗，法术位不消耗。":"体质豁免成功：原法术继续生效。");
    }
    void DrawCounterspellPrompt()
    {
        if(!counterspellPending)return;
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.92f));Surface(new Rect(465,200,670,475),paperTexture);
        GUI.Label(new Rect(495,225,610,50),"法术反制 · 反应",heading);
        GUI.Label(new Rect(495,280,610,100),"木桩正在施放："+counterspellIncoming+"。\n消耗反应，使施法者进行体质豁免；失败则阻止本次法术。法术位暂不限。",label);
        int y=390;foreach(int index in new[]{1,3,4})if(classes[index]>0){if(Btn(new Rect(495,y,610,44),(counterspellClass==index?"✓ ":"")+PlayerClasses.Names[index]+" · DC "+(8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(index)]))))counterspellClass=index;y+=48;}
        if(Btn(new Rect(495,610,285,48),"施放法术反制"))counterspellDecision=1;
        if(Btn(new Rect(820,610,285,48),"放弃，保留反应"))counterspellDecision=2;
    }
}
