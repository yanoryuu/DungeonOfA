using DG.Tweening;
using UnityEngine;

public class LockOnIdle : IPlayerState
{
    private Tweener tweener;
    public void EnterState(PlayerStateMachine player)
    {
        Debug.Log("Enter Lock On Idle");
        player.SetAction("StartLockOn");
        tweener = DOTween.To(() => player._blend.Value, x => player._blend.Value = x, 0f, 1f)
            .SetEase(Ease.Linear);
    }

    public void UpdateState(PlayerStateMachine player)
    {
        if (player.InputMoveDirection.magnitude > 0.1f)
        {
            player.ChangeState(new LockOnMove());
            return;
        }
        
        if (player.JumpAction)
        {
            player.ChangeState(new JumpState());
            return;
        }

        if (player.Height.Value > 0.3f)
        {
            player.ChangeState(new FallState());
            return;
        }
        
        if (player.RollingAction)
        {
            player.ChangeState(new RollingState());
            return;
        }
        
        if (player.AttackAction)
        {
            player.ChangeState(new AttackState());
            return;
        }
    }

    public void ExitState(PlayerStateMachine player)
    {
        tweener?.Kill();
        player.ResetAction("StartLockOn");
        // Debug.Log("Exiting Lock On Idle");
    }
}
