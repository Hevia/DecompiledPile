using UnityEngine;

public class PhysicsController : MonoBehaviour
{
	public Vector3 centerOfMass = Vector3.zero;

	private Rigidbody carRigidbody;

	public Transform cameraTransform;

	public Vector3 PID = new Vector3(1f, 0f, 0f);

	public bool turnOnInput;

	private Vector3 errorSum = Vector3.zero;

	private Vector3 deltaError = Vector3.zero;

	private Vector3 lastError = Vector3.zero;

	private Vector3 desiredHeading;

	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(((Component)this).transform.TransformPoint(centerOfMass), 0.5f);
	}

	private void Awake()
	{
		carRigidbody = ((Component)this).GetComponent<Rigidbody>();
	}

	private void Update()
	{
	}

	private void FixedUpdate()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		if (!turnOnInput || Input.GetAxis("Vertical") > 0f || Input.GetAxis("Vertical") > 0f)
		{
			desiredHeading = cameraTransform.forward;
			desiredHeading = Vector3.Project(desiredHeading, ((Component)this).transform.forward);
			desiredHeading = cameraTransform.forward - desiredHeading;
			Debug.DrawRay(((Component)this).transform.position, desiredHeading * 15f, Color.magenta);
		}
		Vector3 val = -((Component)this).transform.up;
		Debug.DrawRay(((Component)this).transform.position, val * 15f, Color.blue);
		Vector3 val2 = Vector3.Cross(val, desiredHeading);
		Debug.DrawRay(((Component)this).transform.position, val2 * 15f, Color.red);
		val2.x = 0f;
		val2.z = 0f;
		errorSum += val2 * Time.fixedDeltaTime;
		deltaError = (val2 - lastError) / Time.fixedDeltaTime;
		lastError = val2;
		carRigidbody.AddTorque(val2 * PID.x + errorSum * PID.y + deltaError * PID.z, (ForceMode)5);
	}
}
