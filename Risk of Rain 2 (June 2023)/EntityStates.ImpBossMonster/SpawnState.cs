using RoR2;
using UnityEngine;

namespace EntityStates.ImpBossMonster;

public class SpawnState : BaseState
{
	private float stopwatch;

	public static float duration = 4f;

	public static string spawnSoundString;

	public static GameObject spawnEffectPrefab;

	public static Material destealthMaterial;

	public override void OnEnter()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Animator modelAnimator = GetModelAnimator();
		PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
		Util.PlaySound(spawnSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)spawnEffectPrefab))
		{
			EffectData effectData = new EffectData();
			effectData.origin = base.transform.position;
			EffectManager.SpawnEffect(spawnEffectPrefab, effectData, transmit: false);
		}
		if (Object.op_Implicit((Object)(object)destealthMaterial))
		{
			TemporaryOverlay temporaryOverlay = ((Component)modelAnimator).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay.duration = 1f;
			temporaryOverlay.destroyComponentOnEnd = true;
			temporaryOverlay.originalMaterial = destealthMaterial;
			temporaryOverlay.inspectorCharacterModel = ((Component)modelAnimator).gameObject.GetComponent<CharacterModel>();
			temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			temporaryOverlay.animateShaderAlpha = true;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
