using System.Collections.Generic;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Items;

public class RandomDamageZoneBodyBehavior : BaseItemBodyBehavior
{
	private static readonly float wardDuration = 25f;

	private static readonly float wardRespawnRetryTime = 1f;

	private static readonly float baseWardRadius = 16f;

	private static readonly float wardRadiusMultiplierPerStack = 1.5f;

	private float wardResummonCooldown;

	[ItemDefAssociation(useOnServer = true, useOnClient = false)]
	private static ItemDef GetItemDef()
	{
		return RoR2Content.Items.RandomDamageZone;
	}

	private void FixedUpdate()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		CharacterMaster master = base.body.master;
		if (!Object.op_Implicit((Object)(object)master) || !master.IsDeployableSlotAvailable(DeployableSlot.PowerWard))
		{
			return;
		}
		wardResummonCooldown -= Time.fixedDeltaTime;
		if (!(wardResummonCooldown <= 0f))
		{
			return;
		}
		wardResummonCooldown = wardRespawnRetryTime;
		if (master.IsDeployableSlotAvailable(DeployableSlot.PowerWard))
		{
			Vector3? val = FindWardSpawnPosition(base.body.corePosition);
			if (val.HasValue)
			{
				GameObject val2 = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/DamageZoneWard"), val.Value, Quaternion.identity);
				Util.PlaySound("Play_randomDamageZone_appear", val2.gameObject);
				val2.GetComponent<TeamFilter>().teamIndex = TeamIndex.None;
				BuffWard component = val2.GetComponent<BuffWard>();
				component.Networkradius = baseWardRadius * Mathf.Pow(wardRadiusMultiplierPerStack, (float)(stack - 1));
				component.expireDuration = wardDuration;
				NetworkServer.Spawn(val2);
				Deployable component2 = val2.GetComponent<Deployable>();
				master.AddDeployable(component2, DeployableSlot.PowerWard);
			}
		}
	}

	private Vector3? FindWardSpawnPosition(Vector3 ownerPosition)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)SceneInfo.instance))
		{
			return null;
		}
		NodeGraph groundNodes = SceneInfo.instance.groundNodes;
		if (!Object.op_Implicit((Object)(object)groundNodes))
		{
			return null;
		}
		List<NodeGraph.NodeIndex> list = groundNodes.FindNodesInRange(ownerPosition, 0f, 50f, HullMask.None);
		if (list.Count > 0 && groundNodes.GetNodePosition(list[(int)Random.Range(0f, (float)list.Count)], out var position))
		{
			return position;
		}
		return null;
	}
}
