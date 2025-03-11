using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
public enum StatType
{
    strength,
    agility,
    intelligence,
    vitality,

    damage,
    critChance,
    critPower,

    maxHealth,
    armor,
    evasion,
    magicResistance,

    fireDamage,
    iceDamage,
    lightningDamage
}
public class CharacterStats : MonoBehaviour
{
    private EntityFx fx;


    [Header("========[ Major Stats ]========")]
    public Stat strength; // 1 point increase damage 1 by crit.power 1%
    public Stat agility; // 1 point increase invasion by 1% and crit.chance by 1%
    public Stat intelligence; // 1 point increase magic damage by 1 and magic resistance by 3 
    public Stat vitality; // 1 point increase health by 5 points


    [Header("========[ Offensive stats ]========")]
    public Stat damage;
    public Stat critChance;
    public Stat critPower; // default value 150%


    [Header("========[ Defensive stats ]========")]
    public Stat maxHealth; // player total health
    public Stat armor;
    public Stat evasion;
    public Stat magicResistance;

    [Header("========[ Magic stats ]========")]
    public Stat fireDamage;
    public Stat iceDamage;
    public Stat lightningDamage;

    [SerializeField] private float ailmentsDuration = 4f;
    public bool isIgnited; // does damage over time
    public bool isChilled; // reduce armor by 20%
    public bool isShocked; // reduce accuracy by 20%

    private float ignitedTimer;
    private float chilledTimer;
    private float shockedTimer;

    // ignite over time damage
    private float igniteDamageCooldown = .3f;
    private float igniteDamageTimer;
    private int igniteDamage;

    [SerializeField] private GameObject shockStrikePrefab;
    private int shockDamage;

    public int currentHealth;
    public System.Action onHealthChanged;
    public bool IsDead { get; private set; }
    private bool isVulnerable;


    protected virtual void Update()
    {
        ignitedTimer -= Time.deltaTime;
        chilledTimer -= Time.deltaTime;
        shockedTimer -= Time.deltaTime;


        igniteDamageTimer -= Time.deltaTime;

        if (ignitedTimer < 0)
        {
            isIgnited = false;
        }
        if (chilledTimer < 0)
        {
            isChilled = false;
        }
        if (shockedTimer < 0)
        {
            isShocked = false;
        }

        if (isIgnited) { ApplyIgniteDamage(); }

    }



    protected virtual void Start()
    {
        critPower.SetDefaultValue(150);
        currentHealth = GetMaxHealValue();

        fx = GetComponent<EntityFx>();
        if (fx == null) Debug.LogError("EntityFx is not found.");

    }

    public virtual void TakeDamage(int _damage)
    {
        DecreaseHealthBy(_damage);

        GetComponent<Entity>().DamageImpact(); // do the damage
        fx.StartFlashFX();

        if (currentHealth <= 0 && !IsDead)
        {
            Die();
        }
    }
    public virtual void IncreaseHealthBy(int _amount)
    {
        currentHealth += _amount;
        if (currentHealth > GetMaxHealValue())
        {
            currentHealth = GetMaxHealValue();
        }
        if (onHealthChanged != null)
        {
            onHealthChanged();
        }
    }
    protected virtual void DecreaseHealthBy(int _damage)
    {
        // check if it is vulnerable
        if (isVulnerable)
        {
            _damage = Mathf.RoundToInt(_damage * 1.1f);
        }

        currentHealth -= _damage;
        if (onHealthChanged != null)
        {
            onHealthChanged();
        }
    }
    protected virtual void Die()
    {
        IsDead = true;
    }
    public virtual void DoDamage(CharacterStats _targetStats)
    {

        if (TargetCanAvoidAttack(_targetStats))
            return;


        int totalDamage = damage.GetValue() + strength.GetValue();
        if (CanCrit())
        {
            totalDamage = CalculateCriticalDamage(totalDamage);

        }

        totalDamage = CheckTargetArmor(_targetStats, totalDamage);

        _targetStats.TakeDamage(totalDamage);

        // if inventory current has fire element
        // then
        DoMagicalDamage(_targetStats); // remove if you don't want to apply magic hit on primary attack
    }

    #region Function for Skill Tree - Dodge Skill
    public virtual void OnEvasion()
    {

    }

    #endregion

