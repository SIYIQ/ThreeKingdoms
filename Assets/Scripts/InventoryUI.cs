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
	[Header("属性进度条")]
	public Image hpBar;
	public Image mpBar;
	public Image expBar;

	[Header("属性文本")]
	public Text hpText;
	public Text mpText;
	public Text attackText;

	private List<GameObject> createdSlots = new List<GameObject>();
	private List<Image> createdSlotIcons = new List<Image>();
	private List<GameObject> createdSlotSelection = new List<GameObject>();

	// tooltip
	private GameObject tooltipGO;
	private Text tooltipText;
	private int selectedIndex = -1;

	private void OnEnable()
	{
		if (InventoryManager.Instance != null)
		{
			InventoryManager.Instance.OnInventoryChanged += RefreshInventoryGrid;
			InventoryManager.Instance.OnInventoryChanged += RefreshEquipment;
			InventoryManager.Instance.OnStatsChanged += RefreshStats;
		}
		RefreshAll();
	}

	private void OnDisable()
	{
		if (InventoryManager.Instance != null)
		{
			InventoryManager.Instance.OnInventoryChanged -= RefreshInventoryGrid;
			InventoryManager.Instance.OnInventoryChanged -= RefreshEquipment;
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
			// 显示默认灰色底表示空装备槽
			img.sprite = null;
			img.color = new Color(0.25f, 0.25f, 0.25f, 1f);
		}
		else
		{
			if (item.icon != null)
			{
				img.sprite = item.icon;
				img.color = Color.white;
			}
			else
			{
				// 没有图标时用类型颜色填充
				img.sprite = null;
				img.color = GetColorForItemType(item.itemType);
			}
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
				// add outline/shadow for nicer visuals
				var bg = goObj.GetComponent<Image>();
				if (bg != null)
				{
					if (bg.GetComponent<Outline>() == null) bg.gameObject.AddComponent<Outline>().effectColor = new Color(0,0,0,0.6f);
					if (bg.GetComponent<Shadow>() == null) bg.gameObject.AddComponent<Shadow>().effectColor = new Color(0,0,0,0.4f);
				}
				// create selection overlay
				GameObject sel = new GameObject("Selection");
				sel.transform.SetParent(goObj.transform, false);
				var selRt = sel.AddComponent<RectTransform>();
				selRt.anchorMin = Vector2.zero;
				selRt.anchorMax = Vector2.one;
				selRt.offsetMin = Vector2.zero;
				selRt.offsetMax = Vector2.zero;
				var selImg = sel.AddComponent<Image>();
				selImg.color = new Color(1f, 0.9f, 0.2f, 0.18f);
				sel.SetActive(false);
				createdSlotSelection.Add(sel);
				// add SlotUI to handle hover/click
				var slotUI = goObj.GetComponent<SlotUI>();
				if (slotUI == null) slotUI = goObj.AddComponent<SlotUI>();
				slotUI.parentUI = this;
				slotUI.slotIndex = i;
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
		EnsureTooltipExists();

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
					// capture the item reference directly to avoid index mismatch
					var capturedItem = item;
					int captureIndex = i;
					btn.onClick.RemoveAllListeners();
					btn.onClick.AddListener(() =>
					{
						if (capturedItem == null)
						{
							Debug.LogWarning("[InventoryUI] clicked slot but item is null");
							return;
						}
						// Equip to the correct left slot by type using explicit API
						bool result = false;
						switch (capturedItem.itemType)
						{
							case ItemType.Weapon:
								result = InventoryManager.Instance?.EquipToWeapon(capturedItem) ?? false;
								break;
							case ItemType.Clothing:
								result = InventoryManager.Instance?.EquipToClothing(capturedItem) ?? false;
								break;
							case ItemType.Consumable:
							case ItemType.Misc:
								result = InventoryManager.Instance?.EquipToExtra(capturedItem) ?? false;
								break;
						}
						Debug.Log($"[InventoryUI] Equip attempt for {capturedItem.itemName} result={result}");
						RefreshAll();
					});
					// ensure selection overlay updated on click too
					btn.onClick.AddListener(() => { SelectSlot(captureIndex); });
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

	private void EnsureTooltipExists()
	{
		if (tooltipGO != null) return;
		Transform root = rootPanel != null ? rootPanel.transform.parent : (itemGridParent != null ? itemGridParent.root : null);
		if (root == null) return;
		tooltipGO = new GameObject("Tooltip");
		tooltipGO.transform.SetParent(root, false);
		var bg = tooltipGO.AddComponent<Image>();
		bg.color = new Color(0f, 0f, 0f, 0.8f);
		// not block raycasts so it doesn't interfere with pointer events
		bg.raycastTarget = false;
		var cg = tooltipGO.AddComponent<CanvasGroup>();
		cg.blocksRaycasts = false;
		var rt = tooltipGO.GetComponent<RectTransform>();
		rt.sizeDelta = new Vector2(160, 28);
		tooltipText = new GameObject("Text").AddComponent<Text>();
		tooltipText.transform.SetParent(tooltipGO.transform, false);
		tooltipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		tooltipText.alignment = TextAnchor.MiddleCenter;
		tooltipText.color = Color.white;
		tooltipText.rectTransform.anchorMin = Vector2.zero;
		tooltipText.rectTransform.anchorMax = Vector2.one;
		tooltipText.rectTransform.offsetMin = Vector2.zero;
		tooltipText.rectTransform.offsetMax = Vector2.zero;
		tooltipText.raycastTarget = false;
		tooltipGO.SetActive(false);
	}

	public void ShowTooltip(string text)
	{
		EnsureTooltipExists();
		if (tooltipGO == null) return;
		tooltipText.text = text;
		tooltipGO.SetActive(true);
		// offset so the tooltip doesn't occlude pointer and cause OnPointerExit
		var pos = Input.mousePosition + new Vector3(16f, -16f, 0f);
		tooltipGO.GetComponent<RectTransform>().position = pos;
	}

	public void HideTooltip()
	{
		if (tooltipGO != null) tooltipGO.SetActive(false);
	}

	public void SelectSlot(int index)
	{
		if (selectedIndex == index)
		{
			// deselect
			if (selectedIndex >= 0 && selectedIndex < createdSlotSelection.Count)
				createdSlotSelection[selectedIndex].SetActive(false);
			selectedIndex = -1;
			return;
		}
		// clear previous
		if (selectedIndex >= 0 && selectedIndex < createdSlotSelection.Count)
			createdSlotSelection[selectedIndex].SetActive(false);
		selectedIndex = index;
		if (selectedIndex >= 0 && selectedIndex < createdSlotSelection.Count)
			createdSlotSelection[selectedIndex].SetActive(true);
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
		// update bars (guard divide by zero)
		if (hpBar != null)
		{
			int totalHP = mgr.GetTotalHP();
			hpBar.fillAmount = totalHP > 0 ? Mathf.Clamp01((float)mgr.currentHP / totalHP) : 0f;
		}
		if (mpBar != null)
		{
			int totalMP = mgr.GetTotalMP();
			mpBar.fillAmount = totalMP > 0 ? Mathf.Clamp01((float)mgr.currentMP / totalMP) : 0f;
		}
		if (expBar != null)
		{
			expBar.fillAmount = mgr.maxEXP > 0 ? Mathf.Clamp01((float)mgr.currentEXP / mgr.maxEXP) : 0f;
		}
		if (attackText != null) attackText.text = $"ATK: {mgr.GetTotalAttack()}";
	}
}


