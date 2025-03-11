using UnityEngine;

public class PlayerPrimaryAttackState : PlayerState
{
    public int comboCounter { get; private set; }
    private float lastTimeAttacked;
    // private float comboWindow = 2;

    public PlayerPrimaryAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        xInput = 0;

        if (comboCounter > 2 || Time.time >= lastTimeAttacked + comboCounter)
            comboCounter = 0;

        player.anim.SetInteger("ComboCounter", comboCounter);

        #region Choose Attack Direction
        float attackDir = player.facingDir;
        if (xInput != 0)
        {
            attackDir = xInput;
        }
        #endregion

        player.SetVelocity(player.attackMovement[comboCounter].x * attackDir, player.attackMovement[comboCounter].y); // Stop any movement during attack

        stateTimer = .1f; // Duration of the attack animation

        // Start the busy coroutine to prevent movement during the attack
        player.StartBusyCoroutine(.15f); // Duration of busy state
    }

    public override void Exit()
    {
        base.Exit();
        comboCounter++;
        lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();
        if (stateTimer < 0)
            player.SetZeroVelocity();

        if (triggeredCalled)
            stateMachine.ChangeState(player.idleState);
    }
}
