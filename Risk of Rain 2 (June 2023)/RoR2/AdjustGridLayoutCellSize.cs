using UnityEngine;
using UnityEngine.UI;

namespace RoR2;

[RequireComponent(typeof(GridLayoutGroup))]
[ExecuteAlways]
public class AdjustGridLayoutCellSize : MonoBehaviour
{
	public enum ExpandSetting
	{
		X,
		Y,
		Both
	}

	public ExpandSetting expandingSetting;

	public GridLayoutGroup gridlayout;

	private int maxConstraintCount;

	private RectTransform layoutRect;

	private void Update()
	{
		UpdateCellSize();
	}

	private void UpdateCellSize()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)gridlayout))
		{
			maxConstraintCount = gridlayout.constraintCount;
			layoutRect = ((Component)gridlayout).gameObject.GetComponent<RectTransform>();
			float num = (float)(maxConstraintCount - 1) * gridlayout.spacing.x + (float)((LayoutGroup)gridlayout).padding.left + (float)((LayoutGroup)gridlayout).padding.right;
			float num2 = (float)(maxConstraintCount - 1) * gridlayout.spacing.y + (float)((LayoutGroup)gridlayout).padding.top + (float)((LayoutGroup)gridlayout).padding.bottom;
			Rect rect = layoutRect.rect;
			float width = ((Rect)(ref rect)).width;
			rect = layoutRect.rect;
			float height = ((Rect)(ref rect)).height;
			float num3 = width - num;
			float num4 = height - num2;
			float x = num3 / (float)maxConstraintCount;
			float y = num4 / (float)maxConstraintCount;
			Vector2 cellSize = default(Vector2);
			((Vector2)(ref cellSize))._002Ector(gridlayout.cellSize.x, gridlayout.cellSize.y);
			if (expandingSetting == ExpandSetting.X || expandingSetting == ExpandSetting.Both)
			{
				cellSize.x = x;
			}
			if (expandingSetting == ExpandSetting.Y || expandingSetting == ExpandSetting.Both)
			{
				cellSize.y = y;
			}
			gridlayout.cellSize = cellSize;
		}
	}
}
