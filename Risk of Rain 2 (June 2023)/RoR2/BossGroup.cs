using System;
using System.Collections.Generic;
using EntityStates;
using HG;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(CombatSquad))]
public class BossGroup : MonoBehaviour
{
	private struct BossMemory
	{
		public NetworkInstanceId masterInstanceId;

		public float maxObservedMaxHealth;

		public float lastObservedHealth;

		public CharacterMaster cachedMaster;

		public CharacterBody cachedBody;
	}

	private class DefeatBossObjectiveTracker : ObjectivePanelController.ObjectiveTracker
	{
		public DefeatBossObjectiveTracker()
		{
			baseToken = "OBJECTIVE_DEFEAT_BOSS";
		}
	}

	public class EnableHudHealthBarState : EntityState
	{
		private BossGroup bossGroup;

		public override void OnEnter()
		{
			base.OnEnter();
			bossGroup = GetComponent<BossGroup>();
			if (bossGroup != null)
			{
				bossGroup.shouldDisplayHealthBarOnHud = true;
			}
		}

		public override void OnExit()
		{
			if (bossGroup != null)
			{
				bossGroup.shouldDisplayHealthBarOnHud = false;
			}
			base.OnExit();
		}
	}

	public float bossDropChance = 0.15f;

	public Transform dropPosition;

	public PickupDropTable dropTable;

	public bool scaleRewardsByPlayerCount = true;

	[Tooltip("Whether or not this boss group should display a health bar on the HUD while any of its members are alive. Other scripts can change this at runtime to suppress a health bar until the boss is angered, for example. This field is not networked, so whatever is driving the value should be synchronized over the network.")]
	public bool shouldDisplayHealthBarOnHud = true;

	private Xoroshiro128Plus rng;

	private List<PickupDropTable> bossDropTables;

	private Run.FixedTimeStamp enabledTime;

	[Header("Deprecated")]
	public bool forceTier3Reward;

	private List<PickupIndex> bossDrops;

	private static readonly int initialBossMemoryCapacity = 8;

	private BossMemory[] bossMemories = new BossMemory[initialBossMemoryCapacity];

	private int bossMemoryCount;

	private static int lastTotalBossCount = 0;

	private static bool totalBossCountDirty = false;

	public float fixedAge => combatSquad.awakeTime.timeSince;

	public float fixedTimeSinceEnabled => enabledTime.timeSince;

	public int bonusRewardCount { get; set; }

	public CombatSquad combatSquad { get; private set; }

	public string bestObservedName { get; private set; } = "";


	public string bestObservedSubtitle { get; private set; } = "";


	public float totalMaxObservedMaxHealth { get; private set; }

	public float totalObservedHealth { get; private set; }

	public static event Action<BossGroup> onBossGroupStartServer;

	public static event Action<BossGroup> onBossGroupDefeatedServer;

