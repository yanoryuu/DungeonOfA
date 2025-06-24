using System;
using Unity.VisualScripting;
using UnityEngine;

public class BigMagicDamage : MonoBehaviour
{
    [SerializeField] private float damage;
    
    [SerializeField] private Collider damageCollider;


    public void AttackGolem()
    {
        Collider[] hitColliders = Physics.OverlapBox(
            damageCollider.bounds.center, 
            damageCollider.bounds.extents, 
            damageCollider.transform.rotation);

        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerPresenter>().Damage(damage);
            }
        }
    }
}
