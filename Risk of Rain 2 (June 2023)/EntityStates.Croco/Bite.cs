using RoR2;
using UnityEngine;

namespace EntityStates.Croco;

public class Bite : BasicMeleeAttack
{
	public static float recoilAmplitude;

	public static float baseDurationBeforeInterruptable;

	[SerializeField]
	public float bloom;

	public static string biteSound;

	private string animationStateName;

	private float durationBeforeInterruptable;

	private CrocoDamageTypeController crocoDamageTypeController;

	private bool hasGrantedBuff;

	protected override bool allowExitFire
	{
		get
		{
			if (Object.op_Implicit((Object)(object)base.characterBody))
			{
				return !base.characterBody.isSprinting;
			}
			return false;
		}
	}

	public override void OnEnter()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		CharacterDirection obj = base.characterDirection;
		Ray aimRay = GetAimRay();
		obj.forward = ((Ray)(ref aimRay)).direction;
		durationBeforeInterruptable = baseDurationBeforeInterruptable / attackSpeedStat;
		crocoDamageTypeController = GetComponent<CrocoDamageTypeController>();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	protected override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
	{
		base.AuthorityModifyOverlapAttack(overlapAttack);
		DamageType damageType = (Object.op_Implicit((Object)(object)crocoDamageTypeController) ? crocoDamageTypeController.GetDamageType() : DamageType.Generic);
		overlapAttack.damageType = damageType | DamageType.BonusToLowHealth;
	}

	protected override void PlayAnimation()
	{
		float num = Mathf.Max(duration, 0.2f);
		PlayCrossfade("Gesture, Additive", "Bite", "Bite.playbackRate", num, 0.05f);
		PlayCrossfade("Gesture, Override", "Bite", "Bite.playbackRate", num, 0.05f);
		Util.PlaySound(biteSound, base.gameObject);
	}

	protected override void OnMeleeHitAuthority()
	{
		base.OnMeleeHitAuthority();
		base.characterBody.AddSpreadBloom(bloom);
		if (!hasGrantedBuff)
		{
			hasGrantedBuff = true;
			base.characterBody.AddTimedBuffAuthority(RoR2Content.Buffs.CrocoRegen.buffIndex, 0.5f);
		}
	}

	protected override void BeginMeleeAttackEffect()
	{
		AddRecoil(0.9f * recoilAmplitude, 1.1f * recoilAmplitude, -0.1f * recoilAmplitude, 0.1f * recoilAmplitude);
		base.BeginMeleeAttackEffect();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		if (!(base.fixedAge < durationBeforeInterruptable))
		{
			return InterruptPriority.Skill;
		}
		return InterruptPriority.Pain;
	}
}
