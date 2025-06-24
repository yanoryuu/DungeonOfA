using UnityEngine;

public interface IMonsterState
{
    void EnterState(Enemy_Monster _enemy);
    void UpdateState(Enemy_Monster _enemy);
    void ExitState(Enemy_Monster _enemy);
}
