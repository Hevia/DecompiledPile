using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(QuaternionPID))]
[RequireComponent(typeof(VectorPID))]
public class RigidbodyDirection : MonoBehaviour
{
	public Vector3 aimDirection = Vector3.one;

	public Rigidbody rigid;

	public QuaternionPID angularVelocityPID;

	public VectorPID torquePID;

	public bool freezeXRotation;

	public bool freezeYRotation;

	public bool freezeZRotation;

	private ModelLocator modelLocator;

	private Animator animator;

	public string animatorXCycle;

	public string animatorYCycle;

	public string animatorZCycle;

	public float animatorTorqueScale;

	private InputBankTest inputBank;

	private Vector3 targetTorque;

	private void Start()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		inputBank = ((Component)this).GetComponent<InputBankTest>();
		modelLocator = ((Component)this).GetComponent<ModelLocator>();
		if (Object.op_Implicit((Object)(object)modelLocator))
		{
			Transform modelTransform = modelLocator.modelTransform;
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				animator = ((Component)modelTransform).GetComponent<Animator>();
			}
		}
		aimDirection = ((Component)this).transform.forward;
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)animator))
		{
			if (animatorXCycle.Length > 0)
			{
				animator.SetFloat(animatorXCycle, Mathf.Clamp(0.5f + targetTorque.x * 0.5f * animatorTorqueScale, -1f, 1f), 0.1f, Time.deltaTime);
			}
			if (animatorYCycle.Length > 0)
			{
				animator.SetFloat(animatorYCycle, Mathf.Clamp(0.5f + targetTorque.y * 0.5f * animatorTorqueScale, -1f, 1f), 0.1f, Time.deltaTime);
			}
			if (animatorZCycle.Length > 0)
			{
				animator.SetFloat(animatorZCycle, Mathf.Clamp(0.5f + targetTorque.z * 0.5f * animatorTorqueScale, -1f, 1f), 0.1f, Time.deltaTime);
			}
		}
	}

	private void FixedUpdate()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)inputBank) && Object.op_Implicit((Object)(object)rigid) && Object.op_Implicit((Object)(object)angularVelocityPID) && Object.op_Implicit((Object)(object)torquePID))
		{
			angularVelocityPID.inputQuat = rigid.rotation;
			Quaternion targetQuat = Util.QuaternionSafeLookRotation(aimDirection);
			if (freezeXRotation)
			{
				targetQuat.x = 0f;
			}
			if (freezeYRotation)
			{
				targetQuat.y = 0f;
			}
			if (freezeZRotation)
			{
				targetQuat.z = 0f;
			}
			angularVelocityPID.targetQuat = targetQuat;
			Vector3 targetVector = angularVelocityPID.UpdatePID();
			torquePID.inputVector = rigid.angularVelocity;
			torquePID.targetVector = targetVector;
			Vector3 val = torquePID.UpdatePID();
			rigid.AddTorque(val, (ForceMode)5);
		}
	}
}
