using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    IEnumerator ResolveScorchingRays(int caster,int slot)
    {
        int count=slot+1;var targets=new bool[count];for(int i=0;i<count;i++)targets[i]=missileTargetsPlayer[i];
        for(int i=0;i<count;i++){
            if(!combat.PlayerCanAct){history.Insert(0,"灼热射线中止：施法者不能继续行动或战斗已结束。");yield break;}
            if(!targets[i]&&combat.Hp==0){history.Insert(0,"灼热射线跳过已死亡目标。");continue;}
            var beam=new DirectSpell("Scorching Ray Beam","灼热射线 "+(i+1)+" / "+count,"",2,120,6,attack:true,damageType:"火焰",baseDice:2);
            yield return ResolveDirectSpell(beam,targets[i],caster,2,true);
        }
    }
}
