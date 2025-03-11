using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Thunder strike effect", menuName = "Data/Item Effect/Thunder Strike")]
public class ThunderStrikeEffect : ItemEffect
{
    [SerializeField] private GameObject thunderStrikePrefab;

    public override void ExecuteEffect(Transform _enemyTransform)
    {
        GameObject newThunderStrike = Instantiate(thunderStrikePrefab, _enemyTransform.position, Quaternion.identity);

        // TODO: setup thunders strike
        Destroy(newThunderStrike, 1);

    }
}
