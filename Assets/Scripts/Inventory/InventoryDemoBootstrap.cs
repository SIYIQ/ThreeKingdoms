using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Demo bootstrap to create a runnable scene UI at runtime for testing the inventory system.
// Attach this script to an empty GameObject in a new Scene, press Play to see the UI and test with I key.
public class InventoryDemoBootstrap : MonoBehaviour
{
    void Awake()
    {
        // Ensure there's a Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas("Canvas");

        // Create InventoryRoot panel
        GameObject inventoryRoot = CreateUIObject("InventoryRoot", canvas.transform);
        RectTransform invRt = inventoryRoot.AddComponent<RectTransform>();
        Image invImage = inventoryRoot.AddComponent<Image>();
        invImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        invRt.anchorMin = new Vector2(0f, 0f);
        invRt.anchorMax = new Vector2(1f, 1f);
        invRt.offsetMin = new Vector2(10f, 10f);
        invRt.offsetMax = new Vector2(-10f, -10f);

        // Add InventoryUI component
        InventoryUI inventoryUI = inventoryRoot.AddComponent<InventoryUI>();
        inventoryUI.inventoryRoot = inventoryRoot;

        // Left area - portrait and equip grid
        GameObject leftArea = CreateUIObject("LeftArea", inventoryRoot.transform);
        RectTransform leftRt = leftArea.AddComponent<RectTransform>();
        leftRt.anchorMin = new Vector2(0f, 0f);
        leftRt.anchorMax = new Vector2(0.55f, 1f);
        leftRt.offsetMin = new Vector2(10f, 10f);
        leftRt.offsetMax = new Vector2(-10f, -10f);

        // Portrait image (top-left)
        GameObject portraitGO = CreateUIObject("Portrait", leftArea.transform);
        RectTransform portraitRt = portraitGO.AddComponent<RectTransform>();
        portraitRt.anchorMin = new Vector2(0f, 0.55f);
        portraitRt.anchorMax = new Vector2(0.6f, 1f);
        portraitRt.offsetMin = new Vector2(10f, -10f);
        portraitRt.offsetMax = new Vector2(-10f, -10f);
        Image portraitImage = portraitGO.AddComponent<Image>();
        portraitImage.color = Color.gray;
        inventoryUI.portraitImage = portraitImage;

        // EquipGrid (2x2)
        GameObject equipGrid = CreateUIObject("EquipGrid", leftArea.transform);
        RectTransform equipRt = equipGrid.AddComponent<RectTransform>();
        equipRt.anchorMin = new Vector2(0.6f, 0.55f);
        equipRt.anchorMax = new Vector2(1f, 1f);
        equipRt.offsetMin = new Vector2(10f, -10f);
        equipRt.offsetMax = new Vector2(-10f, -10f);
        GridLayoutGroup equipLayout = equipGrid.AddComponent<GridLayoutGroup>();
        equipLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        equipLayout.constraintCount = 2;
        equipLayout.cellSize = new Vector2(64f, 64f);
        equipLayout.spacing = new Vector2(8f, 8f);

        // Create 4 EquipSlot objects and assign to inventoryUI
        inventoryUI.weaponSlot = CreateEquipSlot(equipGrid.transform, ItemType.Weapon, "WeaponSlot");
        inventoryUI.gearSlot = CreateEquipSlot(equipGrid.transform, ItemType.Gear, "GearSlot");
        inventoryUI.consumableSlotA = CreateEquipSlot(equipGrid.transform, ItemType.Consumable, "ConsumableA");
        inventoryUI.consumableSlotB = CreateEquipSlot(equipGrid.transform, ItemType.Consumable, "ConsumableB");

        // Left bottom - status bars
        GameObject statusArea = CreateUIObject("StatusArea", leftArea.transform);
        RectTransform statusRt = statusArea.AddComponent<RectTransform>();
        statusRt.anchorMin = new Vector2(0f, 0f);
        statusRt.anchorMax = new Vector2(1f, 0.55f);
        statusRt.offsetMin = new Vector2(10f, 10f);
        statusRt.offsetMax = new Vector2(-10f, -10f);
        VerticalLayoutGroup statusLayout = statusArea.AddComponent<VerticalLayoutGroup>();
        statusLayout.spacing = 12f;

        // HP, MP, EXP bars
        StatusBar hp = CreateStatusBar(statusArea.transform, "HP", Color.red);
        StatusBar mp = CreateStatusBar(statusArea.transform, "MP", Color.cyan);
        StatusBar exp = CreateStatusBar(statusArea.transform, "EXP", new Color(1f, 0.85f, 0f));

        hp.SetValue(80f, 100f);
        mp.SetValue(40f, 100f);
        exp.SetValue(30f, 100f);

        // Right area - tabs + grid
        GameObject rightArea = CreateUIObject("RightArea", inventoryRoot.transform);
        RectTransform rightRt = rightArea.AddComponent<RectTransform>();
        rightRt.anchorMin = new Vector2(0.55f, 0f);
        rightRt.anchorMax = new Vector2(1f, 1f);
        rightRt.offsetMin = new Vector2(10f, 10f);
        rightRt.offsetMax = new Vector2(-10f, -10f);

        // Tabs row
        GameObject tabsRow = CreateUIObject("TabsRow", rightArea.transform);
        RectTransform tabsRt = tabsRow.AddComponent<RectTransform>();
        tabsRt.anchorMin = new Vector2(0f, 0.9f);
        tabsRt.anchorMax = new Vector2(1f, 1f);
        tabsRt.offsetMin = new Vector2(10f, -10f);
        tabsRt.offsetMax = new Vector2(-10f, -10f);
        HorizontalLayoutGroup tabsLayout = tabsRow.AddComponent<HorizontalLayoutGroup>();
        tabsLayout.spacing = 8f;

        Button tabEquip = CreateButton(tabsRow.transform, "EquipTab", "Equip");
        Button tabConsumable = CreateButton(tabsRow.transform, "ConsumableTab", "Items");
        inventoryUI.tabEquipButton = tabEquip;
        inventoryUI.tabConsumableButton = tabConsumable;

        // Grid area
        GameObject gridArea = CreateUIObject("GridArea", rightArea.transform);
        RectTransform gridRt = gridArea.AddComponent<RectTransform>();
        gridRt.anchorMin = new Vector2(0f, 0f);
        gridRt.anchorMax = new Vector2(1f, 0.9f);
        gridRt.offsetMin = new Vector2(10f, 10f);
        gridRt.offsetMax = new Vector2(-10f, -10f);
        GridLayoutGroup gridLayout = gridArea.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(64f, 64f);
        gridLayout.spacing = new Vector2(10f, 10f);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 5;
        inventoryUI.gridParent = gridArea.transform;

        // Create a simple slot prefab GameObject at runtime
        GameObject slotPrefab = CreateSlotPrefab();
        inventoryUI.slotPrefab = slotPrefab;
        inventoryUI.gridSlotCount = 20;
        inventoryUI.emptySlotSprite = GenerateColoredSprite(Color.grey);

        // Populate runtime items (ScriptableObject instances in memory)
        var sword = ScriptableObject.CreateInstance<ItemData>();
        sword.itemName = "Sword";
        sword.itemType = ItemType.Weapon;
        sword.icon = GenerateColoredSprite(new Color(0.2f, 0.6f, 1f));

        var armor = ScriptableObject.CreateInstance<ItemData>();
        armor.itemName = "Armor";
        armor.itemType = ItemType.Gear;
        armor.icon = GenerateColoredSprite(new Color(1f, 0.6f, 0.2f));

        var potion = ScriptableObject.CreateInstance<ItemData>();
        potion.itemName = "Potion";
        potion.itemType = ItemType.Consumable;
        potion.icon = GenerateColoredSprite(new Color(0.2f, 1f, 0.4f));

        // Add several items to inventory to test pagination
        inventoryUI.AddItemToInventory(sword);
        inventoryUI.AddItemToInventory(armor);
        for (int i = 0; i < 8; i++)
        {
            var p = ScriptableObject.CreateInstance<ItemData>();
            p.itemName = "Potion " + (i + 1);
            p.itemType = ItemType.Consumable;
            p.icon = GenerateColoredSprite(new Color(0.6f, 0.6f, 0.6f));
            inventoryUI.AddItemToInventory(p);
        }

        // Keep inventory hidden initially
        inventoryRoot.SetActive(false);
    }

