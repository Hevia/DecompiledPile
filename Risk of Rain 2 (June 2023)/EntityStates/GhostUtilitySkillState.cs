using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates;

public class GhostUtilitySkillState : GenericCharacterMain
{
	public static float baseDuration;

	public static GameObject coreVfxPrefab;

	public static GameObject footVfxPrefab;

	public static GameObject entryEffectPrefab;

	public static GameObject exitEffectPrefab;

	public static float moveSpeedCoefficient;

	public static float healFractionPerTick;

	public static float healFrequency;

	private HurtBoxGroup hurtBoxGroup;

	private CharacterModel characterModel;

	private GameObject coreVfxInstance;

	private GameObject footVfxInstance;

	private float healTimer;

	private float duration;

	private ICharacterGravityParameterProvider characterGravityParameterProvider;

	private ICharacterFlightParameterProvider characterFlightParameterProvider;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string playbackRateParam;

	public override void OnEnter()
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration;
		characterGravityParameterProvider = base.gameObject.GetComponent<ICharacterGravityParameterProvider>();
		characterFlightParameterProvider = base.gameObject.GetComponent<ICharacterFlightParameterProvider>();
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			if (Object.op_Implicit((Object)(object)base.characterBody.inventory))
			{
				duration *= base.characterBody.inventory.GetItemCount(RoR2Content.Items.LunarUtilityReplacement);
			}
			hurtBoxGroup = base.characterBody.hurtBoxGroup;
			if (Object.op_Implicit((Object)(object)hurtBoxGroup))
			{
				HurtBoxGroup obj = hurtBoxGroup;
				int hurtBoxesDeactivatorCounter = obj.hurtBoxesDeactivatorCounter + 1;
				obj.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}
			if (Object.op_Implicit((Object)(object)coreVfxPrefab))
			{
				coreVfxInstance = Object.Instantiate<GameObject>(coreVfxPrefab);
			}
			if (Object.op_Implicit((Object)(object)footVfxPrefab))
			{
				footVfxInstance = Object.Instantiate<GameObject>(footVfxPrefab);
			}
			UpdateVfxPositions();
			if (Object.op_Implicit((Object)(object)entryEffectPrefab))
			{
				Ray aimRay = GetAimRay();
				EffectManager.SimpleEffect(entryEffectPrefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), transmit: false);
			}
		}
		Transform modelTransform = GetModelTransform();
		characterModel = ((modelTransform != null) ? ((Component)modelTransform).GetComponent<CharacterModel>() : null);
		if (Object.op_Implicit((Object)(object)base.modelAnimator))
		{
			((Behaviour)base.modelAnimator).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.walkSpeedPenaltyCoefficient = moveSpeedCoefficient;
		}
		if (characterGravityParameterProvider != null)
		{
			CharacterGravityParameters gravityParameters = characterGravityParameterProvider.gravityParameters;
			gravityParameters.channeledAntiGravityGranterCount++;
			characterGravityParameterProvider.gravityParameters = gravityParameters;
		}
		if (characterFlightParameterProvider != null)
		{
			CharacterFlightParameters flightParameters = characterFlightParameterProvider.flightParameters;
			flightParameters.channeledFlightGranterCount++;
			characterFlightParameterProvider.flightParameters = flightParameters;
		}
		if (Object.op_Implicit((Object)(object)characterModel))
		{
			characterModel.invisibilityCount++;
		}
		EntityStateMachine[] components = base.gameObject.GetComponents<EntityStateMachine>();
		foreach (EntityStateMachine entityStateMachine in components)
		{
			if (entityStateMachine.customName == "Weapon")
			{
				entityStateMachine.SetNextStateToMain();
			}
		}
	}

	private void UpdateVfxPositions()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			if (Object.op_Implicit((Object)(object)coreVfxInstance))
			{
				coreVfxInstance.transform.position = base.characterBody.corePosition;
			}
			if (Object.op_Implicit((Object)(object)footVfxInstance))
			{
				footVfxInstance.transform.position = base.characterBody.footPosition;
			}
		}
	}

	protected override bool CanExecuteSkill(GenericSkill skillSlot)
	{
		return false;
	}

	public override void Update()
	{
		base.Update();
		UpdateVfxPositions();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		healTimer -= Time.fixedDeltaTime;
		if (healTimer <= 0f)
		{
			if (NetworkServer.active)
			{
				base.healthComponent.HealFraction(healFractionPerTick, default(ProcChainMask));
			}
			healTimer = 1f / healFrequency;
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)exitEffectPrefab) && !outer.destroying)
		{
			Ray aimRay = GetAimRay();
			EffectManager.SimpleEffect(exitEffectPrefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), transmit: false);
		}
		if (Object.op_Implicit((Object)(object)coreVfxInstance))
		{
			EntityState.Destroy((Object)(object)coreVfxInstance);
		}
		if (Object.op_Implicit((Object)(object)footVfxInstance))
		{
			EntityState.Destroy((Object)(object)footVfxInstance);
		}
		if (Object.op_Implicit((Object)(object)characterModel))
		{
			characterModel.invisibilityCount--;
		}
		if (Object.op_Implicit((Object)(object)hurtBoxGroup))
		{
			HurtBoxGroup obj = hurtBoxGroup;
			int hurtBoxesDeactivatorCounter = obj.hurtBoxesDeactivatorCounter - 1;
			obj.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
		}
		if (Object.op_Implicit((Object)(object)base.modelAnimator))
		{
			((Behaviour)base.modelAnimator).enabled = true;
		}
		if (characterFlightParameterProvider != null)
		{
			CharacterFlightParameters flightParameters = characterFlightParameterProvider.flightParameters;
			flightParameters.channeledFlightGranterCount--;
			characterFlightParameterProvider.flightParameters = flightParameters;
		}
		if (characterGravityParameterProvider != null)
		{
			CharacterGravityParameters gravityParameters = characterGravityParameterProvider.gravityParameters;
			gravityParameters.channeledAntiGravityGranterCount--;
			characterGravityParameterProvider.gravityParameters = gravityParameters;
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
		}
		base.OnExit();
	}
}
