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
		mgr.AddItem(new Item("i_sword_01", "短剑", ItemType.Weapon, hp:0, mp:0, atk:15) { icon = sampleWeaponIcon });
		Debug.Log("[TestInventorySetup] Added sample weapon 短剑");
		mgr.AddItem(new Item("i_cloth_01", "布衣", ItemType.Clothing, hp:10, mp:5, atk:0) { icon = sampleClothIcon });
		Debug.Log("[TestInventorySetup] Added sample clothing 布衣");
		mgr.AddItem(new Item("i_potion_01", "小红瓶", ItemType.Consumable) { icon = sampleItemIcon });
		Debug.Log("[TestInventorySetup] Added sample consumable 小红瓶");
		mgr.AddItem(new Item("i_misc_01", "奇怪的石头", ItemType.Misc) { icon = sampleItemIcon });
		Debug.Log("[TestInventorySetup] Added sample misc 奇怪的石头");

		// 设置基础属性（可在 Inspector 调整）
		mgr.baseHP = 311;
		mgr.baseMP = 314;
		mgr.baseAttack = 79;

		// 触发 UI 刷新（事件会在 AddItem 内触发）
	}
}


