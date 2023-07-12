using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidCamp;

public class Deactivate : EntityState
{
	[SerializeField]
	public float duration;

	[SerializeField]
	public string onEnterSoundString;

	[SerializeField]
	public string baseAnimationLayerName;

	[SerializeField]
	public string baseAnimationStateName;

	[SerializeField]
	public string deactivateChildName;

	[SerializeField]
	public string additiveAnimationLayerName;

	[SerializeField]
	public string additiveAnimationStateName;

	[SerializeField]
	public string completeObjectiveChatMessageToken;

	[SerializeField]
	public PickupDropTable rewardDropTable;

	[SerializeField]
	public GameObject rewardPickupPrefab;

	[SerializeField]
	public bool dropItem;

	public override void OnEnter()
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayAnimation(additiveAnimationLayerName, additiveAnimationStateName);
		FogDamageController componentInChildren = ((Component)outer).GetComponentInChildren<FogDamageController>();
		if (Object.op_Implicit((Object)(object)componentInChildren))
		{
			((Behaviour)componentInChildren).enabled = false;
		}
		Util.PlaySound(onEnterSoundString, base.gameObject);
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			((Component)modelChildLocator.FindChild("ActiveFX")).gameObject.SetActive(false);
			((Behaviour)((Component)modelChildLocator.FindChild("RangeFX")).GetComponent<AnimateShaderAlpha>()).enabled = true;
		}
		Chat.SendBroadcastChat(new Chat.SimpleChatMessage
		{
			baseToken = completeObjectiveChatMessageToken
		});
		if (!NetworkServer.active || !dropItem)
		{
			return;
		}
		Transform val = modelChildLocator.FindChild("RewardSpawnTarget");
		int participatingPlayerCount = Run.instance.participatingPlayerCount;
		if (participatingPlayerCount > 0 && Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)rewardDropTable))
		{
			int num = participatingPlayerCount;
			float num2 = 360f / (float)num;
			Vector3 val2 = Quaternion.AngleAxis((float)Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
			Quaternion val3 = Quaternion.AngleAxis(num2, Vector3.up);
			Vector3 position = ((Component)val).transform.position;
			PickupPickerController.Option[] pickerOptions = PickupPickerController.GenerateOptionsFromDropTable(3, rewardDropTable, Run.instance.treasureRng);
			int num3 = 0;
			while (num3 < num)
			{
				GenericPickupController.CreatePickupInfo pickupInfo = default(GenericPickupController.CreatePickupInfo);
				pickupInfo.pickupIndex = PickupCatalog.FindPickupIndex(ItemTier.VoidTier2);
				pickupInfo.pickerOptions = pickerOptions;
				pickupInfo.rotation = Quaternion.identity;
				pickupInfo.prefabOverride = rewardPickupPrefab;
				PickupDropletController.CreatePickupDroplet(pickupInfo, position, val2);
				num3++;
				val2 = val3 * val2;
			}
		}
	}

	public override void OnExit()
	{
		PlayAnimation(baseAnimationLayerName, baseAnimationStateName);
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			modelChildLocator.FindChildGameObject(deactivateChildName).gameObject.SetActive(false);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextState(new EntityStates.Idle());
		}
	}
}
