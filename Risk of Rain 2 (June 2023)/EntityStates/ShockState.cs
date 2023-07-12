using RoR2;
using UnityEngine;

namespace EntityStates;

public class ShockState : BaseState
{
	public static GameObject stunVfxPrefab;

	public float shockDuration = 1f;

	public static float shockInterval = 0.1f;

	public static float shockStrength = 1f;

	public static float healthFractionToForceExit = 0.1f;

	public static string enterSoundString;

	public static string exitSoundString;

	private float shockTimer;

	private Animator animator;

	private TemporaryOverlay temporaryOverlay;

	private float healthFraction;

	private GameObject stunVfxInstance;

	public override void OnEnter()
	{
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		animator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)base.sfxLocator) && base.sfxLocator.barkSound != "")
		{
			Util.PlaySound(base.sfxLocator.barkSound, base.gameObject);
		}
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayAnimation("Body", (Random.Range(0, 2) == 0) ? "Hurt1" : "Hurt2");
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			CharacterModel component = ((Component)modelTransform).GetComponent<CharacterModel>();
			if (Object.op_Implicit((Object)(object)component))
			{
				temporaryOverlay = base.gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = shockDuration;
				temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matIsShocked");
				temporaryOverlay.AddToCharacerModel(component);
			}
		}
		stunVfxInstance = Object.Instantiate<GameObject>(stunVfxPrefab, base.transform);
		stunVfxInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = shockDuration;
		if (Object.op_Implicit((Object)(object)base.characterBody.healthComponent))
		{
			healthFraction = base.characterBody.healthComponent.combinedHealthFraction;
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.isSprinting = false;
		}
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.moveVector = base.characterDirection.forward;
		}
		if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
		{
			base.rigidbodyMotor.moveVector = Vector3.zero;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		shockTimer -= Time.fixedDeltaTime;
		float combinedHealthFraction = base.characterBody.healthComponent.combinedHealthFraction;
		if (shockTimer <= 0f)
		{
			shockTimer += shockInterval;
			PlayShockAnimation();
		}
		if (base.fixedAge > shockDuration || healthFraction - combinedHealthFraction > healthFractionToForceExit)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)temporaryOverlay))
		{
			EntityState.Destroy((Object)(object)temporaryOverlay);
		}
		if (Object.op_Implicit((Object)(object)stunVfxInstance))
		{
			EntityState.Destroy((Object)(object)stunVfxInstance);
		}
		Util.PlaySound(exitSoundString, base.gameObject);
		base.OnExit();
	}

	private void PlayShockAnimation()
	{
		string text = "Flinch";
		int layerIndex = animator.GetLayerIndex(text);
		if (layerIndex >= 0)
		{
			animator.SetLayerWeight(layerIndex, shockStrength);
			animator.Play("FlinchStart", layerIndex);
		}
	}
}
