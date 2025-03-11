using System.Collections;
using System.Runtime.Serialization.Json;
using System.Threading;
using UnityEngine;

public class Entity : MonoBehaviour
{
    #region Components
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public EntityFx fx { get; private set; }
    public CapsuleCollider2D cd { get; private set; }
    public SpriteRenderer sr { get; private set; }
    public CharacterStats stats { get; private set; }

    #endregion

    [Header("Knockback info")]
    [SerializeField] protected Vector2 knockbackDirection;
    [SerializeField] protected float knockbackDuration;
    protected bool isKnocked;

    [Header("Collision info")]
    public Transform attackCheck;
    public float checkDistance = 0.5f;
    public float attackCheckRadius;
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance = 0.2f; 

    [Header("Layer info")]
    [SerializeField] protected LayerMask whatIsCollision; // Combined LayerMask for ground, walls, and slopes


    public int facingDir { get; protected set; } = 1; 
    [Header("Facing Direction")]
    public bool facingRight = true;
    public System.Action onFlipped;




    private bool ignoreWallCheck; // Flag to ignore ground check for a short duration
 
    protected int wallLayer; // Layer index for the wall
    protected int slopeLayer; // Layer index for the slope
    protected int groundLayer; // Layer index for the ground
    protected Vector2 originalGroundCheckBoxSize; // Store the original size of the ground check box

    protected virtual void Awake(){}

    protected virtual void Start()
    {
        CheckGameObjects();
        DefensiveStatement();

        // Initialize the wall, slope, and ground layer indices
        wallLayer = LayerMask.NameToLayer("Wall");
        slopeLayer = LayerMask.NameToLayer("Slope");
        groundLayer = LayerMask.NameToLayer("Ground");
   
    }

    protected virtual void Update(){}

    #region Checking Game Objects
    protected virtual void CheckGameObjects()
    {
        fx = GetComponent<EntityFx>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        cd = GetComponent<CapsuleCollider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        stats = GetComponent<CharacterStats>();
    }

    protected virtual void DefensiveStatement()
    {
        if (rb == null)
        {
            Debug.Log("RigidBody2D is not found.");
        }

        if (anim == null)
        {
            Debug.Log("Animator is not found.");
        }

        if (cd == null)
        {
            Debug.Log("CapsuleCollider2D is not found.");
        }
        if (sr == null)
        {
            Debug.Log("SpriteRenderer is not found.");
        }
        if (stats == null) {
            Debug.Log("Stats is not found.");
        }
    }
    #endregion

    #region Detection
    public virtual bool IsWallDetected()
    {
        if (ignoreWallCheck || wallCheck == null)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsCollision);
        return hit.collider != null && hit.collider.gameObject.layer == wallLayer;
    }
    #endregion

    #region Drawing
    protected virtual void OnDrawGizmos()
    {
        if (cd == null)
            cd = GetComponent<CapsuleCollider2D>();

        if (cd != null)
        {
    
            // Draw the wall check ray
            if (wallCheck != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance * facingDir, wallCheck.position.y));
            }

        }

        // Draw attack check
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackCheck.position, attackCheckRadius);
    }
    #endregion

    #region Flipping Character
    public virtual void Flip()
    {
        facingDir *= -1;
        facingRight = !facingRight;
        transform.Rotate(0, -180, 0);

        if (onFlipped != null)
            onFlipped();
    }

    protected virtual void FlipController(float x)
    {
        if (x > 0 && !facingRight)
            Flip();
        else if (x < 0 && facingRight)
            Flip();
    }
    #endregion

    #region Velocity
    public void SetZeroVelocity()
    {
        if (isKnocked)
            return;

        rb.velocity = Vector2.zero;
    }

    public void SetVelocity(float x, float y)
    {
        if (isKnocked)
            return;

        rb.velocity = new Vector2(x, y);
        FlipController(x);
    }
    #endregion

    #region Damage and Knockback effect
    public virtual void DamageImpact() => StartCoroutine(HitKnockback());

    protected virtual IEnumerator HitKnockback()
    {
        isKnocked = true;

        rb.velocity = new Vector2(knockbackDirection.x * -facingDir, knockbackDirection.y);
        yield return new WaitForSeconds(knockbackDuration);

        isKnocked = false;
    }
    #endregion

    #region Ignore Wall Check
    public void IgnoreWallCheck(float duration)
    {
        StartCoroutine(IgnoreWallCheckCoroutine(duration));
    }

    private IEnumerator IgnoreWallCheckCoroutine(float duration)
    {
        ignoreWallCheck = true;
        yield return new WaitForSeconds(duration);
        ignoreWallCheck = false;
    }
    #endregion

    public virtual void Die() { 

    }

    public virtual void SlowEntityBy(float _slowPercentage, float _slowDuration) {

    }
    protected virtual void ReturnDefaultSpeed() { 
        anim.speed = 1;
    }
}
