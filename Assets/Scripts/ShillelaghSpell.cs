using UnityEngine;
public sealed partial class TrainingGame
{
    readonly ShillelaghEffect shillelagh=new ShillelaghEffect();
    bool shillelaghUseCasting=true,shillelaghForce;
    string PlayerWeaponDamageDice=>shillelagh.Applies(selectedWeapon.Name,worldRounds)?ShillelaghEffect.Count(classes.TotalLevel)+"d"+ShillelaghEffect.Sides(classes.TotalLevel):selectedWeapon.DamageDice;
    void RefreshShillelagh(){if(shillelagh.Refresh(selectedWeapon.Name,PlayerInBeastForm||weaponGrip.Stowed,worldRounds))history.Insert(0,"橡棍术结束：持续时间届满，或收起／切换武器／装备融入形态而放手。");}
    string ShillelaghError()=>PlayerInBeastForm||weaponGrip.Stowed?"须先持握短棒或长棍。":!ShillelaghEffect.Eligible(selectedWeapon.Name)?"请先选择短棒或长棍。":worldRounds>long.MaxValue-10?"模拟时间超出范围。":"";
    void DrawShillelaghOptions()
    {
        GUILayout.Label("橡棍术：短棒／长棍，持续1分钟。切换武器或装备融入形态立即结束。",small);
        shillelaghUseCasting=GUILayout.Toggle(shillelaghUseCasting,"使用施法属性代替力量（否则使用力量）",label);
        shillelaghForce=GUILayout.Toggle(shillelaghForce,"武器基础伤害改为力场（否则保留原类型）",label);
        GUILayout.Label("可在攻击之间更改这些选择；与克敌机先同时使用且选择光耀时，基础伤害按光耀结算。",small);
    }
    void ApplyShillelagh(int caster)
    {
        if(!shillelagh.Begin(selectedWeapon.Name,SpellAbilityIndex(caster),worldRounds))return;
        history.Insert(0,"橡棍术："+selectedWeapon.Name+"基础伤害变为 "+PlayerWeaponDamageDice+"，可使用"+PlayerAttributes.Names[shillelagh.Ability]+"攻击与伤害，持续1分钟，无需专注；不立即发动攻击。");
        dice="橡棍术 · "+selectedWeapon.Name+" · "+PlayerWeaponDamageDice;
    }
}
