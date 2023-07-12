using KinematicCharacterController;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Merc;

public class Assaulter2 : BasicMeleeAttack
{
	public static float speedCoefficientOnExit;

	public static float speedCoefficient;

	public static string endSoundString;

	public static float exitSmallHop;

	public static GameObject selfOnHitOverlayEffectPrefab;

	public bool grantAnotherDash;

	private Transform modelTransform;

	private Vector3 dashVector;

	private bool bufferedSkill2;

	private Vector3 dashVelocity => dashVector * moveSpeedStat * speedCoefficient;

	public override void OnEnter()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		dashVector = base.inputBank.aimDirection;
		base.gameObject.layer = LayerIndex.fakeActor.intVal;
		((BaseCharacterController)base.characterMotor).Motor.RebuildCollidableLayers();
		((BaseCharacterController)base.characterMotor).Motor.ForceUnground();
		base.characterMotor.velocity = Vector3.zero;
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			TemporaryOverlay temporaryOverlay = ((Component)modelTransform).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay.duration = 0.7f;
			temporaryOverlay.animateShaderAlpha = true;
			temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			temporaryOverlay.destroyComponentOnEnd = true;
			temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matMercEnergized");
			temporaryOverlay.AddToCharacerModel(((Component)modelTransform).GetComponent<CharacterModel>());
		}
		PlayCrossfade("FullBody, Override", "AssaulterLoop", 0.1f);
		base.characterDirection.forward = ((Vector3)(ref base.characterMotor.velocity)).normalized;
		if (NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
		}
	}

	public override void OnExit()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
		}
		CharacterMotor obj = base.characterMotor;
		obj.velocity *= speedCoefficientOnExit;
		SmallHop(base.characterMotor, exitSmallHop);
		Util.PlaySound(endSoundString, base.gameObject);
		PlayAnimation("FullBody, Override", "EvisLoopExit");
		base.gameObject.layer = LayerIndex.defaultLayer.intVal;
		((BaseCharacterController)base.characterMotor).Motor.RebuildCollidableLayers();
		base.OnExit();
	}

	protected override void PlayAnimation()
	{
		base.PlayAnimation();
		PlayCrossfade("FullBody, Override", "AssaulterLoop", 0.1f);
	}

	protected override void AuthorityFixedUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		base.AuthorityFixedUpdate();
		if (!base.authorityInHitPause)
		{
			CharacterMotor obj = base.characterMotor;
			obj.rootMotion += dashVelocity * Time.fixedDeltaTime;
			base.characterDirection.forward = dashVelocity;
			base.characterDirection.moveVector = dashVelocity;
			base.characterBody.isSprinting = true;
			if (bufferedSkill2)
			{
				base.skillLocator.secondary.ExecuteIfReady();
				bufferedSkill2 = false;
			}
		}
		if (Object.op_Implicit((Object)(object)base.skillLocator) && base.skillLocator.secondary.IsReady() && base.inputBank.skill2.down)
		{
			bufferedSkill2 = true;
		}
	}

	protected override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
	{
		base.AuthorityModifyOverlapAttack(overlapAttack);
		overlapAttack.damageType = DamageType.Stun1s;
		overlapAttack.damage = damageCoefficient * damageStat;
	}

	protected override void OnMeleeHitAuthority()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		base.OnMeleeHitAuthority();
		grantAnotherDash = true;
		float num = hitPauseDuration / attackSpeedStat;
		if (Object.op_Implicit((Object)(object)selfOnHitOverlayEffectPrefab) && num > 1f / 30f)
		{
			EffectData effectData = new EffectData
			{
				origin = base.transform.position,
				genericFloat = hitPauseDuration / attackSpeedStat
			};
			effectData.SetNetworkedObjectReference(base.gameObject);
			EffectManager.SpawnEffect(selfOnHitOverlayEffectPrefab, effectData, transmit: true);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
