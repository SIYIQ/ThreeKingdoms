using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;

// 负责把 InventoryManager 的数据渲染到屏幕上（简单示例）
public class InventoryUI : MonoBehaviour
{
	[Header("角色显示")]
	public Image characterImage; // 左上全身静态图

	[Header("装备槽")]
	public Image weaponSlotImage;
	public Image clothingSlotImage;
	public Image[] extraEquipSlotImages = new Image[4];
	[Header("默认装备槽贴图（空槽时按索引显示）")]
	public Sprite[] defaultEquipSlotSprites = new Sprite[4];

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
	// track screen size to detect resize
	private Vector2 lastScreenSize = Vector2.zero;
	[Header("可选背景与容器")]
	public RectTransform gridBackground; // 浅色底板（右侧）可指定用于同步大小
	public GameObject inventoryBackgroundPanel; // 整体背包底板（所有元素放在该对象下并通过 I 键切换）

	private void OnEnable()
	{
		EnsureInventoryBackgroundExists();
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

		SetImage(weaponSlotImage, mgr.weaponSlot, 0);
		SetImage(clothingSlotImage, mgr.clothingSlot, 1);
		for (int i = 0; i < extraEquipSlotImages.Length; i++)
		{
			var slotImg = extraEquipSlotImages[i];
			var item = (i < mgr.extraEquipSlots.Length) ? mgr.extraEquipSlots[i] : null;
			SetImage(slotImg, item, i);
		}
	}

