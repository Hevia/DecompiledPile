using KinematicCharacterController;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Merc;

public class Assaulter : BaseState
{
	private Transform modelTransform;

	public static GameObject dashPrefab;

	public static float smallHopVelocity;

	public static float dashPrepDuration;

	public static float dashDuration = 0.3f;

	public static float speedCoefficient = 25f;

	public static string beginSoundString;

	public static string endSoundString;

	public static float damageCoefficient;

	public static float procCoefficient;

	public static GameObject hitEffectPrefab;

	public static float hitPauseDuration;

	private float stopwatch;

	private Vector3 dashVector = Vector3.zero;

	private Animator animator;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	private OverlapAttack overlapAttack;

	private ChildLocator childLocator;

	private bool isDashing;

	private bool inHitPause;

	private float hitPauseTimer;

	private CameraTargetParams.AimRequest aimRequest;

	public bool hasHit { get; private set; }

	public int dashIndex { private get; set; }

	public override void OnEnter()
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
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
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
			hurtboxGroup = ((Component)modelTransform).GetComponent<HurtBoxGroup>();
			if (Object.op_Implicit((Object)(object)childLocator))
			{
				((Component)childLocator.FindChild("PreDashEffect")).gameObject.SetActive(true);
			}
		}
		SmallHop(base.characterMotor, smallHopVelocity);
		PlayAnimation("FullBody, Override", "AssaulterPrep", "AssaulterPrep.playbackRate", dashPrepDuration);
		dashVector = base.inputBank.aimDirection;
		overlapAttack = InitMeleeOverlap(damageCoefficient, hitEffectPrefab, modelTransform, "Assaulter");
		overlapAttack.damageType = DamageType.Stun1s;
		if (NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex);
		}
	}

	private void CreateDashEffect()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Transform val = childLocator.FindChild("DashCenter");
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)dashPrefab))
		{
			Object.Instantiate<GameObject>(dashPrefab, val.position, Util.QuaternionSafeLookRotation(dashVector), val);
		}
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			((Component)childLocator.FindChild("PreDashEffect")).gameObject.SetActive(false);
		}
	}

	public override void FixedUpdate()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		base.characterDirection.forward = dashVector;
		if (stopwatch > dashPrepDuration / attackSpeedStat && !isDashing)
		{
			isDashing = true;
			dashVector = base.inputBank.aimDirection;
			CreateDashEffect();
			PlayCrossfade("FullBody, Override", "AssaulterLoop", 0.1f);
			base.gameObject.layer = LayerIndex.fakeActor.intVal;
			((BaseCharacterController)base.characterMotor).Motor.RebuildCollidableLayers();
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
		}
		if (!isDashing)
		{
			stopwatch += Time.fixedDeltaTime;
		}
		else if (base.isAuthority)
		{
			base.characterMotor.velocity = Vector3.zero;
			if (!inHitPause)
			{
				bool num = overlapAttack.Fire();
				stopwatch += Time.fixedDeltaTime;
				if (num)
				{
					if (!hasHit)
					{
						hasHit = true;
					}
					inHitPause = true;
					hitPauseTimer = hitPauseDuration / attackSpeedStat;
					if (Object.op_Implicit((Object)(object)modelTransform))
					{
						TemporaryOverlay temporaryOverlay2 = ((Component)modelTransform).gameObject.AddComponent<TemporaryOverlay>();
						temporaryOverlay2.duration = hitPauseDuration / attackSpeedStat;
						temporaryOverlay2.animateShaderAlpha = true;
						temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
						temporaryOverlay2.destroyComponentOnEnd = true;
						temporaryOverlay2.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matMercEvisTarget");
						temporaryOverlay2.AddToCharacerModel(((Component)modelTransform).GetComponent<CharacterModel>());
					}
				}
				CharacterMotor obj = base.characterMotor;
				obj.rootMotion += dashVector * moveSpeedStat * speedCoefficient * Time.fixedDeltaTime;
			}
			else
			{
				hitPauseTimer -= Time.fixedDeltaTime;
				if (hitPauseTimer < 0f)
				{
					inHitPause = false;
				}
			}
		}
		if (stopwatch >= dashDuration + dashPrepDuration / attackSpeedStat && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		base.gameObject.layer = LayerIndex.defaultLayer.intVal;
		((BaseCharacterController)base.characterMotor).Motor.RebuildCollidableLayers();
		Util.PlaySound(endSoundString, base.gameObject);
		if (base.isAuthority)
		{
			CharacterMotor obj = base.characterMotor;
			obj.velocity *= 0.1f;
			SmallHop(base.characterMotor, smallHopVelocity);
		}
		aimRequest?.Dispose();
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			((Component)childLocator.FindChild("PreDashEffect")).gameObject.SetActive(false);
		}
		PlayAnimation("FullBody, Override", "EvisLoopExit");
		if (NetworkServer.active)
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex);
		}
		base.OnExit();
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write((byte)dashIndex);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		dashIndex = reader.ReadByte();
	}
}
