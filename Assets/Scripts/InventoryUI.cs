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

	[Header("属性文本")]
	public Text hpText;
	public Text mpText;
	public Text attackText;

	private List<GameObject> createdSlots = new List<GameObject>();

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
		// 清理旧槽
		foreach (var go in createdSlots) Destroy(go);
		createdSlots.Clear();

		// 为每个背包物品创建一个槽（简单实现）
		foreach (var item in mgr.items)
		{
			var go = Instantiate(itemSlotPrefab, itemGridParent, false);
			createdSlots.Add(go);
			var img = go.GetComponentInChildren<Image>();
			if (img != null)
			{
				img.sprite = item.icon;
				img.color = item.icon == null ? new Color(1,1,1,0.5f) : Color.white;
			}
			// 点击时装备该道具（简单交互）
			var btn = go.GetComponentInChildren<Button>();
			if (btn != null)
			{
				btn.onClick.AddListener(() => {
					Debug.Log($"[InventoryUI] item slot clicked: {item?.itemName}");
					InventoryManager.Instance.EquipItem(item);
					RefreshAll();
				});
			}
		}
	}

	public void RefreshStats()
	{
		var mgr = InventoryManager.Instance;
		if (mgr == null) return;
		if (hpText != null) hpText.text = $"HP: {mgr.GetTotalHP()}";
		if (mpText != null) mpText.text = $"MP: {mgr.GetTotalMP()}";
		if (attackText != null) attackText.text = $"ATK: {mgr.GetTotalAttack()}";
	}
}


