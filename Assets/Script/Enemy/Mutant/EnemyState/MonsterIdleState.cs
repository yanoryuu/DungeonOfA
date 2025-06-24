using System;
using R3;
using UnityEngine;

public class MonsterIdleState : IMonsterState
{
    private IDisposable _disposable;
    public void EnterState(Enemy_Monster _enemy)
    {
        Debug.Log("EnemyState Idle");
        _disposable = Observable.Interval(TimeSpan.FromSeconds(3))
            .Take(1)
            .Subscribe(_ => _enemy.ChangeState(new MonsterPatrolState()));
        
        _enemy.NavMeshAgent.destination = _enemy.NavMeshAgent.transform.position;
    }

    public void UpdateState(Enemy_Monster _enemy)
    {
        if (_enemy.PlayerTransform.Value != null)
        {
            _enemy.ChangeState(new MonsterChaseState());
        }
    }

    public void ExitState(Enemy_Monster _enemy)
    {
        _disposable.Dispose();
    }
}
