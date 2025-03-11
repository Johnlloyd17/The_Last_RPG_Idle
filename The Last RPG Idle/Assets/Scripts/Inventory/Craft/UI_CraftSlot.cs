using UnityEngine.EventSystems;

public class UI_CraftSlot : UI_ItemSlot
{

    protected override void Start()
    {
        base.Start();


    }
    //private void OnEnable()
    //{
    //    UpdateSlot(item);
    //}
    public void SetupCraftSlot(ItemData_Equipment _data)
    {
        if (_data == null)
            return;

        // Reset the font size to its default value
        itemText.fontSize = 24f;

        item.data = _data;

        itemImage.sprite = _data.icon;
        itemText.text = _data.itemName;

        // Adjust font size if the text length is greater than 12
        if (itemText.text.Length > 12)
        {
            itemText.fontSize *= 0.9f;
        }

    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        //// Inventory craft item data
        //ItemData_Equipment craftData = item.data as ItemData_Equipment;
        //Inventory.instance.CanCraft(craftData, craftData.craftingMaterials);

        ui.craftWindow.SetupCraftWindow(item.data as ItemData_Equipment);
    }
}
