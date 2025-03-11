using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<ItemData> startingEquipment;

    public List<InventoryItem> equipment;
    public Dictionary<ItemData_Equipment, InventoryItem> equipmentDictionary;

    public List<InventoryItem> inventory;
    public Dictionary<ItemData, InventoryItem> inventoryDictionary;

    public List<InventoryItem> stash;
    public Dictionary<ItemData, InventoryItem> stashDictionary;

    [Header("Inventory UI")]
    [SerializeField] private Transform inventorySlotParent;
    [SerializeField] private Transform stashSlotParent;
    [SerializeField] private Transform equipmentSlotParent;
    [SerializeField] private Transform statsSlotParent; // don't include



    private UI_ItemSlot[] inventoryItemSlot;
    private UI_ItemSlot[] stashItemSlot;
    private UI_EquipmentSlot[] equipmentSlot;
    private UI_StatSlot[] statSlot;

    [Header("Items cooldown")]
    private float lastTimeUseFlask;
    private float lastTimeUsedArmor;

    private float flaskCooldown;
    private float armorCooldown;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject); // destroy the duplications
    }

    #region Start & Update
    private void Start()
    {
        inventory = new List<InventoryItem>();
        inventoryDictionary = new Dictionary<ItemData, InventoryItem>();
        inventoryItemSlot = inventorySlotParent.GetComponentsInChildren<UI_ItemSlot>();

        stash = new List<InventoryItem>();
        stashDictionary = new Dictionary<ItemData, InventoryItem>();
        stashItemSlot = stashSlotParent.GetComponentsInChildren<UI_ItemSlot>();

        equipment = new List<InventoryItem>();
        equipmentDictionary = new Dictionary<ItemData_Equipment, InventoryItem>();
        equipmentSlot = equipmentSlotParent.GetComponentsInChildren<UI_EquipmentSlot>();


        // Stats Slot
        statSlot = statsSlotParent.GetComponentsInChildren<UI_StatSlot>();

        AddStartingItems();
        LogInventory();
    }

    private void AddStartingItems()
    {
        for (int i = 0; i < startingEquipment.Count; i++)
        {
            AddItem(startingEquipment[i]);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ItemData newItem = inventory[inventory.Count - 1].data;
            RemoveItem(newItem);
        }
    }
    #endregion

    public void EquipItem(ItemData _item)
    {
        ItemData_Equipment newEquipment = _item as ItemData_Equipment;
        InventoryItem newItem = new InventoryItem(newEquipment);

        ItemData_Equipment oldEquipment = null;

        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary)
        {
            if (item.Key.equipmentType == newEquipment.equipmentType)
            {
                oldEquipment = item.Key;
            }
        }
        if (oldEquipment != null)
        {
            UnequippedItem(oldEquipment);
            AddItem(oldEquipment);
        }

        equipment.Add(newItem);
        equipmentDictionary.Add(newEquipment, newItem);

        newEquipment.AddModifiers(); // each items is use then add modifiers

        RemoveItem(_item);
        UpdateSlotUI();
    }

    public void UnequippedItem(ItemData_Equipment itemToRemove)
    {
        if (equipmentDictionary.TryGetValue(itemToRemove, out InventoryItem value))
        {
            equipment.Remove(value);
            equipmentDictionary.Remove(itemToRemove);

            itemToRemove.RemoveModifiers(); // each items is use then remove modifiers
        }
    }

    private void UpdateSlotUI()
    {

        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary)
            {
                if (item.Key.equipmentType == equipmentSlot[i].slotType)
                {
                    equipmentSlot[i].UpdateSlot(item.Value);
                }
            }
        }

        #region Clean up slots
        for (int i = 0; i < inventoryItemSlot.Length; i++)
        {
            inventoryItemSlot[i].CleanUpSlot();
        }
        for (int i = 0; i < stashItemSlot.Length; i++)
        {
            stashItemSlot[i].CleanUpSlot();
        }
        #endregion

        for (int i = 0; i < inventory.Count; i++)
        {
            inventoryItemSlot[i].UpdateSlot(inventory[i]);
        }
        for (int i = 0; i < stash.Count; i++)
        {
            stashItemSlot[i].UpdateSlot(stash[i]);
        }

        //for (int i = 0; i < inventory.Count && i < inventoryItemSlot.Length; i++)
        //{
        //    inventoryItemSlot[i].UpdateSlot(inventory[i]);
        //}
        //for (int i = 0; i < stash.Count && i < stashItemSlot.Length; i++)
        //{
        //    stashItemSlot[i].UpdateSlot(stash[i]);
        //}

        UpdateStatsUI();
    }

    // updating stats for the Skill Tree
    public void UpdateStatsUI()
    {
        for (int i = 0; i < statSlot.Length; i++) // update info of stats in character in UI
        {
            statSlot[i].UpdateStateValueUI();
        }
    }

    public bool CanAddItem()
    {
        if (inventory.Count >= inventoryItemSlot.Length)
        {
            //Debug.Log("No more space.");
            return false;
        }
        return true;
    }
    public void AddItem(ItemData _item)
    {
        if (_item.itemType == ItemType.Equipment && CanAddItem())
            AddToInventory(_item);
        else if (_item.itemType == ItemType.Material)
            AddToStash(_item);
        else if (_item.itemType == ItemType.Potion)
            AddToInventory(_item);

        UpdateSlotUI();
    }

    private void AddToStash(ItemData _item)
    {
        if (stashDictionary.TryGetValue(_item, out InventoryItem value))
        {
            value.AddStack();
        }
        else
        {
            InventoryItem newItem = new InventoryItem(_item);
            stash.Add(newItem);
            stashDictionary.Add(_item, newItem);
        }
    }

    private void AddToInventory(ItemData _item)
    {
        if (inventoryDictionary.TryGetValue(_item, out InventoryItem value))
        {
            value.AddStack();
        }
        else
        {
            InventoryItem newItem = new InventoryItem(_item);
            inventory.Add(newItem);
            inventoryDictionary.Add(_item, newItem);
        }
    }

    public void RemoveItem(ItemData _item)
    {
        if (inventoryDictionary.TryGetValue(_item, out InventoryItem value))
        {
            if (value.StackSize <= 1)
            {
                inventory.Remove(value);
                inventoryDictionary.Remove(_item);
            }
            else
            {
                value.RemoveStack();
            }
        }


        if (stashDictionary.TryGetValue(_item, out InventoryItem stashValue))
        {
            if (stashValue.StackSize <= 1)
            {
                stash.Remove(stashValue);
                stashDictionary.Remove(_item);
            }
            else
            {
                stashValue.RemoveStack();
            }
        }
        UpdateSlotUI();
    }

    #region Logs
    public void LogInventory()
    {
        if (inventory == null) Debug.Log("Inventory list is not found.");
        if (inventoryDictionary == null) Debug.Log("Inventory dictionary is not found.");
        if (inventoryItemSlot == null) Debug.Log("Item Slot is not found.");

        if (stash == null) Debug.Log("Stash list is not found.");
        if (stashDictionary == null) Debug.Log("Stash dictionary is not found.");
        if (stashItemSlot == null) Debug.Log("Stash Item Slot is not found.");

        if (equipment == null) Debug.Log("Equipment list is not found.");
        if (equipmentDictionary == null) Debug.Log("Equipment dictionary is not found.");
        if (equipmentSlot == null) Debug.Log("Equipment Slot is not found.");
    }
    #endregion

    public bool CanCraft(ItemData_Equipment _itemToCraft, List<InventoryItem> _requirementMaterials)
    {

        List<InventoryItem> materialsToRemove = new List<InventoryItem>();

        for (int i = 0; i < _requirementMaterials.Count; i++)
        {
            if (stashDictionary.TryGetValue(_requirementMaterials[i].data, out InventoryItem stashValue))
            {
                // add this to used Material
                if (stashValue.StackSize < _requirementMaterials[i].StackSize)
                {
                    Debug.Log("Not enough materials.");

                    return false;
                }
                else
                {
                    materialsToRemove.Add(stashValue);
                }
            }
            else
            {
                Debug.Log("Not enough materials.");
                return false;
            }
        }

        for (int i = 0; i < materialsToRemove.Count; i++)
        {
            RemoveItem(materialsToRemove[i].data);
        }
        AddItem(_itemToCraft);
        Debug.Log("Here's your item - " + _itemToCraft.name);
        return true;
    }

    public List<InventoryItem> GetEquipmentList() => equipment;

    public List<InventoryItem> GetStashList() => stash;

    // TODO: Allow to get data of current item Equipped
    public ItemData_Equipment GetEquipment(EquipmentType _type)
    {
        ItemData_Equipment equippedItem = null;

        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary)
        {
            if (item.Key.equipmentType == _type)
            {
                equippedItem = item.Key;
            }
        }

        return equippedItem;
    }

    public void UseFlask()
    {

        ItemData_Equipment currentFlask = GetEquipment(EquipmentType.Flask);

        if (currentFlask == null)
            return;

        // if can use // cooldown
        bool canUseFlask = Time.time > lastTimeUseFlask + flaskCooldown;

        if (canUseFlask)
        {
            // use flask
            flaskCooldown = currentFlask.itemCooldown;
            currentFlask.Effect(null);
            lastTimeUseFlask = Time.time;
        }
        else
        {
            // set cooldown
            Debug.Log("Flask on cooldown.");
        }
    }


    // Can use armor
    public bool CanUseArmor() {
        ItemData_Equipment currentArmor = GetEquipment(EquipmentType.Armor);

        if (Time.time > lastTimeUsedArmor + armorCooldown)
        {
            armorCooldown = currentArmor.itemCooldown;
            lastTimeUsedArmor = Time.time;
            return true;
        }
        Debug.Log("Armor on cooldown.");
        return false;
    }
}
