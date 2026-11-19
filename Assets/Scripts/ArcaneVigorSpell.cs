using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly int[] vigorDice=new int[PlayerClasses.Names.Length];
    int[] VigorLevels(){var levels=new int[vigorDice.Length];for(int i=0;i<levels.Length;i++)levels[i]=classes[i];return levels;}
    string ArcaneVigorError(bool self,int slot)
    {
        if(!self)return "活力奥法只能治疗自身。";
        return ArcaneVigorRules.Valid(VigorLevels(),hitDiceSpent,vigorDice,slot)?"":"请选择1～"+slot+"颗尚未消耗的生命骰。";
    }
    void DrawArcaneVigorChoices()
    {
        int total=0;foreach(int count in vigorDice)total+=count;
        GUILayout.Label("消耗生命骰（与短休共用），已选 "+total+" / "+directSlotLevel+"；只加一次施法调整值，不加体质。",small);
        for(int i=0;i<vigorDice.Length;i++)if(classes[i]>0||vigorDice[i]>0){
            GUILayout.Label(PlayerClasses.Names[i]+" d"+classes.HitDie(i)+" · 剩余 "+System.Math.Max(0,classes[i]-hitDiceSpent[i])+" · 选择 "+vigorDice[i],small);
            bool enabled=GUI.enabled;GUILayout.BeginHorizontal();
            GUI.enabled=enabled&&vigorDice[i]>0;if(GUILayout.Button("－",button))vigorDice[i]--;
            GUI.enabled=enabled&&total<directSlotLevel&&vigorDice[i]<classes[i]-hitDiceSpent[i];if(GUILayout.Button("＋",button))vigorDice[i]++;
            GUI.enabled=enabled;GUILayout.EndHorizontal();
        }
    }
    IEnumerator ResolveArcaneVigor(int caster,int slot)
    {
        if(!ArcaneVigorRules.TrySpend(VigorLevels(),hitDiceSpent,vigorDice,slot))yield break;
        var chosen=(int[])vigorDice.Clone();int amount=0;
        for(int i=0;i<chosen.Length;i++)if(chosen[i]>0){
            int sides=classes.HitDie(i);var rolls=new int[chosen[i]];
            for(int n=0;n<rolls.Length;n++){rolls[n]=Random.Range(1,sides+1);amount+=rolls[n];}
            dice="活力奥法 · "+PlayerClasses.Names[i]+"生命骰";
            yield return PresentRoll(sides,rolls,"累计骰值 "+amount+"；本次生命骰已消耗");
        }
        int modifier=AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),healing=System.Math.Max(0,amount+modifier);
        int actual=combat.HealPlayer(healing);ApplySteedLifeBond(actual,2);
        history.Insert(0,"活力奥法：生命骰合计 "+amount+" + 施法调整值 "+modifier+" = "+healing+"；实际恢复 "+actual+" HP。生命骰与短休共享，不因治疗受阻或满血返还。");
        System.Array.Clear(vigorDice,0,vigorDice.Length);
    }
}
