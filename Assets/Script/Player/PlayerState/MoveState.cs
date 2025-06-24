using DG.Tweening;
using UnityEngine;

public class MoveState : IPlayerState
{
    public void EnterState(PlayerStateMachine player)
    {
        // Debug.Log("Entering Walk State");
        player.SetAnimation("FrontMove");
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
            player.OutAnimation("FrontMove");
            player.ChangeState(new IdleState());
            return;
        }
        if (player.RollingAction)
        {
            player.OutAnimation("FrontMove");
            player.ChangeState(new RollingState());
            return;
        }
        if (player.AttackAction)
        {
            player.OutAnimation("FrontMove");
            player.ChangeState(new AttackState());
            return;
        }
        
        player.Move();
    }

    public void ExitState(PlayerStateMachine player)
    {
        
    }
}