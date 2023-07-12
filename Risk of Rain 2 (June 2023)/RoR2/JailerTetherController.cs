using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(BezierCurveLine))]
public class JailerTetherController : NetworkBehaviour
{
	[SyncVar]
	public GameObject targetRoot;

	[SyncVar]
	public GameObject ownerRoot;

	[SyncVar]
	public GameObject origin;

	[SyncVar]
	public float reelSpeed = 12f;

	[NonSerialized]
	public float breakDistanceSqr;

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

	private CharacterBody ownerBody;

	private CharacterBody targetBody;

	private BuffDef tetheredBuff;

	private NetworkInstanceId ___targetRootNetId;

	private NetworkInstanceId ___ownerRootNetId;

	private NetworkInstanceId ___originNetId;

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

	public GameObject Networkorigin
	{
		get
		{
			return origin;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref origin, 4u, ref ___originNetId);
		}
	}

	public float NetworkreelSpeed
	{
		get
		{
			return reelSpeed;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref reelSpeed, 8u);
		}
	}

	private void Awake()
	{
		bezierCurveLine = ((Component)this).GetComponent<BezierCurveLine>();
	}

	private void Start()
	{
		if (!Object.op_Implicit((Object)(object)targetHealthComponent))
		{
			targetHealthComponent = targetRoot.GetComponent<HealthComponent>();
		}
		if (!Object.op_Implicit((Object)(object)ownerBody))
		{
			ownerBody = ownerRoot.GetComponent<CharacterBody>();
		}
		Networkorigin = (((Object)(object)origin != (Object)null) ? origin : ownerRoot);
	}

	private void LateUpdate()
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
		Vector3 position = origin.transform.position;
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
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		fixedAge += Time.fixedDeltaTime;
		if (Object.op_Implicit((Object)(object)targetRoot) && Object.op_Implicit((Object)(object)ownerRoot))
		{
			Vector3 targetRootPosition = GetTargetRootPosition();
			if (!beginSiphon && fixedAge >= attachTime)
			{
				beginSiphon = true;
				return;
			}
			Vector3 val = origin.transform.position - targetRootPosition;
			if (NetworkServer.active)
			{
				float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
				tickTimer -= Time.fixedDeltaTime;
				if (tickTimer <= 0f)
				{
					tickTimer += tickInterval;
					DoDamageTick();
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
			Vector3 val2 = reelSpeed * Time.fixedDeltaTime * ((Vector3)(ref val)).normalized;
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

	private void DoDamageTick()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		DamageInfo damageInfo = new DamageInfo
		{
			position = targetRoot.transform.position,
			attacker = null,
			inflictor = null,
			damage = damageCoefficientPerTick * ownerBody.damage,
			damageColorIndex = DamageColorIndex.Default,
			damageType = DamageType.Generic,
			crit = false,
			force = Vector3.zero,
			procChainMask = default(ProcChainMask),
			procCoefficient = 0f
		};
		targetHealthComponent.TakeDamage(damageInfo);
		if (!targetHealthComponent.alive)
		{
			NetworktargetRoot = null;
		}
	}

	public override void OnNetworkDestroy()
	{
		if (NetworkServer.active)
		{
			RemoveBuff();
		}
		((NetworkBehaviour)this).OnNetworkDestroy();
	}

	private void RemoveBuff()
	{
		if (Object.op_Implicit((Object)(object)tetheredBuff) && Object.op_Implicit((Object)(object)targetBody))
		{
			targetBody.RemoveBuff(tetheredBuff);
		}
	}

	public CharacterBody GetTargetBody()
	{
		if (Object.op_Implicit((Object)(object)targetBody))
		{
			return targetBody;
		}
		if (Object.op_Implicit((Object)(object)targetRoot))
		{
			targetBody = targetRoot.GetComponent<CharacterBody>();
			return targetBody;
		}
		return null;
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

	public void SetTetheredBuff(BuffDef buffDef)
	{
		if ((Object)(object)buffDef != (Object)null)
		{
			CharacterBody characterBody = GetTargetBody();
			if (Object.op_Implicit((Object)(object)characterBody))
			{
				tetheredBuff = buffDef;
				characterBody.AddBuff(tetheredBuff);
			}
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
			writer.Write(origin);
			writer.Write(reelSpeed);
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
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(origin);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(reelSpeed);
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
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			___targetRootNetId = reader.ReadNetworkId();
			___ownerRootNetId = reader.ReadNetworkId();
			___originNetId = reader.ReadNetworkId();
			reelSpeed = reader.ReadSingle();
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
		if (((uint)num & 4u) != 0)
		{
			origin = reader.ReadGameObject();
		}
		if (((uint)num & 8u) != 0)
		{
			reelSpeed = reader.ReadSingle();
		}
	}

	public override void PreStartClient()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkInstanceId)(ref ___targetRootNetId)).IsEmpty())
		{
			NetworktargetRoot = ClientScene.FindLocalObject(___targetRootNetId);
		}
		if (!((NetworkInstanceId)(ref ___ownerRootNetId)).IsEmpty())
		{
			NetworkownerRoot = ClientScene.FindLocalObject(___ownerRootNetId);
		}
		if (!((NetworkInstanceId)(ref ___originNetId)).IsEmpty())
		{
			Networkorigin = ClientScene.FindLocalObject(___originNetId);
		}
	}
}
