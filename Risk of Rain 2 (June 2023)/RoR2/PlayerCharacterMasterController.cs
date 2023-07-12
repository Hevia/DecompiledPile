using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rewired;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(CharacterMaster))]
[RequireComponent(typeof(PingerController))]
public class PlayerCharacterMasterController : NetworkBehaviour
{
	private static List<PlayerCharacterMasterController> _instances;

	private static ReadOnlyCollection<PlayerCharacterMasterController> _instancesReadOnly;

	private CharacterBody body;

	private InputBankTest bodyInputs;

	private CharacterMotor bodyMotor;

	[CompilerGenerated]
	private string _003CfinalMessageTokenServer_003Ek__BackingField;

	private PingerController pingerController;

	[SyncVar(hook = "OnSyncNetworkUserInstanceId")]
	private NetworkInstanceId networkUserInstanceId;

	private GameObject resolvedNetworkUserGameObjectInstance;

	private bool networkUserResolved;

	private NetworkUser resolvedNetworkUserInstance;

	private bool alreadyLinkedToNetworkUserOnce;

	public float cameraMinPitch = -70f;

	public float cameraMaxPitch = 70f;

	public GameObject crosshair;

	public Vector3 crosshairPosition;

	private NetworkIdentity netid;

	private static readonly float sprintMinAimMoveDot;

	private bool sprintInputPressReceived;

	private float lunarCoinChanceMultiplier = 0.5f;

	private static int kRpcRpcIncrementRunCount;

	public static ReadOnlyCollection<PlayerCharacterMasterController> instances => _instancesReadOnly;

	private bool bodyIsFlier
	{
		get
		{
			if (Object.op_Implicit((Object)(object)bodyMotor))
			{
				return bodyMotor.isFlying;
			}
			return true;
		}
	}

	public CharacterMaster master { get; private set; }

	private NetworkIdentity networkIdentity => master.networkIdentity;

