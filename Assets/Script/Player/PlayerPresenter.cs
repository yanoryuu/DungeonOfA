using System;
using System.Collections.Generic;
using System.IO;
using R3;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerPresenter : MonoBehaviour
{
    private PlayerModel _playerModel;
    
    //PlayerStateMachineをPlayerViewと扱う
    [SerializeField]private PlayerStateMachine _playerStateMachine;
    
    [SerializeField]private PlayerView _playerView;
    
    [SerializeField]private GameOverView _gameOverView;
    
    [SerializeField] private Collider swordCollider;
    
    [SerializeField] private PlayerStateMachine _stateMachine;
    
    private ReactiveProperty<bool> _isDead = new ReactiveProperty<bool>();
    
    public ReactiveProperty<bool> IsDead => _isDead;

    
    private string saveFilePath => Path.Combine(Application.persistentDataPath, "player_save.json");

    
    private void Start()
    {
        _playerModel = new PlayerModel();
        Bind();
        _gameOverView.Hide();
        LoadPlayerDataJson();
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.PlayBGM(SoundManager.Instance.mainBGM);
    }

    
    public void Bind()
    {
        Observable.EveryUpdate()
            .Where(_=>_isDead.Value==false)
            .Subscribe(_=>_playerStateMachine.manualUpdate())
            .AddTo(this);
        
        _playerModel.PlayerHp.Subscribe(x=>_playerView.SetPlayerHealth(x,_playerModel.PlayerMaxHealth.Value))
            .AddTo(this);
        
        _playerModel.CurrentPlayerExp.Subscribe(x =>_playerView.SetPlayerExp(x,_playerModel.LevelUpExp.Value))
            .AddTo(this);
        
        _playerModel.PlayerLevel.Subscribe(x =>_playerView.SetPlayerLevel(x))
            .AddTo(this);

        _playerModel.PlayerMaxHealth.Subscribe(x =>
            _playerView.SetPlayerHealth(x, _playerModel.PlayerMaxHealth.Value))
            .AddTo(this);

        _playerModel.CurrentPlayerAttackPower.Subscribe(x => _playerView.SetAttackPower(x))
            .AddTo(this);
        
        _playerModel.PlayerHp.Where(x=>x<=0)
            .Subscribe(_ =>_isDead.Value = true)
            .AddTo(this);
        
        _isDead.Where(x =>x)
            .Subscribe(_=>Death())
            .AddTo(this);
        
        
        _gameOverView.ExitButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("ExitButton clicked");
                SceneManager.LoadScene("TitleScene");
            })
            .AddTo(this);
        
        _gameOverView.ContinueButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("ContinueButton clicked");
                if (File.Exists(saveFilePath))
                {
                    SceneManager.LoadScene("DungenOfA");
                }
            })
            .AddTo(this);
    }
    
    //ダメージを受けた時の処理
    public void Damage(float damage)
    {
        // Debug.Log("Player Damaged");
        if (_stateMachine.IsInvincible.Value)return;
        _playerModel.DamagePlayer(damage);
        _stateMachine.DamagedReaction();
        _playerView.ShakeHPBar(0.5f);
        _playerView.FlashRedEffect();
    }

    //敵を倒したときの演出などはここ
    public void KillEnemy(float exp)
    {
        _playerModel.GetExp(exp);
        SoundManager.Instance.PlaySE2D(SoundManager.Instance.KillSE);
    }

    public void OnFootStep()
    {
        SoundManager.Instance.PlaySE3D(SoundManager.Instance.footstepSE,transform.position);
    }
    
    public void Death()
    {
        _gameOverView.Show();
        _playerStateMachine.SetAction("Death");
        SoundManager.Instance.StopBGM();
    }
    
    [SerializeField] private CinemachineImpulseSource _nomalCinemachineImpulseSource;
    [SerializeField] private GameObject _bloodEffect;
    [SerializeField] private Transform _swordTransform;
    public void Attack(float damage)
    {
        EnemyStats _killEnemyStats;
        // SwordColliderのバウンディングボックス内の全てのColliderを取得
        Collider[] hitColliders = Physics.OverlapBox(
            swordCollider.bounds.center, 
            swordCollider.bounds.extents, 
            swordCollider.transform.rotation);
        
        SoundManager.Instance.PlaySE2D(SoundManager.Instance.attackSE);
        
        foreach (Collider hit in hitColliders)
        {
            // Enemyコンポーネントが存在するかをチェック
            IEnemy enemy = hit.GetComponent<IEnemy>();

            if (enemy != null)
            {
                _killEnemyStats = enemy.Damage(_playerModel.CurrentPlayerAttackPower.Value+damage);
                
                _nomalCinemachineImpulseSource.GenerateImpulse();
                
                if (_killEnemyStats!=null)
                {
                    //倒したときのプレイヤーへの経験値などのフィードバッグ
                    _playerStateMachine.JustRollingStopReception();
                    KillEnemy(_killEnemyStats.enemyExp);
                }

                if (_bloodEffect != null)
                {
                    // 敵の位置にエフェクトを生成（必要ならオフセット調整も）
                    GameObject bloodEffect = Instantiate(_bloodEffect, hit.transform.position+ new Vector3(0,1.5f,0), Quaternion.identity);
                    // 一定時間後に自動で破棄
                    Destroy(bloodEffect, 2f);
                }
            }
        }
    }
    
    [SerializeField] private CinemachineImpulseSource _specialCinemachineImpulseSource;
    
    public void SpecialAttack(float damage)
    {
        _playerStateMachine.FocusedEffectObject.SetActive(false);
        
        EnemyStats _killEnemyStats;
        // SwordColliderのバウンディングボックス内の全てのColliderを取得
        Collider[] hitColliders = Physics.OverlapBox(
            swordCollider.bounds.center, 
            swordCollider.bounds.extents, 
            swordCollider.transform.rotation);
        
        SoundManager.Instance.PlaySE2D(SoundManager.Instance.justRollingAttackSE);
        
        foreach (Collider hit in hitColliders)
        {
            // Enemyコンポーネントが存在するかをチェック
            IEnemy enemy = hit.GetComponent<IEnemy>();
            
            _specialCinemachineImpulseSource.GenerateImpulse();
            

            if (enemy != null)
            {
                _killEnemyStats = enemy.Damage(_playerModel.CurrentPlayerAttackPower.Value+damage);
                HitStopUtility.HitStop(0.08f);
                if (_killEnemyStats!=null)
                {
                    //倒したときのプレイヤーへの経験値などのフィードバッグ
                    _playerStateMachine.JustRollingStopReception();
                    KillEnemy(_killEnemyStats.enemyExp);
                }
                if (_bloodEffect != null)
                {
                    // 敵の位置にエフェクトを生成（必要ならオフセット調整も）
                    GameObject bloodEffect = Instantiate(_bloodEffect, hit.transform.position+ new Vector3(0,1.5f,0), Quaternion.identity);
                    // 一定時間後に自動で破棄
                    Destroy(bloodEffect, 2f);
                }
            }
        }
    }
    
    //セーブ関係
    public void OnRetryButton()
    {
        LoadPlayerDataJson();
        _gameOverView.Hide();
    }

    public void SavePlayerDataJson()
    {
        _playerModel.RestorePlayerHealth();
        var data = _playerModel.ToSaveData(transform.position);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("セーブ完了");
    }

    public void LoadPlayerDataJson()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            var data = JsonUtility.FromJson<PlayerSaveData>(json);
            _playerModel.LoadFromData(data);
            transform.position = new Vector3(data.positionX, data.positionY, data.positionZ);
        }
        else
        {
            Debug.Log("セーブデータなし：新規スタート");
            _playerModel.Initialize();
            transform.position = new Vector3(451.98f,24.6f,480.88f);
        }
    }

    //初期化
    public void Initialize()
    {
        _playerModel.Initialize();
        Bind();
        _playerView.Initialize();
    }
    
    
    //Input関係
    private bool _selectAction;
    public bool SelectAction => _selectAction;
    
    public void GetSelectInput(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.01f)
        { 
            _selectAction = true;
        }
        else
        {
            _selectAction = false;
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        Damage(other.gameObject.GetComponent<MagicDamage>().Damage);
    }
}