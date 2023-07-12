using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace RoR2;

[RequireComponent(typeof(PostProcessVolume))]
public class HookLightingIntoPostProcessVolume : MonoBehaviour
{
	[Header("Required Values")]
	public PostProcessVolume volume;

	[ColorUsage(true, true)]
	public Color overrideAmbientColor;

	[Header("Optional Values")]
	public Light directionalLight;

	public Color overrideDirectionalColor;

	public ParticleSystem particleSystem;

	public float overrideParticleSystemMultiplier;

	private Collider[] volumeColliders;

	private Color defaultAmbientColor;

	private Color defaultDirectionalColor;

	private float defaultParticleSystemMultiplier;

	private bool hasCachedInitialValues;

	private void OnEnable()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		volumeColliders = ((Component)this).GetComponents<Collider>();
		if (!hasCachedInitialValues)
		{
			defaultAmbientColor = RenderSettings.ambientLight;
			if (Object.op_Implicit((Object)(object)directionalLight))
			{
				defaultDirectionalColor = directionalLight.color;
			}
			if (Object.op_Implicit((Object)(object)particleSystem))
			{
				EmissionModule emission = particleSystem.emission;
				defaultParticleSystemMultiplier = ((EmissionModule)(ref emission)).rateOverTimeMultiplier;
			}
			hasCachedInitialValues = true;
		}
		SceneCamera.onSceneCameraPreRender += OnPreRenderSceneCam;
	}

	private void OnDisable()
	{
		SceneCamera.onSceneCameraPreRender -= OnPreRenderSceneCam;
	}

	private void OnPreRenderSceneCam(SceneCamera sceneCam)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		float interpFactor = GetInterpFactor(((Component)sceneCam.camera).transform.position);
		RenderSettings.ambientLight = Color.Lerp(defaultAmbientColor, overrideAmbientColor, interpFactor);
		if (Object.op_Implicit((Object)(object)directionalLight))
		{
			directionalLight.color = Color.Lerp(defaultDirectionalColor, overrideDirectionalColor, interpFactor);
		}
		if (Object.op_Implicit((Object)(object)particleSystem))
		{
			EmissionModule emission = particleSystem.emission;
			((EmissionModule)(ref emission)).rateOverTimeMultiplier = Mathf.Lerp(defaultParticleSystemMultiplier, overrideParticleSystemMultiplier, interpFactor);
		}
	}

	private float GetInterpFactor(Vector3 triggerPos)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)volume).enabled || volume.weight <= 0f)
		{
			return 0f;
		}
		if (volume.isGlobal)
		{
			return 1f;
		}
		float num = 0f;
		Collider[] array = volumeColliders;
		foreach (Collider val in array)
		{
			float num2 = float.PositiveInfinity;
			if (val.enabled)
			{
				Vector3 val2 = (val.ClosestPoint(triggerPos) - triggerPos) / 2f;
				float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
				}
				float num3 = volume.blendDistance * volume.blendDistance;
				if (!(num2 > num3) && num3 > 0f)
				{
					num = Mathf.Max(num, 1f - num2 / num3);
				}
			}
		}
		return num;
	}
}
