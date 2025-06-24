using System;
using R3;
using UnityEngine;

public class ArcherIdleState : IArcherState
{
    private IDisposable _disposable;
    public void EnterState(Enemy_Archer _enemy)
    {
        Debug.Log("EnemyState Idle");
        _disposable = Observable.Interval(TimeSpan.FromSeconds(3))
            .Take(1)
            .Subscribe(_ => _enemy.ChangeState(new ArcherPatrolState()));
        
        _enemy.NavMeshAgent.destination = _enemy.NavMeshAgent.transform.position;
    }

    public void UpdateState(Enemy_Archer _enemy)
    {
        if (_enemy.PlayerTransform.Value != null)
        {
            _enemy.ChangeState(new ArcherChaseState());
        }
    }

    public void ExitState(Enemy_Archer _enemy)
    {
        _disposable.Dispose();
    }
}
