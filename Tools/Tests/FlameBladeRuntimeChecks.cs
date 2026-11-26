using System;
using System.Linq;
using UnityEngine;
namespace UnityEngine {
 public sealed class TextAsset {public string text="";}
 public static class Resources {public static T Load<T>(string path)=>default(T);}
 public static class JsonUtility {public static T FromJson<T>(string json)=>default(T);}
}
public sealed class Weapon {public string Name="长剑";public bool RequiresTwoHands=false;}
public sealed class SpellMetadata {public string name;public string[] components;}
public sealed class SpellMetadataSet {public SpellMetadata[] spells;}
class FlameBladeChecksMain {public static void Main()=>TrainingGame.CheckFlameBlade();}
public sealed partial class TrainingGame {
 sealed class CarriedStub {public bool Present=true;public bool Carried(string name)=>Present;}sealed class ResurrectionCasterStub {public bool CanCast=true;}
 readonly CarriedStub carriedWeapons=new CarriedStub();readonly ResurrectionCasterStub playerResurrectionCaster=new ResurrectionCasterStub();readonly WeaponGripState weaponGrip=new WeaponGripState();readonly AmmunitionState ammunition=new AmmunitionState();readonly VisionState vision=new VisionState{BaseLight=2};Weapon selectedWeapon=new Weapon();
 bool PlayerHasShield=false,PlayerInBeastForm=false,PlayerArmorUntrained=false,voiceAllowed=true;
 SpellMetadataSet spellMetadata=new SpellMetadataSet{spells=new[]{new SpellMetadata{name="Shield",components=new[]{"声音","姿势"}},new SpellMetadata{name="Divine Smite",components=new[]{"声音"}}}};
 bool SpellVoiceAllowed(bool player,string name)=>voiceAllowed;
 void EndConcentration(string reason){EndFlameBlade();concentrationSpell="";concentrationExpiresWorld=-1;}
 static TrainingGame BladeGame(int slot=2){UnityEngine.Random.D20=10;UnityEngine.Random.D20Queue.Clear();UnityEngine.Random.D8.Clear();var g=new TrainingGame();g.map.Monster=new Vector2Int(1,0);g.combat.BonusActionAvailable=false;g.CastFlameBlade(3,slot);return g;}
 public static void CheckFlameBlade(){
  C(FlameBladeEffect.Dice(2)==3&&FlameBladeEffect.Dice(9,true)==20&&FlameBladeEffect.Dice(1)==0&&!FlameBladeEffect.ValidTime(long.MaxValue-99),"2024 slot scaling, critical and time bounds");
  var g=BladeGame();Run(g.StrikeFlameBlade());C(g.damage==20&&!g.combat.MagicActionAvailable&&!g.combat.BonusActionAvailable&&g.flameBlade.Active(0)&&g.FlameBladeHeld&&g.hitTargets.Count==1,"initial bonus conjures only; separate Magic action makes exactly one three-d6-plus-modifier attack and retains blade");
  g=BladeGame(3);UnityEngine.Random.D20=20;Run(g.StrikeFlameBlade());C(g.damage==50,"third-slot critical is eight d6 plus two, not doubled modifier");
  g=BladeGame();g.attributes[3]=3;UnityEngine.Random.D20=18;Run(g.StrikeFlameBlade());C(g.damage==14,"negative casting modifier is added and floored only at total zero");
  g=BladeGame();UnityEngine.Random.D20=1;Run(g.StrikeFlameBlade());C(g.damage==0&&g.FlameBladeHeld&&!g.combat.MagicActionAvailable,"miss spends Magic action but preserves blade");
  g=BladeGame();g.flameBladeTarget=0;g.intercept=true;Run(g.StrikeFlameBlade());C(g.damage==0&&g.FlameBladeHeld,"mirror interception preserves ongoing flame");
  g=BladeGame();C(g.vision.Light(0,0,0)==0&&g.vision.Light(10,0,0)==0&&g.vision.Light(15,0,0)==1&&g.vision.Light(20,0,0)==1&&g.vision.Light(21,0,0)==2,"exact ten-foot bright plus ten-foot dim light");
  g.ReleaseFlameBlade("test");C(!g.FlameBladeHeld&&g.flameBlade.Active(0)&&g.vision.Zones.Count==0,"letting go hides blade and light without ending concentration");g.worldRounds=5;g.combat.BonusActionAvailable=true;g.voiceAllowed=false;g.RecallFlameBlade();C(g.FlameBladeHeld&&!g.combat.BonusActionAvailable&&g.flameBlade.Expires==100,"recall needs only bonus and free hand, not voice or new duration");
  g=BladeGame();g.PlayerInBeastForm=true;g.RefreshFlameBlade();C(!g.flameBlade.Held&&g.flameBlade.Active(0)&&g.concentrationSpell=="焰刃"&&g.vision.Zones.Count==0,"entering any beast form lets go but keeps concentration");g.combat.BonusActionAvailable=true;g.RecallFlameBlade();C(!g.FlameBladeHeld&&g.combat.BonusActionAvailable&&g.FlameBladeError()!="","beast cannot conjure or recall and failed attempt spends nothing");g.PlayerInBeastForm=false;g.RefreshFlameBlade();C(!g.FlameBladeHeld,"returning to normal does not automatically restore blade");g.RecallFlameBlade();C(g.FlameBladeHeld&&!g.combat.BonusActionAvailable&&g.flameBlade.Expires==100,"normal form can spend a bonus to recall remaining spell");
  g=BladeGame();C(!g.PlayerFreeHand&&!g.ArmorSpellAllowed("Shield")&&g.ArmorSpellAllowed("Divine Smite"),"actual somatic gate blocks full hands but allows verbal-only spells");g.weaponGrip.TrySetStowed(true,1,0,null,true);C(g.PlayerFreeHand&&g.ArmorSpellAllowed("Shield"),"stowing regular weapon frees a hand without dropping flame");g.PlayerHasShield=true;C(!g.PlayerFreeHand&&!g.ArmorSpellAllowed("Shield")&&!g.PlayerCanDrawOrdinaryWeapon,"shield and flame occupy both hands and prevent drawing a third item");
  g=BladeGame();g.selectedWeapon=new Weapon{RequiresTwoHands=true};C(g.WeaponHandError(g.selectedWeapon)!="","actual attack gate rejects two-handed weapon while flame occupies one hand");g.ReleaseFlameBlade("test");C(g.PlayerFreeHand&&g.WeaponHandError(g.selectedWeapon)==""&&g.FlameBladeError()=="","two-handed regular weapon may be held in one hand between attacks, leaving a hand to conjure");
  g=BladeGame();g.selectedWeapon=new Weapon{Name="手弩"};C(g.WeaponHandError(g.selectedWeapon)==""&&g.ammunition.Fire("手弩",g.PlayerReloadHandFree),"preloaded one-handed ammunition weapon can fire once with flame held");C(g.WeaponHandError(g.selectedWeapon)!=""&&!g.ammunition.Fire("手弩",g.PlayerReloadHandFree),"both occupied hands prevent reloading");g.ReleaseFlameBlade("test");C(g.WeaponHandError(g.selectedWeapon)==""&&g.ammunition.Fire("手弩",g.PlayerReloadHandFree),"letting go frees loading hand");
  g=BladeGame();g.ReleaseFlameBlade("test");g.PlayerHasShield=true;g.combat.BonusActionAvailable=true;g.RecallFlameBlade();C(g.combat.BonusActionAvailable&&!g.FlameBladeHeld,"failed recall with no free hand never spends bonus");
  g=BladeGame();g.carriedWeapons.Present=false;C(g.PlayerFreeHand,"a dropped/absent ordinary weapon cannot occupy a phantom hand");
  g=BladeGame();g.map.Scene=1;g.RefreshFlameBlade();C(g.FlameBladeHeld&&g.vision.Zones.Count==0,"viewing another plane hides carried light without ending spell");g.map.Scene=0;g.RefreshFlameBlade();C(g.vision.Light(0,0,0)==0,"light returns with the caster scene");g.map.PlayerContained=true;g.RefreshFlameBlade();C(g.FlameBladeHeld&&g.vision.Zones.Count==0,"swallowed blade stays held but does not illuminate outside");
  g=BladeGame();g.vision.AddZone(true,0,0,2,0);g.RefreshFlameBlade();C(!g.flameBlade.Active(0)&&g.concentrationSpell=="","second-slot light overlap dispels whole blade and concentration");g=BladeGame(3);g.vision.AddZone(true,0,0,2,0);g.RefreshFlameBlade();C(g.FlameBladeHeld&&g.vision.Light(0,0,0)==0,"third-slot blade survives Darkness");
  g=BladeGame();g.worldRounds=100;g.combat.BonusActionAvailable=true;Run(g.StrikeFlameBlade());g.RecallFlameBlade();C(g.damage==0&&g.combat.MagicActionAvailable&&g.combat.BonusActionAvailable,"expired spell cannot spend an action or bonus");g.EndFlameBlade();C(!g.flameBlade.Active(0)&&g.vision.Zones.Count==0,"cleanup resets held state, duration and light");
  Console.WriteLine("PASS: production Flame Blade magic attacks, 2024 dice/modifier, recall without recasting, beast release/resume, real hand/component/weapon gates, ammo loading, exact light, planes, Darkness and expiry");
 }
}
