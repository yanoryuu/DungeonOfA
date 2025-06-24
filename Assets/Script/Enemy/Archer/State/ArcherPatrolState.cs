using System;
using R3;
using UnityEngine;

public class ArcherPatrolState : IArcherState
{
    private ReactiveProperty<int> _patrolPointIndex = new ReactiveProperty<int>(0);
    private IDisposable _patrolDisposable;
    private IDisposable _arrivalDisposable;
    public void EnterState(Enemy_Archer _enemy)
    {
        Debug.Log("EnemyState Patrol");
        // _patrolPointIndexの値が変わるたびにNavMeshAgentの目的地を更新する
        _patrolDisposable = _patrolPointIndex.Subscribe(x =>
        {
            _enemy.NavMeshAgent.destination = _enemy.PatrolPoint[_patrolPointIndex.Value].position;
        });

        _enemy.NavMeshAgent.speed = _enemy.EnemyStats.moveSpeed;
        
        AnimatorCommon.SetAnimation("Move",_enemy.Animator);
    }

    public void UpdateState(Enemy_Archer _enemy)
    {
        if (!_enemy.NavMeshAgent.pathPending && _enemy.NavMeshAgent.remainingDistance <=
            _enemy.NavMeshAgent.stoppingDistance)
        {
            _patrolPointIndex.Value = (_patrolPointIndex.Value + 1) % _enemy.PatrolPoint.Length;
        }

        if (_enemy.PlayerTransform.Value != null)
        {
            _enemy.ChangeState(new ArcherChaseState());
        }
    }

    public void ExitState(Enemy_Archer _enemy)
    {
        _patrolDisposable?.Dispose();
        AnimatorCommon.OutAnimation("Walk",_enemy.Animator);
    }
}
