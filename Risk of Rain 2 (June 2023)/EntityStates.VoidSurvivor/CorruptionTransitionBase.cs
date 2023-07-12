using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidSurvivor;

public class CorruptionTransitionBase : BaseState
{
	[SerializeField]
	public float duration;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationGroundStateName;

	[SerializeField]
	public string animationAirStateName;

	[SerializeField]
	public string animationPlaybackParameterName;

	[SerializeField]
	public float animationCrossfadeDuration;

	[SerializeField]
	public string entrySound;

	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public GameObject completionEffectPrefab;

	[SerializeField]
	public string effectmuzzle;

	[SerializeField]
	public CharacterCameraParams cameraParams;

	[SerializeField]
	public float dampingCoefficient;

	protected VoidSurvivorController voidSurvivorController;

	private GameObject chargeEffectInstance;

	private CameraTargetParams.CameraParamsOverrideHandle cameraParamsOverrideHandle;

	public override void OnEnter()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		voidSurvivorController = GetComponent<VoidSurvivorController>();
		PlayCrossfade(animationLayerName, base.characterMotor.isGrounded ? animationGroundStateName : animationAirStateName, animationPlaybackParameterName, duration, animationCrossfadeDuration);
		Util.PlaySound(entrySound, base.gameObject);
		if (NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
		}
		StartCameraParamsOverride(duration);
		Transform val = FindModelChild(effectmuzzle);
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
			chargeEffectInstance.transform.parent = val;
			ScaleParticleSystemDuration component = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = duration;
			}
		}
		if (base.isAuthority && Object.op_Implicit((Object)(object)voidSurvivorController))
		{
			voidSurvivorController.weaponStateMachine.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		EndCameraParamsOverride(0f);
		if (NetworkServer.active)
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
		}
		if (Object.op_Implicit((Object)(object)chargeEffectInstance))
		{
			EntityState.Destroy((Object)(object)chargeEffectInstance);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.isAuthority)
		{
			CharacterMotor obj = base.characterMotor;
			obj.velocity -= base.characterMotor.velocity * dampingCoefficient;
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			OnFinishAuthority();
			outer.SetNextStateToMain();
		}
	}

	public virtual void OnFinishAuthority()
	{
		EffectManager.SimpleMuzzleFlash(completionEffectPrefab, base.gameObject, effectmuzzle, transmit: true);
	}

	protected void StartCameraParamsOverride(float transitionDuration)
	{
		if (!cameraParamsOverrideHandle.isValid)
		{
			cameraParamsOverrideHandle = base.cameraTargetParams.AddParamsOverride(new CameraTargetParams.CameraParamsOverrideRequest
			{
				cameraParamsData = cameraParams.data
			}, transitionDuration);
		}
	}

	protected void EndCameraParamsOverride(float transitionDuration)
	{
		if (cameraParamsOverrideHandle.isValid)
		{
			base.cameraTargetParams.RemoveParamsOverride(cameraParamsOverrideHandle, transitionDuration);
			cameraParamsOverrideHandle = default(CameraTargetParams.CameraParamsOverrideHandle);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Frozen;
	}
}
