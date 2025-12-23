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
			Debug.LogWarning("[InventoryInput] No InventoryUI found in Start(); will try again on input.");
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(toggleKey))
		{
			// Try to refresh reference if missing (DemoBuilder may create UI after this component)
			if (inventoryUI == null)
			{
				// First try to find active
				inventoryUI = FindObjectOfType<InventoryUI>();
				// If still null, try to find inactive instances as well
				if (inventoryUI == null)
				{
					var all = Resources.FindObjectsOfTypeAll(typeof(InventoryUI)) as InventoryUI[];
					if (all != null && all.Length > 0)
					{
						inventoryUI = all[0];
						Debug.Log("[InventoryInput] Found InventoryUI via Resources.FindObjectsOfTypeAll (inactive)");
					}
				}
				if (inventoryUI == null)
				{
					Debug.LogWarning("[InventoryInput] Toggle pressed but InventoryUI still not found.");
					return;
				}
			}
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
		// ensure we have valid refs
		if (inventoryUI == null)
		{
			inventoryUI = FindObjectOfType<InventoryUI>();
			if (inventoryUI == null)
			{
				Debug.LogWarning("[InventoryInput] ToggleInventory: InventoryUI not found.");
				return;
			}
		}
		if (inventoryUI.rootPanel == null)
		{
			Debug.LogWarning("[InventoryInput] ToggleInventory: rootPanel not assigned on InventoryUI.");
			return;
		}
		bool newState = !inventoryUI.rootPanel.activeSelf;
		inventoryUI.rootPanel.SetActive(newState);
		Debug.Log($"[InventoryInput] Inventory toggled: {(newState ? "opened" : "closed")}");
	}
}


