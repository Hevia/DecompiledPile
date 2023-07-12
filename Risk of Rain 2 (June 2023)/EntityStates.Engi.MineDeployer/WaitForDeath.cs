using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Engi.MineDeployer;

public class WaitForDeath : BaseMineDeployerState
{
	public static float duration;

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && duration <= base.fixedAge)
		{
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}
}
