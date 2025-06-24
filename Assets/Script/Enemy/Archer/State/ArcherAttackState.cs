using UnityEngine;

public class ArcherAttackState : IArcherState
{
    private int _attackRandomIndex;    
    
    public void EnterState(Enemy_Archer _enemy)
    {
        Debug.Log("Enter MonsterAttackState");
        _attackRandomIndex=Random.Range(0,2);
        
        AnimatorCommon.SetAction("Attack",_enemy.Animator);
        
        _enemy.NavMeshAgent.destination=_enemy.Animator.transform.position;
    }

    public void UpdateState(Enemy_Archer _enemy)
    {
        if (_enemy.PlayerTransform == null)
        {
            _enemy.ChangeState(new ArcherPatrolState());   
            return;
        }
        else
        {
            _enemy.SelfTransform.LookAt(_enemy.PlayerTransform.Value.position);
            return;
        }
        
    }

    public void ExitState(Enemy_Archer _enemy)
    {
        AnimatorCommon.ResetAction("Attack",_enemy.Animator);
    }
}
