using UnityEngine;
using UnityEngine.UI;

namespace RoR2;

public class ShrinkWrapLayoutElement : MonoBehaviour, ILayoutElement
{
	public float minWidth { get; private set; }

	public float preferredWidth { get; private set; }

	public float flexibleWidth { get; private set; }

	public float minHeight { get; private set; }

	public float preferredHeight { get; private set; }

	public float flexibleHeight { get; private set; }

	public int layoutPriority { get; private set; }

	public void CalculateLayoutInputHorizontal()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		preferredWidth = 0f;
		if (((Component)this).transform.childCount != 0)
		{
			Rect rect = ((RectTransform)((Component)this).transform.GetChild(0)).rect;
			int i = 1;
			for (int childCount = ((Component)this).transform.childCount; i < childCount; i++)
			{
				Rect rect2 = ((RectTransform)((Component)this).transform.GetChild(i)).rect;
				((Rect)(ref rect)).xMin = Mathf.Min(((Rect)(ref rect)).xMin, ((Rect)(ref rect2)).xMin);
				((Rect)(ref rect)).xMax = Mathf.Max(((Rect)(ref rect)).xMax, ((Rect)(ref rect2)).xMax);
			}
			minWidth = ((Rect)(ref rect)).width;
			preferredWidth = ((Rect)(ref rect)).width;
			flexibleWidth = 0f;
		}
	}

	public void CalculateLayoutInputVertical()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		preferredHeight = 0f;
		if (((Component)this).transform.childCount != 0)
		{
			Rect rect = ((RectTransform)((Component)this).transform.GetChild(0)).rect;
			int i = 1;
			for (int childCount = ((Component)this).transform.childCount; i < childCount; i++)
			{
				Rect rect2 = ((RectTransform)((Component)this).transform.GetChild(i)).rect;
				((Rect)(ref rect)).yMin = Mathf.Min(((Rect)(ref rect)).yMin, ((Rect)(ref rect2)).yMin);
				((Rect)(ref rect)).yMax = Mathf.Max(((Rect)(ref rect)).yMax, ((Rect)(ref rect2)).yMax);
			}
			minHeight = ((Rect)(ref rect)).height;
			preferredHeight = ((Rect)(ref rect)).height;
			flexibleHeight = 0f;
		}
	}
}
