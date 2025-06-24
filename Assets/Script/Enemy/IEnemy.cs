using R3;
using UnityEngine;
using UnityEngine.AI;

public interface IEnemy
{
    EnemyStats Damage(float damage);
    
    ReactiveProperty<bool> _isDeadReturner();
}
