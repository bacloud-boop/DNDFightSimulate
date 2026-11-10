using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    bool spellTargeting,spellCenterChosen;
    int castingClass=1;
    Vector2Int spellCenter;
    bool CanCastAcid => classes[1]>=1 || classes[3]>=1;
    void DrawSpellPanel()
    {
        const float x=350;
        Surface(new Rect(x,110,480,745),paperTexture);
        if(Btn(new Rect(x+370,125,90,46),"关闭")){panel="";return;}
        GUI.Label(new Rect(x+20,180,440,40),"法术 · 戏法",heading);
        if(!CanCastAcid){GUI.Label(new Rect(x+20,235,440,90),"法师或术士至少 1 级可使用酸液飞溅。",label);return;}
        if(classes[castingClass]<1)castingClass=classes[1]>=1?1:3;
        if(Btn(new Rect(x+20,235,205,48),(castingClass==1?"✓ ":"")+"法师 · 智力",classes[1]>=1))castingClass=1;
        if(Btn(new Rect(x+235,235,205,48),(castingClass==3?"✓ ":"")+"术士 · 魅力",classes[3]>=1))castingClass=3;
        int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[castingClass==1?4:5]);
        GUI.Label(new Rect(x+20,305,440,70),"敏捷豁免 DC "+dc+" · "+AcidSplash.DiceCount(classes.TotalLevel)+"d6 强酸\n射程 60 尺 · 5 尺半径球状",label);
        bool previous=GUI.enabled;GUI.enabled=page=="battle"&&map.Locked&&!rolling&&combat.ActionAvailable;
        if(GUI.Button(new Rect(x+20,400,440,65),new GUIContent("酸液飞溅 · 选择施法位置",AcidSplash.Summary),button)){
            panel="";spellTargeting=true;spellCenterChosen=false;
        }
        GUI.enabled=previous;
        GUI.Label(new Rect(x+20,490,440,90),"悬停查看简概。点击后在地图选择格心，预览目标，再确认施法。",label);
    }
    void DrawSpellTargeting(Rect board)
    {
        if(!spellTargeting)return;
        Vector2 pointer=BoardPointer();var e=Event.current;
        bool hover=board.Contains(pointer);
        var cell=hover?new Vector2Int(Mathf.FloorToInt((pointer.x-board.x)/(board.width/map.Size)),Mathf.FloorToInt((pointer.y-board.y)/(board.width/map.Size))):spellCenter;
        bool inRange=BattleMap.FeetBetween(map.Player,cell)<=60;
        if(hover && e.type==EventType.MouseDown && e.button==0){if(inRange){spellCenter=cell;spellCenterChosen=true;}e.Use();}
        Vector2Int preview=hover?cell:spellCenter;
        if(hover || spellCenterChosen){
            for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(BattleMap.FeetBetween(preview,new Vector2Int(x,y))<=5)
                Fill(CellRect(board,new Vector2Int(x,y)),new Color(.28f,.8f,.12f,.35f));
            Fill(CellRect(board,preview),inRange?new Color(.1f,.8f,.2f,.35f):new Color(.9f,.1f,.05f,.4f));
        }
        bool hits=spellCenterChosen&&BattleMap.FeetBetween(spellCenter,map.Monster)<=5;
        Surface(new Rect(1010,110,225,410),paperTexture);
        GUI.Label(new Rect(1025,125,195,42),"酸液飞溅",heading);
        string target=spellCenterChosen?(hits?"木桩":"无目标"):"请点击地图选择中心";
        GUI.Label(new Rect(1025,180,195,160),"射程 60 尺\n半径 5 尺\n选定目标："+target+(hover?"\n悬停范围内："+(BattleMap.FeetBetween(preview,map.Monster)<=5?"木桩":"无目标"):"")+"\n玩家自身不受影响",label);
        if(Btn(new Rect(1025,355,195,48),"确认施法",spellCenterChosen&&combat.ActionAvailable&&!rolling))CastAcidSplash();
        if(Btn(new Rect(1025,420,195,48),"取消施法")){spellTargeting=false;spellCenterChosen=false;}
    }
    void CastAcidSplash()
    {
        if(page!="battle" || !map.Locked || !spellTargeting || !spellCenterChosen || !CanCastAcid || rolling || BattleMap.FeetBetween(map.Player,spellCenter)>60 || !combat.TryUseAction())return;
        bool hits=BattleMap.FeetBetween(spellCenter,map.Monster)<=5;
        int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[castingClass==1?4:5]);
        int count=AcidSplash.DiceCount(classes.TotalLevel);
        spellTargeting=false;spellCenterChosen=false;
        StartCoroutine(ResolveAcidSplash(hits,dc,count));
    }
    IEnumerator ResolveAcidSplash(bool hits,int dc,int count)
    {
        rolling=true;logScroll=Vector2.zero;
        string caster=PlayerClasses.Names[castingClass];
        if(!hits){dice="酸液飞溅 · 范围内无目标";history.Insert(0,caster+"施放酸液飞溅，范围内无目标，消耗 1 动作。");rolling=false;yield break;}
        int save=Random.Range(1,21),modifier=AcidSplash.Modifier(monsterAttributes[1]);
        dice="酸液飞溅 · 木桩敏捷豁免 D20";
        yield return PresentRoll(20,new[]{save},"豁免："+save+" + 敏捷 "+modifier+" = "+(save+modifier)+" / DC "+dc+(save+modifier>=dc?"，成功":"，失败"));
        string details=caster+"酸液飞溅：木桩敏捷豁免 "+save+" + "+modifier+" = "+(save+modifier)+" / DC "+dc;
        if(save+modifier>=dc){dice="敏捷豁免成功 · 强酸伤害 0";history.Insert(0,details+"，成功，不受伤害。");rolling=false;yield break;}
        int[] values=new int[count];for(int i=0;i<count;i++)values[i]=Random.Range(1,7);
        dice="敏捷豁免失败 · 酸液飞溅 "+count+"d6 强酸";
        yield return PresentRoll(6,values,"强酸伤害："+System.Linq.Enumerable.Sum(values));
        int damage=System.Linq.Enumerable.Sum(values);combat.ApplyDamage(damage);
        dice="酸液飞溅 · 强酸伤害 "+damage;
        history.Insert(0,details+"，失败；"+count+"d6="+string.Join("+",values)+"，强酸伤害 "+damage+"。");
        if(combat.ResetIfDead()){history.Insert(0,"木桩死亡，双方回满，重新部署。");fighter.ResetBattle();BeginDeployment();}
        rolling=false;
    }
    void DrawSpellTooltip()
    {
        if(string.IsNullOrEmpty(GUI.tooltip))return;
        Surface(new Rect(845,180,400,450),paperTexture);
        GUI.Label(new Rect(865,200,360,410),GUI.tooltip,new GUIStyle(label){fontSize=18});
    }
}
