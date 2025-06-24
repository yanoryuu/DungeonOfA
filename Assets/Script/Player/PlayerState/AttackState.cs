using UnityEngine;

public class AttackState : IPlayerState
{
    public void EnterState(PlayerStateMachine player)
    {
        Debug.Log("Enter Attack");
        player.SetAction("Attack");
        // StartComboAttackはアニメーションイベントで呼ばれる
    }

    public void UpdateState(PlayerStateMachine player)
    {
        if (player.IsIdle())
        {
            player.BackIdleAnimation();
        }
    }

    public void ExitState(PlayerStateMachine player)
    {
        player.StopComboAttack(); // 念のため、離脱時にも停止
        player.ResetAction("Combo");
    }
}