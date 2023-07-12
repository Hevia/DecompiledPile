using RoR2;
using UnityEngine;

namespace EntityStates.GolemMonster;

public class ClapState : BaseState
{
	public static float duration = 3.5f;

	public static float damageCoefficient = 4f;

	public static float forceMagnitude = 16f;

	public static float radius = 3f;

	private BlastAttack attack;

	public static string attackSoundString;

	private Animator modelAnimator;

	private Transform modelTransform;

	private bool hasAttacked;

	private GameObject leftHandChargeEffect;

	private GameObject rightHandChargeEffect;

	public override void OnEnter()
	{
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		modelTransform = GetModelTransform();
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
		PlayCrossfade("Body", "Clap", "Clap.playbackRate", duration, 0.1f);
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			GameObject val = LegacyResourcesAPI.Load<GameObject>("Prefabs/GolemClapCharge");
			Transform val2 = component.FindChild("HandL");
			Transform val3 = component.FindChild("HandR");
			if (Object.op_Implicit((Object)(object)val2))
			{
				leftHandChargeEffect = Object.Instantiate<GameObject>(val, val2);
			}
			if (Object.op_Implicit((Object)(object)val3))
			{
				rightHandChargeEffect = Object.Instantiate<GameObject>(val, val3);
			}
		}
	}

	public override void OnExit()
	{
		EntityState.Destroy((Object)(object)leftHandChargeEffect);
		EntityState.Destroy((Object)(object)rightHandChargeEffect);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("Clap.hitBoxActive") > 0.5f && !hasAttacked)
		{
			if (base.isAuthority && Object.op_Implicit((Object)(object)modelTransform))
			{
				ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
				if (Object.op_Implicit((Object)(object)component))
				{
					Transform val = component.FindChild("ClapZone");
					if (Object.op_Implicit((Object)(object)val))
					{
						attack = new BlastAttack();
						attack.attacker = base.gameObject;
						attack.inflictor = base.gameObject;
						attack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
						attack.baseDamage = damageStat * damageCoefficient;
						attack.baseForce = forceMagnitude;
						attack.position = val.position;
						attack.radius = radius;
						attack.attackerFiltering = AttackerFiltering.NeverHitSelf;
						attack.Fire();
					}
				}
			}
			hasAttacked = true;
			EntityState.Destroy((Object)(object)leftHandChargeEffect);
			EntityState.Destroy((Object)(object)rightHandChargeEffect);
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
