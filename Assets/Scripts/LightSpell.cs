using UnityEngine;
public sealed partial class TrainingGame
{
    const int ObjectLightGroup=3;
    readonly LightSpellEffect objectLight=new LightSpellEffect();
    int lightObjectId,selectedEquipmentObject;
    string lightRed="255",lightGreen="230",lightBlue="170",equipmentReturnPanel="";
    Vector2 lightScroll,equipmentScroll;
    bool LightColors(out int red,out int green,out int blue){red=green=blue=0;return int.TryParse(lightRed,out red)&&int.TryParse(lightGreen,out green)&&int.TryParse(lightBlue,out blue)&&red>=0&&red<=255&&green>=0&&green<=255&&blue>=0&&blue<=255;}
    string LightSpellError()
    {
        SyncEquipmentObjects();var item=equipmentObjects.Find(lightObjectId);EquipmentPose pose;
        if(!EquipmentObjectPose(item,out pose))return "请选择场内存在的武器、护甲或盾牌。";
        if(EffectiveObjectSize(item,pose)>3)return "光亮术只能作用于大型或更小的物件。";
        if(pose.Carrier.HasValue&&pose.Carrier.Value!=0)return "2024光亮术不能选择其他角色穿戴或携带的物件。";
        if(!EquipmentInTouch(pose))return "目标须在可接触的5尺内；融入的装备不可触碰，水下装备须先潜水。";
        int r,g,b;if(!LightColors(out r,out g,out b))return "RGB颜色各项须为0～255整数。";
        return worldRounds<0||worldRounds>long.MaxValue-600?"世界时间超出持续时间的可用范围。":"";
    }
    void EndLightSpell(string reason)
    {
        bool active=objectLight.ObjectId>0;objectLight.Clear();vision.ClearLightGroup(ObjectLightGroup);if(active&&reason!="")history.Insert(0,"光亮术结束："+reason+"。");
    }
    void ApplyLightSpell(int target,int red,int green,int blue)
    {
        SyncEquipmentObjects();var chosen=equipmentObjects.Find(target);EquipmentPose current;
        if(!EquipmentObjectPose(chosen,out current)||!LightSpellEffect.CanTouch(EffectiveObjectSize(chosen,current),EquipmentInTouch(current),current.Carrier)){history.Insert(0,"光亮术完成时目标已无法接触或不再合法，施法动作已消耗。");return;}
        if(!objectLight.Cast(target,red,green,blue,worldRounds))return;
        RefreshLightSpell();var item=equipmentObjects.Find(target);dice=objectLight.Active(worldRounds)?"光亮术 · 已附着装备":"光亮术 · 已被驱散";
        history.Insert(0,"光亮术附着于"+(item==null?"装备":item.Name)+"，20尺明亮、外围再20尺微光，非专注1小时。遮盖／融入时暂时阻光，揭开／复原后在剩余时间内恢复。");
    }
    void RefreshLightSpell()
    {
        SyncContainedWeapons();SyncEquipmentObjects();RefreshContinualFlames();
        if(!objectLight.Active(worldRounds)){if(objectLight.ObjectId>0)EndLightSpell("持续时间结束");else vision.ClearLightGroup(ObjectLightGroup);return;}
        if(vision.ConsumeDispelledLightGroup(ObjectLightGroup)){EndLightSpell("与黑暗术重叠，被驱散");return;}
        var item=equipmentObjects.Find(objectLight.ObjectId);EquipmentPose pose;
        if(!EquipmentObjectPose(item,out pose)){EndLightSpell("附着的装备已离场");return;}
        if(item.Covered||pose.Melded||pose.Hidden){vision.HideLightGroup(ObjectLightGroup);return;}
        if(!vision.SetLightGroup(ObjectLightGroup,new[]{new[]{pose.X,pose.Y}},20,40,0,objectLight.Expires,worldRounds))EndLightSpell("与黑暗术重叠，被驱散");
    }
    void DrawObjectLightMap(Rect board)
    {
        if(!objectLight.Active(worldRounds))return;var item=equipmentObjects.Find(objectLight.ObjectId);EquipmentPose pose;
        if(!EquipmentObjectPose(item,out pose)||item.Covered||pose.Melded||pose.Hidden)return;
        float step=board.width/map.Size;var point=new Rect(board.x+((float)pose.X/5+.5f)*step-7,board.y+((float)pose.Y/5+.5f)*step-7,14,14);
        Fill(point,new Color(objectLight.Red/255f,objectLight.Green/255f,objectLight.Blue/255f,.95f));
    }
    void OpenEquipmentObjects(string back){SyncEquipmentObjects();equipmentReturnPanel=back;panel="equipmentObjects";}
}
