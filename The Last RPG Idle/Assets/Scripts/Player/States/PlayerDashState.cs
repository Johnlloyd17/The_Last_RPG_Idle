

using UnityEngine;

public class PlayerDashState : PlayerState
{
    public PlayerDashState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        //player.skill.clone.CreateClone(player.transform, Vector3.zero);
        //player.skill.clone.CreateCloneOnDashStart();
        player.skill.dash.CloneOnDash();
        stateTimer = player.dashDuration;
    }

    public override void Exit()
    {
        base.Exit();
        //player.skill.clone.CreateCloneOnDashOver();
        player.skill.dash.CloneOnArrival();
        player.SetVelocity(0, rb.velocity.y);
    }

    public override void Update()
    {
        base.Update();

        if (!player.IsGroundBoxDetected() && player.IsWallDetected())
            stateMachine.ChangeState(player.wallSlideState);

        player.SetVelocity(player.dashSpeed * player.dashDir, 0);

        if (stateTimer < 0)
            stateMachine.ChangeState(player.idleState);
    }

}
