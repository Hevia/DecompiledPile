using RoR2;
using UnityEngine;

namespace EntityStates.Bison;

public class SpawnState : BaseState
{
	public static GameObject spawnEffectPrefab;

	public static string spawnEffectMuzzle;

	public static float duration;

	public static float snowyOverlayDuration;

	public static Material snowyMaterial;

	public override void OnEnter()
	{
		base.OnEnter();
		Transform modelTransform = GetModelTransform();
		EffectManager.SimpleMuzzleFlash(spawnEffectPrefab, base.gameObject, spawnEffectMuzzle, transmit: false);
		PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
		Util.PlaySound("Play_bison_idle_graze", base.gameObject);
		Util.PlaySound("Play_bison_charge_attack_collide", base.gameObject);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			CharacterModel component = ((Component)modelTransform).GetComponent<CharacterModel>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)snowyMaterial))
			{
				TemporaryOverlay temporaryOverlay = ((Component)component).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = snowyOverlayDuration;
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = snowyMaterial;
				temporaryOverlay.inspectorCharacterModel = component;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.animateShaderAlpha = true;
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextStateToMain();
		}
	}
}
