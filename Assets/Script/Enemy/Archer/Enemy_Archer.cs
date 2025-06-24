using System;
using R3;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Archer : MonoBehaviour, IEnemy
{
    [SerializeField] private Animator _animator;
// アーチャーのアニメーター
    public Animator Animator => _animator;

    [SerializeField] private EnemyStats _archerStats;
// アーチャーのステータス
    public EnemyStats EnemyStats => _archerStats;

    private float _currentHP; // 現在のHP

    private ReactiveProperty<bool> _isDead = new ReactiveProperty<bool>(false); 
// 死亡フラグ（リアクティブ）

    private ReactiveProperty<Transform> _targetPlayer;
// 現在検知しているプレイヤーのTransform
    public ReactiveProperty<Transform> PlayerTransform => _targetPlayer;

    private IDisposable _playerSearchRoutine; 
// プレイヤー探索用購読管理

    private IArcherState _currentState;
// 現在の行動状態
    public IArcherState CurrentState => _currentState;

    [SerializeField] private NavMeshAgent _agent;
// NavMeshAgent（移動処理）
    public NavMeshAgent NavMeshAgent => _agent;

    [SerializeField] private Transform[] _waypoints;
// パトロール地点（アーチャーの巡回ルート）
    public Transform[] PatrolPoint => _waypoints;

    [SerializeField] private Collider _bodyCollider;
// アーチャー本体のコライダー
    public Collider MonsterCollider => _bodyCollider;

    [SerializeField] private Rigidbody _rigidBody;
// 物理演算用Rigidbody
    public Rigidbody EnemyRigidbody => _rigidBody;

    private Transform _archerTransform;
// 自身のTransformキャッシュ
    public Transform SelfTransform => _archerTransform;

    private bool _isOnAttackCooldown;
// 攻撃クールダウン中かどうか
    public bool AttackCoolDown => _isOnAttackCooldown; 
    //弓の場所(発射位置）
    [SerializeField] private Transform _bowTransform;

    [SerializeField] private GameObject _arrowPrefab;
    
    [SerializeField] private Collider _punchCollider;
    
    [SerializeField] private GameObject _flashPrefab;

    [SerializeField] private float _enemyHpBarHight;

    private EnemyUIBase _enemyUI;
    private void Awake()
    {
        _currentHP = _archerStats.maxHP;
        _playerSearchRoutine = new CompositeDisposable();
        _targetPlayer = new ReactiveProperty<Transform>();
        SearchPlayer();
        _currentState = new ArcherPatrolState();
        _currentState.EnterState(this);
        _enemyUI = GetComponent<EnemyUIBase>();
        _enemyUI.InitUI(transform, _archerStats.maxHP,_enemyHpBarHight);
    }

    void Update()
    {
        
        _archerTransform = transform;
        
        _currentState.UpdateState(this);
        
        // グローバル速度（ワールド座標）
        Vector3 globalVelocity = _agent.velocity;

        // ローカル速度（敵の向き基準に変換）
        Vector3 localVelocity = transform.InverseTransformDirection(globalVelocity);

        float forward = localVelocity.z; // 前進(+)/後退(-)
        float strafe = localVelocity.x;  // 右(+)/左(-)

        // Animatorにセット（BlendTreeとリンクさせる）
        _animator.SetFloat("Vertical", forward);
        _animator.SetFloat("Holizontal", strafe);
    }

    public EnemyStats Damage(float damage)
    {
        _currentHP -= damage;

        Debug.Log($"{_archerStats.enemyName} が {damage} ダメージを受けた！（残りHP: {_currentHP}）");
        _enemyUI.SetHP(_currentHP);
        if (_currentHP <= 0)
        {
            Death();
            return _archerStats;
        }

        // 死んでおらず、すでにダメージ中ではない場合のみ状態変更
        if (!(_currentState is ArcherDamageState))
        {
            ChangeState(new ArcherDamageState());
        }
        return null;
    }

    public void ChangeState(IArcherState newState)
    {
        if(_isDead.Value)return;
        _currentState?.ExitState(this);
        _currentState = newState;
        _currentState.EnterState(this);
    }

    public void SearchPlayer()
    {
        
       _playerSearchRoutine = Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                //プレイヤーを発見
                 _targetPlayer.Value = EnemyCommon.FindVisibleTargets(_archerTransform, _archerStats.detectionRange,
                    _archerStats.detectionAngle, _archerStats.targetMask, _archerStats.obstacleMask);
                 Debug.Log(_targetPlayer.Value);
            })
            .AddTo(this);
    }
    
    public void Death()
    {
        Debug.Log($"{_archerStats.enemyName} は倒れた！");
        
        ChangeState(new ArcherDeathState());
        
        _isDead.Value = true;
        
        gameObject.layer = LayerMask.NameToLayer("Death");
        
        _bodyCollider.enabled = false;

        Observable.Interval(TimeSpan.FromSeconds(5))
            .Take(1)
            .Subscribe(_ => Destroy(gameObject))
            .AddTo(this);
        
        _rigidBody.linearVelocity = Vector3.zero;
        
        _agent.isStopped = true;
        
        _enemyUI.DestroyUI();
    }

    public void Attack(float damage)
    {
        SoundManager.Instance.PlaySE3D(SoundManager.Instance.archerAttackSE, transform.position);
        
        Debug.Log($"{_archerStats.enemyName} が攻撃した！");
        
        // 矢の生成と位置調整（まず位置を決める）
        GameObject Arrow = Instantiate(_arrowPrefab, _bowTransform.position + Vector3.up, Quaternion.identity);

        // 先にターゲットの方向を向かせる
        Arrow.transform.LookAt(_targetPlayer.Value.position + new Vector3(0,1,0));

        // ダメージ設定
        Arrow.GetComponent<ArcherArrow>().SetDamage(damage);

        // 進行方向へ力を加える（transform.forward は LookAt によって正しい方向）
        Arrow.GetComponent<Rigidbody>().AddForce(Arrow.transform.forward *100, ForceMode.Impulse);

        Destroy(Arrow,5f);
        
        _isOnAttackCooldown = true;
        
        Observable.Interval(TimeSpan.FromSeconds(_archerStats.attackCooldown))
            .Take(1)
            .Subscribe(_=>_isOnAttackCooldown=false)
            .AddTo(this);
    }
    
    public Transform AttackAreaChecker()
    {
        // Debug.Log("索敵中");
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position, 
            _archerStats.attackRange);
        
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
    
    public void OnFootstep()
    {
        SoundManager.Instance.PlaySE3D(SoundManager.Instance.archerfootstepSE, transform.position);
    }

    public Collider SearchNearPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position, 
            _archerStats.detectionRange);

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
        if (SearchNearPlayer() != null)
        {
            SoundManager.Instance.PlaySE2D(SoundManager.Instance.justRollingChanseSE);
            SearchNearPlayer().GetComponent<PlayerStateMachine>().JustRollingStartReception();
            Transform cam = Camera.main.transform;
            GameObject Flash = Instantiate(_flashPrefab,transform.position+new Vector3(0,1.5f,0),Quaternion.identity);
            Observable.EveryUpdate()
                .Subscribe(_ => Flash.transform.LookAt(cam))
                .AddTo(Flash);
        }
    }

    public void SendStopJustRolling()
    {
        if(SearchNearPlayer()!=null) SearchNearPlayer().GetComponent<PlayerStateMachine>().JustRollingStopReception();
    }
    
    public void ArcherChaseState()
    {
        // Debug.Log("Enemyの攻撃終了");
        ChangeState(new ArcherChaseState());
    }

    public ReactiveProperty<bool> _isDeadReturner()
    {
        return _isDead;
    }

    public void NearAttack(float damage)
    {
        Debug.Log($"{_archerStats.enemyName} が攻撃した！");
        
        // SwordColliderのバウンディングボックス内の全てのColliderを取得
        Collider[] hitColliders = Physics.OverlapBox(
            _punchCollider.bounds.center, 
            _punchCollider.bounds.extents, 
            _punchCollider.transform.rotation);
        
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerPresenter player = hit.GetComponent<PlayerPresenter>();
                player.Damage(damage);
            }
        }

        _isOnAttackCooldown = true;
        
        Observable.Interval(TimeSpan.FromSeconds(_archerStats.attackCooldown))
            .Take(1)
            .Subscribe(_=>_isOnAttackCooldown=false)
            .AddTo(this);
    }

}