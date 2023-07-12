using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(ParticleSystem))]
[ExecuteAlways]
public class NormalizeParticleScale : MonoBehaviour
{
	public bool normalizeWithSkinnedMeshRendererInstead;

	private ParticleSystem particleSystem;

	public void OnEnable()
	{
		UpdateParticleSystem();
	}

	private void UpdateParticleSystem()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)particleSystem))
		{
			particleSystem = ((Component)this).GetComponent<ParticleSystem>();
		}
		MainModule main = particleSystem.main;
		MinMaxCurve startSize = ((MainModule)(ref main)).startSize;
		Vector3 lossyScale = ((Component)this).transform.lossyScale;
		if (normalizeWithSkinnedMeshRendererInstead)
		{
			ShapeModule shape = particleSystem.shape;
			SkinnedMeshRenderer skinnedMeshRenderer = ((ShapeModule)(ref shape)).skinnedMeshRenderer;
			if (Object.op_Implicit((Object)(object)skinnedMeshRenderer))
			{
				lossyScale = ((Component)skinnedMeshRenderer).transform.lossyScale;
			}
		}
		float num = Mathf.Max(new float[3] { lossyScale.x, lossyScale.y, lossyScale.z });
		((MinMaxCurve)(ref startSize)).constantMin = ((MinMaxCurve)(ref startSize)).constantMin / num;
		((MinMaxCurve)(ref startSize)).constantMax = ((MinMaxCurve)(ref startSize)).constantMax / num;
		((MainModule)(ref main)).startSize = startSize;
	}
}
