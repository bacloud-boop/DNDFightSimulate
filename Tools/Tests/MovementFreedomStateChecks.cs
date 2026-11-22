using System;
class MovementFreedomStateChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        bool playerProtected=false,monsterProtected=false;
        var state=new CombatState();state.MovementFreedom=player=>player?playerProtected:monsterProtected;
        state.PlayerParalyzed=true;state.MonsterParalyzed=true;state.ApplyFrostSlow(true);state.ApplyFrostSlow(false);
        C(state.PlayerParalysisEffect&&state.MonsterParalysisEffect&&state.PlayerParalyzed&&state.MonsterParalyzed,"unprotected magical paralysis records source and effective condition");
        C(state.PlayerSlowed&&state.MonsterSlowed&&state.MonsterSpeed==20&&!state.PlayerCanAct&&!state.MonsterCanAct,"ordinary paralysis and frost retain original behavior");
        playerProtected=true;
        C(!state.PlayerParalyzed&&state.PlayerParalysisEffect&&state.PlayerCanAct&&!state.PlayerSlowed,"player immunity suppresses existing paralysis and frost without deleting sources");
        C(state.MonsterParalyzed&&state.MonsterSlowed,"one target's protection cannot affect another");
        monsterProtected=true;
        C(!state.MonsterParalyzed&&state.MonsterParalysisEffect&&!state.MonsterSlowed&&state.MonsterCanMove&&state.MonsterCanAct&&state.MonsterSpeed==30,"monster suppression restores permitted action and speed");
        state.PlayerParalyzed=true;state.MonsterParalyzed=true;
        C(state.PlayerParalysisEffect&&state.MonsterParalysisEffect,"new rejected application does not delete a suppressed older source");
        playerProtected=monsterProtected=false;
        C(state.PlayerParalyzed&&state.MonsterParalyzed&&state.PlayerSlowed&&state.MonsterSlowed,"ending protection restores still-existing effects");
        playerProtected=monsterProtected=true;state.PlayerParalyzed=false;state.MonsterParalyzed=false;
        C(!state.PlayerParalysisEffect&&!state.MonsterParalysisEffect,"removing a paralysis source always clears it even while suppressed");
        state.ClearShortDurationEffects();state.PlayerParalyzed=true;state.MonsterParalyzed=true;state.ApplyFrostSlow(true);state.ApplyFrostSlow(false);
        playerProtected=monsterProtected=false;
        C(!state.PlayerParalyzed&&!state.MonsterParalyzed&&!state.PlayerParalysisEffect&&!state.MonsterParalysisEffect&&!state.PlayerSlowed&&!state.MonsterSlowed,"immunity rejects new paralysis and frost rather than storing delayed effects");
        state.ApplyFrostSlow(true);state.ApplyFrostSlow(false);playerProtected=monsterProtected=true;state.EndRound(false);playerProtected=monsterProtected=false;
        C(!state.PlayerSlowed&&!state.MonsterSlowed,"frost expiry continues during suppression and cannot reappear after its normal end");
        state.MonsterSpeedBonus=-5;state.ApplyFrostSlow(false);C(state.MonsterSpeed==15,"ordinary speed penalty combines with magical frost");monsterProtected=true;
        C(state.MonsterSpeed==25,"freedom suppresses only magical frost, retaining armor/load/exhaustion speed penalties");
        state.MonsterFeignDeath=true;C(state.MonsterSpeed>0&&state.MonsterCanMove&&!state.MonsterCanAct,"freedom suppresses Feign Death magical speed zero but preserves incapacitation");state.MonsterFeignDeath=false;
        state.MonsterRestrained=true;C(state.MonsterSpeed==0,"raw restraint remains external policy rather than being indiscriminately erased");state.MonsterRestrained=false;
        monsterProtected=false;state.PlayerParalyzed=true;state.MonsterParalyzed=true;state.ApplyFrostSlow(true);state.ApplyFrostSlow(false);playerProtected=monsterProtected=true;state.ResetBattle();playerProtected=monsterProtected=false;
        C(!state.PlayerParalysisEffect&&!state.MonsterParalysisEffect&&!state.PlayerParalyzed&&!state.MonsterParalyzed&&!state.PlayerSlowed&&!state.MonsterSlowed,"battle reset clears raw sources regardless of protection");
        var legacy=new CombatState();legacy.PlayerParalyzed=true;legacy.MonsterParalyzed=true;legacy.ApplyFrostSlow(false);C(legacy.PlayerParalyzed&&legacy.MonsterParalyzed&&legacy.MonsterSlowed,"null callback preserves all legacy behavior");
        Console.WriteLine("PASS: movement-freedom suppression, original paralysis source access, new-effect immunity, explicit removal, frost expiry, target isolation, physical speed penalties and reset");
    }
}
