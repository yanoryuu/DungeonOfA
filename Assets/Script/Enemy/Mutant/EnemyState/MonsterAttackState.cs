using R3;
using UnityEngine;

public class MonsterAttackState : IMonsterState
{
    private int _attackRandomIndex;
    public void EnterState(Enemy_Monster _enemy)
    {
        Debug.Log("Enter MonsterAttackState");
        _attackRandomIndex=Random.Range(0,2);
        if (_attackRandomIndex == 0)
        {
            AnimatorCommon.SetAction("Punch",_enemy.Animator);
        }else if (_attackRandomIndex == 1)
        {
            AnimatorCommon.SetAction("Swiping",_enemy.Animator);
        }
        
        _enemy.NavMeshAgent.destination=_enemy.Animator.transform.position;
    }

    public void UpdateState(Enemy_Monster _enemy)
    {
        
    }

    public void ExitState(Enemy_Monster _enemy)
    {
        
    }
}