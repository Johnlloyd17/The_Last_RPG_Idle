using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Heal Effect", menuName = "Data/Item Effect/Heal Effect")]
public class HealEffect : ItemEffect
{

    [Range(0f, 1f)]
    [SerializeField] private float healPercent;
    public override void ExecuteEffect(Transform _enemyTransform)
    {
        // player stats
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        // how much to heal
        int healAmount = Mathf.RoundToInt(playerStats.GetMaxHealValue() * healPercent);

        // heal
        playerStats.IncreaseHealthBy(healAmount);
        
    }
}
