using DG.Tweening;
using R3;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [SerializeField] private float defaultSEVolume = 1f;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;
    [SerializeField] private float bgmFadeTime = 1f;

    [Header("BGM Clips")]
    public AudioClip startBGM;
    public AudioClip mainBGM;
    public AudioClip bossBGM;
    
    [Header("SE Clips")]
    public AudioClip selectSE;

    [Header("Player Clips")]
    public AudioClip attackSE;
    public AudioClip damageSE;
    public AudioClip deathSE;
    public AudioClip justRollingChanseSE;
    public AudioClip justRollingSE;
    public AudioClip justRollingAttackMoveSE;
    public AudioClip justRollingAttackSE;
    public AudioClip KillSE;
    public AudioClip slidingSE;

    [Header("Monster Clips")] 
    public AudioClip monsterRoarSE;
    public AudioClip monsterDamagedSE;
    public AudioClip monsterAttackSE;
    public AudioClip monsterDeathSE;
    public AudioClip monsterfootstepSE;
    
    [Header("Archer Clips")]
    public AudioClip archerAttackSE;
    public AudioClip archerfootstepSE;
    
    [Header("Environment Sources")]
    public AudioClip footstepSE;
    // BGM音量設定
    public ReactiveProperty<float> BgmVolume = new ReactiveProperty<float>(1f);
    public ReactiveProperty<float> SeVolume = new ReactiveProperty<float>(1f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGMSource");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        BgmVolume.Subscribe(vol => bgmSource.volume = vol).AddTo(this);
    }

    // BGMを再生（フェードあり）
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (bgmSource.clip == clip) return;

        DOTween.Kill("BGMFade");

        if (bgmSource.isPlaying)
        {
            // フェードアウトしてから再生
            bgmSource.DOFade(0f, bgmFadeTime)
                .SetId("BGMFade")
                .OnComplete(() =>
                {
                    bgmSource.clip = clip;
                    bgmSource.loop = loop;
                    bgmSource.Play();
                    bgmSource.DOFade(BgmVolume.Value, bgmFadeTime).SetId("BGMFade");
                });
        }
        else
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = 0f;
            bgmSource.Play();
            bgmSource.DOFade(BgmVolume.Value, bgmFadeTime).SetId("BGMFade");
        }
    }

    // BGMを停止（フェード）
    public void StopBGM()
    {
        DOTween.Kill("BGMFade");
        bgmSource.DOFade(0f, bgmFadeTime)
            .SetId("BGMFade")
            .OnComplete(() => bgmSource.Stop());
    }

    // SEを3D空間で鳴らす（位置付き）
    public void PlaySE3D(AudioClip clip, Vector3 position, float volume = -1f)
    {
        if (clip == null) return;

        float vol = (volume < 0f) ? SeVolume.Value : volume;

        GameObject obj = new GameObject("SE3D_" + clip.name);
        obj.transform.position = position;

        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 1f;
        source.volume = vol;
        source.minDistance = 1f;
        source.maxDistance = 20f;
        source.rolloffMode = AudioRolloffMode.Linear;

        source.Play();
        Destroy(obj, clip.length + 0.5f);
    }

    // SEを2D（UI）として再生（オプション）
    public void PlaySE2D(AudioClip clip)
    {
        if (clip == null) return;

        GameObject obj = new GameObject("SE2D_" + clip.name);
        var source = obj.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = SeVolume.Value;
        source.spatialBlend = 0f;
        source.Play();
        Destroy(obj, clip.length + 0.5f);
    }
}