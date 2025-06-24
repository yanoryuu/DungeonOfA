using UnityEngine;
using UnityEngine.UI;

public class EnemyUIBase : MonoBehaviour
{
    [SerializeField] private GameObject hpUIPrefab;

    private GameObject hpUIInstance;
    private Image hpFillImage;

    private Transform _target;
    private Camera _mainCamera;

    private float _maxHP;
    private float _currentHP;

    private float _lastVisibleTime;
    private float _visibleDuration = 20f;
    
    [SerializeField] private Transform worldCanvasParent;
    
    private float _hpBarHight;

    public void InitUI(Transform target, float maxHP,float HpBarHight)
    {
        _target = target;
        _mainCamera = Camera.main;
        _maxHP = maxHP;
        _currentHP = maxHP;
        
        _hpBarHight = HpBarHight;

        if (hpUIPrefab != null)
        {
            hpUIInstance = Instantiate(hpUIPrefab, _target.position + Vector3.up * 10, Quaternion.identity);
            hpUIInstance.transform.SetParent(worldCanvasParent, false);

            hpFillImage = hpUIInstance.GetComponent<EnemyHPFillHolder>()._fillimage;

            hpUIInstance.SetActive(false);
        }

        UpdateHP();
    }


    public void SetHP(float hp)
    {
        _currentHP = Mathf.Clamp(hp, 0, _maxHP);

        UpdateHP();

        if (_currentHP < _maxHP)
        {
            if (hpUIInstance != null && !hpUIInstance.activeSelf)
            {
                hpUIInstance.SetActive(true);
            }

            _lastVisibleTime = Time.time;
        }
        else
        {
            if (hpUIInstance != null && hpUIInstance.activeSelf)
            {
                hpUIInstance.SetActive(false);
            }
        }
    }

    private void UpdateHP()
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = _currentHP / _maxHP;
        }

        if (hpUIInstance != null && _target != null)
        {
            // 高さ調整（敵の上）
            Vector3 uiPos = _target.position + Vector3.up * _hpBarHight;
            hpUIInstance.transform.position = uiPos;

            // カメラの方向を向く
            Vector3 camForward = _mainCamera.transform.forward;
            hpUIInstance.transform.rotation = Quaternion.LookRotation(camForward);
        }
    }


    private void LateUpdate()
    {
        UpdateHP();

        if (hpUIInstance != null && hpUIInstance.activeSelf)
        {
            if (Time.time - _lastVisibleTime > _visibleDuration)
            {
                hpUIInstance.SetActive(false);
            }
        }
    }

    public void DestroyUI()
    {
        if (hpUIInstance != null)
        {
            Destroy(hpUIInstance);
        }
    }
}
