using RoR2;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Commando.CommandoWeapon;

public class PrepLightsOut : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject chargePrefab;

	public static GameObject specialCrosshairPrefab;

	public static string prepSoundString;

	private GameObject chargeEffect;

	private float duration;

	private ChildLocator childLocator;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	public override void OnEnter()
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture, Additive", "PrepRevolver", "PrepRevolver.playbackRate", duration);
		PlayAnimation("Gesture, Override", "PrepRevolver", "PrepRevolver.playbackRate", duration);
		Util.PlaySound(prepSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)specialCrosshairPrefab))
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, specialCrosshairPrefab, CrosshairUtils.OverridePriority.Skill);
		}
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)childLocator))
			{
				Transform val = childLocator.FindChild("MuzzlePistol");
				if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargePrefab))
				{
					chargeEffect = Object.Instantiate<GameObject>(chargePrefab, val.position, val.rotation);
					chargeEffect.transform.parent = val;
					ScaleParticleSystemDuration component = chargeEffect.GetComponent<ScaleParticleSystemDuration>();
					if (Object.op_Implicit((Object)(object)component))
					{
						component.newDuration = duration;
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextState(new FireLightsOut());
		}
	}

	public override void OnExit()
	{
		EntityState.Destroy((Object)(object)chargeEffect);
		crosshairOverrideRequest?.Dispose();
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
