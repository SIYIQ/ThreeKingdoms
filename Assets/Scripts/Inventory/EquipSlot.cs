using UnityEngine;
using UnityEngine.UI;

public class EquipSlot : MonoBehaviour
{
    public ItemType allowedType;
    public Image icon;
    public Sprite emptySprite;

    public ItemData CurrentItem { get; private set; }

    public void SetItem(ItemData data)
    {
        CurrentItem = data;
        if (icon != null)
        {
            icon.sprite = data != null ? data.icon : emptySprite;
            icon.color = data != null ? Color.white : new Color(1f, 1f, 1f, 0.6f);
        }
    }

    public void Clear()
    {
        SetItem(null);
    }
}


