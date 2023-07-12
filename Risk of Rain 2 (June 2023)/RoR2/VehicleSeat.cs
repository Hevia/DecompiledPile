using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using EntityStates;
using KinematicCharacterController;
using RoR2.ConVar;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace RoR2;

[DisallowMultipleComponent]
public class VehicleSeat : NetworkBehaviour, IInteractable
{
	private struct PassengerInfo
	{
		private static readonly List<EntityStateMachine> sharedBuffer = new List<EntityStateMachine>();

		public readonly Transform transform;

		public readonly CharacterBody body;

		public readonly InputBankTest inputBank;

		public readonly InteractionDriver interactionDriver;

		public readonly CharacterMotor characterMotor;

		public readonly EntityStateMachine bodyStateMachine;

		public readonly CharacterModel characterModel;

		public readonly NetworkIdentity networkIdentity;

		public readonly Collider collider;

		public bool hasEffectiveAuthority => Util.HasEffectiveAuthority(networkIdentity);

		public PassengerInfo(GameObject passengerBodyObject)
		{
			transform = passengerBodyObject.transform;
			body = passengerBodyObject.GetComponent<CharacterBody>();
			inputBank = passengerBodyObject.GetComponent<InputBankTest>();
			interactionDriver = passengerBodyObject.GetComponent<InteractionDriver>();
			characterMotor = passengerBodyObject.GetComponent<CharacterMotor>();
			networkIdentity = passengerBodyObject.GetComponent<NetworkIdentity>();
			collider = passengerBodyObject.GetComponent<Collider>();
			bodyStateMachine = null;
			passengerBodyObject.GetComponents<EntityStateMachine>(sharedBuffer);
			for (int i = 0; i < sharedBuffer.Count; i++)
			{
				EntityStateMachine entityStateMachine = sharedBuffer[i];
				if (string.CompareOrdinal(entityStateMachine.customName, "Body") == 0)
				{
					bodyStateMachine = entityStateMachine;
					break;
				}
			}
			sharedBuffer.Clear();
			characterModel = null;
			if (Object.op_Implicit((Object)(object)body.modelLocator) && Object.op_Implicit((Object)(object)body.modelLocator.modelTransform))
			{
				characterModel = ((Component)body.modelLocator.modelTransform).GetComponent<CharacterModel>();
			}
		}

