using R3;
using UnityEngine;

public class PlayerModel
{
    private ReactiveProperty<float> _currentPlayerAttackPower;
    public ReactiveProperty<float> CurrentPlayerAttackPower => _currentPlayerAttackPower;

    private ReactiveProperty<float> _playerMaxHealth;
    public ReactiveProperty<float> PlayerMaxHealth => _playerMaxHealth;

    private ReactiveProperty<float> _currentPlayerHp;
    public ReactiveProperty<float> PlayerHp => _currentPlayerHp;

    private ReactiveProperty<int> _playerLevel;
    public ReactiveProperty<int> PlayerLevel => _playerLevel;

    private ReactiveProperty<float> _currentPlayerExp;
    public ReactiveProperty<float> CurrentPlayerExp => _currentPlayerExp;

    private ReactiveProperty<float> _levelUpExp;
    public ReactiveProperty<float> LevelUpExp => _levelUpExp;

    //コンストラクタ
    public PlayerModel()
    {
        _currentPlayerAttackPower = new ReactiveProperty<float>(PlayerConst.PlayerAttackPower);
        _currentPlayerHp = new ReactiveProperty<float>(PlayerConst.PlayerHP);
        _playerMaxHealth = new ReactiveProperty<float>(PlayerConst.PlayerHP);
        _currentPlayerExp = new ReactiveProperty<float>(0);
        _levelUpExp = new ReactiveProperty<float>(PlayerConst.FirstLevelUpExp);
        _playerLevel = new ReactiveProperty<int>(1);
    }

    public void DamagePlayer(float damage)
    {
        _currentPlayerHp.Value -= damage;
    }

    public void GetExp(float exp)
    {
        _currentPlayerExp.Value += exp;
        if (_currentPlayerExp.Value >= _levelUpExp.Value)
        {
            LevelUp();
        }
    }

    public void LevelUp()
    {
        _levelUpExp.Value *= PlayerConst.LevelUpExpMultiplier;
        _playerLevel.Value++;
        _playerMaxHealth.Value += PlayerConst.HpUp;
        _currentPlayerAttackPower.Value += PlayerConst.AttackPowerUp;
        _currentPlayerExp.Value -= LevelUpExp.Value;
    }

    public void Initialize()
    {
        _currentPlayerAttackPower.Value = PlayerConst.PlayerAttackPower;
        _currentPlayerHp.Value = PlayerConst.PlayerHP;
        _playerMaxHealth.Value = PlayerConst.PlayerHP;
        _currentPlayerExp.Value = 0;
        _levelUpExp.Value = PlayerConst.FirstLevelUpExp;
        _playerLevel.Value = 1;
    }

    public PlayerSaveData ToSaveData(Vector3 playerPosition)
    {
        return new PlayerSaveData
        {
            positionX = playerPosition.x,
            positionY = playerPosition.y,
            positionZ = playerPosition.z,
            playerHP = _currentPlayerHp.Value,
            playerMaxHP = _playerMaxHealth.Value,
            playerEXP = _currentPlayerExp.Value,
            levelUpExp = _levelUpExp.Value,
            playerLevel = _playerLevel.Value,
            attackPower = _currentPlayerAttackPower.Value
        };
    }

    public void LoadFromData(PlayerSaveData data)
    {
        _currentPlayerHp.Value = data.playerHP;
        _playerMaxHealth.Value = data.playerMaxHP;
        _currentPlayerExp.Value = data.playerEXP;
        _levelUpExp.Value = data.levelUpExp;
        _playerLevel.Value = data.playerLevel;
        _currentPlayerAttackPower.Value = data.attackPower;
    }

    public void RestorePlayerHealth()
    {
        _currentPlayerHp.Value = _playerMaxHealth.Value;
    }
}