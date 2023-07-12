using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(Collider))]
public class VehicleForceZone : MonoBehaviour
{
	public Rigidbody vehicleRigidbody;

	public float impactMultiplier;

	private Collider collider;

	private void Start()
	{
		collider = ((Component)this).GetComponent<Collider>();
	}

	public void OnTriggerEnter(Collider other)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		CharacterMotor component = ((Component)other).GetComponent<CharacterMotor>();
		HealthComponent component2 = ((Component)other).GetComponent<HealthComponent>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component2))
		{
			Vector3 position = ((Component)this).transform.position;
			Vector3 velocity = vehicleRigidbody.velocity;
			_ = ((Vector3)(ref velocity)).normalized;
			Vector3 pointVelocity = vehicleRigidbody.GetPointVelocity(position);
			Vector3 val = pointVelocity * vehicleRigidbody.mass * impactMultiplier;
			_ = vehicleRigidbody.mass;
			Mathf.Pow(((Vector3)(ref pointVelocity)).magnitude, 2f);
			float num = component.mass / (component.mass + vehicleRigidbody.mass);
			vehicleRigidbody.AddForceAtPosition(-val * num, position);
			Debug.LogFormat("Impulse: {0}, Ratio: {1}", new object[2]
			{
				((Vector3)(ref val)).magnitude,
				num
			});
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.attacker = ((Component)this).gameObject;
			damageInfo.force = val;
			damageInfo.position = position;
			component2.TakeDamageForce(damageInfo, alwaysApply: true);
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		Debug.LogFormat("Hit {0}", new object[1] { collision.gameObject });
		Rigidbody component = ((Component)collision.collider).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Debug.Log((object)"Hit?");
			HealthComponent component2 = ((Component)component).GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				Vector3 point = ((ContactPoint)(ref collision.contacts[0])).point;
				_ = ((ContactPoint)(ref collision.contacts[0])).normal;
				vehicleRigidbody.GetPointVelocity(point);
				Vector3 impulse = collision.impulse;
				float num = 0f;
				vehicleRigidbody.AddForceAtPosition(impulse * num, point);
				Debug.LogFormat("Impulse: {0}, Ratio: {1}", new object[2] { impulse, num });
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.attacker = ((Component)this).gameObject;
				damageInfo.force = -impulse * (1f - num);
				damageInfo.position = point;
				component2.TakeDamageForce(damageInfo, alwaysApply: true);
			}
		}
	}

	private void Update()
	{
	}
}
