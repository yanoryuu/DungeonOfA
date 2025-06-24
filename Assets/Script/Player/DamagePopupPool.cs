using System.Collections.Generic;
using UnityEngine;

public class DamagePopupPool : MonoBehaviour
{
    public static DamagePopupPool Instance;

    [SerializeField] private DamagePopup popupPrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private Canvas worldCanvas;

    private Queue<DamagePopup> pool = new Queue<DamagePopup>();

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < initialPoolSize; i++)
        {
            var popup = Instantiate(popupPrefab, worldCanvas.transform);
            popup.gameObject.SetActive(false);
            pool.Enqueue(popup);
        }
    }

    public DamagePopup Get()
    {
        if (pool.Count > 0)
        {
            var popup = pool.Dequeue();
            popup.gameObject.SetActive(true);
            return popup;
        }
        else
        {
            var popup = Instantiate(popupPrefab, worldCanvas.transform);
            return popup;
        }
    }

    public void Return(DamagePopup popup)
    {
        popup.gameObject.SetActive(false);
        pool.Enqueue(popup);
    }
}