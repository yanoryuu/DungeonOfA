using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class HitStopUtility
{
    public static async void HitStop(float duration = 0.05f)
    {
        Time.timeScale = 0f;
        await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale: true);
        Time.timeScale = 1f;
    }
}
