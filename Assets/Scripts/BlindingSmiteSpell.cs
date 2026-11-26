using System.Collections;
using System.Collections.Generic;
public sealed partial class TrainingGame
{
    readonly SensoryAfflictions blindingSmite=new SensoryAfflictions();
    const long BlindingSmiteSaveChoice=long.MinValue;
    List<SensoryAffliction> blindingSaveOptions;SensoryAffliction blindingSaveSelected;
    static int BlindingSmiteDice(int slot,bool critical)=>slot>=3&&slot<=9?slot*(critical?2:1):0;
    bool SmiteBlinded(int target)=>(target==0||target==-1)&&blindingSmite.Blinded(target==0,worldRounds);
    int EndBlindingSmite(int target,long now)=>target==0||target==-1?blindingSmite.EndCondition(target==0,true,now):0;
    void ApplyBlindingSmiteHit(int target,SmiteHitResult hit)
    {
        if(!hit.Blinding||!BanishLiving(target)||(target!=0&&target!=-1))return;
        blindingSmite.Add(target==0,true,hit.BlindingDc,worldRounds);
        history.Insert(0,"致盲斩：目标直接陷入目盲，无初始豁免；回合末体质豁免成功结束。持续至多1分钟，无需专注。");
    }
    IEnumerator RepeatBlindingSmiteSave(SensoryAffliction effect)
    {
        int target=effect.Player?0:-1;
        if(!BanishLiving(target)||!blindingSmite.Pending(effect.Player,worldRounds).Contains(effect))yield break;
        var failed=new HashSet<int>();yield return RollMappedAbilitySave(target,0,effect.Dc,failed,"致盲斩 #"+effect.Id,2);
        if(!failed.Contains(target)){blindingSmite.End(effect.Id);history.Insert(0,"致盲斩 #"+effect.Id+"：体质豁免成功，该次施法结束；其他目盲来源仍保留。");}
    }
}
