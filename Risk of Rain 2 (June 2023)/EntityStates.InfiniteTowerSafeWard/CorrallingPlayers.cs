using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.InfiniteTowerSafeWard;

public class CorrallingPlayers : BaseSafeWardState
{
	[SerializeField]
	public float duration;

	[SerializeField]
	public float initialRadius;

	[SerializeField]
	public float finalRadius;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string enterSoundString;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation(animationLayerName, animationStateName);
		Util.PlaySound(enterSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)purchaseInteraction))
		{
			purchaseInteraction.SetAvailable(newAvailable: false);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)zone))
		{
			float num = Mathf.Min(1f, base.fixedAge / duration);
			zone.Networkradius = Mathf.Lerp(initialRadius, finalRadius, num);
		}
		if (NetworkServer.active && base.fixedAge >= duration)
		{
			outer.SetNextState(new AwaitingActivation());
		}
	}
}
