using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EntityStates;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class ArtifactTrialMissionController : NetworkBehaviour
{
	private class ArtifactTrialMissionControllerBaseState : EntityState
	{
		protected ArtifactTrialMissionController missionController { get; private set; }

		protected virtual bool shouldEnableCombatDirector => false;

		protected virtual bool shouldEnableBossGroup => false;

		protected virtual bool shouldAllowMonsters => shouldEnableCombatDirector;

		public override void OnEnter()
		{
			base.OnEnter();
			missionController = GetComponent<ArtifactTrialMissionController>();
			if (NetworkServer.active)
			{
				CombatDirector[] combatDirectors = missionController.combatDirectors;
				for (int i = 0; i < combatDirectors.Length; i++)
				{
					((Behaviour)combatDirectors[i]).enabled = shouldEnableCombatDirector;
				}
				if (!shouldAllowMonsters)
				{
					for (int num = CharacterMaster.readOnlyInstancesList.Count - 1; num >= 0; num--)
					{
						CharacterMaster characterMaster = CharacterMaster.readOnlyInstancesList[num];
						if (characterMaster.teamIndex == TeamIndex.Monster && characterMaster != missionController.artifactShellMaster)
						{
							characterMaster.TrueKill();
						}
					}
				}
			}
			if (Object.op_Implicit((Object)(object)missionController.bossGroup))
			{
				((Behaviour)missionController.bossGroup).enabled = shouldEnableBossGroup;
			}
		}

		public override void OnExit()
		{
			missionController = null;
			base.OnExit();
		}
	}

	private class WaitForPlayersState : ArtifactTrialMissionControllerBaseState
	{
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && NetworkUser.AllParticipatingNetworkUsersReady())
			{
				outer.SetNextState(new IntroState());
			}
		}

		public override void OnEnter()
		{
			base.OnEnter();
			FadeToBlackManager.fadeCount++;
			FadeToBlackManager.ForceFullBlack();
		}

		public override void OnExit()
		{
			FadeToBlackManager.fadeCount--;
			base.OnExit();
		}
	}

	private class IntroState : ArtifactTrialMissionControllerBaseState
	{
		private ForcedCamera cameraController;

		public override void OnEnter()
		{
			base.OnEnter();
			cameraController = base.missionController?.introCameraController;
			if (Object.op_Implicit((Object)(object)cameraController))
			{
				((Component)cameraController).gameObject.SetActive(true);
			}
		}

		public override void OnExit()
		{
			if (Object.op_Implicit((Object)(object)cameraController))
			{
				((Behaviour)cameraController).enabled = false;
				cameraController = null;
			}
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && !((Behaviour)cameraController).enabled)
			{
				outer.SetNextState(new SetupState());
			}
		}
	}

	private class SetupState : ArtifactTrialMissionControllerBaseState
	{
		public static float delayBeforePushingNotification;

		private float keyRespawnTimer;

		private GameObject keyPickupInstance;

		private bool pushedNotification;

		public override void FixedUpdate()
		{
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			base.FixedUpdate();
			if (NetworkServer.active)
			{
				CharacterBody body = base.missionController.artifactShellMaster.GetBody();
				if (!Object.op_Implicit((Object)(object)body) || body.healthComponent.combinedHealthFraction < 1f)
				{
					outer.SetNextState(new PreCombatState());
					return;
				}
				if (!Object.op_Implicit((Object)(object)keyPickupInstance))
				{
					keyRespawnTimer -= Time.deltaTime;
					if (keyRespawnTimer < 0f)
					{
						keyRespawnTimer = 30f;
						GenericPickupController.CreatePickupInfo createPickupInfo = new GenericPickupController.CreatePickupInfo
						{
							pickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.Items.ArtifactKey.itemIndex),
							position = base.missionController.initialKeyLocation.position,
							rotation = Quaternion.identity
						};
						keyPickupInstance = ((Component)GenericPickupController.CreatePickup(in createPickupInfo)).gameObject;
					}
				}
			}
			if (pushedNotification || !(base.fixedAge > delayBeforePushingNotification) || !(base.fixedAge > 1.5f))
			{
				return;
			}
			pushedNotification = true;
			foreach (CharacterMaster readOnlyInstances in CharacterMaster.readOnlyInstancesList)
			{
				if (Object.op_Implicit((Object)(object)readOnlyInstances.playerCharacterMasterController) && Object.op_Implicit((Object)(object)readOnlyInstances.playerCharacterMasterController.networkUserObject) && readOnlyInstances.playerCharacterMasterController.networkUserObject.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					CharacterMasterNotificationQueue.PushArtifactNotification(readOnlyInstances, base.missionController.currentArtifact);
				}
			}
		}
	}

	private class PreCombatState : ArtifactTrialMissionControllerBaseState
	{
		private static float baseDuration = 1f;

		protected override bool shouldAllowMonsters => true;

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && base.fixedAge >= baseDuration)
			{
				outer.SetNextState(new CombatState());
			}
		}
	}

	private class CombatState : ArtifactTrialMissionControllerBaseState
	{
		protected override bool shouldEnableCombatDirector => true;

		protected override bool shouldEnableBossGroup => true;

		public override void OnEnter()
		{
			base.OnEnter();
			if (NetworkServer.active)
			{
				GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
			}
		}

		private void OnCharacterDeathGlobal(DamageReport damageReport)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			if (Util.CheckRoll(Util.GetExpAdjustedDropChancePercent(base.missionController.chanceForKeyDrop * 100f, ((Component)damageReport.victim).gameObject)))
			{
				Debug.LogFormat("Creating artifact key pickup droplet.", Array.Empty<object>());
				PickupDropletController.CreatePickupDroplet(base.missionController.GenerateDrop(), damageReport.victimBody.corePosition, Vector3.up * 20f);
			}
		}

		public override void OnExit()
		{
			GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && (!Object.op_Implicit((Object)(object)base.missionController.artifactShellMaster) || !base.missionController.artifactShellMaster.hasBody))
			{
				outer.SetNextState(new WaitForRewardTaken());
			}
		}
	}

	private class WaitForRewardTaken : ArtifactTrialMissionControllerBaseState
	{
		private float timer = 3f;

		protected bool shouldShowBossHealthBar = true;

		public override void OnEnter()
		{
			base.OnEnter();
			if (Object.op_Implicit((Object)(object)base.missionController.destroyDisplayRingObject))
			{
				base.missionController.destroyDisplayRingObject.SetActive(true);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && !Object.op_Implicit((Object)(object)base.missionController.artifactPickup))
			{
				timer -= Time.fixedDeltaTime;
				if (timer <= 0f)
				{
					outer.SetNextState(new SpawnExitPortalAndIdle());
				}
			}
		}
	}

	private class SpawnExitPortalAndIdle : ArtifactTrialMissionControllerBaseState
	{
		public override void OnEnter()
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			base.OnEnter();
			if (NetworkServer.active)
			{
				GameObject obj = Object.Instantiate<GameObject>(base.missionController.exitPortalPrefab, base.missionController.exitPortalLocation.position, base.missionController.exitPortalLocation.rotation);
				obj.GetComponent<SceneExitController>().useRunNextStageScene = true;
				NetworkServer.Spawn(obj);
			}
		}
	}

	[Header("Artifact")]
	public GenericPickupController artifactPickup;

	public CharacterMaster artifactShellMaster;

	public GameObject destroyDisplayRingObject;

	[Header("Intro Cutscene")]
	public ForcedCamera introCameraController;

	[Header("Mission Key Parameters")]
	public Transform initialKeyLocation;

	public float chanceForKeyDrop = 0.04f;

	public PickupDropTable keyDropTable;

	[Header("Exit Portal")]
	public GameObject exitPortalPrefab;

	public Transform exitPortalLocation;

	[Header("Combat")]
	public CombatDirector[] combatDirectors;

	public BossGroup bossGroup;

	public static ArtifactDef trialArtifact;

	[SyncVar(hook = "TrySetCurrentArtifact")]
	private int currentArtifactIndex = -1;

	private bool artifactWasEnabled;

	private Xoroshiro128Plus rng;

	public ArtifactDef currentArtifact => ArtifactCatalog.GetArtifactDef((ArtifactIndex)currentArtifactIndex);

	public int NetworkcurrentArtifactIndex
	{
		get
		{
			return currentArtifactIndex;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				TrySetCurrentArtifact(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref currentArtifactIndex, 1u);
		}
	}

	public static event Action<ArtifactTrialMissionController, DamageReport> onShellTakeDamageServer;

	public static event Action<ArtifactTrialMissionController, DamageReport> onShellDeathServer;

	private void TrySetCurrentArtifact(int newArtifactIndex)
	{
		Debug.LogFormat("TrySetCurrentArtifact currentArtifactIndex={0} newArtifactIndex={1}", new object[2] { currentArtifactIndex, newArtifactIndex });
		if (newArtifactIndex != currentArtifactIndex)
		{
			((NetworkBehaviour)this).syncVarHookGuard = true;
			SetCurrentArtifact(newArtifactIndex);
			((NetworkBehaviour)this).syncVarHookGuard = false;
		}
	}

	private void SetCurrentArtifact(int newArtifactIndex)
	{
		if (Object.op_Implicit((Object)(object)currentArtifact))
		{
			OnCurrentArtifactLost(currentArtifact);
		}
		NetworkcurrentArtifactIndex = newArtifactIndex;
		if (Object.op_Implicit((Object)(object)currentArtifact))
		{
			OnCurrentArtifactDiscovered(currentArtifact);
		}
	}

	private void Awake()
	{
		if (NetworkServer.active && Object.op_Implicit((Object)(object)trialArtifact))
		{
			TrySetCurrentArtifact((int)trialArtifact.artifactIndex);
			trialArtifact = null;
		}
	}

	private void OnDestroy()
	{
		TrySetCurrentArtifact(-1);
		GlobalEventManager.onServerDamageDealt -= OnServerDamageDealt;
		if (NetworkServer.active)
		{
			RemoveAllMissionKeys();
		}
	}

	public override void OnStartServer()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		((NetworkBehaviour)this).OnStartServer();
		rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
		GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
		GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		if (!NetworkServer.active)
		{
			SetCurrentArtifact(currentArtifactIndex);
		}
	}

	private void OnCurrentArtifactDiscovered(ArtifactDef artifactDef)
	{
		if (NetworkServer.active && Object.op_Implicit((Object)(object)artifactDef))
		{
			artifactWasEnabled = RunArtifactManager.instance.IsArtifactEnabled(artifactDef);
			RunArtifactManager.instance.SetArtifactEnabledServer(artifactDef, newEnabled: true);
			if (Object.op_Implicit((Object)(object)artifactPickup))
			{
				artifactPickup.NetworkpickupIndex = PickupCatalog.FindPickupIndex(artifactDef.artifactIndex);
			}
		}
	}

	private void OnCurrentArtifactLost(ArtifactDef artifactDef)
	{
		if (NetworkServer.active)
		{
			if (Object.op_Implicit((Object)(object)artifactDef) && Object.op_Implicit((Object)(object)RunArtifactManager.instance))
			{
				RunArtifactManager.instance.SetArtifactEnabledServer(artifactDef, artifactWasEnabled);
			}
			if (Object.op_Implicit((Object)(object)artifactPickup))
			{
				artifactPickup.NetworkpickupIndex = PickupIndex.none;
			}
		}
	}

	private void OnServerDamageDealt(DamageReport damageReport)
	{
		if (damageReport.victimMaster == artifactShellMaster)
		{
			OnShellTakeDamageServer(damageReport);
		}
	}

	private void OnCharacterDeathGlobal(DamageReport damageReport)
	{
		if (damageReport.victimMaster == artifactShellMaster)
		{
			OnShellDeathServer(damageReport);
		}
	}

	private void OnShellTakeDamageServer(DamageReport damageReport)
	{
		RemoveAllMissionKeys();
		ArtifactTrialMissionController.onShellTakeDamageServer?.Invoke(this, damageReport);
	}

	private void OnShellDeathServer(DamageReport damageReport)
	{
		ArtifactTrialMissionController.onShellDeathServer?.Invoke(this, damageReport);
	}

	public PickupIndex GenerateDrop()
	{
		if (Object.op_Implicit((Object)(object)keyDropTable))
		{
			Debug.Log((object)"Generating key drop.");
			PickupIndex pickupIndex = keyDropTable.GenerateDrop(rng);
			Debug.LogFormat("itemIndex = {0}, isValid = {1}, pickupNameToken = {2}", new object[3]
			{
				pickupIndex.itemIndex,
				pickupIndex.isValid,
				pickupIndex.GetPickupNameToken()
			});
			return keyDropTable.GenerateDrop(rng);
		}
		Debug.LogError((object)"Failed to generate key drop!");
		return PickupIndex.none;
	}

	public static void RemoveAllMissionKeys()
	{
		ItemIndex itemIndex = RoR2Content.Items.ArtifactKey.itemIndex;
		PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);
		foreach (CharacterMaster readOnlyInstances in CharacterMaster.readOnlyInstancesList)
		{
			int itemCount = readOnlyInstances.inventory.GetItemCount(itemIndex);
			if (itemCount > 0)
			{
				readOnlyInstances.inventory.RemoveItem(itemIndex, itemCount);
			}
		}
		List<GenericPickupController> instancesList = InstanceTracker.GetInstancesList<GenericPickupController>();
		for (int num = instancesList.Count - 1; num >= 0; num--)
		{
			GenericPickupController genericPickupController = instancesList[num];
			if (genericPickupController.pickupIndex == pickupIndex)
			{
				Object.Destroy((Object)(object)((Component)genericPickupController).gameObject);
			}
		}
		List<PickupPickerController> instancesList2 = InstanceTracker.GetInstancesList<PickupPickerController>();
		for (int num2 = instancesList2.Count - 1; num2 >= 0; num2--)
		{
			PickupPickerController pickupPickerController = instancesList2[num2];
			if (pickupPickerController.IsChoiceAvailable(pickupIndex))
			{
				Object.Destroy((Object)(object)((Component)pickupPickerController).gameObject);
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)currentArtifactIndex);
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
			writer.WritePackedUInt32((uint)currentArtifactIndex);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			currentArtifactIndex = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			TrySetCurrentArtifact((int)reader.ReadPackedUInt32());
		}
	}

	public override void PreStartClient()
	{
	}
}
