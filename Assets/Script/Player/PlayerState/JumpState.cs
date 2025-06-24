using UnityEngine;

public class JumpState : IPlayerState
{
    public void EnterState(PlayerStateMachine player)
    {
        Debug.Log("Entering Jump State");
        player.SetAnimation("JumpUp");
    }

    public void UpdateState(PlayerStateMachine player)
    {
        if (player.Rigidbody.linearVelocity.y < 0)
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
        player.OutAnimation("JumpUp");
        Debug.Log("Exiting Jump State");
    }
}