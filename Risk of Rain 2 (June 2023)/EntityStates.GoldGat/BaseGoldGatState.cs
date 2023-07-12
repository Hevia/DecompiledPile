using RoR2;
using UnityEngine;

namespace EntityStates.GoldGat;

public class BaseGoldGatState : EntityState
{
	protected NetworkedBodyAttachment networkedBodyAttachment;

	protected GameObject bodyGameObject;

	protected CharacterBody body;

	protected ChildLocator gunChildLocator;

	protected Animator gunAnimator;

	protected Transform gunTransform;

	protected CharacterMaster bodyMaster;

	protected EquipmentSlot bodyEquipmentSlot;

	protected InputBankTest bodyInputBank;

	protected AimAnimator bodyAimAnimator;

	public bool shouldFire;

	private bool linkedToDisplay;

	public override void OnEnter()
	{
		base.OnEnter();
		networkedBodyAttachment = GetComponent<NetworkedBodyAttachment>();
		if (!Object.op_Implicit((Object)(object)networkedBodyAttachment))
		{
			return;
		}
		bodyGameObject = networkedBodyAttachment.attachedBodyObject;
		body = networkedBodyAttachment.attachedBody;
		if (Object.op_Implicit((Object)(object)bodyGameObject))
		{
			bodyMaster = body.master;
			bodyInputBank = bodyGameObject.GetComponent<InputBankTest>();
			bodyEquipmentSlot = body.equipmentSlot;
			ModelLocator component = ((Component)body).GetComponent<ModelLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				bodyAimAnimator = ((Component)component.modelTransform).GetComponent<AimAnimator>();
			}
			LinkToDisplay();
		}
	}

	private void LinkToDisplay()
	{
		if (linkedToDisplay || !Object.op_Implicit((Object)(object)bodyEquipmentSlot))
		{
			return;
		}
		gunTransform = bodyEquipmentSlot.FindActiveEquipmentDisplay();
		if (Object.op_Implicit((Object)(object)gunTransform))
		{
			gunChildLocator = ((Component)gunTransform).GetComponentInChildren<ChildLocator>();
			if (Object.op_Implicit((Object)(object)gunChildLocator) && Object.op_Implicit((Object)(object)base.modelLocator))
			{
				base.modelLocator.modelTransform = ((Component)gunChildLocator).transform;
				gunAnimator = GetModelAnimator();
				linkedToDisplay = true;
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && Object.op_Implicit((Object)(object)bodyInputBank))
		{
			if (bodyInputBank.activateEquipment.justPressed)
			{
				shouldFire = !shouldFire;
			}
			if (body.inventory.GetItemCount(RoR2Content.Items.AutoCastEquipment) > 0)
			{
				shouldFire = true;
			}
		}
		LinkToDisplay();
		if (Object.op_Implicit((Object)(object)bodyAimAnimator) && Object.op_Implicit((Object)(object)gunAnimator))
		{
			bodyAimAnimator.UpdateAnimatorParameters(gunAnimator, -45f, 45f, 0f, 0f);
		}
	}

	protected bool CheckReturnToIdle()
	{
		if (!base.isAuthority)
		{
			return false;
		}
		if ((Object.op_Implicit((Object)(object)bodyMaster) && bodyMaster.money == 0) || !shouldFire)
		{
			outer.SetNextState(new GoldGatIdle
			{
				shouldFire = shouldFire
			});
			return true;
		}
		return false;
	}
}
