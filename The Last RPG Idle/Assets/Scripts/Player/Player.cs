using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Player : Entity
{
    #region Behaviours
    [Header("Attack details")]
    public Vector2[] attackMovement;
    public float counterAttackDuration = .2f;

    public bool isBusy { get; private set; }

    [Header("Move info")]
    public float moveSpeed = 12f;
    public float jumpForce;
    public float swordReturningImpact;
    private float defaultMoveSpeed;
    private float defaultJumpForce;


    [Header("Dash info")]
    public float dashSpeed;
    public float dashDuration;
    private float defaultDashSpeed;
    public float dashDir { get; private set; }

    [Header("Ground Detection")]
    [SerializeField] protected Vector2 groundCheckBoxSize = new Vector2(1.5f, 0.1f); // Adjustable ground check box size


    public SkillManager skill { get; private set; }
    public GameObject sword { get; private set; }
    #endregion

    #region States

    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerWallSlideState wallSlideState { get; private set; }
    public PlayerWallJumpState wallJumpState { get; private set; }
    public PlayerDashState dashState { get; private set; }

    // attack state
    public PlayerPrimaryAttackState primaryAttackState { get; private set; }
    public PlayerCounterAttackState counterAttackState { get; private set; }

    // Sword states
    public PlayerAimSwordState aimSwordState { get; private set; }
    public PlayerCatchSwordState catchSwordState { get; private set; }

    // Special SKill
    public PlayerBlackHoleState blackHoleState { get; private set; }

    public PlayerDeathState deathState { get; private set; }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        stateMachine = gameObject.AddComponent<PlayerStateMachine>();
        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");
        jumpState = new PlayerJumpState(this, stateMachine, "Jump");
        airState = new PlayerAirState(this, stateMachine, "Jump");
        dashState = new PlayerDashState(this, stateMachine, "Dash");
        wallSlideState = new PlayerWallSlideState(this, stateMachine, "WallSlide");
        wallJumpState = new PlayerWallJumpState(this, stateMachine, "Jump");

        primaryAttackState = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
        counterAttackState = new PlayerCounterAttackState(this, stateMachine, "CounterAttack");

        aimSwordState = new PlayerAimSwordState(this, stateMachine, "AimSword");
        catchSwordState = new PlayerCatchSwordState(this, stateMachine, "CatchSword");

        blackHoleState = new PlayerBlackHoleState(this, stateMachine, "Jump");

        deathState = new PlayerDeathState(this, stateMachine, "Die");
    }
    protected override void Start()
    {
        base.Start();
        // Store the original size of the ground check box
        originalGroundCheckBoxSize = groundCheckBoxSize;

        skill = SkillManager.instance;

        stateMachine.Initialize(moveState);
        SetContinuousCollision();

        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
        defaultDashSpeed = dashSpeed;
    }

    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();
        CheckForDashingInput();

        if (Input.GetKeyDown(KeyCode.F) && skill.crystal.crystalUnlocked)
        {
            if (skill.crystal.CanUseSkill())
                skill.crystal.UseSkill();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Inventory.instance.UseFlask();
        }
    }

    public void SetContinuousCollision()
    {
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void StartBusyCoroutine(float duration)
    {
        StartCoroutine(BusyFor(duration));
    }
    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;
        yield return new WaitForSeconds(_seconds);
        isBusy = false;
    }

    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    private void CheckForDashingInput()
    {

        if (IsWallDetected())
            return;

        if (skill.dash.dashUnlocked == false)
            return;


        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            && SkillManager.instance.dash.CanUseSkill())
        {
            dashDir = Input.GetAxisRaw("Horizontal");
            if (dashDir == 0)
                dashDir = facingDir;

            stateMachine.ChangeState(dashState);
        }
    }

    #region Sword
    public void AssignNewSword(GameObject _newSword)
    {
        sword = _newSword;
    }
    public void CatchTheSword()
    {
        stateMachine.ChangeState(catchSwordState);
        Destroy(sword);
    }
    #endregion


    public virtual bool IsGroundBoxDetected()
    {
        Bounds bounds = cd.bounds;
        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y);
        Vector2 boxSize = new Vector2(groundCheckBoxSize.x, groundCheckBoxSize.y + rb.velocity.y * Time.deltaTime);

        Collider2D[] colliders = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, whatIsCollision);

        DebugDrawBox(boxCenter, boxSize, colliders.Length > 0 ? Color.green : Color.red);

        return colliders.Length > 0;
    }



    #region Handling slope
    public virtual Vector2 GetSlopeNormal()
    {
        Bounds bounds = cd.bounds;
        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y);
        Vector2 boxSize = new Vector2(groundCheckBoxSize.x, groundCheckBoxSize.y);

        Collider2D[] colliders = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, whatIsCollision);

        if (colliders.Length > 0)
        {
            return colliders[0].transform.up;
        }

        return Vector2.up;
    }


    public virtual bool IsOnSlope()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(cd.bounds.center, cd.bounds.size, 0f, whatIsCollision);
        foreach (var collider in colliders)
        {
            if (Vector2.Angle(collider.transform.up, Vector2.up) > 0)
            {
                bool isSlope = collider.gameObject.layer == slopeLayer;
                Debug.Log("Detected slope: " + isSlope);
                return isSlope;
            }
        }
        return false;
    }


    public virtual float GetSlopeAngle()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(cd.bounds.center, cd.bounds.size, 0f, whatIsCollision);
        foreach (var collider in colliders)
        {
            if (Vector2.Angle(collider.transform.up, Vector2.up) > 0)
            {
                return Vector2.Angle(collider.transform.up, Vector2.up);
            }
        }
        return 0f;
    }
    #endregion


    private void DebugDrawBox(Vector2 center, Vector2 size, Color color)
    {
        Vector2 halfSize = size / 2;
        Vector2 topLeft = center + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = center + new Vector2(halfSize.x, halfSize.y);
        Vector2 bottomLeft = center + new Vector2(-halfSize.x, -halfSize.y);
        Vector2 bottomRight = center + new Vector2(halfSize.x, -halfSize.y);

        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
    }

    #region Dynamic LayerMask Box Collider Auto Adjust
    public virtual void ResizeGroundCheckBox(float adjustment)
    {
        groundCheckBoxSize = new Vector2(adjustment, groundCheckBoxSize.y);
    }

    public virtual void ResetGroundCheckBox()
    {
        groundCheckBoxSize = originalGroundCheckBoxSize;
    }

    // New collision detection methods
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == wallLayer || collision.gameObject.layer == slopeLayer || collision.gameObject.layer == groundLayer)
        {
            // Handle collision with wall or slope
            HandleCollisionEnter(collision);
            if (collision.gameObject.layer == wallLayer)
            {
                ResizeGroundCheckBox(0.1f);
            }
            else if (collision.gameObject.layer == slopeLayer)
            {
                ResetGroundCheckBox();
            }
            else if (collision.gameObject.layer == groundLayer)
            {
                ResetGroundCheckBox();

            }
        }
        else if (IsInLayerMask(collision.gameObject, whatIsCollision))
        {
            HandleCollisionEnter(collision);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == wallLayer || collision.gameObject.layer == slopeLayer || collision.gameObject.layer == groundLayer)
        {
            HandleCollisionStay(collision);
            if (collision.gameObject.layer == wallLayer)
            {
                ResizeGroundCheckBox(0.1f);
            }
            else if (collision.gameObject.layer == slopeLayer)
            {
                ResetGroundCheckBox();
            }
            else if (collision.gameObject.layer == groundLayer)
            {
                ResetGroundCheckBox();
            }
        }
        else if (IsInLayerMask(collision.gameObject, whatIsCollision))
        {
            HandleCollisionStay(collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == wallLayer || collision.gameObject.layer == slopeLayer || collision.gameObject.layer == groundLayer)
        {
            HandleCollisionExit(collision);
            if (collision.gameObject.layer == wallLayer)
            {
                ResizeGroundCheckBox(0.1f);
            }
            else if (collision.gameObject.layer == slopeLayer)
            {
                ResetGroundCheckBox();
            }
            else if (collision.gameObject.layer == groundLayer)
            {
                ResizeGroundCheckBox(0.3f);
            }
        }
        else if (IsInLayerMask(collision.gameObject, whatIsCollision))
        {
            // Handle end of collision with ground or other collision objects
            HandleCollisionExit(collision);
        }
    }

    // Methods to handle collision events
    protected virtual void HandleCollisionEnter(Collision2D collision)
    {
        //Debug.Log("Entered collision with: " + collision.gameObject.name);
    }

    protected virtual void HandleCollisionStay(Collision2D collision)
    {
        //Debug.Log("Staying in collision with: " + collision.gameObject.name);
    }

    protected virtual void HandleCollisionExit(Collision2D collision)
    {
        //Debug.Log("Exited collision with: " + collision.gameObject.name);
    }

    // Helper method to check if the collision object is in the specified layer mask
    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return ((1 << obj.layer) & layerMask) != 0;
    }
    #endregion

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (cd != null)
        {
            // Draw the ground check box
            Gizmos.color = Color.red;
            Bounds bounds = cd.bounds;
            Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y);
            Vector2 boxSize = new Vector2(groundCheckBoxSize.x, groundCheckBoxSize.y + (Application.isPlaying ? rb.velocity.y * Time.deltaTime : 0));

            Gizmos.DrawWireCube(boxCenter, boxSize);

        }
    }

    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deathState);
    }

    #region Slowing Movement for ICE element
    public override void SlowEntityBy(float _slowPercentage, float _slowDuration)
    {
        moveSpeed = moveSpeed * (1 - _slowPercentage);
        jumpForce = jumpForce * (1 - _slowPercentage);
        dashSpeed = dashSpeed * (1 - _slowPercentage);

        anim.speed = anim.speed * (1 - _slowPercentage);
        Invoke("ReturnDefaultSpeed", _slowDuration);

    }
    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();
        moveSpeed = defaultMoveSpeed;
        jumpForce = defaultJumpForce;
        dashSpeed = defaultDashSpeed;
    }
    #endregion
}
