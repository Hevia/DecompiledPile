using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EntityStates;
using EntityStates.GummyClone;
using HG;
using RoR2.CharacterAI;
using RoR2.Items;
using RoR2.Stats;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace RoR2;

[DisallowMultipleComponent]
[RequireComponent(typeof(MinionOwnership))]
[RequireComponent(typeof(Inventory))]
public class CharacterMaster : NetworkBehaviour
{
	[Tooltip("This is assigned to the prefab automatically by MasterCatalog at runtime. Do not set this value manually.")]
	[HideInInspector]
	[SerializeField]
	private int _masterIndex;

	[Tooltip("The prefab of this character's body.")]
	public GameObject bodyPrefab;

	[Tooltip("Whether or not to spawn the body at the position of this manager object as soon as Start runs.")]
	public bool spawnOnStart;

	[FormerlySerializedAs("teamIndex")]
	[SerializeField]
	[Tooltip("The team of the body.")]
	private TeamIndex _teamIndex;

	public UnityEvent onBodyDeath;

	[Tooltip("Whether or not to destroy this master when the body dies.")]
	public bool destroyOnBodyDeath = true;

	private static List<CharacterMaster> instancesList;

	private static ReadOnlyCollection<CharacterMaster> _readOnlyInstancesList;

	private BaseAI[] aiComponents;

	private const uint bodyDirtyBit = 1u;

	private const uint moneyDirtyBit = 2u;

	private const uint survivalTimeDirtyBit = 4u;

	private const uint teamDirtyBit = 8u;

	private const uint loadoutDirtyBit = 16u;

	private const uint miscFlagsDirtyBit = 32u;

	private const uint voidCoinsDirtyBit = 64u;

	private const uint allDirtyBits = 127u;

	public readonly Loadout loadout = new Loadout();

	private NetworkInstanceId _bodyInstanceId = NetworkInstanceId.Invalid;

	private GameObject resolvedBodyInstance;

	private bool bodyResolved;

	private uint _money;

	private uint _voidCoins;

	public bool isBoss;

	private Xoroshiro128Plus cloverVoidRng;

	[NonSerialized]
	private List<DeployableInfo> deployablesList;

	public bool preventGameOver = true;

	private Vector3 deathAimVector = Vector3.zero;

	private bool killedByUnsafeArea;

	private const float respawnDelayDuration = 2f;

	private float _internalSurvivalTime;

	private BodyIndex killerBodyIndex = BodyIndex.None;

	private bool preventRespawnUntilNextStageServer;

	private bool godMode;

	private uint lostBodyToDeathFlag = 1u;

	private uint _miscFlags;

	private static int kCmdCmdRespawn;

	public MasterCatalog.MasterIndex masterIndex
	{
		get
		{
			return (MasterCatalog.MasterIndex)_masterIndex;
		}
		set
		{
			_masterIndex = (int)value;
		}
	}

	public NetworkIdentity networkIdentity { get; private set; }

	public bool hasEffectiveAuthority { get; private set; }

	public TeamIndex teamIndex
	{
		get
		{
			return _teamIndex;
		}
		set
		{
			if (_teamIndex != value)
			{
				_teamIndex = value;
				if (NetworkServer.active)
				{
					((NetworkBehaviour)this).SetDirtyBit(8u);
				}
			}
		}
	}

	public static ReadOnlyCollection<CharacterMaster> readOnlyInstancesList => _readOnlyInstancesList;

	public Inventory inventory { get; private set; }

	public PlayerCharacterMasterController playerCharacterMasterController { get; private set; }

	public PlayerStatsComponent playerStatsComponent { get; private set; }

	public MinionOwnership minionOwnership { get; private set; }

