using System.Collections.Generic;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Stats;

[RequireComponent(typeof(PlayerCharacterMasterController))]
[RequireComponent(typeof(CharacterMaster))]
public class PlayerStatsComponent : NetworkBehaviour
{
	public static readonly List<PlayerStatsComponent> instancesList = new List<PlayerStatsComponent>();

	private float serverTransmitTimer;

	private float serverTransmitInterval = 10f;

	private Vector3 previousBodyPosition;

	private GameObject cachedBodyObject;

	private CharacterBody cachedCharacterBody;

	private CharacterMotor cachedBodyCharacterMotor;

	private Transform cachedBodyTransform;

	public StatSheet currentStats;

	private StatSheet clientDeltaStatsBuffer;

	private StatSheet recordedStats;

	public CharacterMaster characterMaster { get; private set; }

	public PlayerCharacterMasterController playerCharacterMasterController { get; private set; }

	private void Awake()
	{
		playerCharacterMasterController = ((Component)this).GetComponent<PlayerCharacterMasterController>();
		characterMaster = ((Component)this).GetComponent<CharacterMaster>();
		instancesList.Add(this);
		currentStats = StatSheet.New();
		if (NetworkClient.active)
		{
			recordedStats = StatSheet.New();
			clientDeltaStatsBuffer = StatSheet.New();
		}
	}

	private void OnDestroy()
	{
		if (NetworkServer.active)
		{
			SendUpdateToClient();
		}
		instancesList.Remove(this);
	}

	public static StatSheet FindBodyStatSheet(GameObject bodyObject)
	{
		if (!Object.op_Implicit((Object)(object)bodyObject))
		{
			return null;
		}
		return FindBodyStatSheet(bodyObject.GetComponent<CharacterBody>());
	}

	public static StatSheet FindBodyStatSheet(CharacterBody characterBody)
	{
		if (characterBody == null)
		{
			return null;
		}
		CharacterMaster master = characterBody.master;
		if (master == null)
		{
			return null;
		}
		return ((Component)master).GetComponent<PlayerStatsComponent>()?.currentStats;
	}

	public static StatSheet FindMasterStatSheet(CharacterMaster master)
	{
		return FindMasterStatsComponent(master)?.currentStats;
	}

	public static PlayerStatsComponent FindBodyStatsComponent(GameObject bodyObject)
	{
		if (!Object.op_Implicit((Object)(object)bodyObject))
		{
			return null;
		}
		return FindBodyStatsComponent(bodyObject.GetComponent<CharacterBody>());
	}

	public static PlayerStatsComponent FindBodyStatsComponent(CharacterBody characterBody)
	{
		if (characterBody == null)
		{
			return null;
		}
		CharacterMaster master = characterBody.master;
		if (master == null)
		{
			return null;
		}
		return ((Component)master).GetComponent<PlayerStatsComponent>();
	}

