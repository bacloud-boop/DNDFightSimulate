using System;
class BeaconSavingThrowChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static int Select(int first,int second,bool hope,bool disadvantage=false)=>PoisonProtectionEffect.SelectDie(first,second,hope,disadvantage);
    static void Main()
    {
        var beacon=new BeaconHopeEffect();C(beacon.Begin(new[]{0,-1,7,int.MinValue},0),"one casting can protect all existing creature categories");
        C(beacon.Active(0,0)&&beacon.Active(-1,0)&&beacon.Active(7,0)&&beacon.Active(int.MinValue,0)&&!beacon.Active(8,0),"protection belongs only to selected creatures");
        int ordinarySuccess=0,advantagedSuccess=0,cancelledSuccess=0;
        for(int first=1;first<=20;first++)for(int second=1;second<=20;second++){
            if(Select(first,second,false)>=10)ordinarySuccess++;
            if(Select(first,second,true)>=10)advantagedSuccess++;
            if(Select(first,second,true,true)>=10)cancelledSuccess++;
            C(Select(first,second,true,true)==first,"any advantage and disadvantage cancel to the original die, even when the second die differs");
        }
        C(ordinarySuccess==220&&advantagedSuccess==319&&cancelledSuccess==ordinarySuccess,"DC10 save success frequencies distinguish real advantage from ordinary or cancelled rolls");
        C(Select(3,18,true)==18&&Select(18,3,true)==18,"a protected saving throw uses the higher result independent of die order");
        C(Select(3,18,false,true)==3&&Select(18,3,false,true)==3,"a remaining disadvantage still uses the lower die");
        var player=new CombatState();player.ApplyPlayerDamage(player.PlayerHp);
        C(player.NeedsDeathSave,"player begins the check at zero HP without dying instantly");
        player.ResolveDeathSave(Select(1,20,beacon.Active(0,0)));
        C(player.PlayerHp==1&&!player.NeedsDeathSave&&player.DeathFailures==0,"advantage-selected natural twenty revives the player and does not process the discarded natural one");
        var rescued=new RescueCarrier();rescued.TakeDamage(rescued.Hp);
        C(rescued.NeedsDeathSave,"rescue creature also begins unconscious rather than dead");
        rescued.ResolveDeathSave(Select(1,20,beacon.Active(int.MinValue,0)),1);
        C(rescued.Hp==1&&!rescued.NeedsDeathSave&&rescued.DeathFailures==0,"rescue death save uses the selected natural twenty before applying modifiers");
        player=new CombatState();player.ApplyPlayerDamage(player.PlayerHp);
        player.ResolveDeathSave(Select(1,20,true,true),100);
        C(player.PlayerHp==0&&player.DeathFailures==2,"cancelled advantage leaves natural one as two death failures despite large fixed bonuses");
        rescued=new RescueCarrier();rescued.TakeDamage(rescued.Hp);
        rescued.ResolveDeathSave(Select(1,20,false),1,100);
        C(rescued.Hp==0&&rescued.DeathFailures==2,"unprotected rescue natural one cannot be overridden by the unused second die or fixed bonus");
        C(!beacon.Active(0,10)&&Select(4,20,beacon.Active(0,10))==4,"benefit ends at exact concentration duration and cannot select an unused die afterward");
        beacon.Clear();C(!beacon.Active(7,0)&&Select(4,20,beacon.Active(7,0))==4,"concentration removal immediately restores ordinary save selection");
        Console.WriteLine("PASS: beacon save target isolation, complete die-pair advantage frequencies, advantage/disadvantage cancellation, player/rescue natural death-save results and exact expiration");
    }
}
