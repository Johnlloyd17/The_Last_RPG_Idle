using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : Entity
{
    [Header("Check Player Layer")]
    [SerializeField] protected LayerMask whatIsPlayer;

    [Header("Stunned Info")]
    public float stunDuration;
    public Vector2 stunDirection;
    protected bool canBeStunned;
    [SerializeField] protected GameObject counterImage;


    [Header("Move info")]
    public float moveSpeed;
    public float idleTime;
    public float battleTime;
    public float defaultMoveSpeed;

    [Header("Attack info")]
    public float attackDistance;
    public float attackCooldown;
    [HideInInspector] public float lastTimeAttacked;

    public EnemyStateMachine stateMachine { get; private set; }
    public string lastAnimBoolName { get; private set; }


    [Header("Collision info")]
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckDistance = 0.2f; // Adjusted for ground detection
    [SerializeField] protected Transform slopeCheck;
    [SerializeField] protected float slopeCheckDistance = 0.2f; // Adjusted for slope detection

    protected override void Awake()
    {
        base.Awake();
        stateMachine = gameObject.AddComponent<EnemyStateMachine>();

        defaultMoveSpeed = moveSpeed;
    }

    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();
    }

    public virtual RaycastHit2D IsPlayerDetected()
    {
        // Perform the raycast to detect the player
        RaycastHit2D playerHit = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, 50, whatIsPlayer);
        if (playerHit.collider != null)
        {
            // Perform another raycast to check for walls or slopes between the enemy and the player
            RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, playerHit.distance, whatIsCollision);

            // Check if the hit collider is a wall or a slope
            if (wallHit.collider == null || !IsWallOrSlope(wallHit.collider))
            {
                return playerHit;
            }
        }
        return new RaycastHit2D(); // Return an empty RaycastHit2D if the player is not detected or if there is a wall or slope in the way
    }

    private bool IsWallOrSlope(Collider2D collider)
    {
        return collider.gameObject.layer == LayerMask.NameToLayer("Wall") ||
               collider.gameObject.layer == LayerMask.NameToLayer("Slope");
    }

    public virtual void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Draw the attack check line
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackDistance * facingDir, transform.position.y));

        // Draw the slope check ray (vertically)
        if (slopeCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(slopeCheck.position, new Vector3(slopeCheck.position.x, slopeCheck.position.y - slopeCheckDistance));
        }

        // Draw the ground check ray (vertically)
        if (groundCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        }
    }

    public virtual void OpenCounterAttackWindow()
    {
        canBeStunned = true;
        counterImage.SetActive(true);
    }

    public virtual void CloseCounterAttackWindow()
    {
        canBeStunned = false;
        counterImage.SetActive(false);
    }

    public virtual bool CanBeStunned()
    {
        if (canBeStunned)
        {
            CloseCounterAttackWindow();
            return true;
        }
        return false;
    }

    #region Detection
    public virtual bool IsSlopeDetected()
    {
        if (slopeCheck == null)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(slopeCheck.position, Vector2.down, slopeCheckDistance, whatIsCollision);
        return hit.collider != null && hit.collider.gameObject.layer == slopeLayer;
    }

    public virtual bool IsGroundDetected()
    {
        if (groundCheck == null)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsCollision);
        return hit.collider != null && hit.collider.gameObject.layer == groundLayer;
    }
    #endregion

    #region Freezing Enemies
    public virtual void FreezeTime(bool _timeFrozen)
    {
        if (_timeFrozen)
        {
            moveSpeed = 0;
            anim.speed = 0;
        }
        else
        {
            moveSpeed = defaultMoveSpeed;
            anim.speed = 1;
        }
    }
    protected virtual IEnumerator FreezeTimerCoroutine(float _seconds)
    {
        FreezeTime(true);
        yield return new WaitForSeconds(_seconds);
        FreezeTime(false);

    }
    public void StartBusyForFreezeTimeCoroutine(float _seconds)
    {
        StartCoroutine(FreezeTimerCoroutine(_seconds));
    }
    #endregion

    public void AssignLastAnimName(string _lastAnimBoolName) => lastAnimBoolName = _lastAnimBoolName;

    public override void SlowEntityBy(float _slowPercentage, float _slowDuration)
    {
        moveSpeed = moveSpeed * (1 - _slowPercentage);

        anim.speed = anim.speed * (1 - _slowPercentage);
        Invoke("ReturnDefaultSpeed", _slowDuration);
    }
    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();
        moveSpeed = defaultMoveSpeed;
    }


    public virtual void FreezeTimeFor(float _duration) => StartCoroutine(FreezeTimerCoroutine(_duration));
}
