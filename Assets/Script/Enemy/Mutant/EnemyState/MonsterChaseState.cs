using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class MonsterChaseState : IMonsterState
{
    public void EnterState(Enemy_Monster _enemy)
    {
        Debug.Log("Entering MonsterChaseState");
        AnimatorCommon.SetAnimation("Run",_enemy.Animator);
        _enemy.NavMeshAgent.speed = _enemy.EnemyStats.RunSpeed;
    }

    public void UpdateState(Enemy_Monster _enemy)
    {
        if (_enemy.AttackAreaChecker()!=null&&_enemy.AttackCoolDown==false)
        {
           _enemy.ChangeState(new MonsterAttackState()); 
           return;
        }

        if (_enemy.PlayerTransform.Value == null)
        {
            _enemy.ChangeState(new MonsterIdleState());
            return;
        }

        
            _enemy.NavMeshAgent.destination = _enemy.PlayerTransform.Value.position;
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

    public void ExitState(Enemy_Monster _enemy)
    {
        AnimatorCommon.OutAnimation("Run",_enemy.Animator);
        EnemyCommon.WaitUntilDestinationReached(_enemy.NavMeshAgent).Forget();
    }
}
