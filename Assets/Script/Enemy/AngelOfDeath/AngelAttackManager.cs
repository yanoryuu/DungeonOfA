using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AngelAttackManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] swordEffects;
    [SerializeField] private Light swordVFXLight;
    [SerializeField] private Transform HandVfx;
    [SerializeField] private ParticleSystem[] hand_particles;
    [SerializeField] private Transform HeadVfx;
    [SerializeField] private ParticleSystem[] head_particles;
    [SerializeField] private Light handVFXLight;

    [SerializeField] private Transform _sword3;
    [SerializeField] private GameObject _magic1;
    [SerializeField] private GameObject _magic2;
    [SerializeField] private GameObject _magic3;
    
    [SerializeField] private float _sword3Damage;
    [SerializeField] private float _magic1Damage;
    [SerializeField] private float _magic2Damage;
    [SerializeField] private float _magic3Damage;

    private void Start()
    {
        hand_particles = HandVfx.GetComponentsInChildren<ParticleSystem>();
        head_particles = HeadVfx.GetComponentsInChildren<ParticleSystem>();
    }

    public void SwordAttack1()
    {
        EnableEffectsAsync(swordEffects,0).Forget();
        DisableEffectsAsync(swordEffects,1.3f).Forget();
        
        IncreaseLightIntensityAsync(swordVFXLight, 1f, 0.25f, 0.1f);
        IncreaseLightIntensityAsync(swordVFXLight, 0, 0.25f, 1.2f);
    }

    public void SwordAttack2()
    {
        EnableEffectsAsync(swordEffects,0).Forget();
        DisableEffectsAsync(swordEffects,1.3f).Forget();
        
        IncreaseLightIntensityAsync(swordVFXLight, 1f, 0.25f, 0.1f);
        IncreaseLightIntensityAsync(swordVFXLight, 0, 0.25f, 1.2f);
    }

    public void SwordAttack3()
    {
        
    }

    public async UniTask Spelling()
    {
        await UniTask.WhenAll(
            EnableEffectsAsync(hand_particles, 0.15f),
            DisableEffectsAsync(hand_particles, 1.05f)
        );
        
    }

    public async UniTask MagicAttack1(Enemy_Angel _enemy)
    {
        await Spelling();
        GameObject Magic =  Instantiate(_magic1, HandVfx.transform.position, Quaternion.identity);
        Magic.transform.LookAt(_enemy.PlayerTransform);
        Magic.GetComponentInChildren<MagicDamage>().SetDamage(_magic1Damage);
        Magic.transform.Rotate(-5,0,0);
        Destroy(Magic, 10);
    }

    public async UniTask MagicAttack2(Enemy_Angel _enemy)
    {
        await Spelling();
        GameObject Magic =  Instantiate(_magic2, transform.position, Quaternion.identity,transform);
        Magic.transform.localPosition = new Vector3(0, 3, 0);
        Magic.transform.LookAt(_enemy.PlayerTransform);
        Magic.transform.Rotate(10, 180f, 0);
        Magic.GetComponentInChildren<MagicDamage>().SetDamage(_magic2Damage);
        Destroy(Magic,10);
    }

    public void MagicAttack3(Enemy_Angel _enemy)
    {
        Debug.Log("MagicAttack3");
        GameObject Magic =  Instantiate(_magic3, HandVfx.transform.position, Quaternion.identity);
        Magic.transform.LookAt(_enemy.PlayerTransform);
        Magic.GetComponentInChildren<MagicDamage>().SetDamage(_magic3Damage);
        Destroy(Magic,10);
    }

    public async UniTask EnableEffectsAsync(ParticleSystem[] particles, float delay, CancellationToken token = default)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
        foreach (var ps in particles)
        {
            var emission = ps.emission;
            emission.enabled = true;
        }
    }

    public async UniTask DisableEffectsAsync(ParticleSystem[] particles, float delay, CancellationToken token = default)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
        foreach (var ps in particles)
        {
            var emission = ps.emission;
            emission.enabled = false;
        }
    }

    public async UniTask IncreaseLightIntensityAsync(Light light, float targetValue, float time, float delay, CancellationToken token = default)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

        float startIntensity = light.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            if (token.IsCancellationRequested) return;

            light.intensity = Mathf.Lerp(startIntensity, targetValue, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        light.intensity = targetValue;
    }
}