	public string finalMessageTokenServer
	{
		[Server]
		[CompilerGenerated]
		get
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning((object)"[Server] function 'System.String RoR2.PlayerCharacterMasterController::get_finalMessageTokenServer()' called on client");
				return null;
			}
			return _003CfinalMessageTokenServer_003Ek__BackingField;
		}
		[CompilerGenerated]
		[Server]
		set
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PlayerCharacterMasterController::set_finalMessageTokenServer(System.String)' called on client");
			}
			else
			{
				_003CfinalMessageTokenServer_003Ek__BackingField = value;
			}
		}
	} = string.Empty;


	public bool hasEffectiveAuthority => master.hasEffectiveAuthority;

	public GameObject networkUserObject
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (!networkUserResolved)
			{
				resolvedNetworkUserGameObjectInstance = Util.FindNetworkObject(networkUserInstanceId);
				resolvedNetworkUserInstance = null;
				if (Object.op_Implicit((Object)(object)resolvedNetworkUserGameObjectInstance))
				{
					resolvedNetworkUserInstance = resolvedNetworkUserGameObjectInstance.GetComponent<NetworkUser>();
					networkUserResolved = true;
					SetBodyPrefabToPreference();
				}
			}
			return resolvedNetworkUserGameObjectInstance;
		}
		private set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			NetworkInstanceId networknetworkUserInstanceId = NetworkInstanceId.Invalid;
			resolvedNetworkUserGameObjectInstance = null;
			resolvedNetworkUserInstance = null;
			networkUserResolved = true;
			if (Object.op_Implicit((Object)(object)value))
			{
				NetworkIdentity component = value.GetComponent<NetworkIdentity>();
				if (Object.op_Implicit((Object)(object)component))
				{
					networknetworkUserInstanceId = component.netId;
					resolvedNetworkUserGameObjectInstance = value;
					resolvedNetworkUserInstance = value.GetComponent<NetworkUser>();
					SetBodyPrefabToPreference();
				}
			}
			NetworknetworkUserInstanceId = networknetworkUserInstanceId;
		}
	}

	public NetworkUser networkUser
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)networkUserObject))
			{
				return null;
			}
			return resolvedNetworkUserInstance;
		}
	}

	public bool isConnected => Object.op_Implicit((Object)(object)networkUserObject);

	public bool preventGameOver => master.preventGameOver;

	public NetworkInstanceId NetworknetworkUserInstanceId
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return networkUserInstanceId;
		}
		[param: In]
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncNetworkUserInstanceId(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<NetworkInstanceId>(value, ref networkUserInstanceId, 1u);
		}
	}

	public event Action onLinkedToNetworkUserServer;

	public static event Action<PlayerCharacterMasterController> onPlayerAdded;

	public static event Action<PlayerCharacterMasterController> onPlayerRemoved;

	public static event Action<PlayerCharacterMasterController> onLinkedToNetworkUserServerGlobal;

	private void OnSyncNetworkUserInstanceId(NetworkInstanceId value)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		resolvedNetworkUserGameObjectInstance = null;
		resolvedNetworkUserInstance = null;
		networkUserResolved = value == NetworkInstanceId.Invalid;
		NetworknetworkUserInstanceId = value;
	}

	[Server]
	public void LinkToNetworkUserServer(NetworkUser networkUser)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PlayerCharacterMasterController::LinkToNetworkUserServer(RoR2.NetworkUser)' called on client");
			return;
		}
		networkUser.masterObject = ((Component)this).gameObject;
		networkUserObject = ((Component)networkUser).gameObject;
		networkIdentity.AssignClientAuthority(((NetworkBehaviour)networkUser).connectionToClient);
		if (!alreadyLinkedToNetworkUserOnce)
		{
			networkUser.CopyLoadoutToMaster();
			alreadyLinkedToNetworkUserOnce = true;
		}
		else
		{
			networkUser.CopyLoadoutFromMaster();
		}
		SetBodyPrefabToPreference();
		this.onLinkedToNetworkUserServer?.Invoke();
		PlayerCharacterMasterController.onLinkedToNetworkUserServerGlobal?.Invoke(this);
	}

	private void Awake()
	{
		master = ((Component)this).GetComponent<CharacterMaster>();
		netid = ((Component)this).GetComponent<NetworkIdentity>();
		pingerController = ((Component)this).GetComponent<PingerController>();
	}

	private void OnEnable()
	{
		_instances.Add(this);
		if (PlayerCharacterMasterController.onPlayerAdded != null)
		{
			PlayerCharacterMasterController.onPlayerAdded(this);
		}
		NetworkUser.onNetworkUserBodyPreferenceChanged += OnNetworkUserBodyPreferenceChanged;
	}

	private void OnDisable()
	{
		_instances.Remove(this);
		if (PlayerCharacterMasterController.onPlayerRemoved != null)
		{
			PlayerCharacterMasterController.onPlayerRemoved(this);
		}
		NetworkUser.onNetworkUserBodyPreferenceChanged -= OnNetworkUserBodyPreferenceChanged;
	}

	private void Start()
	{
		if (NetworkServer.active && Object.op_Implicit((Object)(object)networkUser))
		{
			CallRpcIncrementRunCount();
		}
	}

	[ClientRpc]
	private void RpcIncrementRunCount()
	{
		if (Object.op_Implicit((Object)(object)networkUser))
		{
			LocalUser localUser = networkUser.localUser;
			if (localUser != null)
			{
				localUser.userProfile.totalRunCount++;
			}
		}
	}

	private static bool CanSendBodyInput(NetworkUser networkUser, out LocalUser localUser, out Player inputPlayer, out CameraRigController cameraRigController)
	{
		if (!Object.op_Implicit((Object)(object)networkUser))
		{
			localUser = null;
			inputPlayer = null;
			cameraRigController = null;
			return false;
		}
		localUser = networkUser.localUser;
		inputPlayer = networkUser.inputPlayer;
		cameraRigController = networkUser.cameraRigController;
		if (localUser == null || inputPlayer == null || !Object.op_Implicit((Object)(object)cameraRigController))
		{
			return false;
		}
		if (localUser.isUIFocused)
		{
			return false;
		}
		if (!cameraRigController.isControlAllowed)
		{
			return false;
		}
		return true;
	}

	private void Update()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		if (!hasEffectiveAuthority)
		{
			return;
		}
		SetBody(master.GetBodyObject());
		if (!Object.op_Implicit((Object)(object)bodyInputs))
		{
			return;
		}
		Vector3 moveVector = Vector3.zero;
		Vector3 aimDirection = bodyInputs.aimDirection;
		if (CanSendBodyInput(networkUser, out var _, out var inputPlayer, out var cameraRigController))
		{
			Transform transform = ((Component)cameraRigController).transform;
			sprintInputPressReceived |= inputPlayer.GetButtonDown(18);
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(inputPlayer.GetAxis(0), inputPlayer.GetAxis(1));
			float sqrMagnitude = ((Vector2)(ref val)).sqrMagnitude;
			if (sqrMagnitude > 1f)
			{
				val /= Mathf.Sqrt(sqrMagnitude);
			}
			if (bodyIsFlier)
			{
				moveVector = transform.right * val.x + transform.forward * val.y;
			}
			else
			{
				float y = transform.eulerAngles.y;
				moveVector = Quaternion.Euler(0f, y, 0f) * new Vector3(val.x, 0f, val.y);
			}
			Vector3 val2 = cameraRigController.crosshairWorldPosition - bodyInputs.aimOrigin;
			aimDirection = ((Vector3)(ref val2)).normalized;
		}
		bodyInputs.moveVector = moveVector;
		bodyInputs.aimDirection = aimDirection;
	}

	private void FixedUpdate()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (!hasEffectiveAuthority || !Object.op_Implicit((Object)(object)bodyInputs))
		{
			return;
		}
		bool newState = false;
		bool newState2 = false;
		bool newState3 = false;
		bool newState4 = false;
		bool newState5 = false;
		bool newState6 = false;
		bool newState7 = false;
		bool newState8 = false;
		bool newState9 = false;
		if (CanSendBodyInput(networkUser, out var _, out var inputPlayer, out var _))
		{
			bool flag = false;
			flag = body.isSprinting;
			if (sprintInputPressReceived)
			{
				sprintInputPressReceived = false;
				flag = !flag;
			}
			if (flag)
			{
				Vector3 aimDirection = bodyInputs.aimDirection;
				aimDirection.y = 0f;
				((Vector3)(ref aimDirection)).Normalize();
				Vector3 moveVector = bodyInputs.moveVector;
				moveVector.y = 0f;
				((Vector3)(ref moveVector)).Normalize();
				if ((body.bodyFlags & CharacterBody.BodyFlags.SprintAnyDirection) == 0 && Vector3.Dot(aimDirection, moveVector) < sprintMinAimMoveDot)
				{
					flag = false;
				}
			}
			newState = inputPlayer.GetButton(7);
			newState2 = inputPlayer.GetButton(8);
			newState3 = inputPlayer.GetButton(9);
			newState4 = inputPlayer.GetButton(10);
			newState5 = inputPlayer.GetButton(5);
			newState6 = inputPlayer.GetButton(4);
			newState7 = flag;
			newState8 = inputPlayer.GetButton(6);
			newState9 = inputPlayer.GetButton(28);
		}
		bodyInputs.skill1.PushState(newState);
		bodyInputs.skill2.PushState(newState2);
		bodyInputs.skill3.PushState(newState3);
		bodyInputs.skill4.PushState(newState4);
		bodyInputs.interact.PushState(newState5);
		bodyInputs.jump.PushState(newState6);
		bodyInputs.sprint.PushState(newState7);
		bodyInputs.activateEquipment.PushState(newState8);
		bodyInputs.ping.PushState(newState9);
		CheckPinging();
	}

	private void CheckPinging()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (hasEffectiveAuthority && Object.op_Implicit((Object)(object)body) && Object.op_Implicit((Object)(object)bodyInputs) && bodyInputs.ping.justPressed)
		{
			pingerController.AttemptPing(new Ray(bodyInputs.aimOrigin, bodyInputs.aimDirection), ((Component)body).gameObject);
		}
	}

	public string GetDisplayName()
	{
		string result = "";
		if (Object.op_Implicit((Object)(object)networkUserObject))
		{
			NetworkUser component = networkUserObject.GetComponent<NetworkUser>();
			if (Object.op_Implicit((Object)(object)component))
			{
				result = component.userName;
			}
		}
		return result;
	}

	private void SetBody(GameObject newBody)
	{
		if (Object.op_Implicit((Object)(object)newBody))
		{
			body = newBody.GetComponent<CharacterBody>();
		}
		else
		{
			body = null;
		}
		if (Object.op_Implicit((Object)(object)body))
		{
			bodyInputs = body.inputBank;
			bodyMotor = body.characterMotor;
		}
		else
		{
			bodyInputs = null;
			bodyMotor = null;
		}
	}

	[Server]
	public void OnBodyDeath()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PlayerCharacterMasterController::OnBodyDeath()' called on client");
		}
	}

	public void OnBodyStart()
	{
		if (NetworkServer.active)
		{
			finalMessageTokenServer = string.Empty;
		}
	}

	public static int GetPlayersWithBodiesCount()
	{
		int num = 0;
		foreach (PlayerCharacterMasterController instance in _instances)
		{
			if (instance.master.hasBody)
			{
				num++;
			}
		}
		return num;
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void Init()
	{
		GlobalEventManager.onCharacterDeathGlobal += delegate(DamageReport damageReport)
		{
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			CharacterMaster characterMaster = damageReport.attackerMaster;
			if (Object.op_Implicit((Object)(object)characterMaster))
			{
				if (Object.op_Implicit((Object)(object)characterMaster.minionOwnership.ownerMaster))
				{
					characterMaster = characterMaster.minionOwnership.ownerMaster;
				}
				PlayerCharacterMasterController component = ((Component)characterMaster).GetComponent<PlayerCharacterMasterController>();
				if (Object.op_Implicit((Object)(object)component) && Util.CheckRoll(1f * component.lunarCoinChanceMultiplier))
				{
					PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(RoR2Content.MiscPickups.LunarCoin.miscPickupIndex), ((Component)damageReport.victim).transform.position, Vector3.up * 10f);
					component.lunarCoinChanceMultiplier *= 0.5f;
				}
			}
		};
	}

	private void SetBodyPrefabToPreference()
	{
		master.bodyPrefab = BodyCatalog.GetBodyPrefab(networkUser.bodyIndexPreference);
		if (!Object.op_Implicit((Object)(object)master.bodyPrefab))
		{
			Debug.LogError((object)$"SetBodyPrefabToPreference failed to find a body prefab for index '{networkUser.bodyIndexPreference}'.  Reverting to backup: {master.backupBodyIndex}");
			master.bodyPrefab = BodyCatalog.GetBodyPrefab(master.backupBodyIndex);
			if (!Object.op_Implicit((Object)(object)master.bodyPrefab))
			{
				Debug.LogError((object)$"SetBodyPrefabToPreference backup ({master.backupBodyIndex}) failed.");
			}
		}
	}

	private void OnNetworkUserBodyPreferenceChanged(NetworkUser networkUser)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)this.networkUser))
		{
			NetworkInstanceId netId = ((NetworkBehaviour)networkUser).netId;
			uint value = ((NetworkInstanceId)(ref netId)).Value;
			netId = ((NetworkBehaviour)this.networkUser).netId;
			if (value == ((NetworkInstanceId)(ref netId)).Value)
			{
				SetBodyPrefabToPreference();
			}
		}
	}

	static PlayerCharacterMasterController()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		_instances = new List<PlayerCharacterMasterController>();
		_instancesReadOnly = new ReadOnlyCollection<PlayerCharacterMasterController>(_instances);
		sprintMinAimMoveDot = Mathf.Cos(MathF.PI / 3f);
		kRpcRpcIncrementRunCount = 1915650359;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerCharacterMasterController), kRpcRpcIncrementRunCount, new CmdDelegate(InvokeRpcRpcIncrementRunCount));
		NetworkCRC.RegisterBehaviour("PlayerCharacterMasterController", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcIncrementRunCount(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcIncrementRunCount called on server.");
		}
		else
		{
			((PlayerCharacterMasterController)(object)obj).RpcIncrementRunCount();
		}
	}

	public void CallRpcIncrementRunCount()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcIncrementRunCount called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcIncrementRunCount);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcIncrementRunCount");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (forceAll)
		{
			writer.Write(networkUserInstanceId);
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
			writer.Write(networkUserInstanceId);
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
			networkUserInstanceId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			OnSyncNetworkUserInstanceId(reader.ReadNetworkId());
		}
	}

	public override void PreStartClient()
	{
	}
}
