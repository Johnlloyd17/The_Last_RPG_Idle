using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;

public class SkeletonMoveState : SkeletonGroundedState
{
    public SkeletonMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        enemy.SetVelocity(enemy.moveSpeed * enemy.facingDir, rb.velocity.y);

        bool isWallDetected = enemy.IsWallDetected();
        bool isGroundDetected = enemy.IsGroundDetected();
        bool isSlopeDetected = enemy.IsSlopeDetected();

        if (isWallDetected || isGroundDetected || isSlopeDetected)
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.idleState);
            

        }
        else if (player != null && (enemy.IsPlayerDetected() || Vector2.Distance(enemy.transform.position, player.position) < 2))
        {
            stateMachine.ChangeState(enemy.battleState);
        }

    }

}
