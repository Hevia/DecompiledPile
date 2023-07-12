using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.CaptainSupplyDrop;

public class HealZoneMainState : BaseMainState
{
	public static GameObject healZonePrefab;

	private GameObject healZoneInstance;

	protected override Interactability GetInteractability(Interactor activator)
	{
		return Interactability.Disabled;
	}

	public override void OnEnter()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (NetworkServer.active)
		{
			healZoneInstance = Object.Instantiate<GameObject>(healZonePrefab, base.transform.position, base.transform.rotation);
			healZoneInstance.GetComponent<TeamFilter>().teamIndex = teamFilter.teamIndex;
			NetworkServer.Spawn(healZoneInstance);
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)healZoneInstance))
		{
			EntityState.Destroy((Object)(object)healZoneInstance);
		}
		base.OnExit();
	}
}
