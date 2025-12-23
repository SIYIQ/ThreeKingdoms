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
		inventoryUI.hpText = hpTextGO.GetComponent<Text>();
		inventoryUI.mpText = mpTextGO.GetComponent<Text>();
		inventoryUI.attackText = atkTextGO.GetComponent<Text>();
		inventoryUI.weaponSlotImage = weaponSlot.GetComponent<Image>();
		inventoryUI.clothingSlotImage = clothSlot.GetComponent<Image>();
		inventoryUI.extraEquipSlotImages = new Image[2] { extra0.GetComponent<Image>(), extra1.GetComponent<Image>() };
		// 手动触发一次刷新（因为 InventoryUI 可能在被添加时还没收到字段引用）
		inventoryUI.RefreshAll();

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
		txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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

	private void Start()
	{
		if (buildOnStart) BuildDemoUI();
	}
}


