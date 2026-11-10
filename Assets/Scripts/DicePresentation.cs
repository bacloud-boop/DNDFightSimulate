using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    bool simpleDice;
    string rollResultText="D20",rollContext="准备投骰";
    bool rollResultReady;
    string rollCalculation="";
    const float DiceResultHoldSeconds=2f;
    IEnumerator PresentRoll(int sides,int[] results,string calculation="")
    {
        rollContext=dice;rollResultReady=false;rollCalculation=calculation;
        string values=string.Join("、",results);
        rollResultText="D"+sides+"："+values;
        if(results.Length>1 && sides!=20)rollResultText+="\n骰值合计："+System.Linq.Enumerable.Sum(results);
        if(!simpleDice)yield return diceStage.Roll(sides,results,RollDuration,PlayRoll);
        else PlayRoll();
        rollResultReady=true;
        dice=rollContext+" · 骰值 ["+values+"]"+(calculation!=""?" · "+calculation:"");
        yield return new WaitForSecondsRealtime(DiceResultHoldSeconds);
    }
    void SetSimpleDice(bool value){simpleDice=value;PlayerPrefs.SetInt("SimpleDice",value?1:0);PlayerPrefs.Save();}
}
