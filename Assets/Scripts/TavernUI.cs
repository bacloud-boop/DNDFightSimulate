using UnityEngine;

public sealed partial class TrainingGame
{
    const float CanvasWidth=1600, CanvasHeight=900;
    TavernDice diceStage;
    TavernMiniatures miniatures;
    bool fastAnimation;
    float drawerProgress;
    Vector2 logScroll, panelScroll;
    Texture2D woodTexture, paperTexture, redTexture, hoverTexture;
    GUIStyle small, centered, cardTitle, lightTitle, fieldStyle;
    readonly Color ink=new Color(.23f,.13f,.07f), gold=new Color(.66f,.43f,.18f);
    float RollDuration => fastAnimation?.45f:1.4f;

    Texture2D Grain(Color color,bool wood)
    {
        var texture=new Texture2D(128,128,TextureFormat.RGBA32,false);
        for(int y=0;y<128;y++)for(int x=0;x<128;x++){
            float n=Mathf.PerlinNoise(x*(wood?.035f:.3f),y*(wood?.5f:.3f));
            float shade=wood?(.7f+n*.3f):(.91f+n*.09f);
            texture.SetPixel(x,y,new Color(color.r*shade,color.g*shade,color.b*shade));
        }
        texture.wrapMode=TextureWrapMode.Repeat;texture.Apply();return texture;
    }
    void InitializePresentation()
    {
        var background=new GameObject("Tavern background camera");background.transform.SetParent(transform,false);
        var camera=background.AddComponent<Camera>();camera.cullingMask=0;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.075f,.035f,.015f);camera.depth=-10;
        woodTexture=Grain(new Color(.26f,.125f,.057f),true);
        paperTexture=Grain(new Color(.92f,.81f,.59f),false);
        redTexture=Grain(new Color(.43f,.09f,.065f),false);
        hoverTexture=Grain(new Color(.58f,.19f,.09f),false);
        fastAnimation=PlayerPrefs.GetInt("FastDice",0)==1;
        miniatures=gameObject.AddComponent<TavernMiniatures>();miniatures.Initialize();
        diceStage=gameObject.AddComponent<TavernDice>();diceStage.Initialize(font);
    }
    void EnsureStyles()
    {
        if(title!=null)return;
        title=new GUIStyle(GUI.skin.label){font=font,fontSize=46,fontStyle=FontStyle.Bold,normal={textColor=ink},wordWrap=true};
        heading=new GUIStyle(title){fontSize=25};
        label=new GUIStyle(GUI.skin.label){font=font,fontSize=19,normal={textColor=ink},wordWrap=true};
        small=new GUIStyle(label){fontSize=16};
        centered=new GUIStyle(label){alignment=TextAnchor.MiddleCenter};
        cardTitle=new GUIStyle(heading){alignment=TextAnchor.MiddleCenter};
        lightTitle=new GUIStyle(heading){normal={textColor=new Color(.95f,.81f,.55f)},alignment=TextAnchor.MiddleCenter};
        button=new GUIStyle(GUI.skin.button){font=font,fontSize=20,fixedHeight=48,wordWrap=true,
            normal={background=redTexture,textColor=new Color(1,.9f,.68f)},hover={background=hoverTexture,textColor=Color.white},
            active={background=woodTexture,textColor=Color.white},padding=new RectOffset(12,12,6,6)};
        fieldStyle=new GUIStyle(GUI.skin.textField){font=font,fontSize=20,normal={textColor=ink,background=paperTexture},focused={textColor=ink,background=paperTexture},padding=new RectOffset(8,8,4,4)};
    }
    void Surface(Rect rect,Texture texture)
    {
        Fill(new Rect(rect.x+5,rect.y+7,rect.width,rect.height),new Color(0,0,0,.35f));
        Fill(rect,gold);GUI.DrawTexture(new Rect(rect.x+3,rect.y+3,rect.width-6,rect.height-6),texture,ScaleMode.StretchToFill);
        Fill(new Rect(rect.x+10,rect.y+10,rect.width-20,1),new Color(.42f,.24f,.1f,.5f));
        Fill(new Rect(rect.x+10,rect.yMax-11,rect.width-20,1),new Color(.42f,.24f,.1f,.5f));
    }
    void Fill(Rect rect,Color color){Color old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
    bool Btn(Rect rect,string text,bool enabled=true)
    {
        bool previous=GUI.enabled;GUI.enabled=enabled;
        bool clicked=GUI.Button(rect,text,button);GUI.enabled=previous;return clicked;
    }
    void Update()
    {
        drawerProgress=Mathf.MoveTowards(drawerProgress,panel==""?0:1,Time.unscaledDeltaTime*6);
    }
    void OnGUI()
    {
        EnsureStyles();
        float scale=Mathf.Min(Screen.width/CanvasWidth,Screen.height/CanvasHeight);
        GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-CanvasWidth*scale)/2,(Screen.height-CanvasHeight*scale)/2,0),Quaternion.identity,Vector3.one*scale);
        GUI.DrawTextureWithTexCoords(new Rect(0,0,1600,900),woodTexture,new Rect(0,0,5,3));
        for(int y=0;y<900;y+=150)Fill(new Rect(0,y,1600,3),new Color(.06f,.025f,.01f,.65f));
        if(page=="home") DrawHome();else if(page=="settings") DrawSettings();else if(page=="deployment") DrawDeployment();else DrawBattle();
    }
    void DrawHome()
    {
        Surface(new Rect(65,130,310,610),paperTexture);
        GUI.Label(new Rect(90,165,260,45),"冒险者档案",cardTitle);
        GUI.DrawTexture(new Rect(90,225,260,260),miniatures.Player);
        GUI.Label(new Rect(95,520,250,120),"拿起武器，掷出命运。\n在酒馆桌面上演练每一次交锋。",centered);
        Surface(new Rect(1225,180,310,530),paperTexture);
        GUI.Label(new Rect(1250,215,260,45),"训练木桩",cardTitle);
        GUI.DrawTexture(new Rect(1250,275,260,260),miniatures.Monster);
        GUI.Label(new Rect(1250,555,260,75),"属性 · 装备 · 多职业\n命中 · 伤害 · 暴击",centered);
        Surface(new Rect(450,100,700,685),paperTexture);
        GUI.Label(new Rect(500,130,600,35),"D N D  ·  T A V E R N  T A B L E",centered);
        GUI.Label(new Rect(505,195,590,80),"木桩训练场",new GUIStyle(title){alignment=TextAnchor.MiddleCenter});
        GUI.DrawTexture(new Rect(590,270,420,190),diceStage.Texture);
        if(Btn(new Rect(600,475,400,62),"进入战斗  →"))page=map.Locked?"battle":"deployment";
        if(Btn(new Rect(600,555,400,55),"设置"))page="settings";
        if(Btn(new Rect(600,630,400,55),"退出游戏"))QuitGame();
        GUI.Label(new Rect(560,715,480,40),"一张桌面，一场属于你的冒险。",centered);
    }
    void DrawSettings()
    {
        Surface(new Rect(390,100,820,680),paperTexture);
        GUILayout.BeginArea(new Rect(450,145,700,580));
        GUILayout.Label("酒馆设置",title);GUILayout.Space(25);
        GUILayout.Label("音乐音量  "+Mathf.RoundToInt(musicVolume*100)+"%",label);
        float m=Mathf.Round(GUILayout.HorizontalSlider(musicVolume,0,1)*100)/100f;GUILayout.Space(25);
        GUILayout.Label("音效音量  "+Mathf.RoundToInt(sfxVolume*100)+"%",label);
        float s=Mathf.Round(GUILayout.HorizontalSlider(sfxVolume,0,1)*100)/100f;GUILayout.Space(30);
        if(m!=musicVolume||s!=sfxVolume){musicVolume=m;sfxVolume=s;SaveVolume();}
        GUILayout.Label("投骰动画",heading);GUILayout.BeginHorizontal();
        if(GUILayout.Button((!fastAnimation?"✓ ":"")+"标准 · 约 1.4 秒 / 次",button))SetFast(false);
        if(GUILayout.Button((fastAnimation?"✓ ":"")+"快速 · 约 0.45 秒 / 次",button))SetFast(true);
        GUILayout.EndHorizontal();GUILayout.Space(30);
        if(GUILayout.Button("试听投骰音效",button))PlayRoll();GUILayout.Space(18);
        if(GUILayout.Button("返回酒馆桌面",button))page="home";
        GUILayout.EndArea();
    }
    void SetFast(bool value){fastAnimation=value;PlayerPrefs.SetInt("FastDice",value?1:0);PlayerPrefs.Save();}
    string ClassSummary()
    {
        int count=0;for(int i=0;i<PlayerClasses.Names.Length;i++)if(classes[i]>0)count++;
        return PlayerClasses.Names[classes.InitialClass]+" · 总等级 "+classes.TotalLevel+(count>1?" · 多职业":"");
    }
    void DrawCard(bool player)
    {
        float x=player?35:1265;Surface(new Rect(x,110,300,745),paperTexture);
        GUI.Label(new Rect(x+15,137,270,45),player?"冒险者":"训练木桩",cardTitle);
        GUI.DrawTexture(new Rect(x+25,190,250,250),player?miniatures.Player:miniatures.Monster);
        GUI.Label(new Rect(x+20,445,260,60),player?ClassSummary():"木制靶偶 · 回合结束反击",centered);
        int hp=player?combat.PlayerHp:combat.Hp,max=player?combat.PlayerMaxHp:combat.MaxHp;
        Fill(new Rect(x+23,520,254,20),new Color(.23f,.12f,.07f));
        Fill(new Rect(x+25,522,250*(hp/(float)max),16),new Color(.53f,.11f,.07f));
        GUI.Label(new Rect(x+20,547,260,30),"生命  "+hp+" / "+max,centered);
        GUI.Label(new Rect(x+20,585,260,30),"护甲  "+(player?combat.PlayerArmorClass:combat.MonsterArmorClass),centered);
        if(player){
            GUI.Label(new Rect(x+20,623,260,35),selectedWeapon.Name+" · "+selectedWeapon.DamageDice,centered);
            if(Btn(new Rect(x+25,674,250,46),combat.BattleStarted?"玩家属性（只读）":"玩家属性",panel==""))OpenPanel("attributes");
            if(Btn(new Rect(x+25,728,120,46),"玩家武器",panel==""))OpenPanel("weapons");
            if(Btn(new Rect(x+155,728,120,46),"玩家职业",panel==""))OpenPanel("classes");
            GUI.Label(new Rect(x+20,788,260,55),"每动作攻击 "+fighter.AttacksPerAction(classes[0])+" 次"+(classes[0]>=13?"\n究明攻击：失手后下次优势":""),centered);
        } else {
            GUI.Label(new Rect(x+20,625,260,30),"熟练加值  "+combat.MonsterProficiencyBonus,centered);
            if(Btn(new Rect(x+25,684,250,48),"怪物伤害更改",panel==""))OpenPanel("monster");
            if(Btn(new Rect(x+25,744,250,48),"怪物属性",panel==""))OpenPanel("monsterAttributes");
        }
    }
    void OpenPanel(string value){if(draggingToken>=0)return;panel=value;panelScroll=Vector2.zero;drawerProgress=0;}
    void DrawBattle()
    {
        GUI.Label(new Rect(540,25,520,45),"木桩训练场 · 第 "+combat.Round+" 回合",lightTitle);
        if(Btn(new Rect(35,25,250,48),"← 酒馆桌面",!rolling&&panel==""&&draggingToken<0))page="home";
        if(Btn(new Rect(1310,25,250,48),"重置战斗",!rolling&&panel==""&&draggingToken<0))ResetCombat();
        DrawCard(true);DrawCard(false);
        Fill(new Rect(365,105,870,445),new Color(.10f,.045f,.02f,.65f));
        DrawMap(new Rect(600,110,400,400),!rolling&&panel=="");
        GUI.Label(new Rect(1015,125,205,320),"移动 "+map.MovementRemaining+" / 30 尺\n"+(draggingToken==0?movementMessage:"距回合起点 "+BattleMap.FeetBetween(map.RoundStartPlayer,map.Player)+" 尺\n"+movementMessage),new GUIStyle(lightTitle){fontSize=18});
        GUI.Label(new Rect(380,120,205,130),BattleMap.Names[map.Selected]+" · "+map.Size+"×"+map.Size+"\n每格 5 尺\n双方距离 "+map.DistanceFeet+" 尺",lightTitle);
        if(rolling) {
            Fill(new Rect(365,105,870,405),new Color(.1f,.045f,.02f,.8f));
            GUI.DrawTexture(new Rect(480,155,640,330),diceStage.Texture,ScaleMode.ScaleToFit);
        }
        GUI.Label(new Rect(385,512,830,38),dice,new GUIStyle(centered){fontSize=16,normal={textColor=new Color(.98f,.86f,.63f)}});
        bool canAct=!rolling&&panel==""&&draggingToken<0;
        if(Btn(new Rect(395,555,260,48),classes[0]>=1?"回气（附赠动作） · 剩余 "+fighter.WindRemaining(classes[0])+" / 2":"回气 · 战士 1 级解锁",canAct&&fighter.WindRemaining(classes[0])>0&&combat.BonusActionAvailable))SecondWind();
        if(Btn(new Rect(670,555,260,48),classes[0]>=2?"动作如潮 · 剩余 "+fighter.SurgeRemaining(classes[0])+(fighter.SurgedThisRound?"（本回合已用）":""):"动作如潮 · 战士 2 级解锁",canAct&&fighter.CanSurge(classes[0])))ActionSurge();
        if(Btn(new Rect(945,555,260,48),selectedWeapon.HasLight?"轻型攻击（附赠动作）":"轻型攻击 · 需轻型武器",canAct&&selectedWeapon.HasLight&&combat.BonusActionAvailable))BonusAttack();
        string attackLabel=fighter.AttacksRemaining>0?"继续攻击 · 剩余 "+fighter.AttacksRemaining+" 次":combat.ActionAvailable?"攻击（动作） · "+selectedWeapon.Name:"动作已使用";
        if(Btn(new Rect(430,615,360,57),attackLabel,canAct&&(combat.ActionAvailable||fighter.AttacksRemaining>0)))Attack();
        if(Btn(new Rect(810,615,360,57),"结束回合 / 跳过剩余操作",canAct))EndCombatRound();
        GUI.Label(new Rect(415,674,790,25),"动作 "+combat.ActionsRemaining+" / "+combat.ActionsThisRound+" · 附赠动作 "+(combat.BonusActionAvailable?1:0)+" / 1"+(fighter.NextAttackAdvantage?" · 下次攻击具有优势":"")+(combat.BattleStarted?" · 配置已锁定":" · 可配置"),new GUIStyle(centered){fontSize=16,normal={textColor=new Color(.98f,.86f,.63f)}});
        Surface(new Rect(365,705,870,150),paperTexture);
        GUI.Label(new Rect(390,713,810,30),"战斗卷轴",heading);
        GUILayout.BeginArea(new Rect(390,752,815,87));
        logScroll=GUILayout.BeginScrollView(logScroll);
        if(history.Count==0)GUILayout.Label("卷轴尚未记下战斗。选择武器，开始第一次投掷。",label);
        for(int i=0;i<history.Count;i++){GUILayout.Label(history[i],small);GUILayout.Space(10);}
        GUILayout.EndScrollView();GUILayout.EndArea();
        if(panel!="")DrawPanel();
    }
    void DisposePresentation()
    {
        if(woodTexture)Destroy(woodTexture);if(paperTexture)Destroy(paperTexture);if(redTexture)Destroy(redTexture);if(hoverTexture)Destroy(hoverTexture);
        if(diceStage)Destroy(diceStage);if(miniatures)Destroy(miniatures);
    }
}
