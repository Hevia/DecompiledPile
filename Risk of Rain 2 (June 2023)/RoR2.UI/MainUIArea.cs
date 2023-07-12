using UnityEngine;

namespace RoR2.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class MainUIArea : MonoBehaviour
{
	private RectTransform rectTransform;

	private RectTransform parentRectTransform;

	private void Awake()
	{
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		parentRectTransform = ((Component)((Transform)rectTransform).parent).GetComponent<RectTransform>();
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = parentRectTransform.rect;
		float num = ((Rect)(ref rect)).width * 0.05f;
		float num2 = ((Rect)(ref rect)).height * 0.05f;
		rectTransform.offsetMin = new Vector2(num, num2);
		rectTransform.offsetMax = new Vector2(0f - num, 0f - num2);
	}
}
