using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MonsterRoaringState : IMonsterState
{
    public Transform target;
    public void EnterState(Enemy_Monster _enemy)
    {
        Debug.Log("Entering MonsterRoaringState");
        
        SoundManager.Instance.PlaySE3D(SoundManager.Instance.monsterRoarSE, _enemy.SelfTransform.position);
        
        AnimatorCommon.SetAction("Roaring",_enemy.Animator);

        target = _enemy.PlayerTransform.Value;
        
        _enemy.NavMeshAgent.destination = _enemy.SelfTransform.position;
        
        if (target == null) return;

        Vector3 direction = (target.position - _enemy.SelfTransform.position).normalized;
        if (direction == Vector3.zero) return;

        //5秒で吠えるアニメーションが終わる
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        _enemy.SelfTransform.DORotateQuaternion(targetRotation, 5).SetEase(Ease.InOutSine)
            .OnComplete(()=>_enemy.ChangeState(new MonsterChaseState()));
    }

    public void UpdateState(Enemy_Monster _enemy)
    {
        
    }

    public void ExitState(Enemy_Monster _enemy)
    {
        
    }
}
