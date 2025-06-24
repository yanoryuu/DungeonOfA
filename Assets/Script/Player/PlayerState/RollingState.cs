using UnityEngine;

public class RollingState : IPlayerState
{
    public void EnterState(PlayerStateMachine player)
    {
        Debug.Log("Entering RollingState");
        player.SetAction("Rolling");
        SoundManager.Instance.PlaySE3D(SoundManager.Instance.slidingSE,player.transform.position);
        player.Rolling();
    }

    public void UpdateState(PlayerStateMachine player)
    {
        
    }

    public void ExitState(PlayerStateMachine player)
    {
        
    }
}
