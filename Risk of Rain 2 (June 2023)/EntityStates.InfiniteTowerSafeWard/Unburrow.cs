using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.InfiniteTowerSafeWard;

public class Unburrow : BaseSafeWardState
{
	[SerializeField]
	public float duration;

	[SerializeField]
	public float radius;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string enterSoundString;

	private Xoroshiro128Plus rng;

	public Unburrow()
	{
	}

	public Unburrow(Xoroshiro128Plus rng)
	{
		this.rng = rng;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)safeWardController))
		{
			safeWardController.SetIndicatorEnabled(enabled: true);
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
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && base.fixedAge >= duration)
		{
			outer.SetNextState(new Travelling(rng));
		}
	}
}
