using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(BezierCurveLine))]
public class TarTetherController : NetworkBehaviour
{
	[SyncVar]
	public GameObject targetRoot;

	[SyncVar]
	public GameObject ownerRoot;

	public float reelSpeed = 12f;

	[NonSerialized]
	public float mulchDistanceSqr;

	[NonSerialized]
	public float breakDistanceSqr;

	[NonSerialized]
	public float mulchDamageScale;

	[NonSerialized]
	public float mulchTickIntervalScale;

	[NonSerialized]
	public float damageCoefficientPerTick;

	[NonSerialized]
	public float tickInterval;

	[NonSerialized]
	public float tickTimer;

	public float attachTime;

	private float fixedAge;

	private float age;

	private bool beginSiphon;

	private BezierCurveLine bezierCurveLine;

	private HealthComponent targetHealthComponent;

	private HealthComponent ownerHealthComponent;

	private CharacterBody ownerBody;

	private NetworkInstanceId ___targetRootNetId;

	private NetworkInstanceId ___ownerRootNetId;

	public GameObject NetworktargetRoot
	{
		get
		{
			return targetRoot;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref targetRoot, 1u, ref ___targetRootNetId);
		}
	}

	public GameObject NetworkownerRoot
	{
		get
		{
			return ownerRoot;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref ownerRoot, 2u, ref ___ownerRootNetId);
		}
	}

	private void Awake()
	{
		bezierCurveLine = ((Component)this).GetComponent<BezierCurveLine>();
	}

	private void DoDamageTick(bool mulch)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)targetHealthComponent))
		{
			targetHealthComponent = targetRoot.GetComponent<HealthComponent>();
		}
		if (!Object.op_Implicit((Object)(object)ownerHealthComponent))
		{
			ownerHealthComponent = ownerRoot.GetComponent<HealthComponent>();
		}
		if (!Object.op_Implicit((Object)(object)ownerBody))
		{
			ownerBody = ownerRoot.GetComponent<CharacterBody>();
		}
		if (Object.op_Implicit((Object)(object)ownerRoot))
		{
			DamageInfo damageInfo = new DamageInfo
			{
				position = targetRoot.transform.position,
				attacker = null,
				inflictor = null,
				damage = (mulch ? (damageCoefficientPerTick * mulchDamageScale) : damageCoefficientPerTick) * ownerBody.damage,
				damageColorIndex = DamageColorIndex.Default,
				damageType = DamageType.Generic,
				crit = false,
				force = Vector3.zero,
				procChainMask = default(ProcChainMask),
				procCoefficient = 0f
			};
			targetHealthComponent.TakeDamage(damageInfo);
			if (!damageInfo.rejected)
			{
				ownerHealthComponent.Heal(damageInfo.damage, default(ProcChainMask));
			}
			if (!targetHealthComponent.alive)
			{
				NetworktargetRoot = null;
			}
		}
	}

	private Vector3 GetTargetRootPosition()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)targetRoot))
		{
			Vector3 result = targetRoot.transform.position;
			if (Object.op_Implicit((Object)(object)targetHealthComponent))
			{
				result = targetHealthComponent.body.corePosition;
			}
			return result;
		}
		return ((Component)this).transform.position;
	}

	private void Update()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		age += Time.deltaTime;
		Vector3 position = ownerRoot.transform.position;
		if (!beginSiphon)
		{
			Vector3 position2 = Vector3.Lerp(position, GetTargetRootPosition(), age / attachTime);
			bezierCurveLine.endTransform.position = position2;
		}
		else if (Object.op_Implicit((Object)(object)targetRoot))
		{
			bezierCurveLine.endTransform.position = targetRoot.transform.position;
		}
	}

	private void FixedUpdate()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		fixedAge += Time.fixedDeltaTime;
		if (Object.op_Implicit((Object)(object)targetRoot) && Object.op_Implicit((Object)(object)ownerRoot))
		{
			Vector3 targetRootPosition = GetTargetRootPosition();
			if (!beginSiphon && fixedAge >= attachTime)
			{
				beginSiphon = true;
				return;
			}
			Vector3 val = ownerRoot.transform.position - targetRootPosition;
			if (NetworkServer.active)
			{
				float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
				bool flag = sqrMagnitude < mulchDistanceSqr;
				tickTimer -= Time.fixedDeltaTime;
				if (tickTimer <= 0f)
				{
					tickTimer += (flag ? (tickInterval * mulchTickIntervalScale) : tickInterval);
					DoDamageTick(flag);
				}
				if (sqrMagnitude > breakDistanceSqr)
				{
					Object.Destroy((Object)(object)((Component)this).gameObject);
					return;
				}
			}
			if (!Util.HasEffectiveAuthority(targetRoot))
			{
				return;
			}
			Vector3 val2 = ((Vector3)(ref val)).normalized * (reelSpeed * Time.fixedDeltaTime);
			CharacterMotor component = targetRoot.GetComponent<CharacterMotor>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.rootMotion += val2;
				return;
			}
			Rigidbody component2 = targetRoot.GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.velocity += val2;
			}
		}
		else if (NetworkServer.active)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(targetRoot);
			writer.Write(ownerRoot);
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
			writer.Write(targetRoot);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(ownerRoot);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			___targetRootNetId = reader.ReadNetworkId();
			___ownerRootNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			targetRoot = reader.ReadGameObject();
		}
		if (((uint)num & 2u) != 0)
		{
			ownerRoot = reader.ReadGameObject();
		}
	}

	public override void PreStartClient()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkInstanceId)(ref ___targetRootNetId)).IsEmpty())
		{
			NetworktargetRoot = ClientScene.FindLocalObject(___targetRootNetId);
		}
		if (!((NetworkInstanceId)(ref ___ownerRootNetId)).IsEmpty())
		{
			NetworkownerRoot = ClientScene.FindLocalObject(___ownerRootNetId);
		}
	}
}
