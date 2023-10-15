using RoR2;
using UnityEngine;

namespace EntityStates.BeetleQueenMonster;

public class ChargeSpit : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject effectPrefab;

	public static string attackSoundString;

	private float duration;

	private GameObject chargeEffect;

	public override void OnEnter()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		PlayCrossfade("Gesture", "ChargeSpit", "ChargeSpit.playbackRate", duration, 0.2f);
		Util.PlaySound(attackSoundString, base.gameObject);
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (!Object.op_Implicit((Object)(object)component) || !Object.op_Implicit((Object)(object)effectPrefab))
		{
			return;
		}
		Transform val = component.FindChild("Mouth");
		if (Object.op_Implicit((Object)(object)val))
		{
			chargeEffect = Object.Instantiate<GameObject>(effectPrefab, val.position, val.rotation);
			chargeEffect.transform.parent = val;
			ScaleParticleSystemDuration component2 = chargeEffect.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.newDuration = duration;
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		EntityState.Destroy((Object)(object)chargeEffect);
	}

	public override void Update()
	{
		base.Update();
	}

	public override void FixedUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			CharacterDirection obj = base.characterDirection;
			Ray aimRay = GetAimRay();
			obj.moveVector = aimRay.direction;
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			FireSpit nextState = new FireSpit();
			outer.SetNextState(nextState);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
