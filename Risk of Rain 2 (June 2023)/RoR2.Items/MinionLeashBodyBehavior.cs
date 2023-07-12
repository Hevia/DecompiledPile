using RoR2.Navigation;
using UnityEngine;

namespace RoR2.Items;

public class MinionLeashBodyBehavior : BaseItemBodyBehavior
{
	public const float leashDistSq = 160000f;

	public const float teleportDelayTime = 10f;

	public const float minTeleportDistance = 10f;

	public const float maxTeleportDistance = 40f;

	private GameObject helperPrefab;

	private RigidbodyMotor rigidbodyMotor;

	private float teleportAttemptTimer = 10f;

	[ItemDefAssociation(useOnServer = true, useOnClient = true)]
	private static ItemDef GetItemDef()
	{
		return RoR2Content.Items.MinionLeash;
	}

	public void Start()
	{
		if (base.body.hasEffectiveAuthority)
		{
			helperPrefab = LegacyResourcesAPI.Load<GameObject>("SpawnCards/HelperPrefab");
			rigidbodyMotor = ((Component)this).GetComponent<RigidbodyMotor>();
		}
	}

	private void FixedUpdate()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		if (!base.body.hasEffectiveAuthority)
		{
			return;
		}
		CharacterMaster master = base.body.master;
		CharacterMaster characterMaster = (Object.op_Implicit((Object)(object)master) ? master.minionOwnership.ownerMaster : null);
		CharacterBody characterBody = (Object.op_Implicit((Object)(object)characterMaster) ? characterMaster.GetBody() : null);
		if (!Object.op_Implicit((Object)(object)characterBody))
		{
			return;
		}
		Vector3 corePosition = characterBody.corePosition;
		Vector3 corePosition2 = base.body.corePosition;
		if ((!Object.op_Implicit((Object)(object)base.body.characterMotor) || !(base.body.characterMotor.walkSpeed > 0f)) && (!Object.op_Implicit((Object)(object)rigidbodyMotor) || !(base.body.moveSpeed > 0f)))
		{
			return;
		}
		Vector3 val = corePosition2 - corePosition;
		if (!(((Vector3)(ref val)).sqrMagnitude > 160000f))
		{
			return;
		}
		teleportAttemptTimer -= Time.fixedDeltaTime;
		if (!(teleportAttemptTimer <= 0f))
		{
			return;
		}
		teleportAttemptTimer = 10f;
		SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
		spawnCard.hullSize = base.body.hullClassification;
		spawnCard.nodeGraphType = (base.body.isFlying ? MapNodeGroup.GraphType.Air : MapNodeGroup.GraphType.Ground);
		spawnCard.prefab = helperPrefab;
		GameObject val2 = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
		{
			placementMode = DirectorPlacementRule.PlacementMode.Approximate,
			position = corePosition,
			minDistance = 10f,
			maxDistance = 40f
		}, RoR2Application.rng));
		if (Object.op_Implicit((Object)(object)val2))
		{
			Vector3 position = val2.transform.position;
			val = position - corePosition;
			if (((Vector3)(ref val)).sqrMagnitude < 160000f)
			{
				Debug.Log((object)("MinionLeash teleport for " + ((Object)base.body).name));
				TeleportHelper.TeleportBody(base.body, position);
				GameObject teleportEffectPrefab = Run.instance.GetTeleportEffectPrefab(((Component)base.body).gameObject);
				if (Object.op_Implicit((Object)(object)teleportEffectPrefab))
				{
					EffectManager.SimpleEffect(teleportEffectPrefab, position, Quaternion.identity, transmit: true);
				}
				Object.Destroy((Object)(object)val2);
			}
		}
		Object.Destroy((Object)(object)spawnCard);
	}
}
