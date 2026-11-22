using UnityEngine;
public sealed partial class TrainingGame
{
    readonly ContinualFlameEffect continualFlame=new ContinualFlameEffect();
    int continualObject;
    string ContinualFlameError(int target)
    {
        SyncEquipmentObjects();var item=equipmentObjects.Find(target);EquipmentPose pose;
        return EquipmentObjectPose(item,out pose)&&EquipmentInTouch(pose)?"":"请选择5尺内可接触的武器、护甲或盾牌；体内与体外隔离，融入的装备不可接触。";
    }
    void DrawContinualFlameChoices()
    {
        SyncEquipmentObjects();
        foreach(var item in equipmentObjects.Objects){EquipmentPose pose;if(!EquipmentObjectPose(item,out pose))continue;
            if(GUILayout.Button((continualObject==item.Id?"✓ ":"")+item.Name+" · "+pose.Location+(item.Covered?" · 遮盖":""),button))continualObject=item.Id;
        }
        GUILayout.Label("20尺明亮、外围20尺微光，非专注，直到驱散。无热、不耗燃料，遮盖会阻光，不能用水或闷盖熄灭。不同物件可同时附着；同物件以最高环阶、相同环阶最新来源生效。二环会被重叠的黑暗术驱散，三环及以上只照亮其中，不驱散黑暗术。",small);
        if(GUILayout.Button("物件遮盖／揭开",button))OpenEquipmentObjects("spells");
    }
    void CastContinualFlame(int target,int slot)
    {
        if(ContinualFlameError(target)!=""){history.Insert(0,"不灭明焰：目标已无法触及，施法动作已消耗。");return;}
        ContinualFlameEffect.Source source;if(!continualFlame.Cast(target,slot,out source))return;
        RefreshLightSpell();bool retained=false;foreach(var existing in continualFlame.All)if(existing.Group==source.Group)retained=true;
        if(!retained){dice="不灭明焰 · 被黑暗术驱散";history.Insert(0,"不灭明焰施放完成，但与黑暗术重叠后立即被驱散。");return;}
        dice="不灭明焰 · 已附着";history.Insert(0,"不灭明焰附着于"+equipmentObjects.Find(target).Name+"，"+slot+"环、直到驱散；不会引燃蛛网、造成火焰伤害或改变呼吸状态。");
    }
    void EndContinualFlames(){foreach(var source in continualFlame.All)vision.ClearLightGroup(-source.Group);continualFlame.Clear();}
    void RefreshContinualFlames()
    {
        foreach(var source in continualFlame.All)if(vision.ConsumeDispelledLightGroup(-source.Group)){continualFlame.Remove(source.Group);vision.ClearLightGroup(-source.Group);history.Insert(0,"不灭明焰被黑暗术驱散。");}
        foreach(var source in continualFlame.All)if(continualFlame.Effective(source.ObjectId)!=source)vision.HideLightGroup(-source.Group);
        foreach(int target in continualFlame.Objects){
            ContinualFlameEffect.Source source;
            while((source=continualFlame.Effective(target))!=null){
                var item=equipmentObjects.Find(target);EquipmentPose pose;
                if(!EquipmentObjectPose(item,out pose)||item.Covered||pose.Melded||pose.Hidden){vision.HideLightGroup(-source.Group);break;}
                if(vision.SetLightGroup(-source.Group,new[]{new[]{pose.X,pose.Y}},20,40,source.Slot,-1,worldRounds))break;
                continualFlame.Remove(source.Group);vision.ClearLightGroup(-source.Group);history.Insert(0,"不灭明焰与黑暗术重叠，被驱散。");
            }
        }
    }
    void DrawContinualFlames(Rect board)
    {
        float step=board.width/map.Size;
        foreach(int target in continualFlame.Objects){var item=equipmentObjects.Find(target);EquipmentPose p;
            if(!EquipmentObjectPose(item,out p)||item.Covered||p.Melded||p.Hidden)continue;
            Fill(new Rect(board.x+((float)p.X/5+.5f)*step-5,board.y+((float)p.Y/5+.5f)*step-5,10,10),new Color(1,.62f,.12f));
        }
    }
}
