using UnityEngine;

public class DamagedState_Angel : IAngelState
{
    public void EnterState(Enemy_Angel _enemy)
    {
        Debug.Log("Entered SwordAttackState_Angel");
        AnimatorCommon.SetAction("Damaged",_enemy.Animator);
    }

    public void UpdateState(Enemy_Angel _enemy)
    {
        
    }

    public void ExitState(Enemy_Angel _enemy)
    {
        AnimatorCommon.ResetAction("Damaged", _enemy.Animator);
    }
}
