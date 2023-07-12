using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace EntityStates.DroneWeaponsChainGun;

public abstract class BaseDroneWeaponChainGunState : EntityState
{
	private const string aimAnimatorChildName = "AimAnimator";

	[SerializeField]
	public float pitchRangeMin;

	[SerializeField]
	public float pitchRangeMax;

	[SerializeField]
	public float yawRangeMin;

	[SerializeField]
	public float yawRangeMax;

	protected NetworkedBodyAttachment networkedBodyAttachment;

	protected GameObject bodyGameObject;

	protected CharacterBody body;

	protected List<ChildLocator> gunChildLocators;

	protected List<Animator> gunAnimators;

	protected AimAnimator bodyAimAnimator;

	protected TeamComponent bodyTeamComponent;

	private bool linkedToDisplay;

	public override void OnEnter()
	{
		base.OnEnter();
		networkedBodyAttachment = GetComponent<NetworkedBodyAttachment>();
		if (Object.op_Implicit((Object)(object)networkedBodyAttachment))
		{
			bodyGameObject = networkedBodyAttachment.attachedBodyObject;
			body = networkedBodyAttachment.attachedBody;
			if (Object.op_Implicit((Object)(object)bodyGameObject) && Object.op_Implicit((Object)(object)body))
			{
				ModelLocator component = ((Component)body).GetComponent<ModelLocator>();
				if (Object.op_Implicit((Object)(object)component))
				{
					bodyAimAnimator = ((Component)component.modelTransform).GetComponent<AimAnimator>();
					bodyTeamComponent = ((Component)body).GetComponent<TeamComponent>();
				}
			}
		}
		LinkToDisplay();
	}

	private void LinkToDisplay()
	{
		if (linkedToDisplay)
		{
			return;
		}
		gunAnimators = new List<Animator>();
		gunChildLocators = new List<ChildLocator>();
		if (!Object.op_Implicit((Object)(object)networkedBodyAttachment))
		{
			return;
		}
		bodyGameObject = networkedBodyAttachment.attachedBodyObject;
		body = networkedBodyAttachment.attachedBody;
		if (!Object.op_Implicit((Object)(object)bodyGameObject) || !Object.op_Implicit((Object)(object)body))
		{
			return;
		}
		ModelLocator component = ((Component)body).GetComponent<ModelLocator>();
		if (!Object.op_Implicit((Object)(object)component) || !Object.op_Implicit((Object)(object)component.modelTransform))
		{
			return;
		}
		bodyAimAnimator = ((Component)component.modelTransform).GetComponent<AimAnimator>();
		bodyTeamComponent = ((Component)body).GetComponent<TeamComponent>();
		CharacterModel component2 = ((Component)component.modelTransform).GetComponent<CharacterModel>();
		if (!Object.op_Implicit((Object)(object)component2))
		{
			return;
		}
		List<GameObject> itemDisplayObjects = component2.GetItemDisplayObjects(DLC1Content.Items.DroneWeaponsDisplay1.itemIndex);
		itemDisplayObjects.AddRange(component2.GetItemDisplayObjects(DLC1Content.Items.DroneWeaponsDisplay2.itemIndex));
		foreach (GameObject item in itemDisplayObjects)
		{
			ChildLocator component3 = item.GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				gunChildLocators.Add(component3);
				Animator val = component3.FindChildComponent<Animator>("AimAnimator");
				if (Object.op_Implicit((Object)(object)val))
				{
					gunAnimators.Add(val);
				}
			}
		}
	}

	public void PassDisplayLinks(List<ChildLocator> gunChildLocators, List<Animator> gunAnimators)
	{
		if (!linkedToDisplay)
		{
			linkedToDisplay = true;
			this.gunAnimators = gunAnimators;
			this.gunChildLocators = gunChildLocators;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		LinkToDisplay();
		if (!Object.op_Implicit((Object)(object)bodyAimAnimator))
		{
			return;
		}
		foreach (Animator gunAnimator in gunAnimators)
		{
			bodyAimAnimator.UpdateAnimatorParameters(gunAnimator, pitchRangeMin, pitchRangeMax, yawRangeMin, yawRangeMax);
		}
	}

	protected Transform FindChild(string childName)
	{
		foreach (ChildLocator gunChildLocator in gunChildLocators)
		{
			Transform val = gunChildLocator.FindChild(childName);
			if (Object.op_Implicit((Object)(object)val))
			{
				return val;
			}
		}
		return null;
	}

	protected Ray GetAimRay()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)body.inputBank))
		{
			return new Ray(body.inputBank.aimOrigin, body.inputBank.aimDirection);
		}
		return new Ray(base.transform.position, base.transform.forward);
	}
}
