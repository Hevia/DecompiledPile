using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.InfiniteTowerSafeWard;

public class Active : BaseSafeWardState
{
	[SerializeField]
	public float radius;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string enterSoundString;

	private InfiniteTowerRun run;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)safeWardController))
		{
			safeWardController.SetIndicatorEnabled(enabled: false);
		}
		PlayAnimation(animationLayerName, animationStateName);
		Util.PlaySound(enterSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)purchaseInteraction))
		{
			purchaseInteraction.SetAvailable(newAvailable: false);
		}
		if (Object.op_Implicit((Object)(object)zone))
		{
			zone.Networkradius = radius;
		}
		run = Run.instance as InfiniteTowerRun;
	}

	public void SelfDestruct()
	{
		if (NetworkServer.active)
		{
			outer.SetNextState(new SelfDestruct());
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)zone) && Object.op_Implicit((Object)(object)run))
		{
			float num = 1f;
			if (Object.op_Implicit((Object)(object)run.waveController))
			{
				num = run.waveController.zoneRadiusPercentage;
			}
			zone.Networkradius = radius * num;
		}
	}
}
