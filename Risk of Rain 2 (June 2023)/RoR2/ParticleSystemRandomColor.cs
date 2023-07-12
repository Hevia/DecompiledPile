using UnityEngine;

namespace RoR2;

public class ParticleSystemRandomColor : MonoBehaviour
{
	public Color[] colors;

	public ParticleSystem[] particleSystems;

	private void Awake()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (colors.Length != 0)
		{
			Color val = colors[Random.Range(0, colors.Length)];
			for (int i = 0; i < particleSystems.Length; i++)
			{
				MainModule main = particleSystems[i].main;
				((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(val);
			}
		}
	}

	[AssetCheck(typeof(ParticleSystemRandomColor))]
	private static void CheckParticleSystemRandomColor(AssetCheckArgs args)
	{
		ParticleSystemRandomColor particleSystemRandomColor = (ParticleSystemRandomColor)(object)args.asset;
		for (int i = 0; i < particleSystemRandomColor.particleSystems.Length; i++)
		{
			if (!Object.op_Implicit((Object)(object)particleSystemRandomColor.particleSystems[i]))
			{
				args.LogErrorFormat(args.asset, "Null particle system in slot {0}", i);
			}
		}
	}
}
