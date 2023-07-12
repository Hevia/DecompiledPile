using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates;

public class EntityState
{
	public EntityStateMachine outer;

	protected float age { get; set; }

	protected float fixedAge { get; set; }

	protected GameObject gameObject => ((Component)outer).gameObject;

	protected bool isLocalPlayer
	{
		get
		{
			if (Object.op_Implicit((Object)(object)outer.networker))
			{
				return ((NetworkBehaviour)outer.networker).isLocalPlayer;
			}
			return false;
		}
	}

	protected bool localPlayerAuthority
	{
		get
		{
			if (Object.op_Implicit((Object)(object)outer.networker))
			{
				return ((NetworkBehaviour)outer.networker).localPlayerAuthority;
			}
			return false;
		}
	}

	protected bool isAuthority => Util.HasEffectiveAuthority(outer.networkIdentity);

	protected Transform transform => outer.commonComponents.transform;

	protected CharacterBody characterBody => outer.commonComponents.characterBody;

	protected CharacterMotor characterMotor => outer.commonComponents.characterMotor;

	protected CharacterDirection characterDirection => outer.commonComponents.characterDirection;

	protected Rigidbody rigidbody => outer.commonComponents.rigidbody;

	protected RigidbodyMotor rigidbodyMotor => outer.commonComponents.rigidbodyMotor;

	protected RigidbodyDirection rigidbodyDirection => outer.commonComponents.rigidbodyDirection;

	protected RailMotor railMotor => outer.commonComponents.railMotor;

	protected ModelLocator modelLocator => outer.commonComponents.modelLocator;

	protected InputBankTest inputBank => outer.commonComponents.inputBank;

	protected TeamComponent teamComponent => outer.commonComponents.teamComponent;

	protected HealthComponent healthComponent => outer.commonComponents.healthComponent;

	protected SkillLocator skillLocator => outer.commonComponents.skillLocator;

	protected CharacterEmoteDefinitions characterEmoteDefinitions => outer.commonComponents.characterEmoteDefinitions;

	protected CameraTargetParams cameraTargetParams => outer.commonComponents.cameraTargetParams;

	protected SfxLocator sfxLocator => outer.commonComponents.sfxLocator;

	protected BodyAnimatorSmoothingParameters bodyAnimatorSmoothingParameters => outer.commonComponents.bodyAnimatorSmoothingParameters;

	protected ProjectileController projectileController => outer.commonComponents.projectileController;

	public EntityState()
	{
		EntityStateCatalog.InitializeStateFields(this);
	}

	public virtual void OnEnter()
	{
	}

	public virtual void OnExit()
	{
	}

	public virtual void ModifyNextState(EntityState nextState)
	{
	}

	public virtual void Update()
	{
		age += Time.deltaTime;
	}

	public virtual void FixedUpdate()
	{
		fixedAge += Time.fixedDeltaTime;
	}

	public virtual void OnSerialize(NetworkWriter writer)
	{
	}

	public virtual void OnDeserialize(NetworkReader reader)
	{
	}

