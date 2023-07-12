using UnityEngine;
using UnityEngine.Serialization;

public class VectorPID : MonoBehaviour
{
	[FormerlySerializedAs("name")]
	[Tooltip("Just a field for user naming. Doesn't do anything.")]
	public string customName;

	[Tooltip("PID Constants.")]
	public Vector3 PID = new Vector3(1f, 0f, 0f);

	[HideInInspector]
	[Tooltip("The vector we are currently at.")]
	public Vector3 inputVector = Vector3.zero;

	[HideInInspector]
	[Tooltip("The vector we want to be at.")]
	public Vector3 targetVector = Vector3.zero;

	[HideInInspector]
	[Tooltip("Vector output from PID controller; what we read.")]
	public Vector3 outputVector = Vector3.zero;

	[Tooltip("This is an euler angle, so we need to wrap correctly")]
	public bool isAngle;

	public float gain = 1f;

	private Vector3 errorSum = Vector3.zero;

	private Vector3 deltaError = Vector3.zero;

	private Vector3 lastError = Vector3.zero;

	private float lastTimer;

	private float timer;

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		timer += Time.fixedDeltaTime;
	}

	public Vector3 UpdatePID()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		float num = timer - lastTimer;
		lastTimer = timer;
		if (num != 0f)
		{
			Vector3 val;
			if (isAngle)
			{
				val = Vector3.zero;
				val.x = Mathf.DeltaAngle(inputVector.x, targetVector.x);
				val.y = Mathf.DeltaAngle(inputVector.y, targetVector.y);
				val.z = Mathf.DeltaAngle(inputVector.z, targetVector.z);
			}
			else
			{
				val = targetVector - inputVector;
			}
			errorSum += val * num;
			deltaError = (val - lastError) / num;
			lastError = val;
			outputVector = val * PID.x + errorSum * PID.y + deltaError * PID.z;
			return outputVector * gain;
		}
		return Vector3.zero;
	}
}
