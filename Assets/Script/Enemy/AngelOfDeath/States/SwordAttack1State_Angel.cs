using UnityEngine;

public class SwordAttack1State_Angel : IAngelState
{
    public void EnterState(Enemy_Angel _enemy)
    {
        Debug.Log("Entered SwordAttack1State_Angel");
        AnimatorCommon.SetAction("SwordAttack1",_enemy.Animator);
        _enemy.AngelAttackManager.SwordAttack1();
    }

    public void UpdateState(Enemy_Angel _enemy)
    {
            _enemy.NavMeshAgent.destination = _enemy.PlayerTransform.position;
            // 追跡移動処理
            if (_enemy.NavMeshAgent.remainingDistance <= _enemy.EnemyStats.spacing)
            {
                // Debug.Log("Playerから近い");
                _enemy.NavMeshAgent.speed = 0;
            }
            else
            {
                // Debug.Log("Playerから遠い");
                _enemy.NavMeshAgent.speed = _enemy.EnemyStats.moveSpeed;
            }
        
    }

    public void ExitState(Enemy_Angel _enemy)
    {
        AnimatorCommon.ResetAction("SwordAttack1", _enemy.Animator);
    }
}
