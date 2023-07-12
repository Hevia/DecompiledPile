using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(GenericOwnership))]
public class HealBeamController : NetworkBehaviour
{
	public Transform startPointTransform;

	public Transform endPointTransform;

	public float tickInterval = 1f;

	public bool breakOnTargetFullyHealed;

	public LineRenderer lineRenderer;

	public float lingerAfterBrokenDuration;

	[SyncVar(hook = "OnSyncTarget")]
	private HurtBoxReference netTarget;

	private float stopwatchServer;

	private bool broken;

	private HurtBoxReference previousHurtBoxReference;

	private HurtBox cachedHurtBox;

	private float scaleFactorVelocity;

	private float maxLineWidth = 0.3f;

	private float smoothTime = 0.1f;

	private float scaleFactor;

	public GenericOwnership ownership { get; private set; }

	public HurtBox target
	{
		get
		{
			return cachedHurtBox;
		}
		[Server]
		set
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealBeamController::set_target(RoR2.HurtBox)' called on client");
				return;
			}
			NetworknetTarget = HurtBoxReference.FromHurtBox(value);
			UpdateCachedHurtBox();
		}
	}

	public float healRate { get; set; }

	public HurtBoxReference NetworknetTarget
	{
		get
		{
			return netTarget;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncTarget(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<HurtBoxReference>(value, ref netTarget, 1u);
		}
	}

	private void Awake()
	{
		ownership = ((Component)this).GetComponent<GenericOwnership>();
		startPointTransform.SetParent((Transform)null, true);
		endPointTransform.SetParent((Transform)null, true);
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		UpdateCachedHurtBox();
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)startPointTransform))
		{
			Object.Destroy((Object)(object)((Component)startPointTransform).gameObject);
		}
		if (Object.op_Implicit((Object)(object)endPointTransform))
		{
			Object.Destroy((Object)(object)((Component)endPointTransform).gameObject);
		}
	}

	private void OnEnable()
	{
		InstanceTracker.Add<HealBeamController>(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove<HealBeamController>(this);
	}

	private void LateUpdate()
	{
		UpdateHealBeamVisuals();
	}

	private void OnSyncTarget(HurtBoxReference newValue)
	{
		NetworknetTarget = newValue;
		UpdateCachedHurtBox();
	}

	private void FixedUpdate()
	{
		if (NetworkServer.active)
		{
			FixedUpdateServer();
		}
	}

	private void FixedUpdateServer()
	{
		if (!Object.op_Implicit((Object)(object)cachedHurtBox))
		{
			BreakServer();
		}
		else if (tickInterval > 0f)
		{
			stopwatchServer += Time.fixedDeltaTime;
			while (stopwatchServer >= tickInterval)
			{
				stopwatchServer -= tickInterval;
				OnTickServer();
			}
		}
	}

	private void OnTickServer()
	{
		if (!Object.op_Implicit((Object)(object)cachedHurtBox) || !Object.op_Implicit((Object)(object)cachedHurtBox.healthComponent))
		{
			BreakServer();
			return;
		}
		cachedHurtBox.healthComponent.Heal(healRate * tickInterval, default(ProcChainMask));
		if (breakOnTargetFullyHealed && cachedHurtBox.healthComponent.health >= cachedHurtBox.healthComponent.fullHealth)
		{
			BreakServer();
		}
	}

	private void UpdateCachedHurtBox()
	{
		if (!previousHurtBoxReference.Equals(netTarget))
		{
			cachedHurtBox = netTarget.ResolveHurtBox();
			previousHurtBoxReference = netTarget;
		}
	}

	public static bool HealBeamAlreadyExists(GameObject owner, HurtBox target)
	{
		return HealBeamAlreadyExists(owner, target.healthComponent);
	}

	public static bool HealBeamAlreadyExists(GameObject owner, HealthComponent targetHealthComponent)
	{
		List<HealBeamController> instancesList = InstanceTracker.GetInstancesList<HealBeamController>();
		int i = 0;
		for (int count = instancesList.Count; i < count; i++)
		{
			HealBeamController healBeamController = instancesList[i];
			if (healBeamController.target?.healthComponent == targetHealthComponent && healBeamController.ownership.ownerObject == owner)
			{
				return true;
			}
		}
		return false;
	}

	public static int GetHealBeamCountForOwner(GameObject owner)
	{
		int num = 0;
		List<HealBeamController> instancesList = InstanceTracker.GetInstancesList<HealBeamController>();
		int i = 0;
		for (int count = instancesList.Count; i < count; i++)
		{
			if (instancesList[i].ownership.ownerObject == owner)
			{
				num++;
			}
		}
		return num;
	}

	private void UpdateHealBeamVisuals()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		float num = (Object.op_Implicit((Object)(object)target) ? 1f : 0f);
		scaleFactor = Mathf.SmoothDamp(scaleFactor, num, ref scaleFactorVelocity, smoothTime);
		Vector3 localScale = default(Vector3);
		((Vector3)(ref localScale))._002Ector(scaleFactor, scaleFactor, scaleFactor);
		startPointTransform.SetPositionAndRotation(((Component)this).transform.position, ((Component)this).transform.rotation);
		startPointTransform.localScale = localScale;
		if (Object.op_Implicit((Object)(object)cachedHurtBox))
		{
			endPointTransform.position = ((Component)cachedHurtBox).transform.position;
		}
		endPointTransform.localScale = localScale;
		lineRenderer.widthMultiplier = scaleFactor * maxLineWidth;
	}

	[Server]
	public void BreakServer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealBeamController::BreakServer()' called on client");
		}
		else if (!broken)
		{
			broken = true;
			target = null;
			((Component)this).transform.SetParent((Transform)null);
			ownership.ownerObject = null;
			Object.Destroy((Object)(object)((Component)this).gameObject, lingerAfterBrokenDuration);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WriteHurtBoxReference_None(writer, netTarget);
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
			GeneratedNetworkCode._WriteHurtBoxReference_None(writer, netTarget);
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
			netTarget = GeneratedNetworkCode._ReadHurtBoxReference_None(reader);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			OnSyncTarget(GeneratedNetworkCode._ReadHurtBoxReference_None(reader));
		}
	}

	public override void PreStartClient()
	{
	}
}
