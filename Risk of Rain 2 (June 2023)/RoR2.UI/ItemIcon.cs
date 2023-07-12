using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class ItemIcon : MonoBehaviour
{
	public RawImage glowImage;

	public RawImage image;

	public TextMeshProUGUI stackText;

	public TooltipProvider tooltipProvider;

	private ItemIndex itemIndex;

	private int itemCount;

	public RectTransform rectTransform { get; private set; }

	private void Awake()
	{
		CacheRectTransform();
	}

	public void CacheRectTransform()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if (rectTransform == null)
		{
			rectTransform = (RectTransform)((Component)this).transform;
		}
	}

	public void SetItemIndex(ItemIndex newItemIndex, int newItemCount)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		if (itemIndex == newItemIndex && itemCount == newItemCount)
		{
			return;
		}
		itemIndex = newItemIndex;
		itemCount = newItemCount;
		string titleToken = "";
		string bodyToken = "";
		Color val = Color.white;
		Color bodyColor = default(Color);
		((Color)(ref bodyColor))._002Ector(0.6f, 0.6f, 0.6f, 1f);
		ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
		if ((Object)(object)itemDef != (Object)null)
		{
			image.texture = itemDef.pickupIconTexture;
			if (itemCount > 1)
			{
				((Behaviour)stackText).enabled = true;
				((TMP_Text)stackText).text = $"x{itemCount}";
			}
			else
			{
				((Behaviour)stackText).enabled = false;
			}
			titleToken = itemDef.nameToken;
			bodyToken = itemDef.pickupToken;
			val = Color32.op_Implicit(ColorCatalog.GetColor(itemDef.darkColorIndex));
		}
		if (Object.op_Implicit((Object)(object)glowImage))
		{
			((Graphic)glowImage).color = new Color(val.r, val.g, val.b, 0.75f);
		}
		if (Object.op_Implicit((Object)(object)tooltipProvider))
		{
			tooltipProvider.titleToken = titleToken;
			tooltipProvider.bodyToken = bodyToken;
			tooltipProvider.titleColor = val;
			tooltipProvider.bodyColor = bodyColor;
		}
	}
}
