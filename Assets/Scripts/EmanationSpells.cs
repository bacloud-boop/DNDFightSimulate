using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    bool emanationSelf,emanationMonster=true;
    static bool IsSimpleEmanation(string name) => name=="Thunderclap"||name=="Word of Radiance";
    void DrawEmanationPreview(Rect board)
    {
        if(panel!="spells"||!IsSimpleEmanation(selectedSpellName))return;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){var cell=new Vector2Int(x,y);if(map.DistanceToCell(true,cell)<=5&&(emanationSelf||!BattleMap.Overlap(cell,1,map.Player,map.PlayerSpaces)))Fill(CellRect(board,cell),new Color(.7f,.6f,.1f,.35f));}
    }
    void DrawEmanationTargets(DirectSpell spell)
    {
        GUILayout.Label("自身周围5尺光环，按整个占地计算范围。",small);
        emanationSelf=GUILayout.Toggle(emanationSelf,"将源点自身纳入范围",label);
        if(spell.Name=="Word of Radiance")emanationMonster=GUILayout.Toggle(emanationMonster,"选择范围内木桩",label);
        GUILayout.Label(map.DistanceFeet<=5?"木桩在范围内":"木桩在范围外",small);
    }
    IEnumerator ResolveSimpleEmanation(DirectSpell spell,int caster)
    {
        bool[] targets={combat.Hp>0&&map.DistanceFeet<=5&&(spell.Name=="Thunderclap"||emanationMonster),emanationSelf};
        bool[] failed=new bool[2];int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        if(!targets[0]&&!targets[1]){history.Insert(0,spell.Chinese+"：未选中范围内生物，已消耗动作。");yield break;}
        for(int i=0;i<2;i++)if(targets[i]){
            int value=Random.Range(1,21),modifier=i==0?AcidSplash.Modifier(monsterAttributes[2]):PlayerSavingThrowModifier(2);dice=spell.Chinese+" · "+(i==0?"木桩":"玩家")+"体质豁免";
            var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(i==1,false,adjustment);modifier+=adjustment.Value;
            yield return PresentRoll(20,new[]{value},value+" + "+modifier+" / DC "+dc);failed[i]=value+modifier<dc;
        }
        if(!failed[0]&&!failed[1]){history.Insert(0,spell.Chinese+"：目标均通过体质豁免，伤害0。");yield break;}
        int[] values=new int[AcidSplash.DiceCount(classes.TotalLevel)];int damage=0;for(int i=0;i<values.Length;i++){values[i]=Random.Range(1,7);damage+=values[i];}
        dice=spell.Chinese+" · 共享伤害骰";yield return PresentRoll(6,values,"豁免失败者受到 "+damage+" 点"+spell.DamageType+"伤害，成功者0");
        int monsterDamage=failed[0]?combat.AdjustTypedDamage(false,damage,DamageTypes.Index(spell.DamageType)):0,playerDamage=failed[1]?combat.AdjustTypedDamage(true,damage,DamageTypes.Index(spell.DamageType)):0;
        if(failed[0])combat.ApplyDamage(monsterDamage);if(failed[1])combat.ApplyPlayerDamage(playerDamage);
        history.Insert(0,spell.Chinese+"：木桩 "+monsterDamage+"，玩家 "+playerDamage+" 点"+spell.DamageType+"伤害。");
        if(failed[1])yield return MaintainConcentration(playerDamage);
        CheckBattleOutcome();
    }
}
