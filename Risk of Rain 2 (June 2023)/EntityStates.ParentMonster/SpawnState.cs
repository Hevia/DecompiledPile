using RoR2;
using UnityEngine;

namespace EntityStates.ParentMonster;

public class SpawnState : EntityState
{
	public static float duration = 2f;

	public static string spawnSoundString;

	public static GameObject spawnEffectPrefab;

	private ParentEnergyFXController FXController;

	public override void OnEnter()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		GetModelAnimator();
		Util.PlaySound(spawnSoundString, base.gameObject);
		PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)spawnEffectPrefab))
		{
			EffectData effectData = new EffectData
			{
				origin = ((Component)base.modelLocator.modelTransform).GetComponent<ChildLocator>().FindChild("SpawnEffectOrigin").position
			};
			EffectManager.SpawnEffect(spawnEffectPrefab, effectData, transmit: true);
		}
		PrintController component = ((Component)base.modelLocator.modelTransform).gameObject.GetComponent<PrintController>();
		((Behaviour)component).enabled = false;
		component.printTime = duration;
		component.startingPrintHeight = 4f;
		component.maxPrintHeight = -1f;
		component.startingPrintBias = 2f;
		component.maxPrintBias = 0.95f;
		component.disableWhenFinished = true;
		component.printCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		((Behaviour)component).enabled = true;
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
