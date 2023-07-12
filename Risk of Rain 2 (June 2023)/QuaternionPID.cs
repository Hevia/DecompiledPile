using UnityEngine;
using UnityEngine.Serialization;

public class QuaternionPID : MonoBehaviour
{
	[FormerlySerializedAs("name")]
	[Tooltip("Just a field for user naming. Doesn't do anything.")]
	public string customName;

	[Tooltip("PID Constants.")]
	public Vector3 PID = new Vector3(1f, 0f, 0f);

	[Tooltip("The quaternion we are currently at.")]
	public Quaternion inputQuat = Quaternion.identity;

	[Tooltip("The quaternion we want to be at.")]
	public Quaternion targetQuat = Quaternion.identity;

	[Tooltip("Vector output from PID controller; what we read.")]
	[HideInInspector]
	public Vector3 outputVector = Vector3.zero;

	public float gain = 1f;

	private Vector3 errorSum = Vector3.zero;

	private Vector3 deltaError = Vector3.zero;

	private Vector3 lastError = Vector3.zero;

	private float lastTimer;

	private float timer;

	private void Start()
	{
	}

	private void Update()
	{
		timer += Time.deltaTime;
	}

	public Vector3 UpdatePID()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		float num = timer - lastTimer;
		lastTimer = timer;
		if (num != 0f)
		{
			Quaternion val = targetQuat * Quaternion.Inverse(inputQuat);
			if (val.w < 0f)
			{
				val.x *= -1f;
				val.y *= -1f;
				val.z *= -1f;
				val.w *= -1f;
			}
			Vector3 val2 = default(Vector3);
			val2.x = val.x;
			val2.y = val.y;
			val2.z = val.z;
			errorSum += val2 * num;
			deltaError = (val2 - lastError) / num;
			lastError = val2;
			outputVector = val2 * PID.x + errorSum * PID.y + deltaError * PID.z;
			return outputVector * gain;
		}
		return Vector3.zero;
	}
}
