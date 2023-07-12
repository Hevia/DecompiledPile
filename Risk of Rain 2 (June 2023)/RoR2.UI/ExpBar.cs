using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class ExpBar : MonoBehaviour
{
	public CharacterMaster source;

	public RectTransform fillRectTransform;

	private RectTransform rectTransform;

	private void Awake()
	{
		rectTransform = ((Component)this).GetComponent<RectTransform>();
	}

	public void Update()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		TeamIndex teamIndex = (Object.op_Implicit((Object)(object)source) ? source.teamIndex : TeamIndex.Neutral);
		float num = 0f;
		if (Object.op_Implicit((Object)(object)source) && Object.op_Implicit((Object)(object)TeamManager.instance))
		{
			num = Mathf.InverseLerp((float)TeamManager.instance.GetTeamCurrentLevelExperience(teamIndex), (float)TeamManager.instance.GetTeamNextLevelExperience(teamIndex), (float)TeamManager.instance.GetTeamExperience(teamIndex));
		}
		if (Object.op_Implicit((Object)(object)fillRectTransform))
		{
			_ = rectTransform.rect;
			_ = fillRectTransform.rect;
			fillRectTransform.anchorMin = new Vector2(0f, 0f);
			fillRectTransform.anchorMax = new Vector2(num, 1f);
			fillRectTransform.sizeDelta = new Vector2(1f, 1f);
		}
	}
}
