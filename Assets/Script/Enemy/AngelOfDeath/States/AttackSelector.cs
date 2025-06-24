using UnityEngine;

public enum AttackType { None, Sword, Magic }

public class AttackSelector
{
    private AttackType _lastAttackType = AttackType.None;
    private int _sameTypeCount = 0;
    private int _maxSameTypeCount;

    private float _switchDistance; // 魔法と剣の切り替え距離

    public AttackSelector(int maxSameTypeCount = 3, float switchDistance = 5f)
    {
        _maxSameTypeCount = maxSameTypeCount;
        _switchDistance = switchDistance;
    }

    public AttackType SelectAttackType(float distanceToPlayer)
    {
        AttackType chosen = (distanceToPlayer < _switchDistance) ? AttackType.Sword : AttackType.Magic;

        if (chosen == _lastAttackType)
        {
            _sameTypeCount++;
            Debug.Log(_sameTypeCount);
            if (_sameTypeCount >= _maxSameTypeCount)
            {
                chosen = (chosen == AttackType.Sword) ? AttackType.Magic : AttackType.Sword;
                _sameTypeCount = 0;
            }
        }
        else
        {
            _sameTypeCount = 0;
        }

        _lastAttackType = chosen;
        return chosen;
    }

    public void Reset()
    {
        _lastAttackType = AttackType.None;
        _sameTypeCount = 0;
    }
}