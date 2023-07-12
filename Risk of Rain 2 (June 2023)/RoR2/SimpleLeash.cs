using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[DefaultExecutionOrder(99999)]
public class SimpleLeash : MonoBehaviour
{
	public float minLeashRadius = 1f;

	public float maxLeashRadius = 20f;

	public float maxFollowSpeed = 40f;

	public float smoothTime = 0.15f;

	private Transform transform;

	[CanBeNull]
	private NetworkIdentity networkIdentity;

	private Vector3 velocity = Vector3.zero;

	private bool isNetworkControlled;

	public Vector3 leashOrigin { get; set; }

	private void Awake()
	{
		transform = ((Component)this).transform;
		networkIdentity = ((Component)this).GetComponent<NetworkIdentity>();
	}

	private void OnEnable()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		leashOrigin = transform.position;
	}

	private void LateUpdate()
	{
		isNetworkControlled = networkIdentity != null && !Util.HasEffectiveAuthority(networkIdentity);
		if (!isNetworkControlled)
		{
			Simulate(Time.deltaTime);
		}
	}

	private void Simulate(float deltaTime)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = transform.position;
		Vector3 val = leashOrigin;
		Vector3 val2 = position - val;
		float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
		if (sqrMagnitude > minLeashRadius * minLeashRadius)
		{
			float num = Mathf.Sqrt(sqrMagnitude);
			Vector3 val3 = val2 / num;
			Vector3 val4 = val + val3 * minLeashRadius;
			Vector3 val5 = position;
			if (num > maxLeashRadius)
			{
				val5 = val + val3 * maxLeashRadius;
			}
			val5 = Vector3.SmoothDamp(val5, val4, ref velocity, smoothTime, maxFollowSpeed, deltaTime);
			if (val5 != position)
			{
				transform.position = val5;
			}
		}
	}
}
