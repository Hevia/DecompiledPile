using RoR2;
using UnityEngine;

namespace EntityStates.Huntress;

public class BlinkState : BaseState
{
	private Transform modelTransform;

	public static GameObject blinkPrefab;

	private float stopwatch;

	private Vector3 blinkVector = Vector3.zero;

	[SerializeField]
	public float duration = 0.3f;

	[SerializeField]
	public float speedCoefficient = 25f;

	[SerializeField]
	public string beginSoundString;

	[SerializeField]
	public string endSoundString;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	public override void OnEnter()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(beginSoundString, base.gameObject);
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			characterModel = ((Component)modelTransform).GetComponent<CharacterModel>();
			hurtboxGroup = ((Component)modelTransform).GetComponent<HurtBoxGroup>();
		}
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
		blinkVector = GetBlinkVector();
		CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
	}

	protected virtual Vector3 GetBlinkVector()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return base.inputBank.aimDirection;
	}

	private void CreateBlinkEffect(Vector3 origin)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData();
		effectData.rotation = Util.QuaternionSafeLookRotation(blinkVector);
		effectData.origin = origin;
		EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: false);
	}

	public override void FixedUpdate()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (Object.op_Implicit((Object)(object)base.characterMotor) && Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterMotor.velocity = Vector3.zero;
			CharacterMotor obj = base.characterMotor;
			obj.rootMotion += blinkVector * (moveSpeedStat * speedCoefficient * Time.fixedDeltaTime);
		}
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!outer.destroying)
		{
			Util.PlaySound(endSoundString, base.gameObject);
			CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
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
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.disableAirControlUntilCollision = false;
		}
		base.OnExit();
	}
}
