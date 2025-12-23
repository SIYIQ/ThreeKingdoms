using UnityEngine;

// 用于在运行时快速填充背包与演示（编辑器中也可修改）
public class TestInventorySetup : MonoBehaviour
{
	public Sprite sampleWeaponIcon;
	public Sprite sampleClothIcon;
	public Sprite sampleItemIcon;

	private void Start()
	{
		var mgr = InventoryManager.Instance;
		if (mgr == null) return;

		Debug.Log("[TestInventorySetup] Start - populating sample items");
		// 清空并添加示例物品
		mgr.items.Clear();
		// 尝试从 Resources 或 Assets/Textures 加载示例图标（编辑器模式下使用 AssetDatabase 作为后备）
		Sprite sWeapon = sampleWeaponIcon ?? LoadSpriteByName("arms");
		Sprite sCloth = sampleClothIcon ?? LoadSpriteByName("clothes");
		Sprite sItem = sampleItemIcon ?? LoadSpriteByName("blue");

		mgr.AddItem(new Item("i_sword_01", "短剑", ItemType.Weapon, hp:0, mp:0, atk:15) { icon = sWeapon });
		Debug.Log("[TestInventorySetup] Added sample weapon 短剑");
		mgr.AddItem(new Item("i_cloth_01", "布衣", ItemType.Clothing, hp:10, mp:5, atk:0) { icon = sCloth });
		Debug.Log("[TestInventorySetup] Added sample clothing 布衣");
		mgr.AddItem(new Item("i_potion_01", "小红瓶", ItemType.Consumable) { icon = sItem });
		Debug.Log("[TestInventorySetup] Added sample consumable 小红瓶");
		mgr.AddItem(new Item("i_misc_01", "奇怪的石头", ItemType.Misc) { icon = sItem });
		Debug.Log("[TestInventorySetup] Added sample misc 奇怪的石头");

		// 设置基础属性（可在 Inspector 调整）
		mgr.baseHP = 311;
		mgr.baseMP = 314;
		mgr.baseAttack = 79;

		// 触发 UI 刷新（事件会在 AddItem 内触发）
	}

	private Sprite LoadSpriteByName(string name)
	{
		// 尝试从 Resources/Textures 加载
		var s = Resources.Load<Sprite>($"Textures/{name}");
#if UNITY_EDITOR
		if (s == null)
		{
			// 编辑器下尝试直接从 Assets/Textures 加载
			var path = $"Assets/Textures/{name}.png";
			s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
		}
#endif
		return s;
	}
}


