using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2;
using RoR2.HudOverlay;
using UnityEngine;
using UnityEngine.UI;

namespace EntityStates.VoidRaidCrab;

public class ChannelGauntlet : BaseState
{
	[SerializeField]
	public float duration;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public GameObject overlayPrefab;

	[SerializeField]
	public string overlayChildLocatorEntryName;

	[SerializeField]
	public string fillImageChildLocatorEntryName;

	private List<OverlayController> overlayControllers;

	private HashSet<Image> overlayFillImages;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		Util.PlaySound(enterSoundString, base.gameObject);
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(TeamIndex.Player);
		overlayControllers = new List<OverlayController>();
		overlayFillImages = new HashSet<Image>();
		foreach (TeamComponent item in teamMembers)
		{
			OverlayController overlayController = HudOverlayManager.AddOverlay(((Component)item).gameObject, new OverlayCreationParams
			{
				prefab = overlayPrefab,
				childLocatorEntry = overlayChildLocatorEntryName
			});
			overlayController.onInstanceAdded += OnOverlayInstanceAdded;
			overlayController.onInstanceRemove += OnOverlayInstanceRemoved;
			overlayControllers.Add(overlayController);
		}
	}

	private void OnOverlayInstanceAdded(OverlayController controller, GameObject instance)
	{
		if (!Object.op_Implicit((Object)(object)instance))
		{
			return;
		}
		ChildLocator component = instance.GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Image val = component.FindChildComponent<Image>(fillImageChildLocatorEntryName);
			if ((Object)(object)val != (Object)null)
			{
				overlayFillImages.Add(val);
			}
		}
	}

	private void OnOverlayInstanceRemoved(OverlayController controller, GameObject instance)
	{
		if (!Object.op_Implicit((Object)(object)instance))
		{
			return;
		}
		ChildLocator component = instance.GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Image val = component.FindChildComponent<Image>(overlayChildLocatorEntryName);
			if ((Object)(object)val != (Object)null)
			{
				overlayFillImages.Remove(val);
			}
		}
	}

	public override void OnExit()
	{
		overlayFillImages.Clear();
		foreach (OverlayController overlayController in overlayControllers)
		{
			HudOverlayManager.RemoveOverlay(overlayController);
			overlayController.onInstanceAdded -= OnOverlayInstanceAdded;
			overlayController.onInstanceRemove -= OnOverlayInstanceRemoved;
		}
		overlayControllers.Clear();
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (duration > 0f)
		{
			float fillAmount = base.fixedAge / duration;
			foreach (Image overlayFillImage in overlayFillImages)
			{
				overlayFillImage.fillAmount = fillAmount;
			}
		}
		if (base.isAuthority && base.fixedAge >= duration)
		{
			Object.op_Implicit((Object)(object)VoidRaidGauntletController.instance);
			outer.SetNextState(new CloseGauntlet());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
