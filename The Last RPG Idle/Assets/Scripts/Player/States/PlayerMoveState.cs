using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    public PlayerMoveState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        rb.isKinematic = false; // Ensure the character is not kinematic while moving
    }

    public override void Exit()
    {
        base.Exit();
        rb.isKinematic = false; // Ensure the character is not kinematic while moving
    }

    public override void Update()
    {
        base.Update();

        player.SetVelocity(xInput * player.moveSpeed, rb.velocity.y);

        if (xInput == 0 || player.IsWallDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }
        else
        {
            float moveSpeed = player.moveSpeed;
            Vector2 moveDirection = new Vector2(xInput * moveSpeed, rb.velocity.y);

            if (player.IsOnSlope())
            {
                Vector2 stairNormal = player.GetSlopeNormal();
                stairNormal.Normalize();

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    return;
                }

                float dotProduct = Vector2.Dot(moveDirection, stairNormal);
                Vector2 projectedMovement = moveDirection - dotProduct * stairNormal;
                player.SetVelocity(projectedMovement.x, rb.velocity.y);
            }
            else
            {
                player.SetVelocity(moveSpeed * xInput, rb.velocity.y);
            }
        }
    }
}
