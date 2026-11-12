public sealed class FighterFeatures
{
    public int SecondWindUsed { get; private set; }
    public int ActionSurgeUsed { get; private set; }
    public bool SurgedThisRound { get; private set; }
    int advantageTurnsRemaining;
    public bool NextAttackAdvantage { get; private set; }
    public int AttacksRemaining { get; private set; }
    public int AttacksPerAction(int level) => level>=20?4:level>=11?3:level>=5?2:1;
    public int WindMaximum(int level) => level>=10?4:level>=4?3:level>=1?2:0;
    public int WindRemaining(int level) => System.Math.Max(0,WindMaximum(level)-SecondWindUsed);
    public void ShortRest() {SecondWindUsed=System.Math.Max(0,SecondWindUsed-1);ActionSurgeUsed=0;}
    public void LongRest() {SecondWindUsed=0;ActionSurgeUsed=0;}
    public int SurgeRemaining(int level) => level>=2?(level>=17?2:1)-ActionSurgeUsed:0;
    public bool CanSurge(int level) => SurgeRemaining(level)>0 && !SurgedThisRound;
    public void UseWind() { SecondWindUsed++; }
    public void UseSurge() { ActionSurgeUsed++; SurgedThisRound=true; }
    public void BeginAttackAction(int level) { AttacksRemaining=AttacksPerAction(level); }
    public void CancelAttackAction(){AttacksRemaining=0;}
    public void ConsumeAttack() { if(AttacksRemaining>0)AttacksRemaining--; }
    public bool ConsumeAdvantage(int level) { bool value=level>=13 && NextAttackAdvantage;NextAttackAdvantage=false;advantageTurnsRemaining=0;return value; }
    public void RecordMiss(int level) { if(level>=13){NextAttackAdvantage=true;advantageTurnsRemaining=2;} }
    public void EndRound() { if(advantageTurnsRemaining>0 && --advantageTurnsRemaining==0)NextAttackAdvantage=false;SurgedThisRound=false;AttacksRemaining=0; }
    public void ResetBattle() {SecondWindUsed=0;ActionSurgeUsed=0;SurgedThisRound=false;NextAttackAdvantage=false;advantageTurnsRemaining=0;AttacksRemaining=0;}
}
