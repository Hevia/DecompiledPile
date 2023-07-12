using RoR2;
using UnityEngine;

namespace EntityStates.GummyClone;

public class GummyCloneDeathState : BaseState
{
	[SerializeField]
	public string soundString;

	[SerializeField]
	public GameObject effectPrefab;

	public override void OnEnter()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(soundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			if (Object.op_Implicit((Object)(object)base.modelLocator.modelBaseTransform))
			{
				EntityState.Destroy((Object)(object)((Component)base.modelLocator.modelBaseTransform).gameObject);
			}
			if (Object.op_Implicit((Object)(object)base.modelLocator.modelTransform))
			{
				EntityState.Destroy((Object)(object)((Component)base.modelLocator.modelTransform).gameObject);
			}
		}
		if (base.isAuthority && Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleImpactEffect(effectPrefab, base.transform.position, Vector3.up, transmit: true);
		}
		EntityState.Destroy((Object)(object)base.gameObject);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
