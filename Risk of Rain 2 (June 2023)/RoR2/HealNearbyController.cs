using System.Collections.Generic;
using System.Runtime.InteropServices;
using HG;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(NetworkedBodyAttachment))]
public class HealNearbyController : NetworkBehaviour
{
	[SyncVar]
	public float radius;

	public float damagePerSecondCoefficient;

	[Min(float.Epsilon)]
	public float tickRate = 1f;

	[SyncVar]
	public int maxTargets;

	public TetherVfxOrigin tetherVfxOrigin;

	public GameObject activeVfx;

	protected Transform transform;

	protected NetworkedBodyAttachment networkedBodyAttachment;

	protected SphereSearch sphereSearch;

	protected float timer;

	private bool isTetheredToAtLeastOneObject;

	public float Networkradius
	{
		get
		{
			return radius;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref radius, 1u);
		}
	}

	public int NetworkmaxTargets
	{
		get
		{
			return maxTargets;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref maxTargets, 2u);
		}
	}

	protected void Awake()
	{
		transform = ((Component)this).transform;
		networkedBodyAttachment = ((Component)this).GetComponent<NetworkedBodyAttachment>();
		sphereSearch = new SphereSearch();
		timer = 0f;
	}

	protected void FixedUpdate()
	{
		timer -= Time.fixedDeltaTime;
		if (timer <= 0f)
		{
			timer += 1f / tickRate;
			Tick();
		}
	}

	protected void Tick()
	{
		if (!Object.op_Implicit((Object)(object)networkedBodyAttachment) || !Object.op_Implicit((Object)(object)networkedBodyAttachment.attachedBody) || !Object.op_Implicit((Object)(object)networkedBodyAttachment.attachedBodyObject))
		{
			return;
		}
		List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
		SearchForTargets(list);
		float amount = damagePerSecondCoefficient * networkedBodyAttachment.attachedBody.damage / tickRate;
		List<Transform> list2 = CollectionPool<Transform, List<Transform>>.RentCollection();
		for (int i = 0; i < list.Count; i++)
		{
			HurtBox hurtBox = list[i];
			if (Object.op_Implicit((Object)(object)hurtBox) && Object.op_Implicit((Object)(object)hurtBox.healthComponent) && networkedBodyAttachment.attachedBody.healthComponent.alive && hurtBox.healthComponent.health < hurtBox.healthComponent.fullHealth && !hurtBox.healthComponent.body.HasBuff(DLC1Content.Buffs.EliteEarth))
			{
				HealthComponent healthComponent = hurtBox.healthComponent;
				if ((Object)(object)hurtBox.healthComponent.body == (Object)(object)networkedBodyAttachment.attachedBody)
				{
					continue;
				}
				Transform item = healthComponent.body?.coreTransform ?? ((Component)hurtBox).transform;
				list2.Add(item);
				if (NetworkServer.active)
				{
					healthComponent.Heal(amount, default(ProcChainMask));
				}
			}
			if (list2.Count >= maxTargets)
			{
				break;
			}
		}
		isTetheredToAtLeastOneObject = (float)list2.Count > 0f;
		if (Object.op_Implicit((Object)(object)tetherVfxOrigin))
		{
			tetherVfxOrigin.SetTetheredTransforms(list2);
		}
		if (Object.op_Implicit((Object)(object)activeVfx))
		{
			activeVfx.SetActive(isTetheredToAtLeastOneObject);
		}
		CollectionPool<Transform, List<Transform>>.ReturnCollection(list2);
		CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
	}

	protected void SearchForTargets(List<HurtBox> dest)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		TeamMask none = TeamMask.none;
		none.AddTeam(networkedBodyAttachment.attachedBody.teamComponent.teamIndex);
		sphereSearch.mask = LayerIndex.entityPrecise.mask;
		sphereSearch.origin = transform.position;
		sphereSearch.radius = radius + networkedBodyAttachment.attachedBody.radius;
		sphereSearch.queryTriggerInteraction = (QueryTriggerInteraction)0;
		sphereSearch.RefreshCandidates();
		sphereSearch.FilterCandidatesByHurtBoxTeam(none);
		sphereSearch.OrderCandidatesByDistance();
		sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
		sphereSearch.GetHurtBoxes(dest);
		sphereSearch.ClearCandidates();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(radius);
			writer.WritePackedUInt32((uint)maxTargets);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(radius);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)maxTargets);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			radius = reader.ReadSingle();
			maxTargets = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			radius = reader.ReadSingle();
		}
		if (((uint)num & 2u) != 0)
		{
			maxTargets = (int)reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
	}
}
