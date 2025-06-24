using Cysharp.Threading.Tasks;
using UnityEngine;

public class ChaseState_Angel : IAngelState
{
    public void EnterState(Enemy_Angel _enemy)
    {
        Debug.Log("Entered ChaseState_Angel");
        AnimatorCommon.SetAnimation("Move", _enemy.Animator);
        _enemy.NavMeshAgent.enabled = true;
    }

    public void UpdateState(Enemy_Angel _enemy)
    {
        
       
            _enemy.NavMeshAgent.destination = _enemy.PlayerTransform.position;
            // 追跡移動処理
            if (_enemy.NavMeshAgent.remainingDistance <= _enemy.EnemyStats.spacing)
            {
                // Debug.Log("Playerから近い");
                _enemy.NavMeshAgent.speed = 0;
            }
            else
            {
                // Debug.Log("Playerから遠い");
                _enemy.NavMeshAgent.speed = _enemy.EnemyStats.moveSpeed;
            }

        if (_enemy.AttackCoolDown) return;

        Transform targetNear = _enemy.NearRangeAreaChecker();
        Transform targetFar = _enemy.LongRangeAreaChecker();

        // プレイヤーを検知しているか
        if (targetNear == null && targetFar == null) return;

        AttackType type = _enemy.SelectAttackType();

        // 攻撃タイプごとの分岐（距離も確認）
        switch (type)
        {
            case AttackType.Sword:
                if (targetNear != null)
                {
                    _enemy.ChangeState(SelectRandomSwordAttack());
                }
                break;

            case AttackType.Magic:
                if (targetFar != null)
                {
                    _enemy.ChangeState(SelectRandomMagicAttack());
                }
                break;
        }
    }

    public void ExitState(Enemy_Angel _enemy)
    {
        AnimatorCommon.ResetAction("Move", _enemy.Animator);
        _enemy.NavMeshAgent.speed = _enemy.EnemyStats.moveSpeed;
    }

    // 補助：剣攻撃ランダム選択
    private IAngelState SelectRandomSwordAttack()
    {
        if (Random.value > 0.3f)
        {
            return Random.value > 0.5f
                ? new SwordAttack1State_Angel()
                : new SwordAttack2State_Angel();
        }
        return new SwordAttack3State_Angel();
    }

    // 補助：魔法攻撃ランダム選択
    private IAngelState SelectRandomMagicAttack()
    {
        if (Random.value > 0.3f)
        {
            return Random.value > 0.5f
                ? new MagicAttack1_Angel()
                : new MagicAttack2_Angel();
        }
        return new BigMagicAttack_Angel();
    }
}
