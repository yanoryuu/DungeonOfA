using UnityEngine;

public interface IArcherState 
{
    void EnterState(Enemy_Archer _enemy);
    void UpdateState(Enemy_Archer _enemy);
    void ExitState(Enemy_Archer _enemy);
}
