using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Merc;

public class EvisDash : BaseState
{
	private Transform modelTransform;

	public static GameObject blinkPrefab;

	private float stopwatch;

	private Vector3 dashVector = Vector3.zero;

	public static float smallHopVelocity;

	public static float dashPrepDuration;

	public static float dashDuration = 0.3f;

	public static float speedCoefficient = 25f;

	public static string beginSoundString;

	public static string endSoundString;

	public static float overlapSphereRadius;

	public static float lollypopFactor;

	private Animator animator;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	private bool isDashing;

	private CameraTargetParams.AimRequest aimRequest;

	public override void OnEnter()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(beginSoundString, base.gameObject);
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)base.cameraTargetParams))
		{
			aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
		}
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			animator = ((Component)modelTransform).GetComponent<Animator>();
			characterModel = ((Component)modelTransform).GetComponent<CharacterModel>();
		}
		if (base.isAuthority)
		{
			SmallHop(base.characterMotor, smallHopVelocity);
		}
		if (NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
		}
		PlayAnimation("FullBody, Override", "EvisPrep", "EvisPrep.playbackRate", dashPrepDuration);
		dashVector = base.inputBank.aimDirection;
		base.characterDirection.forward = dashVector;
	}

	private void CreateBlinkEffect(Vector3 origin)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData();
		effectData.rotation = Util.QuaternionSafeLookRotation(dashVector);
		effectData.origin = origin;
		EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: false);
	}

	public override void FixedUpdate()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch > dashPrepDuration && !isDashing)
		{
			isDashing = true;
			dashVector = base.inputBank.aimDirection;
			CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
			PlayCrossfade("FullBody, Override", "EvisLoop", 0.1f);
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				TemporaryOverlay temporaryOverlay = ((Component)modelTransform).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = 0.6f;
				temporaryOverlay.animateShaderAlpha = true;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashBright");
				temporaryOverlay.AddToCharacerModel(((Component)modelTransform).GetComponent<CharacterModel>());
				TemporaryOverlay temporaryOverlay2 = ((Component)modelTransform).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay2.duration = 0.7f;
				temporaryOverlay2.animateShaderAlpha = true;
				temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay2.destroyComponentOnEnd = true;
				temporaryOverlay2.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashExpanded");
				temporaryOverlay2.AddToCharacerModel(((Component)modelTransform).GetComponent<CharacterModel>());
			}
		}
		bool flag = stopwatch >= dashDuration + dashPrepDuration;
		if (isDashing)
		{
			if (Object.op_Implicit((Object)(object)base.characterMotor) && Object.op_Implicit((Object)(object)base.characterDirection))
			{
				CharacterMotor obj = base.characterMotor;
				obj.rootMotion += dashVector * (moveSpeedStat * speedCoefficient * Time.fixedDeltaTime);
			}
			if (base.isAuthority)
			{
				Collider[] array = Physics.OverlapSphere(base.transform.position, base.characterBody.radius + overlapSphereRadius * (flag ? lollypopFactor : 1f), LayerMask.op_Implicit(LayerIndex.entityPrecise.mask));
				for (int i = 0; i < array.Length; i++)
				{
					HurtBox component = ((Component)array[i]).GetComponent<HurtBox>();
					if (Object.op_Implicit((Object)(object)component) && (Object)(object)component.healthComponent != (Object)(object)base.healthComponent)
					{
						Evis nextState = new Evis();
						outer.SetNextState(nextState);
						return;
					}
				}
			}
		}
		if (flag && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound(endSoundString, base.gameObject);
		CharacterMotor obj = base.characterMotor;
		obj.velocity *= 0.1f;
		SmallHop(base.characterMotor, smallHopVelocity);
		aimRequest?.Dispose();
		PlayAnimation("FullBody, Override", "EvisLoopExit");
		if (NetworkServer.active)
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
		}
		base.OnExit();
	}
}
