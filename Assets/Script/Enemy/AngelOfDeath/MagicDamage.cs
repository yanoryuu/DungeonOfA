using System;
using UnityEngine;

public class MagicDamage : MonoBehaviour
{
    private float _damage;
    public float Damage => _damage;

    public void SetDamage(float damage)
    {
        _damage = damage;
    }
    
    void Start()
    {
        var physicsMotion = GetComponentInChildren<RFX4_PhysicsMotion>(true);
        if (physicsMotion != null)
            physicsMotion.CollisionEnter += CollisionEnter;
    }

    private void CollisionEnter(object sender, RFX4_PhysicsMotion.RFX4_CollisionInfo e)
    {

        var presenter = e.HitCollider.GetComponent<PlayerPresenter>();
        if (presenter != null)
        {
            presenter.Damage(_damage);
        }
        
    }
}
