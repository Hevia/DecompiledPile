using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class VelocityRandomOnStart : MonoBehaviour
{
	public enum DirectionMode
	{
		Sphere,
		Hemisphere,
		Cone
	}

	public float minSpeed;

	public float maxSpeed;

	public Vector3 baseDirection = Vector3.up;

	public bool localDirection;

	public DirectionMode directionMode;

	public float coneAngle = 30f;

	[Tooltip("Minimum angular speed in degrees/second.")]
	public float minAngularSpeed;

	[Tooltip("Maximum angular speed in degrees/second.")]
	public float maxAngularSpeed;

	private void Start()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		float num = ((minSpeed != maxSpeed) ? Random.Range(minSpeed, maxSpeed) : minSpeed);
		if (num != 0f)
		{
			Vector3 val = Vector3.zero;
			Vector3 val2 = (localDirection ? (((Component)this).transform.rotation * baseDirection) : baseDirection);
			switch (directionMode)
			{
			case DirectionMode.Sphere:
				val = Random.onUnitSphere;
				break;
			case DirectionMode.Hemisphere:
				val = Random.onUnitSphere;
				if (Vector3.Dot(val, val2) < 0f)
				{
					val = -val;
				}
				break;
			case DirectionMode.Cone:
				val = Util.ApplySpread(val2, 0f, coneAngle, 1f, 1f);
				break;
			}
			component.velocity = val * num;
		}
		float num2 = ((minAngularSpeed != maxAngularSpeed) ? Random.Range(minAngularSpeed, maxAngularSpeed) : minAngularSpeed);
		if (num2 != 0f)
		{
			component.angularVelocity = Random.onUnitSphere * (num2 * (MathF.PI / 180f));
		}
	}
}
