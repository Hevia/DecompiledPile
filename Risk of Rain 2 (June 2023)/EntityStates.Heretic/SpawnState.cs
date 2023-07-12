using RoR2;
using UnityEngine;

namespace EntityStates.Heretic;

public class SpawnState : BaseState
{
	public static float duration = 4f;

	public static string spawnSoundString;

	public static GameObject effectPrefab;

	public static Material overlayMaterial;

	public static float overlayDuration;

	public override void OnEnter()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(spawnSoundString, base.gameObject);
		EffectManager.SimpleEffect(effectPrefab, base.characterBody.corePosition, Quaternion.identity, transmit: false);
		PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			CharacterModel component = ((Component)modelTransform).GetComponent<CharacterModel>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)overlayMaterial))
			{
				TemporaryOverlay temporaryOverlay = ((Component)component).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = overlayDuration;
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = overlayMaterial;
				temporaryOverlay.inspectorCharacterModel = component;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.animateShaderAlpha = true;
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
