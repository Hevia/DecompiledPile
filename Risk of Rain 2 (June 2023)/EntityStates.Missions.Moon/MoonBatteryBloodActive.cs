using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Missions.Moon;

public class MoonBatteryBloodActive : MoonBatteryActive
{
	[SerializeField]
	public GameObject siphonPrefab;

	[SerializeField]
	public string siphonRootName;

	private GameObject siphonObject;

	public override void OnEnter()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (NetworkServer.active)
		{
			Transform val = FindModelChild(siphonRootName);
			if (!Object.op_Implicit((Object)(object)val))
			{
				val = base.transform;
			}
			siphonObject = Object.Instantiate<GameObject>(siphonPrefab, val.position, val.rotation, val);
			NetworkServer.Spawn(siphonObject);
		}
	}

	public override void OnExit()
	{
		if (NetworkServer.active && Object.op_Implicit((Object)(object)siphonObject))
		{
			NetworkServer.Destroy(siphonObject);
		}
		base.OnExit();
	}
}
