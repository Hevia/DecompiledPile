using RoR2;
using UnityEngine;

namespace EntityStates;

public class FlyState : BaseState
{
	private Animator modelAnimator;

	private bool skill1InputReceived;

	private bool skill2InputReceived;

	private bool skill3InputReceived;

	private bool skill4InputReceived;

	private bool hasPivotPitchLayer;

	private bool hasPivotYawLayer;

	private bool hasPivotRollLayer;

	private static readonly int pivotPitchCycle = Animator.StringToHash("pivotPitchCycle");

	private static readonly int pivotYawCycle = Animator.StringToHash("pivotYawCycle");

	private static readonly int pivotRollCycle = Animator.StringToHash("pivotRollCycle");

	private static readonly int flyRate = Animator.StringToHash("fly.rate");

	public override void OnEnter()
	{
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		PlayAnimation("Body", "Idle");
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			hasPivotPitchLayer = modelAnimator.GetLayerIndex("PivotPitch") != -1;
			hasPivotYawLayer = modelAnimator.GetLayerIndex("PivotYaw") != -1;
			hasPivotRollLayer = modelAnimator.GetLayerIndex("PivotRoll") != -1;
		}
	}

	public override void Update()
	{
		base.Update();
	}

	public override void FixedUpdate()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)base.rigidbodyDirection))
		{
			Quaternion rotation = base.transform.rotation;
			Quaternion val = Util.QuaternionSafeLookRotation(base.rigidbodyDirection.aimDirection);
			Quaternion val2 = Quaternion.Inverse(rotation) * val;
			if (Object.op_Implicit((Object)(object)modelAnimator))
			{
				if (hasPivotPitchLayer)
				{
					modelAnimator.SetFloat(pivotPitchCycle, Mathf.Clamp01(Util.Remap(val2.x * Mathf.Sign(val2.w), -1f, 1f, 0f, 1f)), 1f, Time.fixedDeltaTime);
				}
				if (hasPivotYawLayer)
				{
					modelAnimator.SetFloat(pivotYawCycle, Mathf.Clamp01(Util.Remap(val2.y * Mathf.Sign(val2.w), -1f, 1f, 0f, 1f)), 1f, Time.fixedDeltaTime);
				}
				if (hasPivotRollLayer)
				{
					modelAnimator.SetFloat(pivotRollCycle, Mathf.Clamp01(Util.Remap(val2.z * Mathf.Sign(val2.w), -1f, 1f, 0f, 1f)), 1f, Time.fixedDeltaTime);
				}
			}
		}
		PerformInputs();
	}

	protected virtual bool CanExecuteSkill(GenericSkill skillSlot)
	{
		return true;
	}

	protected virtual void PerformInputs()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isAuthority)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
			{
				base.rigidbodyMotor.moveVector = base.inputBank.moveVector * base.characterBody.moveSpeed;
				if (Object.op_Implicit((Object)(object)modelAnimator))
				{
					modelAnimator.SetFloat(flyRate, Vector3.Magnitude(base.rigidbodyMotor.rigid.velocity));
				}
			}
			if (Object.op_Implicit((Object)(object)base.rigidbodyDirection))
			{
				RigidbodyDirection obj = base.rigidbodyDirection;
				Ray aimRay = GetAimRay();
				obj.aimDirection = aimRay.direction;
			}
			skill1InputReceived = base.inputBank.skill1.down;
			skill2InputReceived = base.inputBank.skill2.down;
			skill3InputReceived = base.inputBank.skill3.down;
			skill4InputReceived = base.inputBank.skill4.down;
		}
		if (Object.op_Implicit((Object)(object)base.skillLocator))
		{
			if (skill1InputReceived && Object.op_Implicit((Object)(object)base.skillLocator.primary) && CanExecuteSkill(base.skillLocator.primary))
			{
				base.skillLocator.primary.ExecuteIfReady();
			}
			if (skill2InputReceived && Object.op_Implicit((Object)(object)base.skillLocator.secondary) && CanExecuteSkill(base.skillLocator.secondary))
			{
				base.skillLocator.secondary.ExecuteIfReady();
			}
			if (skill3InputReceived && Object.op_Implicit((Object)(object)base.skillLocator.utility) && CanExecuteSkill(base.skillLocator.utility))
			{
				base.skillLocator.utility.ExecuteIfReady();
			}
			if (skill4InputReceived && Object.op_Implicit((Object)(object)base.skillLocator.special) && CanExecuteSkill(base.skillLocator.special))
			{
				base.skillLocator.special.ExecuteIfReady();
			}
		}
	}
}
