using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCounterAttackState : PlayerState
{
    private bool canCreateClone;

    public PlayerCounterAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        canCreateClone = true;
        stateTimer = player.counterAttackDuration;
        player.anim.SetBool("SuccessfulCounterAttack", false);
    }

    public override void Exit()
    {
        base.Exit();

    }

    public override void Update()
    {
        base.Update();

        player.SetZeroVelocity();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackCheck.position, player.attackCheckRadius);
  
        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
         
                if (hit.GetComponent<Enemy>().CanBeStunned())
                {
           
                    stateTimer = 10; // any value bigger than 1
                    player.anim.SetBool("SuccessfulCounterAttack", true);

                    // Unlocking skill on the Skill Tree
                    player.skill.parry.UseSkill(); //going to use to restore health on parry

                    if (canCreateClone) {
                        canCreateClone = false;
                        //player.skill.clone.CreateCloneWithDelay(hit.transform);

                        player.skill.parry.MakeMirageOnParry(hit.transform);
                    }
                }
            }
        }


        if (stateTimer < 0 || triggeredCalled)
        {
            stateMachine.ChangeState(player.idleState);

        }
    }
}
