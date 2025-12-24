using System;
using System.Collections.Generic;
using UnityEngine;

// 单例管理背包与装备状态，提供事件给 UI 订阅
public class InventoryManager : MonoBehaviour
{
	public static InventoryManager Instance { get; private set; }

	[Header("背包设置")]
	public int maxSlots = 20;
	public List<Item> items = new List<Item>();

	[Header("装备栏（示例：武器、衣服、道具1、道具2）")]
	public Item weaponSlot;
	public Item clothingSlot;
	public Item[] extraEquipSlots = new Item[2];

	[Header("角色基础属性")]
	public int baseHP = 100;
	public int baseMP = 30;
	public int baseAttack = 10;
	// 经验值（用于 UI 的进度条示例）
	public int currentEXP;
	public int maxEXP;
	// 实时属性（用于演示使用道具时的当前血蓝值）
	public int currentHP;
	public int currentMP;

	// 事件：背包或装备变化，UI 可订阅刷新
	public event Action OnInventoryChanged;
	public event Action OnStatsChanged;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		Debug.Log("[InventoryManager] Awake - Instance set");
		// 不 DestroyOnLoad 以便编辑测试时可见，根据需要修改
	}
	private void Start()
	{
		// 初始化当前血蓝为总量
		currentHP = GetTotalHP();
		currentMP = GetTotalMP();
		OnStatsChanged?.Invoke();
	}

	#region 背包操作
	public bool AddItem(Item item)
	{
		if (item == null) return false;
		if (items.Count >= maxSlots) return false;
		items.Add(item);
		Debug.Log($"[InventoryManager] AddItem: {item?.itemName} (slots now {items.Count}/{maxSlots})");
		OnInventoryChanged?.Invoke();
		return true;
	}

	public bool RemoveItem(Item item)
	{
		bool removed = items.Remove(item);
		if (removed)
		{
			Debug.Log($"[InventoryManager] RemoveItem: {item?.itemName} (slots now {items.Count}/{maxSlots})");
			OnInventoryChanged?.Invoke();
		}
		else
		{
			Debug.LogWarning($"[InventoryManager] RemoveItem FAILED: {item?.itemName} not found");
		}
		return removed;
	}
	#endregion

	#region 直接装备到特定槽（供 UI 调用的更明确接口）
	public bool EquipToWeapon(Item item)
	{
		if (item == null || item.itemType != ItemType.Weapon) return false;
		// If same item already equipped, do nothing
		if (weaponSlot == item) return false;
		// If there's an existing weapon, return it to inventory
		if (weaponSlot != null)
		{
			bool returned = AddItem(weaponSlot);
			if (!returned)
			{
				Debug.LogWarning("[InventoryManager] EquipToWeapon failed - inventory full, cannot unequip current weapon");
				return false;
			}
			Debug.Log($"[InventoryManager] EquipToWeapon: returned previous {weaponSlot.itemName} to inventory");
		}
		// Equip new item (remove from inventory only if it exists there)
		weaponSlot = item;
		if (items.Contains(item)) RemoveItem(item);
		OnInventoryChanged?.Invoke();
		OnStatsChanged?.Invoke();
		Debug.Log($"[InventoryManager] EquipToWeapon: {item.itemName}");
		return true;
	}

	public bool EquipToClothing(Item item)
	{
		if (item == null || item.itemType != ItemType.Clothing) return false;
		if (clothingSlot == item) return false;
		if (clothingSlot != null)
		{
			bool returned = AddItem(clothingSlot);
			if (!returned)
			{
				Debug.LogWarning("[InventoryManager] EquipToClothing failed - inventory full, cannot unequip current clothing");
				return false;
			}
			Debug.Log($"[InventoryManager] EquipToClothing: returned previous {clothingSlot.itemName} to inventory");
		}
		clothingSlot = item;
		if (items.Contains(item)) RemoveItem(item);
		OnInventoryChanged?.Invoke();
		OnStatsChanged?.Invoke();
		Debug.Log($"[InventoryManager] EquipToClothing: {item.itemName}");
		return true;
	}

	public bool EquipToExtra(Item item)
	{
		if (item == null) return false;
		// If item already in an extra slot, do nothing
		for (int k = 0; k < extraEquipSlots.Length; k++)
		{
			if (extraEquipSlots[k] == item) return false;
		}
		// Put into first empty extra slot
		for (int i = 0; i < extraEquipSlots.Length; i++)
		{
			if (extraEquipSlots[i] == null)
			{
				extraEquipSlots[i] = item;
				if (items.Contains(item)) RemoveItem(item);
				OnInventoryChanged?.Invoke();
				OnStatsChanged?.Invoke();
				Debug.Log($"[InventoryManager] EquipToExtra[{i}]: {item.itemName}");
				return true;
			}
		}
		// If full, swap with slot 0 (return previous to inventory first)
		var prev = extraEquipSlots[0];
		if (prev != null)
		{
			bool returned = AddItem(prev);
			if (!returned)
			{
				Debug.LogWarning("[InventoryManager] EquipToExtra failed - inventory full, cannot unequip previous extra slot 0");
				return false;
			}
		}
		extraEquipSlots[0] = item;
		if (items.Contains(item)) RemoveItem(item);
		OnInventoryChanged?.Invoke();
		OnStatsChanged?.Invoke();
		Debug.Log($"[InventoryManager] EquipToExtra[0] replace: {item.itemName}");
		return true;
	}

	public bool UnequipExtraSlot(int index)
	{
		if (index < 0 || index >= extraEquipSlots.Length) return false;
		if (extraEquipSlots[index] == null) return false;
		bool added = AddItem(extraEquipSlots[index]);
		if (!added) return false;
		Debug.Log($"[InventoryManager] UnequipExtraSlot[{index}]: {extraEquipSlots[index].itemName}");
		extraEquipSlots[index] = null;
		OnInventoryChanged?.Invoke();
		OnStatsChanged?.Invoke();
		return true;
	}
	#endregion

	#region 使用道具
	// 使用一个道具（只处理 Consumable 类型）
	public bool UseItem(Item item)
	{
		if (item == null) return false;
		if (item.itemType != ItemType.Consumable)
		{
			Debug.LogWarning($"[InventoryManager] UseItem: {item.itemName} is not consumable");
			return false;
		}
		// 增加当前属性并限制到最大值
		currentHP = Mathf.Min(GetTotalHP(), currentHP + item.hpBonus);
		currentMP = Mathf.Min(GetTotalMP(), currentMP + item.mpBonus);
		bool removed = RemoveItem(item);
		if (removed)
		{
			Debug.Log($"[InventoryManager] UseItem: {item.itemName} used. HP={currentHP}/{GetTotalHP()} MP={currentMP}/{GetTotalMP()}");
			OnStatsChanged?.Invoke();
			return true;
		}
		else
		{
			Debug.LogWarning($"[InventoryManager] UseItem failed to remove {item.itemName}");
			return false;
		}
	}

	// 查找背包中第一个可用的消耗品并使用
	public bool UseFirstConsumable()
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] != null && items[i].itemType == ItemType.Consumable)
			{
				return UseItem(items[i]);
			}
		}
		Debug.Log("[InventoryManager] UseFirstConsumable: none found");
		return false;
	}
	#endregion

	#region 装备操作
	// 简单按 ItemType 装备
	public bool EquipItem(Item item)
	{
		if (item == null)
		{
			Debug.LogWarning("[InventoryManager] EquipItem called with null item");
			return false;
		}
		Debug.Log($"[InventoryManager] EquipItem start: {item.itemName} type:{item.itemType}");
		switch (item.itemType)
		{
			case ItemType.Weapon:
				weaponSlot = item;
				break;
			case ItemType.Clothing:
				clothingSlot = item;
				break;
			case ItemType.Misc:
			case ItemType.Consumable:
				// 放到额外两个槽的第一个空位
				for (int i = 0; i < extraEquipSlots.Length; i++)
				{
					if (extraEquipSlots[i] == null)
					{
						extraEquipSlots[i] = item;
						goto afterEquip;
					}
				}
				// 如果没有空位，替换第0位
				extraEquipSlots[0] = item;
				break;
			default:
				Debug.LogWarning($"[InventoryManager] EquipItem unsupported type: {item.itemType}");
				return false;
		}
	afterEquip:
		// 从背包移除（如果存在）
		bool removed = RemoveItem(item);
		if (!removed)
		{
			// 可能是直接从外部创建并装备的，仍然允许
			Debug.Log($"[InventoryManager] EquipItem: {item.itemName} was not in inventory but equipped anyway");
		}
		Debug.Log($"[InventoryManager] EquipItem end: weapon:{weaponSlot?.itemName} clothing:{clothingSlot?.itemName}");
		OnInventoryChanged?.Invoke();
		OnStatsChanged?.Invoke();
		return true;
	}

	public bool UnequipWeapon()
	{
		if (weaponSlot == null) return false;
		bool added = AddItem(weaponSlot);
		if (!added)
		{
			Debug.LogWarning("[InventoryManager] UnequipWeapon failed - inventory full");
			return false; // 背包满时不卸下
		}
		Debug.Log($"[InventoryManager] UnequipWeapon: {weaponSlot.itemName} -> back to inventory");
		weaponSlot = null;
		OnInventoryChanged?.Invoke();
		OnStatsChanged?.Invoke();
		return true;
	}

	public bool UnequipClothing()
	{
		if (clothingSlot == null) return false;
		bool added = AddItem(clothingSlot);
		if (!added)
		{
			Debug.LogWarning("[InventoryManager] UnequipClothing failed - inventory full");
			return false;
		}
		Debug.Log($"[InventoryManager] UnequipClothing: {clothingSlot.itemName} -> back to inventory");
		clothingSlot = null;
		OnInventoryChanged?.Invoke();
		OnStatsChanged?.Invoke();
		return true;
	}
	#endregion

	#region 统计属性
	public int GetTotalHP()
	{
		int total = baseHP;
		if (weaponSlot != null) total += weaponSlot.hpBonus;
		if (clothingSlot != null) total += clothingSlot.hpBonus;
		foreach (var e in extraEquipSlots) if (e != null) total += e.hpBonus;
		return total;
	}
	public int GetTotalMP()
	{
		int total = baseMP;
		if (weaponSlot != null) total += weaponSlot.mpBonus;
		if (clothingSlot != null) total += clothingSlot.mpBonus;
		foreach (var e in extraEquipSlots) if (e != null) total += e.mpBonus;
		return total;
	}
	public int GetTotalAttack()
	{
		int total = baseAttack;
		if (weaponSlot != null) total += weaponSlot.attackBonus;
		if (clothingSlot != null) total += clothingSlot.attackBonus;
		foreach (var e in extraEquipSlots) if (e != null) total += e.attackBonus;
		return total;
	}
	#endregion
}


