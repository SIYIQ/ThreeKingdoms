using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 运行时在场景中构建一个最小化的 Inventory UI 用于可视化测试
public class InventoryDemoBuilder : MonoBehaviour
{
	public bool buildOnStart = true;
	public bool use2D = true;

	[ContextMenu("Build Demo UI")]
	public void BuildDemoUI()
	{
		// InventoryManager（单例）
		GameObject mgrGO = new GameObject("InventoryManager");
		var mgr = mgrGO.AddComponent<InventoryManager>();

		// Canvas
		GameObject canvasGO = new GameObject("DemoCanvas");
		var canvas = canvasGO.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvasGO.AddComponent<CanvasScaler>();
		canvasGO.AddComponent<GraphicRaycaster>();

		// 创建简单 Camera（避免 Game 视图显示 "No cameras rendering"）
		GameObject camGO = new GameObject("Main Camera");
		var cam = camGO.AddComponent<Camera>();
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.backgroundColor = Color.black;
		camGO.tag = "MainCamera";
		camGO.transform.position = new Vector3(0, 0, -10);

		// EventSystem（如果场景里没有）
		if (FindObjectOfType<EventSystem>() == null)
		{
			GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
		}

		// 创建 itemSlotPrefab（运行时 prefab 模拟）
		GameObject itemSlotPrefab = new GameObject("ItemSlotPrefab");
		var rt = itemSlotPrefab.AddComponent<RectTransform>();
		rt.sizeDelta = new Vector2(64, 64);
		var bgImage = itemSlotPrefab.AddComponent<Image>();
		bgImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
		var btn = itemSlotPrefab.AddComponent<Button>();

		// Icon 子对象
		GameObject iconGO = new GameObject("Icon");
		iconGO.transform.SetParent(itemSlotPrefab.transform, false);
		var iconRt = iconGO.AddComponent<RectTransform>();
		iconRt.anchorMin = Vector2.zero;
		iconRt.anchorMax = Vector2.one;
		iconRt.offsetMin = new Vector2(6, 6);
		iconRt.offsetMax = new Vector2(-6, -6);
		var iconImage = iconGO.AddComponent<Image>();
		iconImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
		// Quantity 子对象（右下角）
		GameObject qtyGO = new GameObject("Quantity");
		qtyGO.transform.SetParent(itemSlotPrefab.transform, false);
		var qtyRt = qtyGO.AddComponent<RectTransform>();
		qtyRt.anchorMin = new Vector2(1f, 0f);
		qtyRt.anchorMax = new Vector2(1f, 0f);
		qtyRt.pivot = new Vector2(1f, 0f);
		qtyRt.anchoredPosition = new Vector2(-6f, 6f);
		qtyRt.sizeDelta = new Vector2(30, 20);
		var qtyText = qtyGO.AddComponent<Text>();
		qtyText.text = "";
		qtyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		qtyText.fontSize = 14;
		qtyText.alignment = TextAnchor.LowerRight;
		qtyText.color = Color.white;

		// 创建 itemGridParent 并放置在 Canvas 右侧
		GameObject gridGO = new GameObject("ItemGridParent");
		gridGO.transform.SetParent(canvasGO.transform, false);
		var gridRt = gridGO.AddComponent<RectTransform>();
		gridRt.anchorMin = new Vector2(0.55f, 0.1f);
		gridRt.anchorMax = new Vector2(0.95f, 0.9f);
		gridRt.offsetMin = Vector2.zero;
		gridRt.offsetMax = Vector2.zero;
		var gridBg = gridGO.AddComponent<Image>();
		gridBg.color = new Color(0.95f, 0.95f, 0.9f, 0.6f); // 淡色背景
		var gridLayout = gridGO.AddComponent<UnityEngine.UI.GridLayoutGroup>();
		gridLayout.cellSize = new Vector2(68, 68);
		gridLayout.spacing = new Vector2(6, 6);
		gridLayout.padding = new RectOffset(8, 8, 8, 8);
		gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
		gridLayout.constraintCount = 5;

		// 创建简单属性文本（左侧）
		GameObject statsGO = new GameObject("Stats");
		statsGO.transform.SetParent(canvasGO.transform, false);
		var statsRt = statsGO.AddComponent<RectTransform>();
		statsRt.anchorMin = new Vector2(0.05f, 0.6f);
		statsRt.anchorMax = new Vector2(0.35f, 0.95f);
		statsRt.offsetMin = Vector2.zero;
		statsRt.offsetMax = Vector2.zero;

		GameObject hpTextGO = CreateTextChild(statsGO.transform, "HP: 0", new Vector2(0.5f, 0.8f));
		GameObject mpTextGO = CreateTextChild(statsGO.transform, "MP: 0", new Vector2(0.5f, 0.5f));
		GameObject atkTextGO = CreateTextChild(statsGO.transform, "ATK: 0", new Vector2(0.5f, 0.2f));

		// 创建简单装备槽（左上）
		// 大头像（左上）
		GameObject charGO = new GameObject("CharacterImage");
		charGO.transform.SetParent(canvasGO.transform, false);
		var charRt = charGO.AddComponent<RectTransform>();
		charRt.anchorMin = new Vector2(0.05f, 0.55f);
		charRt.anchorMax = new Vector2(0.35f, 0.95f);
		charRt.offsetMin = Vector2.zero;
		charRt.offsetMax = Vector2.zero;
		var charImg = charGO.AddComponent<Image>();
		charImg.color = new Color(0.9f, 0.9f, 0.9f, 1f);

		GameObject equipGO = new GameObject("EquipSlots");
		equipGO.transform.SetParent(canvasGO.transform, false);
		var equipRt = equipGO.AddComponent<RectTransform>();
		equipRt.anchorMin = new Vector2(0.05f, 0.3f);
		equipRt.anchorMax = new Vector2(0.35f, 0.6f);
		equipRt.offsetMin = Vector2.zero;
		equipRt.offsetMax = Vector2.zero;

		GameObject weaponSlot = CreateImageChild(equipGO.transform, "WeaponSlot", new Vector2(0.2f, 0.7f));
		GameObject clothSlot = CreateImageChild(equipGO.transform, "ClothSlot", new Vector2(0.6f, 0.7f));
		GameObject extra0 = CreateImageChild(equipGO.transform, "Extra0", new Vector2(0.2f, 0.35f));
		GameObject extra1 = CreateImageChild(equipGO.transform, "Extra1", new Vector2(0.6f, 0.35f));

		// 创建 InventoryUI 对象并绑定字段
		GameObject uiGO = new GameObject("InventoryUI");
		uiGO.transform.SetParent(canvasGO.transform, false);
		var inventoryUI = uiGO.AddComponent<InventoryUI>();
		inventoryUI.itemGridParent = gridGO.transform;
		inventoryUI.itemSlotPrefab = itemSlotPrefab;
		inventoryUI.characterImage = charImg;
		inventoryUI.hpText = hpTextGO.GetComponent<Text>();
		inventoryUI.mpText = mpTextGO.GetComponent<Text>();
		inventoryUI.attackText = atkTextGO.GetComponent<Text>();
		inventoryUI.weaponSlotImage = weaponSlot.GetComponent<Image>();
		inventoryUI.clothingSlotImage = clothSlot.GetComponent<Image>();
		inventoryUI.extraEquipSlotImages = new Image[2] { extra0.GetComponent<Image>(), extra1.GetComponent<Image>() };
		// 创建一个根面板，将页面一分为二：左侧为人物+属性，右侧为背包格子
		GameObject rootPanel = new GameObject("InventoryRootPanel");
		rootPanel.transform.SetParent(canvasGO.transform, false);
		var rootRt = rootPanel.AddComponent<RectTransform>();
		rootRt.anchorMin = new Vector2(0.03f, 0.05f);
		rootRt.anchorMax = new Vector2(0.97f, 0.95f);
		rootRt.offsetMin = Vector2.zero;
		rootRt.offsetMax = Vector2.zero;
		var rootImg = rootPanel.AddComponent<Image>();
		// 更明显的底色，避免看起来太透明
		rootImg.color = new Color(0f, 0f, 0f, 0.95f);

		// 左侧面板（人物和属性），占 root 的左部分（留更多空间）
		GameObject leftPanel = new GameObject("LeftPanel");
		leftPanel.transform.SetParent(rootPanel.transform, false);
		var leftRt = leftPanel.AddComponent<RectTransform>();
		leftRt.anchorMin = new Vector2(0f, 0f);
		leftRt.anchorMax = new Vector2(0.58f, 1f);
		leftRt.offsetMin = Vector2.zero;
		leftRt.offsetMax = Vector2.zero;

		// 右侧面板（背包格子），占 root 的右部分
		GameObject rightPanel = new GameObject("RightPanel");
		rightPanel.transform.SetParent(rootPanel.transform, false);
		var rightRt = rightPanel.AddComponent<RectTransform>();
		rightRt.anchorMin = new Vector2(0.58f, 0f);
		rightRt.anchorMax = new Vector2(1f, 1f);
		rightRt.offsetMin = Vector2.zero;
		rightRt.offsetMax = Vector2.zero;

		// 将已有 UI 元素移到左右两侧面板
		gridGO.transform.SetParent(rightPanel.transform, false);
		// make grid fill rightPanel with padding
		gridRt.anchorMin = new Vector2(0.03f, 0.05f);
		gridRt.anchorMax = new Vector2(0.97f, 0.95f);
		gridRt.offsetMin = Vector2.zero;
		gridRt.offsetMax = Vector2.zero;
		// adjust grid look to fit screen (smaller cells)
		gridLayout.cellSize = new Vector2(72, 72);
		gridLayout.constraintCount = 5;
		statsGO.transform.SetParent(leftPanel.transform, false);
		uiGO.transform.SetParent(rightPanel.transform, false);
		// create leftTop container for char + equip
		GameObject leftTop = new GameObject("LeftTop");
		leftTop.transform.SetParent(leftPanel.transform, false);
		var leftTopRt = leftTop.AddComponent<RectTransform>();
		leftTopRt.anchorMin = new Vector2(0f, 0.45f);
		leftTopRt.anchorMax = new Vector2(1f, 1f);
		leftTopRt.offsetMin = Vector2.zero;
		leftTopRt.offsetMax = Vector2.zero;
		// parent char and equip into leftTop
		charGO.transform.SetParent(leftTop.transform, false);
		equipGO.transform.SetParent(leftTop.transform, false);
		// stats placed in bottom area
		statsGO.transform.SetParent(leftPanel.transform, false);

		// 在 leftTop 中调整 charGO 与 equipGO 的 Anchors（char 占左大区，equip 放右侧窄列）
		charRt.anchorMin = new Vector2(0f, 0f);
		charRt.anchorMax = new Vector2(0.72f, 1f);
		charRt.offsetMin = new Vector2(8, 8);
		charRt.offsetMax = new Vector2(-8, -8);
		equipRt.anchorMin = new Vector2(0.74f, 0.1f);
		equipRt.anchorMax = new Vector2(1f, 0.9f);
		equipRt.offsetMin = Vector2.zero;
		equipRt.offsetMax = Vector2.zero;
		// create 2x2 grid for equip slots inside equipGO
		var equipLayout = equipGO.AddComponent<UnityEngine.UI.GridLayoutGroup>();
		equipLayout.cellSize = new Vector2(56, 56);
		equipLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
		equipLayout.constraintCount = 2;
		equipLayout.spacing = new Vector2(6, 6);
		equipLayout.padding = new RectOffset(4, 4, 4, 4);
		// clear any previous children and create 4 equip slot images
		for (int i = equipGO.transform.childCount - 1; i >= 0; i--) DestroyImmediate(equipGO.transform.GetChild(i).gameObject);
		var eqSlots = new GameObject[4];
		var eqImgs = new Image[4];
		for (int i = 0; i < 4; i++)
		{
			var s = new GameObject("EquipSlot_" + i);
			s.transform.SetParent(equipGO.transform, false);
			var srt = s.AddComponent<RectTransform>();
			srt.sizeDelta = new Vector2(56, 56);
			var simg = s.AddComponent<Image>();
			simg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
			// add button so user can click to unequip
			var sbtn = s.AddComponent<UnityEngine.UI.Button>();
			eqSlots[i] = s;
			eqImgs[i] = simg;
			// add listener: unequip corresponding slot when clicked
			int capture = i;
			sbtn.onClick.AddListener(() =>
			{
				if (capture == 0) InventoryManager.Instance?.UnequipWeapon();
				else if (capture == 1) InventoryManager.Instance?.UnequipClothing();
				else InventoryManager.Instance?.UnequipExtraSlot(capture - 2);
			});
		}
		// bind to inventoryUI: weapon, clothing, extra0, extra1
		inventoryUI.weaponSlotImage = eqImgs[0];
		inventoryUI.clothingSlotImage = eqImgs[1];
		inventoryUI.extraEquipSlotImages = new Image[2] { eqImgs[2], eqImgs[3] };
		// statsGO 放在左侧底部区域
		statsRt.anchorMin = new Vector2(0f, 0f);
		statsRt.anchorMax = new Vector2(1f, 0.45f);
		statsRt.offsetMin = Vector2.zero;
		statsRt.offsetMax = Vector2.zero;

		// 把 inventoryUI 的 rootPanel 字段指向该面板，并初始关闭（按 I 打开）
		inventoryUI.rootPanel = rootPanel;
		rootPanel.SetActive(false);

		// 手动触发一次刷新（因为 InventoryUI 可能在被添加时还没收到字段引用）
		inventoryUI.RefreshAll();

		// 在左侧下方创建三条进度条（HP/MP/EXP）并使用 VerticalLayoutGroup 以保证规整
		GameObject barsGO = new GameObject("StatBars");
		barsGO.transform.SetParent(leftPanel.transform, false);
		var barsRt = barsGO.AddComponent<RectTransform>();
		barsRt.anchorMin = new Vector2(0.05f, 0.05f);
		barsRt.anchorMax = new Vector2(0.95f, 0.4f);
		barsRt.offsetMin = Vector2.zero;
		barsRt.offsetMax = Vector2.zero;
		var vlg = barsGO.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
		vlg.childControlHeight = false;
		vlg.childControlWidth = true;
		vlg.spacing = 8;

		// Helper to create a bar: background + fill + label
		System.Func<Transform, string, Color, Image> createBar = (parent, label, color) =>
		{
			GameObject bar = new GameObject(label + "_Bar");
			bar.transform.SetParent(parent, false);
			var brt = bar.AddComponent<RectTransform>();
			brt.sizeDelta = new Vector2(0, 34);
			var bg = bar.AddComponent<Image>();
			bg.color = new Color(0.2f, 0.15f, 0.12f, 1f);

			GameObject fill = new GameObject("Fill");
			fill.transform.SetParent(bar.transform, false);
			var frit = fill.AddComponent<RectTransform>();
			frit.anchorMin = new Vector2(0f, 0f);
			frit.anchorMax = new Vector2(1f, 1f);
			frit.offsetMin = new Vector2(4, 4);
			frit.offsetMax = new Vector2(-4, -4);
			var fimg = fill.AddComponent<Image>();
			fimg.color = color;
			fimg.type = Image.Type.Filled;
			fimg.fillMethod = Image.FillMethod.Horizontal;
			fimg.fillOrigin = 0;
			fimg.fillAmount = 0.5f;

			GameObject lbl = new GameObject("Label");
			lbl.transform.SetParent(bar.transform, false);
			var ltxt = lbl.AddComponent<Text>();
			ltxt.text = label;
			ltxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			ltxt.alignment = TextAnchor.MiddleLeft;
			ltxt.color = Color.white;
			ltxt.rectTransform.anchorMin = new Vector2(0f, 0f);
			ltxt.rectTransform.anchorMax = new Vector2(1f, 1f);
			ltxt.rectTransform.offsetMin = new Vector2(8, 0);
			ltxt.rectTransform.offsetMax = new Vector2(-8, 0);

			return fimg;
		};

		var hpFill = createBar(barsGO.transform, "HP", new Color(1f, 0.2f, 0.2f));
		var mpFill = createBar(barsGO.transform, "MP", new Color(0.2f, 0.6f, 1f));
		var expFill = createBar(barsGO.transform, "EXP", new Color(1f, 0.8f, 0.2f));
		inventoryUI.hpBar = hpFill;
		inventoryUI.mpBar = mpFill;
		inventoryUI.expBar = expFill;
		// 创建几个场景中的“拾取物”，用按钮点击拾取（便于演示）
		CreatePickupButton(canvasGO.transform, new Vector2(0.15f, 0.2f), "拾取: 小红瓶", "i_potion_01", ItemType.Consumable, 0, 50, 0);
		CreatePickupButton(canvasGO.transform, new Vector2(0.3f, 0.2f), "拾取: 短剑", "i_sword_02", ItemType.Weapon, 0, 0, 20);
		CreatePickupButton(canvasGO.transform, new Vector2(0.45f, 0.2f), "拾取: 布衣", "i_cloth_02", ItemType.Clothing, 10, 5, 0);

		// 添加 Input 控制器（处理打开背包和快捷键）
		var inputGO = new GameObject("InventoryInput");
		inputGO.AddComponent<InventoryInput>();

		// 创建一个简单的场景：2D 或 3D（根据 use2D）
		GameObject worldRoot = new GameObject("DemoWorld");
		if (use2D)
		{
			// 创建 2D 玩家（Sprite）和 2D 拾取物（Sprite）
			GameObject player2D = new GameObject("Player2D");
			player2D.transform.SetParent(worldRoot.transform, false);
			player2D.transform.position = new Vector3(-2f, 0f, 0f);
			var spr = player2D.AddComponent<SpriteRenderer>();
			// 尝试加载 character 图
			var charSprite = Resources.Load<Sprite>("Textures/character");
			if (charSprite != null)
			{
				spr.sprite = charSprite;
			}
			else
			{
				spr.sprite = CreateColoredSprite(48, 48, new Color(0.2f, 0.6f, 1f));
			}
			player2D.transform.localScale = Vector3.one * 0.5f;
			player2D.tag = "Player";
			var rb2d = player2D.AddComponent<Rigidbody2D>();
			rb2d.bodyType = RigidbodyType2D.Kinematic;
			var col2d = player2D.AddComponent<BoxCollider2D>();
			col2d.isTrigger = false;
			player2D.AddComponent<PlayerController>();

			GameObject pickup2D = new GameObject("Pickup_Potion2D");
			pickup2D.transform.SetParent(worldRoot.transform, false);
			pickup2D.transform.position = new Vector3(0f, 0f, 0f);
			var pspr = pickup2D.AddComponent<SpriteRenderer>();
			var potionSprite = Resources.Load<Sprite>("Textures/red");
			if (potionSprite != null) pspr.sprite = potionSprite;
			else pspr.sprite = CreateColoredSprite(32, 32, Color.red);
			pickup2D.transform.localScale = Vector3.one * 0.5f;
			var pcol = pickup2D.AddComponent<BoxCollider2D>();
			pcol.isTrigger = true;
			var pickupScript2 = pickup2D.AddComponent<PickupItem>();
			pickupScript2.id = "i_potion_world";
			pickupScript2.itemName = "小红瓶(场景)";
			pickupScript2.itemType = ItemType.Consumable;
			pickupScript2.mp = 30;
			pickupScript2.hp = 0;
		}
		else
		{
			// 3D fallback (cube)
			GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
			player.name = "Player";
			player.transform.SetParent(worldRoot.transform, false);
			player.transform.position = new Vector3(-2f, 0f, 0f);
			player.transform.localScale = Vector3.one * 0.5f;
			player.tag = "Player";
			var prb = player.AddComponent<Rigidbody>();
			prb.useGravity = false;
			player.AddComponent<Player3DController>();

			GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
			pickup.name = "Pickup_Potion";
			pickup.transform.SetParent(worldRoot.transform, false);
			pickup.transform.position = new Vector3(0f, 0f, 0f);
			pickup.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
			var prend = pickup.GetComponent<Renderer>();
			if (prend != null)
			{
				var potionTex = Resources.Load<Sprite>("Textures/red");
				if (potionTex != null)
				{
					// apply as material color fallback
					prend.material.color = Color.white;
					// can't set sprite on mesh renderer; keep red color
				}
				else
				{
					prend.material.color = Color.red;
				}
			}
			var pc = pickup.GetComponent<Collider>();
			if (pc != null) pc.isTrigger = true;
			var pickupScript = pickup.AddComponent<PickupItem>();
			pickupScript.id = "i_potion_world";
			pickupScript.itemName = "小红瓶(场景)";
			pickupScript.itemType = ItemType.Consumable;
			pickupScript.mp = 30;
			pickupScript.hp = 0;
		}

		// 额外调试信息，输出 items 与生成格子数（如果有）
		int itemCount = InventoryManager.Instance != null ? InventoryManager.Instance.items.Count : 0;
		Debug.Log($"[InventoryDemoBuilder] Demo UI built. Inventory items count: {itemCount}");
		if (inventoryUI.itemGridParent != null)
		{
			Debug.Log($"[InventoryDemoBuilder] ItemGridParent childCount = {inventoryUI.itemGridParent.childCount}");
		}
		Debug.Log("[InventoryDemoBuilder] Demo UI built. Play the scene and check Console for Inventory logs.");
	}

