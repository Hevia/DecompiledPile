using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.BeetleQueenMonster;

public class WeakState : BaseState
{
	private float stopwatch;

	private float grubStopwatch;

	public static float weakDuration;

	public static float weakToIdleTransitionDuration;

	public static string weakPointChildString;

	public static int maxGrubCount;

	public static float grubSpawnFrequency;

	public static float grubSpawnDelay;

	private int grubCount;

	private bool beginExitTransition;

	private ChildLocator childLocator;

	public override void OnEnter()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		grubStopwatch -= grubSpawnDelay;
		if (Object.op_Implicit((Object)(object)base.sfxLocator) && base.sfxLocator.barkSound != "")
		{
			Util.PlaySound(base.sfxLocator.barkSound, base.gameObject);
		}
		PlayAnimation("Body", "WeakEnter");
		Transform modelTransform = GetModelTransform();
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			Transform val = childLocator.FindChild(weakPointChildString);
			if (Object.op_Implicit((Object)(object)val))
			{
				Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/WeakPointProcEffect"), val.position, val.rotation);
			}
		}
	}

	public override void FixedUpdate()
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		grubStopwatch += Time.fixedDeltaTime;
		if (grubStopwatch >= 1f / grubSpawnFrequency && grubCount < maxGrubCount)
		{
			grubCount++;
			grubStopwatch -= 1f / grubSpawnFrequency;
			if (NetworkServer.active)
			{
				Transform val = childLocator.FindChild("GrubSpawnPoint" + Random.Range(1, 10));
				if (Object.op_Implicit((Object)(object)val))
				{
					NetworkServer.Spawn(Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/GrubPack"), ((Component)val).transform.position, Random.rotation));
					((Component)val).gameObject.SetActive(false);
				}
			}
		}
		if (stopwatch >= weakDuration - weakToIdleTransitionDuration && !beginExitTransition)
		{
			beginExitTransition = true;
			PlayCrossfade("Body", "WeakExit", "WeakExit.playbackRate", weakToIdleTransitionDuration, 0.5f);
		}
		if (stopwatch >= weakDuration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Pain;
	}
}