	private void Awake()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		((Behaviour)this).enabled = false;
		combatSquad = ((Component)this).GetComponent<CombatSquad>();
		combatSquad.onMemberDiscovered += OnMemberDiscovered;
		combatSquad.onMemberLost += OnMemberLost;
		if (NetworkServer.active)
		{
			combatSquad.onDefeatedServer += OnDefeatedServer;
			combatSquad.onMemberAddedServer += OnMemberAddedServer;
			combatSquad.onMemberDefeatedServer += OnMemberDefeatedServer;
			rng = new Xoroshiro128Plus(Run.instance.bossRewardRng.nextUlong);
			bossDrops = new List<PickupIndex>();
			bossDropTables = new List<PickupDropTable>();
		}
	}

	private void Start()
	{
		if (NetworkServer.active)
		{
			BossGroup.onBossGroupStartServer?.Invoke(this);
		}
	}

	private void OnEnable()
	{
		InstanceTracker.Add(this);
		ObjectivePanelController.collectObjectiveSources += ReportObjective;
		enabledTime = Run.FixedTimeStamp.now;
	}

	private void OnDisable()
	{
		ObjectivePanelController.collectObjectiveSources -= ReportObjective;
		InstanceTracker.Remove(this);
	}

	private void FixedUpdate()
	{
		UpdateBossMemories();
	}

	private void OnDefeatedServer()
	{
		DropRewards();
		Run.instance.OnServerBossDefeated(this);
		BossGroup.onBossGroupDefeatedServer?.Invoke(this);
	}

	private void OnMemberAddedServer(CharacterMaster memberMaster)
	{
		Run.instance.OnServerBossAdded(this, memberMaster);
	}

	private void OnMemberDefeatedServer(CharacterMaster memberMaster, DamageReport damageReport)
	{
		GameObject bodyObject = memberMaster.GetBodyObject();
		DeathRewards deathRewards = ((bodyObject != null) ? bodyObject.GetComponent<DeathRewards>() : null);
		if (!Object.op_Implicit((Object)(object)deathRewards))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)deathRewards.bossDropTable))
		{
			bossDropTables.Add(deathRewards.bossDropTable);
			return;
		}
		PickupIndex pickupIndex = (PickupIndex)deathRewards.bossPickup;
		if (pickupIndex != PickupIndex.none)
		{
			bossDrops.Add(pickupIndex);
		}
	}

	private void OnMemberDiscovered(CharacterMaster memberMaster)
	{
		((Behaviour)this).enabled = true;
		memberMaster.isBoss = true;
		totalBossCountDirty = true;
		RememberBoss(memberMaster);
	}

	private void OnMemberLost(CharacterMaster memberMaster)
	{
		memberMaster.isBoss = false;
		totalBossCountDirty = true;
		if (combatSquad.memberCount == 0)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void DropRewards()
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)Run.instance))
		{
			Debug.LogError((object)"No valid run instance!");
			return;
		}
		if (rng == null)
		{
			Debug.LogError((object)"RNG is null!");
			return;
		}
		int participatingPlayerCount = Run.instance.participatingPlayerCount;
		if (participatingPlayerCount == 0)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)dropPosition))
		{
			PickupIndex none = PickupIndex.none;
			if (Object.op_Implicit((Object)(object)dropTable))
			{
				none = dropTable.GenerateDrop(rng);
			}
			else
			{
				List<PickupIndex> list = Run.instance.availableTier2DropList;
				if (forceTier3Reward)
				{
					list = Run.instance.availableTier3DropList;
				}
				none = rng.NextElementUniform<PickupIndex>(list);
			}
			int num = 1 + bonusRewardCount;
			if (scaleRewardsByPlayerCount)
			{
				num *= participatingPlayerCount;
			}
			float num2 = 360f / (float)num;
			Vector3 val = Quaternion.AngleAxis((float)Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
			Quaternion val2 = Quaternion.AngleAxis(num2, Vector3.up);
			bool flag = bossDrops != null && bossDrops.Count > 0;
			bool flag2 = bossDropTables != null && bossDropTables.Count > 0;
			int num3 = 0;
			while (num3 < num)
			{
				PickupIndex pickupIndex = none;
				if (bossDrops != null && (flag || flag2) && rng.nextNormalizedFloat <= bossDropChance)
				{
					if (flag2)
					{
						PickupDropTable pickupDropTable = rng.NextElementUniform<PickupDropTable>(bossDropTables);
						if ((Object)(object)pickupDropTable != (Object)null)
						{
							pickupIndex = pickupDropTable.GenerateDrop(rng);
						}
					}
					else
					{
						pickupIndex = rng.NextElementUniform<PickupIndex>(bossDrops);
					}
				}
				PickupDropletController.CreatePickupDroplet(pickupIndex, dropPosition.position, val);
				num3++;
				val = val2 * val;
			}
		}
		else
		{
			Debug.LogWarning((object)"dropPosition not set for BossGroup! No item will be spawned.");
		}
	}

	private int FindBossMemoryIndex(NetworkInstanceId id)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < bossMemoryCount; i++)
		{
			if (bossMemories[i].masterInstanceId == id)
			{
				return i;
			}
		}
		return -1;
	}

	private void RememberBoss(CharacterMaster master)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)master))
		{
			int num = FindBossMemoryIndex(((NetworkBehaviour)master).netId);
			if (num == -1)
			{
				num = AddBossMemory(master);
			}
			ref BossMemory reference = ref bossMemories[num];
			reference.cachedMaster = master;
			reference.cachedBody = master.GetBody();
			UpdateObservations(ref reference);
		}
	}

	private void UpdateObservations(ref BossMemory memory)
	{
		memory.lastObservedHealth = 0f;
		if (Object.op_Implicit((Object)(object)memory.cachedMaster) && !Object.op_Implicit((Object)(object)memory.cachedBody))
		{
			memory.cachedBody = memory.cachedMaster.GetBody();
		}
		if (!Object.op_Implicit((Object)(object)memory.cachedBody))
		{
			return;
		}
		if (bestObservedName.Length == 0 && bestObservedSubtitle.Length == 0 && Time.fixedDeltaTime * 3f < memory.cachedBody.localStartTime.timeSince)
		{
			bestObservedName = Util.GetBestBodyName(((Component)memory.cachedBody).gameObject);
			bestObservedSubtitle = memory.cachedBody.GetSubtitle();
			if (bestObservedSubtitle.Length == 0)
			{
				bestObservedSubtitle = Language.GetString("NULL_SUBTITLE");
			}
			bestObservedSubtitle = "<sprite name=\"CloudLeft\" tint=1> " + bestObservedSubtitle + "<sprite name=\"CloudRight\" tint=1>";
		}
		HealthComponent healthComponent = memory.cachedBody.healthComponent;
		memory.maxObservedMaxHealth = Mathf.Max(memory.maxObservedMaxHealth, healthComponent.fullCombinedHealth);
		memory.lastObservedHealth = healthComponent.combinedHealth;
	}

	private int AddBossMemory(CharacterMaster master)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		BossMemory bossMemory = default(BossMemory);
		bossMemory.masterInstanceId = ((NetworkBehaviour)master).netId;
		bossMemory.maxObservedMaxHealth = 0f;
		bossMemory.cachedMaster = master;
		BossMemory bossMemory2 = bossMemory;
		ArrayUtils.ArrayAppend<BossMemory>(ref bossMemories, ref bossMemoryCount, ref bossMemory2);
		return bossMemoryCount - 1;
	}

	private void UpdateBossMemories()
	{
		totalMaxObservedMaxHealth = 0f;
		totalObservedHealth = 0f;
		for (int i = 0; i < bossMemoryCount; i++)
		{
			ref BossMemory reference = ref bossMemories[i];
			UpdateObservations(ref reference);
			totalMaxObservedMaxHealth += reference.maxObservedMaxHealth;
			totalObservedHealth += Mathf.Max(reference.lastObservedHealth, 0f);
		}
	}

	public static int GetTotalBossCount()
	{
		if (totalBossCountDirty)
		{
			totalBossCountDirty = false;
			lastTotalBossCount = 0;
			List<BossGroup> instancesList = InstanceTracker.GetInstancesList<BossGroup>();
			for (int i = 0; i < instancesList.Count; i++)
			{
				lastTotalBossCount += instancesList[i].combatSquad.readOnlyMembersList.Count;
			}
		}
		return lastTotalBossCount;
	}

	public static BossGroup FindBossGroup(CharacterBody body)
	{
		if (!Object.op_Implicit((Object)(object)body) || !body.isBoss)
		{
			return null;
		}
		CharacterMaster master = body.master;
		if (!Object.op_Implicit((Object)(object)master))
		{
			return null;
		}
		List<BossGroup> instancesList = InstanceTracker.GetInstancesList<BossGroup>();
		for (int i = 0; i < instancesList.Count; i++)
		{
			BossGroup bossGroup = instancesList[i];
			if (bossGroup.combatSquad.ContainsMember(master))
			{
				return bossGroup;
			}
		}
		return null;
	}

	public void ReportObjective(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> output)
	{
		if (combatSquad.readOnlyMembersList.Count != 0)
		{
			output.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
			{
				source = (Object)(object)this,
				master = master,
				objectiveType = typeof(DefeatBossObjectiveTracker)
			});
		}
	}
}
