using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Bandit2;

public class StealthMode : BaseState
{
	public static float duration;

	public static string enterStealthSound;

	public static string exitStealthSound;

	public static float blastAttackRadius;

	public static float blastAttackDamageCoefficient;

	public static float blastAttackProcCoefficient;

	public static float blastAttackForce;

	public static GameObject smokeBombEffectPrefab;

	public static string smokeBombMuzzleString;

	public static float shortHopVelocity;

	private Animator animator;

	public override void OnEnter()
	{
		base.OnEnter();
		animator = GetModelAnimator();
		Object.op_Implicit((Object)(object)animator);
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			if (NetworkServer.active)
			{
				base.characterBody.AddBuff(RoR2Content.Buffs.Cloak);
				base.characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
			}
			base.characterBody.onSkillActivatedAuthority += OnSkillActivatedAuthority;
		}
		FireSmokebomb();
		Util.PlaySound(enterStealthSound, base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		if (!outer.destroying)
		{
			FireSmokebomb();
		}
		Util.PlaySound(exitStealthSound, base.gameObject);
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.onSkillActivatedAuthority -= OnSkillActivatedAuthority;
			if (NetworkServer.active)
			{
				base.characterBody.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
				base.characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
			}
		}
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.SetLayerWeight(animator.GetLayerIndex("Body, StealthWeapon"), 0f);
		}
		base.OnExit();
	}

	private void OnSkillActivatedAuthority(GenericSkill skill)
	{
		if (skill.skillDef.isCombatSkill)
		{
			outer.SetNextStateToMain();
		}
	}

	private void FireSmokebomb()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		if (base.isAuthority)
		{
			BlastAttack obj = new BlastAttack
			{
				radius = blastAttackRadius,
				procCoefficient = blastAttackProcCoefficient,
				position = base.transform.position,
				attacker = base.gameObject,
				crit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master),
				baseDamage = base.characterBody.damage * blastAttackDamageCoefficient,
				falloffModel = BlastAttack.FalloffModel.None,
				damageType = DamageType.Stun1s,
				baseForce = blastAttackForce
			};
			obj.teamIndex = TeamComponent.GetObjectTeam(obj.attacker);
			obj.attackerFiltering = AttackerFiltering.NeverHitSelf;
			obj.Fire();
		}
		if (Object.op_Implicit((Object)(object)smokeBombEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(smokeBombEffectPrefab, base.gameObject, smokeBombMuzzleString, transmit: false);
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, shortHopVelocity, base.characterMotor.velocity.z);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
