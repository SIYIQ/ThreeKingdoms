using UnityEngine;
using UnityEngine.EventSystems;

// 处理格子的悬停与点击回调
public class SlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public InventoryUI parentUI;
	public int slotIndex = -1;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (parentUI == null) return;
		// 获取物品名
		var mgr = InventoryManager.Instance;
		if (mgr != null && slotIndex >= 0 && slotIndex < mgr.items.Count && mgr.items[slotIndex] != null)
		{
			parentUI.ShowTooltip(mgr.items[slotIndex].itemName);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		parentUI?.HideTooltip();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		parentUI?.SelectSlot(slotIndex);
	}
}


