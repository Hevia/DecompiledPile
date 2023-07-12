using TMPro;
using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(HudElement))]
public class SniperRangeIndicator : MonoBehaviour
{
	public TextMeshProUGUI label;

	private HudElement hudElement;

	private void Awake()
	{
		hudElement = ((Component)this).GetComponent<HudElement>();
	}

	private void FixedUpdate()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		float num = float.PositiveInfinity;
		if (Object.op_Implicit((Object)(object)hudElement.targetCharacterBody))
		{
			InputBankTest component = ((Component)hudElement.targetCharacterBody).GetComponent<InputBankTest>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Ray ray = default(Ray);
				((Ray)(ref ray))._002Ector(component.aimOrigin, component.aimDirection);
				RaycastHit hitInfo = default(RaycastHit);
				if (Util.CharacterRaycast(((Component)hudElement.targetCharacterBody).gameObject, ray, out hitInfo, float.PositiveInfinity, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)), (QueryTriggerInteraction)0))
				{
					num = ((RaycastHit)(ref hitInfo)).distance;
				}
			}
		}
		((TMP_Text)label).text = "Dis: " + ((num > 999f) ? "999m" : $"{Mathf.FloorToInt(num):D3}m");
	}
}
