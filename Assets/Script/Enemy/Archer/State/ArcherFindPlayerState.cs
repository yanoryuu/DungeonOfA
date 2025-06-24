using DG.Tweening;
using UnityEngine;

public class ArcherFindPlayerState : IArcherState
{
    public Transform target;
    public void EnterState(Enemy_Archer _enemy)
    {
        Debug.Log("Entering MonsterRoaringState");
        
        AnimatorCommon.SetAction("Roaring",_enemy.Animator);

        target = _enemy.PlayerTransform.Value;
        
        _enemy.NavMeshAgent.destination = _enemy.SelfTransform.position;
        
        if (target == null) return;

        Vector3 direction = (target.position - _enemy.SelfTransform.position).normalized;
        if (direction == Vector3.zero) return;

        //5秒で吠えるアニメーションが終わる
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        _enemy.SelfTransform.DORotateQuaternion(targetRotation, 5).SetEase(Ease.InOutSine)
            .OnComplete(()=>_enemy.ChangeState(new ArcherChaseState()));
    }

    public void UpdateState(Enemy_Archer _enemy)
    {
        
    }

    public void ExitState(Enemy_Archer _enemy)
    {
        
    }
}