    #region Major Stats Calculations
    protected bool TargetCanAvoidAttack(CharacterStats _targetStats)
    {
        int totalInvasion = _targetStats.evasion.GetValue() + _targetStats.agility.GetValue();
        if (isShocked)
            totalInvasion += 20;

        if (Random.Range(0, 100) < totalInvasion)
        {
            _targetStats.OnEvasion();
            return true;
        }
        return false;
    }
    protected int CheckTargetArmor(CharacterStats _targetStats, int totalDamage)
    {

        if (_targetStats.isChilled)
        {
            totalDamage -= Mathf.RoundToInt(_targetStats.armor.GetValue() * .8f);
        }
        else
        {
            totalDamage -= _targetStats.armor.GetValue();
        }

        totalDamage = Mathf.Clamp(totalDamage, 0, int.MaxValue);
        return totalDamage;
    }
    protected int CheckTargetResistance(CharacterStats _targetStats, int totalMagicalDamage)
    {
        totalMagicalDamage -= _targetStats.magicResistance.GetValue() + (_targetStats.intelligence.GetValue() * 3);
        totalMagicalDamage = Mathf.Clamp(totalMagicalDamage, 0, int.MaxValue);
        return totalMagicalDamage;
    }


    protected bool CanCrit()
    {
        int totalCriticalChance = critChance.GetValue() + agility.GetValue();
        if (Random.Range(0, 100) <= totalCriticalChance)
        {
            return true;
        }
        return false;
    }

    protected int CalculateCriticalDamage(int _damage)
    {
        float totalCritPower = (critPower.GetValue() + strength.GetValue()) * .01f;
        float critDamage = _damage * totalCritPower;

        return Mathf.RoundToInt(critDamage);

    }

    public int GetMaxHealValue()
    {
        return maxHealth.GetValue() + vitality.GetValue() * 5;
    }
    #endregion

    #region Magical Damage and Ailments
    public virtual void DoMagicalDamage(CharacterStats _targetStats)
    {
        int _fireDamage = fireDamage.GetValue();
        int _iceDamage = iceDamage.GetValue();
        int _lightningDamage = lightningDamage.GetValue();

        int totalMagicalDamage = _fireDamage + _iceDamage + _lightningDamage + intelligence.GetValue();
        totalMagicalDamage = CheckTargetResistance(_targetStats, totalMagicalDamage);

        _targetStats.TakeDamage(totalMagicalDamage);

        if (Mathf.Max(_fireDamage, _iceDamage, _lightningDamage) <= 0)
        {
            return;
        }

        AttemptToApplyAilments(_targetStats, _fireDamage, _iceDamage, _lightningDamage);

    }

    private void AttemptToApplyAilments(CharacterStats _targetStats, int _fireDamage, int _iceDamage, int _lightningDamage)
    {

        // elemental ailments
        bool canApplyIgnite = _fireDamage > _iceDamage && _fireDamage > _lightningDamage;
        bool canApplyChill = _iceDamage > _fireDamage && _iceDamage > _lightningDamage;
        bool canApplyShock = _lightningDamage > _fireDamage && _lightningDamage > _iceDamage;

        while (!canApplyIgnite && !canApplyChill && !canApplyShock)
        {
            if (Random.value < .3f && _fireDamage > 0)
            {
                canApplyIgnite = true;
                _targetStats.ApplyAilments(canApplyIgnite, canApplyChill, canApplyShock);
                Debug.Log("Applied fire");
                return;
            }

            if (Random.value < .5f && _iceDamage > 0)
            {
                canApplyChill = true;
                _targetStats.ApplyAilments(canApplyIgnite, canApplyChill, canApplyShock);
                Debug.Log("Applied ice");
                return;
            }
            if (Random.value < .5f && _lightningDamage > 0)
            {
                canApplyShock = true;
                _targetStats.ApplyAilments(canApplyIgnite, canApplyChill, canApplyShock);
                Debug.Log("Applied lightning");
                return;
            }
        }

        if (canApplyIgnite)
        {
            _targetStats.SetupIgniteDamage(Mathf.RoundToInt(_fireDamage * .2f));
        }

        if (canApplyShock)
        {
            _targetStats.SetupShockStrikeDamage(Mathf.RoundToInt(_lightningDamage * .1f));
        }
        _targetStats.ApplyAilments(canApplyIgnite, canApplyChill, canApplyShock);
    }


