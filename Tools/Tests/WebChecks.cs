using System;
using UnityEngine;
class WebChecks
{
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
    static void Main()
    {
        var web=new WebEffect();web.Begin(2,2,65535&~(1<<5),19,10);
        C(web.Cell(2,2,10)&&!web.Cell(3,3,10)&&!web.Cell(6,2,10),"land-only four-by-four area respects water holes and bounds");
        C(web.Active(609)&&!web.Active(610)&&!web.Active(9),"one-hour concentration duration");
        C(web.Intersects(1,1,2,10)&&!web.Intersects(0,0,1,10),"one occupied web cell can affect a large creature");
        C(!web.Restrained(0,10),"creating web does not instantly restrain or demand an initial save");
        C(web.TrySave(0,1,10)&&!web.TrySave(0,1,10)&&web.TrySave(0,2,10)&&web.TrySave(-1,2,10),"entry and own-start triggers are limited per creature per turn, including other creatures' turns");
        C(web.Bind(0,10)&&web.Restrained(0,10)&&web.Release(0)&&!web.Restrained(0,10)&&web.Cell(2,2,10),"Athletics escape removes restraint, preserving terrain");
        long token=web.Revision;try{web.Begin(0,0,65536,10,20);throw new Exception("invalid mask accepted");}catch(ArgumentOutOfRangeException){}
        C(web.Revision==token&&web.Cell(2,2,10),"invalid cast is atomic");
        web.StartTurn(0);C(web.Ignite(2,2,0,10)&&!web.Ignite(3,3,0,10)&&!web.Ignite(2,2,0,10),"only existing land web ignites; another exposure cannot extend its burn");
        web.StartTurn(-1);C(web.BurningBody(1,1,2,10)&&web.Cell(2,2,10),"fire remains through other actors' intervening turns");
        web.StartTurn(0);C(!web.Cell(2,2,11)&&web.Cell(3,2,11),"burned cells vanish at the next initiative point, leaving other web intact");
        web.Ignite(3,2,-1,11);web.Ignite(4,2,12,11);web.ExpireMissingSummonOwners(id=>false);C(web.Cell(3,2,11)&&!web.Cell(4,2,11),"a vanished summon cannot indefinitely delay fire expiry");
        web.Refresh(12,true);C(!web.Cell(3,2,12),"aftermath waiting burns a cell away after six seconds");
        web.Begin(0,0,65535,12,20);var vision=new VisionState{BaseLight=0,LightObscuration=(x,y,now)=>web.Cell((int)Math.Floor((x+2.5)/5),(int)Math.Floor((y+2.5)/5),now)};
        C(vision.Quality(30,0,5,5,120,20)==1&&vision.Quality(5,5,30,0,0,20)==0,"web lightly obscures its contents, without blocking attacks or sight through to outside");
        C(vision.Quality(0,0,5,5,0,20,10)==0&&vision.Quality(0,0,5,5,0,20,0,true)==1,"blindsight bypasses physical obscuration while truesight alone does not");
        var map=new BattleMap();web.Begin(1,0,65535,12,30);map.DifficultTerrain=(x,y,n)=>{for(int j=0;j<n;j++)for(int i=0;i<n;i++)if(!web.Cell(x+i,y+j,30))return false;return n>0;};map.RestrainedActor=player=>web.Restrained(player?0:-1,30);map.Lock();
        C(map.PlayerMovementCost(V(1,0))==10,"fully entered web adds difficult-terrain cost");web.Bind(0,30);C(!map.CanMoveTo(V(1,0)),"restrained players cannot voluntarily move");web.Release(0);C(map.CanMoveTo(V(1,0)),"escaping restores movement access");
        var combat=new CombatState();int fireContacts=0;combat.FireExposure=player=>fireContacts++;combat.DamageImmunity=(player,type)=>type==5;
        C(combat.AdjustTypedDamage(true,9,5)==0&&fireContacts==1,"fire immunity prevents damage without preventing exposure of the surrounding web");
        var packet=new long[13];packet[1]=9;combat.AdjustDamagePacket(true,packet);C(fireContacts==1,"nonfire attacks do not accidentally ignite a zero fire component");packet[5]=1;combat.AdjustDamagePacket(false,packet);C(fireContacts==2,"mixed custom damage exposes fire before resistance and immunity");
        int started=99;combat.TurnStarted=actor=>started=actor;combat.StartMonsterTurn(false);C(started==99,"resource-only resumption does not fake a monster turn");combat.StartMonsterTurn();C(started==-1,"monster turn start hook");combat.EndRound();C(started==0,"new player turn hook");
        var unit=new SummonedUnit(2,"spirit",20,12,30,2,1);unit.FireExposure=()=>fireContacts++;unit.Damage(2,5);C(fireContacts==3,"summon damage exposes fire");var rescue=new RescueCarrier();rescue.BeginTurn(1);rescue.Restrained=true;C(rescue.EffectiveSpeed==0&&!rescue.Move(5)&&rescue.UseAction(),"restrained rescuers retain actions but lose movement");
        var spell=DirectSpell.Find("Web");C(spell!=null&&spell.Level==2&&spell.Range==60&&!spell.Attack&&!spell.Healing,"Web has its dedicated nondamage casting route");
        web.Clear();C(!web.Active(30)&&!web.Restrained(0,30)&&!web.Burning(1,0,30)&&web.Revision!=token,"concentration ending or resetting clears cells, bindings, fire and stale queue identity");
        Console.WriteLine("PASS: Web land mask, duration, once-per-turn saves, escape, partial coverage, movement, vision, fire exposure and burn timing, turn hooks, summons and rescue");
    }
}