	private NetworkInstanceId bodyInstanceId
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _bodyInstanceId;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (!(value == _bodyInstanceId))
			{
				((NetworkBehaviour)this).SetDirtyBit(1u);
				_bodyInstanceId = value;
			}
		}
	}

	public BodyIndex backupBodyIndex { get; private set; }

	private GameObject bodyInstanceObject
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (!bodyResolved)
			{
				resolvedBodyInstance = Util.FindNetworkObject(bodyInstanceId);
				if (Object.op_Implicit((Object)(object)resolvedBodyInstance))
				{
					bodyResolved = true;
					StoreBackupBodyIndex();
				}
			}
			return resolvedBodyInstance;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			NetworkInstanceId val = NetworkInstanceId.Invalid;
			resolvedBodyInstance = null;
			bodyResolved = true;
			if (Object.op_Implicit((Object)(object)value))
			{
				NetworkIdentity component = value.GetComponent<NetworkIdentity>();
				if (Object.op_Implicit((Object)(object)component))
				{
					val = component.netId;
					resolvedBodyInstance = value;
					StoreBackupBodyIndex();
				}
			}
			bodyInstanceId = val;
		}
	}

	public uint money
	{
		get
		{
			return _money;
		}
		set
		{
			if (value != _money)
			{
				((NetworkBehaviour)this).SetDirtyBit(2u);
				_money = value;
			}
		}
	}

	public uint voidCoins
	{
		get
		{
			return _voidCoins;
		}
		set
		{
			if (value != _voidCoins)
			{
				((NetworkBehaviour)this).SetDirtyBit(64u);
				_voidCoins = value;
			}
		}
	}

	public float luck { get; set; }

	public bool hasBody => Object.op_Implicit((Object)(object)bodyInstanceObject);

	public Vector3 deathFootPosition { get; private set; } = Vector3.zero;


	private float internalSurvivalTime
	{
		get
		{
			return _internalSurvivalTime;
		}
		set
		{
			if (value != _internalSurvivalTime)
			{
				((NetworkBehaviour)this).SetDirtyBit(4u);
				_internalSurvivalTime = value;
			}
		}
	}

	public float currentLifeStopwatch
	{
		get
		{
			if (internalSurvivalTime <= 0f)
			{
				return 0f - internalSurvivalTime;
			}
			if (Object.op_Implicit((Object)(object)Run.instance))
			{
				return Run.instance.GetRunStopwatch() - internalSurvivalTime;
			}
			return 0f;
		}
	}

	private uint miscFlags
	{
		get
		{
			return _miscFlags;
		}
		set
		{
			if (value != _miscFlags)
			{
				_miscFlags = value;
				if (NetworkServer.active)
				{
					((NetworkBehaviour)this).SetDirtyBit(32u);
				}
			}
		}
	}

	public bool lostBodyToDeath
	{
		get
		{
			return (miscFlags & lostBodyToDeathFlag) != 0;
		}
		private set
		{
			if (value)
			{
				miscFlags |= lostBodyToDeathFlag;
			}
			else
			{
				miscFlags &= ~lostBodyToDeathFlag;
			}
		}
	}

	public static event Action<CharacterMaster> onStartGlobal;

	public static event Action<CharacterMaster> onCharacterMasterDiscovered;

	public static event Action<CharacterMaster> onCharacterMasterLost;

	public event Action<CharacterBody> onBodyStart;

	public event Action<CharacterBody> onBodyDestroyed;

	private void UpdateAuthority()
	{
		hasEffectiveAuthority = Util.HasEffectiveAuthority(networkIdentity);
	}

	public override void OnStartAuthority()
	{
		((NetworkBehaviour)this).OnStartAuthority();
		UpdateAuthority();
	}

	public override void OnStopAuthority()
	{
		UpdateAuthority();
		((NetworkBehaviour)this).OnStopAuthority();
	}

	[Server]
	public void SetLoadoutServer(Loadout newLoadout)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMaster::SetLoadoutServer(RoR2.Loadout)' called on client");
			return;
		}
		newLoadout.Copy(loadout);
		((NetworkBehaviour)this).SetDirtyBit(16u);
	}

	private void StoreBackupBodyIndex()
	{
		if (Object.op_Implicit((Object)(object)resolvedBodyInstance))
		{
			CharacterBody component = resolvedBodyInstance.GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component))
			{
				backupBodyIndex = component.bodyIndex;
			}
		}
	}

	private void OnSyncBodyInstanceId(NetworkInstanceId value)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		resolvedBodyInstance = null;
		bodyResolved = value == NetworkInstanceId.Invalid;
		_bodyInstanceId = value;
	}

	[Server]
	public void GiveExperience(ulong amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMaster::GiveExperience(System.UInt64)' called on client");
		}
		else
		{
			TeamManager.instance.GiveTeamExperience(teamIndex, amount);
		}
	}

	public void GiveMoney(uint amount)
	{
		money += amount;
		StatManager.OnGoldCollected(this, amount);
	}

	public void GiveVoidCoins(uint amount)
	{
		voidCoins += amount;
	}

	public int GetDeployableSameSlotLimit(DeployableSlot slot)
	{
		int result = 0;
		int num = 1;
		if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.swarmsArtifactDef))
		{
			num = 2;
		}
		switch (slot)
		{
		case DeployableSlot.EngiMine:
			result = 4;
			if (Object.op_Implicit((Object)(object)bodyInstanceObject))
			{
				result = bodyInstanceObject.GetComponent<SkillLocator>().secondary.maxStock;
			}
			break;
		case DeployableSlot.EngiTurret:
			result = ((inventory.GetItemCount(DLC1Content.Items.EquipmentMagazineVoid) <= 0) ? 2 : 3);
			break;
		case DeployableSlot.BeetleGuardAlly:
			result = inventory.GetItemCount(RoR2Content.Items.BeetleGland) * num;
			break;
		case DeployableSlot.EngiBubbleShield:
			result = 1;
			break;
		case DeployableSlot.LoaderPylon:
			result = 3;
			break;
		case DeployableSlot.EngiSpiderMine:
			result = 4;
			if (Object.op_Implicit((Object)(object)bodyInstanceObject))
			{
				result = bodyInstanceObject.GetComponent<SkillLocator>().secondary.maxStock;
			}
			break;
		case DeployableSlot.RoboBallMini:
			result = 3;
			break;
		case DeployableSlot.ParentPodAlly:
			result = inventory.GetItemCount(JunkContent.Items.Incubator) * num;
			break;
		case DeployableSlot.ParentAlly:
			result = inventory.GetItemCount(JunkContent.Items.Incubator) * num;
			break;
		case DeployableSlot.PowerWard:
			result = 1;
			break;
		case DeployableSlot.CrippleWard:
			result = 5;
			break;
		case DeployableSlot.DeathProjectile:
			result = 3;
			break;
		case DeployableSlot.RoboBallRedBuddy:
		case DeployableSlot.RoboBallGreenBuddy:
			result = num;
			break;
		case DeployableSlot.GummyClone:
			result = 3;
			break;
		case DeployableSlot.LunarSunBomb:
			result = LunarSunBehavior.GetMaxProjectiles(inventory);
			break;
		case DeployableSlot.VendingMachine:
			result = 1;
			break;
		case DeployableSlot.VoidMegaCrabItem:
			result = VoidMegaCrabItemBehavior.GetMaxProjectiles(inventory);
			break;
		case DeployableSlot.DroneWeaponsDrone:
			result = 1;
			break;
		case DeployableSlot.MinorConstructOnKill:
			result = inventory.GetItemCount(DLC1Content.Items.MinorConstructOnKill) * 4;
			break;
		case DeployableSlot.CaptainSupplyDrop:
			result = 2;
			break;
		}
		return result;
	}

	[Server]
	public void AddDeployable(Deployable deployable, DeployableSlot slot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMaster::AddDeployable(RoR2.Deployable,RoR2.DeployableSlot)' called on client");
			return;
		}
		if (Object.op_Implicit((Object)(object)deployable.ownerMaster))
		{
			Debug.LogErrorFormat("Attempted to add deployable {0} which already belongs to master {1} to master {2}.", new object[3]
			{
				((Component)deployable).gameObject,
				((Component)deployable.ownerMaster).gameObject,
				((Component)this).gameObject
			});
		}
		if (deployablesList == null)
		{
			deployablesList = new List<DeployableInfo>();
		}
		int num = 0;
		int deployableSameSlotLimit = GetDeployableSameSlotLimit(slot);
		for (int num2 = deployablesList.Count - 1; num2 >= 0; num2--)
		{
			if (deployablesList[num2].slot == slot)
			{
				num++;
				if (num >= deployableSameSlotLimit)
				{
					Deployable deployable2 = deployablesList[num2].deployable;
					deployablesList.RemoveAt(num2);
					deployable2.ownerMaster = null;
					deployable2.onUndeploy.Invoke();
				}
			}
		}
		deployablesList.Add(new DeployableInfo
		{
			deployable = deployable,
			slot = slot
		});
		deployable.ownerMaster = this;
	}

	[Server]
	public int GetDeployableCount(DeployableSlot slot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Int32 RoR2.CharacterMaster::GetDeployableCount(RoR2.DeployableSlot)' called on client");
			return 0;
		}
		if (deployablesList == null)
		{
			return 0;
		}
		int num = 0;
		for (int num2 = deployablesList.Count - 1; num2 >= 0; num2--)
		{
			if (deployablesList[num2].slot == slot)
			{
				num++;
			}
		}
		return num;
	}

	[Server]
	public bool IsDeployableLimited(DeployableSlot slot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.CharacterMaster::IsDeployableLimited(RoR2.DeployableSlot)' called on client");
			return false;
		}
		return GetDeployableCount(slot) >= GetDeployableSameSlotLimit(slot);
	}

	[Server]
	public void RemoveDeployable(Deployable deployable)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMaster::RemoveDeployable(RoR2.Deployable)' called on client");
		}
		else
		{
			if (deployablesList == null || (Object)(object)deployable.ownerMaster != (Object)(object)this)
			{
				return;
			}
			for (int num = deployablesList.Count - 1; num >= 0; num--)
			{
				if ((Object)(object)deployablesList[num].deployable == (Object)(object)deployable)
				{
					deployablesList.RemoveAt(num);
				}
			}
			deployable.ownerMaster = null;
		}
	}

	[Server]
	public bool IsDeployableSlotAvailable(DeployableSlot deployableSlot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.CharacterMaster::IsDeployableSlotAvailable(RoR2.DeployableSlot)' called on client");
			return false;
		}
		return GetDeployableCount(deployableSlot) < GetDeployableSameSlotLimit(deployableSlot);
	}

	[Server]
	public CharacterBody SpawnBody(Vector3 position, Quaternion rotation)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'RoR2.CharacterBody RoR2.CharacterMaster::SpawnBody(UnityEngine.Vector3,UnityEngine.Quaternion)' called on client");
			return null;
		}
		if (Object.op_Implicit((Object)(object)bodyInstanceObject))
		{
			Debug.LogError((object)"Character cannot have more than one body at this time.");
			return null;
		}
		if (!Object.op_Implicit((Object)(object)bodyPrefab))
		{
			Debug.LogErrorFormat("Attempted to spawn body of character master {0} with no body prefab.", new object[1] { ((Component)this).gameObject });
		}
		if (!Object.op_Implicit((Object)(object)bodyPrefab.GetComponent<CharacterBody>()))
		{
			Debug.LogErrorFormat("Attempted to spawn body of character master {0} with a body prefab that has no {1} component attached.", new object[2]
			{
				((Component)this).gameObject,
				typeof(CharacterBody).Name
			});
		}
		bool flag = Object.op_Implicit((Object)(object)bodyPrefab.GetComponent<CharacterDirection>());
		GameObject val = Object.Instantiate<GameObject>(bodyPrefab, position, flag ? Quaternion.identity : rotation);
		CharacterBody component = val.GetComponent<CharacterBody>();
		component.masterObject = ((Component)this).gameObject;
		component.teamComponent.teamIndex = teamIndex;
		component.SetLoadoutServer(loadout);
		if (flag)
		{
			CharacterDirection component2 = val.GetComponent<CharacterDirection>();
			float y = ((Quaternion)(ref rotation)).eulerAngles.y;
			component2.yaw = y;
		}
		NetworkConnection clientAuthorityOwner = ((Component)this).GetComponent<NetworkIdentity>().clientAuthorityOwner;
		if (clientAuthorityOwner != null)
		{
			clientAuthorityOwner.isReady = true;
			NetworkServer.SpawnWithClientAuthority(val, clientAuthorityOwner);
		}
		else
		{
			NetworkServer.Spawn(val);
		}
		bodyInstanceObject = val;
		Run.instance.OnServerCharacterBodySpawned(component);
		return component;
	}

	[Server]
	public void DestroyBody()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMaster::DestroyBody()' called on client");
		}
		else if (Object.op_Implicit((Object)(object)bodyInstanceObject))
		{
			CharacterBody body = GetBody();
			Object.Destroy((Object)(object)bodyInstanceObject);
			OnBodyDestroyed(body);
			bodyInstanceObject = null;
		}
	}

	public GameObject GetBodyObject()
	{
		return bodyInstanceObject;
	}

	public CharacterBody GetBody()
	{
		GameObject bodyObject = GetBodyObject();
		if (!Object.op_Implicit((Object)(object)bodyObject))
		{
			return null;
		}
		return bodyObject.GetComponent<CharacterBody>();
	}

	private void Awake()
	{
		networkIdentity = ((Component)this).GetComponent<NetworkIdentity>();
		inventory = ((Component)this).GetComponent<Inventory>();
		aiComponents = (NetworkServer.active ? ((Component)this).GetComponents<BaseAI>() : Array.Empty<BaseAI>());
		playerCharacterMasterController = ((Component)this).GetComponent<PlayerCharacterMasterController>();
		playerStatsComponent = ((Component)this).GetComponent<PlayerStatsComponent>();
		minionOwnership = ((Component)this).GetComponent<MinionOwnership>();
		inventory.onInventoryChanged += OnInventoryChanged;
		inventory.onItemAddedClient += OnItemAddedClient;
		inventory.onEquipmentExternalRestockServer += OnInventoryEquipmentExternalRestockServer;
		OnInventoryChanged();
		Stage.onServerStageBegin += OnServerStageBegin;
	}

	private void OnItemAddedClient(ItemIndex itemIndex)
	{
		((MonoBehaviour)this).StartCoroutine(HighlightNewItem(itemIndex));
	}

	private IEnumerator HighlightNewItem(ItemIndex itemIndex)
	{
		yield return (object)new WaitForSeconds(0.05f);
		GameObject bodyObject = GetBodyObject();
		if (!Object.op_Implicit((Object)(object)bodyObject))
		{
			yield break;
		}
		ModelLocator component = bodyObject.GetComponent<ModelLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			yield break;
		}
		Transform modelTransform = component.modelTransform;
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			CharacterModel component2 = ((Component)modelTransform).GetComponent<CharacterModel>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.HighlightItemDisplay(itemIndex);
			}
		}
	}

	private void Start()
	{
		UpdateAuthority();
		if (NetworkServer.active && spawnOnStart && !Object.op_Implicit((Object)(object)bodyInstanceObject))
		{
			SpawnBodyHere();
		}
		CharacterMaster.onStartGlobal?.Invoke(this);
	}

	private void OnInventoryChanged()
	{
		luck = 0f;
		luck += inventory.GetItemCount(RoR2Content.Items.Clover);
		luck -= inventory.GetItemCount(RoR2Content.Items.LunarBadLuck);
		if (NetworkServer.active && Object.op_Implicit((Object)(object)inventory))
		{
			CharacterBody body = GetBody();
			if (Object.op_Implicit((Object)(object)body) && body.bodyIndex != BodyCatalog.FindBodyIndex("HereticBody") && inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement.itemIndex) > 0 && inventory.GetItemCount(RoR2Content.Items.LunarSecondaryReplacement.itemIndex) > 0 && inventory.GetItemCount(RoR2Content.Items.LunarSpecialReplacement.itemIndex) > 0 && inventory.GetItemCount(RoR2Content.Items.LunarUtilityReplacement.itemIndex) > 0)
			{
				TransformBody("HereticBody");
			}
		}
		SetUpGummyClone();
	}

	private void OnInventoryEquipmentExternalRestockServer()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		CharacterBody body = GetBody();
		if (Object.op_Implicit((Object)(object)body))
		{
			EffectData effectData = new EffectData();
			effectData.origin = body.corePosition;
			effectData.SetNetworkedObjectReference(((Component)body).gameObject);
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/EquipmentRestockEffect"), effectData, transmit: true);
		}
	}

	[Server]
	public void SpawnBodyHere()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMaster::SpawnBodyHere()' called on client");
		}
		else
		{
			SpawnBody(((Component)this).transform.position, ((Component)this).transform.rotation);
		}
	}

	private void OnEnable()
	{
		instancesList.Add(this);
		CharacterMaster.onCharacterMasterDiscovered?.Invoke(this);
	}

	private void OnDisable()
	{
		try
		{
			CharacterMaster.onCharacterMasterLost?.Invoke(this);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
		instancesList.Remove(this);
	}

	private void OnDestroy()
	{
		if (isBoss)
		{
			isBoss = false;
		}
		Stage.onServerStageBegin -= OnServerStageBegin;
	}

	public void OnBodyStart(CharacterBody body)
	{
		if (NetworkServer.active)
		{
			lostBodyToDeath = false;
		}
		preventGameOver = true;
		killerBodyIndex = BodyIndex.None;
		killedByUnsafeArea = false;
		body.RecalculateStats();
		if (NetworkServer.active)
		{
			BaseAI[] array = aiComponents;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].OnBodyStart(body);
			}
		}
		if (Object.op_Implicit((Object)(object)playerCharacterMasterController))
		{
			if (Object.op_Implicit((Object)(object)playerCharacterMasterController.networkUserObject))
			{
				_ = playerCharacterMasterController.networkUserObject.GetComponent<NetworkIdentity>().isLocalPlayer;
			}
			playerCharacterMasterController.OnBodyStart();
		}
		if (inventory.GetItemCount(RoR2Content.Items.Ghost) > 0)
		{
			Util.PlaySound("Play_item_proc_ghostOnKill", ((Component)body).gameObject);
		}
		if (NetworkServer.active)
		{
			HealthComponent healthComponent = body.healthComponent;
			if (Object.op_Implicit((Object)(object)healthComponent))
			{
				if (teamIndex == TeamIndex.Player && Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse1)
				{
					healthComponent.Networkhealth = healthComponent.fullHealth * 0.5f;
				}
				else
				{
					healthComponent.Networkhealth = healthComponent.fullHealth;
				}
			}
			UpdateBodyGodMode();
			StartLifeStopwatch();
		}
		SetUpGummyClone();
		this.onBodyStart?.Invoke(body);
	}

	[Server]
	public bool IsExtraLifePendingServer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.CharacterMaster::IsExtraLifePendingServer()' called on client");
			return false;
		}
		if (!((MonoBehaviour)this).IsInvoking("RespawnExtraLife"))
		{
			return ((MonoBehaviour)this).IsInvoking("RespawnExtraLifeVoid");
		}
		return true;
	}

	[Server]
	public bool IsDeadAndOutOfLivesServer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.CharacterMaster::IsDeadAndOutOfLivesServer()' called on client");
			return false;
		}
		CharacterBody body = GetBody();
		if (!Object.op_Implicit((Object)(object)body) || !body.healthComponent.alive)
		{
			if (inventory.GetItemCount(RoR2Content.Items.ExtraLife) <= 0 && inventory.GetItemCount(DLC1Content.Items.ExtraLifeVoid) <= 0)
			{
				return !IsExtraLifePendingServer();
			}
			return false;
		}
		return false;
	}

	public void OnBodyDeath(CharacterBody body)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			lostBodyToDeath = true;
			deathFootPosition = body.footPosition;
			BaseAI[] array = aiComponents;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].OnBodyDeath(body);
			}
			if (Object.op_Implicit((Object)(object)playerCharacterMasterController))
			{
				playerCharacterMasterController.OnBodyDeath();
			}
			if (inventory.GetItemCount(RoR2Content.Items.ExtraLife) > 0)
			{
				inventory.RemoveItem(RoR2Content.Items.ExtraLife);
				((MonoBehaviour)this).Invoke("RespawnExtraLife", 2f);
				((MonoBehaviour)this).Invoke("PlayExtraLifeSFX", 1f);
			}
			else if (inventory.GetItemCount(DLC1Content.Items.ExtraLifeVoid) > 0)
			{
				inventory.RemoveItem(DLC1Content.Items.ExtraLifeVoid);
				((MonoBehaviour)this).Invoke("RespawnExtraLifeVoid", 2f);
				((MonoBehaviour)this).Invoke("PlayExtraLifeVoidSFX", 1f);
			}
			else
			{
				if (destroyOnBodyDeath)
				{
					Object.Destroy((Object)(object)((Component)this).gameObject, 1f);
				}
				preventGameOver = false;
				preventRespawnUntilNextStageServer = true;
			}
			ResetLifeStopwatch();
		}
		UnityEvent obj = onBodyDeath;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	public void TrueKill()
	{
		TrueKill(null, null, DamageType.Generic);
	}

	public void TrueKill(GameObject killerOverride = null, GameObject inflictorOverride = null, DamageType damageTypeOverride = DamageType.Generic)
	{
		int itemCount = inventory.GetItemCount(RoR2Content.Items.ExtraLife);
		if (itemCount > 0)
		{
			inventory.ResetItem(RoR2Content.Items.ExtraLife);
			inventory.GiveItem(RoR2Content.Items.ExtraLifeConsumed, itemCount);
			CharacterMasterNotificationQueue.SendTransformNotification(this, RoR2Content.Items.ExtraLife.itemIndex, RoR2Content.Items.ExtraLifeConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
		}
		if (inventory.GetItemCount(DLC1Content.Items.ExtraLifeVoid) > 0)
		{
			inventory.ResetItem(DLC1Content.Items.ExtraLifeVoid);
			inventory.GiveItem(DLC1Content.Items.ExtraLifeVoidConsumed, itemCount);
			CharacterMasterNotificationQueue.SendTransformNotification(this, DLC1Content.Items.ExtraLifeVoid.itemIndex, DLC1Content.Items.ExtraLifeVoidConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
		}
		((MonoBehaviour)this).CancelInvoke("RespawnExtraLife");
		((MonoBehaviour)this).CancelInvoke("PlayExtraLifeSFX");
		((MonoBehaviour)this).CancelInvoke("RespawnExtraLifeVoid");
		((MonoBehaviour)this).CancelInvoke("PlayExtraLifeVoidSFX");
		CharacterBody body = GetBody();
		if (Object.op_Implicit((Object)(object)body))
		{
			body.healthComponent.Suicide(killerOverride, inflictorOverride, damageTypeOverride);
		}
	}

	private void PlayExtraLifeSFX()
	{
		GameObject val = bodyInstanceObject;
		if (Object.op_Implicit((Object)(object)val))
		{
			Util.PlaySound("Play_item_proc_extraLife", val);
		}
	}

	private void PlayExtraLifeVoidSFX()
	{
		GameObject val = bodyInstanceObject;
		if (Object.op_Implicit((Object)(object)val))
		{
			Util.PlaySound("Play_item_void_extraLife", val);
		}
	}

	public void RespawnExtraLife()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		inventory.GiveItem(RoR2Content.Items.ExtraLifeConsumed);
		CharacterMasterNotificationQueue.SendTransformNotification(this, RoR2Content.Items.ExtraLife.itemIndex, RoR2Content.Items.ExtraLifeConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
		Vector3 val = deathFootPosition;
		if (killedByUnsafeArea)
		{
			val = (Vector3)(((_003F?)TeleportHelper.FindSafeTeleportDestination(deathFootPosition, bodyPrefab.GetComponent<CharacterBody>(), RoR2Application.rng)) ?? deathFootPosition);
		}
		Respawn(val, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
		GetBody().AddTimedBuff(RoR2Content.Buffs.Immune, 3f);
		GameObject val2 = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HippoRezEffect");
		if (Object.op_Implicit((Object)(object)bodyInstanceObject))
		{
			EntityStateMachine[] components = bodyInstanceObject.GetComponents<EntityStateMachine>();
			foreach (EntityStateMachine obj in components)
			{
				obj.initialStateType = obj.mainStateType;
			}
			if (Object.op_Implicit((Object)(object)val2))
			{
				EffectManager.SpawnEffect(val2, new EffectData
				{
					origin = val,
					rotation = bodyInstanceObject.transform.rotation
				}, transmit: true);
			}
		}
	}

	public void RespawnExtraLifeVoid()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		inventory.GiveItem(DLC1Content.Items.ExtraLifeVoidConsumed);
		CharacterMasterNotificationQueue.SendTransformNotification(this, DLC1Content.Items.ExtraLifeVoid.itemIndex, DLC1Content.Items.ExtraLifeVoidConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
		Vector3 val = deathFootPosition;
		if (killedByUnsafeArea)
		{
			val = (Vector3)(((_003F?)TeleportHelper.FindSafeTeleportDestination(deathFootPosition, bodyPrefab.GetComponent<CharacterBody>(), RoR2Application.rng)) ?? deathFootPosition);
		}
		Respawn(val, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
		GetBody().AddTimedBuff(RoR2Content.Buffs.Immune, 3f);
		if (Object.op_Implicit((Object)(object)bodyInstanceObject))
		{
			EntityStateMachine[] components = bodyInstanceObject.GetComponents<EntityStateMachine>();
			foreach (EntityStateMachine obj in components)
			{
				obj.initialStateType = obj.mainStateType;
			}
			if (Object.op_Implicit((Object)(object)ExtraLifeVoidManager.rezEffectPrefab))
			{
				EffectManager.SpawnEffect(ExtraLifeVoidManager.rezEffectPrefab, new EffectData
				{
					origin = val,
					rotation = bodyInstanceObject.transform.rotation
				}, transmit: true);
			}
		}
		Enumerator<ContagiousItemManager.TransformationInfo> enumerator = ContagiousItemManager.transformationInfos.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ContagiousItemManager.TransformationInfo current = enumerator.Current;
				ContagiousItemManager.TryForceReplacement(inventory, current.originalItem);
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
	}

	public void OnBodyDamaged(DamageReport damageReport)
	{
		BaseAI[] array = aiComponents;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnBodyDamaged(damageReport);
		}
	}

	public void OnBodyDestroyed(CharacterBody characterBody)
	{
		if (characterBody != GetBody())
		{
			return;
		}
		if (NetworkServer.active)
		{
			BaseAI[] array = aiComponents;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].OnBodyDestroyed(characterBody);
			}
			PauseLifeStopwatch();
		}
		this.onBodyDestroyed?.Invoke(characterBody);
	}

	[Server]
	private void StartLifeStopwatch()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMaster::StartLifeStopwatch()' called on client");
		}
		else if (!(internalSurvivalTime > 0f))
		{
			internalSurvivalTime = Run.instance.GetRunStopwatch() - currentLifeStopwatch;
		}
	}

	[Server]
	private void PauseLifeStopwatch()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMaster::PauseLifeStopwatch()' called on client");
		}
		else if (!(internalSurvivalTime <= 0f))
		{
			internalSurvivalTime = 0f - currentLifeStopwatch;
		}
	}

	[Server]
	private void ResetLifeStopwatch()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMaster::ResetLifeStopwatch()' called on client");
		}
		else
		{
			internalSurvivalTime = 0f;
		}
	}

	[Server]
	public BodyIndex GetKillerBodyIndex()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'RoR2.BodyIndex RoR2.CharacterMaster::GetKillerBodyIndex()' called on client");
			return default(BodyIndex);
		}
		return killerBodyIndex;
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		GlobalEventManager.onCharacterDeathGlobal += delegate(DamageReport damageReport)
		{
			CharacterMaster victimMaster = damageReport.victimMaster;
			if (Object.op_Implicit((Object)(object)victimMaster))
			{
				victimMaster.killerBodyIndex = BodyCatalog.FindBodyIndex(damageReport.damageInfo.attacker);
				victimMaster.killedByUnsafeArea = Object.op_Implicit((Object)(object)damageReport.damageInfo.inflictor) && Object.op_Implicit((Object)(object)damageReport.damageInfo.inflictor.GetComponent<MapZone>());
			}
		};
		Stage.onServerStageBegin += delegate
		{
			foreach (CharacterMaster instances in instancesList)
			{
				instances.preventRespawnUntilNextStageServer = false;
			}
		};
	}

	[Command]
	public void CmdRespawn(string bodyName)
	{
		if (preventRespawnUntilNextStageServer)
		{
			return;
		}
		if (!string.IsNullOrEmpty(bodyName))
		{
			bodyPrefab = BodyCatalog.FindBodyPrefab(bodyName);
			if (!Object.op_Implicit((Object)(object)bodyPrefab))
			{
				Debug.LogError((object)("CmdRespawn failed to find bodyPrefab for name '" + bodyName + "'."));
			}
		}
		if (Object.op_Implicit((Object)(object)Stage.instance))
		{
			Stage.instance.RespawnCharacter(this);
		}
	}

	[Server]
	public void TransformBody(string bodyName)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMaster::TransformBody(System.String)' called on client");
		}
		else if (!string.IsNullOrEmpty(bodyName))
		{
			bodyPrefab = BodyCatalog.FindBodyPrefab(bodyName);
			if ((Object)(object)bodyPrefab != (Object)null)
			{
				Transform component = bodyInstanceObject.GetComponent<Transform>();
				Vector3 position = component.position;
				Quaternion rotation = component.rotation;
				DestroyBody();
				CharacterBody component2 = bodyPrefab.GetComponent<CharacterBody>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					position = CalculateSafeGroundPosition(position, component2);
					SpawnBody(position, rotation);
				}
				else
				{
					Debug.LogErrorFormat("Trying to respawn as object {0} who has no Character Body!", new object[1] { bodyPrefab });
				}
			}
			else
			{
				Debug.LogError((object)("Can't TransformBody because there's no prefab for body named '" + bodyName + "'"));
			}
		}
		else
		{
			Debug.LogError((object)"Can't TransformBody with null or empty body name.");
		}
	}

	[Server]
	private void OnServerStageBegin(Stage stage)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMaster::OnServerStageBegin(RoR2.Stage)' called on client");
			return;
		}
		TryCloverVoidUpgrades();
		TryRegenerateScrap();
	}

	private void TryRegenerateScrap()
	{
		int itemCount = inventory.GetItemCount(DLC1Content.Items.RegeneratingScrapConsumed);
		if (itemCount > 0)
		{
			inventory.RemoveItem(DLC1Content.Items.RegeneratingScrapConsumed, itemCount);
			inventory.GiveItem(DLC1Content.Items.RegeneratingScrap, itemCount);
			CharacterMasterNotificationQueue.SendTransformNotification(this, DLC1Content.Items.RegeneratingScrapConsumed.itemIndex, DLC1Content.Items.RegeneratingScrap.itemIndex, CharacterMasterNotificationQueue.TransformationType.RegeneratingScrapRegen);
		}
	}

	[Server]
	private void TryCloverVoidUpgrades()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CharacterMaster::TryCloverVoidUpgrades()' called on client");
			return;
		}
		if (cloverVoidRng == null)
		{
			cloverVoidRng = new Xoroshiro128Plus(Run.instance.seed);
		}
		int itemCount = inventory.GetItemCount(DLC1Content.Items.CloverVoid);
		List<PickupIndex> list = new List<PickupIndex>(Run.instance.availableTier2DropList);
		List<PickupIndex> list2 = new List<PickupIndex>(Run.instance.availableTier3DropList);
		List<ItemIndex> list3 = new List<ItemIndex>(inventory.itemAcquisitionOrder);
		Util.ShuffleList(list3, cloverVoidRng);
		int num = itemCount * 3;
		int num2 = 0;
		int num3 = 0;
		while (num2 < num && num3 < list3.Count)
		{
			ItemDef startingItemDef = ItemCatalog.GetItemDef(list3[num3]);
			ItemDef itemDef = null;
			List<PickupIndex> list4 = null;
			switch (startingItemDef.tier)
			{
			case ItemTier.Tier1:
				list4 = list;
				break;
			case ItemTier.Tier2:
				list4 = list2;
				break;
			}
			if (list4 != null && list4.Count > 0)
			{
				Util.ShuffleList(list4, cloverVoidRng);
				list4.Sort(CompareTags);
				itemDef = ItemCatalog.GetItemDef(list4[0].itemIndex);
			}
			if ((Object)(object)itemDef != (Object)null)
			{
				if (inventory.GetItemCount(itemDef.itemIndex) == 0)
				{
					list3.Add(itemDef.itemIndex);
				}
				num2++;
				int itemCount2 = inventory.GetItemCount(startingItemDef.itemIndex);
				inventory.RemoveItem(startingItemDef.itemIndex, itemCount2);
				inventory.GiveItem(itemDef.itemIndex, itemCount2);
				CharacterMasterNotificationQueue.SendTransformNotification(this, startingItemDef.itemIndex, itemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.CloverVoid);
			}
			num3++;
			int CompareTags(PickupIndex lhs, PickupIndex rhs)
			{
				int num4 = 0;
				int num5 = 0;
				ItemDef itemDef2 = ItemCatalog.GetItemDef(lhs.itemIndex);
				ItemDef itemDef3 = ItemCatalog.GetItemDef(rhs.itemIndex);
				if (startingItemDef.ContainsTag(ItemTag.Damage))
				{
					if (itemDef2.ContainsTag(ItemTag.Damage))
					{
						num4 = 1;
					}
					if (itemDef3.ContainsTag(ItemTag.Damage))
					{
						num5 = 1;
					}
				}
				if (startingItemDef.ContainsTag(ItemTag.Healing))
				{
					if (itemDef2.ContainsTag(ItemTag.Healing))
					{
						num4 = 1;
					}
					if (itemDef3.ContainsTag(ItemTag.Healing))
					{
						num5 = 1;
					}
				}
				if (startingItemDef.ContainsTag(ItemTag.Utility))
				{
					if (itemDef2.ContainsTag(ItemTag.Utility))
					{
						num4 = 1;
					}
					if (itemDef3.ContainsTag(ItemTag.Utility))
					{
						num5 = 1;
					}
				}
				return num5 - num4;
			}
		}
		if (num2 > 0)
		{
			GameObject val = bodyInstanceObject;
			if (Object.op_Implicit((Object)(object)val))
			{
				Util.PlaySound("Play_item_proc_extraLife", val);
			}
		}
	}

	private static GameObject PickRandomSurvivorBodyPrefab(Xoroshiro128Plus rng, NetworkUser networkUser, bool allowHidden)
	{
		SurvivorDef[] array = SurvivorCatalog.allSurvivorDefs.Where(SurvivorIsUnlockedAndAvailable).ToArray();
		return rng.NextElementUniform<SurvivorDef>(array).bodyPrefab;
		bool SurvivorIsUnlockedAndAvailable(SurvivorDef survivorDef)
		{
			if (allowHidden || !survivorDef.hidden)
			{
				if (!survivorDef.CheckRequiredExpansionEnabled(networkUser))
				{
					return false;
				}
				UnlockableDef unlockableDef = survivorDef.unlockableDef;
				if (unlockableDef != null)
				{
					return networkUser.unlockables.Contains(unlockableDef);
				}
				return true;
			}
			return false;
		}
	}

	[Server]
	public CharacterBody Respawn(Vector3 footPosition, Quaternion rotation)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'RoR2.CharacterBody RoR2.CharacterMaster::Respawn(UnityEngine.Vector3,UnityEngine.Quaternion)' called on client");
			return null;
		}
		DestroyBody();
		if (Object.op_Implicit((Object)(object)playerCharacterMasterController) && RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.randomSurvivorOnRespawnArtifactDef))
		{
			bodyPrefab = PickRandomSurvivorBodyPrefab(Run.instance.randomSurvivorOnRespawnRng, playerCharacterMasterController.networkUser, allowHidden: false);
		}
		if (Object.op_Implicit((Object)(object)bodyPrefab))
		{
			CharacterBody component = bodyPrefab.GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Vector3 position = footPosition;
				if (true)
				{
					position = CalculateSafeGroundPosition(footPosition, component);
				}
				return SpawnBody(position, rotation);
			}
			Debug.LogErrorFormat("Trying to respawn as object {0} who has no Character Body!", new object[1] { bodyPrefab });
		}
		else
		{
			Debug.LogErrorFormat("CharacterMaster.Respawn failed. {0} does not have a valid body prefab assigned.", new object[1] { ((Object)((Component)this).gameObject).name });
		}
		return null;
	}

	private Vector3 CalculateSafeGroundPosition(Vector3 desiredFootPos, CharacterBody body)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)body))
		{
			Vector3 result = desiredFootPos;
			RaycastHit val = default(RaycastHit);
			Ray val2 = default(Ray);
			((Ray)(ref val2))._002Ector(desiredFootPos + Vector3.up * 2f, Vector3.down);
			float num = 4f;
			if (Physics.SphereCast(val2, body.radius, ref val, num, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				result.y = ((Ray)(ref val2)).origin.y - ((RaycastHit)(ref val)).distance;
			}
			float bodyPrefabFootOffset = Util.GetBodyPrefabFootOffset(bodyPrefab);
			result.y += bodyPrefabFootOffset;
			return result;
		}
		Debug.LogError((object)"Can't calculate safe ground position if the CharacterBody is null");
		return desiredFootPos;
	}

	private void SetUpGummyClone()
	{
		if (!NetworkServer.active || !Object.op_Implicit((Object)(object)inventory) || inventory.GetItemCount(DLC1Content.Items.GummyCloneIdentifier.itemIndex) <= 0)
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)((Component)this).gameObject.GetComponent<MasterSuicideOnTimer>()))
		{
			((Component)this).gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = 30f;
		}
		CharacterBody body = GetBody();
		if (Object.op_Implicit((Object)(object)body))
		{
			CharacterDeathBehavior component = ((Component)body).GetComponent<CharacterDeathBehavior>();
			if (Object.op_Implicit((Object)(object)component) && component.deathState.stateType != typeof(GummyCloneDeathState))
			{
				component.deathState = new SerializableEntityStateType(typeof(GummyCloneDeathState));
			}
			body.portraitIcon = LegacyResourcesAPI.Load<Texture>("Textures/BodyIcons/texGummyCloneBody");
		}
	}

	private void ToggleGod()
	{
		godMode = !godMode;
		UpdateBodyGodMode();
	}

	private void UpdateBodyGodMode()
	{
		if (Object.op_Implicit((Object)(object)bodyInstanceObject))
		{
			HealthComponent component = bodyInstanceObject.GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.godMode = godMode;
			}
		}
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		uint num = ((NetworkBehaviour)this).syncVarDirtyBits;
		if (initialState)
		{
			num = 127u;
		}
		bool num2 = (num & 1) != 0;
		bool flag = (num & 2) != 0;
		bool flag2 = (num & 0x40) != 0;
		bool flag3 = (num & 4) != 0;
		bool flag4 = (num & 8) != 0;
		bool flag5 = (num & 0x10) != 0;
		bool flag6 = (num & 0x20) != 0;
		writer.Write((byte)num);
		if (num2)
		{
			writer.Write(_bodyInstanceId);
		}
		if (flag)
		{
			writer.WritePackedUInt32(_money);
		}
		if (flag2)
		{
			writer.WritePackedUInt32(_voidCoins);
		}
		if (flag3)
		{
			writer.Write(_internalSurvivalTime);
		}
		if (flag4)
		{
			writer.Write(teamIndex);
		}
		if (flag5)
		{
			loadout.Serialize(writer);
		}
		if (flag6)
		{
			writer.WritePackedUInt32(miscFlags);
		}
		return num != 0;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		byte num = reader.ReadByte();
		bool flag = (num & 1) != 0;
		bool flag2 = (num & 2) != 0;
		bool flag3 = (num & 0x40) != 0;
		bool flag4 = (num & 4) != 0;
		bool flag5 = (num & 8) != 0;
		bool flag6 = (num & 0x10) != 0;
		bool num2 = (num & 0x20) != 0;
		if (flag)
		{
			NetworkInstanceId value = reader.ReadNetworkId();
			OnSyncBodyInstanceId(value);
		}
		if (flag2)
		{
			_money = reader.ReadPackedUInt32();
		}
		if (flag3)
		{
			_voidCoins = reader.ReadPackedUInt32();
		}
		if (flag4)
		{
			_internalSurvivalTime = reader.ReadSingle();
		}
		if (flag5)
		{
			teamIndex = reader.ReadTeamIndex();
		}
		if (flag6)
		{
			loadout.Deserialize(reader);
		}
		if (num2)
		{
			miscFlags = reader.ReadPackedUInt32();
		}
	}

	static CharacterMaster()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		instancesList = new List<CharacterMaster>();
		_readOnlyInstancesList = new ReadOnlyCollection<CharacterMaster>(instancesList);
		kCmdCmdRespawn = 1097984413;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterMaster), kCmdCmdRespawn, new CmdDelegate(InvokeCmdCmdRespawn));
		NetworkCRC.RegisterBehaviour("CharacterMaster", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdRespawn(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdRespawn called on client.");
		}
		else
		{
			((CharacterMaster)(object)obj).CmdRespawn(reader.ReadString());
		}
	}

	public void CallCmdRespawn(string bodyName)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdRespawn called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdRespawn(bodyName);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdRespawn);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(bodyName);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdRespawn");
	}

	public override void PreStartClient()
	{
	}
}
