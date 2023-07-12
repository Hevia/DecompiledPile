using UnityEngine;

namespace RoR2.PostProcessing;

public class ScreenDamage : MonoBehaviour
{
	private CameraRigController cameraRigController;

	public Material mat;

	public float DistortionScale = 1f;

	public float DistortionPower = 1f;

	public float DesaturationScale = 1f;

	public float DesaturationPower = 1f;

	public float TintScale = 1f;

	public float TintPower = 1f;

	private float healthPercentage = 1f;

	private const float hitTintDecayTime = 0.6f;

	private const float hitTintScale = 1.6f;

	private const float deathWeight = 2f;

	private void Awake()
	{
		cameraRigController = ((Component)this).GetComponentInParent<CameraRigController>();
		mat = Object.Instantiate<Material>(mat);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		float num = 0f;
		float num2 = 0f;
		if (Object.op_Implicit((Object)(object)cameraRigController))
		{
			if (Object.op_Implicit((Object)(object)cameraRigController.target))
			{
				num2 = 0.5f;
				HealthComponent component = cameraRigController.target.GetComponent<HealthComponent>();
				if (Object.op_Implicit((Object)(object)component))
				{
					healthPercentage = Mathf.Clamp(component.health / component.fullHealth, 0f, 1f);
					num = Mathf.Clamp01(1f - component.timeSinceLastHit / 0.6f) * 1.6f;
					if (component.health <= 0f)
					{
						num2 = 0f;
					}
				}
			}
			mat.SetFloat("_DistortionStrength", num2 * DistortionScale * Mathf.Pow(1f - healthPercentage, DistortionPower));
			mat.SetFloat("_DesaturationStrength", num2 * DesaturationScale * Mathf.Pow(1f - healthPercentage, DesaturationPower));
			mat.SetFloat("_TintStrength", num2 * TintScale * (Mathf.Pow(1f - healthPercentage, TintPower) + num));
		}
		Graphics.Blit((Texture)(object)source, destination, mat);
	}
}
