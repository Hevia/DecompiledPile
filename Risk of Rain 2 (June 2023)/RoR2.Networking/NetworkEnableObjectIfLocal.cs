using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

public class NetworkEnableObjectIfLocal : NetworkBehaviour
{
	[Tooltip("The GameObject to enable/disable.")]
	public GameObject target;

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)target))
		{
			target.SetActive(((NetworkBehaviour)this).hasAuthority);
		}
	}

	public override void OnStartAuthority()
	{
		((NetworkBehaviour)this).OnStartAuthority();
		if (Object.op_Implicit((Object)(object)target))
		{
			target.SetActive(true);
		}
	}

	public override void OnStopAuthority()
	{
		if (Object.op_Implicit((Object)(object)target))
		{
			target.SetActive(false);
		}
		((NetworkBehaviour)this).OnStopAuthority();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
