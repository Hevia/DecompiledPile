using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Commando;

public class DodgeState : BaseState
{
	[SerializeField]
	public float duration = 0.9f;

	[SerializeField]
	public float initialSpeedCoefficient;

	[SerializeField]
	public float finalSpeedCoefficient;

	public static string dodgeSoundString;

	public static GameObject jetEffect;

	public static float dodgeFOV;

	public static int primaryReloadStockCount;

	private float rollSpeed;

	private Vector3 forwardDirection;

	private Animator animator;

	private Vector3 previousPosition;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(dodgeSoundString, base.gameObject);
		animator = GetModelAnimator();
		ChildLocator component = ((Component)animator).GetComponent<ChildLocator>();
		if (base.isAuthority)
		{
			if (Object.op_Implicit((Object)(object)base.inputBank) && Object.op_Implicit((Object)(object)base.characterDirection))
			{
				Vector3 val = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector);
				forwardDirection = ((Vector3)(ref val)).normalized;
			}
			if (Object.op_Implicit((Object)(object)base.skillLocator.primary))
			{
				base.skillLocator.primary.Reset();
				base.skillLocator.primary.stock = primaryReloadStockCount;
			}
		}
		Vector3 val2 = (Object.op_Implicit((Object)(object)base.characterDirection) ? base.characterDirection.forward : forwardDirection);
		Vector3 val3 = Vector3.Cross(Vector3.up, val2);
		float num = Vector3.Dot(forwardDirection, val2);
		float num2 = Vector3.Dot(forwardDirection, val3);
		animator.SetFloat("forwardSpeed", num, 0.1f, Time.fixedDeltaTime);
		animator.SetFloat("rightSpeed", num2, 0.1f, Time.fixedDeltaTime);
		if (Mathf.Abs(num) > Mathf.Abs(num2))
		{
			PlayAnimation("Body", (num > 0f) ? "DodgeForward" : "DodgeBackward", "Dodge.playbackRate", duration);
		}
		else
		{
			PlayAnimation("Body", (num2 > 0f) ? "DodgeRight" : "DodgeLeft", "Dodge.playbackRate", duration);
		}
		if (Object.op_Implicit((Object)(object)jetEffect))
		{
			Transform val4 = component.FindChild("LeftJet");
			Transform val5 = component.FindChild("RightJet");
			if (Object.op_Implicit((Object)(object)val4))
			{
				Object.Instantiate<GameObject>(jetEffect, val4);
			}
			if (Object.op_Implicit((Object)(object)val5))
			{
				Object.Instantiate<GameObject>(jetEffect, val5);
			}
		}
		RecalculateRollSpeed();
		if (Object.op_Implicit((Object)(object)base.characterMotor) && Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterMotor.velocity.y = 0f;
			base.characterMotor.velocity = forwardDirection * rollSpeed;
		}
		Vector3 val6 = (Object.op_Implicit((Object)(object)base.characterMotor) ? base.characterMotor.velocity : Vector3.zero);
		previousPosition = base.transform.position - val6;
	}

	private void RecalculateRollSpeed()
	{
		rollSpeed = moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, base.fixedAge / duration);
	}

	public override void FixedUpdate()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		RecalculateRollSpeed();
		if (Object.op_Implicit((Object)(object)base.cameraTargetParams))
		{
			base.cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, base.fixedAge / duration);
		}
		Vector3 val = base.transform.position - previousPosition;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		if (Object.op_Implicit((Object)(object)base.characterMotor) && Object.op_Implicit((Object)(object)base.characterDirection) && normalized != Vector3.zero)
		{
			Vector3 val2 = normalized * rollSpeed;
			float y = val2.y;
			val2.y = 0f;
			float num = Mathf.Max(Vector3.Dot(val2, forwardDirection), 0f);
			val2 = forwardDirection * num;
			val2.y += Mathf.Max(y, 0f);
			base.characterMotor.velocity = val2;
		}
		previousPosition = base.transform.position;
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)base.cameraTargetParams))
		{
			base.cameraTargetParams.fovOverride = -1f;
		}
		base.OnExit();
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		base.OnSerialize(writer);
		writer.Write(forwardDirection);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeserialize(reader);
		forwardDirection = reader.ReadVector3();
	}
}
