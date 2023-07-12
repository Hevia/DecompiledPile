using RoR2;
using UnityEngine;

namespace EntityStates.Huntress;

public class BaseBeginArrowBarrage : BaseState
{
	private Transform modelTransform;

	[SerializeField]
	public float basePrepDuration;

	[SerializeField]
	public float blinkDuration = 0.3f;

	[SerializeField]
	public float jumpCoefficient = 25f;

	public static GameObject blinkPrefab;

	public static string blinkSoundString;

	[SerializeField]
	public Vector3 blinkVector;

	private Vector3 worldBlinkVector;

	private float prepDuration;

	private bool beginBlink;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	protected CameraTargetParams.AimRequest aimRequest;

	public override void OnEnter()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(blinkSoundString, base.gameObject);
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			characterModel = ((Component)modelTransform).GetComponent<CharacterModel>();
			hurtboxGroup = ((Component)modelTransform).GetComponent<HurtBoxGroup>();
		}
		prepDuration = basePrepDuration / attackSpeedStat;
		PlayAnimation("FullBody, Override", "BeginArrowRain", "BeginArrowRain.playbackRate", prepDuration);
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity = Vector3.zero;
		}
		if (Object.op_Implicit((Object)(object)base.cameraTargetParams))
		{
			aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
		}
		Ray aimRay = GetAimRay();
		Vector3 direction = ((Ray)(ref aimRay)).direction;
		direction.y = 0f;
		((Vector3)(ref direction)).Normalize();
		Vector3 up = Vector3.up;
		Matrix4x4 val = Matrix4x4.TRS(base.transform.position, Util.QuaternionSafeLookRotation(direction, up), new Vector3(1f, 1f, 1f));
		worldBlinkVector = ((Matrix4x4)(ref val)).MultiplyPoint3x4(blinkVector) - base.transform.position;
		((Vector3)(ref worldBlinkVector)).Normalize();
	}

	private void CreateBlinkEffect(Vector3 origin)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData();
		effectData.rotation = Util.QuaternionSafeLookRotation(worldBlinkVector);
		effectData.origin = origin;
		EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: false);
	}

	public override void FixedUpdate()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.fixedAge >= prepDuration && !beginBlink)
		{
			beginBlink = true;
			CreateBlinkEffect(base.transform.position);
			if (Object.op_Implicit((Object)(object)characterModel))
			{
				characterModel.invisibilityCount++;
			}
			if (Object.op_Implicit((Object)(object)hurtboxGroup))
			{
				HurtBoxGroup hurtBoxGroup = hurtboxGroup;
				int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
				hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}
		}
		if (beginBlink && Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity = Vector3.zero;
			CharacterMotor obj = base.characterMotor;
			obj.rootMotion += worldBlinkVector * (base.characterBody.jumpPower * jumpCoefficient * Time.fixedDeltaTime);
		}
		if (base.fixedAge >= blinkDuration + prepDuration && base.isAuthority)
		{
			outer.SetNextState(InstantiateNextState());
		}
	}

	protected virtual EntityState InstantiateNextState()
	{
		return null;
	}

	public override void OnExit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		CreateBlinkEffect(base.transform.position);
		modelTransform = GetModelTransform();
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
		if (Object.op_Implicit((Object)(object)characterModel))
		{
			characterModel.invisibilityCount--;
		}
		if (Object.op_Implicit((Object)(object)hurtboxGroup))
		{
			HurtBoxGroup hurtBoxGroup = hurtboxGroup;
			int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
			hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
		}
		aimRequest?.Dispose();
		base.OnExit();
	}
}
