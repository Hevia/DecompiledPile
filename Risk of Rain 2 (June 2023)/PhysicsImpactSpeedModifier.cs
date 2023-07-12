using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsImpactSpeedModifier : MonoBehaviour
{
	public float normalSpeedModifier;

	public float perpendicularSpeedModifier;

	private Rigidbody rigid;

	private void Awake()
	{
		rigid = ((Component)this).GetComponent<Rigidbody>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 normal = ((ContactPoint)(ref collision.contacts[0])).normal;
		Vector3 velocity = rigid.velocity;
		Vector3 val = Vector3.Project(velocity, normal);
		Vector3 val2 = velocity - val;
		val *= normalSpeedModifier;
		val2 *= perpendicularSpeedModifier;
		rigid.velocity = val + val2;
	}
}
