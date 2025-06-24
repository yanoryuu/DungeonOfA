using System;
using UnityEngine;

public class ArcherArrow : MonoBehaviour
{
    private float _damage;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerPresenter>().Damage(_damage);
            Destroy(gameObject);
        }
    }

    public void SetDamage(float damage)
    {
        _damage = damage;
    }
}
