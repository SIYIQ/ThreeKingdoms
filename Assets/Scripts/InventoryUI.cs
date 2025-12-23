using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 负责把 InventoryManager 的数据渲染到屏幕上（简单示例）
public class InventoryUI : MonoBehaviour
{
	[Header("角色显示")]
	public Image characterImage; // 左上全身静态图

	[Header("装备槽")]
	public Image weaponSlotImage;
	public Image clothingSlotImage;
	public Image[] extraEquipSlotImages = new Image[2];

	[Header("背包网格")]
	public Transform itemGridParent;
	public GameObject itemSlotPrefab; // 需要一个包含 Image + Button 的预制件
	[Header("Grid 设置")]
	public int gridColumns = 5;
	public int gridRows = 4;
	[Header("根面板（用于打开/关闭背包）")]
	public GameObject rootPanel;

	[Header("属性文本")]
	public Text hpText;
	public Text mpText;
	public Text attackText;

	private List<GameObject> createdSlots = new List<GameObject>();
	private List<Image> createdSlotIcons = new List<Image>();

	private void OnEnable()
	{
		if (InventoryManager.Instance != null)
		{
			InventoryManager.Instance.OnInventoryChanged += RefreshInventoryGrid;
			InventoryManager.Instance.OnStatsChanged += RefreshStats;
		}
		RefreshAll();
	}

	private void OnDisable()
	{
		if (InventoryManager.Instance != null)
		{
			InventoryManager.Instance.OnInventoryChanged -= RefreshInventoryGrid;
			InventoryManager.Instance.OnStatsChanged -= RefreshStats;
		}
	}

	[ContextMenu("RefreshAll")]
	public void RefreshAll()
	{
		RefreshEquipment();
		RefreshInventoryGrid();
		RefreshStats();
	}

	public void RefreshEquipment()
	{
		var mgr = InventoryManager.Instance;
		if (mgr == null) return;

		SetImage(weaponSlotImage, mgr.weaponSlot);
		SetImage(clothingSlotImage, mgr.clothingSlot);
		for (int i = 0; i < extraEquipSlotImages.Length; i++)
		{
			var slotImg = extraEquipSlotImages[i];
			var item = (i < mgr.extraEquipSlots.Length) ? mgr.extraEquipSlots[i] : null;
			SetImage(slotImg, item);
		}
	}

	private void SetImage(Image img, Item item)
	{
		if (img == null) return;
		if (item == null)
		{
			img.sprite = null;
			img.color = new Color(1,1,1,0); // 隐藏
		}
		else
		{
			img.sprite = item.icon;
			img.color = Color.white;
		}
	}

	public void RefreshInventoryGrid()
	{
		var mgr = InventoryManager.Instance;
		if (mgr == null || itemSlotPrefab == null || itemGridParent == null) return;

		Debug.Log($"[InventoryUI] RefreshInventoryGrid - items count {mgr.items.Count}");
		// 如果还没创建固定槽，先创建 gridColumns * gridRows 个槽
		int totalSlots = Mathf.Max(1, gridColumns) * Mathf.Max(1, gridRows);
		if (createdSlots.Count == 0)
		{
			for (int i = 0; i < totalSlots; i++)
			{
				var goObj = Instantiate(itemSlotPrefab, itemGridParent, false);
				if (goObj == null) continue;
				goObj.name = $"ItemSlot_{i}";
				createdSlots.Add(goObj);
				// find icon image
				Image img = null;
				var iconTransform = goObj.transform.Find("Icon");
				if (iconTransform != null) img = iconTransform.GetComponent<Image>();
				if (img == null) img = goObj.GetComponentInChildren<Image>();
				createdSlotIcons.Add(img);
				// clear
				if (img != null)
				{
					img.sprite = null;
					img.color = new Color(1,1,1,0.2f);
				}
				// clear button listeners
				var btn = goObj.GetComponentInChildren<Button>();
				if (btn != null) btn.onClick.RemoveAllListeners();
			}
			Debug.Log($"[InventoryUI] Created fixed grid slots: {createdSlots.Count}");
		}

		// 将背包内的物品按顺序填充到固定格子上（若物品数超过格子数，多余物品暂不显示）
		for (int i = 0; i < createdSlots.Count; i++)
		{
			Image iconImage = (i < createdSlotIcons.Count) ? createdSlotIcons[i] : null;
			if (i < mgr.items.Count && mgr.items[i] != null)
			{
				var item = mgr.items[i];
				if (iconImage != null)
				{
					iconImage.sprite = item.icon;
					// 使用物品类型映射颜色填充背景效果（若没有 sprite，则用颜色块）
					iconImage.color = item.icon == null ? GetColorForItemType(item.itemType) : Color.white;
					// 额外：如果你希望用纯色覆盖背景，可设置父对象的 Image.color
				}
				// 点击格子装备或使用（示例：装备/使用）
				var btn = createdSlots[i].GetComponentInChildren<Button>();
				if (btn != null)
				{
					int captureIndex = i;
					btn.onClick.RemoveAllListeners();
					btn.onClick.AddListener(() =>
					{
						var it = InventoryManager.Instance.items[Mathf.Clamp(captureIndex, 0, InventoryManager.Instance.items.Count-1)];
						if (it.itemType == ItemType.Consumable)
						{
							InventoryManager.Instance.UseItem(it);
						}
						else
						{
							InventoryManager.Instance.EquipItem(it);
						}
						RefreshAll();
					});
				}
			}
			else
			{
				// 空槽
				if (iconImage != null)
				{
					iconImage.sprite = null;
					iconImage.color = new Color(1,1,1,0.2f);
				}
				var btn = createdSlots[i].GetComponentInChildren<Button>();
				if (btn != null) btn.onClick.RemoveAllListeners();
			}
		}
	}

	private Color GetColorForItemType(ItemType t)
	{
		switch (t)
		{
			case ItemType.Weapon: return new Color(1f, 0.6f, 0.2f, 1f); // 橙
			case ItemType.Clothing: return new Color(0.4f, 0.9f, 0.4f, 1f); // 绿
			case ItemType.Consumable: return new Color(0.4f, 0.6f, 1f, 1f); // 蓝
			case ItemType.Misc: default: return new Color(0.8f, 0.8f, 0.8f, 1f); // 灰
		}
	}

	public void RefreshStats()
	{
		var mgr = InventoryManager.Instance;
		if (mgr == null) return;
		if (hpText != null) hpText.text = $"HP: {mgr.currentHP}/{mgr.GetTotalHP()}";
		if (mpText != null) mpText.text = $"MP: {mgr.currentMP}/{mgr.GetTotalMP()}";
		if (attackText != null) attackText.text = $"ATK: {mgr.GetTotalAttack()}";
	}
}


