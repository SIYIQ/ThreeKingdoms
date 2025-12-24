using UnityEngine;
using UnityEngine.UI;

// Attach this to a child GameObject under your inventory to create a configurable
// smaller background panel (anchors & color adjustable). InventoryUI will detect
// and toggle the GameObject along with the rest of the inventory.
[ExecuteInEditMode]
public class InventoryBackground : MonoBehaviour
{
	public Color backgroundColor = new Color(0.92f, 0.92f, 0.92f, 1f);
	// anchors define the area this background will cover relative to its parent canvas
	public Vector2 anchorMin = new Vector2(0.5f, 0.05f);
	public Vector2 anchorMax = new Vector2(0.98f, 0.95f);
	[Tooltip("If true and there's no Image on this GameObject, one will be added automatically.")]
	public bool ensureImage = true;

	private Image _image;
	private RectTransform _rt;

	private void Reset()
	{
		// sensible default that covers the right area (can be changed in Inspector)
		anchorMin = new Vector2(0.5f, 0.05f);
		anchorMax = new Vector2(0.98f, 0.95f);
		backgroundColor = new Color(0.92f, 0.92f, 0.92f, 1f);
	}

	private void OnValidate()
	{
		ApplySettings();
	}

	private void Awake()
	{
		ApplySettings();
	}

	private void ApplySettings()
	{
		_rt = GetComponent<RectTransform>();
		if (_rt == null) _rt = gameObject.AddComponent<RectTransform>();

		// set anchors and reset offsets so the rect fills the anchor rectangle
		_rt.anchorMin = anchorMin;
		_rt.anchorMax = anchorMax;
		_rt.offsetMin = Vector2.zero;
		_rt.offsetMax = Vector2.zero;

		_image = GetComponent<Image>();
		if (_image == null && ensureImage)
		{
			_image = gameObject.AddComponent<Image>();
			// default spriteless image uses color
			_image.raycastTarget = true;
		}
		if (_image != null)
		{
			_image.color = backgroundColor;
		}
	}
}


