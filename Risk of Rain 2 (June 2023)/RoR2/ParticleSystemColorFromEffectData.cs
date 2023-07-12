using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(EffectComponent))]
public class ParticleSystemColorFromEffectData : MonoBehaviour
{
	public ParticleSystem[] particleSystems;

	public EffectComponent effectComponent;

	private void Start()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Color val = Color32.op_Implicit(effectComponent.effectData.color);
		for (int i = 0; i < particleSystems.Length; i++)
		{
			MainModule main = particleSystems[i].main;
			((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(val);
			particleSystems[i].Clear();
			particleSystems[i].Play();
		}
	}
}
