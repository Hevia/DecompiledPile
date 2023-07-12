using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HG;
using HG.Collections.Generic;
using Rewired;
using RoR2.UI;
using UnityEngine;

namespace RoR2.GamepadVibration;

public static class GamepadVibrationManager
{
	private static readonly AssociationList<Joystick, GamepadVibrationController> joystickToVibrationController = new AssociationList<Joystick, GamepadVibrationController>(4, (IEqualityComparer<Joystick>)null, false);

	private static int[] joystickIndexToLocalUserIndex = new int[4];

	[SystemInitializer(new Type[] { typeof(GamepadVibrationController) })]
	private static void Init()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		RoR2Application.onUpdate += Update;
		ReInput.ControllerConnectedEvent += OnControllerConnected;
		ReInput.ControllerPreDisconnectEvent += OnControllerPreDisconnect;
		RoR2Application.onShutDown = (Action)Delegate.Combine(RoR2Application.onShutDown, new Action(OnApplicationShutDown));
		foreach (Controller controller in ReInput.controllers.Controllers)
		{
			OnControllerConnected(new ControllerStatusChangedEventArgs(controller.name, controller.id, controller.type));
		}
	}

	private static void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		Controller controller = args.controller;
		Joystick val = (Joystick)(object)((controller is Joystick) ? controller : null);
		if (val != null)
		{
			joystickToVibrationController.Add(val, GamepadVibrationController.Create(val));
		}
	}

	private static void OnControllerPreDisconnect(ControllerStatusChangedEventArgs args)
	{
		Controller controller = args.controller;
		Joystick val = (Joystick)(object)((controller is Joystick) ? controller : null);
		if (val != null)
		{
			int num = joystickToVibrationController.FindKeyIndex(val);
			if (num >= 0)
			{
				joystickToVibrationController[num].Value.Dispose();
				joystickToVibrationController.RemoveAt(num);
			}
		}
	}

	private static void OnApplicationShutDown()
	{
		for (int num = joystickToVibrationController.Count - 1; num >= 0; num--)
		{
			joystickToVibrationController[num].Value.Dispose();
			joystickToVibrationController.RemoveAt(num);
		}
	}

	private static void Update()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (Time.deltaTime <= 0f)
		{
			Enumerator<Joystick, GamepadVibrationController> enumerator = joystickToVibrationController.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					enumerator.Current.Value.StopVibration();
				}
				return;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}
		ReadOnlyCollection<LocalUser> readOnlyLocalUsersList = LocalUserManager.readOnlyLocalUsersList;
		ArrayUtils.EnsureCapacity<int>(ref joystickIndexToLocalUserIndex, joystickToVibrationController.Count);
		int[] array = joystickIndexToLocalUserIndex;
		int num = -1;
		ArrayUtils.SetRange<int>(array, ref num, 0, joystickToVibrationController.Count);
		for (int i = 0; i < readOnlyLocalUsersList.Count; i++)
		{
			LocalUser localUser = readOnlyLocalUsersList[i];
			if (localUser.inputPlayer == null || !Object.op_Implicit((Object)(object)localUser.eventSystem) || localUser.eventSystem.currentInputSource != MPEventSystem.InputSource.Gamepad)
			{
				continue;
			}
			Player inputPlayer = localUser.inputPlayer;
			Controller obj = ((inputPlayer != null) ? inputPlayer.controllers.GetLastActiveController<Joystick>() : null);
			Joystick val = (Joystick)(object)((obj is Joystick) ? obj : null);
			if (val != null)
			{
				int num2 = joystickToVibrationController.FindKeyIndex(val);
				if (num2 >= 0)
				{
					joystickIndexToLocalUserIndex[num2] = i;
				}
			}
		}
		for (int j = 0; j < joystickToVibrationController.Count; j++)
		{
			GamepadVibrationController value = joystickToVibrationController[j].Value;
			int num3 = joystickIndexToLocalUserIndex[j];
			LocalUser localUser2 = ((num3 >= 0) ? readOnlyLocalUsersList[num3] : null);
			if (localUser2 != null && localUser2.userProfile != null)
			{
				VibrationContext vibrationContext = default(VibrationContext);
				vibrationContext.localUser = localUser2;
				vibrationContext.cameraRigController = localUser2.cameraRigController;
				vibrationContext.userVibrationScale = localUser2.userProfile.gamepadVibrationScale;
				VibrationContext vibrationContext2 = vibrationContext;
				value.ApplyVibration(in vibrationContext2);
			}
			else
			{
				value.StopVibration();
			}
		}
	}
}
