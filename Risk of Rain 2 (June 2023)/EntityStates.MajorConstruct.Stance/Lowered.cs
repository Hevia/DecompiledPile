using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.MajorConstruct.Stance;

public class Lowered : BaseState
{
	[SerializeField]
	public GameObject attachmentPrefab;

	private NetworkedBodyAttachment attachment;

	public override void OnEnter()
	{
		base.OnEnter();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)attachmentPrefab))
		{
			attachment = Object.Instantiate<GameObject>(attachmentPrefab).GetComponent<NetworkedBodyAttachment>();
			attachment.AttachToGameObjectAndSpawn(((Component)base.characterBody).gameObject);
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)attachment))
		{
			EntityState.Destroy((Object)(object)((Component)attachment).gameObject);
		}
		base.OnExit();
	}
}
