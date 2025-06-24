using System;
using UnityEngine;

public class BossGate : MonoBehaviour
{
    [SerializeField] private Enemy_Angel _enemyAngel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _enemyAngel.StartBattle();
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlayBGM(SoundManager.Instance.bossBGM);
        }
    }
}
