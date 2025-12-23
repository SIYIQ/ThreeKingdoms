using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 运行时在场景中构建一个最小化的 Inventory UI 用于可视化测试
public class InventoryDemoBuilder : MonoBehaviour
{
	public bool buildOnStart = true;

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
		// 创建一个根面板，将装备/属性/背包网格放到该面板之下（用于打开/关闭）
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

		// 将已有 UI 元素移到 rootPanel 下
		gridGO.transform.SetParent(rootPanel.transform, false);
		statsGO.transform.SetParent(rootPanel.transform, false);
		equipGO.transform.SetParent(rootPanel.transform, false);
		uiGO.transform.SetParent(rootPanel.transform, false);
		charGO.transform.SetParent(rootPanel.transform, false);

		// 把 inventoryUI 的 rootPanel 字段指向该面板，并初始关闭（按 I 打开）
		inventoryUI.rootPanel = rootPanel;
		rootPanel.SetActive(false);

		// 手动触发一次刷新（因为 InventoryUI 可能在被添加时还没收到字段引用）
		inventoryUI.RefreshAll();

		// 创建几个场景中的“拾取物”，用按钮点击拾取（便于演示）
		CreatePickupButton(canvasGO.transform, new Vector2(0.15f, 0.2f), "拾取: 小红瓶", "i_potion_01", ItemType.Consumable, 0, 50, 0);
		CreatePickupButton(canvasGO.transform, new Vector2(0.3f, 0.2f), "拾取: 短剑", "i_sword_02", ItemType.Weapon, 0, 0, 20);
		CreatePickupButton(canvasGO.transform, new Vector2(0.45f, 0.2f), "拾取: 布衣", "i_cloth_02", ItemType.Clothing, 10, 5, 0);

		// 添加 Input 控制器（处理打开背包和快捷键）
		var inputGO = new GameObject("InventoryInput");
		inputGO.AddComponent<InventoryInput>();

		// 创建一个简单的 3D 场景：玩家与场景内拾取物（用于按 D 向前走并触发拾取）
		GameObject worldRoot = new GameObject("DemoWorld");
		// 创建玩家（立方体）
		GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
		player.name = "Player";
		player.transform.SetParent(worldRoot.transform, false);
		player.transform.position = new Vector3(-2f, 0f, 0f);
		player.transform.localScale = Vector3.one * 0.5f;
		player.tag = "Player";
		var prb = player.AddComponent<Rigidbody>();
		prb.useGravity = false;
		// 添加 3D 玩家控制器
		player.AddComponent<Player3DController>();

		// 创建场景中的拾取物（小红瓶），使用 3D cube 表示并附 PickupItem
		GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
		pickup.name = "Pickup_Potion";
		pickup.transform.SetParent(worldRoot.transform, false);
		pickup.transform.position = new Vector3(0f, 0f, 0f);
		pickup.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
		var prend = pickup.GetComponent<Renderer>();
		if (prend != null) prend.material.color = Color.red;
		// 将 collider 设置为 trigger 并添加 PickupItem 脚本
		var pc = pickup.GetComponent<Collider>();
		if (pc != null) pc.isTrigger = true;
		var pickupScript = pickup.AddComponent<PickupItem>();
		pickupScript.id = "i_potion_world";
		pickupScript.itemName = "小红瓶(场景)";
		pickupScript.itemType = ItemType.Consumable;
		pickupScript.mp = 30;
		pickupScript.hp = 0;

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

	private void Start()
	{
		if (buildOnStart) BuildDemoUI();
	}
}


