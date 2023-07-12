using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using EntityStates;
using RoR2.CharacterAI;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(SceneExitController))]
[RequireComponent(typeof(HoldoutZoneController))]
public sealed class TeleporterInteraction : NetworkBehaviour, IInteractable
{
	public enum ActivationState
	{
		Idle,
		IdleToCharging,
		Charging,
		Charged,
		Finished
	}

	private abstract class BaseTeleporterState : BaseState
	{
		protected TeleporterInteraction teleporterInteraction { get; private set; }

		public abstract ActivationState backwardsCompatibleActivationState { get; }

		protected virtual bool shouldEnableChargingSphere => false;

		public override void OnEnter()
		{
			base.OnEnter();
			teleporterInteraction = GetComponent<TeleporterInteraction>();
			((Behaviour)teleporterInteraction.holdoutZoneController).enabled = shouldEnableChargingSphere;
		}

		public virtual Interactability GetInteractability(Interactor activator)
		{
			return Interactability.Disabled;
		}

		public virtual string GetContextString(Interactor activator)
		{
			return null;
		}

		public virtual void OnInteractionBegin(Interactor activator)
		{
		}

		protected void SetChildActive(string childLocatorName, bool newActive)
		{
			Transform val = teleporterInteraction.modelChildLocator.FindChild(childLocatorName);
			if (Object.op_Implicit((Object)(object)val))
			{
				((Component)val).gameObject.SetActive(newActive);
			}
		}
	}

	private class IdleState : BaseTeleporterState
	{
		public override ActivationState backwardsCompatibleActivationState => ActivationState.Idle;

		public override Interactability GetInteractability(Interactor activator)
		{
			return Interactability.Available;
		}

		public override string GetContextString(Interactor activator)
		{
			return Language.GetString(base.teleporterInteraction.beginContextString);
		}

		public override void OnInteractionBegin(Interactor activator)
		{
			Chat.SendBroadcastChat(new SubjectChatMessage
			{
				subjectAsCharacterBody = ((Component)activator).GetComponent<CharacterBody>(),
				baseToken = "PLAYER_ACTIVATED_TELEPORTER"
			});
			if (base.teleporterInteraction.showExtraBossesIndicator)
			{
				Chat.SendBroadcastChat(new Chat.SimpleChatMessage
				{
					baseToken = "SHRINE_BOSS_BEGIN_TRIAL"
				});
			}
			base.teleporterInteraction.chargeActivatorServer = ((Component)activator).gameObject;
			outer.SetNextState(new IdleToChargingState());
		}
	}

	private class IdleToChargingState : BaseTeleporterState
	{
		public override ActivationState backwardsCompatibleActivationState => ActivationState.IdleToCharging;

		public override void OnEnter()
		{
			base.OnEnter();
			SetChildActive("IdleToChargingEffect", newActive: true);
			SetChildActive("PPVolume", newActive: true);
		}

