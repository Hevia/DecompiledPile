using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class BoxZone : BaseZoneBehavior, IZone
{
	[Tooltip("If false, \"IsInBounds\" returns true when inside the box.  If true, outside the box is considered in bounds.")]
	[SerializeField]
	private bool isInverted;

	public override bool IsInBounds(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = position - ((Component)this).transform.position;
		val.x = Mathf.Abs(val.x);
		val.y = Mathf.Abs(val.y);
		val.z = Mathf.Abs(val.z);
		Vector3 val2 = ((Component)this).transform.lossyScale * 0.5f;
		if (isInverted)
		{
			if (val.x > val2.x && val.y > val2.y)
			{
				return val.z > val2.z;
			}
			return false;
		}
		if (val.x < val2.x && val.y < val2.y)
		{
			return val.z < val2.z;
		}
		return false;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
