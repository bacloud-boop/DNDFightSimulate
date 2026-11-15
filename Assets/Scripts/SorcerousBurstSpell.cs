using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    static readonly string[] BurstTypes={"强酸","寒冷","火焰","闪电","毒素","心灵","雷鸣"};
    int burstType;bool burstExtra=true;
    void DrawSorcerousBurstChoices(){GUILayout.Label("本次伤害类型",small);burstType=GUILayout.SelectionGrid(burstType,BurstTypes,3,new GUIStyle(button){fontSize=16,fixedHeight=34});burstExtra=GUILayout.Toggle(burstExtra,"掷出8时自动追加伤害骰（受施法调整值上限限制）",label);}
    sealed class BurstDamage {public int Extra;}
    IEnumerator RollSorcerousBurstExtra(int[] first,int modifier,bool critical,BurstDamage result)
    {
        if(!burstExtra)yield break;
        int used=0;int[] last=first;
        while(true){
            int count=SorcerousBurstRules.NextCount(last,used,modifier,critical);if(count==0)yield break;
            last=new int[count];for(int i=0;i<count;i++){last[i]=Random.Range(1,9);result.Extra+=last[i];}used+=count;
            yield return PresentRoll(8,last,"术法迸发追加骰："+used+" / "+SorcerousBurstRules.MaximumExtra(modifier,critical)+"，追加伤害累计 "+result.Extra);
        }
    }
}
