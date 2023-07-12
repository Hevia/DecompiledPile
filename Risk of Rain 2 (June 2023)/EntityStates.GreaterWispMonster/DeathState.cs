using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.GreaterWispMonster;

public class DeathState : GenericCharacterDeath
{
	[SerializeField]
	public GameObject initialEffect;

	[SerializeField]
	public GameObject deathEffect;

	private static float duration = 2f;

	private GameObject initialEffectInstance;

	public override void OnEnter()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (!Object.op_Implicit((Object)(object)base.modelLocator))
		{
			return;
		}
		ChildLocator component = ((Component)base.modelLocator.modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild("Mask");
			((Component)val).gameObject.SetActive(true);
			((Component)val).GetComponent<AnimateShaderAlpha>().timeMax = duration;
			if (Object.op_Implicit((Object)(object)initialEffect))
			{
				initialEffectInstance = Object.Instantiate<GameObject>(initialEffect, val.position, val.rotation, val);
			}
		}
	}

	public override void FixedUpdate()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.fixedAge >= duration && NetworkServer.active)
		{
			if (Object.op_Implicit((Object)(object)deathEffect))
			{
				EffectManager.SpawnEffect(deathEffect, new EffectData
				{
					origin = base.transform.position
				}, transmit: true);
			}
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)initialEffectInstance))
		{
			EntityState.Destroy((Object)(object)initialEffectInstance);
		}
	}
}
