using UnityEngine;

namespace RoR2;

public class HoverEngineDisplay : MonoBehaviour
{
	public HoverEngine hoverEngine;

	[Tooltip("The local pitch at zero engine strength")]
	public float minPitch = -20f;

	[Tooltip("The local pitch at max engine strength")]
	public float maxPitch = 60f;

	public float smoothTime = 0.2f;

	public float forceScale = 1f;

	private float smoothVelocity;

	private void FixedUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localEulerAngles = ((Component)this).transform.localEulerAngles;
		float num = Mathf.Clamp01(hoverEngine.forceStrength / hoverEngine.hoverForce * forceScale);
		float num2 = Mathf.LerpAngle(minPitch, maxPitch, num);
		float num3 = Mathf.SmoothDampAngle(localEulerAngles.x, num2, ref smoothVelocity, smoothTime);
		((Component)this).transform.localRotation = Quaternion.Euler(num3, 0f, 0f);
	}
}
