using TMPro;
using UnityEngine;
using DG.Tweening;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    private CanvasGroup canvasGroup;
    private Tween moveTween;
    private Tween fadeTween;
    private Camera _mainCamera;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show(int damage, bool isCritical = false)
    {
        damageText.text = isCritical ? $"<b><color=yellow>CRITICAL!\n{damage}</color></b>" : damage.ToString();
        damageText.color = isCritical ? Color.yellow : Color.red;
        canvasGroup.alpha = 1;
        transform.localScale = Vector3.one;

        moveTween?.Kill();
        fadeTween?.Kill();

        moveTween = transform.DOLocalMoveY(transform.localPosition.y + 50f, 0.6f).SetEase(Ease.OutQuad);
        fadeTween = canvasGroup.DOFade(0, 0.6f).OnComplete(() =>
        {
            DamagePopupManager.Instance.ReturnToPool(this);
        });
    }

    private void OnEnable()
    {
        _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_mainCamera != null)
        {
            transform.LookAt(transform.position + _mainCamera.transform.forward,
                _mainCamera.transform.up);
        }
    }
}