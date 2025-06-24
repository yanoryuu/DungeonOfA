using System;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;

public class SpecialAttackState : IPlayerState
{
    private ChromaticAberration chromaticAberration;
    private MotionBlur motionBlur;
    private Vignette vignette;

    private bool hasAttacked = false;
    
    private IDisposable disposable = null;

    public void EnterState(PlayerStateMachine player)
    {
        Debug.Log("SpecialAttack");
        player.SetAction("SpecialAttack");
        player.Rolling();
        player._enemyManager.SlowEnemies(3);
        
        SoundManager.Instance.PlaySE2D(SoundManager.Instance.justRollingAttackMoveSE);

        var target = player.PlayerLockon.TargetObj;
        var volume = Camera.main.GetComponent<PostProcessVolume>();
        
        

        // PostProcessing演出（色収差、ブラー、ビネット）
        if (volume.profile.TryGetSettings(out chromaticAberration))
        {
            DOTween.To(() => 0f, x => chromaticAberration.intensity.value = x, 1f, 0.2f)
                .OnComplete(() =>
                {
                    DOTween.To(() => 1f, x => chromaticAberration.intensity.value = x, 0f, 0.5f);
                });
        }

        if (volume.profile.TryGetSettings(out motionBlur))
        {
            motionBlur.enabled.value = true;
            DOTween.To(() => 0f, x => motionBlur.shutterAngle.value = x, 270f, 0.2f)
                .OnComplete(() =>
                {
                    DOTween.To(() => 270f, x => motionBlur.shutterAngle.value = x, 0f, 0.5f);
                });
        }

        if (volume.profile.TryGetSettings(out vignette))
        {
            vignette.color.value = Color.cyan;
            DOTween.To(() => 0.2f, x => vignette.intensity.value = x, 0.5f, 0.2f)
                .OnComplete(() =>
                {
                    DOTween.To(() => 0.5f, x => vignette.intensity.value = x, 0f, 0.5f);
                });
        }

        if (target != null)
        {
            player.FocusedEffectObject.SetActive(true);
            
            // 攻撃対象のNavMeshAgentを取得して、動きを停止
            var enemyAgent = target.GetComponent<NavMeshAgent>();
            if (enemyAgent != null)
            {
                enemyAgent.isStopped = true;
                Observable.Interval(TimeSpan.FromSeconds(4))
                    .Subscribe(_ => enemyAgent.isStopped = false)
                    .AddTo(target);
            }

            Vector3 enemyPos = target.transform.position;
            Vector3 moveTo = enemyPos + (player.transform.position - enemyPos).normalized * 1.5f;

            // 移動処理（敵に近づく）
            player.transform.DOMove(moveTo, 0.4f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    // 移動完了後に敵の方向に回転
                    Quaternion targetRotation = Quaternion.LookRotation(enemyPos - player.transform.position);
                    player.transform.DORotateQuaternion(targetRotation, 0.2f);
                });
        }


    }

    public void UpdateState(PlayerStateMachine player)
    {
       
    }

    public void ExitState(PlayerStateMachine player)
    {
        
    }
}