		public string GetString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("transform=").Append(Object.op_Implicit((Object)(object)transform)).AppendLine();
			stringBuilder.Append("body=").Append(Object.op_Implicit((Object)(object)body)).AppendLine();
			stringBuilder.Append("inputBank=").Append(Object.op_Implicit((Object)(object)inputBank)).AppendLine();
			stringBuilder.Append("interactionDriver=").Append(Object.op_Implicit((Object)(object)interactionDriver)).AppendLine();
			stringBuilder.Append("characterMotor=").Append(Object.op_Implicit((Object)(object)characterMotor)).AppendLine();
			stringBuilder.Append("bodyStateMachine=").Append(Object.op_Implicit((Object)(object)bodyStateMachine)).AppendLine();
			stringBuilder.Append("characterModel=").Append(Object.op_Implicit((Object)(object)characterModel)).AppendLine();
			stringBuilder.Append("networkIdentity=").Append(Object.op_Implicit((Object)(object)networkIdentity)).AppendLine();
			stringBuilder.Append("hasEffectiveAuthority=").Append(hasEffectiveAuthority).AppendLine();
			return stringBuilder.ToString();
		}
	}

	public delegate void InteractabilityCheckDelegate(CharacterBody activator, ref Interactability? resultOverride);

	public SerializableEntityStateType passengerState;

	public Transform seatPosition;

	public Transform exitPosition;

	public bool ejectOnCollision;

	public bool hidePassenger = true;

	public float exitVelocityFraction = 1f;

	public UnityEvent onPassengerEnterUnityEvent;

	[FormerlySerializedAs("OnPassengerExitUnityEvent")]
	public UnityEvent onPassengerExitUnityEvent;

	public string enterVehicleContextString;

	public string exitVehicleContextString;

	public bool disablePassengerMotor;

	public bool isEquipmentActivationAllowed;

	[SyncVar(hook = "SetPassenger")]
	private GameObject passengerBodyObject;

	private PassengerInfo passengerInfo;

	private Rigidbody rigidbody;

	private Collider collider;

	public CallbackCheck<Interactability, CharacterBody> enterVehicleAllowedCheck = new CallbackCheck<Interactability, CharacterBody>();

	public CallbackCheck<Interactability, CharacterBody> exitVehicleAllowedCheck = new CallbackCheck<Interactability, CharacterBody>();

	public CallbackCheck<bool, GameObject> handleVehicleEnterRequestServer = new CallbackCheck<bool, GameObject>();

	public CallbackCheck<bool, GameObject> handleVehicleExitRequestServer = new CallbackCheck<bool, GameObject>();

	private static readonly BoolConVar cvVehicleSeatDebug = new BoolConVar("vehicle_seat_debug", ConVarFlags.None, "0", "Enables debug logging for VehicleSeat.");

	private Run.FixedTimeStamp passengerAssignmentTime = Run.FixedTimeStamp.negativeInfinity;

	private readonly float passengerAssignmentCooldown = 0.2f;

	private NetworkInstanceId ___passengerBodyObjectNetId;

	public CharacterBody currentPassengerBody => passengerInfo.body;

	public InputBankTest currentPassengerInputBank => passengerInfo.inputBank;

	private static bool shouldLog => cvVehicleSeatDebug.value;

	public bool hasPassenger => Object.op_Implicit((Object)(object)passengerBodyObject);

	public GameObject NetworkpassengerBodyObject
	{
		get
		{
			return passengerBodyObject;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SetPassenger(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref passengerBodyObject, 1u, ref ___passengerBodyObjectNetId);
		}
	}

	public event Action<GameObject> onPassengerEnter;

	public event Action<GameObject> onPassengerExit;

	public static event Action<VehicleSeat, GameObject> onPassengerEnterGlobal;

	public static event Action<VehicleSeat, GameObject> onPassengerExitGlobal;

	public string GetContextString(Interactor activator)
	{
		if (!Object.op_Implicit((Object)(object)passengerBodyObject))
		{
			return Language.GetString(enterVehicleContextString);
		}
		if (passengerBodyObject == ((Component)activator).gameObject)
		{
			return Language.GetString(exitVehicleContextString);
		}
		return null;
	}

	public Interactability GetInteractability(Interactor activator)
	{
		CharacterBody component = ((Component)activator).GetComponent<CharacterBody>();
		if (!Object.op_Implicit((Object)(object)passengerBodyObject))
		{
			return enterVehicleAllowedCheck.Evaluate(component) ?? Interactability.Available;
		}
		if (passengerBodyObject == ((Component)activator).gameObject && passengerAssignmentTime.timeSince >= passengerAssignmentCooldown)
		{
			return exitVehicleAllowedCheck.Evaluate(component) ?? Interactability.Available;
		}
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.currentVehicle) && component.currentVehicle != this)
		{
			return Interactability.ConditionsNotMet;
		}
		return Interactability.Disabled;
	}

	public void OnInteractionBegin(Interactor activator)
	{
		if (!Object.op_Implicit((Object)(object)passengerBodyObject))
		{
			if (!handleVehicleEnterRequestServer.Evaluate(((Component)activator).gameObject).HasValue)
			{
				SetPassenger(((Component)activator).gameObject);
			}
		}
		else if (((Component)activator).gameObject == passengerBodyObject && !handleVehicleExitRequestServer.Evaluate(((Component)activator).gameObject).HasValue)
		{
			SetPassenger(null);
		}
	}

	public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
	{
		return false;
	}

	public bool ShouldShowOnScanner()
	{
		return true;
	}

	private void Awake()
	{
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
		collider = ((Component)this).GetComponent<Collider>();
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		if (!NetworkServer.active && Object.op_Implicit((Object)(object)passengerBodyObject))
		{
			OnPassengerEnter(passengerBodyObject);
		}
	}

	private void SetPassengerInternal(GameObject newPassengerBodyObject)
	{
		if (Object.op_Implicit((Object)(object)passengerBodyObject))
		{
			OnPassengerExit(passengerBodyObject);
		}
		NetworkpassengerBodyObject = newPassengerBodyObject;
		passengerInfo = default(PassengerInfo);
		passengerAssignmentTime = Run.FixedTimeStamp.now;
		if (Object.op_Implicit((Object)(object)passengerBodyObject))
		{
			OnPassengerEnter(passengerBodyObject);
		}
		if (shouldLog)
		{
			Debug.Log((object)"End SetPassenger.");
		}
	}

	private void SetPassenger(GameObject newPassengerBodyObject)
	{
		string text = (Object.op_Implicit((Object)(object)newPassengerBodyObject) ? Util.GetBestBodyName(newPassengerBodyObject) : "null");
		if (shouldLog)
		{
			Debug.LogFormat("SetPassenger passenger={0}", new object[1] { text });
		}
		if (((NetworkBehaviour)this).syncVarHookGuard)
		{
			if (shouldLog)
			{
				Debug.Log((object)"syncVarHookGuard==true Setting passengerBodyObject=newPassengerBodyObject");
			}
			NetworkpassengerBodyObject = newPassengerBodyObject;
			return;
		}
		if (shouldLog)
		{
			Debug.Log((object)"syncVarHookGuard==false");
		}
		if (passengerBodyObject == newPassengerBodyObject)
		{
			if (shouldLog)
			{
				Debug.Log((object)"ReferenceEquals(passengerBodyObject, newPassengerBodyObject)==true");
			}
			return;
		}
		if (shouldLog)
		{
			Debug.Log((object)"ReferenceEquals(passengerBodyObject, newPassengerBodyObject)==false");
		}
		SetPassengerInternal(newPassengerBodyObject);
	}

	private void OnPassengerMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
	{
		if (NetworkServer.active && ejectOnCollision && passengerAssignmentTime.timeSince > Time.fixedDeltaTime)
		{
			SetPassenger(null);
		}
	}

	private void ForcePassengerState()
	{
		if (Object.op_Implicit((Object)(object)passengerInfo.bodyStateMachine) && passengerInfo.hasEffectiveAuthority)
		{
			Type type = passengerState.GetType();
			if (passengerInfo.bodyStateMachine.state.GetType() != type)
			{
				passengerInfo.bodyStateMachine.SetInterruptState(EntityStateCatalog.InstantiateState(passengerState), InterruptPriority.Vehicle);
			}
		}
	}

	private void Update()
	{
		UpdatePassengerPosition();
	}

	private void FixedUpdate()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		ForcePassengerState();
		UpdatePassengerPosition();
		if (Object.op_Implicit((Object)(object)passengerInfo.characterMotor))
		{
			passengerInfo.characterMotor.velocity = Vector3.zero;
		}
	}

	private void UpdatePassengerPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = seatPosition.position;
		if (Object.op_Implicit((Object)(object)passengerInfo.characterMotor))
		{
			passengerInfo.characterMotor.velocity = Vector3.zero;
			((BaseCharacterController)passengerInfo.characterMotor).Motor.BaseVelocity = Vector3.zero;
			((BaseCharacterController)passengerInfo.characterMotor).Motor.SetPosition(position, true);
			if (!disablePassengerMotor && Time.inFixedTimeStep)
			{
				passengerInfo.characterMotor.rootMotion = position - passengerInfo.transform.position;
			}
		}
		else if (Object.op_Implicit((Object)(object)passengerInfo.transform))
		{
			passengerInfo.transform.position = position;
		}
	}

	[Server]
	public bool AssignPassenger(GameObject bodyObject)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.VehicleSeat::AssignPassenger(UnityEngine.GameObject)' called on client");
			return false;
		}
		if (Object.op_Implicit((Object)(object)passengerBodyObject))
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)bodyObject))
		{
			CharacterBody component = bodyObject.GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.currentVehicle))
			{
				component.currentVehicle.EjectPassenger(bodyObject);
			}
		}
		SetPassenger(bodyObject);
		return true;
	}

	[Server]
	public void EjectPassenger(GameObject bodyObject)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.VehicleSeat::EjectPassenger(UnityEngine.GameObject)' called on client");
		}
		else if (bodyObject == passengerBodyObject)
		{
			SetPassenger(null);
		}
	}

	[Server]
	public void EjectPassenger()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.VehicleSeat::EjectPassenger()' called on client");
		}
		else
		{
			SetPassenger(null);
		}
	}

	private void OnDestroy()
	{
		SetPassenger(null);
	}

	private void OnPassengerEnter(GameObject passenger)
	{
		passengerInfo = new PassengerInfo(passengerBodyObject);
		if (Object.op_Implicit((Object)(object)passengerInfo.body))
		{
			passengerInfo.body.currentVehicle = this;
		}
		if (hidePassenger && Object.op_Implicit((Object)(object)passengerInfo.characterModel))
		{
			passengerInfo.characterModel.invisibilityCount++;
		}
		ForcePassengerState();
		if (Object.op_Implicit((Object)(object)passengerInfo.characterMotor))
		{
			if (disablePassengerMotor)
			{
				((Behaviour)passengerInfo.characterMotor).enabled = false;
			}
			else
			{
				passengerInfo.characterMotor.onMovementHit += OnPassengerMovementHit;
			}
		}
		if (Object.op_Implicit((Object)(object)passengerInfo.collider) && Object.op_Implicit((Object)(object)collider))
		{
			Physics.IgnoreCollision(collider, passengerInfo.collider, true);
		}
		if (Object.op_Implicit((Object)(object)passengerInfo.interactionDriver))
		{
			passengerInfo.interactionDriver.interactableOverride = ((Component)this).gameObject;
		}
		if (shouldLog)
		{
			Debug.Log((object)"Taking control of passengerBodyObject.");
			Debug.Log((object)passengerInfo.GetString());
		}
		this.onPassengerEnter?.Invoke(passengerBodyObject);
		UnityEvent obj = onPassengerEnterUnityEvent;
		if (obj != null)
		{
			obj.Invoke();
		}
		VehicleSeat.onPassengerEnterGlobal?.Invoke(this, passengerBodyObject);
	}

	private void OnPassengerExit(GameObject passenger)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		if (shouldLog)
		{
			Debug.Log((object)"Releasing passenger.");
		}
		if (hidePassenger && Object.op_Implicit((Object)(object)passengerInfo.characterModel))
		{
			passengerInfo.characterModel.invisibilityCount--;
		}
		if (Object.op_Implicit((Object)(object)passengerInfo.body))
		{
			passengerInfo.body.currentVehicle = null;
		}
		if (Object.op_Implicit((Object)(object)passengerInfo.characterMotor))
		{
			if (disablePassengerMotor)
			{
				((Behaviour)passengerInfo.characterMotor).enabled = true;
			}
			else
			{
				passengerInfo.characterMotor.onMovementHit -= OnPassengerMovementHit;
			}
			passengerInfo.characterMotor.velocity = Vector3.zero;
			passengerInfo.characterMotor.rootMotion = Vector3.zero;
			((BaseCharacterController)passengerInfo.characterMotor).Motor.BaseVelocity = Vector3.zero;
		}
		if (Object.op_Implicit((Object)(object)passengerInfo.collider) && Object.op_Implicit((Object)(object)collider))
		{
			Physics.IgnoreCollision(collider, passengerInfo.collider, false);
		}
		if (passengerInfo.hasEffectiveAuthority)
		{
			if (Object.op_Implicit((Object)(object)passengerInfo.bodyStateMachine) && passengerInfo.bodyStateMachine.CanInterruptState(InterruptPriority.Vehicle))
			{
				passengerInfo.bodyStateMachine.SetNextStateToMain();
			}
			Vector3 newPosition = (Object.op_Implicit((Object)(object)exitPosition) ? exitPosition.position : seatPosition.position);
			TeleportHelper.TeleportGameObject(((Component)passengerInfo.transform).gameObject, newPosition);
		}
		if (Object.op_Implicit((Object)(object)passengerInfo.interactionDriver) && passengerInfo.interactionDriver.interactableOverride == ((Component)this).gameObject)
		{
			passengerInfo.interactionDriver.interactableOverride = null;
		}
		if (Object.op_Implicit((Object)(object)rigidbody) && Object.op_Implicit((Object)(object)passengerInfo.characterMotor))
		{
			passengerInfo.characterMotor.velocity = rigidbody.velocity * exitVelocityFraction;
		}
		this.onPassengerExit?.Invoke(passengerBodyObject);
		UnityEvent obj = onPassengerExitUnityEvent;
		if (obj != null)
		{
			obj.Invoke();
		}
		VehicleSeat.onPassengerExitGlobal?.Invoke(this, passengerBodyObject);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(passengerBodyObject);
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
			writer.Write(passengerBodyObject);
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
		if (initialState)
		{
			___passengerBodyObjectNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetPassenger(reader.ReadGameObject());
		}
	}

	public override void PreStartClient()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkInstanceId)(ref ___passengerBodyObjectNetId)).IsEmpty())
		{
			NetworkpassengerBodyObject = ClientScene.FindLocalObject(___passengerBodyObjectNetId);
		}
	}
}
