using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using TMPro;

public class PlayerStateMachine : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    
    [SerializeField] private PlayerLockon _playerLockon;
    public PlayerLockon PlayerLockon => _playerLockon;
    
    private IPlayerState _currentState;
    public IPlayerState CurrentState => _currentState;
    
    [SerializeField]private Animator _animator;
    
    //地面についているかのフラグ
    [SerializeField] private bool _onGround;
    public bool OnGround => _onGround;
    
    public ReactiveProperty<float> _blend; // デフォルト値
    private float _blendSpeed = 3f; // Blend の変化速度
    private float _runMultiplier = 1.5f; // 走るときの速度倍率

    private ReactiveProperty<Vector2> _LockOnMoveDirection;
    
    [SerializeField] private Transform cameraTransform; // カメラのTransformを取得
    
    //プレイヤーの高さ
    private ReactiveProperty<float> _height;
    public ReactiveProperty<float> Height => _height;
    
    //プレイヤーの高さを図るためにRayの出度どころ
    [SerializeField]private Transform _groundCheck;
    
    //プレイヤーの加速度のためのRididBody
    [SerializeField]private Rigidbody _rigidbody;
    public Rigidbody Rigidbody => _rigidbody;

    //敵対判定
    [SerializeField] private bool _lockOn;
    public bool LockOn => _lockOn;
    
    //無敵状態のフラグ
    private ReactiveProperty<bool> _isInvincible;
    
    public ReactiveProperty<bool> IsInvincible => _isInvincible;

    public EnemyManager _enemyManager;
    
    [SerializeField] private GameObject _ecadeEffectPrefab;
    
    public GameObject EcadeEffectPrefab => _ecadeEffectPrefab;
    
    // 追加（段差登り用）
    [SerializeField] private float stepHeight = 0.3f;
    [SerializeField] private float stepDetectionDistance = 0.5f;
    [SerializeField] private float stepSmoothSpeed = 4f;
    [SerializeField] private LayerMask groundLayer;
    
    [SerializeField] private GameObject _focusedEffectObject;
    public GameObject FocusedEffectObject => _focusedEffectObject;

    private void Start()
    {
        ChangeState(new IdleState());
        
        _height = new ReactiveProperty<float>();
        
        _blend = new ReactiveProperty<float>(0);
        
        _LockOnMoveDirection = new ReactiveProperty<Vector2>();
        
        _isInvincible = new ReactiveProperty<bool>(false);
        
        Bind();
    }

    public void Bind()
    {
        _blend.Subscribe(x=>_animator.SetFloat("Blend", x))
            .AddTo(this);
        
        _LockOnMoveDirection.Subscribe(dir =>
            {
                _animator.SetFloat("MoveX",dir.x);
                _animator.SetFloat("MoveY",dir.y);
            })
            .AddTo(this);
    }

    public void manualUpdate()
    {
        _animator.speed = Time.timeScale;
        
        CheckHight();
        
        StepClimb();
        
        if (_height.Value < 0.3f)
        {
            _onGround = true;
        }
        else
        {
            _onGround = false;
        }
        _currentState?.UpdateState(this);
    }

    public void ChangeState(IPlayerState newState)
    {
        _currentState?.ExitState(this);
        _currentState = newState;
        _currentState.EnterState(this);
    }

    public void SetAnimation(string animationName)
    {
        AnimatorCommon.SetAnimation(animationName, _animator);
    }

    public void OutAnimation(string animationName)
    {
        AnimatorCommon.OutAnimation(animationName, _animator);
    }
    
    public void SetAction(string actionName)
    {
        AnimatorCommon.SetAction(actionName, _animator);
    }

    public void ResetAction(string actionName)
    {
        AnimatorCommon.ResetAction(actionName, _animator);
    }

    //通常のプレイヤーのうごき
    public void Move()
    {
        if (cameraTransform == null || _rigidbody == null) return;

        Vector2 moveInput = _moveInputDirection; 

        // _runAction の状態に応じて Blend を徐々に変化させる
        float targetBlend = _runAction ? 1f : 0.5f;
        _blend.Value = Mathf.Lerp(_blend.Value, targetBlend, Time.unscaledDeltaTime * _blendSpeed);

        // カメラの前方向と右方向を基準に移動方向を計算
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // Y軸方向の回転だけを考慮（カメラの傾きを無視）
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // カメラ基準で移動方向を決定
        Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;

        if (moveDirection.magnitude > 0.01f) // 入力があるとき
        {
            // 走る場合の速度補正
            float speedMultiplier = _runAction ? _runMultiplier : 1f;
            // Blend と speedMultiplier を適用した移動速度（PlayerConst.PlayerSpeed は基準速度）
            float speed = PlayerConst.PlayerSpeed * _blend.Value * speedMultiplier;

            // 向きをスムーズに変更（プレイヤーを移動方向に回転）
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.unscaledDeltaTime * 10f);

            // Rigidbody の velocity を使って移動させる
            // 重力などの影響を維持するため、Y軸の速度はそのままにする
            Vector3 horizontalVelocity = moveDirection.normalized * speed;
            horizontalVelocity.y = _rigidbody.linearVelocity.y;
            _rigidbody.linearVelocity = horizontalVelocity;
        }
    }
    
    // 段差を登る処理（Move() の後で呼ぶ）
    private void StepClimb()
    {
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit forwardHit, stepDetectionDistance, groundLayer))
        {
            Vector3 upperOrigin = transform.position + Vector3.up * stepHeight;

            if (!Physics.Raycast(upperOrigin, direction, stepDetectionDistance, groundLayer))
            {
                // 現在の位置
                Vector3 currentPos = _rigidbody.position;
                // 新しい位置（Yだけ持ち上げる）
                Vector3 targetPos = new Vector3(currentPos.x, currentPos.y + stepHeight, currentPos.z);
                // 滑らかに補間
                _rigidbody.position = Vector3.Lerp(currentPos, targetPos, Time.fixedDeltaTime * stepSmoothSpeed);
            }
        }
    }
    
    //ロックオン時のプレイヤーの動き
    public void LockOnMove()
    {
        if (cameraTransform == null || _rigidbody == null) return;

        Vector2 moveInput = _moveInputDirection;
        
        // _runAction の状態に応じて Blend を徐々に変化させる
        float targetBlend = _runAction ? 1f : 0.5f;
        _blend.Value = Mathf.Lerp(_blend.Value, targetBlend, Time.unscaledDeltaTime * _blendSpeed);
        
        _LockOnMoveDirection.Value = _moveInputDirection;
        // カメラの前方向と右方向を基準に移動方向を計算
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // Y軸方向の回転だけを考慮（カメラの傾きを無視）
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // カメラ基準で移動方向を決定
        Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;

        if (moveDirection.magnitude > 0.01f) // 入力があるとき
        {
            // 走る場合の速度補正
            float speedMultiplier = _runAction ? _runMultiplier : 1f;
            // Blend と speedMultiplier を適用した移動速度（PlayerConst.PlayerSpeed は基準速度）
            float speed = PlayerConst.PlayerLockOnSpeed * _blend.Value * speedMultiplier;

            // 向きをスムーズに変更（プレイヤーを移動方向に回転）
            gameObject.transform.LookAt(_playerLockon.TargetObj.transform.position, Vector3.up);
            gameObject.transform.rotation= Quaternion.Euler(new Vector3(0,gameObject.transform.rotation.eulerAngles.y,gameObject.transform.rotation.eulerAngles.z));

            // Rigidbody の velocity を使って移動させる
            // 重力などの影響を維持するため、Y軸の速度はそのままにする
            Vector3 horizontalVelocity = moveDirection.normalized * speed;
            horizontalVelocity.y = _rigidbody.linearVelocity.y;
            _rigidbody.linearVelocity = horizontalVelocity;
        }
    }
    
    //プレイヤーの高さを計測
    public void CheckHight()
    {
        // プレイヤーの位置から下方向に Raycast を飛ばす
        if (Physics.Raycast(_groundCheck.transform.position, Vector3.down, out RaycastHit hit))
        {
            if(text!=null)text.text = _height.ToString();
            _height.Value = hit.distance;
        }
    }
    
    public bool IsIdle()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Idle");
    }

    //ジャンプ
    public void Jump()
    {
        _onGround = false;
        GetComponent<Rigidbody>().linearVelocity = PlayerConst.jumpForce+_rigidbody.linearVelocity;
    }
    //ローリング
    public void Rolling()
    {
        if (cameraTransform == null || _rigidbody == null) return;

        Vector2 moveInput = _moveInputDirection;

        // カメラの前方向と右方向を基準に移動方向を計算
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // Y軸方向の回転だけを考慮（カメラの傾きを無視）
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // カメラ基準で移動方向を決定
        Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;

        if (moveDirection.magnitude > 0.01f) // 入力があるとき
        {
            // Blend と speedMultiplier を適用した移動速度（PlayerConst.PlayerSpeed は基準速度）
            float speed = PlayerConst.PlayerRollingSpeed;

            // 向きをスムーズに変更（プレイヤーを移動方向に回転）
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = targetRotation;

            // Rigidbody の velocity を使って移動させる
            // 重力などの影響を維持するため、Y軸の速度はそのままにする
            Vector3 horizontalVelocity = moveDirection.normalized * speed;
            horizontalVelocity.y = _rigidbody.linearVelocity.y;
            _rigidbody.linearVelocity = horizontalVelocity;
            
            Debug.Log(horizontalVelocity);
        }
        else
        {
            // Blend と speedMultiplier を適用した移動速度（PlayerConst.PlayerSpeed は基準速度）
            float speed = PlayerConst.PlayerRollingSpeed;
            
            // Rigidbody の velocity を使って移動させる
            // 重力などの影響を維持するため、Y軸の速度はそのままにする
            Vector3 horizontalVelocity = transform.forward * speed;
            horizontalVelocity.y = _rigidbody.linearVelocity.y;
            _rigidbody.linearVelocity = horizontalVelocity;
            
            Debug.Log(horizontalVelocity);
        }
    }

    //攻撃時の移動
    public void AttackMove()
    {
        Vector3 horizontalVelocity = transform.forward * PlayerConst.attackMovePower; 
        _rigidbody.linearVelocity = horizontalVelocity;
    }
    
    //ダメージを受けた時のリアクション
    public void DamagedReaction()
    {
        SetAction("Damaged");
    }

    
    //敵を探す
    private IDisposable _disposable;
    public void SearchEnemy()
    {
        if (_lockOn)
        {
            ChangeState(new IdleState());
            _disposable.Dispose();
            _lockOn = _playerLockon.Lockon();
            
            _animator.SetBool("LockOnMove", false);
        }else{
            
            _lockOn = _playerLockon.Lockon();
            
            if (_lockOn == false) return;
            
            _disposable = Observable.EveryUpdate()
                .Subscribe(_=>
                {
                    _playerLockon.ManualUpdate();
                    _playerLockon.OnCameraXY(_changeTargetAction);
                })
                .AddTo(this);
            
            _playerLockon.TargetObj.GetComponent<IEnemy>()._isDeadReturner()
                .Where(x => x)
                .Subscribe(_ =>
            {
                Debug.Log("敵を倒した");
                SearchEnemy();
            })
                .AddTo(_playerLockon.TargetObj);
            
            AnimatorCommon.OutAnimation("FrontMove",_animator);
            
            if (_moveInputDirection.magnitude > 0.01f)
            {
                SetAction("LockOnMoveTrigger");
                ChangeState(new LockOnMove());
            }
            else
            {
                
                ChangeState(new LockOnIdle());
            }
            ChangeState(new LockOnIdle());
        } 
    }

    //アクション系のアニメーションが終わったときに呼び出して、Stateを戻す
    public void BackIdleAnimation()
    {
        _rollingAction = false;
        _attackAction = false;
        _isInvincible.Value = false;
        if (_lockOn)
        {
            if (_moveInputDirection.magnitude > 0.01f)
            {
                SetAction("LockOnMoveTrigger");
                ChangeState(new LockOnMove());
            }
            else
            {
                
                ChangeState(new LockOnIdle());
            }
        }
        else
        {
            if (_moveInputDirection.magnitude > 0.01f)
            {
                SetAction("MoveTrigger");
                ChangeState(new MoveState());
            }
            else
            {
                
                ChangeState(new IdleState());
            }
        }
    }

    private CompositeDisposable _justRollingAction = new CompositeDisposable();
    public void JustRollingStartReception()
    {
        Debug.Log("ジャスト回避受付開始");
        Observable.EveryUpdate()
            .Where(_ => _rollingAction)
            .Take(1)
            .Subscribe(_=>
            {
                _isInvincible.Value = true;
                ChangeState(new JustRollingState());
                Debug.Log("ジャスト回避！");
            })
            .AddTo(_justRollingAction);
        
        Observable.Interval(TimeSpan.FromSeconds(2))
            .Take(1)
            .Subscribe(_=>JustRollingStopReception())
            .AddTo(_justRollingAction);
    }

    public void JustRollingStopReception()
    {
        Debug.Log("ジャスト回避受付終了");
        _justRollingAction?.Dispose();
        _justRollingAction = new CompositeDisposable();
    }

    private void OnDestroy()
    {
        _justRollingAction?.Dispose();
    }

    private IDisposable _ComboAttackAction;
    public void StartComboAttack()
    {
        Debug.Log("コンボ受付開始");
        _ComboAttackAction = Observable.EveryUpdate()
            .Where(_=>_attackAction)
            .Take(1)
            .Subscribe(_ =>
            {
                AnimatorCommon.SetAction("Combo",_animator);
                AttackMove();
            })
            .AddTo(this);
    }

    public void StopComboAttack()
    {
        Debug.Log("コンボ受付終了");
        _ComboAttackAction?.Dispose();
        
    }
    
    //入力まわり
    private Vector2 _moveInputDirection;
    public Vector2 InputMoveDirection => _moveInputDirection;
    
    private bool _jumpAction;
    public bool JumpAction => _jumpAction;
    
    private bool _runAction;
    public bool RunAction => _runAction;
    
    private bool _crouchAction;
    public bool CrouchAction => _crouchAction;
    
    private bool _lockOnAction;
    public bool LockOnAction => _lockOnAction;
    
    private Vector2 _changeTargetAction;
    public Vector2 ChangeTargetAction => _changeTargetAction;
    
    private bool _rollingAction;
    public bool RollingAction => _rollingAction;

    private bool _attackAction;
    public bool AttackAction => _attackAction;
    
    //PlayerInputの入力受付
    public void GetEnemySearchInput(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed)return;
        SearchEnemy();
    }
    
    public void GetMoveInput(InputAction.CallbackContext context)
    {
        _moveInputDirection = context.ReadValue<Vector2>();
    }

    public void GetJumpInput(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.01f)
        {
            _jumpAction = true;
        }
        else
        {
            _jumpAction = false;
        }
        
    }

    public void GetRunInput(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.01f)
        {
            _runAction = true;
        }
        else
        {
            _runAction = false;
        }
    }

    public void GetCrouchInput(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.01f)
        {
            _crouchAction = true;
        }
        else
        {
            _crouchAction = false;
        }
    }

    public void GetLockOnInput(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.01f)
        {
            _lockOnAction = true;
        }
        else
        {
            _lockOnAction = false;
        }
    }

    public void GetChangeTargetAction(InputAction.CallbackContext context)
    {
        _changeTargetAction = context.ReadValue<Vector2>();
    }

    public void GetRollingAction(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.01f)
        {
            _rollingAction = true;
        }
        else
        {
            _rollingAction = false;
        }
    }

    public void GetAttackAction(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.01f)
        {
            _attackAction = true;
        }
        else
        {
            _attackAction = false;
        }
    }
}