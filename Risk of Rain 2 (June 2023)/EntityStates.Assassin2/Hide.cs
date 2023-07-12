using KinematicCharacterController;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Assassin2;

public class Hide : BaseState
{
	private Transform modelTransform;

	public static GameObject hideEfffectPrefab;

	public static GameObject smokeEffectPrefab;

	public static Material destealthMaterial;

	private float stopwatch;

	private Vector3 blinkDestination = Vector3.zero;

	private Vector3 blinkStart = Vector3.zero;

	[Tooltip("the length of time to stay hidden")]
	public static float hiddenDuration = 5f;

	[Tooltip("the entire duration of the hidden state (hidden time + time after)")]
	public static float fullDuration = 10f;

	public static string beginSoundString;

	public static string endSoundString;

	private Animator animator;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	private bool hidden;

	private GameObject smokeEffectInstance;

	public override void OnEnter()
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(beginSoundString, base.gameObject);
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			animator = ((Component)modelTransform).GetComponent<Animator>();
			characterModel = ((Component)modelTransform).GetComponent<CharacterModel>();
			hurtboxGroup = ((Component)modelTransform).GetComponent<HurtBoxGroup>();
			if (Object.op_Implicit((Object)(object)smokeEffectPrefab))
			{
				Transform val = modelTransform;
				if (Object.op_Implicit((Object)(object)val))
				{
					smokeEffectInstance = Object.Instantiate<GameObject>(smokeEffectPrefab, val);
					ScaleParticleSystemDuration component = smokeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
					if (Object.op_Implicit((Object)(object)component))
					{
						component.newDuration = component.initialDuration;
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)hurtboxGroup))
		{
			HurtBoxGroup hurtBoxGroup = hurtboxGroup;
			int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
			hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
		}
		PlayAnimation("Gesture", "Disappear");
		if (Object.op_Implicit((Object)(object)base.characterBody) && NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.Cloak);
		}
		CreateHiddenEffect(Util.GetCorePosition(base.gameObject));
		if (Object.op_Implicit((Object)(object)base.healthComponent))
		{
			base.healthComponent.dontShowHealthbar = true;
		}
		hidden = true;
	}

	private void CreateHiddenEffect(Vector3 origin)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData();
		effectData.origin = origin;
		EffectManager.SpawnEffect(hideEfffectPrefab, effectData, transmit: false);
	}

	private void SetPosition(Vector3 newPosition)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			((BaseCharacterController)base.characterMotor).Motor.SetPositionAndRotation(newPosition, Quaternion.identity, true);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= hiddenDuration && hidden)
		{
			Reveal();
		}
		if (base.isAuthority && stopwatch > fullDuration)
		{
			outer.SetNextStateToMain();
		}
	}

	private void Reveal()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound(endSoundString, base.gameObject);
		CreateHiddenEffect(Util.GetCorePosition(base.gameObject));
		if (Object.op_Implicit((Object)(object)modelTransform) && Object.op_Implicit((Object)(object)destealthMaterial))
		{
			TemporaryOverlay temporaryOverlay = ((Component)animator).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay.duration = 1f;
			temporaryOverlay.destroyComponentOnEnd = true;
			temporaryOverlay.originalMaterial = destealthMaterial;
			temporaryOverlay.inspectorCharacterModel = ((Component)animator).gameObject.GetComponent<CharacterModel>();
			temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			temporaryOverlay.animateShaderAlpha = true;
		}
		if (Object.op_Implicit((Object)(object)hurtboxGroup))
		{
			HurtBoxGroup hurtBoxGroup = hurtboxGroup;
			int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
			hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			((Behaviour)base.characterMotor).enabled = true;
		}
		PlayAnimation("Gesture", "Appear");
		if (Object.op_Implicit((Object)(object)base.characterBody) && NetworkServer.active)
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
		}
		if (Object.op_Implicit((Object)(object)base.healthComponent))
		{
			base.healthComponent.dontShowHealthbar = false;
		}
		if (Object.op_Implicit((Object)(object)smokeEffectInstance))
		{
			EntityState.Destroy((Object)(object)smokeEffectInstance);
		}
		hidden = false;
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
