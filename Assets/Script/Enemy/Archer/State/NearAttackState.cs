using UnityEngine;

public class NearAttackState : IArcherState
{
    private int _attackRandomIndex;    
    
    public void EnterState(Enemy_Archer _enemy)
    {
        Debug.Log("Enter NearAttackState");
        
        AnimatorCommon.SetAction("Punch",_enemy.Animator);
    }

    public void UpdateState(Enemy_Archer _enemy)
    {
        _enemy.SelfTransform.LookAt(_enemy.PlayerTransform.Value.position);
    }

    public void ExitState(Enemy_Archer _enemy)
    {
        AnimatorCommon.ResetAction("Punch",_enemy.Animator);
    }
}
