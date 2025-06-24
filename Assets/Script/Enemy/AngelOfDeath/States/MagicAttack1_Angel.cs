using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AI;

public class MagicAttack1_Angel : IAngelState
{
    // 移動離脱の設定
    private ReactiveProperty<bool> _isMovingAway = new ReactiveProperty<bool>(false);
    private float _moveDuration = 1.5f;      // 離れる時間
    private float _timer = 0f;

    public void EnterState(Enemy_Angel _enemy)
    {
        Debug.Log("Enter MagicAttack1_Angel");
        
        _isMovingAway.Where(x => x == false)
            .Subscribe(_ =>
            {
                AnimatorCommon.SetAction("MagicAttack1", _enemy.Animator);
                _enemy.AngelAttackManager.MagicAttack1(_enemy).Forget();
            });
        
        // 攻撃開始と同時に離脱処理を開始
        if (_enemy.NavMeshAgent.remainingDistance <= _enemy.EnemyStats.magicAttackBackRange)
        {
            _isMovingAway.Value = true;
        }
        else
        {
            _isMovingAway.Value = false;
        }
    }
    
    public void UpdateState(Enemy_Angel _enemy)
    {
        if (_isMovingAway.Value)
        {
            _timer += Time.deltaTime;
            
            // プレイヤーから離れる方向を計算
            Vector3 awayDirection = (_enemy.transform.position - _enemy.PlayerTransform.position).normalized;
            Vector3 targetPosition = _enemy.transform.position + awayDirection * _enemy.EnemyStats.magicAttackBackRange;

            // NavMeshAgentを使って目標位置へ移動
            if (_enemy.NavMeshAgent != null)
            {
                _enemy.NavMeshAgent.SetDestination(targetPosition);
            }
            
            // プレイヤーの方向を常に向く（Y軸のみ回転）
            Vector3 directionToPlayer = (_enemy.PlayerTransform.position - _enemy.transform.position).normalized;
            directionToPlayer.y = 0f;  // 垂直方向は無視
            _enemy.NavMeshAgent.angularSpeed = 0;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                _enemy.transform.rotation = Quaternion.Slerp(_enemy.transform.rotation, targetRotation, Time.deltaTime * 5f);
            }

            if (_enemy.EnemyStats.magicAttackBackRange <=
                Vector3.Distance(_enemy.transform.position, _enemy.PlayerTransform.position))
            {
                _isMovingAway.Value = false;
            }
            
            // 一定時間経過後、離脱処理を終了
            if (_timer >= _moveDuration)
            {
                _isMovingAway.Value = false;
            }
        }
    }
    
    public void ExitState(Enemy_Angel _enemy)
    {
        AnimatorCommon.ResetAction("MagicAttack1", _enemy.Animator);
        if (_enemy.NavMeshAgent != null)
        {
            _enemy.NavMeshAgent.speed = 0;
        }
        _enemy.NavMeshAgent.ResetPath();
        _enemy.NavMeshAgent.angularSpeed = _enemy.EnemyStats.RotateSpeed;
    }
}