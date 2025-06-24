using UnityEngine;

public class RunJumpState : IPlayerState
{
    public void EnterState(PlayerStateMachine player)
    {
        Debug.Log("Entering Jump State");
        player.SetAnimation("RunJump");
    }

    public void UpdateState(PlayerStateMachine player)
    {
        if (player.Rigidbody.linearVelocity.y < 0||player.OnGround)
        {
            player.ChangeState(new RunFallState());
        }
    }

    public void ExitState(PlayerStateMachine player)
    {
        player.OutAnimation("RunJump");
        Debug.Log("Exiting Jump State");
    }
}