	private GameObject CreateTextChild(Transform parent, string text, Vector2 anchor)
	{
		GameObject go = new GameObject("Text");
		go.transform.SetParent(parent, false);
		var rt = go.AddComponent<RectTransform>();
		rt.anchorMin = anchor;
		rt.anchorMax = anchor;
		rt.sizeDelta = new Vector2(200, 30);
		var txt = go.AddComponent<Text>();
		txt.text = text;
		// Unity 部分版本不再包含 Arial.ttf，使用 LegacyRuntime.ttf 更通用
		Font builtin = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		if (builtin == null)
		{
			Debug.LogWarning("[InventoryDemoBuilder] LegacyRuntime.ttf not found; text may not render.");
		}
		txt.font = builtin;
		txt.fontSize = 18;
		txt.alignment = TextAnchor.MiddleCenter;
		txt.color = Color.white;
		return go;
	}

	private GameObject CreateImageChild(Transform parent, string name, Vector2 anchor)
	{
		GameObject go = new GameObject(name);
		go.transform.SetParent(parent, false);
		var rt = go.AddComponent<RectTransform>();
		rt.anchorMin = anchor;
		rt.anchorMax = anchor;
		rt.sizeDelta = new Vector2(64, 64);
		var img = go.AddComponent<Image>();
		img.color = new Color(0.2f, 0.2f, 0.2f, 1f);
		return go;
	}

