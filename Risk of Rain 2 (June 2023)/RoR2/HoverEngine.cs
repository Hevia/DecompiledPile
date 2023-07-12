using UnityEngine;

namespace RoR2;

public class HoverEngine : MonoBehaviour
{
	public Rigidbody engineRigidbody;

	public Transform wheelVisual;

	public float hoverForce = 65f;

	public float hoverHeight = 3.5f;

	public float hoverDamping = 0.1f;

	public float hoverRadius;

	[HideInInspector]
	public float forceStrength;

	private Ray castRay;

	private Vector3 castPosition;

	[HideInInspector]
	public RaycastHit raycastHit;

	public float compression;

	public Vector3 offsetVector = Vector3.zero;

	public bool isGrounded;

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(((Component)this).transform.TransformPoint(offsetVector), hoverRadius);
		Gizmos.DrawRay(castRay);
		if (isGrounded)
		{
			Gizmos.DrawSphere(((RaycastHit)(ref raycastHit)).point, hoverRadius);
		}
	}

	private void FixedUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01(Vector3.Dot(-((Component)this).transform.up, Vector3.down));
		castPosition = ((Component)this).transform.TransformPoint(offsetVector);
		castRay = new Ray(castPosition, -((Component)this).transform.up);
		isGrounded = false;
		forceStrength = 0f;
		compression = 0f;
		Vector3 position = ((Ray)(ref castRay)).origin + ((Ray)(ref castRay)).direction * hoverHeight;
		if (Physics.SphereCast(castRay, hoverRadius, ref raycastHit, hoverHeight, LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			isGrounded = true;
			float num2 = (hoverHeight - ((RaycastHit)(ref raycastHit)).distance) / hoverHeight;
			Vector3 val = Vector3.up * (num2 * hoverForce);
			Vector3 val2 = Vector3.Project(engineRigidbody.GetPointVelocity(castPosition), -((Component)this).transform.up) * hoverDamping;
			Vector3 val3 = val - val2;
			forceStrength = ((Vector3)(ref val3)).magnitude;
			engineRigidbody.AddForceAtPosition(Vector3.Project(val - val2, -((Component)this).transform.up), castPosition, (ForceMode)5);
			compression = Mathf.Clamp01(num2 * num);
			position = ((RaycastHit)(ref raycastHit)).point;
		}
		wheelVisual.position = position;
		_ = isGrounded;
	}
}
