using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    string concentrationSpell="";
    int concentrationExpiresRound;
    int holdPersonDc;
    bool resolvingHoldPerson;
    bool IsConcentrating => concentrationSpell!="";
    void EndConcentration(string reason)
    {
        if(!IsConcentrating)return;
        history.Insert(0,concentrationSpell+"专注结束："+reason+"。");
        concentrationSpell="";combat.PlayerParalyzed=false;combat.MonsterParalyzed=false;
    }
    void RefreshConcentration()
    {
        if(!IsConcentrating||resolvingHoldPerson)return;
        if(combat.Hp==0)combat.MonsterParalyzed=false;
        if(combat.PlayerHp==0||combat.PlayerDead||combat.PlayerParalyzed)EndConcentration("施法者失能或死亡");
        else if(combat.Round>=concentrationExpiresRound)EndConcentration("持续时间结束");
        else if(concentrationSpell=="人类定身术"&&!combat.PlayerParalyzed&&!combat.MonsterParalyzed)EndConcentration("所有目标均已摆脱效果");
    }
    int PlayerSavingThrowModifier(int ability)
    {
        int modifier=AcidSplash.Modifier(EffectivePlayerAbility(ability));
        bool beastProficient=IsWildShaped&&wildForm.saves[ability]>AcidSplash.Modifier(wildForm.abilities[ability]);
        int result=modifier+((classes.SavingThrowProficient(ability)||beastProficient)?classes.ProficiencyBonus:0);
        return beastProficient?Mathf.Max(result,wildForm.saves[ability]):result;
    }
    IEnumerator MaintainConcentration(int damage)
    {
        if(!IsConcentrating||damage<=0)yield break;
        if(combat.PlayerHp==0||combat.PlayerDead||combat.PlayerParalyzed){EndConcentration("施法者失能或死亡");yield break;}
        int dc=ConcentrationRules.DamageSaveDc(damage),value=Random.Range(1,21),modifier=PlayerSavingThrowModifier(2);
        dice=concentrationSpell+" · 受伤专注豁免";
        yield return PresentRoll(20,new[]{value},value+" + 体质豁免 "+modifier+" / DC "+dc);
        if(value+modifier<dc)EndConcentration("专注豁免失败");else history.Insert(0,"专注豁免成功，维持"+concentrationSpell+"。");
    }
    IEnumerator RepeatHoldPersonSave()
    {
        if(!combat.MonsterParalyzed||concentrationSpell!="人类定身术"||combat.Hp==0)yield break;
        int value=Random.Range(1,21),modifier=AcidSplash.Modifier(monsterAttributes[3]);
        dice="木桩回合结束 · 人类定身术感知豁免";
        yield return PresentRoll(20,new[]{value},value+" + "+modifier+" / DC "+holdPersonDc);
        if(value+modifier>=holdPersonDc){combat.MonsterParalyzed=false;history.Insert(0,"木桩通过感知豁免，摆脱麻痹。");RefreshConcentration();}
        else history.Insert(0,"木桩感知豁免失败，继续麻痹。");
    }
    IEnumerator ResolveHoldPerson(DirectSpell spell,bool player,int caster,bool both)
    {
        EndConcentration("开始新的专注法术");
        resolvingHoldPerson=true;concentrationSpell=spell.Chinese;concentrationExpiresRound=combat.Round+10;
        holdPersonDc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        bool[] targets={both||!player,both||player};
        for(int i=0;i<2;i++)if(targets[i]){
            bool targetPlayer=i==1;
            if(!targetPlayer&&(!monsterProfile.IsHumanoid||combat.Hp==0))continue;
            if(targetPlayer&&combat.PlayerDead)continue;
            string target=targetPlayer?"玩家":"木桩";
            int value=Random.Range(1,21),modifier=targetPlayer?PlayerSavingThrowModifier(3):AcidSplash.Modifier(monsterAttributes[3]);
            dice="人类定身术 · "+target+"感知豁免";
            yield return PresentRoll(20,new[]{value},value+" + "+modifier+" / DC "+holdPersonDc);
            if(value+modifier<holdPersonDc){if(targetPlayer){combat.PlayerParalyzed=true;EndWildShape("麻痹导致失能");}else combat.MonsterParalyzed=true;history.Insert(0,target+"豁免失败，陷入麻痹。");}
            else history.Insert(0,target+"豁免成功，未受定身影响。");
        }
        resolvingHoldPerson=false;RefreshConcentration();
    }
}
