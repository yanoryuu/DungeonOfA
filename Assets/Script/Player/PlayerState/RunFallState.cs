using UnityEngine;

public class RunFallState :IPlayerState
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
                if (player.InputMoveDirection.magnitude> 0.1f)
                {
                    player.SetAction("RunJumpToRun");
                    player.ChangeState(new LockOnMove());
                }
                else
                {
                    player.SetAction("RunJumpToIdle");
                    player.OutAnimation("FrontMove");
                    player.ChangeState(new LockOnIdle());
                }
            }
            else
            {
                if (player.InputMoveDirection.magnitude> 0.1f)
                {
                    player.SetAction("RunJumpToRun");
                    player.ChangeState(new MoveState());
                }
                else
                {
                    player.SetAction("RunJumpToIdle");
                    player.OutAnimation("FrontMove");
                    player.ChangeState(new IdleState());
                }
            }
        }
    }

    public void ExitState(PlayerStateMachine player)
    {
        player.OutAnimation("Fall");
        Debug.Log("Exiting Down State");
    }
}