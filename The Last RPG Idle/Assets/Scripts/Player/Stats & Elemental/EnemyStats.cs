using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : CharacterStats
{
    private Enemy enemy;
    private ItemDrops myDropSystem;


    [Header("Level details")]
    [SerializeField] private int level = 1;

    [Range(0f, 1f)]
    [SerializeField] private float percentageModifier = .4f;


    protected override void Start()
    {
        ApplyLevelModifier();

        base.Start();
        CheckComponents();

    }

    private void ApplyLevelModifier()
    {
        Modify(strength);
        Modify(agility);
        Modify(intelligence);
        Modify(vitality);

        Modify(damage);
        Modify(critChance);
        Modify(critPower);

        Modify(maxHealth);
        Modify(armor);
        Modify(evasion);
        Modify(magicResistance);

        Modify(fireDamage);
        Modify(iceDamage);
        Modify(lightningDamage);
    }

    public override void TakeDamage(int _damage)
    {
        base.TakeDamage(_damage);
        //enemy.DamageEffect();
    }
    protected override void Die()
    {
        base.Die();
        enemy.Die();

        myDropSystem.GenerateDrop();
    }
    private void Modify(Stat _stat) {
        for (int i = 1; i < level; i++)
        {
            float modifier = _stat.GetValue() * percentageModifier;
            _stat.AddModifier(Mathf.RoundToInt(modifier));
        }
    }
    #region Checking Components / GameObject
    private void CheckComponents()
    {
        enemy = GetComponent<Enemy>();
        myDropSystem = GetComponent<ItemDrops>();


        DefensiveStatement();
    }
    private void DefensiveStatement()
    {
        if (enemy == null) Debug.LogError("Enemy component not found on this GameObject!");
        if (myDropSystem == null) Debug.LogError("ItemDrop component not found on this GameObject!");
    }
    #endregion
}
