using RoR2;
using UnityEngine;

namespace EntityStates.ArtifactShell;

public class Death : ArtifactShellBaseState
{
	public static float duration;

	protected override bool interactionAvailable => false;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(GetComponent<SfxLocator>().deathSound, base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
