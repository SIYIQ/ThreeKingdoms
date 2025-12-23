using UnityEngine;

// 附在场景拾取物上：被玩家触碰后添加到背包并销毁自己
public class PickupItem : MonoBehaviour
{
	public string id = "i_potion_pick";
	public string itemName = "小红瓶";
	public ItemType itemType = ItemType.Consumable;
	public int hp = 0;
	public int mp = 50;
	public int attack = 0;

	private void Reset()
	{
		// 自动添加 2D/3D 碰撞体并设置为触发器，方便在不同演示方式下使用
		var col2d = GetComponent<Collider2D>();
		if (col2d == null) col2d = gameObject.AddComponent<BoxCollider2D>();
		col2d.isTrigger = true;
		var col3d = GetComponent<Collider>();
		if (col3d == null) col3d = gameObject.AddComponent<BoxCollider>();
		col3d.isTrigger = true;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other == null) return;
		if (!other.CompareTag("Player")) return;

		var item = new Item(id, itemName, itemType, hp, mp, attack);
		bool added = InventoryManager.Instance != null && InventoryManager.Instance.AddItem(item);
		Debug.Log($"[PickupItem] Player collided with {itemName}, added={added}");
		if (added)
		{
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other == null) return;
		if (!other.CompareTag("Player")) return;
		var item = new Item(id, itemName, itemType, hp, mp, attack);
		bool added = InventoryManager.Instance != null && InventoryManager.Instance.AddItem(item);
		Debug.Log($"[PickupItem] Player collided (3D) with {itemName}, added={added}");
		if (added)
		{
			Destroy(gameObject);
		}
	}
}