    public void ApplyAilments(bool _ignite, bool _chill, bool _shock)
    {
        bool canApplyIgnite = !isIgnited && !isChilled && !isShocked;
        bool canApplyChill = !isIgnited && !isChilled && !isShocked;
        bool canApplyShock = !isIgnited && !isChilled;

        //if (isIgnited || isChilled || isShocked)
        //    return;

        if (_ignite && canApplyIgnite)
        {
            isIgnited = _ignite;
            ignitedTimer = ailmentsDuration;
            fx.IgniteFxFor(ailmentsDuration);
        }

        if (_chill && canApplyChill)
        {
            chilledTimer = ailmentsDuration;
            isChilled = _chill;

            float slowPercentage = .2f;
            GetComponent<Entity>().SlowEntityBy(slowPercentage, ailmentsDuration);

            fx.ChillFxFor(ailmentsDuration);
        }
        if (_shock && canApplyShock)
        {
            if (!isShocked)
            {
                ApplyShock(_shock);
            }
            else
            {
                if (GetComponent<Player>() != null) return;

                HitNearestTargetWithShockStrike();
            }
        }
        isIgnited = _ignite;
        isChilled = _chill;
        isShocked = _shock;
    }

    public void ApplyShock(bool _shock)
    {
        if (isShocked) return;

        shockedTimer = ailmentsDuration;
        isShocked = _shock;
        fx.ShockFxFor(ailmentsDuration);
    }

    private void HitNearestTargetWithShockStrike()
    {
        // find closest target, only among the enemy
        // instantiate thunder strike
        // setup thunder strike
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 25f);

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null && Vector2.Distance(transform.position, hit.transform.position) > 1)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, hit.transform.position);
                if (distanceToEnemy < closestDistance)
                {
                    closestDistance = distanceToEnemy;
                    closestEnemy = hit.transform;
                }
            }

            if (closestEnemy == null)
            {
                closestEnemy = transform;
            }
        }

        if (closestEnemy != null)
        {
            GameObject newShockStrike = Instantiate(shockStrikePrefab, transform.position, Quaternion.identity);
            newShockStrike.GetComponent<ShockStrikeController>().Setup(shockDamage, closestEnemy.GetComponent<CharacterStats>());
        }
    }

    public void SetupIgniteDamage(int _damage) => igniteDamage = _damage;
    public void SetupShockStrikeDamage(int _damage) => shockDamage = _damage;
    private void ApplyIgniteDamage()
    {
        if (igniteDamageTimer < 0)
        {
            //Debug.Log("Take burn damage " + igniteDamage);
            DecreaseHealthBy(igniteDamage);

            if (currentHealth < 0 && !IsDead)
            {
                Die();
            }
            igniteDamageTimer = igniteDamageCooldown;
        }
    }
    #endregion

    public virtual void IncreaseStatBy(int _modifier, float _duration, Stat _statModify)
    {

        // Start coroutine for stat increase
        StartCoroutine(StatModCoroutine(_modifier, _duration, _statModify));
    }

    IEnumerator StatModCoroutine(int _modifier, float _duration, Stat _statModify)
    {
        _statModify.AddModifier(_modifier);
        yield return new WaitForSeconds(_duration);
        _statModify.RemoveModifier(_modifier);
    }
    public Stat GetStat(StatType _statType)
    {
        if (_statType == StatType.strength) return strength;
        else if (_statType == StatType.agility) return agility;
        else if (_statType == StatType.intelligence) return intelligence;
        else if (_statType == StatType.vitality) return agility;

        else if (_statType == StatType.damage) return damage;
        else if (_statType == StatType.critChance) return critChance;
        else if (_statType == StatType.critPower) return critPower;

        else if (_statType == StatType.maxHealth) return maxHealth;
        else if (_statType == StatType.armor) return armor;
        else if (_statType == StatType.evasion) return evasion;
        else if (_statType == StatType.magicResistance) return magicResistance;

        else if (_statType == StatType.fireDamage) return fireDamage;
        else if (_statType == StatType.iceDamage) return iceDamage;
        else if (_statType == StatType.lightningDamage) return lightningDamage;

        return null;
    }

    // Check if it is vulnerable
    public void MakeVulnerableFor(float _duration) => StartCoroutine(VulnerableForCoroutine(_duration));
    private IEnumerator VulnerableForCoroutine(float _duration) {
        isVulnerable = true;
        yield return new WaitForSeconds(_duration);
        isVulnerable = false;
    }
}
