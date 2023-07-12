using RoR2;
using UnityEngine;

namespace EntityStates.SurvivorPod.BatteryPanel;

public class BaseBatteryPanelState : BaseState
{
	protected struct PodInfo
	{
		public GameObject podObject;

		public Animator podAnimator;
	}

	protected PodInfo podInfo;

	public override void OnEnter()
	{
		base.OnEnter();
		VehicleSeat componentInParent = base.gameObject.GetComponentInParent<VehicleSeat>();
		SetPodObject((componentInParent != null) ? ((Component)componentInParent).gameObject : null);
	}

	private void SetPodObject(GameObject podObject)
	{
		podInfo = default(PodInfo);
		if (!Object.op_Implicit((Object)(object)podObject))
		{
			return;
		}
		podInfo.podObject = podObject;
		ModelLocator component = podObject.GetComponent<ModelLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform modelTransform = component.modelTransform;
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				podInfo.podAnimator = ((Component)modelTransform).GetComponent<Animator>();
			}
		}
	}

	protected void PlayPodAnimation(string layerName, string animationStateName, string playbackRateParam, float duration)
	{
		if (Object.op_Implicit((Object)(object)podInfo.podAnimator))
		{
			EntityState.PlayAnimationOnAnimator(podInfo.podAnimator, layerName, animationStateName, playbackRateParam, duration);
		}
	}

	protected void PlayPodAnimation(string layerName, string animationStateName)
	{
		if (Object.op_Implicit((Object)(object)podInfo.podAnimator))
		{
			EntityState.PlayAnimationOnAnimator(podInfo.podAnimator, layerName, animationStateName);
		}
	}

	protected void EnablePickup()
	{
		ChildLocator component = ((Component)podInfo.podAnimator).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild("BatteryAttachmentPoint");
			if (Object.op_Implicit((Object)(object)val))
			{
				Transform val2 = val.Find("QuestVolatileBatteryWorldPickup(Clone)");
				if (Object.op_Implicit((Object)(object)val2))
				{
					GenericPickupController component2 = ((Component)val2).GetComponent<GenericPickupController>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						((Behaviour)component2).enabled = true;
					}
					else
					{
						Debug.Log((object)"Could not find pickup controller.");
					}
				}
				else
				{
					Debug.Log((object)"Could not find battery transform");
				}
			}
			else
			{
				Debug.Log((object)"Could not find battery attachment point.");
			}
		}
		else
		{
			Debug.Log((object)"Could not find pod child locator.");
		}
	}
}
