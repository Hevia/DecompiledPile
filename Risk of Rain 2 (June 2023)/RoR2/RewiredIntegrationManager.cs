using System.Collections.Generic;
using Rewired;
using UnityEngine;

namespace RoR2;

public static class RewiredIntegrationManager
{
	public static void Init()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/Rewired Input Manager"));
		ReInput.ControllerConnectedEvent += AssignNewController;
		ControllerType[] array = (ControllerType[])(object)new ControllerType[3]
		{
			default(ControllerType),
			(ControllerType)1,
			(ControllerType)2
		};
		foreach (ControllerType val in array)
		{
			Controller[] controllers = ReInput.controllers.GetControllers(val);
			if (controllers != null)
			{
				for (int j = 0; j < controllers.Length; j++)
				{
					AssignNewController(controllers[j]);
				}
			}
		}
	}

	private static void AssignJoystickToAvailablePlayer(Controller controller)
	{
		IList<Player> players = ReInput.players.Players;
		for (int i = 0; i < players.Count; i++)
		{
			Player val = players[i];
			if (val.name != "PlayerMain" && val.controllers.joystickCount == 0 && !val.controllers.hasKeyboard && !val.controllers.hasMouse)
			{
				val.controllers.AddController(controller, false);
				break;
			}
		}
	}

	private static void AssignNewController(ControllerStatusChangedEventArgs args)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		AssignNewController(ReInput.controllers.GetController(args.controllerType, args.controllerId));
	}

	private static void AssignNewController(Controller controller)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		ReInput.players.GetPlayer("PlayerMain").controllers.AddController(controller, false);
		if ((int)controller.type == 2)
		{
			AssignJoystickToAvailablePlayer(controller);
		}
	}
}
