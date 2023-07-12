using KinematicCharacterController;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Mage;

public class FlyUpState : MageCharacterMain
{
	public static GameObject blinkPrefab;

	public static float duration = 0.3f;

	public static string beginSoundString;

	public static string endSoundString;

	public static AnimationCurve speedCoefficientCurve;

	public static GameObject muzzleflashEffect;

	public static float blastAttackRadius;

	public static float blastAttackDamageCoefficient;

	public static float blastAttackProcCoefficient;

	public static float blastAttackForce;

	private Vector3 flyVector = Vector3.zero;

	private Transform modelTransform;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	private Vector3 blastPosition;

	public override void OnEnter()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(beginSoundString, base.gameObject);
		modelTransform = GetModelTransform();
		flyVector = Vector3.up;
		CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
		PlayCrossfade("Body", "FlyUp", "FlyUp.playbackRate", duration, 0.1f);
		((BaseCharacterController)base.characterMotor).Motor.ForceUnground();
		base.characterMotor.velocity = Vector3.zero;
		EffectManager.SimpleMuzzleFlash(muzzleflashEffect, base.gameObject, "MuzzleLeft", transmit: false);
		EffectManager.SimpleMuzzleFlash(muzzleflashEffect, base.gameObject, "MuzzleRight", transmit: false);
		if (base.isAuthority)
		{
			blastPosition = base.characterBody.corePosition;
		}
		if (NetworkServer.active)
		{
			BlastAttack obj = new BlastAttack
			{
				radius = blastAttackRadius,
				procCoefficient = blastAttackProcCoefficient,
				position = blastPosition,
				attacker = base.gameObject,
				crit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master),
				baseDamage = base.characterBody.damage * blastAttackDamageCoefficient,
				falloffModel = BlastAttack.FalloffModel.None,
				baseForce = blastAttackForce
			};
			obj.teamIndex = TeamComponent.GetObjectTeam(obj.attacker);
			obj.damageType = DamageType.Stun1s;
			obj.attackerFiltering = AttackerFiltering.NeverHitSelf;
			obj.Fire();
		}
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		base.OnSerialize(writer);
		writer.Write(blastPosition);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeserialize(reader);
		blastPosition = reader.ReadVector3();
	}

	public override void HandleMovements()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		base.HandleMovements();
		CharacterMotor obj = base.characterMotor;
		obj.rootMotion += flyVector * (moveSpeedStat * speedCoefficientCurve.Evaluate(base.fixedAge / duration) * Time.fixedDeltaTime);
		base.characterMotor.velocity.y = 0f;
	}

	protected override void UpdateAnimationParameters()
	{
		base.UpdateAnimationParameters();
	}

	private void CreateBlinkEffect(Vector3 origin)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData();
		effectData.rotation = Util.QuaternionSafeLookRotation(flyVector);
		effectData.origin = origin;
		EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: false);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		if (!outer.destroying)
		{
			Util.PlaySound(endSoundString, base.gameObject);
		}
		base.OnExit();
	}
}
