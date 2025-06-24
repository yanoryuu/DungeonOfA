using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FlashEffect : MonoBehaviour
{
    public Material flashMaterial;
    public float duration = 0.8f;     // アニメーション全体の長さ
    public float fadeDelay = 0.2f;    // 発光が終わってからのフェード遅延
    public float fadeSpeed = 2f;

    private float fade = 1f;

    void Start()
    {
        flashMaterial = GetComponent<MeshRenderer>().material;
        
        // 初期値設定
        flashMaterial.SetFloat("_Fade", 1f);
        flashMaterial.SetFloat("_Intensity", 0f);
        flashMaterial.SetFloat("_Power", 1800);

        // DOTweenアニメーション開始
        Sequence seq = DOTween.Sequence();

        seq.Append(flashMaterial
                .DOFloat(20f, "_Intensity", duration * 0.6f)
                .SetEase(Ease.OutQuad)) // 明るくなる
            .Join(flashMaterial
                .DOFloat(100f, "_Power", duration * 0.6f)
                .SetEase(Ease.OutQuad)) // シャープになる
            .AppendInterval(fadeDelay)
            .Append(flashMaterial
                .DOFloat(0f, "_Fade", duration * 0.4f)
                .SetEase(Ease.InQuad)) // 透明に
            .OnComplete(() => Destroy(gameObject));
    }
}