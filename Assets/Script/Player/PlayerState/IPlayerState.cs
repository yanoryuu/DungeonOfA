using UnityEngine;

public interface IPlayerState
{
    void EnterState(PlayerStateMachine player);
    void UpdateState(PlayerStateMachine player);
    void ExitState(PlayerStateMachine player);
}
