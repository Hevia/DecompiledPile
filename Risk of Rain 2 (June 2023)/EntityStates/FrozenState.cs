using RoR2;
using UnityEngine;

namespace EntityStates;

public class FrozenState : BaseState
{
	private float duration;

	private Animator modelAnimator;

	private TemporaryOverlay temporaryOverlay;

	public float freezeDuration = 0.35f;

	public static GameObject frozenEffectPrefab;

	public static GameObject executeEffectPrefab;

	public override void OnEnter()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.sfxLocator) && base.sfxLocator.barkSound != "")
		{
			Util.PlaySound(base.sfxLocator.barkSound, base.gameObject);
		}
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			CharacterModel component = ((Component)modelTransform).GetComponent<CharacterModel>();
			if (Object.op_Implicit((Object)(object)component))
			{
				temporaryOverlay = base.gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = freezeDuration;
				temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matIsFrozen");
				temporaryOverlay.AddToCharacerModel(component);
			}
		}
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			((Behaviour)modelAnimator).enabled = false;
			duration = freezeDuration;
			EffectManager.SpawnEffect(frozenEffectPrefab, new EffectData
			{
				origin = base.characterBody.corePosition,
				scale = (Object.op_Implicit((Object)(object)base.characterBody) ? base.characterBody.radius : 1f)
			}, transmit: false);
		}
		if (Object.op_Implicit((Object)(object)base.rigidbody) && !base.rigidbody.isKinematic)
		{
			base.rigidbody.velocity = Vector3.zero;
			if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
			{
				base.rigidbodyMotor.moveVector = Vector3.zero;
			}
		}
		base.healthComponent.isInFrozenState = true;
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.moveVector = base.characterDirection.forward;
		}
	}

	public override void OnExit()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			((Behaviour)modelAnimator).enabled = true;
		}
		if (Object.op_Implicit((Object)(object)temporaryOverlay))
		{
			EntityState.Destroy((Object)(object)temporaryOverlay);
		}
		EffectManager.SpawnEffect(frozenEffectPrefab, new EffectData
		{
			origin = base.characterBody.corePosition,
			scale = (Object.op_Implicit((Object)(object)base.characterBody) ? base.characterBody.radius : 1f)
		}, transmit: false);
		base.healthComponent.isInFrozenState = false;
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Frozen;
	}
}
