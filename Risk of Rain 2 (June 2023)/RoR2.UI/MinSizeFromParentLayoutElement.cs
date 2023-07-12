using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class MinSizeFromParentLayoutElement : MonoBehaviour, ILayoutElement
{
	public bool useParentWidthAsMinWidth = true;

	public bool useParentHeightAsMinHeight = true;

	[SerializeField]
	private int _layoutPriority = 1;

	public float minWidth { get; protected set; } = -1f;


	public float preferredWidth { get; } = -1f;


	public float flexibleWidth { get; } = -1f;


	public float minHeight { get; protected set; } = -1f;


	public float preferredHeight { get; } = -1f;


	public float flexibleHeight { get; } = -1f;


	public int layoutPriority
	{
		get
		{
			return _layoutPriority;
		}
		set
		{
			_layoutPriority = value;
		}
	}

	public void CalculateLayoutInputHorizontal()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!useParentWidthAsMinWidth)
		{
			minWidth = -1f;
			return;
		}
		Transform parent = ((Component)this).transform.parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		float num;
		if (!Object.op_Implicit((Object)(object)val))
		{
			num = -1f;
		}
		else
		{
			Rect rect = val.rect;
			num = ((Rect)(ref rect)).width;
		}
		minWidth = num;
	}

	public void CalculateLayoutInputVertical()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!useParentHeightAsMinHeight)
		{
			minHeight = -1f;
			return;
		}
		Transform parent = ((Component)this).transform.parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		float num;
		if (!Object.op_Implicit((Object)(object)val))
		{
			num = -1f;
		}
		else
		{
			Rect rect = val.rect;
			num = ((Rect)(ref rect)).height;
		}
		minHeight = num;
	}
}
