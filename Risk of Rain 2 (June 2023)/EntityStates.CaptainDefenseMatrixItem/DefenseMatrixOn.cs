using System.Collections.Generic;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.CaptainDefenseMatrixItem;

public class DefenseMatrixOn : BaseBodyAttachmentState
{
	public static float projectileEraserRadius;

	public static float minimumFireFrequency;

	public static float baseRechargeFrequency;

	public static GameObject tracerEffectPrefab;

	private float rechargeTimer;

	private float rechargeFrequency => baseRechargeFrequency * (Object.op_Implicit((Object)(object)base.attachedBody) ? base.attachedBody.attackSpeed : 1f);

	private float fireFrequency => Mathf.Max(minimumFireFrequency, rechargeFrequency);

	private float timeBetweenFiring => 1f / fireFrequency;

	private bool isReadyToFire => rechargeTimer <= 0f;

	protected int GetItemStack()
	{
		if (!Object.op_Implicit((Object)(object)base.attachedBody) || !Object.op_Implicit((Object)(object)base.attachedBody.inventory))
		{
			return 1;
		}
		return base.attachedBody.inventory.GetItemCount(RoR2Content.Items.CaptainDefenseMatrix);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (!Object.op_Implicit((Object)(object)base.attachedBody))
		{
			return;
		}
		PlayerCharacterMasterController component = ((Component)base.attachedBody.master).GetComponent<PlayerCharacterMasterController>();
		if (Object.op_Implicit((Object)(object)component))
		{
			NetworkUser networkUser = component.networkUser;
			if (Object.op_Implicit((Object)(object)networkUser))
			{
				PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.Items.CaptainDefenseMatrix.itemIndex);
				networkUser.localUser?.userProfile.DiscoverPickup(pickupIndex);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!NetworkServer.active)
		{
			return;
		}
		rechargeTimer -= Time.fixedDeltaTime;
		if (base.fixedAge > timeBetweenFiring)
		{
			base.fixedAge -= timeBetweenFiring;
			if (isReadyToFire && DeleteNearbyProjectile())
			{
				rechargeTimer = 1f / rechargeFrequency;
			}
		}
	}

	private bool DeleteNearbyProjectile()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (Object.op_Implicit((Object)(object)base.attachedBody) ? base.attachedBody.corePosition : Vector3.zero);
		TeamIndex teamIndex = (Object.op_Implicit((Object)(object)base.attachedBody) ? base.attachedBody.teamComponent.teamIndex : TeamIndex.None);
		float num = projectileEraserRadius * projectileEraserRadius;
		int num2 = 0;
		int itemStack = GetItemStack();
		bool result = false;
		List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
		List<ProjectileController> list = new List<ProjectileController>();
		int i = 0;
		for (int count = instancesList.Count; i < count; i++)
		{
			if (num2 >= itemStack)
			{
				break;
			}
			ProjectileController projectileController = instancesList[i];
			if (!projectileController.cannotBeDeleted && projectileController.teamFilter.teamIndex != teamIndex)
			{
				Vector3 val2 = ((Component)projectileController).transform.position - val;
				if (((Vector3)(ref val2)).sqrMagnitude < num)
				{
					list.Add(projectileController);
					num2++;
				}
			}
		}
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			ProjectileController projectileController2 = list[j];
			if (Object.op_Implicit((Object)(object)projectileController2))
			{
				result = true;
				Vector3 position = ((Component)projectileController2).transform.position;
				Vector3 start = val;
				if (Object.op_Implicit((Object)(object)tracerEffectPrefab))
				{
					EffectData effectData = new EffectData
					{
						origin = position,
						start = start
					};
					EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
				}
				EntityState.Destroy((Object)(object)((Component)projectileController2).gameObject);
			}
		}
		return result;
	}
}
