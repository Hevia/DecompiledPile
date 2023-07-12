using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(Image))]
public class StageFadeTransitionController : MonoBehaviour
{
	private Image fadeImage;

	private float startEngineTime;

	private const float transitionDuration = 0.5f;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		fadeImage = ((Component)this).GetComponent<Image>();
		Color color = ((Graphic)fadeImage).color;
		color.a = 1f;
		((Graphic)fadeImage).color = color;
	}

	private void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((Graphic)fadeImage).color;
		color.a = 1f;
		((Graphic)fadeImage).color = color;
		((Graphic)fadeImage).CrossFadeColor(Color.black, 0.5f, false, true);
		startEngineTime = Time.time;
	}

	private void Update()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Stage.instance))
		{
			Run.FixedTimeStamp stageAdvanceTime = Stage.instance.stageAdvanceTime;
			float num = Time.time - startEngineTime;
			float num2 = 0f;
			float num3 = 0f;
			if (num < 0.5f)
			{
				num2 = 1f - Mathf.Clamp01((Time.time - startEngineTime) / 0.5f);
			}
			if (!stageAdvanceTime.isInfinity)
			{
				float num4 = Stage.instance.stageAdvanceTime - 0.25f - Run.FixedTimeStamp.now;
				num3 = 1f - Mathf.Clamp01(num4 / 0.5f);
			}
			Color color = ((Graphic)fadeImage).color;
			color.a = Mathf.Max(num2, num3);
			((Graphic)fadeImage).color = color;
		}
	}
}
