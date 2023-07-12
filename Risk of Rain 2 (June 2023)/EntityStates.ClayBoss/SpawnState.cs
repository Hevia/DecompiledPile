using RoR2;
using UnityEngine;

namespace EntityStates.ClayBoss;

public class SpawnState : BaseState
{
	public static float duration;

	public static string spawnSoundString;

	public static GameObject spawnEffectPrefab;

	public static string spawnEffectChildString;

	public override void OnEnter()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		ChildLocator component = ((Component)GetModelTransform()).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild(spawnEffectChildString);
			if (Object.op_Implicit((Object)(object)val))
			{
				Object.Instantiate<GameObject>(spawnEffectPrefab, val.position, Quaternion.identity);
			}
		}
		Util.PlaySound(spawnSoundString, base.gameObject);
		PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
