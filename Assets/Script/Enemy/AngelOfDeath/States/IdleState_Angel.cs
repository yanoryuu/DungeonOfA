using UnityEngine;

public class IdleState_Angel : IAngelState
{
    public void EnterState(Enemy_Angel _enemy)
    {
        Debug.Log("Entered IdleState_Angel");
    }

    public void UpdateState(Enemy_Angel _enemy)
    {
        if (_enemy.IsFighting)
        {
            _enemy.AngelCanvas.SetActive(true);
            _enemy.ChangeState(new ChaseState_Angel());
        }
    }

    public void ExitState(Enemy_Angel _enemy)
    {
        Debug.Log("戦闘開始");
    }
}
