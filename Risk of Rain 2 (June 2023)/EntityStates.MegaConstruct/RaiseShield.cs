using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.MegaConstruct;

public class RaiseShield : FlyState
{
	[SerializeField]
	public GameObject attachmentPrefab;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationEnterStateName;

	[SerializeField]
	public float duration;

	private NetworkedBodyAttachment attachment;

	public override void OnEnter()
	{
		base.OnEnter();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)attachmentPrefab))
		{
			attachment = Object.Instantiate<GameObject>(attachmentPrefab).GetComponent<NetworkedBodyAttachment>();
			attachment.AttachToGameObjectAndSpawn(((Component)base.characterBody).gameObject);
		}
		MasterSpawnSlotController component = GetComponent<MasterSpawnSlotController>();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)component))
		{
			component.SpawnAllOpen(base.gameObject, Run.instance.stageRng);
		}
		PlayAnimation(animationLayerName, animationEnterStateName);
	}

	protected override bool CanExecuteSkill(GenericSkill skillSlot)
	{
		return false;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextState(new ExitShield());
		}
	}

	public override void OnExit()
	{
		if (NetworkServer.active && Object.op_Implicit((Object)(object)attachment))
		{
			EntityState.Destroy((Object)(object)((Component)attachment).gameObject);
		}
		base.OnExit();
	}
}
