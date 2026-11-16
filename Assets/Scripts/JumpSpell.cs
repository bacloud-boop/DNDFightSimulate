using UnityEngine;
public sealed partial class TrainingGame
{
    readonly JumpEffect jumpEffect=new JumpEffect();
    string jumpColumn="1",jumpRow="1";
    bool jumpWilling;
    bool JumpEnvironmentAllowed(bool player)=>JumpEffect.EnvironmentAllowed(false,false,true,player?combat.PlayerProne:combat.MonsterProne);
    bool CanUseJump(bool player,Vector2Int cell,bool tactical=false)=>JumpEnvironmentAllowed(player)&&jumpEffect.Available(player,worldRounds,combat.Round)&&map.CanJump(player,cell,tactical);
    string JumpSpellError(bool player,int slot,bool both)
    {
        if(both&&slot<2)return "二环起才能同时选择双方。";
        if((!player||both)&&!jumpWilling)return "跳跃术需要自愿目标。";
        if((!player||both)&&(combat.Hp==0||map.DistanceFeet>5))return "请接近存活木桩至5尺。";
        return worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出可记录范围。":"";
    }
    void ApplyJumpSpell(bool player){jumpEffect.Grant(player,worldRounds);history.Insert(0,"跳跃术："+(player?"玩家":"木桩")+"每回合一次，花10尺所选移动力跳至30尺内合法落点，持续1分钟，无需专注；潜水或拖行遗体时不可跳跃。");}
    void DrawJumpChoices()
    {
        if(!directTargetPlayer||directBothTargets)jumpWilling=GUILayout.Toggle(jumpWilling,"确认木桩自愿接受跳跃术",label);
    }
    void DrawJumpAction()
    {
        if(!jumpEffect.Active(true,worldRounds)||combat.Aftermath)return;
        GUI.Label(new Rect(380,455,205,28),"跳跃落点 · 列／行",small);
        jumpColumn=GUI.TextField(new Rect(380,485,98,34),jumpColumn,fieldStyle);jumpRow=GUI.TextField(new Rect(487,485,98,34),jumpRow,fieldStyle);
        int x=0,y=0;var valid=int.TryParse(jumpColumn,out x)&&int.TryParse(jumpRow,out y);var cell=new Vector2Int(x-1,y-1);
        bool ready=valid&&page=="battle"&&panel==""&&!rolling&&!spellTargeting&&draggingToken<0&&combat.PlayerCanAct&&CanUseJump(true,cell,tacticalMovement);
        if(Btn(new Rect(380,530,205,48),combat.PlayerProne?"请先起身":tacticalMovement?"战术跳跃（10尺）":"跳跃（10尺移动）",ready))StartCoroutine(ResolveMovement(true,cell,tacticalMovement,false,true));
    }
}
