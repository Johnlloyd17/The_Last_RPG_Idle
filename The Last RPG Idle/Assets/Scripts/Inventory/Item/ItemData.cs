using System.Text;
using UnityEngine;

public enum ItemType
{
    Material,
    Potion,
    Equipment
}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
    public ItemType itemType;
    public string itemName;
    public Sprite icon;

    [Range(0, 100)]
    public float dropChance;


    protected StringBuilder sb = new StringBuilder();

    private void OnEnable()
    {
        if (sb == null)
        {
            sb = new StringBuilder();
        }
    }
    public virtual string GetDescription() {
        return "";

    }

     
}
