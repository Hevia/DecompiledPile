using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates;

public class GenericCharacterDeath : BaseState
{
	private static readonly float bodyPreservationDuration = 1f;

	private static readonly float hardCutoffDuration = 10f;

	private static readonly float maxFallDuration = 4f;

	private static readonly float minTimeToKeepBodyForNetworkMessages = 0.5f;

	public static GameObject voidDeathEffect;

	private float restStopwatch;

	private float fallingStopwatch;

	private bool bodyMarkedForDestructionServer;

	private CameraTargetParams.AimRequest aimRequest;

	protected Transform cachedModelTransform { get; private set; }

	protected bool isBrittle { get; private set; }

	protected bool isVoidDeath { get; private set; }

	protected bool isPlayerDeath { get; private set; }

	protected virtual bool shouldAutoDestroy => true;

	protected virtual float GetDeathAnimationCrossFadeDuration()
	{
		return 0.1f;
	}

	public override void OnEnter()
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		bodyMarkedForDestructionServer = false;
		cachedModelTransform = (Object.op_Implicit((Object)(object)base.modelLocator) ? base.modelLocator.modelTransform : null);
		isBrittle = Object.op_Implicit((Object)(object)base.characterBody) && base.characterBody.isGlass;
		isVoidDeath = Object.op_Implicit((Object)(object)base.healthComponent) && (base.healthComponent.killingDamageType & DamageType.VoidDeath) != 0;
		isPlayerDeath = Object.op_Implicit((Object)(object)base.characterBody.master) && (Object)(object)((Component)base.characterBody.master).GetComponent<PlayerCharacterMasterController>() != (Object)null;
		if (isVoidDeath)
		{
			if (Object.op_Implicit((Object)(object)base.characterBody) && base.isAuthority)
			{
				EffectManager.SpawnEffect(voidDeathEffect, new EffectData
				{
					origin = base.characterBody.corePosition,
					scale = base.characterBody.bestFitRadius
				}, transmit: true);
			}
			if (Object.op_Implicit((Object)(object)cachedModelTransform))
			{
				EntityState.Destroy((Object)(object)((Component)cachedModelTransform).gameObject);
				cachedModelTransform = null;
			}
		}
		if (isPlayerDeath && Object.op_Implicit((Object)(object)base.characterBody))
		{
			Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/PlayerDeathEffect"), base.characterBody.corePosition, Quaternion.identity).GetComponent<LocalCameraEffect>().targetCharacter = ((Component)base.characterBody).gameObject;
		}
		if (Object.op_Implicit((Object)(object)cachedModelTransform))
		{
			if (isBrittle)
			{
				TemporaryOverlay temporaryOverlay = ((Component)cachedModelTransform).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = 0.5f;
				temporaryOverlay.destroyObjectOnEnd = true;
				temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matShatteredGlass");
				temporaryOverlay.destroyEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BrittleDeath");
				temporaryOverlay.destroyEffectChildString = "Chest";
				temporaryOverlay.inspectorCharacterModel = ((Component)cachedModelTransform).gameObject.GetComponent<CharacterModel>();
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
				temporaryOverlay.animateShaderAlpha = true;
			}
			if (Object.op_Implicit((Object)(object)base.cameraTargetParams))
			{
				ChildLocator component = ((Component)cachedModelTransform).GetComponent<ChildLocator>();
				if (Object.op_Implicit((Object)(object)component))
				{
					Transform val = component.FindChild("Chest");
					if (Object.op_Implicit((Object)(object)val))
					{
						base.cameraTargetParams.cameraPivotTransform = val;
						aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
						base.cameraTargetParams.dontRaycastToPivot = true;
					}
				}
			}
		}
		if (!isVoidDeath)
		{
			PlayDeathSound();
			PlayDeathAnimation();
			CreateDeathEffects();
		}
	}

	protected virtual void PlayDeathSound()
	{
		if (Object.op_Implicit((Object)(object)base.sfxLocator) && base.sfxLocator.deathSound != "")
		{
			Util.PlaySound(base.sfxLocator.deathSound, base.gameObject);
		}
	}

	protected virtual void PlayDeathAnimation(float crossfadeDuration = 0.1f)
	{
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.CrossFadeInFixedTime("Death", crossfadeDuration);
		}
	}

	protected virtual void CreateDeathEffects()
	{
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!NetworkServer.active)
		{
			return;
		}
		bool flag = false;
		bool flag2 = true;
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			flag = base.characterMotor.isGrounded;
			flag2 = base.characterMotor.atRest;
		}
		else if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
		{
			flag = false;
			flag2 = false;
		}
		fallingStopwatch = (flag ? 0f : (fallingStopwatch + Time.fixedDeltaTime));
		restStopwatch = ((!flag2) ? 0f : (restStopwatch + Time.fixedDeltaTime));
		if (!(base.fixedAge >= minTimeToKeepBodyForNetworkMessages))
		{
			return;
		}
		if (!bodyMarkedForDestructionServer)
		{
			if ((restStopwatch >= bodyPreservationDuration || fallingStopwatch >= maxFallDuration || base.fixedAge > hardCutoffDuration) && shouldAutoDestroy)
			{
				DestroyBodyAsapServer();
			}
		}
		else
		{
			OnPreDestroyBodyServer();
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}

	protected void DestroyBodyAsapServer()
	{
		bodyMarkedForDestructionServer = true;
	}

	protected virtual void OnPreDestroyBodyServer()
	{
	}

	protected void DestroyModel()
	{
		if (Object.op_Implicit((Object)(object)cachedModelTransform))
		{
			EntityState.Destroy((Object)(object)((Component)cachedModelTransform).gameObject);
			cachedModelTransform = null;
		}
	}

	public override void OnExit()
	{
		aimRequest?.Dispose();
		if (shouldAutoDestroy && fallingStopwatch >= maxFallDuration)
		{
			DestroyModel();
		}
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
