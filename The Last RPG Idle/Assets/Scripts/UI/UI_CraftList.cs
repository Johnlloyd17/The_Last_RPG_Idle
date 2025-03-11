using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_CraftList : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    protected UI ui;

    [SerializeField] private Transform craftSlotParent;
    [SerializeField] private GameObject craftSlotPrefab;

    [SerializeField] private List<ItemData_Equipment> craftEquipment;

    [SerializeField] private string craftName;
    private void Start()
    {
        ui = GetComponentInParent<UI>();
        transform.parent.GetChild(0).GetComponent<UI_CraftList>().SetupCraftList();
        SetupDefaultCraftWindow();
    }



    public void SetupCraftList()
    {
        for (int i = 0; i < craftSlotParent.childCount; i++)
        {
            Destroy(craftSlotParent.GetChild(i).gameObject);
        }


        // Create new slots for each craftable equipment
        for (int i = 0; i < craftEquipment.Count; i++)
        {
            GameObject newSlot = Instantiate(craftSlotPrefab, craftSlotParent);
            newSlot.GetComponent<UI_CraftSlot>().SetupCraftSlot(craftEquipment[i]);

        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetupCraftList();
    }

    public void SetupDefaultCraftWindow()
    {
        if (craftEquipment != null) { 
            GetComponentInParent<UI>().craftWindow.SetupCraftWindow(craftEquipment[0]);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Show Craft list Tooltip first
        ui.craftListToolTip.ShowCraftListToolTip(craftName);

        Vector2 mousePosition = Input.mousePosition;

        // Set the initial offsets to move the tooltip away from the trigger area
        float xOffset = 30f; // Increased offset to avoid overlapping
        float yOffset = 30f; // Increased offset to avoid overlapping

        // Get the tooltip's RectTransform
        RectTransform tooltipRect = ui.craftListToolTip.GetComponent<RectTransform>();

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

        ui.craftListToolTip.transform.position = new Vector3(newXPos, newYPos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide the tooltip when the pointer exits the trigger area
        ui.craftListToolTip.HideCraftListTooltip();
    }

}
