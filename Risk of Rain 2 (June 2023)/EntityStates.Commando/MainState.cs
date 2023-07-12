using RoR2;
using UnityEngine;

namespace EntityStates.Commando;

public class MainState : BaseState
{
	private Animator modelAnimator;

	private GenericSkill skill1;

	private GenericSkill skill2;

	private GenericSkill skill3;

	private GenericSkill skill4;

	private bool skill1InputRecieved;

	private bool skill2InputRecieved;

	private bool skill3InputRecieved;

	private bool skill4InputRecieved;

	private Vector3 previousPosition;

	private Vector3 estimatedVelocity;

	public override void OnEnter()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		GenericSkill[] components = base.gameObject.GetComponents<GenericSkill>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i].skillName == "FirePistol")
			{
				skill1 = components[i];
			}
			else if (components[i].skillName == "FireFMJ")
			{
				skill2 = components[i];
			}
			else if (components[i].skillName == "Roll")
			{
				skill3 = components[i];
			}
			else if (components[i].skillName == "FireBarrage")
			{
				skill4 = components[i];
			}
		}
		modelAnimator = GetModelAnimator();
		previousPosition = base.transform.position;
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			int layerIndex = modelAnimator.GetLayerIndex("Body");
			modelAnimator.CrossFadeInFixedTime("Walk", 0.1f, layerIndex);
			modelAnimator.Update(0f);
		}
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			AimAnimator component = ((Component)modelTransform).GetComponent<AimAnimator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Behaviour)component).enabled = true;
			}
		}
	}

	public override void OnExit()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			AimAnimator component = ((Component)modelTransform).GetComponent<AimAnimator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Behaviour)component).enabled = false;
			}
		}
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.moveDirection = Vector3.zero;
		}
		base.OnExit();
	}

	public override void Update()
	{
		base.Update();
		if (base.inputBank.skill1.down)
		{
			skill1InputRecieved = true;
		}
		if (base.inputBank.skill2.down)
		{
			skill2InputRecieved = true;
		}
		if (base.inputBank.skill3.down)
		{
			skill3InputRecieved = true;
		}
		if (base.inputBank.skill4.down)
		{
			skill4InputRecieved = true;
		}
	}

	public override void FixedUpdate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		Vector3 position = base.transform.position;
		if (Time.fixedDeltaTime != 0f)
		{
			estimatedVelocity = (position - previousPosition) / Time.fixedDeltaTime;
		}
		if (base.isAuthority)
		{
			Vector3 moveVector = base.inputBank.moveVector;
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				base.characterMotor.moveDirection = moveVector;
				if (skill3InputRecieved)
				{
					if (Object.op_Implicit((Object)(object)skill3))
					{
						skill3.ExecuteIfReady();
					}
					skill3InputRecieved = false;
				}
			}
			if (Object.op_Implicit((Object)(object)base.characterDirection))
			{
				if (Object.op_Implicit((Object)(object)base.characterBody) && base.characterBody.shouldAim)
				{
					base.characterDirection.moveVector = base.inputBank.aimDirection;
				}
				else
				{
					base.characterDirection.moveVector = moveVector;
				}
			}
			if (skill1InputRecieved)
			{
				if (Object.op_Implicit((Object)(object)skill1))
				{
					skill1.ExecuteIfReady();
				}
				skill1InputRecieved = false;
			}
			if (skill2InputRecieved)
			{
				if (Object.op_Implicit((Object)(object)skill2))
				{
					skill2.ExecuteIfReady();
				}
				skill2InputRecieved = false;
			}
			if (skill4InputRecieved)
			{
				if (Object.op_Implicit((Object)(object)skill4))
				{
					skill4.ExecuteIfReady();
				}
				skill4InputRecieved = false;
			}
		}
		if (Object.op_Implicit((Object)(object)modelAnimator) && Object.op_Implicit((Object)(object)base.characterDirection))
		{
			Vector3 val = estimatedVelocity;
			val.y = 0f;
			Vector3 forward = base.characterDirection.forward;
			Vector3 val2 = Vector3.Cross(Vector3.up, forward);
			float magnitude = ((Vector3)(ref val)).magnitude;
			float num = Vector3.Dot(val, forward);
			float num2 = Vector3.Dot(val, val2);
			modelAnimator.SetBool("isMoving", magnitude != 0f);
			modelAnimator.SetFloat("walkSpeed", magnitude);
			modelAnimator.SetFloat("forwardSpeed", num, 0.2f, Time.fixedDeltaTime);
			modelAnimator.SetFloat("rightSpeed", num2, 0.2f, Time.fixedDeltaTime);
		}
		previousPosition = position;
	}
}
