using UnityEditor;
using UnityEngine;

public static class ItemSlotPrefabSaver
{
	[MenuItem("Tools/Save ItemSlot Prefab")]
	public static void SaveItemSlotPrefab()
	{
		// 查找场景中的 ItemSlotPrefab
		var prefabObj = GameObject.Find("ItemSlotPrefab");
		if (prefabObj == null)
		{
			Debug.LogError("[ItemSlotPrefabSaver] No GameObject named 'ItemSlotPrefab' found in scene. Run DemoBuilder first.");
			return;
		}

		string folder = "Assets/Prefabs";
		if (!AssetDatabase.IsValidFolder(folder))
		{
			AssetDatabase.CreateFolder("Assets", "Prefabs");
		}
		string path = folder + "/ItemSlotPrefab.prefab";
		var saved = PrefabUtility.SaveAsPrefabAsset(prefabObj, path, out bool success);
		if (success)
		{
			Debug.Log("[ItemSlotPrefabSaver] Saved prefab to " + path);
			Selection.activeObject = saved;
		}
		else
		{
			Debug.LogError("[ItemSlotPrefabSaver] Failed to save prefab.");
		}
	}
}


