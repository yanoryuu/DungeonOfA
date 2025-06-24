using DG.Tweening;
using UnityEngine;

public class IdleState : IPlayerState
{
    private Tweener tweener;
    public void EnterState(PlayerStateMachine player)
    {
        // Debug.Log("Entering Idle State");
        player.SetAction("StopLockOn");
        tweener = DOTween.To(() => player._blend.Value, x => player._blend.Value = x, 0f, 1f)
            .SetEase(Ease.Linear);
    }

    public void UpdateState(PlayerStateMachine player)
    {
        Vector2 moveInput = player.InputMoveDirection;
        
        // 移動方向ベクトルを作成（3D空間対応）
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y); // 前後（z）と左右（x）の移動
        
        if (moveDirection.magnitude > 0.1f) // 入力があるとき
        {
            player.ChangeState(new MoveState());
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
        // player.ResetAction("StopLockOn");
        tweener?.Kill();
    }
}