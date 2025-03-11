using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected Image itemImage;
    [SerializeField] protected TextMeshProUGUI itemText;

    protected UI ui;

    protected virtual void Start()
    {
        ui = GetComponentInParent<UI>();
    }

    public InventoryItem item;

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null)
            return;

        // By holding control and left click to remove an item
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Inventory.instance.RemoveItem(item.data);
            return;
        }

        if (item.data.itemType == ItemType.Equipment)
            Inventory.instance.EquipItem(item.data);

        ui.itemToolTip.HideToolTip();
    }

    public void UpdateSlot(InventoryItem _newItem)
    {
        item = _newItem;
        itemImage.color = Color.white;

        if (item != null)
        {
            itemImage.sprite = item.data.icon; // we assign the image to the icon of the item
            if (item.StackSize > 1)
            {
                itemText.text = item.StackSize.ToString();
            }
            else
            {
                itemText.text = "";
            }
        }
    }

    public void CleanUpSlot()
    {
        item = null;
        itemImage.sprite = null;
        itemImage.color = Color.clear;
        itemText.text = "";
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null || item.data == null)
            return;

        Vector2 mousePosition = Input.mousePosition;

        float xOffset = 20;
        float yOffset = 20;

        RectTransform tooltipRect = null;

        // Show appropriate tooltip based on item type
        if (item.data.itemType == ItemType.Equipment)
        {
            ui.itemToolTip.ShowToolTip(item.data as ItemData_Equipment);
            tooltipRect = ui.itemToolTip.GetComponent<RectTransform>();
        }
        else if (item.data.itemType == ItemType.Material)
        {
            ui.materialToolTip.ShowToolTip(item.data.itemName);
            tooltipRect = ui.materialToolTip.GetComponent<RectTransform>();
        }

        if (tooltipRect != null)
        {
            float tooltipWidth = tooltipRect.rect.width;
            float tooltipHeight = tooltipRect.rect.height;

            float newXPos = mousePosition.x + xOffset;
            float newYPos = mousePosition.y - yOffset;

            if (newXPos + tooltipWidth > Screen.width)
            {
                newXPos = mousePosition.x - tooltipWidth - xOffset;
            }

            // Ensure the tooltip doesn't go off-screen at the top
            if (newYPos - tooltipHeight < 0)
            {
                newYPos = mousePosition.y + tooltipHeight + yOffset;
            }

            // Ensure the tooltip doesn't go off-screen at the bottom
            if (newYPos > Screen.height - tooltipHeight)
            {
                newYPos = mousePosition.y - tooltipHeight - yOffset;
            }

            tooltipRect.transform.position = new Vector2(newXPos, newYPos);
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (item == null || item.data == null)
            return;

        if (item.data.itemType == ItemType.Equipment)
        {
            ui.itemToolTip.HideToolTip();
        }
        else if (item.data.itemType == ItemType.Material)
        {
            ui.materialToolTip.HideToolTip();
        }
    }
}
