using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class SwordSkillController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private CircleCollider2D cd;
    private Player player;
    private SwordType swordType; // Add this field

    private bool canRotate = true;
    private bool isReturning;
    private bool isStuck;

    private float freezeTimeDuration;
    private float returnSpeed = 12f;


    [Header("Pierce info")]
    private float pierceAmount = 2f;

    [Header("Bounce info")]
    private float bounceSpeed;
    private bool isBouncing;
    private int bounceAmount;
    private List<Transform> enemyTarget = new List<Transform>();
    private int targetIndex;

    [Header("Spin info")]
    private float maxTravelDistance;
    private float spinDuration;
    private float spinTimer;
    private bool wasStopped;
    private bool isSpinning;

    private float hitTimer;
    private float hitCooldown;
    private float spinDirection;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        if (canRotate)
            transform.right = rb.velocity;

        if (isReturning && !isStuck)
        {
            if (player != null)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.transform.position, returnSpeed * Time.deltaTime);
                if (Vector2.Distance(transform.position, player.transform.position) < 1)
                {
                    player.CatchTheSword();
                }
            }
            else
            {
                Destroy(gameObject); // Destroy the sword if the player reference is null
            }
        }

        BounceLogic();
        SpinLogic();
    }

    private void SpinLogic()
    {
        if (isSpinning)
        {
            if (Vector2.Distance(player.transform.position, transform.position) > maxTravelDistance && !wasStopped)
            {
                StopWhenSpinning();
            }
            if (wasStopped)
            {
                spinTimer -= Time.deltaTime;
                //transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x + spinDirection, transform.position.y), 1.5f * Time.deltaTime);
                if (spinTimer < 0)
                {
                    isReturning = true;
                    isSpinning = false;
                }

                hitTimer -= Time.deltaTime;
                if (hitTimer < 0)
                {
                    hitTimer = hitCooldown;
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1);
                    foreach (var hit in colliders)
                    {
                        if (hit.GetComponent<Enemy>() != null)
                        {
                            SwordSkillDamage(hit.GetComponent<Enemy>());
                        }
                    }
                }
            }
        }
    }

    private void BounceLogic()
    {
        if (isBouncing && enemyTarget.Count > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, enemyTarget[targetIndex].position, bounceSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, enemyTarget[targetIndex].position) < .1f)
            {
                SwordSkillDamage(enemyTarget[targetIndex].GetComponent<Enemy>());       

                targetIndex++;
                bounceAmount--;

                if (bounceAmount <= 0)
                {
                    isBouncing = false;
                    isReturning = true;
                }
                else if (targetIndex >= enemyTarget.Count)
                {
                    targetIndex = 0;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isReturning && !isStuck)
            return;

        if (isStuck) // Prevent further processing if the sword is already stuck
            return;

        if (collision.GetComponent<Enemy>() != null)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            SwordSkillDamage(enemy);
        }

        SetupTargetForBounce(collision);
        StuckInto(collision);
    }

    private void SwordSkillDamage(Enemy enemy)
    {
        EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();

        //enemy.DamageEffect();
        player.stats.DoDamage(enemy.GetComponent<CharacterStats>());

        // Skill tree checking timestop
        if (player.skill.sword.timeStopUnlocked) {
            enemy.FreezeTimeFor(freezeTimeDuration);
        }

        if (player.skill.sword.vulnerableUnlocked)
        {
            enemyStats.MakeVulnerableFor(freezeTimeDuration);
        }
        // ===============

        enemy.StartBusyForFreezeTimeCoroutine(freezeTimeDuration);

        ItemData_Equipment equiomentAmulet = Inventory.instance.GetEquipment(EquipmentType.Amulet);
        if (equiomentAmulet != null)
        {
            equiomentAmulet.Effect(enemy.transform);
        } 
    }

    private void SetupTargetForBounce(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() != null)
        {
            if (isBouncing)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 10);
                foreach (var hit in colliders)
                {
                    if (hit.GetComponent<Enemy>() != null && hit.transform != collision.transform)
                    {
                        enemyTarget.Add(hit.transform);
                    }
                }

                // Check if only one enemy is found
                if (enemyTarget.Count == 0)
                {
                    isBouncing = false;
                    isReturning = false;
                    isStuck = true;
                }
                else
                {
                    enemyTarget.Insert(0, collision.transform); // Ensure the first hit enemy is the first target
                }
            }
        }
    }

    private void StuckInto(Collider2D collision)
    {
        if (swordType == SwordType.Pierce && pierceAmount > 0 && collision.GetComponent<Enemy>() != null)
        {
            pierceAmount--;
            return;
        }

        if (isSpinning)
        {
            if (!wasStopped)
            {
                StopWhenSpinning();
            }
            return;
        }

        if (swordType == SwordType.Regular && collision.GetComponent<Enemy>() != null)
        {
            // Regular sword sticks to the first enemy hit
            isStuck = true;
            isReturning = false;
            canRotate = false;
            cd.enabled = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            anim.SetBool("Rotation", false);
            transform.parent = collision.transform;
            return; // Exit early to prevent multiple sticking
        }

        canRotate = false;
        cd.enabled = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        if (isBouncing && enemyTarget.Count > 0)
            return;

        anim.SetBool("Rotation", false);
        transform.parent = collision.transform;
    }

    private void StopWhenSpinning()
    {
        wasStopped = true;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        spinTimer = spinDuration;
    }

    public void ReturnSword()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        transform.parent = null;

        isReturning = true;
        isStuck = false; // Ensure the sword can return
    }

    public void SetupBounce(bool _isBouncing, int _amountOfBounces, float _bounceSpeed)
    {
        isBouncing = _isBouncing;
        bounceAmount = _amountOfBounces;
        bounceSpeed = _bounceSpeed;
        anim.SetBool("Rotation", true);
    }

    public void SetupPierce(int _pierceAmount)
    {
        pierceAmount = _pierceAmount;
        anim.SetBool("Rotation", false); // Turn off rotation animation for pierce type
    }
    public void SetupSword(Vector2 _dir, float _gravityScale, Player _player, float _freezeTimeDuration, float _returnSpeed,SwordType _swordType)
    {
        player = _player;
        rb.gravityScale = _gravityScale;
        rb.velocity = _dir;

        freezeTimeDuration = _freezeTimeDuration;
        returnSpeed = _returnSpeed;

        swordType = _swordType; // Set the sword type
        anim.SetBool("Rotation", swordType != SwordType.Pierce); // Set rotation animation based on sword type
        Invoke("DestroyMe", 7);
    }
    public void SetupSpin(bool _isSpinning, float _maxTravelDistance, float _spinDuration, float _hitCooldown)
    {
        isSpinning = _isSpinning;
        maxTravelDistance = _maxTravelDistance;
        spinDuration = _spinDuration;
        hitCooldown = _hitCooldown;

        anim.SetBool("Rotation", true);
        spinDirection = Mathf.Clamp(rb.velocity.x, -1, 1);
    }

    private void DestroyMe() {
        Destroy(gameObject);
    }
}
