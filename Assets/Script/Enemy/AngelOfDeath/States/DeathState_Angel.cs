using UnityEngine;

public class DeathState_Angel : IAngelState
{
    public void EnterState(Enemy_Angel _enemy)
    {
        Debug.Log("Entered  DeathState_Angel");
        AnimatorCommon.SetAction("Death",_enemy.Animator);
    }

    public void UpdateState(Enemy_Angel _enemy)
    {
        
    }

    public void ExitState(Enemy_Angel _enemy)
    {
        AnimatorCommon.ResetAction("Death", _enemy.Animator);
    }
}
