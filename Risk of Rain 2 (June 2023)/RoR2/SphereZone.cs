using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class SphereZone : BaseZoneBehavior, IZone
{
	[Tooltip("The area of effect.")]
	[SyncVar]
	public float radius;

	[Tooltip("The child range indicator object. Will be scaled to the radius.")]
	public Transform rangeIndicator;

	[Tooltip("The time it takes the range indicator to update")]
	public float indicatorSmoothTime = 0.2f;

	[Tooltip("If false, \"IsInBounds\" returns true when inside the sphere.  If true, outside the sphere is considered in bounds.")]
	public bool isInverted;

	private float rangeIndicatorScaleVelocity;

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

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)rangeIndicator))
		{
			((Component)rangeIndicator).gameObject.SetActive(true);
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)rangeIndicator))
		{
			((Component)rangeIndicator).gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)rangeIndicator))
		{
			float num = Mathf.SmoothDamp(rangeIndicator.localScale.x, radius, ref rangeIndicatorScaleVelocity, indicatorSmoothTime);
			rangeIndicator.localScale = new Vector3(num, num, num);
		}
	}

	public override bool IsInBounds(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = position - ((Component)this).transform.position;
		if (isInverted)
		{
			return ((Vector3)(ref val)).sqrMagnitude > radius * radius;
		}
		return ((Vector3)(ref val)).sqrMagnitude <= radius * radius;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(radius);
			return true;
		}
		bool flag2 = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(radius);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			radius = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			radius = reader.ReadSingle();
		}
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
