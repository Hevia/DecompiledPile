using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Interactables.StoneGate;

public class Opening : BaseState
{
	public static string leftGateChildLocatorEntry;

	public static string rightGateChildLocatorEntry;

	public static AnimationCurve doorPositionCurve;

	public static float duration;

	public static string doorBeginOpenEffectChildLocatorEntry;

	public static string doorBeginOpenSoundString;

	public static string doorFinishedOpenEffectChildLocatorEntry;

	public static string doorFinishedOpenSoundString;

	private ChildLocator childLocator;

	private bool doorIsOpen;

	private Transform leftGateTransform;

	private Transform rightGateTransform;

	public override void OnEnter()
	{
		base.OnEnter();
		childLocator = GetComponent<ChildLocator>();
		((Component)childLocator.FindChild(doorBeginOpenEffectChildLocatorEntry)).gameObject.SetActive(true);
		Util.PlaySound(doorBeginOpenSoundString, base.gameObject);
		if (NetworkServer.active)
		{
			Chat.SendBroadcastChat(new Chat.SimpleChatMessage
			{
				baseToken = "STONEGATE_OPEN"
			});
		}
	}

	public override void Update()
	{
		base.Update();
		UpdateGateTransform(ref leftGateTransform, leftGateChildLocatorEntry);
		UpdateGateTransform(ref rightGateTransform, rightGateChildLocatorEntry);
	}

	private void UpdateGateTransform(ref Transform gateTransform, string childLocatorString)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)gateTransform))
		{
			gateTransform = childLocator.FindChild(childLocatorString);
			return;
		}
		Vector3 localPosition = gateTransform.localPosition;
		gateTransform.localPosition = new Vector3(localPosition.x, localPosition.y, doorPositionCurve.Evaluate(base.age / duration));
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && !doorIsOpen)
		{
			doorIsOpen = true;
			Util.PlaySound(doorFinishedOpenSoundString, base.gameObject);
			((Component)childLocator.FindChild(doorBeginOpenEffectChildLocatorEntry)).gameObject.SetActive(false);
			((Component)childLocator.FindChild(doorFinishedOpenEffectChildLocatorEntry)).gameObject.SetActive(true);
		}
	}
}
