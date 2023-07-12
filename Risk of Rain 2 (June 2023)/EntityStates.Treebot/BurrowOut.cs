using KinematicCharacterController;
using RoR2;
using UnityEngine;

namespace EntityStates.Treebot;

public class BurrowOut : GenericCharacterMain
{
	public static GameObject burrowPrefab;

	public static float baseDuration;

	public static string burrowOutSoundString;

	public static float jumpVelocity;

	private float stopwatch;

	private Transform modelTransform;

	private ChildLocator childLocator;

	private float duration;

	public override void OnEnter()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelTransform = GetModelTransform();
		childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		Util.PlaySound(burrowOutSoundString, base.gameObject);
		EffectManager.SimpleMuzzleFlash(burrowPrefab, base.gameObject, "BurrowCenter", transmit: false);
		base.characterMotor.velocity = new Vector3(0f, jumpVelocity, 0f);
		((BaseCharacterController)base.characterMotor).Motor.ForceUnground();
	}

	public override void OnExit()
	{
		base.OnExit();
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
		return InterruptPriority.Skill;
	}
}
