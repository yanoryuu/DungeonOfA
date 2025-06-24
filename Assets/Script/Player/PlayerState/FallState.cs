using UnityEngine;

public class FallState :IPlayerState
{
    public void EnterState(PlayerStateMachine player)
    {
        Debug.Log("Entering Down State");
        player.SetAnimation("Fall");
    }

    public void UpdateState(PlayerStateMachine player)
    {
        if (player.OnGround)
        {
            if (player.LockOn)
            {
                player.ChangeState(new LockOnIdle());
            }
            else
            {
                player.ChangeState(new IdleState());
            }
        }
    }

    public void ExitState(PlayerStateMachine player)
    {
        player.OutAnimation("Fall");
        // Debug.Log("Exiting Down State");
    }
}
