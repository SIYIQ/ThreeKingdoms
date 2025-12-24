#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class InventoryEditorUtilities
{
    [MenuItem("Inventory/Create Slot Prefab and Sample Items")]
    public static void CreateSlotPrefabAndSamples()
    {
        // Ensure directories
        string prefabsDir = "Assets/Inventory/Prefabs";
        if (!Directory.Exists(prefabsDir)) Directory.CreateDirectory(prefabsDir);

        string resourcesItemsDir = "Assets/Resources/Inventory/Items";
        if (!Directory.Exists(resourcesItemsDir)) Directory.CreateDirectory(resourcesItemsDir);

        // Create a basic SlotPrefab (a Button with child Icon) in scene and save as prefab
        GameObject slot = new GameObject("SlotPrefab_Temp");
        var rt = slot.AddComponent<RectTransform>();
        var img = slot.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f);
        var btn = slot.AddComponent<UnityEngine.UI.Button>();
        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(slot.transform, false);
        var iconImg = icon.AddComponent<UnityEngine.UI.Image>();
        iconImg.color = Color.white;
        slot.AddComponent<InventorySlot>().icon = iconImg;
        slot.GetComponent<InventorySlot>().button = btn;

        string prefabPath = Path.Combine(prefabsDir, "SlotPrefab.prefab");
        PrefabUtility.SaveAsPrefabAsset(slot, prefabPath);

        // Also save a copy into Resources for runtime loading convenience
        string resourcesPrefabsDir = "Assets/Resources/Inventory/Prefabs";
        if (!Directory.Exists(resourcesPrefabsDir)) Directory.CreateDirectory(resourcesPrefabsDir);
        PrefabUtility.SaveAsPrefabAsset(slot, Path.Combine(resourcesPrefabsDir, "SlotPrefab.prefab"));

        GameObject.DestroyImmediate(slot);

        // Create several sample ItemData assets with generated colored textures
        CreateSampleItem("Sword", ItemType.Weapon, new Color(0.2f, 0.6f, 1f), resourcesItemsDir);
        CreateSampleItem("Armor", ItemType.Gear, new Color(1f, 0.6f, 0.2f), resourcesItemsDir);
        CreateSampleItem("Potion", ItemType.Consumable, new Color(0.2f, 1f, 0.4f), resourcesItemsDir);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created SlotPrefab and sample ItemData assets.");
    }

    static void CreateSampleItem(string name, ItemType type, Color color, string folder)
    {
        // Create texture
        Texture2D tex = new Texture2D(32, 32);
        Color[] cols = new Color[32 * 32];
        for (int i = 0; i < cols.Length; i++) cols[i] = color;
        tex.SetPixels(cols);
        tex.Apply();

        string texPath = Path.Combine(folder, name + "_tex.png");
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);
        TextureImporter ti = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.SaveAndReimport();
        }
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);

        // Create ItemData asset
        ItemData item = ScriptableObject.CreateInstance<ItemData>();
        item.itemName = name;
        item.itemType = type;
        item.icon = sprite;
        string assetPath = Path.Combine(folder, name + ".asset");
        AssetDatabase.CreateAsset(item, assetPath);
    }
}
#endif


