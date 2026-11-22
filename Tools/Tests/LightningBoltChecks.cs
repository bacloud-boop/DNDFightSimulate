using System;
using System.Collections.Generic;
class LightningBoltChecks
{
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static void Main()
    {
        BreathTemplate line;
        C(BreathTemplate.TryCreate(0,0,1,0,1,30,.5,true,out line,false,100)&&line.Line&&line.LengthFeet==100,"100-foot line can be created from the selected source edge");
        C(line.Cell(1,0)&&line.Cell(20,0)&&!line.Cell(21,0)&&!line.Cell(1,1)&&!line.Cell(1,-1),"line remains five feet wide and stops at the hundred-foot end");
        C(!line.Cell(0,0)&&!line.Cell(-1,0),"outward edge source does not retroactively hit its caster or cells behind it");
        BreathTemplate oldLine,cone;
        C(BreathTemplate.TryCreate(0,0,1,0,1,30,.5,true,out oldLine)&&oldLine.LengthFeet==30&&oldLine.Cell(6,0)&&!oldLine.Cell(7,0),"default racial line remains thirty feet");
        C(BreathTemplate.TryCreate(0,0,1,0,1,30,.5,false,out cone)&&cone.LengthFeet==15&&cone.Cell(3,0)&&!cone.Cell(4,0),"default cone remains fifteen feet");
        C(!BreathTemplate.TryCreate(0,0,1,0,1,-10,.5,true,out oldLine,false,100),"source-edge direction must point outwards");
        C(!BreathTemplate.TryCreate(0,0,1,0,1,30,.5,true,out oldLine,false,-1),"negative length rejects without creating a valid template");
        C(BreathTemplate.TryCreate(0,0,1,1,1,-30,.5,true,out oldLine,false,100)&&oldLine.Cell(-20,0),"aims beyond the map can represent an outward westward bolt");
        BreathTemplate centered;
        C(BreathTemplate.TryCreate(0,0,1,4,0,30,.5,true,out centered,false,100)&&!centered.Cell(0,0),"center source excludes its exact origin by default");
        C(BreathTemplate.TryCreate(0,0,1,4,0,30,.5,true,out centered,true,100)&&centered.Cell(0,0),"explicit origin inclusion can affect the caster");
        var friendlies=new[]{new BreathBody(1,2,0,1),new BreathBody(2,3,0,2),new BreathBody(3,4,2,1)};int affected=0;foreach(var body in friendlies)if(line.Intersects(body.X,body.Y,body.Spaces))affected++;
        C(affected==2&&line.Intersects(3,0,2),"geometry does not exclude allies, and a multi-cell body is selected only once");
        C(LightningBoltRules.DiceCount(3)==8&&LightningBoltRules.DiceCount(4)==9&&LightningBoltRules.DiceCount(9)==14&&LightningBoltRules.DiceCount(2)==0&&LightningBoltRules.DiceCount(10)==0,"damage grows by slot from eight d6, never by character level");
        double aimX,aimY;BreathTemplate firstDirection=default(BreathTemplate),changedDirection=default(BreathTemplate);
        C(LightningBoltRules.DirectionAim(2,3,3,0,1,1,out aimX,out aimY)&&BreathTemplate.TryCreate(2,3,3,0,1,aimX,aimY,true,out firstDirection,false,100),"shortcut aim uses the current source edge");
        C(LightningBoltRules.DirectionAim(2,3,3,3,4,1,out aimX,out aimY)&&BreathTemplate.TryCreate(2,3,3,3,4,aimX,aimY,true,out changedDirection,false,100),"shortcut aim is recomputed after changing source edge and offset");
        C(Math.Abs(firstDirection.Dx-changedDirection.Dx)<.000001&&Math.Abs(firstDirection.Dy-changedDirection.Dy)<.000001,"changing source preserves the selected northeast shortcut instead of silently tilting its direction");
        C(LightningBoltRules.DirectionAim(0,0,1,1,1,4,out aimX,out aimY)&&aimX<0&&BreathTemplate.TryCreate(0,0,1,1,1,aimX,aimY,true,out changedDirection,false,100)&&changedDirection.Dx==-1,"west shortcut retains a negative map-exterior aim rather than resetting it east");
        var enemies=new List<BreathBody>{new BreathBody(0,7,0,1),new BreathBody(1,12,0,1)};BreathTemplate best;
        C(BreathWeaponRules.ChooseLine(20,0,0,1,enemies,out best)&&best.Line&&best.LengthFeet==100&&!best.Intersects(0,0,1)&&best.Intersects(7,0,1)&&best.Intersects(12,0,1),"monster line planner safely covers distant collinear enemies beyond old thirty-foot breath range");
        enemies=new List<BreathBody>{new BreathBody(0,3,1,1),new BreathBody(1,8,5,1),new BreathBody(2,9,5,1)};
        C(BreathWeaponRules.ChooseLine(20,5,5,1,enemies,out best)&&best.Intersects(8,5,1)&&best.Intersects(9,5,1)&&!best.Intersects(5,5,1),"monster prioritizes greatest covered enemy count while avoiding itself");
        enemies=new List<BreathBody>{new BreathBody(0,8,5,1),new BreathBody(1,4,5,1)};
        C(BreathWeaponRules.ChooseLine(20,5,5,1,enemies,out best)&&best.Intersects(4,5,1)&&!best.Intersects(8,5,1),"equal coverage prefers the nearest enemy on opposite sides");
        BreathTemplate repeat;C(BreathWeaponRules.ChooseLine(20,5,5,1,enemies,out repeat)&&repeat.X==best.X&&repeat.Y==best.Y&&repeat.Dx==best.Dx&&repeat.Dy==best.Dy,"equal candidates use a stable deterministic order");
        C(!BreathWeaponRules.ChooseLine(10,0,0,1,new List<BreathBody>(),out best),"no targets produces no safe cast, enabling existing custom-attack fallback");
        enemies=new List<BreathBody>{new BreathBody(0,2,2,1),new BreathBody(1,3,2,1)};
        C(BreathWeaponRules.ChooseLine(10,0,0,2,enemies,out best)&&!best.Intersects(0,0,2),"multi-cell caster's full footprint is excluded by safe planning");
        var damage=new CombatState();damage.PlayerEnergyResistance=7;int amount=damage.AdjustTypedDamage(true,15/2,7);C(amount==3,"successful save halves before lightning resistance and rounds down");
        damage.DamageImmunity=(p,type)=>type==7;C(damage.AdjustTypedDamage(true,15,7)==0,"lightning immunity prevents the typed result");
        Console.WriteLine("PASS: Lightning Bolt 100x5 geometry, unchanged breath defaults, origin/direction choices, ally coverage, multi-cell targets, slot scaling, safe monster planning and typed lightning mitigation");
    }
}
