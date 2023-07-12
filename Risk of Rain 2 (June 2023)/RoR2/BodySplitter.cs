using System;
using UnityEngine;

namespace RoR2;

public class BodySplitter
{
	public CharacterBody body;

	public float moneyMultiplier;

	private int _count = 1;

	public readonly MasterSummon masterSummon;

	public int count
	{
		get
		{
			return _count;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentException("'value' cannot be non-positive.", "value");
			}
			_count = value;
		}
	}

	public Vector3 splinterInitialVelocityLocal { get; set; } = Vector3.zero;


	public float minSpawnCircleRadius { get; set; }

	public BodySplitter()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		masterSummon = new MasterSummon
		{
			masterPrefab = null,
			ignoreTeamMemberLimit = false,
			useAmbientLevel = null,
			teamIndexOverride = null
		};
	}

	public void Perform()
	{
		PerformInternal(masterSummon);
	}

	private void PerformInternal(MasterSummon masterSummon)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		if (body == null)
		{
			throw new InvalidOperationException("'body' is null.");
		}
		if (!Object.op_Implicit((Object)(object)body))
		{
			throw new InvalidOperationException("'body' is an invalid object.");
		}
		GameObject masterPrefab = masterSummon.masterPrefab;
		CharacterMaster obj = masterPrefab.GetComponent<CharacterMaster>() ?? throw new InvalidOperationException("'splinterMasterPrefab' does not have a CharacterMaster component.");
		if (!obj.masterIndex.isValid)
		{
			throw new InvalidOperationException("'splinterMasterPrefab' is not registered with MasterCatalog.");
		}
		if (MasterCatalog.GetMasterPrefab(obj.masterIndex) != masterPrefab)
		{
			throw new InvalidOperationException("'splinterMasterPrefab' is not a prefab.");
		}
		Vector3 position = ((Component)body).transform.position;
		Quaternion val = Quaternion.LookRotation(body.inputBank.aimDirection);
		float y = ((Quaternion)(ref val)).eulerAngles.y;
		GameObject bodyPrefab = obj.bodyPrefab;
		bodyPrefab.GetComponent<CharacterBody>();
		float num = CalcBodyXZRadius(bodyPrefab);
		float num2 = 0f;
		if (count > 1)
		{
			num2 = num / Mathf.Sin(MathF.PI / (float)count);
		}
		num2 = Mathf.Max(num2, minSpawnCircleRadius);
		masterSummon.summonerBodyObject = ((Component)body).gameObject;
		masterSummon.inventoryToCopy = body.inventory;
		masterSummon.loadout = (Object.op_Implicit((Object)(object)body.master) ? body.master.loadout : null);
		masterSummon.inventoryItemCopyFilter = CopyItemFilter;
		RaycastHit val4 = default(RaycastHit);
		foreach (float item in new DegreeSlices(count, 0.5f))
		{
			Quaternion val2 = Quaternion.Euler(0f, y + item + 180f, 0f);
			Vector3 val3 = val2 * Vector3.forward;
			float num3 = num2;
			if (Physics.Raycast(new Ray(position, val3), ref val4, num2 + num, LayerIndex.world.intVal, (QueryTriggerInteraction)1))
			{
				num3 = ((RaycastHit)(ref val4)).distance - num;
			}
			Vector3 position2 = position + val3 * num3;
			masterSummon.position = position2;
			masterSummon.rotation = val2;
			try
			{
				CharacterMaster characterMaster = masterSummon.Perform();
				if (Object.op_Implicit((Object)(object)characterMaster))
				{
					CharacterBody characterBody = characterMaster.GetBody();
					if (Object.op_Implicit((Object)(object)characterBody))
					{
						Vector3 additionalVelocity = val2 * splinterInitialVelocityLocal;
						AddBodyVelocity(characterBody, additionalVelocity);
						characterMaster.money = (uint)Mathf.FloorToInt((float)body.master.money * moneyMultiplier);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
		}
	}

	private static float CalcBodyXZRadius(GameObject bodyPrefab)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		Collider component = bodyPrefab.GetComponent<Collider>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return 0f;
		}
		Vector3 position = bodyPrefab.transform.position;
		Bounds bounds = component.bounds;
		Vector3 min = ((Bounds)(ref bounds)).min;
		Vector3 max = ((Bounds)(ref bounds)).max;
		return Mathf.Max(Mathf.Max(Mathf.Max(Mathf.Max(0f, position.x - min.x), position.z - min.z), max.x - position.x), max.z - position.z);
	}

	private static void AddBodyVelocity(CharacterBody body, Vector3 additionalVelocity)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		IPhysMotor component = ((Component)body).GetComponent<IPhysMotor>();
		if (component != null)
		{
			PhysForceInfo physForceInfo = PhysForceInfo.Create();
			physForceInfo.force = additionalVelocity;
			physForceInfo.massIsOne = true;
			physForceInfo.ignoreGroundStick = true;
			physForceInfo.disableAirControlUntilCollision = false;
			component.ApplyForceImpulse(in physForceInfo);
		}
	}

	private static bool CopyItemFilter(ItemIndex itemIndex)
	{
		ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
		if (Object.op_Implicit((Object)(object)itemDef))
		{
			return itemDef.tier == ItemTier.NoTier;
		}
		return false;
	}
}