	public virtual InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Any;
	}

	protected static void Destroy(Object obj)
	{
		Object.Destroy(obj);
	}

	protected T GetComponent<T>() where T : Component
	{
		return ((Component)outer).GetComponent<T>();
	}

	protected Component GetComponent(Type type)
	{
		return ((Component)outer).GetComponent(type);
	}

	protected Component GetComponent(string type)
	{
		return ((Component)outer).GetComponent(type);
	}

	protected Transform GetModelBaseTransform()
	{
		if (!Object.op_Implicit((Object)(object)modelLocator))
		{
			return null;
		}
		return modelLocator.modelBaseTransform;
	}

	protected Transform GetModelTransform()
	{
		if (!Object.op_Implicit((Object)(object)modelLocator))
		{
			return null;
		}
		return modelLocator.modelTransform;
	}

	protected AimAnimator GetAimAnimator()
	{
		if (Object.op_Implicit((Object)(object)modelLocator) && Object.op_Implicit((Object)(object)modelLocator.modelTransform))
		{
			return ((Component)modelLocator.modelTransform).GetComponent<AimAnimator>();
		}
		return null;
	}

	protected Animator GetModelAnimator()
	{
		if (Object.op_Implicit((Object)(object)modelLocator) && Object.op_Implicit((Object)(object)modelLocator.modelTransform))
		{
			return ((Component)modelLocator.modelTransform).GetComponent<Animator>();
		}
		return null;
	}

	protected ChildLocator GetModelChildLocator()
	{
		if (Object.op_Implicit((Object)(object)modelLocator) && Object.op_Implicit((Object)(object)modelLocator.modelTransform))
		{
			return ((Component)modelLocator.modelTransform).GetComponent<ChildLocator>();
		}
		return null;
	}

	protected RootMotionAccumulator GetModelRootMotionAccumulator()
	{
		if (Object.op_Implicit((Object)(object)modelLocator) && Object.op_Implicit((Object)(object)modelLocator.modelTransform))
		{
			return ((Component)modelLocator.modelTransform).GetComponent<RootMotionAccumulator>();
		}
		return null;
	}

	protected void PlayAnimation(string layerName, string animationStateName, string playbackRateParam, float duration)
	{
		if (duration <= 0f)
		{
			Debug.LogWarningFormat("EntityState.PlayAnimation: Zero duration is not allowed. type={0}", new object[1] { GetType().Name });
			return;
		}
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayAnimationOnAnimator(modelAnimator, layerName, animationStateName, playbackRateParam, duration);
		}
	}

	protected static void PlayAnimationOnAnimator(Animator modelAnimator, string layerName, string animationStateName, string playbackRateParam, float duration)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		modelAnimator.speed = 1f;
		modelAnimator.Update(0f);
		int layerIndex = modelAnimator.GetLayerIndex(layerName);
		if (layerIndex >= 0)
		{
			modelAnimator.SetFloat(playbackRateParam, 1f);
			modelAnimator.PlayInFixedTime(animationStateName, layerIndex, 0f);
			modelAnimator.Update(0f);
			AnimatorStateInfo currentAnimatorStateInfo = modelAnimator.GetCurrentAnimatorStateInfo(layerIndex);
			float length = ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).length;
			modelAnimator.SetFloat(playbackRateParam, length / duration);
		}
	}

	protected void PlayCrossfade(string layerName, string animationStateName, string playbackRateParam, float duration, float crossfadeDuration)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (duration <= 0f)
		{
			Debug.LogWarningFormat("EntityState.PlayCrossfade: Zero duration is not allowed. type={0}", new object[1] { GetType().Name });
			return;
		}
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.speed = 1f;
			modelAnimator.Update(0f);
			int layerIndex = modelAnimator.GetLayerIndex(layerName);
			modelAnimator.SetFloat(playbackRateParam, 1f);
			modelAnimator.CrossFadeInFixedTime(animationStateName, crossfadeDuration, layerIndex);
			modelAnimator.Update(0f);
			AnimatorStateInfo nextAnimatorStateInfo = modelAnimator.GetNextAnimatorStateInfo(layerIndex);
			float length = ((AnimatorStateInfo)(ref nextAnimatorStateInfo)).length;
			modelAnimator.SetFloat(playbackRateParam, length / duration);
		}
	}

	protected void PlayCrossfade(string layerName, string animationStateName, float crossfadeDuration)
	{
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.speed = 1f;
			modelAnimator.Update(0f);
			int layerIndex = modelAnimator.GetLayerIndex(layerName);
			modelAnimator.CrossFadeInFixedTime(animationStateName, crossfadeDuration, layerIndex);
		}
	}

	public virtual void PlayAnimation(string layerName, string animationStateName)
	{
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayAnimationOnAnimator(modelAnimator, layerName, animationStateName);
		}
	}

	protected static void PlayAnimationOnAnimator(Animator modelAnimator, string layerName, string animationStateName)
	{
		int layerIndex = modelAnimator.GetLayerIndex(layerName);
		modelAnimator.speed = 1f;
		modelAnimator.Update(0f);
		modelAnimator.PlayInFixedTime(animationStateName, layerIndex, 0f);
	}

	protected void GetBodyAnimatorSmoothingParameters(out BodyAnimatorSmoothingParameters.SmoothingParameters smoothingParameters)
	{
		if (Object.op_Implicit((Object)(object)bodyAnimatorSmoothingParameters))
		{
			smoothingParameters = bodyAnimatorSmoothingParameters.smoothingParameters;
		}
		else
		{
			smoothingParameters = BodyAnimatorSmoothingParameters.defaultParameters;
		}
	}
}
