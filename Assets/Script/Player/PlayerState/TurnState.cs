using UnityEngine;

public class TurnState : IPlayerState
{ 
    public void EnterState(PlayerStateMachine player)
    {
        Debug.Log("Entering Turn State");
        player.SetAction("Turn");
    }

    public void UpdateState(PlayerStateMachine player)
    {
        if (player.JumpAction)
        {
            player.ChangeState(new JumpState());
            return;
        }
        if (player.InputMoveDirection == new Vector2(0, 0))
        {
            player.ChangeState(new IdleState());
            return;
        }
    }

    public void ExitState(PlayerStateMachine player)
    {
        Debug.Log("Exiting Walk State");
        player._blend.Value = 0;
        player.OutAnimation("FrontMove");
    }
    
}
