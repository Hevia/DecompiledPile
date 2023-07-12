using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HG;
using JetBrains.Annotations;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class NetworkUIPromptController : NetworkBehaviour
{
	private struct LocalUserInfo
	{
		public LocalUser localUser;

		public NetworkUIPromptController currentController;
	}

	private float lastCurrentLocalParticipantUpdateTime = float.NegativeInfinity;

	private LocalUser _currentLocalParticipant;

	private CharacterMaster _currentParticipantMaster;

	private bool _inControl;

	[SyncVar(hook = "SetParticipantMasterId")]
	private NetworkInstanceId masterObjectInstanceId;

	private CameraRigController _currentCamera;

	private static LocalUserInfo[] allLocalUserInfo = Array.Empty<LocalUserInfo>();

	private static int allLocalUserInfoCount = 0;

	public Action<NetworkReader> messageFromClientHandler;

	private LocalUser currentLocalParticipant
	{
		get
		{
			return _currentLocalParticipant;
		}
		set
		{
			if (_currentLocalParticipant != value)
			{
				if (_currentLocalParticipant != null)
				{
					OnLocalParticipantLost(_currentLocalParticipant);
				}
				_currentLocalParticipant = value;
				if (_currentLocalParticipant != null)
				{
					OnLocalParticipantDiscovered(_currentLocalParticipant);
				}
			}
		}
	}

	public CharacterMaster currentParticipantMaster
	{
		get
		{
			return _currentParticipantMaster;
		}
		private set
		{
			if (_currentParticipantMaster != value)
			{
				if (_currentParticipantMaster != null)
				{
					OnParticipantLost(_currentParticipantMaster);
				}
				_currentParticipantMaster = value;
				if (_currentParticipantMaster != null)
				{
					OnParticipantDiscovered(_currentParticipantMaster);
				}
			}
		}
	}

	private bool inControl
	{
		get
		{
			return _inControl;
		}
		set
		{
			if (_inControl != value)
			{
				_inControl = value;
				if (_inControl)
				{
					OnControlBegin();
				}
				else
				{
					OnControlEnd();
				}
			}
		}
	}

	private CameraRigController currentCamera
	{
		get
		{
			return _currentCamera;
		}
		set
		{
			if (_currentCamera != value)
			{
				if (_currentCamera != null)
				{
					this.onDisplayEnd?.Invoke(this, currentLocalParticipant, _currentCamera);
				}
				_currentCamera = value;
				if (_currentCamera != null)
				{
					this.onDisplayBegin?.Invoke(this, currentLocalParticipant, _currentCamera);
				}
			}
		}
	}

	public bool inUse => Object.op_Implicit((Object)(object)currentParticipantMaster);

	public bool isDisplaying => Object.op_Implicit((Object)(object)currentCamera);

	public NetworkInstanceId NetworkmasterObjectInstanceId
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return masterObjectInstanceId;
		}
		[param: In]
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SetParticipantMasterId(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<NetworkInstanceId>(value, ref masterObjectInstanceId, 1u);
		}
	}

	public event Action<NetworkUIPromptController, LocalUser, CameraRigController> onDisplayBegin;

	public event Action<NetworkUIPromptController, LocalUser, CameraRigController> onDisplayEnd;

	private void OnParticipantDiscovered([NotNull] CharacterMaster master)
	{
		LocalUser localUser = null;
		if (Object.op_Implicit((Object)(object)master.playerCharacterMasterController) && Object.op_Implicit((Object)(object)master.playerCharacterMasterController.networkUser))
		{
			localUser = master.playerCharacterMasterController.networkUser.localUser;
		}
		currentLocalParticipant = localUser;
	}

	private void OnParticipantLost([NotNull] CharacterMaster master)
	{
		currentLocalParticipant = null;
	}

	private void OnLocalParticipantDiscovered([NotNull] LocalUser localUser)
	{
		lastCurrentLocalParticipantUpdateTime = Time.unscaledTime;
		UpdateBestControllerForLocalUser(localUser);
	}

	private void OnLocalParticipantLost([NotNull] LocalUser localUser)
	{
		ref LocalUserInfo localUserInfo = ref GetLocalUserInfo(localUser);
		if ((Object)(object)localUserInfo.currentController == (Object)(object)this)
		{
			localUserInfo.currentController.inControl = false;
			localUserInfo.currentController = null;
		}
	}

	private void HandleCameraDiscovered(CameraRigController cameraRigController)
	{
		currentCamera = cameraRigController;
	}

	private void HandleCameraLost(CameraRigController cameraRigController)
	{
		currentCamera = null;
	}

	private void OnControlBegin()
	{
		currentCamera = currentLocalParticipant.cameraRigController;
		currentLocalParticipant.onCameraDiscovered += HandleCameraDiscovered;
		currentLocalParticipant.onCameraLost += HandleCameraLost;
	}

	private void OnControlEnd()
	{
		currentLocalParticipant.onCameraLost -= HandleCameraLost;
		currentLocalParticipant.onCameraDiscovered -= HandleCameraDiscovered;
		currentCamera = null;
	}

	[CanBeNull]
	private static NetworkUIPromptController FindBestControllerForLocalUser([NotNull] LocalUser localUser)
	{
		NetworkUIPromptController result = null;
		float num = float.PositiveInfinity;
		List<NetworkUIPromptController> instancesList = InstanceTracker.GetInstancesList<NetworkUIPromptController>();
		for (int i = 0; i < instancesList.Count; i++)
		{
			NetworkUIPromptController networkUIPromptController = instancesList[i];
			if (networkUIPromptController.currentLocalParticipant == localUser && networkUIPromptController.lastCurrentLocalParticipantUpdateTime < num)
			{
				num = networkUIPromptController.lastCurrentLocalParticipantUpdateTime;
				result = networkUIPromptController;
			}
		}
		return result;
	}

	private static void UpdateBestControllerForLocalUser([NotNull] LocalUser localUser)
	{
		ref LocalUserInfo localUserInfo = ref GetLocalUserInfo(localUser);
		NetworkUIPromptController currentController = localUserInfo.currentController;
		NetworkUIPromptController networkUIPromptController = FindBestControllerForLocalUser(localUser);
		if (currentController != networkUIPromptController)
		{
			if (currentController != null)
			{
				currentController.inControl = false;
			}
			if (networkUIPromptController != null)
			{
				networkUIPromptController.inControl = true;
			}
			localUserInfo.currentController = networkUIPromptController;
		}
	}

	private void OnEnable()
	{
		InstanceTracker.Add<NetworkUIPromptController>(this);
	}

	private void OnDisable()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SetParticipantMasterId(NetworkInstanceId.Invalid);
		InstanceTracker.Remove<NetworkUIPromptController>(this);
	}

	public override void OnStartClient()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		((NetworkBehaviour)this).OnStartClient();
		if (!NetworkServer.active)
		{
			SetParticipantMasterId(masterObjectInstanceId);
		}
	}

	private void SetParticipantMasterId(NetworkInstanceId newMasterObjectInstanceId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		NetworkmasterObjectInstanceId = newMasterObjectInstanceId;
		GameObject val = Util.FindNetworkObject(masterObjectInstanceId);
		CharacterMaster characterMaster = null;
		if (Object.op_Implicit((Object)(object)val))
		{
			characterMaster = val.GetComponent<CharacterMaster>();
		}
		currentParticipantMaster = characterMaster;
	}

	[Server]
	public void ClearParticipant()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkUIPromptController::ClearParticipant()' called on client");
		}
		else
		{
			SetParticipantMaster(null);
		}
	}

	[Server]
	public void SetParticipantMaster([CanBeNull] CharacterMaster newParticipantMaster)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkUIPromptController::SetParticipantMaster(RoR2.CharacterMaster)' called on client");
			return;
		}
		NetworkIdentity val = (Object.op_Implicit((Object)(object)newParticipantMaster) ? newParticipantMaster.networkIdentity : null);
		SetParticipantMasterId(Object.op_Implicit((Object)(object)val) ? val.netId : NetworkInstanceId.Invalid);
	}

	[Server]
	public void SetParticipantMasterFromInteractor([CanBeNull] Interactor newParticipantInteractor)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkUIPromptController::SetParticipantMasterFromInteractor(RoR2.Interactor)' called on client");
			return;
		}
		CharacterMaster participantMaster = ((!Object.op_Implicit((Object)(object)newParticipantInteractor)) ? null : ((Component)newParticipantInteractor).GetComponent<CharacterBody>()?.master);
		SetParticipantMaster(participantMaster);
	}

	[Server]
	public void SetParticipantMasterFromInteractorObject([CanBeNull] Object newParticipantInteractor)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkUIPromptController::SetParticipantMasterFromInteractorObject(UnityEngine.Object)' called on client");
		}
		else
		{
			SetParticipantMasterFromInteractor(newParticipantInteractor as Interactor);
		}
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		LocalUserManager.onUserSignIn += OnUserSignIn;
		LocalUserManager.onUserSignOut += OnUserSignOut;
	}

	private static void OnUserSignIn(LocalUser localUser)
	{
		LocalUserInfo localUserInfo = default(LocalUserInfo);
		localUserInfo.localUser = localUser;
		localUserInfo.currentController = null;
		LocalUserInfo localUserInfo2 = localUserInfo;
		ArrayUtils.ArrayAppend<LocalUserInfo>(ref allLocalUserInfo, ref allLocalUserInfoCount, ref localUserInfo2);
	}

	private static void OnUserSignOut(LocalUser localUser)
	{
		for (int i = 0; i < allLocalUserInfoCount; i++)
		{
			if (allLocalUserInfo[i].localUser == localUser)
			{
				ArrayUtils.ArrayRemoveAt<LocalUserInfo>(allLocalUserInfo, ref allLocalUserInfoCount, i, 1);
				break;
			}
		}
	}

	private static ref LocalUserInfo GetLocalUserInfo(LocalUser localUser)
	{
		for (int i = 0; i < allLocalUserInfoCount; i++)
		{
			if (allLocalUserInfo[i].localUser == localUser)
			{
				return ref allLocalUserInfo[i];
			}
		}
		throw new ArgumentException("localUser must be signed in");
	}

	[Client]
	public NetworkWriter BeginMessageToServer()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'UnityEngine.Networking.NetworkWriter RoR2.NetworkUIPromptController::BeginMessageToServer()' called on server");
			return default(NetworkWriter);
		}
		NetworkWriter val = new NetworkWriter();
		val.StartMessage((short)76);
		val.Write(((Component)this).gameObject);
		return val;
	}

	[Client]
	public void FinishMessageToServer(NetworkWriter writer)
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.NetworkUIPromptController::FinishMessageToServer(UnityEngine.Networking.NetworkWriter)' called on server");
			return;
		}
		writer.FinishMessage();
		NetworkUser networkUser = FindParticipantNetworkUser(this);
		if (Object.op_Implicit((Object)(object)networkUser))
		{
			((NetworkBehaviour)networkUser).connectionToServer.SendWriter(writer, ((NetworkBehaviour)this).GetNetworkChannel());
		}
	}

	private static NetworkUser FindParticipantNetworkUser(NetworkUIPromptController instance)
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			CharacterMaster characterMaster = instance.currentParticipantMaster;
			if (Object.op_Implicit((Object)(object)characterMaster))
			{
				PlayerCharacterMasterController playerCharacterMasterController = characterMaster.playerCharacterMasterController;
				if (Object.op_Implicit((Object)(object)playerCharacterMasterController))
				{
					return playerCharacterMasterController.networkUser;
				}
			}
		}
		return null;
	}

	[NetworkMessageHandler(client = false, server = true, msgType = 76)]
	private static void HandleNetworkUIPromptMessage(NetworkMessage netMsg)
	{
		GameObject val = netMsg.reader.ReadGameObject();
		if (!Object.op_Implicit((Object)(object)val))
		{
			return;
		}
		NetworkUIPromptController component = val.GetComponent<NetworkUIPromptController>();
		if (Object.op_Implicit((Object)(object)component))
		{
			NetworkUser networkUser = FindParticipantNetworkUser(component);
			NetworkConnection val2 = (Object.op_Implicit((Object)(object)networkUser) ? ((NetworkBehaviour)networkUser).connectionToClient : null);
			if (netMsg.conn == val2)
			{
				component.messageFromClientHandler?.Invoke(netMsg.reader);
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (forceAll)
		{
			writer.Write(masterObjectInstanceId);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(masterObjectInstanceId);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			masterObjectInstanceId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetParticipantMasterId(reader.ReadNetworkId());
		}
	}

	public override void PreStartClient()
	{
	}
}
