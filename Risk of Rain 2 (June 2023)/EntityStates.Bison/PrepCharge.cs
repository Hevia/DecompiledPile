using RoR2;
using UnityEngine;

namespace EntityStates.Bison;

public class PrepCharge : BaseState
{
	public static float basePrepDuration;

	public static string enterSoundString;

	public static GameObject chargeEffectPrefab;

	private float stopwatch;

	private float prepDuration;

	private GameObject chargeEffectInstance;

	public override void OnEnter()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		prepDuration = basePrepDuration / attackSpeedStat;
		base.characterBody.SetAimTimer(prepDuration);
		PlayCrossfade("Body", "PrepCharge", "PrepCharge.playbackRate", prepDuration, 0.2f);
		Util.PlaySound(enterSoundString, base.gameObject);
		Transform modelTransform = GetModelTransform();
		AimAnimator component = ((Component)modelTransform).GetComponent<AimAnimator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			((Behaviour)component).enabled = true;
		}
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			CharacterDirection obj = base.characterDirection;
			Ray aimRay = GetAimRay();
			obj.moveVector = ((Ray)(ref aimRay)).direction;
		}
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component2 = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (!Object.op_Implicit((Object)(object)component2))
		{
			return;
		}
		Transform val = component2.FindChild("ChargeIndicator");
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
			chargeEffectInstance.transform.parent = val;
			ScaleParticleSystemDuration component3 = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				component3.newDuration = prepDuration;
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)chargeEffectInstance))
		{
			EntityState.Destroy((Object)(object)chargeEffectInstance);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch > prepDuration && base.isAuthority)
		{
			outer.SetNextState(new Charge());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
