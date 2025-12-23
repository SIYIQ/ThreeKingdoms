using System;
using UnityEngine;

[Serializable]
public enum ItemType
{
	Weapon,
	Clothing,
	Consumable,
	Misc
}

[Serializable]
public class Item
{
	// 唯一 id（可用于保存/加载）
	public string id;
	public string itemName;
	public ItemType itemType;
	public Sprite icon;

	// 简单属性加成（示例）
	public int hpBonus;
	public int mpBonus;
	public int attackBonus;

	// 无参构造用于序列化 / 在编辑器创建示例数据
	public Item() { }

	public Item(string id, string name, ItemType type, int hp = 0, int mp = 0, int atk = 0)
	{
		this.id = id;
		this.itemName = name;
		this.itemType = type;
		this.hpBonus = hp;
		this.mpBonus = mp;
		this.attackBonus = atk;
	}
}


