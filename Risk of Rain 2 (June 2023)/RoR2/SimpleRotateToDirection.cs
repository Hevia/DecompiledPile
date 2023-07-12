using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[DefaultExecutionOrder(99999)]
public class SimpleRotateToDirection : MonoBehaviour
{
	public float smoothTime;

	public float maxRotationSpeed;

	private Transform transform;

	[CanBeNull]
	private NetworkIdentity networkIdentity;

	private float velocity;

	private bool isNetworkControlled;

	public Quaternion targetRotation { get; set; }

	private void Awake()
	{
		transform = ((Component)this).transform;
		networkIdentity = ((Component)this).GetComponent<NetworkIdentity>();
	}

	private void OnEnable()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		targetRotation = transform.rotation;
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
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		transform.rotation = Util.SmoothDampQuaternion(transform.rotation, targetRotation, ref velocity, smoothTime, maxRotationSpeed, deltaTime);
	}
}
