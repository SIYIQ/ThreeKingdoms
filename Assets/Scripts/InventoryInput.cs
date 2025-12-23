using UnityEngine;

// 处理打开/关闭背包和快速使用快捷键
public class InventoryInput : MonoBehaviour
{
	public KeyCode toggleKey = KeyCode.I;
	public KeyCode useConsumableKey1 = KeyCode.Alpha1;
	public KeyCode useConsumableKey2 = KeyCode.Alpha2;

	private InventoryUI inventoryUI;

	private void Start()
	{
		inventoryUI = FindObjectOfType<InventoryUI>();
		if (inventoryUI == null)
		{
			Debug.LogWarning("[InventoryInput] No InventoryUI found in scene.");
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(toggleKey))
		{
			ToggleInventory();
		}
		if (Input.GetKeyDown(useConsumableKey1))
		{
			InventoryManager.Instance?.UseFirstConsumable();
		}
		if (Input.GetKeyDown(useConsumableKey2))
		{
			// 第二键暂时复用第一个（可扩展）
			InventoryManager.Instance?.UseFirstConsumable();
		}
	}

	public void ToggleInventory()
	{
		if (inventoryUI == null || inventoryUI.rootPanel == null)
		{
			Debug.LogWarning("[InventoryInput] InventoryUI or rootPanel not assigned.");
			return;
		}
		bool newState = !inventoryUI.rootPanel.activeSelf;
		inventoryUI.rootPanel.SetActive(newState);
		Debug.Log($"[InventoryInput] Inventory toggled: {(newState ? "opened" : "closed")}");
	}
}


