using UnityEngine;

public class ArcherDeathState : IArcherState
{
    public void EnterState(Enemy_Archer _enemy)
    {
        Debug.Log("EnemyState Death");
        AnimatorCommon.SetAction("Death", _enemy.Animator);
    }

    public void UpdateState(Enemy_Archer _enemy)
    {
        
    }

    public void ExitState(Enemy_Archer _enemy)
    {
        
    }
}
