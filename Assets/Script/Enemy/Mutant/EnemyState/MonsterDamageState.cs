using UnityEngine;
using R3;
using DG.Tweening;
using System;

public class MonsterDamageState : IMonsterState
{
    private IDisposable _damageTimer;

    public void EnterState(Enemy_Monster _enemy)
    {
        Debug.Log("Enter MonsterDamageState");

        AnimatorCommon.SetAction("Damage", _enemy.Animator);

        var agent = _enemy.NavMeshAgent;
        var rigidbody = _enemy.EnemyRigidbody;
        var selfTransform = _enemy.SelfTransform;
        var playerTransform = _enemy.PlayerTransform.Value;

        // NavMeshAgent一時停止
        agent.isStopped = true;

        // ノックバック方向と強さ
        if (playerTransform != null)
        {
            Vector3 knockbackDir = (selfTransform.position - playerTransform.position).normalized;
            knockbackDir.y = 0; // 上方向のノックバックを防ぐ

            float knockbackForce = 5f;
            rigidbody.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        }

        // 1.5秒後に状態復帰
        _damageTimer = Observable.Timer(TimeSpan.FromSeconds(1.5f))
            .Subscribe(_ =>
            {
                // Rigidbodyの速度をゼロに戻す（滑りすぎ防止）
                rigidbody.linearVelocity = Vector3.zero;

                agent.isStopped = false;

                if (_enemy.PlayerTransform.Value != null)
                {
                    _enemy.ChangeState(new MonsterChaseState());
                }
                else
                {
                    _enemy.ChangeState(new MonsterPatrolState());
                }
            });
    }

    public void UpdateState(Enemy_Monster _enemy)
    {
        // ダメージ中は移動や攻撃など無効
    }

    public void ExitState(Enemy_Monster _enemy)
    {
        _damageTimer?.Dispose();
    }
}