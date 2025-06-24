using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using DG.Tweening;
using R3;

public class JustRollingState : IPlayerState
{
    private ChromaticAberration chromaticAberration;
    private ColorGrading colorGrading;
    private MotionBlur motionBlur; // ← Motion Blur 追加
    private IDisposable _disposable;

    public void EnterState(PlayerStateMachine player)
    {
        player.SetAction("Rolling");
        player.Rolling();
        player._enemyManager.SlowEnemies(1);

        var volume = Camera.main.GetComponent<PostProcessVolume>();
        
        SoundManager.Instance.PlaySE2D(SoundManager.Instance.justRollingSE);

        // === 色温度の変更 ===
        if (volume.profile.TryGetSettings(out colorGrading))
        {
            float start = colorGrading.temperature.value;
            DOTween.To(() => start, x =>
            {
                colorGrading.temperature.value = x;
            }, -50f, 0.2f).OnComplete(() =>
            {
                DOTween.To(() => -50f, x =>
                {
                    colorGrading.temperature.value = x;
                }, 0f, 1f);
            });
        }

        // === 色収差の演出 ===
        if (volume.profile.TryGetSettings(out chromaticAberration))
        {
            float originalValue = chromaticAberration.intensity.value;

            DOTween.To(() => 0f, x =>
            {
                chromaticAberration.intensity.value = x;
            }, 1f, 0.15f).OnComplete(() =>
            {
                DOTween.To(() => 1f, x =>
                {
                    chromaticAberration.intensity.value = x;
                }, originalValue, 1f);
            });
        }

        // === モーションブラーの演出 ===
        if (volume.profile.TryGetSettings(out motionBlur))
        {
            float originalShutter = motionBlur.shutterAngle.value;

            motionBlur.enabled.value = true;
            DOTween.To(() => 0f, x =>
            {
                motionBlur.shutterAngle.value = x;
            }, 270f, 0.15f).OnComplete(() =>
            {
                DOTween.To(() => 270f, x =>
                {
                    motionBlur.shutterAngle.value = x;
                }, originalShutter, 1f);
            });
        }

        // // === 回避時パーティクル ===
        // if (player.EcadeEffectPrefab != null)
        // {
        //     player.EcadeEffectPrefab.SetActive(true);
        //     _disposable = Observable.Interval(TimeSpan.FromSeconds(1))
        //         .Take(1)
        //         .Subscribe(_ => player.EcadeEffectPrefab.SetActive(false));
        // }
    }

    public void UpdateState(PlayerStateMachine player)
    {
        if (player.AttackAction&&player.PlayerLockon.TargetObj!=null)
        {
            player.ChangeState(new SpecialAttackState());
        }
    }

    public void ExitState(PlayerStateMachine player)
    {
        // 任意でリセット処理を追加
    }
}