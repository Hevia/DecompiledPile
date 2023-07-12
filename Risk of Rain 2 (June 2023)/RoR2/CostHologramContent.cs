using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2;

public class CostHologramContent : MonoBehaviour
{
	public int displayValue;

	public TextMeshPro targetTextMesh;

	public CostTypeIndex costType;

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	private void FixedUpdate()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)targetTextMesh))
		{
			sharedStringBuilder.Clear();
			Color color = Color.white;
			CostTypeDef costTypeDef = CostTypeCatalog.GetCostTypeDef(costType);
			if (costTypeDef != null)
			{
				costTypeDef.BuildCostStringStyled(displayValue, sharedStringBuilder, forWorldDisplay: true, includeColor: false);
				color = Color32.op_Implicit(costTypeDef.GetCostColor(forWorldDisplay: true));
			}
			((TMP_Text)targetTextMesh).SetText(sharedStringBuilder);
			((Graphic)targetTextMesh).color = color;
		}
	}
}
