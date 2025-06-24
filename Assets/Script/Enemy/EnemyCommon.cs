using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public static class EnemyCommon
{
    // 視野内にあるターゲットを検出する処理
    public static Transform FindVisibleTargets(Transform enemytransform, float viewRadius,
        float viewAngle, LayerMask targetMask,LayerMask obstacleMask)
    {
        Transform visibleTargets = null;
        // viewRadius 内のターゲットを取得
        Collider[] targetsInViewRadius = Physics.OverlapSphere(enemytransform.position, viewRadius, targetMask);

        foreach (Collider target in targetsInViewRadius)
        {
            Transform targetTransform = target.transform;
            Vector3 dirToTarget = (targetTransform.position - enemytransform.position).normalized;
            // 自身の forward とターゲット方向の角度が視野角の半分以内かチェック
            if (Vector3.Angle(enemytransform.forward, dirToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(enemytransform.position, targetTransform.position);
                // Raycast で障害物が間にないか確認
                if (!Physics.Raycast(enemytransform.position, dirToTarget, distanceToTarget, obstacleMask))
                {
                    visibleTargets = targetTransform;
                }
            }
        }

        if (visibleTargets != null)
        {
            return visibleTargets;
        }
        else
        {
            return null;
        }
    }
    
    public static async UniTaskVoid WaitUntilDestinationReached(NavMeshAgent _navMeshAgent)
    {
        // NavMeshAgentのパスが存在せず、停止状態になっているかをチェック
        await UniTask.WaitUntil(() =>
            !_navMeshAgent.pathPending &&
            _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance &&
            (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude == 0f)
        );
    }
}
