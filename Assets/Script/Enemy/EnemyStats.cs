using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "ScriptableObjects/EnemyStats", order = 1)]
public class EnemyStats : ScriptableObject
{
    [Header("基本ステータス")]
    public string enemyName = "Enemy";
    public int maxHP = 100;
    public int attackPower = 10;
    public float moveSpeed = 2.0f;
    public float RunSpeed = 3.0f;
    public float RotateSpeed = 270.0f;

    [Header("AI・行動設定")]
    public float detectionRange = 10.0f;
    public float detectionAngle = 20.0f;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.5f;
    public float magicAttackBackRange = 10.0f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public float spacing = 2;

    [Header("特殊効果・その他")]
    public bool isBoss = false;
    public EnemyType enemyType;
    public float enemyExp;

    public enum EnemyType
    {
        Normal,
        Ranged,
        Tank,
        Stealth,
        Exploder
    }
}
