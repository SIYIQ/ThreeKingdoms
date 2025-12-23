using UnityEditor;
using UnityEngine;

// 编辑器工具：在不移动资源的前提下，从项目中查找匹配的 sprite/texture 并注入到 DemoBuilder / TestInventorySetup
public static class AutoInjectDemoSprites
{
    [MenuItem("Tools/Auto Inject Demo Sprites")]
    public static void AutoInject()
    {
        // 需要在场景中存在 DemoBuilder 和 TestInventorySetup
        var demo = Object.FindObjectOfType<InventoryDemoBuilder>();
        var test = Object.FindObjectOfType<TestInventorySetup>();
        if (demo == null && test == null)
        {
            Debug.LogError("[AutoInjectDemoSprites] No InventoryDemoBuilder or TestInventorySetup found in scene. Please add DemoBuilder to scene.");
            return;
        }

        // 要查找的资源名（优先级）
        var names = new System.Collections.Generic.Dictionary<string, string>
        {
            {"weapon","arms"},
            {"cloth","clothes"},
            {"item","blue"},
            {"red","red"},
            {"character","character"}
        };

        var found = new System.Collections.Generic.Dictionary<string, Sprite>();

        foreach (var pair in names)
        {
            string key = pair.Key;
            string wanted = pair.Value;
            // 查找 Sprite 类型
            string[] guids = AssetDatabase.FindAssets(wanted + " t:Sprite");
            Sprite s = null;
            if (guids != null && guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (s != null) found[key] = s;
            }
            if (s == null)
            {
                // 查找 Texture2D 并尝试加载/create sprite
                guids = AssetDatabase.FindAssets(wanted + " t:Texture2D");
                if (guids != null && guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (tex != null)
                    {
                        s = Sprite.Create(tex, new Rect(0,0,tex.width, tex.height), new Vector2(0.5f,0.5f), 100f);
                        found[key] = s;
                    }
                }
            }
            Debug.Log($"[AutoInjectDemoSprites] lookup {wanted} -> {(s!=null)}");
        }

        // 注入到 DemoBuilder 与 TestInventorySetup
        if (demo != null)
        {
            if (found.ContainsKey("weapon")) demo.demoWeaponSprite = found["weapon"];
            if (found.ContainsKey("cloth")) demo.demoClothSprite = found["cloth"];
            if (found.ContainsKey("item")) demo.demoItemSprite = found["item"];
            if (found.ContainsKey("character")) demo.demoCharacterSprite = found["character"];
            EditorUtility.SetDirty(demo);
            Debug.Log("[AutoInjectDemoSprites] Injected sprites into DemoBuilder (if found).");
        }
        if (test != null)
        {
            if (found.ContainsKey("weapon")) test.sampleWeaponIcon = found["weapon"];
            if (found.ContainsKey("cloth")) test.sampleClothIcon = found["cloth"];
            if (found.ContainsKey("item")) test.sampleItemIcon = found["item"];
            EditorUtility.SetDirty(test);
            // call setup so runtime reflects new sprites immediately in editor play
            test.SetupIconsAndItems();
            Debug.Log("[AutoInjectDemoSprites] Injected sprites into TestInventorySetup and invoked SetupIconsAndItems.");
        }

        // 刷新编辑器视图
        AssetDatabase.Refresh();
        Debug.Log("[AutoInjectDemoSprites] Done.");
    }
}


