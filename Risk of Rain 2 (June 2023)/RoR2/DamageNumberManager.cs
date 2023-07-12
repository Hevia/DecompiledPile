using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

[ExecuteAlways]
public class DamageNumberManager : MonoBehaviour
{
	private List<Vector4> customData = new List<Vector4>();

	private ParticleSystem ps;

	public static DamageNumberManager instance { get; private set; }

	private void OnEnable()
	{
		instance = SingletonHelper.Assign<DamageNumberManager>(instance, this);
	}

	private void OnDisable()
	{
		instance = SingletonHelper.Unassign<DamageNumberManager>(instance, this);
	}

	private void Awake()
	{
		ps = ((Component)this).GetComponent<ParticleSystem>();
	}

	private void Update()
	{
	}

	public void SpawnDamageNumber(float amount, Vector3 position, bool crit, TeamIndex teamIndex, DamageColorIndex damageColorIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		Color val = DamageColor.FindColor(damageColorIndex);
		Color val2 = Color.white;
		switch (teamIndex)
		{
		case TeamIndex.None:
			val2 = Color.gray;
			break;
		case TeamIndex.Monster:
			((Color)(ref val2))._002Ector(0.5568628f, 0.29411766f, 0.6039216f);
			break;
		}
		ParticleSystem obj = ps;
		EmitParams val3 = default(EmitParams);
		((EmitParams)(ref val3)).position = position;
		((EmitParams)(ref val3)).startColor = Color32.op_Implicit(val * val2);
		((EmitParams)(ref val3)).applyShapeToPosition = true;
		obj.Emit(val3, 1);
		ps.GetCustomParticleData(customData, (ParticleSystemCustomData)0);
		customData[customData.Count - 1] = new Vector4(1f, 0f, amount, crit ? 1f : 0f);
		ps.SetCustomParticleData(customData, (ParticleSystemCustomData)0);
	}
}
