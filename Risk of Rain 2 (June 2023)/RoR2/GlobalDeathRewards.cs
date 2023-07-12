using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class GlobalDeathRewards : MonoBehaviour
{
	[Serializable]
	public struct PickupReward
	{
		[SerializeField]
		public PickupDropTable dropTable;

		[EnumMask(typeof(CharacterBody.BodyFlags))]
		[SerializeField]
		public CharacterBody.BodyFlags requiredBodyFlags;

		[Range(0f, 1f)]
		[SerializeField]
		public float chance;

		[SerializeField]
		public Vector3 velocity;
	}

	[SerializeField]
	private bool requirePlayerAttacker;

	[SerializeField]
	private PickupReward[] pickupRewards;

	private Xoroshiro128Plus rng;

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (NetworkServer.active)
		{
			rng = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
		}
	}

	private void OnEnable()
	{
		GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
	}

	private void OnDisable()
	{
		GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
	}

	private void OnCharacterDeathGlobal(DamageReport damageReport)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		bool flag = true;
		if (requirePlayerAttacker)
		{
			flag = Object.op_Implicit((Object)(object)damageReport.attackerMaster) && Object.op_Implicit((Object)(object)((Component)damageReport.attackerMaster).GetComponent<PlayerCharacterMasterController>());
		}
		if (!flag)
		{
			return;
		}
		PickupReward[] array = pickupRewards;
		for (int i = 0; i < array.Length; i++)
		{
			PickupReward pickupReward = array[i];
			if ((damageReport.victimBody.bodyFlags & pickupReward.requiredBodyFlags) == pickupReward.requiredBodyFlags && Object.op_Implicit((Object)(object)pickupReward.dropTable) && rng.nextNormalizedFloat <= pickupReward.chance)
			{
				PickupDropletController.CreatePickupDroplet(pickupReward.dropTable.GenerateDrop(rng), ((Component)damageReport.victim).transform.position, pickupReward.velocity);
			}
		}
	}
}
