using UnityEngine;

public class LockOnMove : IPlayerState
{
    public void EnterState(PlayerStateMachine player)
    {
        Debug.Log("Lock On Idle");
        player.SetAnimation("LockOnMove");
    }

    public void UpdateState(PlayerStateMachine player)
    {
        if (player.JumpAction)
        {
            player.ChangeState(new RunJumpState());
            return;
        }
        if (player.InputMoveDirection.magnitude < 0.1f)
        {
            player.ChangeState(new LockOnIdle());
            
            player.OutAnimation("LockOnMove");
            return;
        }
        if (player.RollingAction)
        {
            
            player.OutAnimation("LockOnMove");
            player.ChangeState(new RollingState());
            return;
        }
        if (player.AttackAction)
        {
            player.OutAnimation("LockOnMove");
            player.ChangeState(new AttackState());
            return;
        }

        if (player.PlayerLockon.TargetObj == null)
        {
            player.OutAnimation("LockOnMove");
            player.ChangeState(new IdleState());
        }
        
        player.LockOnMove();
    }

    public void ExitState(PlayerStateMachine player)
    {
        // Debug.Log("Exiting LockOnMove State");
    }
}
