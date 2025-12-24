using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject inventoryRoot; // 整个底板（按 I 键显示/隐藏）

    [Header("Left Top")]
    public Image portraitImage; // 立绘放这里

    [Header("Equip Slots (4)")]
    public EquipSlot weaponSlot; // allowedType = Weapon
    public EquipSlot gearSlot;   // allowedType = Gear
    public EquipSlot consumableSlotA; // allowedType = Consumable
    public EquipSlot consumableSlotB; // allowedType = Consumable

    [Header("Right Grid")]
    public GameObject slotPrefab;
    public Transform gridParent;
    public int gridSlotCount = 20;
    public Sprite emptySlotSprite;

    [Header("Tabs")]
    public Button tabEquipButton;
    public Button tabConsumableButton;

    [Header("Data")]
    public List<ItemData> inventoryItems = new List<ItemData>();

    List<InventorySlot> gridSlots = new List<InventorySlot>();
    enum Tab { Equipment, Consumables }
    Tab activeTab = Tab.Equipment;

    void Start()
    {
        if (inventoryRoot != null) inventoryRoot.SetActive(false);
        CreateGridSlots();
        if (tabEquipButton != null) tabEquipButton.onClick.AddListener(() => SwitchTab(Tab.Equipment));
        if (tabConsumableButton != null) tabConsumableButton.onClick.AddListener(() => SwitchTab(Tab.Consumables));
        RefreshGrid();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        if (inventoryRoot == null) return;
        bool show = !inventoryRoot.activeSelf;
        inventoryRoot.SetActive(show);
        if (show) RefreshGrid();
    }

    void CreateGridSlots()
    {
        if (slotPrefab == null || gridParent == null) return;
        // Clear existing children created previously
        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            // Use Destroy in runtime contexts; DestroyImmediate was editor-only.
            var child = gridParent.GetChild(i).gameObject;
            Destroy(child);
        }

        gridSlots.Clear();
        for (int i = 0; i < gridSlotCount; i++)
        {
            GameObject go = Instantiate(slotPrefab, gridParent);
            InventorySlot slot = go.GetComponent<InventorySlot>();
            if (slot != null)
            {
                slot.Init(this, emptySlotSprite);
                gridSlots.Add(slot);
            }
        }
    }

    public void OnGridSlotClicked(InventorySlot slot)
    {
        if (slot == null) return;
        if (slot.Item == null) return;
        ItemData item = slot.Item;
        if (item == null) return;

        switch (item.itemType)
        {
            case ItemType.Weapon:
                if (weaponSlot != null)
                {
                    weaponSlot.SetItem(item);
                    slot.SetItem(null);
                }
                break;
            case ItemType.Gear:
                if (gearSlot != null)
                {
                    gearSlot.SetItem(item);
                    slot.SetItem(null);
                }
                break;
            case ItemType.Consumable:
                if (consumableSlotA != null && consumableSlotA.CurrentItem == null)
                {
                    consumableSlotA.SetItem(item);
                    slot.SetItem(null);
                }
                else if (consumableSlotB != null && consumableSlotB.CurrentItem == null)
                {
                    consumableSlotB.SetItem(item);
                    slot.SetItem(null);
                }
                else
                {
                    Debug.Log("Consumable slots full");
                }
                break;
        }
    }

    void SwitchTab(Tab t)
    {
        activeTab = t;
        RefreshGrid();
    }

    void RefreshGrid()
    {
        List<ItemData> filtered = new List<ItemData>();
        foreach (var it in inventoryItems)
        {
            if (it == null) continue;
            if (activeTab == Tab.Equipment && (it.itemType == ItemType.Weapon || it.itemType == ItemType.Gear))
                filtered.Add(it);
            if (activeTab == Tab.Consumables && it.itemType == ItemType.Consumable)
                filtered.Add(it);
        }

        for (int i = 0; i < gridSlots.Count; i++)
        {
            if (i < filtered.Count)
                gridSlots[i].SetItem(filtered[i]);
            else
                gridSlots[i].SetItem(null);
        }
    }

    // Helper: add item to inventory and refresh
    public void AddItemToInventory(ItemData item)
    {
        if (item == null) return;
        inventoryItems.Add(item);
        RefreshGrid();
    }
}


