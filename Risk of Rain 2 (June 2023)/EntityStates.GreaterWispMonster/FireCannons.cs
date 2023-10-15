using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.GreaterWispMonster;

public class FireCannons : BaseState
{
	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public GameObject effectPrefab;

	public static float baseDuration = 2f;

	[SerializeField]
	public float damageCoefficient = 1.2f;

	[SerializeField]
	public float force = 20f;

	private float duration;

	public override void OnEnter()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Ray aimRay = GetAimRay();
		string text = "MuzzleLeft";
		string text2 = "MuzzleRight";
		duration = baseDuration / attackSpeedStat;
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, text, transmit: false);
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, text2, transmit: false);
		}
		PlayAnimation("Gesture", "FireCannons", "FireCannons.playbackRate", duration);
		if (!base.isAuthority || !Object.op_Implicit((Object)(object)base.modelLocator) || !Object.op_Implicit((Object)(object)base.modelLocator.modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)base.modelLocator.modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			int childIndex = component.FindChildIndex(text);
			int childIndex2 = component.FindChildIndex(text2);
			Transform val = component.FindChild(childIndex);
			Transform val2 = component.FindChild(childIndex2);
			if (Object.op_Implicit((Object)(object)val))
			{
				ProjectileManager.instance.FireProjectile(projectilePrefab, val.position, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
			}
			if (Object.op_Implicit((Object)(object)val2))
			{
				ProjectileManager.instance.FireProjectile(projectilePrefab, val2.position, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
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
		return InterruptPriority.Skill;
	}
}
