using System;
using R3;
using Unity.Cinemachine;
using UnityEngine;

public class MonsterDeathState :IMonsterState
{
    public void EnterState(Enemy_Monster _enemy)
    {
        Debug.Log("EnemyState Death");
        AnimatorCommon.SetAction("Death", _enemy.Animator);
        
    }

    public void UpdateState(Enemy_Monster _enemy)
    {
        
    }

    public void ExitState(Enemy_Monster _enemy)
    {
        
    }
}
