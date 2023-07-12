using UnityEngine;

namespace RoR2.Mecanim;

public class CrouchMecanim : MonoBehaviour
{
	public float duckHeight;

	public Animator animator;

	public float smoothdamp;

	public float initialVerticalOffset;

	public Transform crouchOriginOverride;

	private float crouchCycle;

	private const float crouchRaycastFrequency = 2f;

	private float crouchStopwatch;

	private static readonly int crouchCycleOffsetParamNameHash = Animator.StringToHash("crouchCycleOffset");

	private void FixedUpdate()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		crouchStopwatch -= Time.fixedDeltaTime;
		if (crouchStopwatch <= 0f)
		{
			crouchStopwatch = 0.5f;
			Transform val = (Object.op_Implicit((Object)(object)crouchOriginOverride) ? crouchOriginOverride : ((Component)this).transform);
			Vector3 up = val.up;
			RaycastHit val2 = default(RaycastHit);
			bool flag = Physics.Raycast(new Ray(val.position - up * initialVerticalOffset, up), ref val2, duckHeight + initialVerticalOffset, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1);
			crouchCycle = (flag ? Mathf.Clamp01(1f - (((RaycastHit)(ref val2)).distance - initialVerticalOffset) / duckHeight) : 0f);
		}
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.SetFloat(crouchCycleOffsetParamNameHash, crouchCycle, smoothdamp, Time.deltaTime);
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + ((Component)this).transform.up * duckHeight);
		Gizmos.color = Color.red;
		Gizmos.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + -((Component)this).transform.up * initialVerticalOffset);
	}
}
