using RoR2;
using UnityEngine;

namespace EntityStates.Bandit2.Weapon;

public class SlashBlade : BasicMeleeAttack
{
	public static float shortHopVelocity;

	public static float selfForceStrength;

	public static float minimumBaseDuration;

	public static AnimationCurve bloomCurve;

	private GameObject bladeMeshObject;

	private float minimumDuration => minimumBaseDuration / attackSpeedStat;

	public override void OnEnter()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayAnimation("Gesture, Additive", "SlashBlade", "SlashBlade.playbackRate", duration);
		bladeMeshObject = ((Component)FindModelChild("BladeMesh")).gameObject;
		if (Object.op_Implicit((Object)(object)bladeMeshObject))
		{
			bladeMeshObject.SetActive(true);
		}
		base.characterMotor.ApplyForce(base.inputBank.moveVector * selfForceStrength, alwaysApply: true);
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, Mathf.Max(base.characterMotor.velocity.y, shortHopVelocity), base.characterMotor.velocity.z);
		}
	}

	protected override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
	{
		base.AuthorityModifyOverlapAttack(overlapAttack);
		overlapAttack.damageType = DamageType.SuperBleedOnCrit;
	}

	public override void Update()
	{
		base.Update();
		base.characterBody.SetSpreadBloom(bloomCurve.Evaluate(base.age / duration), canOnlyIncreaseBloom: false);
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)bladeMeshObject))
		{
			bladeMeshObject.SetActive(false);
		}
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			if (!(base.fixedAge > minimumDuration))
			{
				return InterruptPriority.PrioritySkill;
			}
			return InterruptPriority.Skill;
		}
		return InterruptPriority.Skill;
	}
}
