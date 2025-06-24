using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Enemy_Angel : MonoBehaviour,IEnemy
{
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;
    
    [SerializeField] private EnemyStats _enemyStats;
    public EnemyStats EnemyStats => _enemyStats;

    private ReactiveProperty<float> _currentHP;
    
    private ReactiveProperty<bool> _isDead = new ReactiveProperty<bool>(false);
    
    [SerializeField] private Transform _playerTransform;
    public Transform PlayerTransform => _playerTransform;
    
    private IDisposable _searchDisposable;
    
    private IAngelState _currentState;
    public IAngelState CurrentState => _currentState;

    [SerializeField] private NavMeshAgent _navMeshAgent;
    public NavMeshAgent NavMeshAgent => _navMeshAgent;

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
    
    public bool IsSeceondMode { get;private set; }
    
    private AttackSelector _attackSelector;
    
    [SerializeField] private bool _isFighting;
    public bool IsFighting => _isFighting;
    
    [SerializeField] private Animator _wingsAnimator;
    
    private Vector3 _lastVelocity;

    [SerializeField] private AngelAttackManager _angelAttackManager;
    
    public AngelAttackManager AngelAttackManager => _angelAttackManager;

    [SerializeField] private GameObject _flashPrefab;

    [SerializeField] private GameObject _angelCanvas;
    public GameObject AngelCanvas => _angelCanvas;
    [SerializeField] private Image _hpImage;
    
    private void Awake()
    {
        _attackSelector = new AttackSelector(3, _enemyStats.attackRange); // 3回まで同じ攻撃OK、5mが近距離扱い
        _currentHP = new ReactiveProperty<float>(0);
        _currentHP.Value = _enemyStats.maxHP;
        _searchDisposable = new CompositeDisposable();
        _currentState = new IdleState_Angel();
        _currentState.EnterState(this);
        _lastVelocity = _enemyRigidbody.linearVelocity;
        Bind();
    }

    public void StartBattle()
    {
        _isFighting = true;
        _angelCanvas.SetActive(true);
    }

    private void Bind()
    {
        _currentHP.Subscribe(x =>
            {
                _hpImage.fillAmount = x/_enemyStats.maxHP;
            })
            .AddTo(this);
    }

    void Update()
    {
        _selfTransform = transform;
        _currentState.UpdateState(this);
        
        Vector3 currentVelocity = _enemyRigidbody.linearVelocity;
        Vector3 acceleration = (currentVelocity - _lastVelocity) / Time.fixedDeltaTime;
        _lastVelocity = currentVelocity;

        // 水平方向（XとZ）の加速度の大きさ
        float horizontalAccel = new Vector2(acceleration.x, acceleration.z).magnitude;
        float verticalAccel = new Vector2(acceleration.y, acceleration.z).magnitude;
        _wingsAnimator.SetFloat("Speed", verticalAccel);
        _wingsAnimator.SetFloat("SpeedX", horizontalAccel);
    }
    public EnemyStats Damage(float damage)
    {
        _currentHP.Value -= damage;
        Debug.Log($"{_enemyStats.enemyName} が {damage} ダメージを受けた！（残りHP: {_currentHP}）");

        if (_currentHP.Value <= 0)
        {
            Death();
            return _enemyStats;
        }

        // // フェーズ2に移行
        // if (!IsSeceondMode && _currentHP <= _enemyStats.maxHP * 0.5f)
        // {
        //     EnterSecondPhase();
        //     return null;
        // }

        return null;
    }

    // private void EnterSecondPhase()
    // {
    //     IsSeceondMode = true;
    //     Debug.Log("第2形態に突入！");
    //
    //     // 例：エフェクトやアニメ再生
    //     _animator.SetTrigger("Phase2");
    //
    //     // 状態をリセット or 特定の状態に遷移
    //     ChangeState(new IdleState_Angel()); // ←なければIdleでもOK
    // }
    
    public void ChangeState(IAngelState newState)
    {
        if(_isDead.Value)return;
        _currentState?.ExitState(this);
        _currentState = newState;
        _currentState.EnterState(this);
    }

    public void Attack(float damage)
    {
        Debug.Log($"{_enemyStats.enemyName} が攻撃した！");
        
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
    
    public void Death()
    {
        gameObject.layer = LayerMask.NameToLayer("Death");
         
        AnimatorCommon.SetAction("Death",_animator);
        
        Debug.Log($"{_enemyStats.enemyName} は倒れた！");
        
        ChangeState(new DeathState_Angel());
        
        _isDead.Value = true;
        
        _monsterCollider.enabled = false;
        
        _enemyRigidbody.linearVelocity = Vector3.zero;
        
        _angelCanvas.SetActive(false);
        
        Observable.Interval(TimeSpan.FromSeconds(5))
            .Take(1)
            .Subscribe(_ =>
            {
                SceneManager.LoadScene("GameClearScene");
            });
    }

    public void SwordAttack(float damage)
    {
        Debug.Log($"{_enemyStats.enemyName} が攻撃した！");
        
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
    
    public Transform NearRangeAreaChecker()
    {
        float detectionRange = _enemyStats.attackRange;
        // Debug.Log("索敵中");
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            detectionRange);
        
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
    
    public Transform LongRangeAreaChecker()
    {
        float detectionRange = _enemyStats.detectionRange;

        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            detectionRange);

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                return hit.transform;
            }
        }

        return null;
    }

    public Collider SearchNearPlayer()
    {
        float detectionRange = _enemyStats.detectionRange;
        
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            detectionRange);

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                return hit;
            }
        }
        return null;
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

    public ReactiveProperty<bool> _isDeadReturner()
    {
        return _isDead;
    }
    
    public AttackType SelectAttackType()
    {
        float distance = Vector3.Distance(transform.position, PlayerTransform.position);
        return _attackSelector.SelectAttackType(distance);
    }

    //アニメーションクリップから呼び出し
    public void BackChaseState()
    {
        ChangeState(new ChaseState_Angel());
    }
    
    private void OnDrawGizmosSelected()
    {
        if (_enemyStats == null) return;

        // 近接攻撃用（例：赤色）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _enemyStats.attackRange);

        // 遠距離攻撃用（例：青色）
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _enemyStats.detectionRange);
    }
}
