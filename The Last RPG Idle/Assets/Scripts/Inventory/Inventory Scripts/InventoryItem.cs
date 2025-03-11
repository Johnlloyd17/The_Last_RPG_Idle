using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class InventoryItem
{
    public ItemData data;
    public int StackSize;
    // This method which assign data with the new data
    public InventoryItem(ItemData _newItemData)
    {
        data = _newItemData;
        // TODO: Add to stack 
        AddStack();
    }
    public void AddStack () => StackSize++;
    public void RemoveStack () => StackSize--;

}
