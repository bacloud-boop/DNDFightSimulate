// A round completes only after the player and the deployed rescuer finish.
public sealed class AftermathTurns
{
    public bool Active {get;private set;}
    public bool RescueTurn {get;private set;}
    public bool PlayerTurn=>!Active||!RescueTurn;
    public void Enter(){if(Active)return;Active=true;RescueTurn=false;}
    public bool Finish(bool rescueDeployed)
    {
        if(!Active)return false;
        if(!RescueTurn&&rescueDeployed){RescueTurn=true;return false;}
        RescueTurn=false;return true;
    }
    public void Exit(){Active=false;RescueTurn=false;}
}
