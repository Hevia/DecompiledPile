using UnityEngine;

namespace RoR2.UI;

public class VoidRaidCrabHealthBarPipController : MonoBehaviour
{
	[SerializeField]
	private GameObject pipPrefab;

	[SerializeField]
	private ItemDef minHealthPercentageDef;

	public void InitializePips(PhasedInventorySetter phasedInventory)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		int numPhases = phasedInventory.GetNumPhases();
		for (int i = 0; i < numPhases; i++)
		{
			float num = 0.01f * (float)phasedInventory.GetItemCountForPhase(i, minHealthPercentageDef);
			if (num > 0f)
			{
				RectTransform component = Object.Instantiate<GameObject>(pipPrefab, ((Component)this).transform).GetComponent<RectTransform>();
				component.anchorMin = new Vector2(num, component.anchorMin.y);
				component.anchorMax = new Vector2(num, component.anchorMax.y);
			}
		}
	}
}
