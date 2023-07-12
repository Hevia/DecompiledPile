using KinematicCharacterController;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidSurvivor;

public class VoidBlinkBase : BaseState
{
	public class VoidBlinkUp : VoidBlinkBase
	{
	}

	public class VoidBlinkDown : VoidBlinkBase
	{
	}

	private Transform modelTransform;

	[SerializeField]
	public GameObject blinkEffectPrefab;

	[SerializeField]
	public float duration = 0.3f;

	[SerializeField]
	public float speedCoefficient = 25f;

	[SerializeField]
	public string beginSoundString;

	[SerializeField]
	public string endSoundString;

	[SerializeField]
	public AnimationCurve forwardSpeed;

	[SerializeField]
	public AnimationCurve upSpeed;

	[SerializeField]
	public GameObject blinkVfxPrefab;

	[SerializeField]
	public float overlayDuration;

	[SerializeField]
	public Material overlayMaterial;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	private Vector3 forwardVector;

	private GameObject blinkVfxInstance;

	private uint soundID;

	public override void OnEnter()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		soundID = Util.PlaySound(beginSoundString, base.gameObject);
		Vector3 val = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector);
		forwardVector = ((Vector3)(ref val)).normalized;
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
		if (NetworkServer.active)
		{
			Util.CleanseBody(base.characterBody, removeDebuffs: true, removeBuffs: false, removeCooldownBuffs: false, removeDots: true, removeStun: true, removeNearbyProjectiles: false);
		}
		blinkVfxInstance = Object.Instantiate<GameObject>(blinkVfxPrefab);
		blinkVfxInstance.transform.SetParent(base.transform, false);
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
		effectData.rotation = Util.QuaternionSafeLookRotation(GetVelocity());
		effectData.origin = origin;
		EffectManager.SpawnEffect(blinkEffectPrefab, effectData, transmit: false);
	}

	private Vector3 GetVelocity()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		float num = base.fixedAge / duration;
		Vector3 val = forwardSpeed.Evaluate(num) * forwardVector;
		Vector3 val2 = upSpeed.Evaluate(num) * Vector3.up;
		return (val + val2) * speedCoefficient * moveSpeedStat;
	}

	public override void FixedUpdate()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)base.characterMotor) && Object.op_Implicit((Object)(object)base.characterDirection))
		{
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				((BaseCharacterController)base.characterMotor).Motor.ForceUnground();
			}
			Vector3 velocity = GetVelocity();
			base.characterMotor.velocity = velocity;
			if (Object.op_Implicit((Object)(object)blinkVfxInstance))
			{
				blinkVfxInstance.transform.forward = velocity;
			}
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		AkSoundEngine.StopPlayingID(soundID);
		if (!outer.destroying)
		{
			Util.PlaySound(endSoundString, base.gameObject);
			CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
		}
		if (Object.op_Implicit((Object)(object)blinkVfxInstance))
		{
			VfxKillBehavior.KillVfxObject(blinkVfxInstance);
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
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			TemporaryOverlay temporaryOverlay = ((Component)modelTransform).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay.duration = overlayDuration;
			temporaryOverlay.animateShaderAlpha = true;
			temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			temporaryOverlay.destroyComponentOnEnd = true;
			temporaryOverlay.originalMaterial = overlayMaterial;
			temporaryOverlay.AddToCharacerModel(((Component)modelTransform).GetComponent<CharacterModel>());
		}
		base.OnExit();
	}
}
