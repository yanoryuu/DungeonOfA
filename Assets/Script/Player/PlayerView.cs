using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI; // ← PostProcessing用

public class PlayerView : MonoBehaviour
{
    // 既存の変数...
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _expBar;
    [SerializeField] private TextMeshProUGUI _playerLevelText;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _expText;
    [SerializeField] private TextMeshProUGUI _attackPowerText;

    // PostProcess用変数
    private Vignette _vignette;
    private PostProcessVolume _volume;

    private void Start()
    {
        _volume = Camera.main.GetComponent<PostProcessVolume>();
        if (_volume.profile.TryGetSettings(out Vignette vignette))
        {
            _vignette = vignette;
        }
    }

    public void SetPlayerHealth(float health, float maxHealth)
    {
        _healthBar.fillAmount = health / maxHealth;
        _healthBar.color = Color.HSVToRGB(120 * health / maxHealth / 360, 1, 1);
        _healthText.text = $"Health:{health}/{maxHealth}";
    }

    public void SetPlayerExp(float exp, float maxExp)
    {
        _expBar.fillAmount = exp / maxExp;
        _expText.text = $"Exp:{exp}/{maxExp}";
    }

    public void SetAttackPower(float attackPower)
    {
        _attackPowerText.text = $"Atk:{attackPower}";
    }

    public void SetPlayerLevel(float level)
    {
        _playerLevelText.text = "Lv." + level;
    }

    public void Initialize()
    {
        _playerLevelText.text = "Lv.1";
    }

    public void ShakeHPBar(float duration)
    {
        _healthBar.transform.DOShakePosition(duration, 25, 30, 90, false, true);
    }
    
    private Tweener _tweener;

    /// <summary>
    /// 🔴 被ダメージ時に画面を一瞬赤くする
    /// </summary>
    public void FlashRedEffect(float duration = 0.1f)
    {
        if (_vignette == null) return;
        
        _tweener.Kill();
        
        _vignette.color.value = Color.red;

        // 一瞬赤くして戻す
        _tweener = DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x, 0.2f, 0.1f)
            .OnComplete(() =>
            {
                DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x, 0f, duration);
            });
    }
}
