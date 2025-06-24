using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Monster :MonoBehaviour, IEnemy
{
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;
    
    [SerializeField] private EnemyStats _enemyStats;
    public EnemyStats EnemyStats => _enemyStats;

    private float _currentHP;
    
    private ReactiveProperty<bool> _isDead = new ReactiveProperty<bool>(false);
    
    private ReactiveProperty<Transform> _playerTransform;
    public ReactiveProperty<Transform> PlayerTransform => _playerTransform;
    
    private IDisposable _searchDisposable;
    
    private IMonsterState _currentState;
    public IMonsterState CurrentState => _currentState;
    
    [SerializeField] private NavMeshAgent _navMeshAgent;
    public NavMeshAgent NavMeshAgent => _navMeshAgent;
    
    [SerializeField] private Transform[] _patrolPoint;
    public Transform[] PatrolPoint => _patrolPoint;

    [SerializeField] private Collider _monsterAttackArea;
    public Collider MonsterAttackArea => _monsterAttackArea;

    [SerializeField] private Collider _monsterCollider;
    public Collider MonsterCollider => _monsterCollider;
    
    [SerializeField] private Rigidbody _enemyRigidbody;
    public Rigidbody EnemyRigidbody => _enemyRigidbody;
    
    private Transform _selfTransform;
    public Transform SelfTransform => _selfTransform;
    
    private bool _attackCoolDown;
    public bool AttackCoolDown => _attackCoolDown;
    
    [SerializeField] private GameObject _flashPrefab;

    private EnemyUIBase _enemyUI;
    
    [SerializeField] private float _enemyHpBarHight = 5;
    
    private void Start()
    {
        _currentHP = _enemyStats.maxHP;
        _searchDisposable = new CompositeDisposable();
        _playerTransform = new ReactiveProperty<Transform>();
        SearchPlayer();
        _currentState = new MonsterPatrolState();
        _currentState.EnterState(this);
        _enemyUI = GetComponent<EnemyUIBase>();
        _enemyUI.InitUI(transform, _enemyStats.maxHP,_enemyHpBarHight);
    }

    void Update()
    {
        
        _selfTransform = transform;
        _currentState.UpdateState(this);
    }
    public EnemyStats Damage(float damage)
    {
        _currentHP -= damage;
        
        DamagePopupManager.Instance.ShowDamage(transform.position,(int)damage,false);
        
        SoundManager.Instance.PlaySE3D(SoundManager.Instance.monsterDamagedSE, transform.position);

        Debug.Log($"{_enemyStats.enemyName} が {damage} ダメージを受けた！（残りHP: {_currentHP}）");
        
        _enemyUI.SetHP(_currentHP);

        if (_currentHP <= 0)
        {
            Death();
            return _enemyStats;
        }

        // 死んでおらず、すでにダメージ中ではない場合のみ状態変更
        if (!(_currentState is MonsterDamageState))
        {
            ChangeState(new MonsterDamageState());
        }
        return null;
    }


    public void ChangeState(IMonsterState newState)
    {
        if(_isDead.Value)return;
        _currentState?.ExitState(this);
        _currentState = newState;
        _currentState.EnterState(this);
    }

    public void SearchPlayer()
    {
        
       _searchDisposable = Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                //プレイヤーを発見
                 _playerTransform.Value = EnemyCommon.FindVisibleTargets(_selfTransform, _enemyStats.detectionRange,
                    _enemyStats.detectionAngle, _enemyStats.targetMask, _enemyStats.obstacleMask);
            })
            .AddTo(this);
    }
    
    public void Death()
    {
        Debug.Log($"{_enemyStats.enemyName} は倒れた！");
        
        ChangeState(new MonsterDeathState());
        
        SoundManager.Instance.PlaySE3D(SoundManager.Instance.monsterDeathSE, transform.position);
        
        _isDead.Value = true;
        
        _monsterCollider.enabled = false;
        
        gameObject.layer = LayerMask.NameToLayer("Death");

        Observable.Interval(TimeSpan.FromSeconds(5))
            .Take(1)
            .Subscribe(_ => Destroy(gameObject));
        
        _enemyRigidbody.linearVelocity = Vector3.zero;
        
        _navMeshAgent.isStopped = true;
        
        _enemyUI.DestroyUI();
    }

    public void Attack(float damage)
    {
        Debug.Log($"{_enemyStats.enemyName} が攻撃した！");
        
        SoundManager.Instance.PlaySE3D(SoundManager.Instance.monsterAttackSE, transform.position);
        
        // SwordColliderのバウンディングボックス内の全てのColliderを取得
        Collider[] hitColliders = Physics.OverlapBox(
            _monsterAttackArea.bounds.center, 
            _monsterAttackArea.bounds.extents, 
            _monsterAttackArea.transform.rotation);
        
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerPresenter player = hit.GetComponent<PlayerPresenter>();
                player.Damage(damage);
            }
        }

        _attackCoolDown = true;
        
        Observable.Interval(TimeSpan.FromSeconds(_enemyStats.attackCooldown))
            .Take(1)
            .Subscribe(_=>_attackCoolDown=false)
            .AddTo(this);
    }
    
    public Transform AttackAreaChecker()
    {
        // Debug.Log("索敵中");
        Collider[] hitColliders = Physics.OverlapBox(
            _monsterAttackArea.bounds.center, 
            _monsterAttackArea.bounds.extents, 
            _monsterAttackArea.transform.rotation);
        
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                // Debug.Log("攻撃！");
                return hit.transform;
            }
        }
        return null;
    }

    public Collider SearchNearPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position, 
            _enemyStats.detectionRange);

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Playerを発見");
                return hit;
            }
        }
        return null;
    }

    public void OnFootstep()
    {
        SoundManager.Instance.PlaySE3D(SoundManager.Instance.monsterfootstepSE, transform.position);
    }
    
    public void SendStartJustRolling()
    {
        SoundManager.Instance.PlaySE2D(SoundManager.Instance.justRollingChanseSE);
        SearchNearPlayer().GetComponent<PlayerStateMachine>().JustRollingStartReception();
        Transform cam = Camera.main.transform;
        GameObject Flash = Instantiate(_flashPrefab,transform.position+new Vector3(0,1.5f,0),Quaternion.identity);
        Observable.EveryUpdate()
            .Subscribe(_ => Flash.transform.LookAt(cam))
            .AddTo(Flash);
    }

    public void SendStopJustRolling()
    {
        SearchNearPlayer()?.GetComponent<PlayerStateMachine>().JustRollingStopReception();
    }
    
    public void MonsterChaseState()
    {
        // Debug.Log("Enemyの攻撃終了");
        ChangeState(new MonsterChaseState());
    }

    public ReactiveProperty<bool> _isDeadReturner()
    {
        return _isDead;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (_enemyStats == null) return;
        // 遠距離攻撃用（例：青色）
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _enemyStats.detectionRange);
    }
}