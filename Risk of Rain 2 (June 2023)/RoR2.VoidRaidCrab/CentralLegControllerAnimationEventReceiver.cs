using UnityEngine;

namespace RoR2.VoidRaidCrab;

[RequireComponent(typeof(Animator))]
public class CentralLegControllerAnimationEventReceiver : MonoBehaviour
{
	public CentralLegController target;

	public ChildLocator childLocator;

	public void VoidRaidCrabFootStep(string targetName)
	{
		Transform val = childLocator.FindChild(targetName);
		if (Object.op_Implicit((Object)(object)val))
		{
			LegController componentInParent = ((Component)val).GetComponentInParent<LegController>();
			if (Object.op_Implicit((Object)(object)componentInParent) && componentInParent.mainBodyHasEffectiveAuthority)
			{
				componentInParent.DoToeConcussionBlastAuthority();
			}
		}
	}
}
