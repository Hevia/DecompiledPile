using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Toolbot;

public class ToolbotDash : BaseCharacterMain
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public float speedMultiplier;

	public static float chargeDamageCoefficient;

	public static float awayForceMagnitude;

	public static float upwardForceMagnitude;

	public static GameObject impactEffectPrefab;

	public static float hitPauseDuration;

	public static string impactSoundString;

	public static float recoilAmplitude;

	public static string startSoundString;

	public static string endSoundString;

	public static GameObject knockbackEffectPrefab;

	public static float knockbackDamageCoefficient;

	public static float massThresholdForKnockback;

	public static float knockbackForce;

	[SerializeField]
	public GameObject startEffectPrefab;

	[SerializeField]
	public GameObject endEffectPrefab;

	private uint soundID;

	private float duration;

	private float hitPauseTimer;

	private Vector3 idealDirection;

	private OverlapAttack attack;

	private bool inHitPause;

	private List<HurtBox> victimsStruck = new List<HurtBox>();

	public override void OnEnter()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration;
		if (base.isAuthority)
		{
			if (Object.op_Implicit((Object)(object)base.inputBank))
			{
				idealDirection = base.inputBank.aimDirection;
				idealDirection.y = 0f;
			}
			UpdateDirection();
		}
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			base.modelLocator.normalizeToFloor = true;
		}
		if (Object.op_Implicit((Object)(object)startEffectPrefab) && Object.op_Implicit((Object)(object)base.characterBody))
		{
			EffectManager.SpawnEffect(startEffectPrefab, new EffectData
			{
				origin = base.characterBody.corePosition
			}, transmit: false);
		}
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.forward = idealDirection;
		}
		soundID = Util.PlaySound(startSoundString, base.gameObject);
		PlayCrossfade("Body", "BoxModeEnter", 0.1f);
		PlayCrossfade("Stance, Override", "PutAwayGun", 0.1f);
		base.modelAnimator.SetFloat("aimWeight", 0f);
		if (NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);
		}
		HitBoxGroup hitBoxGroup = null;
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Charge");
		}
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = GetTeam();
		attack.damage = chargeDamageCoefficient * damageStat;
		attack.hitEffectPrefab = impactEffectPrefab;
		attack.forceVector = Vector3.up * upwardForceMagnitude;
		attack.pushAwayForce = awayForceMagnitude;
		attack.hitBoxGroup = hitBoxGroup;
		attack.isCrit = RollCrit();
	}

	public override void OnExit()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		AkSoundEngine.StopPlayingID(soundID);
		Util.PlaySound(endSoundString, base.gameObject);
		if (!outer.destroying && Object.op_Implicit((Object)(object)base.characterBody))
		{
			if (Object.op_Implicit((Object)(object)endEffectPrefab))
			{
				EffectManager.SpawnEffect(endEffectPrefab, new EffectData
				{
					origin = base.characterBody.corePosition
				}, transmit: false);
			}
			PlayAnimation("Body", "BoxModeExit");
			PlayCrossfade("Stance, Override", "Empty", 0.1f);
			base.characterBody.isSprinting = false;
			if (NetworkServer.active)
			{
				base.characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor) && !base.characterMotor.disableAirControlUntilCollision)
		{
			CharacterMotor obj = base.characterMotor;
			obj.velocity += GetIdealVelocity();
		}
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			base.modelLocator.normalizeToFloor = false;
		}
		base.modelAnimator.SetFloat("aimWeight", 1f);
		base.OnExit();
	}

	private float GetDamageBoostFromSpeed()
	{
		return Mathf.Max(1f, base.characterBody.moveSpeed / base.characterBody.baseMoveSpeed);
	}

	private void UpdateDirection()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			Vector2 val = Util.Vector3XZToVector2XY(base.inputBank.moveVector);
			if (val != Vector2.zero)
			{
				((Vector2)(ref val)).Normalize();
				Vector3 val2 = new Vector3(val.x, 0f, val.y);
				idealDirection = ((Vector3)(ref val2)).normalized;
			}
		}
	}

	private Vector3 GetIdealVelocity()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return base.characterDirection.forward * base.characterBody.moveSpeed * speedMultiplier;
	}

	public override void FixedUpdate()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
		else
		{
			if (!base.isAuthority)
			{
				return;
			}
			if (Object.op_Implicit((Object)(object)base.characterBody))
			{
				base.characterBody.isSprinting = true;
			}
			if (Object.op_Implicit((Object)(object)base.skillLocator.special) && base.inputBank.skill4.down)
			{
				base.skillLocator.special.ExecuteIfReady();
			}
			UpdateDirection();
			if (!inHitPause)
			{
				if (Object.op_Implicit((Object)(object)base.characterDirection))
				{
					base.characterDirection.moveVector = idealDirection;
					if (Object.op_Implicit((Object)(object)base.characterMotor) && !base.characterMotor.disableAirControlUntilCollision)
					{
						CharacterMotor obj = base.characterMotor;
						obj.rootMotion += GetIdealVelocity() * Time.fixedDeltaTime;
					}
				}
				attack.damage = damageStat * (chargeDamageCoefficient * GetDamageBoostFromSpeed());
				if (!attack.Fire(victimsStruck))
				{
					return;
				}
				Util.PlaySound(impactSoundString, base.gameObject);
				inHitPause = true;
				hitPauseTimer = hitPauseDuration;
				AddRecoil(-0.5f * recoilAmplitude, -0.5f * recoilAmplitude, -0.5f * recoilAmplitude, 0.5f * recoilAmplitude);
				PlayAnimation("Gesture, Additive", "BoxModeImpact", "BoxModeImpact.playbackRate", hitPauseDuration);
				for (int i = 0; i < victimsStruck.Count; i++)
				{
					float num = 0f;
					HurtBox hurtBox = victimsStruck[i];
					if (!Object.op_Implicit((Object)(object)hurtBox.healthComponent))
					{
						continue;
					}
					CharacterMotor component = ((Component)hurtBox.healthComponent).GetComponent<CharacterMotor>();
					if (Object.op_Implicit((Object)(object)component))
					{
						num = component.mass;
					}
					else
					{
						Rigidbody component2 = ((Component)hurtBox.healthComponent).GetComponent<Rigidbody>();
						if (Object.op_Implicit((Object)(object)component2))
						{
							num = component2.mass;
						}
					}
					if (num >= massThresholdForKnockback)
					{
						outer.SetNextState(new ToolbotDashImpact
						{
							victimHealthComponent = hurtBox.healthComponent,
							idealDirection = idealDirection,
							damageBoostFromSpeed = GetDamageBoostFromSpeed(),
							isCrit = attack.isCrit
						});
						break;
					}
				}
			}
			else
			{
				base.characterMotor.velocity = Vector3.zero;
				hitPauseTimer -= Time.fixedDeltaTime;
				if (hitPauseTimer < 0f)
				{
					inHitPause = false;
				}
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Frozen;
	}
}