	private void SetImage(Image img, Item item, int defaultSpriteIndex = -1)
	{
		if (img == null) return;
		if (item == null)
		{
			// 如果指定了默认贴图数组且索引有效，使用默认贴图
			if (defaultSpriteIndex >= 0 && defaultEquipSlotSprites != null && defaultSpriteIndex < defaultEquipSlotSprites.Length && defaultEquipSlotSprites[defaultSpriteIndex] != null)
			{
				img.sprite = defaultEquipSlotSprites[defaultSpriteIndex];
				img.color = Color.white;
			}
			else
			{
				// 显示默认灰色底表示空装备槽
				img.sprite = null;
				img.color = new Color(0.25f, 0.25f, 0.25f, 1f);
			}
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
		// 如果格子数量与期望不符，先清理并重建（支持在运行时修改 rows/cols）
		if (createdSlots.Count != totalSlots)
		{
			// 销毁旧对象
			for (int i = 0; i < createdSlots.Count; i++)
			{
				var g = createdSlots[i];
				if (g != null) Destroy(g);
			}
			createdSlots.Clear();
			createdSlotIcons.Clear();
			createdSlotSelection.Clear();
		}
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
		// 同步 grid 背景大小（若存在）
		if (gridBackground != null && itemGridParent != null)
		{
			var gridRt = itemGridParent.GetComponent<RectTransform>();
			if (gridRt != null)
			{
				// 将背景高度/宽度与网格父对象匹配（保持锚点不变）
				gridBackground.sizeDelta = new Vector2(gridRt.rect.width, gridRt.rect.height);
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

	private void EnsureInventoryBackgroundExists()
	{
		if (inventoryBackgroundPanel != null) return;
		// Prefer to wrap rootPanel if available, otherwise wrap itemGridParent
		Transform toWrap = null;
		if (rootPanel != null) toWrap = rootPanel.transform;
		else if (itemGridParent != null) toWrap = itemGridParent;
		if (toWrap == null) return;

		// 如果场景中已有名为 InventoryBackground 的对象，则复用
		var existing = GameObject.Find("InventoryBackground");
		if (existing != null)
		{
			inventoryBackgroundPanel = existing;
		}
		else
		{
			var bg = new GameObject("InventoryBackground");
			var rt = bg.AddComponent<RectTransform>();
			// make it stretch to parent canvas
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.sizeDelta = Vector2.zero;
			var img = bg.AddComponent<Image>();
			img.color = new Color(0.9f, 0.9f, 0.9f, 1f);
			inventoryBackgroundPanel = bg;
		}

		// Insert inventoryBackgroundPanel into hierarchy: make it parent of toWrap
		var invBgRt = inventoryBackgroundPanel.GetComponent<RectTransform>();
		if (invBgRt == null) invBgRt = inventoryBackgroundPanel.AddComponent<RectTransform>();

		// Preserve old parent
		var oldParent = toWrap.parent;
		inventoryBackgroundPanel.transform.SetParent(oldParent, false);
		// Move the target under our background
		toWrap.SetParent(inventoryBackgroundPanel.transform, false);
		// if gridBackground not set, try to find a child named "GridBackground" under the new parent
		if (gridBackground == null)
		{
			var found = inventoryBackgroundPanel.transform.Find("GridBackground");
			if (found != null) gridBackground = found.GetComponent<RectTransform>();
		}
	}

	public void ShowTooltip(string text)
	{
		EnsureTooltipExists();
		if (tooltipGO == null) return;
		tooltipText.text = text;
		tooltipGO.SetActive(true);
		// 将屏幕坐标转换到父 RectTransform 的本地坐标（兼容不同 Canvas renderMode）
		var parentRt = tooltipGO.transform.parent as RectTransform;
		if (parentRt == null)
		{
			// 回退到屏幕坐标（较少见）
			var pos = Input.mousePosition + new Vector3(16f, -16f, 0f);
			tooltipGO.GetComponent<RectTransform>().position = pos;
			return;
		}
		var canvas = tooltipGO.GetComponentInParent<Canvas>();
		Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, Input.mousePosition, cam, out localPoint);
		tooltipGO.GetComponent<RectTransform>().anchoredPosition = localPoint + new Vector2(16f, -16f);
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
	private void Update()
	{
		// 切换整体背包面板（按 I）
		// 优先切换 inventoryBackgroundPanel（如果存在），避免只切换 child 导致 parent 背景残留造成灰色遮挡
		if (Input.GetKeyDown(KeyCode.I))
		{
			// If rootPanel is child of inventoryBackgroundPanel, toggle the parent to avoid leaving background visible.
			if (inventoryBackgroundPanel != null && rootPanel != null && rootPanel.transform.IsChildOf(inventoryBackgroundPanel.transform))
			{
				inventoryBackgroundPanel.SetActive(!inventoryBackgroundPanel.activeSelf);
			}
			else if (inventoryBackgroundPanel != null && rootPanel == null)
			{
				inventoryBackgroundPanel.SetActive(!inventoryBackgroundPanel.activeSelf);
			}
			else if (rootPanel != null)
			{
				rootPanel.SetActive(!rootPanel.activeSelf);
			}
		}

		// 检测窗口尺寸变化并在变化时刷新网格与背景（简单实现）
		Vector2 screenSize = new Vector2(Screen.width, Screen.height);
		if (screenSize != lastScreenSize)
		{
			lastScreenSize = screenSize;
			RefreshInventoryGrid();
		}
	}

#if UNITY_EDITOR
	[ContextMenu("Auto Assign Default Equip Sprites")]
	private void AutoAssignDefaultSpritesEditor()
	{
		// 搜索 Assets/Textures 下的所有文件，尽量把它们以 Sprite 的形式分配到 defaultEquipSlotSprites
		string[] guids = AssetDatabase.FindAssets("", new[] { "Assets/Textures" });
		List<Sprite> found = new List<Sprite>();
		foreach (var g in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(g);
			if (string.IsNullOrEmpty(path)) continue;
			// 先尝试以 Sprite 加载
			var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
			if (sprite != null)
			{
				found.Add(sprite);
				continue;
			}
			// 若不是 Sprite，尝试把 Texture 的导入器设置为 Sprite 并重新导入后再加载
			var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
			if (tex == null) continue;
			var importer = AssetImporter.GetAtPath(path) as TextureImporter;
			if (importer != null && importer.textureType != TextureImporterType.Sprite)
			{
				importer.textureType = TextureImporterType.Sprite;
				importer.SaveAndReimport();
			}
			// 重新尝试加载为 Sprite（有时需要 AssetDatabase 刷新）
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
			if (sprite != null) found.Add(sprite);
		}

		if (defaultEquipSlotSprites == null || defaultEquipSlotSprites.Length < 4) defaultEquipSlotSprites = new Sprite[4];

		// 精确匹配文件名（更严格规则），优先使用确切关键字
		foreach (var s in found)
		{
			string n = s.name.ToLower();
			if (n.Contains("arms") || n.Contains("weapon") || n.Contains("arm"))
			{
				defaultEquipSlotSprites[0] = s;
				continue;
			}
			if (n.Contains("clothes") || n.Contains("cloth") || n.Contains("clothing"))
			{
				defaultEquipSlotSprites[1] = s;
				continue;
			}
			if (n.Contains("potion") || n.Contains("hp") || n.Contains("consume") || n.Contains("consumable"))
			{
				defaultEquipSlotSprites[2] = s;
				continue;
			}
			if (n.Contains("misc") || n.Contains("character") || n.Contains("misc1") || n.Contains("misc2") || n.Contains("red"))
			{
				defaultEquipSlotSprites[3] = s;
				continue;
			}
		}

		// 回退填充：如果某些槽仍为空，按顺序用找到的贴图填充
		int idx = 0;
		for (int i = 0; i < defaultEquipSlotSprites.Length; i++)
		{
			if (defaultEquipSlotSprites[i] == null)
			{
				while (idx < found.Count && found[idx] == null) idx++;
				if (idx < found.Count) defaultEquipSlotSprites[i] = found[idx++];
			}
		}

		EditorUtility.SetDirty(this);
		Debug.Log($"[InventoryUI] AutoAssignDefaultSpritesEditor finished; assigned {found.Count} textures (check Inspector).");
	}
#endif

#if UNITY_EDITOR
	private void OnValidate()
	{
		// 在编辑器中尽量自动分配贴图，减少手动操作
		AutoAssignDefaultSpritesEditor();
	}
#endif
}


