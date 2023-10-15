using RoR2;
using UnityEngine;

namespace EntityStates.ParentMonster;

public class GroundSlam : BaseState
{
	public static float duration = 3.5f;

	public static float damageCoefficient = 4f;

	public static float forceMagnitude = 16f;

	public static float radius = 3f;

	private BlastAttack attack;

	public static string attackSoundString;

	public static GameObject slamImpactEffect;

	public static GameObject meleeTrailEffectL;

	public static GameObject meleeTrailEffectR;

	private Animator modelAnimator;

	private Transform modelTransform;

	private bool hasAttacked;

	public override void OnEnter()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		modelTransform = GetModelTransform();
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
		PlayCrossfade("Body", "Slam", "Slam.playbackRate", duration, 0.1f);
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			CharacterDirection obj = base.characterDirection;
			Ray aimRay = GetAimRay();
			obj.moveVector = aimRay.direction;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("Slam.hitBoxActive") > 0.5f && !hasAttacked)
		{
			if (base.isAuthority)
			{
				if (Object.op_Implicit((Object)(object)base.characterDirection))
				{
					base.characterDirection.moveVector = base.characterDirection.forward;
				}
				if (Object.op_Implicit((Object)(object)modelTransform))
				{
					Transform val = FindModelChild("SlamZone");
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
						attack.Fire();
					}
				}
			}
			hasAttacked = true;
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