	public static PlayerStatsComponent FindMasterStatsComponent(CharacterMaster master)
	{
		return master?.playerStatsComponent;
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void Init()
	{
		GlobalEventManager.onCharacterDeathGlobal += delegate(DamageReport damageReport)
		{
			if (NetworkServer.active)
			{
				PlayerStatsComponent playerStatsComponent = FindBodyStatsComponent(((Component)damageReport.victim).gameObject);
				if (Object.op_Implicit((Object)(object)playerStatsComponent))
				{
					playerStatsComponent.serverTransmitTimer = 0f;
				}
			}
		};
	}

	private void FixedUpdate()
	{
		if (NetworkServer.active)
		{
			ServerFixedUpdate();
		}
	}

	[Server]
	public void ForceNextTransmit()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Stats.PlayerStatsComponent::ForceNextTransmit()' called on client");
		}
		else
		{
			serverTransmitTimer = 0f;
		}
	}

	[Server]
	private void ServerFixedUpdate()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Stats.PlayerStatsComponent::ServerFixedUpdate()' called on client");
			return;
		}
		float num = 0f;
		float runTime = 0f;
		if (Object.op_Implicit((Object)(object)Run.instance) && !Run.instance.isRunStopwatchPaused)
		{
			num = Time.fixedDeltaTime;
			runTime = Run.instance.GetRunStopwatch();
		}
		StatManager.CharacterUpdateEvent e = default(StatManager.CharacterUpdateEvent);
		e.statsComponent = this;
		e.runTime = runTime;
		GameObject bodyObject = characterMaster.GetBodyObject();
		if ((Object)(object)bodyObject != (Object)(object)cachedBodyObject)
		{
			cachedBodyObject = bodyObject;
			cachedBodyObject = bodyObject;
			cachedBodyTransform = ((bodyObject != null) ? bodyObject.transform : null);
			if (Object.op_Implicit((Object)(object)cachedBodyTransform))
			{
				previousBodyPosition = cachedBodyTransform.position;
			}
			cachedCharacterBody = ((bodyObject != null) ? bodyObject.GetComponent<CharacterBody>() : null);
			cachedBodyCharacterMotor = ((bodyObject != null) ? bodyObject.GetComponent<CharacterMotor>() : null);
		}
		if (Object.op_Implicit((Object)(object)cachedBodyTransform))
		{
			Vector3 position = cachedBodyTransform.position;
			e.additionalDistanceTraveled = Vector3.Distance(position, previousBodyPosition);
			previousBodyPosition = position;
		}
		if (characterMaster.hasBody)
		{
			e.additionalTimeAlive += num;
		}
		if (Object.op_Implicit((Object)(object)cachedCharacterBody))
		{
			e.level = (int)cachedCharacterBody.level;
		}
		StatManager.PushCharacterUpdateEvent(e);
		serverTransmitTimer -= Time.fixedDeltaTime;
		if (serverTransmitTimer <= 0f)
		{
			serverTransmitTimer = serverTransmitInterval;
			SendUpdateToClient();
		}
	}

	[Server]
	private void SendUpdateToClient()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Stats.PlayerStatsComponent::SendUpdateToClient()' called on client");
			return;
		}
		NetworkUser networkUser = playerCharacterMasterController.networkUser;
		if (Object.op_Implicit((Object)(object)networkUser))
		{
			NetworkWriter val = new NetworkWriter();
			val.StartMessage((short)58);
			val.Write(((Component)this).gameObject);
			currentStats.Write(val);
			val.FinishMessage();
			((NetworkBehaviour)networkUser).connectionToClient.SendWriter(val, ((NetworkBehaviour)this).GetNetworkChannel());
		}
	}

	[NetworkMessageHandler(client = true, msgType = 58)]
	private static void HandleStatsUpdate(NetworkMessage netMsg)
	{
		GameObject val = netMsg.reader.ReadGameObject();
		if (Object.op_Implicit((Object)(object)val))
		{
			PlayerStatsComponent component = val.GetComponent<PlayerStatsComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.InstanceHandleStatsUpdate(netMsg.reader);
			}
		}
	}

	[Client]
	private void InstanceHandleStatsUpdate(NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.Stats.PlayerStatsComponent::InstanceHandleStatsUpdate(UnityEngine.Networking.NetworkReader)' called on server");
			return;
		}
		if (!NetworkServer.active)
		{
			currentStats.Read(reader);
		}
		FlushStatsToUserProfile();
	}

	[Client]
	private void FlushStatsToUserProfile()
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.Stats.PlayerStatsComponent::FlushStatsToUserProfile()' called on server");
			return;
		}
		StatSheet.GetDelta(clientDeltaStatsBuffer, currentStats, recordedStats);
		StatSheet.Copy(currentStats, recordedStats);
		(playerCharacterMasterController.networkUser?.localUser)?.userProfile?.ApplyDeltaStatSheet(clientDeltaStatsBuffer);
	}

	[ConCommand(commandName = "print_stats", flags = ConVarFlags.None, helpText = "Prints all current stats of the sender.")]
	private static void CCPrintStats(ConCommandArgs args)
	{
		GameObject senderMasterObject = args.senderMasterObject;
		StatSheet statSheet = ((senderMasterObject == null) ? null : senderMasterObject.GetComponent<PlayerStatsComponent>()?.currentStats);
		if (statSheet != null)
		{
			string[] array = new string[statSheet.fields.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = $"[\"{statSheet.fields[i].name}\"]={statSheet.fields[i].ToString()}";
			}
			Debug.Log((object)string.Join("\n", array));
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
