using UnityEngine;

public class LightIntensityCurve : MonoBehaviour
{
	public AnimationCurve curve;

	public float timeMax = 5f;

	private float time;

	private Light light;

	private float maxIntensity;

	[Tooltip("Loops the animation curve.")]
	public bool loop;

	[Tooltip("Starts in a random point in the animation curve.")]
	public bool randomStart;

	[Tooltip("Causes the lights to be negative, casting shadows instead.")]
	public bool enableNegativeLights;

	private void Start()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		light = ((Component)this).GetComponent<Light>();
		maxIntensity = light.intensity;
		light.intensity = 0f;
		if (randomStart)
		{
			time = Random.Range(0f, timeMax);
		}
		if (enableNegativeLights)
		{
			light.color = new Color(0f - light.color.r, 0f - light.color.g, 0f - light.color.b);
		}
	}

	private void Update()
	{
		time += Time.deltaTime;
		light.intensity = curve.Evaluate(time / timeMax) * maxIntensity;
		if (time >= timeMax && loop)
		{
			time = 0f;
		}
	}
}
