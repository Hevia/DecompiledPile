using KinematicCharacterController;
using RoR2;
using UnityEngine;

namespace EntityStates;

public class GenericCharacterMain : BaseCharacterMain
{
	private AimAnimator aimAnimator;

	protected bool jumpInputReceived;

	protected bool sprintInputReceived;

	private Vector3 moveVector = Vector3.zero;

	private Vector3 aimDirection = Vector3.forward;

	private int emoteRequest = -1;

	private bool hasAimAnimator;

	public override void OnEnter()
	{
		base.OnEnter();
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			aimAnimator = ((Component)modelTransform).GetComponent<AimAnimator>();
			if (Object.op_Implicit((Object)(object)aimAnimator))
			{
				((Behaviour)aimAnimator).enabled = true;
			}
		}
		hasAimAnimator = Object.op_Implicit((Object)(object)aimAnimator);
	}

	public override void OnExit()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			AimAnimator component = ((Component)modelTransform).GetComponent<AimAnimator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Behaviour)component).enabled = false;
			}
		}
		if (base.isAuthority)
		{
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				base.characterMotor.moveDirection = Vector3.zero;
			}
			if (Object.op_Implicit((Object)(object)base.railMotor))
			{
				base.railMotor.inputMoveVector = Vector3.zero;
			}
		}
		base.OnExit();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		GatherInputs();
		HandleMovements();
		PerformInputs();
	}

	public virtual void HandleMovements()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		if (useRootMotion)
		{
			if (hasCharacterMotor)
			{
				base.characterMotor.moveDirection = Vector3.zero;
			}
			if (hasRailMotor)
			{
				base.railMotor.inputMoveVector = moveVector;
			}
		}
		else
		{
			if (hasCharacterMotor)
			{
				base.characterMotor.moveDirection = moveVector;
			}
			if (hasRailMotor)
			{
				base.railMotor.inputMoveVector = moveVector;
			}
		}
		_ = base.isGrounded;
		if (!hasRailMotor && hasCharacterDirection && hasCharacterBody)
		{
			if (hasAimAnimator && aimAnimator.aimType == AimAnimator.AimType.Smart)
			{
				Vector3 val = ((moveVector == Vector3.zero) ? base.characterDirection.forward : moveVector);
				float num = Vector3.Angle(aimDirection, val);
				float num2 = Mathf.Max(aimAnimator.pitchRangeMax + aimAnimator.pitchGiveupRange, aimAnimator.yawRangeMax + aimAnimator.yawGiveupRange);
				base.characterDirection.moveVector = ((Object.op_Implicit((Object)(object)base.characterBody) && base.characterBody.shouldAim && num > num2) ? aimDirection : val);
			}
			else
			{
				base.characterDirection.moveVector = ((Object.op_Implicit((Object)(object)base.characterBody) && base.characterBody.shouldAim) ? aimDirection : moveVector);
			}
		}
		if (!base.isAuthority)
		{
			return;
		}
		ProcessJump();
		if (hasCharacterBody)
		{
			bool isSprinting = sprintInputReceived;
			if (((Vector3)(ref moveVector)).magnitude <= 0.5f)
			{
				isSprinting = false;
			}
			base.characterBody.isSprinting = isSprinting;
		}
	}

	public static void ApplyJumpVelocity(CharacterMotor characterMotor, CharacterBody characterBody, float horizontalBonus, float verticalBonus, bool vault = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = characterMotor.moveDirection;
		if (vault)
		{
			characterMotor.velocity = val;
		}
		else
		{
			val.y = 0f;
			float magnitude = ((Vector3)(ref val)).magnitude;
			if (magnitude > 0f)
			{
				val /= magnitude;
			}
			Vector3 velocity = val * characterBody.moveSpeed * horizontalBonus;
			velocity.y = characterBody.jumpPower * verticalBonus;
			characterMotor.velocity = velocity;
		}
		((BaseCharacterController)characterMotor).Motor.ForceUnground();
	}

	public virtual void ProcessJump()
	{
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		if (!hasCharacterMotor)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (!jumpInputReceived || !Object.op_Implicit((Object)(object)base.characterBody) || base.characterMotor.jumpCount >= base.characterBody.maxJumpCount)
		{
			return;
		}
		int itemCount = base.characterBody.inventory.GetItemCount(RoR2Content.Items.JumpBoost);
		float horizontalBonus = 1f;
		float verticalBonus = 1f;
		if (base.characterMotor.jumpCount >= base.characterBody.baseJumpCount)
		{
			flag = true;
			horizontalBonus = 1.5f;
			verticalBonus = 1.5f;
		}
		else if ((float)itemCount > 0f && base.characterBody.isSprinting)
		{
			float num = base.characterBody.acceleration * base.characterMotor.airControl;
			if (base.characterBody.moveSpeed > 0f && num > 0f)
			{
				flag2 = true;
				float num2 = Mathf.Sqrt(10f * (float)itemCount / num);
				float num3 = base.characterBody.moveSpeed / num;
				horizontalBonus = (num2 + num3) / num3;
			}
		}
		ApplyJumpVelocity(base.characterMotor, base.characterBody, horizontalBonus, verticalBonus);
		if (hasModelAnimator)
		{
			int layerIndex = base.modelAnimator.GetLayerIndex("Body");
			if (layerIndex >= 0)
			{
				if (base.characterMotor.jumpCount == 0 || base.characterBody.baseJumpCount == 1)
				{
					base.modelAnimator.CrossFadeInFixedTime("Jump", smoothingParameters.intoJumpTransitionTime, layerIndex);
				}
				else
				{
					base.modelAnimator.CrossFadeInFixedTime("BonusJump", smoothingParameters.intoJumpTransitionTime, layerIndex);
				}
			}
		}
		if (flag)
		{
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FeatherEffect"), new EffectData
			{
				origin = base.characterBody.footPosition
			}, transmit: true);
		}
		else if (base.characterMotor.jumpCount > 0)
		{
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CharacterLandImpact"), new EffectData
			{
				origin = base.characterBody.footPosition,
				scale = base.characterBody.radius
			}, transmit: true);
		}
		if (flag2)
		{
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BoostJumpEffect"), new EffectData
			{
				origin = base.characterBody.footPosition,
				rotation = Util.QuaternionSafeLookRotation(base.characterMotor.velocity)
			}, transmit: true);
		}
		base.characterMotor.jumpCount++;
	}

	protected virtual bool CanExecuteSkill(GenericSkill skillSlot)
	{
		return true;
	}

	protected void PerformInputs()
	{
		if (base.isAuthority)
		{
			if (hasSkillLocator)
			{
				HandleSkill(base.skillLocator.primary, ref base.inputBank.skill1);
				HandleSkill(base.skillLocator.secondary, ref base.inputBank.skill2);
				HandleSkill(base.skillLocator.utility, ref base.inputBank.skill3);
				HandleSkill(base.skillLocator.special, ref base.inputBank.skill4);
			}
			jumpInputReceived = false;
			sprintInputReceived = false;
		}
		void HandleSkill(GenericSkill skillSlot, ref InputBankTest.ButtonState buttonState)
		{
			if (buttonState.down && Object.op_Implicit((Object)(object)skillSlot) && (!skillSlot.mustKeyPress || !buttonState.hasPressBeenClaimed) && CanExecuteSkill(skillSlot) && skillSlot.ExecuteIfReady())
			{
				buttonState.hasPressBeenClaimed = true;
			}
		}
	}

	protected void GatherInputs()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (hasInputBank)
		{
			moveVector = base.inputBank.moveVector;
			aimDirection = base.inputBank.aimDirection;
			emoteRequest = base.inputBank.emoteRequest;
			base.inputBank.emoteRequest = -1;
			jumpInputReceived = base.inputBank.jump.justPressed;
			sprintInputReceived |= base.inputBank.sprint.down;
		}
	}
}
