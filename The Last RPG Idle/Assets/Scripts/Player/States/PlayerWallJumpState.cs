using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerState
{
    public PlayerWallJumpState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        rb.isKinematic = false; // Ensure kinematic is set to false on exit
        stateTimer = .4f;
        player.SetVelocity(5 * -player.facingDir, player.jumpForce);
        player.IgnoreWallCheck(0.2f); // Ignore ground check for a short duration after jump
    }

    public override void Exit()
    {
        base.Exit();
        rb.isKinematic = false; // Ensure kinematic is set to false on exit
    }

    public override void Update()
    {
        base.Update();
        if (stateTimer < 0)
            stateMachine.ChangeState(player.airState);

        if (player.IsGroundBoxDetected())
            stateMachine.ChangeState(player.idleState);
    }
}