	private void CreatePickupButton(Transform parent, Vector2 anchor, string label, string id, ItemType type, int hp, int mp, int atk)
	{
		GameObject go = new GameObject("Pickup_" + id);
		go.transform.SetParent(parent, false);
		var rt = go.AddComponent<RectTransform>();
		rt.anchorMin = anchor;
		rt.anchorMax = anchor;
		rt.sizeDelta = new Vector2(120, 40);
		var img = go.AddComponent<Image>();
		img.color = new Color(0.8f, 0.4f, 0.1f, 1f);

		var btn = go.AddComponent<UnityEngine.UI.Button>();
		// 文本
		GameObject txt = CreateTextChild(go.transform, label, new Vector2(0.5f, 0.5f));
		txt.GetComponent<UnityEngine.UI.Text>().fontSize = 14;

		btn.onClick.AddListener(() =>
		{
			var item = new Item(id, label, type, hp, mp, atk);
			bool added = InventoryManager.Instance?.AddItem(item) ?? false;
			Debug.Log($"[Pickup] {label} picked, added={added}");
		});
	}

	private Sprite CreateColoredSprite(int w, int h, Color c)
	{
		Texture2D tex = new Texture2D(w, h);
		Color[] cols = new Color[w * h];
		for (int i = 0; i < cols.Length; i++) cols[i] = c;
		tex.SetPixels(cols);
		tex.Apply();
		return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
	}

	private void Start()
	{
		if (buildOnStart) BuildDemoUI();
	}
}


