using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class AnimateUIAlpha : MonoBehaviour
{
	public AnimationCurve alphaCurve;

	public Image image;

	public RawImage rawImage;

	public SpriteRenderer spriteRenderer;

	public float timeMax = 5f;

	public bool destroyOnEnd;

	public bool loopOnEnd;

	public bool disableGameObjectOnEnd;

	[HideInInspector]
	public float time;

	private Color originalColor;

	private void Start()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)image))
		{
			originalColor = ((Graphic)image).color;
		}
		if (Object.op_Implicit((Object)(object)rawImage))
		{
			originalColor = ((Graphic)rawImage).color;
		}
		else if (Object.op_Implicit((Object)(object)spriteRenderer))
		{
			originalColor = spriteRenderer.color;
		}
		UpdateAlphas(0f);
	}

	private void OnDisable()
	{
		time = 0f;
	}

	private void Update()
	{
		UpdateAlphas(Time.unscaledDeltaTime);
	}

	private void UpdateAlphas(float deltaTime)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		time = Mathf.Min(timeMax, time + deltaTime);
		float num = alphaCurve.Evaluate(time / timeMax);
		Color color = default(Color);
		((Color)(ref color))._002Ector(originalColor.r, originalColor.g, originalColor.b, originalColor.a * num);
		if (Object.op_Implicit((Object)(object)image))
		{
			((Graphic)image).color = color;
		}
		if (Object.op_Implicit((Object)(object)rawImage))
		{
			((Graphic)rawImage).color = color;
		}
		else if (Object.op_Implicit((Object)(object)spriteRenderer))
		{
			spriteRenderer.color = color;
		}
		if (loopOnEnd && time >= timeMax)
		{
			time -= timeMax;
		}
		if (destroyOnEnd && time >= timeMax)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		if (disableGameObjectOnEnd && time >= timeMax)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
