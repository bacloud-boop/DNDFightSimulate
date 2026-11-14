using UnityEngine;
public sealed partial class TrainingGame
{
    readonly CheckSpellEffect checkBuff=new CheckSpellEffect();
    int guidanceSkill,enhancePlayerChoice,enhanceMonsterChoice;
    static readonly int[] EnhanceAbilities={0,1,4,3,5};
    void DrawCheckBuffChoices(DirectSpell spell)
    {
        if(spell.Name=="Guidance"){
            GUILayout.Label("指引的技能",small);guidanceSkill=GUILayout.SelectionGrid(guidanceSkill,SkillCheckRules.Names,3,new GUIStyle(button){fontSize=15,fixedHeight=32});
            if(!directTargetPlayer)energyTargetWilling=GUILayout.Toggle(energyTargetWilling,"确认木桩自愿接受指引术",label);
        }
        if(spell.Name=="Enhance Ability"){
            string[] names={"力量","敏捷","智力","感知","魅力"};
            if(directTargetPlayer||directBothTargets){GUILayout.Label("玩家强化属性",small);enhancePlayerChoice=GUILayout.SelectionGrid(enhancePlayerChoice,names,3,new GUIStyle(button){fontSize=15,fixedHeight=32});}
            if(!directTargetPlayer||directBothTargets){GUILayout.Label("木桩强化属性",small);enhanceMonsterChoice=GUILayout.SelectionGrid(enhanceMonsterChoice,names,3,new GUIStyle(button){fontSize=15,fixedHeight=32});}
        }
    }
    void ApplyCheckBuff(DirectSpell spell,bool player)
    {
        EndConcentration("开始新的专注法术");concentrationSpell=spell.Chinese;concentrationExpiresRound=int.MaxValue;
        if(spell.Name=="Guidance"){
            checkBuff.Guidance(player,guidanceSkill,worldRounds);concentrationExpiresWorld=worldRounds+10;
            history.Insert(0,"指引术："+(player?"玩家":"木桩")+"的"+SkillCheckRules.Names[guidanceSkill]+"检定每次+1d4，专注至多1分钟，使用后不会消失。");
        }else{
            if(player||directBothTargets)checkBuff.Enhance(true,EnhanceAbilities[enhancePlayerChoice],worldRounds);
            if(!player||directBothTargets)checkBuff.Enhance(false,EnhanceAbilities[enhanceMonsterChoice],worldRounds);
            concentrationExpiresWorld=worldRounds+600;
            history.Insert(0,"强化属性："+(directBothTargets?"双方分别选择的属性":player?"玩家的"+PlayerAttributes.Names[EnhanceAbilities[enhancePlayerChoice]]:"木桩的"+PlayerAttributes.Names[EnhanceAbilities[enhanceMonsterChoice]])+"检定具有优势，专注至多1小时，不影响攻击或豁免。");
        }
        dice=spell.Chinese+" · 效果已生效";
    }
}
