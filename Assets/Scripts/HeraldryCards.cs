using UnityEngine;

public sealed partial class TrainingGame
{
    Texture2D brassTexture,ironTexture,gemTexture;
    Texture2D[] emblems;
    int lastPlayerHp=-1,lastMonsterHp=-1;
    float playerFlashUntil,monsterFlashUntil;
    bool playerHealed,monsterHealed;
    void InitializeHeraldry()
    {
        brassTexture=Grain(new Color(.60f,.40f,.17f),false);
        ironTexture=Grain(new Color(.30f,.28f,.25f),true);
        gemTexture=HeraldryArt.Gem();emblems=new Texture2D[PlayerClasses.Names.Length+1];
        for(int i=0;i<emblems.Length;i++)emblems[i]=HeraldryArt.Create(i);
    }
    void ObserveVitalChanges()
    {
        if(lastPlayerHp>=0 && lastPlayerHp!=combat.PlayerHp){playerHealed=combat.PlayerHp>lastPlayerHp;playerFlashUntil=Time.unscaledTime+.65f;}
        if(lastMonsterHp>=0 && lastMonsterHp!=combat.Hp){monsterHealed=combat.Hp>lastMonsterHp;monsterFlashUntil=Time.unscaledTime+.65f;}
        lastPlayerHp=combat.PlayerHp;lastMonsterHp=combat.Hp;
    }
    GUIStyle PlaqueText(bool player,int size=18,TextAnchor alignment=TextAnchor.MiddleLeft)
    {
        return new GUIStyle(label){fontSize=size,alignment=alignment,normal={textColor=player?new Color(1,.88f,.60f):new Color(.88f,.84f,.75f)}};
    }
    void Rivet(float x,float y,bool player)
    {
        Fill(new Rect(x-4,y-4,9,9),new Color(.08f,.06f,.04f));
        Fill(new Rect(x-2,y-2,5,5),player?new Color(.85f,.65f,.3f):new Color(.5f,.46f,.4f));
        Fill(new Rect(x-1,y-1,3,1),new Color(.95f,.85f,.65f,.65f));
    }
    void EngravedRow(float x,float y,string name,string value,bool player)
    {
        GUI.Label(new Rect(x+20,y,128,25),name,PlaqueText(player,16));
        GUI.Label(new Rect(x+137,y,113,25),value,PlaqueText(player,17,TextAnchor.MiddleRight));
        Fill(new Rect(x+20,y+25,230,1),new Color(0,0,0,.13f));
    }
    void CardTab(Rect rect,string name,string destination,bool player)
    {
        var style=new GUIStyle(button){fontSize=18,normal={background=player?brassTexture:ironTexture,textColor=new Color(1,.91f,.72f)},hover={background=hoverTexture,textColor=Color.white}};
        bool before=GUI.enabled;GUI.enabled=panel==""&&draggingToken<0;
        if(GUI.Button(rect,name,style))OpenPanel(destination);
        GUI.enabled=before;
        Fill(new Rect(player?rect.x:rect.xMax-3,rect.y+6,3,rect.height-12),new Color(.9f,.65f,.28f,.6f));
    }
    void DrawHeraldryCard(bool player)
    {
        float x=player?35:1295;
        Color trim=player?new Color(.84f,.61f,.25f):new Color(.44f,.40f,.34f);
        Fill(new Rect(x+5,118,270,745),new Color(0,0,0,.4f));
        Fill(new Rect(x,110,270,745),trim);
        GUI.DrawTexture(new Rect(x+4,114,262,737),player?brassTexture:ironTexture);
        Fill(new Rect(x+12,122,246,721),player?new Color(.13f,.075f,.03f,.74f):new Color(.055f,.05f,.045f,.75f));
        for(int i=0;i<8;i++) {float y=163+i*83;Fill(new Rect(x+15,y,240,1),new Color(1,.85f,.6f,.045f));}
        Rivet(x+9,119,player);Rivet(x+261,119,player);Rivet(x+9,846,player);Rivet(x+261,846,player);
        GUI.Label(new Rect(x+17,132,236,35),player?"冒险者铭牌":"训练木桩",PlaqueText(player,26,TextAnchor.MiddleCenter));
        Fill(new Rect(x+35,177,200,169),new Color(0,0,0,.23f));
        GUI.DrawTexture(new Rect(x+53,182,164,164),emblems[player?classes.InitialClass:PlayerClasses.Names.Length],ScaleMode.ScaleToFit);
        GUI.Label(new Rect(x+18,352,234,49),player?ClassSummary():"木桩靶心 · 训练目标",PlaqueText(player,17,TextAnchor.MiddleCenter));
        int hp=player?combat.PlayerHp:combat.Hp,maxHp=player?combat.PlayerMaxHp:combat.MaxHp;
        float flash=(player?playerFlashUntil:monsterFlashUntil)-Time.unscaledTime;
        bool healed=player?playerHealed:monsterHealed;
        Color old=GUI.color;GUI.color=flash>0?(healed?new Color(.3f,1,.45f):new Color(1,.22f,.15f)):new Color(.75f,.12f,.14f);
        GUI.DrawTexture(new Rect(x+25,410,42,50),gemTexture,ScaleMode.ScaleToFit);GUI.color=old;
        GUI.Label(new Rect(x+78,407,169,24),"生命",PlaqueText(player,15));
        GUI.Label(new Rect(x+78,430,169,32),hp+" / "+maxHp,PlaqueText(player,25));
        Fill(new Rect(x+20,477,230,38),trim*new Color(.65f,.65f,.65f,1));
        GUI.Label(new Rect(x+28,479,214,32),"护甲等级  ·  "+(player?combat.PlayerArmorClass:combat.MonsterArmorClass),PlaqueText(player,21,TextAnchor.MiddleCenter));
        if(player){
            EngravedRow(x,530,"当前武器",selectedWeapon.Name,player);
            EngravedRow(x,554,"伤害 / 射程",selectedWeapon.DamageDice+" / "+selectedWeapon.RangeText,player);
            EngravedRow(x,578,"剩余动作",combat.ActionsRemaining+" / "+combat.ActionsThisRound,player);
            EngravedRow(x,602,"附赠动作",(combat.BonusActionAvailable?1:0)+" / 1",player);
            EngravedRow(x,626,"移动力",map.MovementRemaining+" / 30尺",player);
            EngravedRow(x,650,"战术转进移动",map.TacticalMovementRemaining+" 尺",player);
            EngravedRow(x,674,"反应",combat.PlayerReactionAvailable?"1 / 1":"0 / 1",player);
            EngravedRow(x,698,"本动作剩余攻击",fighter.AttacksRemaining.ToString(),player);
            EngravedRow(x,722,"回气",classes[0]>=1?fighter.WindRemaining(classes[0])+" / "+fighter.WindMaximum(classes[0]):"未解锁",player);
            EngravedRow(x,746,"动作如潮",classes[0]>=2?fighter.SurgeRemaining(classes[0])+" / "+(classes[0]>=17?2:1):"未解锁",player);
            GUI.Label(new Rect(x+20,769,230,55),combat.PlayerHp==0?combat.PlayerLifeLabel+"\n死亡豁免 成功"+combat.DeathSuccesses+" / 失败"+combat.DeathFailures: fighter.NextAttackAdvantage?"究明攻击 · 下次优势":fighter.SurgedThisRound?"动作如潮 · 本回合已用":"本场状态 · "+(combat.BattleStarted?"配置锁定":"可调整配置"),PlaqueText(player,16,TextAnchor.MiddleCenter));
            CardTab(new Rect(x+270,206,60,62),"属\n性","attributes",true);
            CardTab(new Rect(x+270,282,60,62),"武\n器","weapons",true);
            CardTab(new Rect(x+270,358,60,62),"职\n业","classes",true);
            CardTab(new Rect(x+270,434,60,62),"法\n术","spells",true);
        }else{
            EngravedRow(x,540,"熟练加值",combat.MonsterProficiencyBonus.ToString(),false);
            EngravedRow(x,575,"反击时机","回合结束",false);
            EngravedRow(x,610,"移动力",map.MonsterMovementRemaining+" / 30尺",false);
            EngravedRow(x,645,"与玩家距离",map.DistanceFeet+" 尺",false);
            EngravedRow(x,680,"攻击距离",combat.MonsterAttackRange+" 尺",false);
            EngravedRow(x,715,"反应",combat.MonsterReactionAvailable?"1 / 1":"0 / 1",false);
            GUI.Label(new Rect(x+20,755,230,64),rolling?"战斗结算中":"等候玩家行动\n配置 · "+(combat.BattleStarted?"已锁定":"可编辑"),PlaqueText(false,17,TextAnchor.MiddleCenter));
            CardTab(new Rect(x-60,206,60,62),"伤\n害","monster",false);
            CardTab(new Rect(x-60,282,60,62),"属\n性","monsterAttributes",false);
        }
    }
    void DisposeHeraldry()
    {
        if(brassTexture)Destroy(brassTexture);if(ironTexture)Destroy(ironTexture);if(gemTexture)Destroy(gemTexture);
        if(emblems!=null)foreach(var emblem in emblems)if(emblem)Destroy(emblem);
    }
}
