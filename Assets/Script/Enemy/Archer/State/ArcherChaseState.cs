using UnityEngine;
using UnityEngine.AI;

public class ArcherChaseState : IArcherState
{
    public void EnterState(Enemy_Archer enemy)
    {
        Debug.Log("Entered ArcherChaseState");
        enemy.NavMeshAgent.speed = enemy.EnemyStats.moveSpeed;
    }

    public void UpdateState(Enemy_Archer enemy)
    {
        Transform target = enemy.PlayerTransform.Value;

        if (target == null)
        {
            enemy.ChangeState(new ArcherIdleState());
            return;
        }

        // 攻撃範囲内かつクールダウンでなければ攻撃へ
        if (enemy.AttackAreaChecker() != null && !enemy.AttackCoolDown)
        {
            enemy.ChangeState(new ArcherAttackState());
            return;
        }

        Vector3 enemyPos = enemy.SelfTransform.position;
        Vector3 playerPos = target.position;
        float distance = Vector3.Distance(enemyPos, playerPos);

        // プレイヤーの方向を常に向く
        Vector3 lookDir = (playerPos - enemyPos).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            enemy.SelfTransform.rotation = Quaternion.Slerp(
                enemy.SelfTransform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * 10f
            );
        }

        // 近すぎたら後退、それ以外は追跡
        if (distance <= enemy.EnemyStats.spacing)
        {
            Vector3 retreatDir = (enemyPos - playerPos).normalized;
            retreatDir.y = 0;

            Vector3 rawRetreatTarget = enemyPos + retreatDir * 3f;

            // NavMesh 上の正しい位置を取得（1.0f 半径で周辺サンプル）
            if (NavMesh.SamplePosition(rawRetreatTarget, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                enemy.NavMeshAgent.SetDestination(hit.position);
            }
            else
            {
                // NavMesh上に見つからなければそのまま追跡に戻すなど fallback 処理
                enemy.NavMeshAgent.SetDestination(playerPos);
            }
        }
        else
        {
            enemy.NavMeshAgent.SetDestination(playerPos);
        }

        // Blend Tree用アニメーション更新
        Vector3 globalVelocity = enemy.NavMeshAgent.velocity;
        Vector3 localVelocity = enemy.SelfTransform.InverseTransformDirection(globalVelocity);

        float forward = localVelocity.z;
        float strafe = localVelocity.x;

        // 滑らかにアニメーションパラメータを更新
        enemy.Animator.SetFloat("Vertical", forward, 0.1f, Time.deltaTime);
        enemy.Animator.SetFloat("Holizontal", strafe, 0.1f, Time.deltaTime);
    }

    public void ExitState(Enemy_Archer enemy)
    {
        // アニメーターの速度をリセット
        enemy.Animator.SetFloat("Vertical", 0);
        enemy.Animator.SetFloat("Holizontal", 0);
    }
}
