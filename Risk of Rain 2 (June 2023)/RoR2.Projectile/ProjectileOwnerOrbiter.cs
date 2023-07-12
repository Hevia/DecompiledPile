using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[DisallowMultipleComponent]
[RequireComponent(typeof(ProjectileController))]
public class ProjectileOwnerOrbiter : NetworkBehaviour
{
	[SyncVar]
	[SerializeField]
	private Vector3 offset;

	[SyncVar]
	[SerializeField]
	private float initialDegreesFromOwnerForward;

	[SyncVar]
	[SerializeField]
	private float degreesPerSecond;

	[SerializeField]
	[SyncVar]
	private float radius;

	[SyncVar]
	[SerializeField]
	private Vector3 planeNormal = Vector3.up;

	private Transform ownerTransform;

	private Rigidbody rigidBody;

	private bool resetOnAcquireOwner = true;

	[SyncVar]
	private Vector3 initialRadialDirection;

	[SyncVar]
	private float initialRunTime;

	public Vector3 Networkoffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return offset;
		}
		[param: In]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((NetworkBehaviour)this).SetSyncVar<Vector3>(value, ref offset, 1u);
		}
	}

	public float NetworkinitialDegreesFromOwnerForward
	{
		get
		{
			return initialDegreesFromOwnerForward;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref initialDegreesFromOwnerForward, 2u);
		}
	}

	public float NetworkdegreesPerSecond
	{
		get
		{
			return degreesPerSecond;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref degreesPerSecond, 4u);
		}
	}

	public float Networkradius
	{
		get
		{
			return radius;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref radius, 8u);
		}
	}

	public Vector3 NetworkplaneNormal
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return planeNormal;
		}
		[param: In]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((NetworkBehaviour)this).SetSyncVar<Vector3>(value, ref planeNormal, 16u);
		}
	}

	public Vector3 NetworkinitialRadialDirection
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return initialRadialDirection;
		}
		[param: In]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((NetworkBehaviour)this).SetSyncVar<Vector3>(value, ref initialRadialDirection, 32u);
		}
	}

	public float NetworkinitialRunTime
	{
		get
		{
			return initialRunTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref initialRunTime, 64u);
		}
	}

	public void Initialize(Vector3 planeNormal, float radius, float degreesPerSecond, float initialDegreesFromOwnerForward)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		NetworkplaneNormal = planeNormal;
		Networkradius = radius;
		NetworkdegreesPerSecond = degreesPerSecond;
		NetworkinitialDegreesFromOwnerForward = initialDegreesFromOwnerForward;
		ResetState();
	}

	private void OnEnable()
	{
		rigidBody = ((Component)this).GetComponent<Rigidbody>();
		ProjectileController component = ((Component)this).GetComponent<ProjectileController>();
		if (Object.op_Implicit((Object)(object)component.owner))
		{
			AcquireOwner(component);
		}
		else
		{
			component.onInitialized += AcquireOwner;
		}
	}

	public void FixedUpdate()
	{
		UpdatePosition(doSnap: false);
	}

	private void ResetState()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		NetworkinitialRunTime = Run.instance.GetRunStopwatch();
		((Vector3)(ref planeNormal)).Normalize();
		if (Object.op_Implicit((Object)(object)ownerTransform))
		{
			NetworkinitialRadialDirection = Quaternion.AngleAxis(initialDegreesFromOwnerForward, planeNormal) * ownerTransform.forward;
			resetOnAcquireOwner = false;
		}
		UpdatePosition(doSnap: true);
	}

	private void UpdatePosition(bool doSnap)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ownerTransform))
		{
			float num = (Run.instance.GetRunStopwatch() - initialRunTime) * degreesPerSecond;
			Vector3 val = ownerTransform.position + offset + Quaternion.AngleAxis(num, planeNormal) * initialRadialDirection * radius;
			if (!Object.op_Implicit((Object)(object)rigidBody) || doSnap)
			{
				((Component)this).transform.position = val;
			}
			else if (Object.op_Implicit((Object)(object)rigidBody))
			{
				rigidBody.MovePosition(val);
			}
		}
	}

	private void AcquireOwner(ProjectileController controller)
	{
		ownerTransform = controller.owner.transform;
		controller.onInitialized -= AcquireOwner;
		if (resetOnAcquireOwner)
		{
			resetOnAcquireOwner = false;
			ResetState();
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		if (forceAll)
		{
			writer.Write(offset);
			writer.Write(initialDegreesFromOwnerForward);
			writer.Write(degreesPerSecond);
			writer.Write(radius);
			writer.Write(planeNormal);
			writer.Write(initialRadialDirection);
			writer.Write(initialRunTime);
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
			writer.Write(offset);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(initialDegreesFromOwnerForward);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(degreesPerSecond);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(radius);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(planeNormal);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(initialRadialDirection);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(initialRunTime);
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
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			offset = reader.ReadVector3();
			initialDegreesFromOwnerForward = reader.ReadSingle();
			degreesPerSecond = reader.ReadSingle();
			radius = reader.ReadSingle();
			planeNormal = reader.ReadVector3();
			initialRadialDirection = reader.ReadVector3();
			initialRunTime = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			offset = reader.ReadVector3();
		}
		if (((uint)num & 2u) != 0)
		{
			initialDegreesFromOwnerForward = reader.ReadSingle();
		}
		if (((uint)num & 4u) != 0)
		{
			degreesPerSecond = reader.ReadSingle();
		}
		if (((uint)num & 8u) != 0)
		{
			radius = reader.ReadSingle();
		}
		if (((uint)num & 0x10u) != 0)
		{
			planeNormal = reader.ReadVector3();
		}
		if (((uint)num & 0x20u) != 0)
		{
			initialRadialDirection = reader.ReadVector3();
		}
		if (((uint)num & 0x40u) != 0)
		{
			initialRunTime = reader.ReadSingle();
		}
	}

	public override void PreStartClient()
	{
	}
}
