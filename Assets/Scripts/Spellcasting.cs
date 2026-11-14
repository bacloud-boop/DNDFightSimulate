using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    bool spellTargeting,spellCenterChosen;
    int castingClass=1;
    Vector2Int spellCenter;
    bool CanCastAcid => classes[1]>=1 || classes[3]>=1;
    void DrawAcidSpellPanel()
    {
        const float x=350;
        Surface(new Rect(x,110,480,745),paperTexture);
        if(Btn(new Rect(x+370,125,90,46),"返回")){selectedSpellName="";return;}
        GUI.Label(new Rect(x+20,180,440,40),"法术 · 戏法",heading);
        if(classes[1]>=1||classes[2]>=1) {
            GUI.Label(new Rect(x+20,565,440,40),"一环功能性法术 · 法术位暂不扣除",label);
            bool enabled=GUI.enabled;GUI.enabled=enabled&&!combat.BattleStarted;
            if(GUI.Button(new Rect(x+20,615,440,55),new GUIContent(alarmLearned?"警报术 · 已学习（不可战斗施放）":"学习警报术（游侠 / 法师）",AlarmSpell.Summary),button))alarmLearned=true;
            GUI.enabled=enabled;
        }
        if(!CanCastAcid){GUI.Label(new Rect(x+20,235,440,90),"法师或术士至少 1 级可使用酸液飞溅。",label);return;}
        if(classes[castingClass]<1)castingClass=classes[1]>=1?1:3;
        if(Btn(new Rect(x+20,235,205,48),(castingClass==1?"✓ ":"")+"法师 · 智力",classes[1]>=1))castingClass=1;
        if(Btn(new Rect(x+235,235,205,48),(castingClass==3?"✓ ":"")+"术士 · 魅力",classes[3]>=1))castingClass=3;
        int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[castingClass==1?4:5]);
        GUI.Label(new Rect(x+20,305,440,70),"敏捷豁免 DC "+dc+" · "+AcidSplash.DiceCount(classes.TotalLevel)+"d6 强酸\n射程 60 尺 · 5 尺半径球状",label);
        bool previous=GUI.enabled;GUI.enabled=page=="battle"&&map.Locked&&!rolling&&combat.MagicActionAvailable&&CanCastInCurrentForm;
        if(GUI.Button(new Rect(x+20,400,440,65),new GUIContent("酸液飞溅 · 选择施法位置",AcidSplash.Summary),button)){
            panel="";thunderwaveTargeting=false;spellTargeting=true;spellCenterChosen=false;
        }
        GUI.enabled=previous;
        GUI.Label(new Rect(x+20,490,440,90),"悬停查看简概。点击后在地图选择格心，预览目标，再确认施法。",label);
    }
    void DrawSpellTargeting(Rect board)
    {
        if(!spellTargeting)return;
        if(thunderwaveTargeting){DrawThunderwaveTargeting(board);return;}
        Vector2 pointer=BoardPointer();var e=Event.current;
        bool hover=board.Contains(pointer);
        var cell=hover?new Vector2Int(Mathf.FloorToInt((pointer.x-board.x)/(board.width/map.Size)),Mathf.FloorToInt((pointer.y-board.y)/(board.width/map.Size))):spellCenter;
        bool inRange=map.DistanceToCell(true,cell)<=60;
        if(hover && e.type==EventType.MouseDown && e.button==0){if(inRange){spellCenter=cell;spellCenterChosen=true;}e.Use();}
        Vector2Int preview=hover?cell:spellCenter;
        if(hover || spellCenterChosen){
            for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(BattleMap.FeetBetween(preview,new Vector2Int(x,y))<=5)
                Fill(CellRect(board,new Vector2Int(x,y)),new Color(.28f,.8f,.12f,.35f));
            Fill(CellRect(board,preview),inRange?new Color(.1f,.8f,.2f,.35f):new Color(.9f,.1f,.05f,.4f));
        }
        bool hits=spellCenterChosen&&combat.Hp>0&&map.DistanceToCell(false,spellCenter)<=5;
        Surface(new Rect(1010,110,225,410),paperTexture);
        GUI.Label(new Rect(1025,125,195,42),"酸液飞溅",heading);
        string target=spellCenterChosen?AcidTargets(spellCenter):"请点击地图选择中心";
        GUI.Label(new Rect(1025,180,195,160),"射程 60 尺\n半径 5 尺\n选定目标："+target+(hover?"\n悬停范围内："+AcidTargets(preview):"")+"\n范围内玩家也需豁免",label);
        if(Btn(new Rect(1025,355,195,48),"确认施法",spellCenterChosen&&combat.MagicActionAvailable&&!rolling&&CanCastInCurrentForm))CastAcidSplash();
        if(Btn(new Rect(1025,420,195,48),"取消施法")){spellTargeting=false;spellCenterChosen=false;}
    }
    void CastAcidSplash()
    {
        if(page!="battle" || !map.Locked || !spellTargeting || !spellCenterChosen || !CanCastAcid || !CanCastInCurrentForm || rolling || map.DistanceToCell(true,spellCenter)>60 || !combat.TryUseMagicAction())return;
        bool hits=combat.Hp>0&&map.DistanceToCell(false,spellCenter)<=5;
        bool hitsPlayer=!combat.PlayerDead&&map.DistanceToCell(true,spellCenter)<=5;
        int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[castingClass==1?4:5]);
        int count=AcidSplash.DiceCount(classes.TotalLevel);
        spellTargeting=false;spellCenterChosen=false;
        StartCoroutine(ResolveAcidSplash(hits,hitsPlayer,dc,count));
    }
    string AcidTargets(Vector2Int center)
    {
        bool monster=combat.Hp>0&&map.DistanceToCell(false,center)<=5;
        bool player=!combat.PlayerDead&&map.DistanceToCell(true,center)<=5;
        return monster&&player?"木桩、玩家":monster?"木桩":player?"玩家":"无目标";
    }
    bool DexteritySaveProficient => classes.SavingThrowProficient(1);
    IEnumerator ResolveAcidSplash(bool hitsMonster,bool hitsPlayer,int dc,int count){long token=senses.Token(true,worldRounds);yield return ResolveAcidSplashCore(hitsMonster,hitsPlayer,dc,count);BreakOrdinaryInvisibility(true,"施法",token);}
    IEnumerator ResolveAcidSplashCore(bool hitsMonster,bool hitsPlayer,int dc,int count)
    {
        rolling=true;logScroll=Vector2.zero;
        bool[] hit={hitsMonster,hitsPlayer},failed=new bool[2];
        if(!hitsMonster&&!hitsPlayer){dice="酸液飞溅：无目标，已消耗动作。";history.Insert(0,dice);rolling=false;yield break;}
        for(int i=0;i<2;i++)if(hit[i]){
            string target=i==0?"木桩":"玩家";
            int value=Random.Range(1,21),modifier=AcidSplash.Modifier(i==0?monsterAttributes[1]:attributes[1]);
            if(i==1)modifier=PlayerSavingThrowModifier(1);
            bool automaticFailure=i==0?combat.MonsterParalyzed:combat.PlayerHp==0||combat.PlayerParalyzed;
            var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(i==1,automaticFailure,adjustment);modifier+=adjustment.Value;
            if(!automaticFailure){
                dice="酸液飞溅 · "+target+"敏捷豁免";
                yield return PresentRoll(20,new[]{value},"豁免："+value+" + "+modifier+" / DC "+dc);
            }
            failed[i]=automaticFailure||value+modifier<dc;
            history.Insert(0,"酸液飞溅："+target+(automaticFailure?"昏迷，敏捷豁免自动失败":("豁免 "+value+" + "+modifier+" / DC "+dc))+(failed[i]?"，失败。":"，成功，伤害0。"));
        }
        if(failed[0]||failed[1]){
            int[] values=new int[count];for(int i=0;i<count;i++)values[i]=Random.Range(1,7);
            int damage=System.Linq.Enumerable.Sum(values);dice="酸液飞溅 · 强酸伤害";
            yield return PresentRoll(6,values,"强酸伤害："+damage+"；豁免失败者使用同一伤害骰结果");
            int monsterDamage=failed[0]?combat.AdjustTypedDamage(false,damage,3):0,playerDamage=failed[1]?combat.AdjustTypedDamage(true,damage,3):0;
            if(failed[0])combat.ApplyDamage(monsterDamage);
            if(failed[1]){combat.ApplyPlayerDamage(playerDamage,sourcePlayer:true);yield return MaintainConcentration(playerDamage);}
            dice="酸液飞溅："+damage+" 点强酸伤害";
            history.Insert(0,count+"d6="+string.Join("+",values)+"，木桩伤害 "+monsterDamage+"，玩家伤害 "+playerDamage+"。");
        }else dice="酸液飞溅：目标全部豁免成功，伤害0。";
        CheckBattleOutcome();rolling=false;
    }
    void DrawSpellTooltip()
    {
        if(string.IsNullOrEmpty(GUI.tooltip))return;
        var tooltipStyle=new GUIStyle(label){fontSize=18};
        float height=Mathf.Min(740,tooltipStyle.CalcHeight(new GUIContent(GUI.tooltip),460)+40);
        Surface(new Rect(845,80,500,height),paperTexture);
        GUI.Label(new Rect(865,100,460,height-40),GUI.tooltip,tooltipStyle);
    }
}
