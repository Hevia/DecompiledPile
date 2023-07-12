using UnityEngine;

namespace RoR2;

public class RTPCController : MonoBehaviour
{
	public string akSoundString;

	public string rtpcString;

	public float rtpcValue;

	public bool useCurveInstead;

	public AnimationCurve rtpcValueCurve;

	private float fixedAge;

	private void Start()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (akSoundString.Length > 0)
		{
			Util.PlaySound(akSoundString, ((Component)this).gameObject, rtpcString, rtpcValue);
		}
		else
		{
			AkSoundEngine.SetRTPCValue(rtpcString, rtpcValue, ((Component)this).gameObject);
		}
	}

	private void FixedUpdate()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (useCurveInstead)
		{
			fixedAge += Time.fixedDeltaTime;
			AkSoundEngine.SetRTPCValue(rtpcString, rtpcValueCurve.Evaluate(fixedAge), ((Component)this).gameObject);
		}
	}
}
