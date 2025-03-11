using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StatSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UI ui;

    [SerializeField] private string statName;

    [SerializeField] private StatType statType;
    [SerializeField] private TextMeshProUGUI statValueText;
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private RectTransform uiRectTransform;

    [TextArea]
    [SerializeField] private string statDescription;

    private void OnValidate()
    {
        gameObject.name = "Stat - " + statName;

        if (statName != null)
        {
            statNameText.text = statName;
        }
    }

    private void Start()
    {
        UpdateStateValueUI();
        ui = GetComponentInParent<UI>();

    }
    public void UpdateStateValueUI()
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        if (playerStats != null)
        {
            statValueText.text = playerStats.GetStat(statType).GetValue().ToString();

            if (statType == StatType.maxHealth)
                statValueText.text = playerStats.GetMaxHealValue().ToString();

            if (statType == StatType.damage)
                statValueText.text = (playerStats.damage.GetValue() + playerStats.strength.GetValue()).ToString();

            if (statType == StatType.critPower)
                statValueText.text = (playerStats.critPower.GetValue() + playerStats.strength.GetValue()).ToString();

            if (statType == StatType.critChance)
                statValueText.text = (playerStats.critChance.GetValue() + playerStats.agility.GetValue()).ToString();


            if (statType == StatType.evasion)
                statValueText.text = (playerStats.evasion.GetValue() + playerStats.agility.GetValue()).ToString();

            if (statType == StatType.magicResistance)
                statValueText.text = (playerStats.magicResistance.GetValue() + (playerStats.intelligence.GetValue() * 3)).ToString();
        
        
        
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Show the tooltip first
        ui.statToolTip.ShowStatToolTip(statDescription);

        Vector2 mousePosition = Input.mousePosition;

        // Set the initial offsets
        float xOffset = 20; // Adjust to suit your needs
        float yOffset = 20; // Adjust to suit your needs

        // Get the tooltip's RectTransform
        RectTransform tooltipRect = ui.statToolTip.GetComponent<RectTransform>();

        // Calculate the tooltip's width and height
        float tooltipWidth = tooltipRect.rect.width;
        float tooltipHeight = tooltipRect.rect.height;

        // Calculate the new position
        float newXPos = mousePosition.x + xOffset;
        float newYPos = mousePosition.y - yOffset;

        // Adjust X position if tooltip is going off-screen
        if (newXPos + tooltipWidth > Screen.width)
        {
            newXPos = mousePosition.x - tooltipWidth - xOffset;
        }

        // Adjust Y position if tooltip is going off-screen
        if (newYPos - tooltipHeight < 0)
        {
            newYPos = mousePosition.y + tooltipHeight + yOffset;
        }

        // Set the new position
        ui.statToolTip.transform.position = new Vector2(newXPos, newYPos);
    }





    public void OnPointerExit(PointerEventData eventData)
    {
        ui.statToolTip.HideStatToolTip();
    }

}
