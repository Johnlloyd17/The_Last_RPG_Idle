using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_ItemToolTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemDescription;


    public void ShowToolTip(ItemData_Equipment _item)
    {

        // Reset the font size to its default value
        itemNameText.fontSize = 32f;

        itemNameText.text = _item.itemName;
        itemTypeText.text = _item.equipmentType.ToString();
        itemDescription.text = _item.GetDescription();


        // Adjust font size if the text length is greater than 12
        if (itemNameText.text.Length > 12)
        {
            itemNameText.fontSize *= 0.9f;
        }

        gameObject.SetActive(true);
    }
    public void HideToolTip() => gameObject.SetActive(false);
}
