using System.Collections.Generic;
using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    [SerializeField] private DamagePopup popupPrefab;
    [SerializeField] private Transform worldCanvas;
    [SerializeField] private int initialPoolSize = 20;

    private Queue<DamagePopup> pool = new Queue<DamagePopup>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            var popup = Instantiate(popupPrefab, worldCanvas);
            popup.gameObject.SetActive(false);
            pool.Enqueue(popup);
        }
    }

    private DamagePopup GetFromPool()
    {
        if (pool.Count > 0)
        {
            var popup = pool.Dequeue();
            popup.gameObject.SetActive(true);
            return popup;
        }

        var newPopup = Instantiate(popupPrefab, worldCanvas);
        return newPopup;
    }

    public void ShowDamage(Vector3 worldPosition, int damage, bool isCritical = false)
    {
        DamagePopup popup = GetFromPool();

        // ShowDamage() での位置設定
        Vector3 adjustedWorldPos = worldPosition + Vector3.up * 2f + Camera.main.transform.forward * 0.05f;
        Vector3 localCanvasPos = worldCanvas.InverseTransformPoint(adjustedWorldPos);


        popup.transform.SetParent(worldCanvas, false);
        popup.transform.localPosition = localCanvasPos;

        popup.Show(damage, isCritical);
    }

    public void ReturnToPool(DamagePopup popup)
    {
        popup.gameObject.SetActive(false);
        pool.Enqueue(popup);
    }
}