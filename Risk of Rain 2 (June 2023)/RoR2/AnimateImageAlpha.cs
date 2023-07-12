using UnityEngine;
using UnityEngine.UI;

namespace RoR2;

public class AnimateImageAlpha : MonoBehaviour
{
	public AnimationCurve alphaCurve;

	public Image[] images;

	public float timeMax = 5f;

	public float delayBetweenElements;

	[HideInInspector]
	public float stopwatch;

	private void OnEnable()
	{
		stopwatch = 0f;
	}

	public void ResetStopwatch()
	{
		stopwatch = 0f;
	}

	private void LateUpdate()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		stopwatch += Time.unscaledDeltaTime;
		int num = 0;
		Image[] array = images;
		foreach (Image obj in array)
		{
			num++;
			float num2 = alphaCurve.Evaluate((stopwatch + delayBetweenElements * (float)num) / timeMax);
			Color color = ((Graphic)obj).color;
			((Graphic)obj).color = new Color(color.r, color.g, color.b, num2);
		}
	}
}
