using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Button button;
    public Sprite emptySprite;

    public ItemData Item { get; private set; }
    InventoryUI parentUI;

    public void Init(InventoryUI parent, Sprite empty)
    {
        parentUI = parent;
        emptySprite = empty;
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
        UpdateVisual();
    }

    public void SetItem(ItemData data)
    {
        Item = data;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (icon == null) return;
        icon.sprite = Item != null ? Item.icon : emptySprite;
        icon.color = Item != null ? Color.white : new Color(1f, 1f, 1f, 0.6f);
    }

    void OnClick()
    {
        if (parentUI != null)
            parentUI.OnGridSlotClicked(this);
    }
}


