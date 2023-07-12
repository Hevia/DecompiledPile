using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Merc;

public class GroundLight : BaseState
{
	public enum ComboState
	{
		GroundLight1,
		GroundLight2,
		GroundLight3
	}

	private struct ComboStateInfo
	{
		private string mecanimStateName;

		private string mecanimPlaybackRateName;
	}

	public static float baseComboAttackDuration;

	public static float baseFinisherAttackDuration;

	public static float baseEarlyExitDuration;

	public static string comboAttackSoundString;

	public static string finisherAttackSoundString;

	public static float comboDamageCoefficient;

	public static float finisherDamageCoefficient;

	public static float forceMagnitude;

	public static GameObject comboHitEffectPrefab;

	public static GameObject finisherHitEffectPrefab;

	public static GameObject comboSwingEffectPrefab;

	public static GameObject finisherSwingEffectPrefab;

	public static float hitPauseDuration;

	public static float selfForceMagnitude;

	public static string hitSoundString;

	public static float slashPitch;

	private float stopwatch;

	private float attackDuration;

	private float earlyExitDuration;

	private Animator animator;

	private OverlapAttack overlapAttack;

	private float hitPauseTimer;

	private bool isInHitPause;

	private bool hasSwung;

	private bool hasHit;

	private GameObject swingEffectInstance;

	public ComboState comboState;

	private Vector3 characterForward;

	private string slashChildName;

	private HitStopCachedState hitStopCachedState;

	private GameObject swingEffectPrefab;

	private GameObject hitEffectPrefab;

	private string attackSoundString;

	public override void OnEnter()
	{
		base.OnEnter();
		stopwatch = 0f;
		earlyExitDuration = baseEarlyExitDuration / attackSpeedStat;
		animator = GetModelAnimator();
		bool @bool = animator.GetBool("isMoving");
		bool bool2 = animator.GetBool("isGrounded");
		switch (comboState)
		{
		case ComboState.GroundLight1:
			attackDuration = baseComboAttackDuration / attackSpeedStat;
			overlapAttack = InitMeleeOverlap(comboDamageCoefficient, hitEffectPrefab, GetModelTransform(), "Sword");
			if (@bool || !bool2)
			{
				PlayAnimation("Gesture, Additive", "GroundLight1", "GroundLight.playbackRate", attackDuration);
				PlayAnimation("Gesture, Override", "GroundLight1", "GroundLight.playbackRate", attackDuration);
			}
			else
			{
				PlayAnimation("FullBody, Override", "GroundLight1", "GroundLight.playbackRate", attackDuration);
			}
			slashChildName = "GroundLight1Slash";
			swingEffectPrefab = comboSwingEffectPrefab;
			hitEffectPrefab = comboHitEffectPrefab;
			attackSoundString = comboAttackSoundString;
			break;
		case ComboState.GroundLight2:
			attackDuration = baseComboAttackDuration / attackSpeedStat;
			overlapAttack = InitMeleeOverlap(comboDamageCoefficient, hitEffectPrefab, GetModelTransform(), "Sword");
			if (@bool || !bool2)
			{
				PlayAnimation("Gesture, Additive", "GroundLight2", "GroundLight.playbackRate", attackDuration);
				PlayAnimation("Gesture, Override", "GroundLight2", "GroundLight.playbackRate", attackDuration);
			}
			else
			{
				PlayAnimation("FullBody, Override", "GroundLight2", "GroundLight.playbackRate", attackDuration);
			}
			slashChildName = "GroundLight2Slash";
			swingEffectPrefab = comboSwingEffectPrefab;
			hitEffectPrefab = comboHitEffectPrefab;
			attackSoundString = comboAttackSoundString;
			break;
		case ComboState.GroundLight3:
			attackDuration = baseFinisherAttackDuration / attackSpeedStat;
			overlapAttack = InitMeleeOverlap(finisherDamageCoefficient, hitEffectPrefab, GetModelTransform(), "SwordLarge");
			if (@bool || !bool2)
			{
				PlayAnimation("Gesture, Additive", "GroundLight3", "GroundLight.playbackRate", attackDuration);
				PlayAnimation("Gesture, Override", "GroundLight3", "GroundLight.playbackRate", attackDuration);
			}
			else
			{
				PlayAnimation("FullBody, Override", "GroundLight3", "GroundLight.playbackRate", attackDuration);
			}
			slashChildName = "GroundLight3Slash";
			swingEffectPrefab = finisherSwingEffectPrefab;
			hitEffectPrefab = finisherHitEffectPrefab;
			attackSoundString = finisherAttackSoundString;
			break;
		}
		base.characterBody.SetAimTimer(attackDuration + 1f);
		overlapAttack.hitEffectPrefab = hitEffectPrefab;
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		hitPauseTimer -= Time.fixedDeltaTime;
		if (base.isAuthority)
		{
			bool flag = FireMeleeOverlap(overlapAttack, animator, "Sword.active", forceMagnitude);
			hasHit |= flag;
			if (flag)
			{
				Util.PlaySound(hitSoundString, base.gameObject);
				if (!isInHitPause)
				{
					hitStopCachedState = CreateHitStopCachedState(base.characterMotor, animator, "GroundLight.playbackRate");
					hitPauseTimer = hitPauseDuration / attackSpeedStat;
					isInHitPause = true;
				}
			}
			if (hitPauseTimer <= 0f && isInHitPause)
			{
				ConsumeHitStopCachedState(hitStopCachedState, base.characterMotor, animator);
				isInHitPause = false;
			}
		}
		if (animator.GetFloat("Sword.active") > 0f && !hasSwung)
		{
			Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, slashPitch);
			HealthComponent healthComponent = base.characterBody.healthComponent;
			CharacterDirection component = ((Component)base.characterBody).GetComponent<CharacterDirection>();
			if (Object.op_Implicit((Object)(object)healthComponent))
			{
				healthComponent.TakeDamageForce(selfForceMagnitude * component.forward, alwaysApply: true);
			}
			hasSwung = true;
			EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, slashChildName, transmit: false);
		}
		if (!isInHitPause)
		{
			stopwatch += Time.fixedDeltaTime;
		}
		else
		{
			base.characterMotor.velocity = Vector3.zero;
			animator.SetFloat("GroundLight.playbackRate", 0f);
		}
		if (base.isAuthority && stopwatch >= attackDuration - earlyExitDuration)
		{
			if (!hasSwung)
			{
				overlapAttack.Fire();
			}
			if (base.inputBank.skill1.down && comboState != ComboState.GroundLight3)
			{
				GroundLight groundLight = new GroundLight();
				groundLight.comboState = comboState + 1;
				outer.SetNextState(groundLight);
			}
			else if (stopwatch >= attackDuration)
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write((byte)comboState);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		comboState = (ComboState)reader.ReadByte();
	}
}
