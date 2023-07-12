using UnityEngine;

namespace RoR2;

[ExecuteAlways]
public class GlassesSize : MonoBehaviour
{
	public Transform glassesModelBase;

	public Transform glassesBridgeLeft;

	public Transform glassesBridgeRight;

	public float bridgeOffsetScale;

	public Vector3 offsetVector = Vector3.right;

	private void Start()
	{
	}

	private void Update()
	{
		UpdateGlasses();
	}

	private void UpdateGlasses()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localScale = ((Component)this).transform.localScale;
		float num = Mathf.Max(localScale.y, localScale.z);
		Vector3 localScale2 = default(Vector3);
		((Vector3)(ref localScale2))._002Ector(1f / localScale.x * num, 1f / localScale.y * num, 1f / localScale.z * num);
		if (Object.op_Implicit((Object)(object)glassesModelBase))
		{
			((Component)glassesModelBase).transform.localScale = localScale2;
		}
		if (Object.op_Implicit((Object)(object)glassesBridgeLeft) && Object.op_Implicit((Object)(object)glassesBridgeRight))
		{
			float num2 = (localScale.x / num - 1f) * bridgeOffsetScale;
			((Component)glassesBridgeLeft).transform.localPosition = offsetVector * (0f - num2);
			((Component)glassesBridgeRight).transform.localPosition = offsetVector * num2;
		}
	}
}
