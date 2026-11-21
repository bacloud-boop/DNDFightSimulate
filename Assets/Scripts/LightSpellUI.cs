using System;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    void DrawLightSpellPanel()
    {
        SyncEquipmentObjects();var candidates=equipmentObjects.Objects.Where(item=>{EquipmentPose p;return EquipmentObjectPose(item,out p);}).ToArray();
        if(!candidates.Any(item=>item.Id==lightObjectId))lightObjectId=candidates.Length==0?0:candidates[0].Id;
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"返回",!rolling)){selectedSpellName="";return;}
        var spell=DirectSpell.Find("Light");var metadata=SelectedSpellMetadata();var casters=metadata==null?new int[0]:EligibleCasters(metadata).ToArray();
        if(Array.IndexOf(casters,directCastingClass)<0&&casters.Length>0)directCastingClass=casters[0];
        GUILayout.BeginArea(new Rect(375,185,390,630));lightScroll=GUILayout.BeginScrollView(lightScroll);
        GUILayout.Label("光亮术",heading);GUILayout.Label(spell.Summary,small);
        bool enabled=GUI.enabled;GUI.enabled=enabled&&!rolling;
        foreach(int caster in casters)if(GUILayout.Button((caster==directCastingClass?"✓ ":"")+CasterLabel(caster),button))directCastingClass=caster;
        if(map.PlayerAquatic.Submerged&&!WaterBreathingAllowed(true))GUILayout.Label("此法术有声音成分，水下施放会结束屏息并呛水。",small);
        foreach(var item in candidates){EquipmentPose pose;EquipmentObjectPose(item,out pose);
            if(GUILayout.Button((lightObjectId==item.Id?"✓ ":"")+item.Name+" · "+CreatureProfile.SizeNames[EffectiveObjectSize(item,pose)]+"\n"+pose.Location+(item.Covered?" · 已遮盖":pose.Melded?" · 已融入":""),new GUIStyle(button){fontSize=15,fixedHeight=62}))lightObjectId=item.Id;
        }
        GUILayout.Label("光色RGB（各0～255整数）",small);GUILayout.BeginHorizontal();lightRed=IntegerField(lightRed,false);lightGreen=IntegerField(lightGreen,false);lightBlue=IntegerField(lightBlue,false);GUILayout.EndHorizontal();
        string error=DirectSpellInvalidReason(spell,casters.Length>0);GUILayout.Label(error==""?"接触合法；消耗1个魔法动作，无需专注。":error,small);
        GUI.enabled=enabled&&error=="";if(GUILayout.Button("确认施放光亮术",button)){panel="";StartCoroutine(ResolveDirectSpell(spell,true,directCastingClass,0));}
        GUI.enabled=enabled&&!rolling;if(GUILayout.Button("物件体型与遮盖／揭开",button))OpenEquipmentObjects("spells");GUI.enabled=enabled;
        if(objectLight.Active(worldRounds))GUILayout.Label("当前光亮术最多剩余 "+Math.Max(0,objectLight.Expires-worldRounds)*6+" 秒；再次施放会结束原效果。",small);
        GUILayout.EndScrollView();GUILayout.EndArea();
        Rect board=new Rect(800,250,400,400);DrawMap(board,false);var selected=equipmentObjects.Find(lightObjectId);EquipmentPose location;
        if(EquipmentObjectPose(selected,out location)&&!selected.Covered&&!location.Melded&&!location.Hidden){int r,g,b;bool colors=LightColors(out r,out g,out b);var tint=colors?new Color(r/255f,g/255f,b/255f,.2f):new Color(1,.8f,.2f,.2f);
            for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(VisionState.Distance(location.X,location.Y,x*5,y*5)<=40)Fill(CellRect(board,new Vector2Int(x,y)),tint);
        }
        GUI.Label(new Rect(800,670,400,100),"预览：20尺明亮，外围再20尺微光。光源随装备移动；被遮盖、融入或置于吞咽内部时暂时阻光。",small);
    }
    void DrawEquipmentObjectsPanel()
    {
        SyncEquipmentObjects();var items=equipmentObjects.Objects.Where(item=>{EquipmentPose p;return EquipmentObjectPose(item,out p)||!combat.BattleStarted&&item.Kind==EquipmentObjectKind.Weapon&&item.Owner==0;}).ToArray();
        if(!items.Any(item=>item.Id==selectedEquipmentObject))selectedEquipmentObject=items.Length==0?0:items[0].Id;
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"关闭",!rolling)){panel=equipmentReturnPanel;return;}
        GUILayout.BeginArea(new Rect(375,185,390,630));equipmentScroll=GUILayout.BeginScrollView(equipmentScroll);
        GUILayout.Label("装备物件",heading);GUILayout.Label("物件基础体型默认跟随原身体体型，可在首次操作前调整。战斗中遮盖／揭开使用物件交互：每回合一次自由交互，额外操作消耗动作。",small);
        foreach(var item in items)if(GUILayout.Button((selectedEquipmentObject==item.Id?"✓ ":"")+item.Name+" · 来源"+ActorName(item.Owner),button))selectedEquipmentObject=item.Id;
        GUILayout.EndScrollView();GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(800,195,400,610));var selected=equipmentObjects.Find(selectedEquipmentObject);
        if(selected!=null){
            EquipmentPose pose;bool onMap=EquipmentObjectPose(selected,out pose);GUILayout.Label(selected.Name,heading);GUILayout.Label(onMap?pose.Location:"场外存放",small);
            bool enabled=GUI.enabled;GUI.enabled=enabled&&!combat.BattleStarted&&!rolling;
            int size=GUILayout.SelectionGrid(selected.Size,CreatureProfile.SizeNames,3,new GUIStyle(button){fontSize=16,fixedHeight=36});if(size!=selected.Size)equipmentObjects.Configure(selected.Id,size,selected.Covered);
            bool metal=GUILayout.Toggle(selected.Metal,"金属物件（灼热金属可选目标）",label);if(metal!=selected.Metal)equipmentObjects.Configure(selected.Id,selected.Size,selected.Covered,metal);GUI.enabled=enabled;
            if(onMap)GUILayout.Label("当前体型："+CreatureProfile.SizeNames[EffectiveObjectSize(selected,pose)]+(pose.Melded?" · 装备已融入":"")+"\n遮盖："+(selected.Covered?"完全遮盖，阻光":"未遮盖")+"\n"+(objectLight.ObjectId==selected.Id&&objectLight.Active(worldRounds)?"附着光亮术，最多剩余 "+Math.Max(0,objectLight.Expires-worldRounds)*6+" 秒":""),label);
            string error=!combat.BattleStarted?"":EquipmentHandleError(selected);GUI.enabled=enabled&&!rolling&&error=="";
            if(GUILayout.Button(selected.Covered?"揭开物件":"完全遮盖物件（不透明遮盖）",button))ToggleEquipmentCover(selected);GUI.enabled=enabled;
            if(error!="")GUILayout.Label(error,small);
            GUILayout.Label("体型与金属标记仅能在首次操作前更改；盾牌默认金属，武器与护甲按常见材质预填。重量仍由装备资料结算。回收进背包的护甲／盾牌先按遮盖状态存放，可再揭开；不会自动穿戴。",small);
        }
        GUILayout.EndArea();
    }
}
