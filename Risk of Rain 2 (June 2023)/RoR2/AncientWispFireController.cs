using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(CharacterModel))]
public class AncientWispFireController : MonoBehaviour
{
	public ParticleSystem normalParticles;

	public Light normalLight;

	public ParticleSystem rageParticles;

	public Light rageLight;

	private CharacterModel characterModel;

	private void Awake()
	{
		characterModel = ((Component)this).GetComponent<CharacterModel>();
	}

	private void Update()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		CharacterBody body = characterModel.body;
		if (Object.op_Implicit((Object)(object)body))
		{
			flag = body.HasBuff(JunkContent.Buffs.EnrageAncientWisp);
		}
		if (Object.op_Implicit((Object)(object)normalParticles))
		{
			EmissionModule emission = normalParticles.emission;
			if (((EmissionModule)(ref emission)).enabled == flag)
			{
				((EmissionModule)(ref emission)).enabled = !flag;
				if (!flag)
				{
					normalParticles.Play();
				}
			}
		}
		if (Object.op_Implicit((Object)(object)rageParticles))
		{
			EmissionModule emission2 = rageParticles.emission;
			if (((EmissionModule)(ref emission2)).enabled != flag)
			{
				((EmissionModule)(ref emission2)).enabled = flag;
				if (flag)
				{
					rageParticles.Play();
				}
			}
		}
		if (Object.op_Implicit((Object)(object)normalLight))
		{
			((Behaviour)normalLight).enabled = !flag;
		}
		if (Object.op_Implicit((Object)(object)rageLight))
		{
			((Behaviour)rageLight).enabled = flag;
		}
	}
}
