using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace RoR2;

[RequireComponent(typeof(NetworkedBodyAttachment))]
public class HelfireController : NetworkBehaviour
{
	[SyncVar]
	public int stack = 1;

	[FormerlySerializedAs("radius")]
	public float baseRadius;

	public float dotDuration;

	public float interval;

	[Range(0f, 1f)]
	public float healthFractionPerSecond = 0.05f;

	public float allyDamageScalar = 0.5f;

	public float enemyDamageScalar = 24f;

	public Transform auraEffectTransform;

	private float timer;

	private float radius;

	private CameraTargetParams.AimRequest aimRequest;

	private CameraTargetParams cameraTargetParams;

	public NetworkedBodyAttachment networkedBodyAttachment { get; private set; }

	public int Networkstack
	{
		get
		{
			return stack;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref stack, 1u);
		}
	}

	private void Awake()
	{
		networkedBodyAttachment = ((Component)this).GetComponent<NetworkedBodyAttachment>();
		auraEffectTransform.SetParent((Transform)null);
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)auraEffectTransform))
		{
			Object.Destroy((Object)(object)((Component)auraEffectTransform).gameObject);
			auraEffectTransform = null;
		}
		aimRequest?.Dispose();
	}

	private void FixedUpdate()
	{
		radius = baseRadius * (1f + (float)(stack - 1) * 0.5f);
		if (NetworkServer.active)
		{
			ServerFixedUpdate();
		}
	}

	private void ServerFixedUpdate()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		timer -= Time.fixedDeltaTime;
		if (!(timer <= 0f))
		{
			return;
		}
		timer = interval;
		float num = healthFractionPerSecond * dotDuration * networkedBodyAttachment.attachedBody.healthComponent.fullCombinedHealth;
		float num2 = 1f;
		TeamDef teamDef = TeamCatalog.GetTeamDef(networkedBodyAttachment.attachedBody.teamComponent.teamIndex);
		if (teamDef != null && teamDef.friendlyFireScaling > 0f)
		{
			num2 = 1f / teamDef.friendlyFireScaling;
		}
		Collider[] array = Physics.OverlapSphere(((Component)this).transform.position, radius, LayerMask.op_Implicit(LayerIndex.entityPrecise.mask), (QueryTriggerInteraction)2);
		GameObject[] array2 = (GameObject[])(object)new GameObject[array.Length];
		int count = 0;
		for (int i = 0; i < array.Length; i++)
		{
			CharacterBody characterBody = Util.HurtBoxColliderToBody(array[i]);
			GameObject val = (Object.op_Implicit((Object)(object)characterBody) ? ((Component)characterBody).gameObject : null);
			if (!Object.op_Implicit((Object)(object)val) || Array.IndexOf(array2, val, 0, count) != -1)
			{
				continue;
			}
			float num3 = num;
			float num4 = 1f;
			if (networkedBodyAttachment.attachedBody.teamComponent.teamIndex == characterBody.teamComponent.teamIndex)
			{
				num3 *= num2;
				num4 *= num2;
				if (networkedBodyAttachment.attachedBody != characterBody)
				{
					num3 *= allyDamageScalar;
					num4 *= allyDamageScalar;
				}
			}
			else
			{
				num3 *= enemyDamageScalar;
				num4 *= enemyDamageScalar;
			}
			InflictDotInfo inflictDotInfo = default(InflictDotInfo);
			inflictDotInfo.attackerObject = networkedBodyAttachment.attachedBodyObject;
			inflictDotInfo.victimObject = val;
			inflictDotInfo.totalDamage = num3;
			inflictDotInfo.damageMultiplier = num4;
			inflictDotInfo.dotIndex = DotController.DotIndex.Helfire;
			inflictDotInfo.maxStacksFromAttacker = 1u;
			InflictDotInfo dotInfo = inflictDotInfo;
			StrengthenBurnUtils.CheckDotForUpgrade(networkedBodyAttachment.attachedBody.inventory, ref dotInfo);
			DotController.InflictDot(ref dotInfo);
			array2[count++] = val;
		}
	}

	private void LateUpdate()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		CharacterBody attachedBody = networkedBodyAttachment.attachedBody;
		if (Object.op_Implicit((Object)(object)attachedBody))
		{
			auraEffectTransform.position = networkedBodyAttachment.attachedBody.corePosition;
			auraEffectTransform.localScale = new Vector3(radius, radius, radius);
			if (!Object.op_Implicit((Object)(object)cameraTargetParams))
			{
				cameraTargetParams = ((Component)attachedBody).GetComponent<CameraTargetParams>();
			}
			else if (aimRequest == null)
			{
				aimRequest = cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
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
			writer.WritePackedUInt32((uint)stack);
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
			writer.WritePackedUInt32((uint)stack);
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
			stack = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			stack = (int)reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
	}
}