		public override void OnExit()
		{
			SetChildActive("IdleToChargingEffect", newActive: false);
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge > 3f && NetworkServer.active)
			{
				outer.SetNextState(new ChargingState());
			}
		}
	}

	private class ChargingState : BaseTeleporterState
	{
		private CombatDirector bonusDirector => base.teleporterInteraction.bonusDirector;

		private CombatDirector bossDirector => base.teleporterInteraction.bossDirector;

		protected override bool shouldEnableChargingSphere => true;

		public override ActivationState backwardsCompatibleActivationState => ActivationState.Charging;

		public override void OnEnter()
		{
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			base.OnEnter();
			TeleporterInteraction.onTeleporterBeginChargingGlobal?.Invoke(base.teleporterInteraction);
			if (NetworkServer.active)
			{
				if (Object.op_Implicit((Object)(object)bonusDirector))
				{
					((Behaviour)bonusDirector).enabled = true;
				}
				if (Object.op_Implicit((Object)(object)bossDirector))
				{
					((Behaviour)bossDirector).enabled = true;
					bossDirector.monsterCredit += (int)(600f * Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 0.5f) * (float)(1 + base.teleporterInteraction.shrineBonusStacks));
					bossDirector.currentSpawnTarget = base.gameObject;
					bossDirector.SetNextSpawnAsBoss();
				}
				if (Object.op_Implicit((Object)(object)DirectorCore.instance))
				{
					CombatDirector[] components = ((Component)DirectorCore.instance).GetComponents<CombatDirector>();
					if (components.Length != 0)
					{
						CombatDirector[] array = components;
						for (int i = 0; i < array.Length; i++)
						{
							((Behaviour)array[i]).enabled = false;
						}
					}
				}
				if (Object.op_Implicit((Object)(object)base.teleporterInteraction.outsideInteractableLocker))
				{
					((Behaviour)base.teleporterInteraction.outsideInteractableLocker).enabled = true;
				}
				ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(TeamIndex.Player);
				for (int j = 0; j < teamMembers.Count; j++)
				{
					TeamComponent teamComponent = teamMembers[j];
					CharacterBody body = teamComponent.body;
					if (!Object.op_Implicit((Object)(object)body))
					{
						continue;
					}
					CharacterMaster master = teamComponent.body.master;
					if (Object.op_Implicit((Object)(object)master))
					{
						int itemCount = master.inventory.GetItemCount(RoR2Content.Items.WardOnLevel);
						if (itemCount > 0)
						{
							GameObject obj = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/WarbannerWard"), ((Component)body).transform.position, Quaternion.identity);
							obj.GetComponent<TeamFilter>().teamIndex = TeamIndex.Player;
							obj.GetComponent<BuffWard>().Networkradius = 8f + 8f * (float)itemCount;
							NetworkServer.Spawn(obj);
						}
					}
				}
			}
			SetChildActive("ChargingEffect", newActive: true);
		}

		public override void OnExit()
		{
			if (NetworkServer.active)
			{
				if (Object.op_Implicit((Object)(object)base.teleporterInteraction.outsideInteractableLocker))
				{
					((Behaviour)base.teleporterInteraction.outsideInteractableLocker).enabled = false;
				}
				if (Object.op_Implicit((Object)(object)bossDirector))
				{
					((Behaviour)bossDirector).enabled = false;
				}
				if (Object.op_Implicit((Object)(object)bonusDirector))
				{
					((Behaviour)bonusDirector).enabled = false;
				}
			}
			SetChildActive("ChargingEffect", newActive: false);
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active)
			{
				if (base.teleporterInteraction.holdoutZoneController.charge >= 1f)
				{
					if (Object.op_Implicit((Object)(object)bonusDirector))
					{
						((Behaviour)bonusDirector).enabled = false;
					}
					if (base.teleporterInteraction.monstersCleared)
					{
						if (Object.op_Implicit((Object)(object)bossDirector))
						{
							((Behaviour)bossDirector).enabled = false;
						}
						outer.SetNextState(new ChargedState());
					}
				}
				if (Object.op_Implicit((Object)(object)base.teleporterInteraction.outsideInteractableLocker))
				{
					base.teleporterInteraction.outsideInteractableLocker.radius = base.teleporterInteraction.holdoutZoneController.currentRadius;
				}
			}
			if (Object.op_Implicit((Object)(object)SceneWeatherController.instance))
			{
				SceneWeatherController.instance.weatherLerp = SceneWeatherController.instance.weatherLerpOverChargeTime.Evaluate((float)base.teleporterInteraction.chargePercent * 0.01f);
			}
			base.teleporterInteraction.UpdateMonstersClear();
		}

		public override Interactability GetInteractability(Interactor activator)
		{
			return Interactability.ConditionsNotMet;
		}
	}

	private class ChargedState : BaseTeleporterState
	{
		public override ActivationState backwardsCompatibleActivationState => ActivationState.Charged;

		public override void OnEnter()
		{
			base.OnEnter();
			((Component)base.teleporterInteraction.teleporterPositionIndicator).GetComponent<ChargeIndicatorController>().isCharged = true;
			SetChildActive("ChargedEffect", newActive: true);
			if (NetworkServer.active)
			{
				base.teleporterInteraction.AttemptToSpawnAllEligiblePortals();
			}
			TeleporterInteraction.onTeleporterChargedGlobal?.Invoke(base.teleporterInteraction);
		}

		public override void OnExit()
		{
			SetChildActive("ChargedEffect", newActive: false);
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
		}

		public override Interactability GetInteractability(Interactor activator)
		{
			if (!base.teleporterInteraction.monstersCleared)
			{
				return Interactability.ConditionsNotMet;
			}
			return Interactability.Available;
		}

		public override string GetContextString(Interactor activator)
		{
			return Language.GetString(base.teleporterInteraction.exitContextString);
		}

		public override void OnInteractionBegin(Interactor activator)
		{
			outer.SetNextState(new FinishedState());
		}
	}

	private class FinishedState : BaseTeleporterState
	{
		public override ActivationState backwardsCompatibleActivationState => ActivationState.Finished;

		public override void OnEnter()
		{
			base.OnEnter();
			if (NetworkServer.active)
			{
				base.teleporterInteraction.sceneExitController.Begin();
			}
			TeleporterInteraction.onTeleporterFinishGlobal?.Invoke(base.teleporterInteraction);
		}
	}

	[Header("Component and Object References")]
	public CombatDirector bonusDirector;

	public CombatDirector bossDirector;

	public EntityStateMachine mainStateMachine;

	public OutsideInteractableLocker outsideInteractableLocker;

	[Header("Interactability")]
	public string beginContextString;

	public string exitContextString;

	private BossGroup bossGroup;

	private ChildLocator modelChildLocator;

	[SyncVar]
	private bool _locked;

	private PositionIndicator teleporterPositionIndicator;

	private ChargeIndicatorController teleporterChargeIndicatorController;

	private Color originalTeleporterColor;

	private GameObject bossShrineIndicator;

	[SyncVar(hook = "OnSyncShouldAttemptToSpawnShopPortal")]
	private bool _shouldAttemptToSpawnShopPortal;

	[SyncVar(hook = "OnSyncShouldAttemptToSpawnGoldshoresPortal")]
	private bool _shouldAttemptToSpawnGoldshoresPortal;

	[SyncVar(hook = "OnSyncShouldAttemptToSpawnMSPortal")]
	private bool _shouldAttemptToSpawnMSPortal;

	private Xoroshiro128Plus rng;

	private GameObject chargeActivatorServer;

	private bool monstersCleared;

	[SyncVar]
	private bool showExtraBossesIndicator;

	public SpawnCard shopPortalSpawnCard;

	public SpawnCard goldshoresPortalSpawnCard;

	public SpawnCard msPortalSpawnCard;

	public PortalSpawner[] portalSpawners;

	public float baseShopSpawnChance = 0.375f;

	private static int kRpcRpcClientOnActivated;

	public HoldoutZoneController holdoutZoneController { get; private set; }

	public SceneExitController sceneExitController { get; private set; }

	private BaseTeleporterState currentState => mainStateMachine.state as BaseTeleporterState;

	public ActivationState activationState => currentState?.backwardsCompatibleActivationState ?? ActivationState.Idle;

	public bool locked
	{
		get
		{
			return _locked;
		}
		set
		{
			Network_locked = value;
		}
	}

	public bool isIdle => currentState is IdleState;

	public bool isIdleToCharging => currentState is IdleToChargingState;

	public bool isInFinalSequence => currentState is FinishedState;

	public bool isCharging => currentState is ChargingState;

	public bool isCharged => activationState >= ActivationState.Charged;

	public float chargeFraction => holdoutZoneController.charge;

	public int shrineBonusStacks { get; set; }

	private int chargePercent => holdoutZoneController.displayChargePercent;

	public static TeleporterInteraction instance { get; private set; }

	public bool shouldAttemptToSpawnShopPortal
	{
		get
		{
			return _shouldAttemptToSpawnShopPortal;
		}
		set
		{
			if (_shouldAttemptToSpawnShopPortal != value)
			{
				Network_shouldAttemptToSpawnShopPortal = value;
				if (_shouldAttemptToSpawnShopPortal && NetworkServer.active)
				{
					Chat.SendBroadcastChat(new Chat.SimpleChatMessage
					{
						baseToken = "PORTAL_SHOP_WILL_OPEN"
					});
				}
			}
		}
	}

	public bool shouldAttemptToSpawnGoldshoresPortal
	{
		get
		{
			return _shouldAttemptToSpawnGoldshoresPortal;
		}
		set
		{
			if (_shouldAttemptToSpawnGoldshoresPortal != value)
			{
				Network_shouldAttemptToSpawnGoldshoresPortal = value;
				if (_shouldAttemptToSpawnGoldshoresPortal && NetworkServer.active)
				{
					Chat.SendBroadcastChat(new Chat.SimpleChatMessage
					{
						baseToken = "PORTAL_GOLDSHORES_WILL_OPEN"
					});
				}
			}
		}
	}

	public bool shouldAttemptToSpawnMSPortal
	{
		get
		{
			return _shouldAttemptToSpawnMSPortal;
		}
		set
		{
			if (_shouldAttemptToSpawnMSPortal != value)
			{
				Network_shouldAttemptToSpawnMSPortal = value;
				if (_shouldAttemptToSpawnMSPortal && NetworkServer.active)
				{
					Chat.SendBroadcastChat(new Chat.SimpleChatMessage
					{
						baseToken = "PORTAL_MS_WILL_OPEN"
					});
				}
			}
		}
	}

	public bool Network_locked
	{
		get
		{
			return _locked;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref _locked, 1u);
		}
	}

	public bool Network_shouldAttemptToSpawnShopPortal
	{
		get
		{
			return _shouldAttemptToSpawnShopPortal;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncShouldAttemptToSpawnShopPortal(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref _shouldAttemptToSpawnShopPortal, 2u);
		}
	}

	public bool Network_shouldAttemptToSpawnGoldshoresPortal
	{
		get
		{
			return _shouldAttemptToSpawnGoldshoresPortal;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncShouldAttemptToSpawnGoldshoresPortal(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref _shouldAttemptToSpawnGoldshoresPortal, 4u);
		}
	}

	public bool Network_shouldAttemptToSpawnMSPortal
	{
		get
		{
			return _shouldAttemptToSpawnMSPortal;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncShouldAttemptToSpawnMSPortal(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref _shouldAttemptToSpawnMSPortal, 8u);
		}
	}

	public bool NetworkshowExtraBossesIndicator
	{
		get
		{
			return showExtraBossesIndicator;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref showExtraBossesIndicator, 16u);
		}
	}

	public static event Action<TeleporterInteraction> onTeleporterBeginChargingGlobal;

	public static event Action<TeleporterInteraction> onTeleporterChargedGlobal;

	public static event Action<TeleporterInteraction> onTeleporterFinishGlobal;

	private void OnSyncShouldAttemptToSpawnShopPortal(bool newValue)
	{
		Network_shouldAttemptToSpawnShopPortal = newValue;
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			((Component)modelChildLocator.FindChild("ShopPortalIndicator")).gameObject.SetActive(newValue);
		}
	}

	private void OnSyncShouldAttemptToSpawnGoldshoresPortal(bool newValue)
	{
		Network_shouldAttemptToSpawnGoldshoresPortal = newValue;
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			((Component)modelChildLocator.FindChild("GoldshoresPortalIndicator")).gameObject.SetActive(newValue);
		}
	}

	private void OnSyncShouldAttemptToSpawnMSPortal(bool newValue)
	{
		Network_shouldAttemptToSpawnMSPortal = newValue;
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			((Component)modelChildLocator.FindChild("MSPortalIndicator")).gameObject.SetActive(newValue);
		}
	}

	private void Awake()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		modelChildLocator = ((Component)((Component)this).GetComponent<ModelLocator>().modelTransform).GetComponent<ChildLocator>();
		holdoutZoneController = ((Component)this).GetComponent<HoldoutZoneController>();
		sceneExitController = ((Component)this).GetComponent<SceneExitController>();
		bossShrineIndicator = ((Component)modelChildLocator.FindChild("BossShrineSymbol")).gameObject;
		bossGroup = ((Component)this).GetComponent<BossGroup>();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)bossDirector))
		{
			((UnityEvent<GameObject>)bossDirector.onSpawnedServer).AddListener((UnityAction<GameObject>)OnBossDirectorSpawnedMonsterServer);
		}
		teleporterPositionIndicator = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/PositionIndicators/TeleporterChargingPositionIndicator"), ((Component)this).transform.position, Quaternion.identity).GetComponent<PositionIndicator>();
		teleporterPositionIndicator.targetTransform = ((Component)this).transform;
		teleporterChargeIndicatorController = ((Component)teleporterPositionIndicator).GetComponent<ChargeIndicatorController>();
		teleporterChargeIndicatorController.holdoutZoneController = holdoutZoneController;
		((Component)teleporterPositionIndicator).gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)teleporterPositionIndicator))
		{
			Object.Destroy((Object)(object)((Component)teleporterPositionIndicator).gameObject);
			teleporterPositionIndicator = null;
			teleporterChargeIndicatorController = null;
		}
	}

	private void OnEnable()
	{
		instance = SingletonHelper.Assign<TeleporterInteraction>(instance, this);
		InstanceTracker.Add<TeleporterInteraction>(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove<TeleporterInteraction>(this);
		instance = SingletonHelper.Unassign<TeleporterInteraction>(instance, this);
	}

	private void Start()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (NetworkServer.active)
		{
			rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
			Run.instance.PickNextStageSceneFromCurrentSceneDestinations();
			float nextNormalizedFloat = rng.nextNormalizedFloat;
			float num = baseShopSpawnChance / (float)(Run.instance.shopPortalCount + 1);
			shouldAttemptToSpawnShopPortal = nextNormalizedFloat < num;
			int stageClearCount = Run.instance.stageClearCount;
			if ((stageClearCount + 1) % Run.stagesPerLoop == 3 && stageClearCount > Run.stagesPerLoop && !Run.instance.GetEventFlag("NoMysterySpace"))
			{
				shouldAttemptToSpawnMSPortal = true;
			}
		}
	}

	public void FixedUpdate()
	{
		bossShrineIndicator.SetActive(showExtraBossesIndicator && !isCharged);
		((Component)teleporterPositionIndicator).gameObject.SetActive(!isIdle);
	}

	public string GetContextString(Interactor activator)
	{
		return currentState?.GetContextString(activator);
	}

	public Interactability GetInteractability(Interactor activator)
	{
		if (locked)
		{
			return Interactability.Disabled;
		}
		return currentState?.GetInteractability(activator) ?? Interactability.Disabled;
	}

	public void OnInteractionBegin(Interactor activator)
	{
		CallRpcClientOnActivated(((Component)activator).gameObject);
		currentState?.OnInteractionBegin(activator);
	}

	public bool ShouldShowOnScanner()
	{
		return true;
	}

	public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
	{
		return false;
	}

	[ClientRpc]
	private void RpcClientOnActivated(GameObject activatorObject)
	{
		Util.PlaySound("Play_env_teleporter_active_button", ((Component)this).gameObject);
	}

	private void UpdateMonstersClear()
	{
		monstersCleared = !((Behaviour)bossGroup).enabled;
	}

	[Server]
	public void AddShrineStack()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.TeleporterInteraction::AddShrineStack()' called on client");
		}
		else if (activationState <= ActivationState.IdleToCharging)
		{
			BossGroup obj = bossGroup;
			int bonusRewardCount = obj.bonusRewardCount + 1;
			obj.bonusRewardCount = bonusRewardCount;
			shrineBonusStacks++;
			NetworkshowExtraBossesIndicator = true;
		}
	}

	[Obsolete]
	public bool IsInChargingRange(CharacterBody body)
	{
		return holdoutZoneController.IsBodyInChargingRadius(body);
	}

	private void OnBossDirectorSpawnedMonsterServer(GameObject masterObject)
	{
		BaseAI ai;
		if (Object.op_Implicit((Object)(object)chargeActivatorServer))
		{
			ai = masterObject.GetComponent<BaseAI>();
			if (Object.op_Implicit((Object)(object)ai))
			{
				ai.onBodyDiscovered += AiOnBodyDiscovered;
			}
		}
		void AiOnBodyDiscovered(CharacterBody newBody)
		{
			ai.currentEnemy.gameObject = chargeActivatorServer;
			ai.onBodyDiscovered -= AiOnBodyDiscovered;
		}
	}

	[Server]
	private void AttemptToSpawnAllEligiblePortals()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.TeleporterInteraction::AttemptToSpawnAllEligiblePortals()' called on client");
			return;
		}
		if (shouldAttemptToSpawnShopPortal)
		{
			AttemptToSpawnShopPortal();
		}
		if (shouldAttemptToSpawnGoldshoresPortal)
		{
			AttemptToSpawnGoldshoresPortal();
		}
		if (shouldAttemptToSpawnMSPortal)
		{
			AttemptToSpawnMSPortal();
		}
		PortalSpawner[] array = portalSpawners;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AttemptSpawnPortalServer();
		}
	}

	private bool AttemptSpawnPortal(SpawnCard portalSpawnCard, float minDistance, float maxDistance, string successChatToken)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(portalSpawnCard, new DirectorPlacementRule
		{
			minDistance = minDistance,
			maxDistance = maxDistance,
			placementMode = DirectorPlacementRule.PlacementMode.Approximate,
			position = ((Component)this).transform.position,
			spawnOnTarget = ((Component)this).transform
		}, rng));
		if (Object.op_Implicit((Object)(object)obj))
		{
			Chat.SendBroadcastChat(new Chat.SimpleChatMessage
			{
				baseToken = successChatToken
			});
		}
		return Object.op_Implicit((Object)(object)obj);
	}

	[Server]
	private void AttemptToSpawnShopPortal()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.TeleporterInteraction::AttemptToSpawnShopPortal()' called on client");
		}
		else if (AttemptSpawnPortal(shopPortalSpawnCard, 0f, 30f, "PORTAL_SHOP_OPEN"))
		{
			Run.instance.shopPortalCount++;
		}
	}

	[Server]
	private void AttemptToSpawnGoldshoresPortal()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.TeleporterInteraction::AttemptToSpawnGoldshoresPortal()' called on client");
		}
		else
		{
			AttemptSpawnPortal(goldshoresPortalSpawnCard, 10f, 40f, "PORTAL_GOLDSHORES_OPEN");
		}
	}

	[Server]
	private void AttemptToSpawnMSPortal()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.TeleporterInteraction::AttemptToSpawnMSPortal()' called on client");
		}
		else
		{
			AttemptSpawnPortal(msPortalSpawnCard, 10f, 40f, "PORTAL_MS_OPEN");
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcClientOnActivated(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcClientOnActivated called on server.");
		}
		else
		{
			((TeleporterInteraction)(object)obj).RpcClientOnActivated(reader.ReadGameObject());
		}
	}

	public void CallRpcClientOnActivated(GameObject activatorObject)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcClientOnActivated called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcClientOnActivated);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(activatorObject);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcClientOnActivated");
	}

	static TeleporterInteraction()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kRpcRpcClientOnActivated = 1157394167;
		NetworkBehaviour.RegisterRpcDelegate(typeof(TeleporterInteraction), kRpcRpcClientOnActivated, new CmdDelegate(InvokeRpcRpcClientOnActivated));
		NetworkCRC.RegisterBehaviour("TeleporterInteraction", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(_locked);
			writer.Write(_shouldAttemptToSpawnShopPortal);
			writer.Write(_shouldAttemptToSpawnGoldshoresPortal);
			writer.Write(_shouldAttemptToSpawnMSPortal);
			writer.Write(showExtraBossesIndicator);
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
			writer.Write(_locked);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_shouldAttemptToSpawnShopPortal);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_shouldAttemptToSpawnGoldshoresPortal);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_shouldAttemptToSpawnMSPortal);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(showExtraBossesIndicator);
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
			_locked = reader.ReadBoolean();
			_shouldAttemptToSpawnShopPortal = reader.ReadBoolean();
			_shouldAttemptToSpawnGoldshoresPortal = reader.ReadBoolean();
			_shouldAttemptToSpawnMSPortal = reader.ReadBoolean();
			showExtraBossesIndicator = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			_locked = reader.ReadBoolean();
		}
		if (((uint)num & 2u) != 0)
		{
			OnSyncShouldAttemptToSpawnShopPortal(reader.ReadBoolean());
		}
		if (((uint)num & 4u) != 0)
		{
			OnSyncShouldAttemptToSpawnGoldshoresPortal(reader.ReadBoolean());
		}
		if (((uint)num & 8u) != 0)
		{
			OnSyncShouldAttemptToSpawnMSPortal(reader.ReadBoolean());
		}
		if (((uint)num & 0x10u) != 0)
		{
			showExtraBossesIndicator = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
	}
}
