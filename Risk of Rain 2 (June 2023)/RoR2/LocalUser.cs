using System;
using Rewired;
using RoR2.Stats;
using RoR2.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RoR2;

public class LocalUser
{
	private Player _inputPlayer;

	private UserProfile _userProfile;

	public int id;

	private CameraRigController _cameraRigController;

	public Player inputPlayer
	{
		get
		{
			return _inputPlayer;
		}
		set
		{
			if (_inputPlayer != value)
			{
				if (_inputPlayer != null)
				{
					OnRewiredPlayerLost(_inputPlayer);
				}
				_inputPlayer = value;
				eventSystem = MPEventSystemManager.FindEventSystem(_inputPlayer);
				if (_inputPlayer != null)
				{
					OnRewiredPlayerDiscovered(_inputPlayer);
				}
			}
		}
	}

	public MPEventSystem eventSystem { get; private set; }

	public UserProfile userProfile
	{
		get
		{
			return _userProfile;
		}
		set
		{
			_userProfile = value;
			ApplyUserProfileBindingsToRewiredPlayer();
		}
	}

	public bool isUIFocused => Object.op_Implicit((Object)(object)((EventSystem)eventSystem).currentSelectedGameObject);

	public NetworkUser currentNetworkUser { get; private set; }

	public PlayerCharacterMasterController cachedMasterController { get; private set; }

	public CharacterMaster cachedMaster { get; private set; }

	public GameObject cachedMasterObject { get; private set; }

	public CharacterBody cachedBody { get; private set; }

	public GameObject cachedBodyObject { get; private set; }

	public PlayerStatsComponent cachedStatsComponent { get; private set; }

	public CameraRigController cameraRigController
	{
		get
		{
			return _cameraRigController;
		}
		set
		{
			if (_cameraRigController != value)
			{
				if (_cameraRigController != null)
				{
					this.onCameraLost?.Invoke(_cameraRigController);
				}
				_cameraRigController = value;
				if (_cameraRigController != null)
				{
					this.onCameraDiscovered?.Invoke(_cameraRigController);
				}
			}
		}
	}

	public event Action onBodyChanged;

	public event Action onMasterChanged;

	public event Action<CameraRigController> onCameraDiscovered;

	public event Action<CameraRigController> onCameraLost;

	public event Action<NetworkUser> onNetworkUserFound;

	public event Action<NetworkUser> onNetworkUserLost;

	static LocalUser()
	{
		ReInput.ControllerConnectedEvent += OnControllerConnected;
		ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
	}

	private static void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		foreach (LocalUser readOnlyLocalUsers in LocalUserManager.readOnlyLocalUsersList)
		{
			if (readOnlyLocalUsers.inputPlayer.controllers.ContainsController(args.controllerType, args.controllerId))
			{
				readOnlyLocalUsers.OnControllerDiscovered(ReInput.controllers.GetController(args.controllerType, args.controllerId));
			}
		}
	}

	private static void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		foreach (LocalUser readOnlyLocalUsers in LocalUserManager.readOnlyLocalUsersList)
		{
			if (readOnlyLocalUsers.inputPlayer.controllers.ContainsController(args.controllerType, args.controllerId))
			{
				readOnlyLocalUsers.OnControllerLost(ReInput.controllers.GetController(args.controllerType, args.controllerId));
			}
		}
	}

	private void OnRewiredPlayerDiscovered(Player player)
	{
		foreach (Controller controller in player.controllers.Controllers)
		{
			OnControllerDiscovered(controller);
		}
	}

	private void OnRewiredPlayerLost(Player player)
	{
		foreach (Controller controller in player.controllers.Controllers)
		{
			OnControllerLost(controller);
		}
	}

	private void OnControllerDiscovered(Controller controller)
	{
		ApplyUserProfileBindingstoRewiredController(controller);
	}

	private void OnControllerLost(Controller controller)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		inputPlayer.controllers.maps.ClearMapsForController(controller.type, controller.id, true);
	}

	private void ApplyUserProfileBindingstoRewiredController(Controller controller)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected I4, but got Unknown
		if (userProfile != null)
		{
			ControllerMap val = null;
			ControllerType type = controller.type;
			switch ((int)type)
			{
			case 0:
				val = (ControllerMap)(object)userProfile.keyboardMap;
				break;
			case 1:
				val = (ControllerMap)(object)userProfile.mouseMap;
				break;
			case 2:
				val = (ControllerMap)(object)userProfile.joystickMap;
				break;
			}
			if (val != null)
			{
				inputPlayer.controllers.maps.AddMap(controller, val);
			}
		}
	}

	public void ApplyUserProfileBindingsToRewiredPlayer()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (inputPlayer == null || userProfile == null)
		{
			return;
		}
		inputPlayer.controllers.maps.ClearAllMaps(false);
		foreach (Controller controller in inputPlayer.controllers.Controllers)
		{
			inputPlayer.controllers.maps.LoadMap(controller.type, controller.id, 2, 0);
			ApplyUserProfileBindingstoRewiredController(controller);
		}
		inputPlayer.controllers.maps.SetAllMapsEnabled(true);
	}

	public void RebuildControlChain()
	{
		PlayerCharacterMasterController playerCharacterMasterController = cachedMasterController;
		cachedMasterController = null;
		cachedMasterObject = null;
		cachedMaster = null;
		cachedStatsComponent = null;
		CharacterBody characterBody = cachedBody;
		cachedBody = null;
		cachedBodyObject = null;
		if (Object.op_Implicit((Object)(object)currentNetworkUser))
		{
			cachedMasterObject = currentNetworkUser.masterObject;
			if (Object.op_Implicit((Object)(object)cachedMasterObject))
			{
				cachedMasterController = cachedMasterObject.GetComponent<PlayerCharacterMasterController>();
			}
			if (Object.op_Implicit((Object)(object)cachedMasterController))
			{
				cachedMaster = cachedMasterController.master;
				if (Object.op_Implicit((Object)(object)cachedMaster))
				{
					cachedStatsComponent = cachedMaster.playerStatsComponent;
				}
				cachedBody = cachedMaster.GetBody();
				if (Object.op_Implicit((Object)(object)cachedBody))
				{
					cachedBodyObject = ((Component)cachedBody).gameObject;
				}
			}
		}
		if (characterBody != cachedBody)
		{
			this.onBodyChanged?.Invoke();
		}
		if (playerCharacterMasterController != cachedMasterController)
		{
			this.onMasterChanged?.Invoke();
		}
	}

	public void LinkNetworkUser(NetworkUser newNetworkUser)
	{
		if (!Object.op_Implicit((Object)(object)currentNetworkUser))
		{
			currentNetworkUser = newNetworkUser;
			newNetworkUser.localUser = this;
			this.onNetworkUserFound?.Invoke(newNetworkUser);
		}
	}

	public void UnlinkNetworkUser()
	{
		this.onNetworkUserLost?.Invoke(currentNetworkUser);
		currentNetworkUser.localUser = null;
		currentNetworkUser = null;
		cachedMasterController = null;
		cachedMasterObject = null;
		cachedBody = null;
		cachedBodyObject = null;
	}
}
