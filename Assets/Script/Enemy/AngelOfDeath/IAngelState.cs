using UnityEngine;

public interface IAngelState 
{
    void EnterState(Enemy_Angel _enemy);
    void UpdateState(Enemy_Angel _enemy);
    void ExitState(Enemy_Angel _enemy);
}