    // -- Helper builders --
    Canvas CreateCanvas(string name)
    {
        GameObject go = new GameObject(name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    EquipSlot CreateEquipSlot(Transform parent, ItemType type, string name)
    {
        GameObject go = CreateUIObject(name, parent);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.5f, 0.5f, 0.5f);
        EquipSlot slot = go.AddComponent<EquipSlot>();
        slot.allowedType = type;
        slot.icon = img;
        slot.emptySprite = GenerateColoredSprite(Color.grey);
        return slot;
    }

    StatusBar CreateStatusBar(Transform parent, string label, Color fillColor)
    {
        GameObject go = CreateUIObject(label + "Bar", parent);
        HorizontalLayoutGroup hl = go.AddComponent<HorizontalLayoutGroup>();
        hl.spacing = 8f;
        GameObject labelGO = CreateUIObject("Label", go.transform);
        Text t = labelGO.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.text = label;
        t.color = Color.white;
        RectTransform fillRt = CreateUIObject("Fill", go.transform).AddComponent<RectTransform>();
        Image fill = fillRt.gameObject.AddComponent<Image>();
        fill.color = fillColor;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        StatusBar sb = go.AddComponent<StatusBar>();
        sb.fillImage = fill;
        sb.labelText = t;
        return sb;
    }

    Button CreateButton(Transform parent, string name, string text)
    {
        GameObject go = CreateUIObject(name, parent);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.8f, 0.8f, 0.8f);
        Button btn = go.AddComponent<Button>();
        GameObject txt = CreateUIObject("Text", go.transform);
        Text t = txt.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.text = text;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.black;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120f, 28f);
        return btn;
    }

    GameObject CreateSlotPrefab()
    {
        GameObject go = new GameObject("SlotPrefab");
        RectTransform rt = go.AddComponent<RectTransform>();
        Image bg = go.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.3f, 0.3f);
        Button btn = go.AddComponent<Button>();
        // Icon child
        GameObject iconGO = CreateUIObject("Icon", go.transform);
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = Color.white;
        InventorySlot slot = go.AddComponent<InventorySlot>();
        slot.icon = iconImg;
        slot.button = btn;
        return go;
    }

    // Generate a simple 32x32 colored sprite
    Sprite GenerateColoredSprite(Color c)
    {
        Texture2D tex = new Texture2D(32, 32);
        Color[] cols = new Color[32 * 32];
        for (int i = 0; i < cols.Length; i++) cols[i] = c;
        tex.SetPixels(cols);
        tex.Apply();
        Rect rect = new Rect(0, 0, tex.width, tex.height);
        return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f));
    }
}


