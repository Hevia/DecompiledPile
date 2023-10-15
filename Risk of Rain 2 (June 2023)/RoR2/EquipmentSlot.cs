using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RoR2.DirectionalSearch;
using RoR2.Orbs;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(CharacterBody))]
public class EquipmentSlot : NetworkBehaviour
{
	private struct UserTargetInfo
	{
		public readonly HurtBox hurtBox;

		public readonly GameObject rootObject;

		public readonly GenericPickupController pickupController;

		public readonly Transform transformToIndicateAt;

		public UserTargetInfo(HurtBox source)
		{
			hurtBox = source;
			rootObject = (Object.op_Implicit((Object)(object)hurtBox) ? ((Component)hurtBox.healthComponent).gameObject : null);
			pickupController = null;
			transformToIndicateAt = (Object.op_Implicit((Object)(object)hurtBox) ? ((Component)hurtBox).transform : null);
		}

		public UserTargetInfo(GenericPickupController source)
		{
			pickupController = source;
			hurtBox = null;
			rootObject = (Object.op_Implicit((Object)(object)pickupController) ? ((Component)pickupController).gameObject : null);
			transformToIndicateAt = (Object.op_Implicit((Object)(object)pickupController) ? ((Component)pickupController.pickupDisplay).transform : null);
		}
	}

	private Inventory inventory;

	private Run.FixedTimeStamp _rechargeTime;

	private bool hasEffectiveAuthority;

	private Xoroshiro128Plus rng;

	private HealthComponent healthComponent;

	private InputBankTest inputBank;

	private TeamComponent teamComponent;

	private const float fullCritDuration = 8f;

	private static readonly float tonicBuffDuration;

	public static string equipmentActivateString;

	private float missileTimer;

	private float bfgChargeTimer;

	private float subcooldownTimer;

	private const float missileInterval = 0.125f;

	private int remainingMissiles;

	private HealingFollowerController passiveHealingFollower;

	private GameObject goldgatControllerObject;

	private Indicator targetIndicator;

	private BullseyeSearch targetFinder = new BullseyeSearch();

	private UserTargetInfo currentTarget;

	private PickupSearch pickupSearch;

	private static int kRpcRpcOnClientEquipmentActivationRecieved;

	private static int kCmdCmdExecuteIfReady;

	private static int kCmdCmdOnEquipmentExecuted;

	public byte activeEquipmentSlot { get; private set; }

	public EquipmentIndex equipmentIndex { get; private set; }

	public int stock { get; private set; }

	public int maxStock { get; private set; }

	public CharacterBody characterBody { get; private set; }

	public float cooldownTimer => _rechargeTime.timeUntil;

	public static event Action<EquipmentSlot, EquipmentIndex> onServerEquipmentActivated;

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		UpdateAuthority();
	}

	public override void OnStartAuthority()
	{
		((NetworkBehaviour)this).OnStartAuthority();
		UpdateAuthority();
	}

	public override void OnStopAuthority()
	{
		((NetworkBehaviour)this).OnStopAuthority();
		UpdateAuthority();
	}

	private void UpdateAuthority()
	{
		hasEffectiveAuthority = Util.HasEffectiveAuthority(((Component)this).gameObject);
	}

	private void UpdateInventory()
	{
		inventory = characterBody.inventory;
		if (Object.op_Implicit((Object)(object)inventory))
		{
			activeEquipmentSlot = inventory.activeEquipmentSlot;
			equipmentIndex = inventory.GetEquipmentIndex();
			stock = inventory.GetEquipment(inventory.activeEquipmentSlot).charges;
			maxStock = inventory.GetActiveEquipmentMaxCharges();
			_rechargeTime = inventory.GetEquipment(inventory.activeEquipmentSlot).chargeFinishTime;
		}
		else
		{
			activeEquipmentSlot = 0;
			equipmentIndex = EquipmentIndex.None;
			stock = 0;
			maxStock = 0;
			_rechargeTime = Run.FixedTimeStamp.positiveInfinity;
		}
	}

	[Server]
	private void UpdateGoldGat()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.EquipmentSlot::UpdateGoldGat()' called on client");
			return;
		}
		bool flag = equipmentIndex == RoR2Content.Equipment.GoldGat.equipmentIndex;
		if (flag != Object.op_Implicit((Object)(object)goldgatControllerObject))
		{
			if (flag)
			{
				goldgatControllerObject = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/GoldGatController"));
				goldgatControllerObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(((Component)this).gameObject);
			}
			else
			{
				Object.Destroy((Object)(object)goldgatControllerObject);
			}
		}
	}

	public Transform FindActiveEquipmentDisplay()
	{
		ModelLocator component = ((Component)this).GetComponent<ModelLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform modelTransform = component.modelTransform;
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				CharacterModel component2 = ((Component)modelTransform).GetComponent<CharacterModel>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					List<GameObject> equipmentDisplayObjects = component2.GetEquipmentDisplayObjects(equipmentIndex);
					if (equipmentDisplayObjects.Count > 0)
					{
						return equipmentDisplayObjects[0].transform;
					}
				}
			}
		}
		return null;
	}

	[ClientRpc]
	private void RpcOnClientEquipmentActivationRecieved()
	{
		Util.PlaySound(equipmentActivateString, ((Component)this).gameObject);
		EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
		if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.DroneBackup)
		{
			Util.PlaySound("Play_item_use_radio", ((Component)this).gameObject);
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.BFG)
		{
			Transform val = FindActiveEquipmentDisplay();
			if (Object.op_Implicit((Object)(object)val))
			{
				Animator componentInChildren = ((Component)val).GetComponentInChildren<Animator>();
				if (Object.op_Implicit((Object)(object)componentInChildren))
				{
					componentInChildren.SetTrigger("Fire");
				}
			}
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.Blackhole)
		{
			Transform val2 = FindActiveEquipmentDisplay();
			if (Object.op_Implicit((Object)(object)val2))
			{
				GravCubeController component = ((Component)val2).GetComponent<GravCubeController>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.ActivateCube(9f);
				}
			}
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.CritOnUse)
		{
			Transform val3 = FindActiveEquipmentDisplay();
			if (Object.op_Implicit((Object)(object)val3))
			{
				Animator componentInChildren2 = ((Component)val3).GetComponentInChildren<Animator>();
				if (Object.op_Implicit((Object)(object)componentInChildren2))
				{
					componentInChildren2.SetBool("active", true);
					componentInChildren2.SetFloat("activeDuration", 8f);
					componentInChildren2.SetFloat("activeStopwatch", 0f);
				}
			}
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.GainArmor)
		{
			Util.PlaySound("Play_item_use_gainArmor", ((Component)this).gameObject);
		}
	}

	private void Start()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		characterBody = ((Component)this).GetComponent<CharacterBody>();
		healthComponent = ((Component)this).GetComponent<HealthComponent>();
		inputBank = ((Component)this).GetComponent<InputBankTest>();
		teamComponent = ((Component)this).GetComponent<TeamComponent>();
		targetIndicator = new Indicator(((Component)this).gameObject, null);
		rng = new Xoroshiro128Plus(Run.instance.seed ^ (ulong)Run.instance.stageClearCount);
	}

	private void OnDestroy()
	{
		if (targetIndicator != null)
		{
			targetIndicator.active = false;
		}
	}

	private void FixedUpdate()
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		UpdateInventory();
		if (NetworkServer.active)
		{
			EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
			subcooldownTimer -= Time.fixedDeltaTime;
			if (missileTimer > 0f)
			{
				missileTimer = Mathf.Max(missileTimer - Time.fixedDeltaTime, 0f);
			}
			if (missileTimer == 0f && remainingMissiles > 0)
			{
				remainingMissiles--;
				missileTimer = 0.125f;
				FireMissile();
			}
			UpdateGoldGat();
			if (bfgChargeTimer > 0f)
			{
				bfgChargeTimer -= Time.fixedDeltaTime;
				if (bfgChargeTimer < 0f)
				{
					_ = ((Component)this).transform.position;
					Ray aimRay = GetAimRay();
					Transform val = FindActiveEquipmentDisplay();
					if (Object.op_Implicit((Object)(object)val))
					{
						ChildLocator componentInChildren = ((Component)val).GetComponentInChildren<ChildLocator>();
						if (Object.op_Implicit((Object)(object)componentInChildren))
						{
							Transform val2 = componentInChildren.FindChild("Muzzle");
							if (Object.op_Implicit((Object)(object)val2))
							{
								aimRay.origin = val2.position;
							}
						}
					}
					healthComponent.TakeDamageForce(aimRay.direction * -1500f, alwaysApply: true);
					ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/BeamSphere"), aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), ((Component)this).gameObject, characterBody.damage * 2f, 0f, Util.CheckRoll(characterBody.crit, characterBody.master), DamageColorIndex.Item);
					bfgChargeTimer = 0f;
				}
			}
			if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.PassiveHealing != Object.op_Implicit((Object)(object)passiveHealingFollower))
			{
				if (!Object.op_Implicit((Object)(object)passiveHealingFollower))
				{
					GameObject val3 = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/HealingFollower"), ((Component)this).transform.position, Quaternion.identity);
					passiveHealingFollower = val3.GetComponent<HealingFollowerController>();
					passiveHealingFollower.NetworkownerBodyObject = ((Component)this).gameObject;
					NetworkServer.Spawn(val3);
				}
				else
				{
					Object.Destroy((Object)(object)((Component)passiveHealingFollower).gameObject);
					passiveHealingFollower = null;
				}
			}
		}
		bool num = inputBank.activateEquipment.justPressed || (inventory?.GetItemCount(RoR2Content.Items.AutoCastEquipment) ?? 0) > 0;
		bool isEquipmentActivationAllowed = characterBody.isEquipmentActivationAllowed;
		if (num && isEquipmentActivationAllowed && hasEffectiveAuthority)
		{
			if (NetworkServer.active)
			{
				ExecuteIfReady();
			}
			else
			{
				CallCmdExecuteIfReady();
			}
		}
	}

	[Command]
	private void CmdExecuteIfReady()
	{
		ExecuteIfReady();
	}

	[Server]
	public bool ExecuteIfReady()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.EquipmentSlot::ExecuteIfReady()' called on client");
			return false;
		}
		if (equipmentIndex != EquipmentIndex.None && stock > 0)
		{
			Execute();
			return true;
		}
		return false;
	}

	[Server]
	private void Execute()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.EquipmentSlot::Execute()' called on client");
			return;
		}
		EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
		if (equipmentDef != null && subcooldownTimer <= 0f && PerformEquipmentAction(equipmentDef))
		{
			OnEquipmentExecuted();
		}
	}

	[Command]
	public void CmdOnEquipmentExecuted()
	{
		OnEquipmentExecuted();
	}

	[Server]
	public void OnEquipmentExecuted()
	{
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.EquipmentSlot::OnEquipmentExecuted()' called on client");
			return;
		}
		EquipmentIndex arg = this.equipmentIndex;
		inventory.DeductEquipmentCharges(activeEquipmentSlot, 1);
		UpdateInventory();
		CallRpcOnClientEquipmentActivationRecieved();
		EquipmentSlot.onServerEquipmentActivated?.Invoke(this, arg);
		if (!Object.op_Implicit((Object)(object)characterBody) || !Object.op_Implicit((Object)(object)inventory))
		{
			return;
		}
		int itemCount = inventory.GetItemCount(RoR2Content.Items.EnergizedOnEquipmentUse);
		if (itemCount > 0)
		{
			characterBody.AddTimedBuff(RoR2Content.Buffs.Energized, 8 + 4 * (itemCount - 1));
		}
		int itemCount2 = inventory.GetItemCount(DLC1Content.Items.RandomEquipmentTrigger);
		if (itemCount2 <= 0 || EquipmentCatalog.randomTriggerEquipmentList.Count <= 0)
		{
			return;
		}
		List<EquipmentIndex> list = new List<EquipmentIndex>(EquipmentCatalog.randomTriggerEquipmentList);
		if (inventory.currentEquipmentIndex != EquipmentIndex.None)
		{
			list.Remove(inventory.currentEquipmentIndex);
		}
		Util.ShuffleList(list, rng);
		if (inventory.currentEquipmentIndex != EquipmentIndex.None)
		{
			list.Add(inventory.currentEquipmentIndex);
		}
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < itemCount2; i++)
		{
			if (flag2)
			{
				break;
			}
			EquipmentIndex equipmentIndex = EquipmentIndex.None;
			do
			{
				if (num >= list.Count)
				{
					if (!flag)
					{
						flag2 = true;
						break;
					}
					flag = false;
					num %= list.Count;
				}
				equipmentIndex = list[num];
				num++;
			}
			while (!PerformEquipmentAction(EquipmentCatalog.GetEquipmentDef(equipmentIndex)));
			if (equipmentIndex == RoR2Content.Equipment.BFG.equipmentIndex)
			{
				ModelLocator component = ((Component)this).GetComponent<ModelLocator>();
				if (Object.op_Implicit((Object)(object)component))
				{
					Transform modelTransform = component.modelTransform;
					if (Object.op_Implicit((Object)(object)modelTransform))
					{
						CharacterModel component2 = ((Component)modelTransform).GetComponent<CharacterModel>();
						if (Object.op_Implicit((Object)(object)component2))
						{
							List<GameObject> itemDisplayObjects = component2.GetItemDisplayObjects(DLC1Content.Items.RandomEquipmentTrigger.itemIndex);
							if (itemDisplayObjects.Count > 0)
							{
								Object.Instantiate<GameObject>(Addressables.LoadAssetAsync<GameObject>((object)"RoR2/Base/BFG/ChargeBFG.prefab").WaitForCompletion(), itemDisplayObjects[0].transform);
							}
						}
					}
				}
			}
			flag = true;
		}
		EffectData effectData = new EffectData();
		effectData.origin = characterBody.corePosition;
		effectData.SetNetworkedObjectReference(((Component)this).gameObject);
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/RandomEquipmentTriggerProcEffect"), effectData, transmit: true);
	}

	private void FireMissile()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		GameObject projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/MissileProjectile");
		float num = 3f;
		bool isCrit = Util.CheckRoll(characterBody.crit, characterBody.master);
		MissileUtils.FireMissile(characterBody.corePosition, characterBody, default(ProcChainMask), null, characterBody.damage * num, isCrit, projectilePrefab, DamageColorIndex.Item, addMissileProc: false);
	}

	[Server]
	private bool PerformEquipmentAction(EquipmentDef equipmentDef)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.EquipmentSlot::PerformEquipmentAction(RoR2.EquipmentDef)' called on client");
			return false;
		}
		Func<bool> func = null;
		if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.CommandMissile)
		{
			func = FireCommandMissile;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.Fruit)
		{
			func = FireFruit;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.DroneBackup)
		{
			func = FireDroneBackup;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.Meteor)
		{
			func = FireMeteor;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.Blackhole)
		{
			func = FireBlackhole;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.Saw)
		{
			func = FireSaw;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)JunkContent.Equipment.OrbitalLaser)
		{
			func = FireOrbitalLaser;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)JunkContent.Equipment.GhostGun)
		{
			func = FireGhostGun;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.CritOnUse)
		{
			func = FireCritOnUse;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.BFG)
		{
			func = FireBfg;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.Jetpack)
		{
			func = FireJetpack;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.Lightning)
		{
			func = FireLightning;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.PassiveHealing)
		{
			func = FirePassiveHealing;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.BurnNearby)
		{
			func = FireBurnNearby;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)JunkContent.Equipment.SoulCorruptor)
		{
			func = FireSoulCorruptor;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.Scanner)
		{
			func = FireScanner;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.CrippleWard)
		{
			func = FireCrippleWard;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.Gateway)
		{
			func = FireGateway;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.Tonic)
		{
			func = FireTonic;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.Cleanse)
		{
			func = FireCleanse;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.FireBallDash)
		{
			func = FireFireBallDash;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.Recycle)
		{
			func = FireRecycle;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.GainArmor)
		{
			func = FireGainArmor;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.LifestealOnHit)
		{
			func = FireLifeStealOnHit;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.TeamWarCry)
		{
			func = FireTeamWarCry;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)RoR2Content.Equipment.DeathProjectile)
		{
			func = FireDeathProjectile;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)DLC1Content.Equipment.Molotov)
		{
			func = FireMolotov;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)DLC1Content.Equipment.VendingMachine)
		{
			func = FireVendingMachine;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)DLC1Content.Equipment.BossHunter)
		{
			func = FireBossHunter;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)DLC1Content.Equipment.BossHunterConsumed)
		{
			func = FireBossHunterConsumed;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)DLC1Content.Equipment.GummyClone)
		{
			func = FireGummyClone;
		}
		else if ((Object)(object)equipmentDef == (Object)(object)DLC1Content.Equipment.LunarPortalOnUse)
		{
			func = FireLunarPortalOnUse;
		}
		return func?.Invoke() ?? false;
	}

	private Ray GetAimRay()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Ray result = default(Ray);
		((Ray)(ref result)).direction = inputBank.aimDirection;
		((Ray)(ref result)).origin = inputBank.aimOrigin;
		return result;
	}

	[Server]
	[CanBeNull]
	private CharacterMaster SummonMaster([NotNull] GameObject masterPrefab, Vector3 position, Quaternion rotation)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'RoR2.CharacterMaster RoR2.EquipmentSlot::SummonMaster(UnityEngine.GameObject,UnityEngine.Vector3,UnityEngine.Quaternion)' called on client");
			return null;
		}
		return new MasterSummon
		{
			masterPrefab = masterPrefab,
			position = position,
			rotation = rotation,
			summonerBodyObject = ((Component)this).gameObject,
			ignoreTeamMemberLimit = false
		}.Perform();
	}

	private void ConfigureTargetFinderBase()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		targetFinder.teamMaskFilter = TeamMask.allButNeutral;
		targetFinder.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
		targetFinder.sortMode = BullseyeSearch.SortMode.Angle;
		targetFinder.filterByLoS = true;
		float extraRaycastDistance;
		Ray val = CameraRigController.ModifyAimRayIfApplicable(GetAimRay(), ((Component)this).gameObject, out extraRaycastDistance);
		targetFinder.searchOrigin = ((Ray)(ref val)).origin;
		targetFinder.searchDirection = ((Ray)(ref val)).direction;
		targetFinder.maxAngleFilter = 10f;
		targetFinder.viewer = characterBody;
	}

	private void ConfigureTargetFinderForEnemies()
	{
		ConfigureTargetFinderBase();
		targetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(teamComponent.teamIndex);
		targetFinder.RefreshCandidates();
		targetFinder.FilterOutGameObject(((Component)this).gameObject);
	}

	private void ConfigureTargetFinderForBossesWithRewards()
	{
		ConfigureTargetFinderBase();
		targetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(teamComponent.teamIndex);
		targetFinder.RefreshCandidates();
		targetFinder.FilterOutGameObject(((Component)this).gameObject);
	}

	private void ConfigureTargetFinderForFriendlies()
	{
		ConfigureTargetFinderBase();
		targetFinder.teamMaskFilter = TeamMask.none;
		targetFinder.teamMaskFilter.AddTeam(teamComponent.teamIndex);
		targetFinder.RefreshCandidates();
		targetFinder.FilterOutGameObject(((Component)this).gameObject);
	}

	private void InvalidateCurrentTarget()
	{
		currentTarget = default(UserTargetInfo);
	}

	private void UpdateTargets(EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
	{
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		bool flag = targetingEquipmentIndex == DLC1Content.Equipment.BossHunter.equipmentIndex;
		bool flag2 = (targetingEquipmentIndex == RoR2Content.Equipment.Lightning.equipmentIndex || targetingEquipmentIndex == JunkContent.Equipment.SoulCorruptor.equipmentIndex || flag) && userShouldAnticipateTarget;
		bool flag3 = targetingEquipmentIndex == RoR2Content.Equipment.PassiveHealing.equipmentIndex && userShouldAnticipateTarget;
		bool num = flag2 || flag3;
		bool flag4 = targetingEquipmentIndex == RoR2Content.Equipment.Recycle.equipmentIndex;
		if (num)
		{
			if (flag2)
			{
				ConfigureTargetFinderForEnemies();
			}
			if (flag3)
			{
				ConfigureTargetFinderForFriendlies();
			}
			HurtBox source = null;
			if (flag)
			{
				foreach (HurtBox result in targetFinder.GetResults())
				{
					if (Object.op_Implicit((Object)(object)result) && Object.op_Implicit((Object)(object)result.healthComponent) && Object.op_Implicit((Object)(object)result.healthComponent.body))
					{
						DeathRewards component = ((Component)result.healthComponent.body).gameObject.GetComponent<DeathRewards>();
						if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.bossDropTable) && !result.healthComponent.body.HasBuff(RoR2Content.Buffs.Immune))
						{
							source = result;
							break;
						}
					}
				}
			}
			else
			{
				source = targetFinder.GetResults().FirstOrDefault();
			}
			currentTarget = new UserTargetInfo(source);
		}
		else if (flag4)
		{
			currentTarget = new UserTargetInfo(FindPickupController(GetAimRay(), 10f, 30f, requireLoS: true, targetingEquipmentIndex == RoR2Content.Equipment.Recycle.equipmentIndex));
		}
		else
		{
			currentTarget = default(UserTargetInfo);
		}
		GenericPickupController pickupController = currentTarget.pickupController;
		bool flag5 = Object.op_Implicit((Object)(object)currentTarget.transformToIndicateAt);
		if (flag5)
		{
			if (targetingEquipmentIndex == RoR2Content.Equipment.Lightning.equipmentIndex)
			{
				targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");
			}
			else if (targetingEquipmentIndex == RoR2Content.Equipment.PassiveHealing.equipmentIndex)
			{
				targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/WoodSpriteIndicator");
			}
			else if (targetingEquipmentIndex == RoR2Content.Equipment.Recycle.equipmentIndex)
			{
				if (!pickupController.Recycled)
				{
					targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerIndicator");
				}
				else
				{
					targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerBadIndicator");
				}
			}
			else if (targetingEquipmentIndex == DLC1Content.Equipment.BossHunter.equipmentIndex)
			{
				targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/BossHunterIndicator");
			}
			else
			{
				targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");
			}
		}
		targetIndicator.active = flag5;
		targetIndicator.targetTransform = (flag5 ? currentTarget.transformToIndicateAt : null);
	}

	private GenericPickupController FindPickupController(Ray aimRay, float maxAngle, float maxDistance, bool requireLoS, bool requireTransmutable)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (pickupSearch == null)
		{
			pickupSearch = new PickupSearch();
		}
		aimRay = CameraRigController.ModifyAimRayIfApplicable(aimRay, ((Component)this).gameObject, out var extraRaycastDistance);
		pickupSearch.searchOrigin = aimRay.origin;
		pickupSearch.searchDirection = aimRay.direction;
		pickupSearch.minAngleFilter = 0f;
		pickupSearch.maxAngleFilter = maxAngle;
		pickupSearch.minDistanceFilter = 0f;
		pickupSearch.maxDistanceFilter = maxDistance + extraRaycastDistance;
		pickupSearch.filterByDistinctEntity = false;
		pickupSearch.filterByLoS = requireLoS;
		pickupSearch.sortMode = SortMode.DistanceAndAngle;
		pickupSearch.requireTransmutable = requireTransmutable;
		return pickupSearch.SearchCandidatesForSingleTarget(InstanceTracker.GetInstancesList<GenericPickupController>());
	}

	private void Update()
	{
		UpdateTargets(equipmentIndex, stock > 0);
	}

	private bool FireCommandMissile()
	{
		remainingMissiles += 12;
		return true;
	}

	private bool FireFruit()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)healthComponent))
		{
			EffectData effectData = new EffectData();
			effectData.origin = ((Component)this).transform.position;
			effectData.SetNetworkedObjectReference(((Component)this).gameObject);
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FruitHealEffect"), effectData, transmit: true);
			healthComponent.HealFraction(0.5f, default(ProcChainMask));
		}
		return true;
	}

	private bool FireDroneBackup()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		int sliceCount = 4;
		float num = 25f;
		if (NetworkServer.active)
		{
			Ray aimRay = GetAimRay();
			Quaternion val = Quaternion.LookRotation(aimRay.direction);
			float y = ((Quaternion)(ref val)).eulerAngles.y;
			float num2 = 3f;
			foreach (float item in new DegreeSlices(sliceCount, 0.5f))
			{
				Quaternion val2 = Quaternion.Euler(-30f, y + item, 0f);
				Quaternion rotation = Quaternion.Euler(0f, y + item + 180f, 0f);
				Vector3 position = ((Component)this).transform.position + val2 * (Vector3.forward * num2);
				CharacterMaster characterMaster = SummonMaster(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/DroneBackupMaster"), position, rotation);
				if (Object.op_Implicit((Object)(object)characterMaster))
				{
					((Component)characterMaster).gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = num + Random.Range(0f, 3f);
				}
			}
		}
		subcooldownTimer = 0.5f;
		return true;
	}

	private bool FireMeteor()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		MeteorStormController component = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/MeteorStorm"), characterBody.corePosition, Quaternion.identity).GetComponent<MeteorStormController>();
		component.owner = ((Component)this).gameObject;
		component.ownerDamage = characterBody.damage;
		component.isCrit = Util.CheckRoll(characterBody.crit, characterBody.master);
		NetworkServer.Spawn(((Component)component).gameObject);
		return true;
	}

	private bool FireBlackhole()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		Ray aimRay = GetAimRay();
		ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/GravSphere"), position, Util.QuaternionSafeLookRotation(aimRay.direction), ((Component)this).gameObject, 0f, 0f, crit: false);
		return true;
	}

	private bool FireSaw()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		Quaternion val = Quaternion.LookRotation(aimRay.direction);
		float num = 15f;
		FireSingleSaw(characterBody, aimRay.origin, val * Quaternion.Euler(0f, 0f - num, 0f));
		FireSingleSaw(characterBody, aimRay.origin, val);
		FireSingleSaw(characterBody, aimRay.origin, val * Quaternion.Euler(0f, num, 0f));
		return true;
		void FireSingleSaw(CharacterBody firingCharacterBody, Vector3 origin, Quaternion rotation)
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			GameObject projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/Sawmerang");
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.projectilePrefab = projectilePrefab;
			fireProjectileInfo.crit = characterBody.RollCrit();
			fireProjectileInfo.damage = characterBody.damage;
			fireProjectileInfo.damageColorIndex = DamageColorIndex.Item;
			fireProjectileInfo.force = 0f;
			fireProjectileInfo.owner = ((Component)this).gameObject;
			fireProjectileInfo.position = origin;
			fireProjectileInfo.rotation = rotation;
			FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
			ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
		}
	}

	private bool FireOrbitalLaser()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(GetAimRay(), ref val2, 900f, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.defaultLayer.mask)))
		{
			val = ((RaycastHit)(ref val2)).point;
		}
		GameObject obj = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/OrbitalLaser"), val, Quaternion.identity);
		obj.GetComponent<OrbitalLaserController>().ownerBody = characterBody;
		NetworkServer.Spawn(obj);
		return true;
	}

	private bool FireGhostGun()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/GhostGun"), ((Component)this).transform.position, Quaternion.identity);
		obj.GetComponent<GhostGunController>().owner = ((Component)this).gameObject;
		NetworkServer.Spawn(obj);
		return true;
	}

	private bool FireCritOnUse()
	{
		characterBody.AddTimedBuff(RoR2Content.Buffs.FullCrit, 8f);
		return true;
	}

	private bool FireBfg()
	{
		bfgChargeTimer = 2f;
		subcooldownTimer = 2.2f;
		return true;
	}

	private bool FireJetpack()
	{
		JetpackController jetpackController = JetpackController.FindJetpackController(((Component)this).gameObject);
		if (!Object.op_Implicit((Object)(object)jetpackController))
		{
			Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BodyAttachments/JetpackController")).GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(((Component)this).gameObject);
		}
		else
		{
			jetpackController.ResetTimer();
		}
		return true;
	}

	private bool FireLightning()
	{
		UpdateTargets(RoR2Content.Equipment.Lightning.equipmentIndex, userShouldAnticipateTarget: true);
		HurtBox hurtBox = currentTarget.hurtBox;
		if (Object.op_Implicit((Object)(object)hurtBox))
		{
			subcooldownTimer = 0.2f;
			OrbManager.instance.AddOrb(new LightningStrikeOrb
			{
				attacker = ((Component)this).gameObject,
				damageColorIndex = DamageColorIndex.Item,
				damageValue = characterBody.damage * 30f,
				isCrit = Util.CheckRoll(characterBody.crit, characterBody.master),
				procChainMask = default(ProcChainMask),
				procCoefficient = 1f,
				target = hurtBox
			});
			InvalidateCurrentTarget();
			return true;
		}
		return false;
	}

	private bool FireBossHunter()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		UpdateTargets(DLC1Content.Equipment.BossHunter.equipmentIndex, userShouldAnticipateTarget: true);
		HurtBox hurtBox = currentTarget.hurtBox;
		object obj;
		if (hurtBox == null)
		{
			obj = null;
		}
		else
		{
			HealthComponent obj2 = hurtBox.healthComponent;
			if (obj2 == null)
			{
				obj = null;
			}
			else
			{
				CharacterBody body = obj2.body;
				if (body == null)
				{
					obj = null;
				}
				else
				{
					GameObject gameObject = ((Component)body).gameObject;
					obj = ((gameObject != null) ? gameObject.GetComponent<DeathRewards>() : null);
				}
			}
		}
		DeathRewards deathRewards = (DeathRewards)obj;
		if (Object.op_Implicit((Object)(object)hurtBox) && Object.op_Implicit((Object)(object)deathRewards))
		{
			Vector3 val = (Object.op_Implicit((Object)(object)((Component)hurtBox).transform) ? ((Component)hurtBox).transform.position : Vector3.zero);
			Vector3 val2 = val - characterBody.corePosition;
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			PickupDropletController.CreatePickupDroplet(deathRewards.bossDropTable.GenerateDrop(rng), val, normalized * 15f);
			if (Object.op_Implicit((Object)(object)hurtBox?.healthComponent?.body?.master))
			{
				hurtBox.healthComponent.body.master.TrueKill(((Component)this).gameObject);
			}
			CharacterModel component = ((Component)hurtBox.hurtBoxGroup).GetComponent<CharacterModel>();
			if (Object.op_Implicit((Object)(object)component))
			{
				TemporaryOverlay temporaryOverlay = ((Component)component).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = 0.1f;
				temporaryOverlay.animateShaderAlpha = true;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashBright");
				temporaryOverlay.AddToCharacerModel(component);
				TemporaryOverlay temporaryOverlay2 = ((Component)component).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay2.duration = 1.2f;
				temporaryOverlay2.animateShaderAlpha = true;
				temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay2.destroyComponentOnEnd = true;
				temporaryOverlay2.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matGhostEffect");
				temporaryOverlay2.AddToCharacerModel(component);
			}
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.attacker = ((Component)this).gameObject;
			damageInfo.force = -normalized * 2500f;
			healthComponent.TakeDamageForce(damageInfo, alwaysApply: true);
			GameObject effectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BossHunterKillEffect");
			Quaternion rotation = Util.QuaternionSafeLookRotation(normalized, Vector3.up);
			EffectManager.SpawnEffect(effectPrefab, new EffectData
			{
				origin = val,
				rotation = rotation
			}, transmit: true);
			ModelLocator component2 = ((Component)this).gameObject.GetComponent<ModelLocator>();
			object obj3;
			if (component2 == null)
			{
				obj3 = null;
			}
			else
			{
				Transform modelTransform = component2.modelTransform;
				obj3 = ((modelTransform != null) ? ((Component)modelTransform).GetComponent<CharacterModel>() : null);
			}
			CharacterModel characterModel = (CharacterModel)obj3;
			if (Object.op_Implicit((Object)(object)characterModel))
			{
				foreach (GameObject equipmentDisplayObject in characterModel.GetEquipmentDisplayObjects(DLC1Content.Equipment.BossHunter.equipmentIndex))
				{
					if (((Object)equipmentDisplayObject).name.Contains("DisplayTricorn"))
					{
						EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BossHunterHatEffect"), new EffectData
						{
							origin = equipmentDisplayObject.transform.position,
							rotation = equipmentDisplayObject.transform.rotation,
							scale = equipmentDisplayObject.transform.localScale.x
						}, transmit: true);
					}
					else
					{
						EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BossHunterGunEffect"), new EffectData
						{
							origin = equipmentDisplayObject.transform.position,
							rotation = Util.QuaternionSafeLookRotation(val - equipmentDisplayObject.transform.position, Vector3.up),
							scale = equipmentDisplayObject.transform.localScale.x
						}, transmit: true);
					}
				}
			}
			if (Object.op_Implicit((Object)(object)characterBody?.inventory))
			{
				CharacterMasterNotificationQueue.SendTransformNotification(characterBody.master, characterBody.inventory.currentEquipmentIndex, DLC1Content.Equipment.BossHunterConsumed.equipmentIndex, CharacterMasterNotificationQueue.TransformationType.Default);
				characterBody.inventory.SetEquipmentIndex(DLC1Content.Equipment.BossHunterConsumed.equipmentIndex);
			}
			InvalidateCurrentTarget();
			return true;
		}
		return false;
	}

	private bool FireBossHunterConsumed()
	{
		if (Object.op_Implicit((Object)(object)characterBody))
		{
			Chat.SendBroadcastChat(new Chat.BodyChatMessage
			{
				bodyObject = ((Component)characterBody).gameObject,
				token = "EQUIPMENT_BOSSHUNTERCONSUMED_CHAT"
			});
			subcooldownTimer = 1f;
		}
		return true;
	}

	private bool FirePassiveHealing()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		UpdateTargets(RoR2Content.Equipment.PassiveHealing.equipmentIndex, userShouldAnticipateTarget: true);
		GameObject rootObject = currentTarget.rootObject;
		CharacterBody characterBody = ((rootObject != null) ? rootObject.GetComponent<CharacterBody>() : null) ?? this.characterBody;
		if (Object.op_Implicit((Object)(object)characterBody))
		{
			EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/WoodSpriteHeal"), characterBody.corePosition, Vector3.up, transmit: true);
			characterBody.healthComponent?.HealFraction(0.1f, default(ProcChainMask));
		}
		if (Object.op_Implicit((Object)(object)passiveHealingFollower))
		{
			passiveHealingFollower.AssignNewTarget(currentTarget.rootObject);
			InvalidateCurrentTarget();
		}
		return true;
	}

	private bool FireBurnNearby()
	{
		if (Object.op_Implicit((Object)(object)characterBody))
		{
			characterBody.AddHelfireDuration(12f);
		}
		return true;
	}

	private bool FireSoulCorruptor()
	{
		UpdateTargets(JunkContent.Equipment.SoulCorruptor.equipmentIndex, userShouldAnticipateTarget: true);
		HurtBox hurtBox = currentTarget.hurtBox;
		if (!Object.op_Implicit((Object)(object)hurtBox))
		{
			return false;
		}
		if (!Object.op_Implicit((Object)(object)hurtBox.healthComponent) || hurtBox.healthComponent.combinedHealthFraction > 0.25f)
		{
			return false;
		}
		Util.TryToCreateGhost(hurtBox.healthComponent.body, characterBody, 30);
		hurtBox.healthComponent.Suicide(((Component)this).gameObject);
		InvalidateCurrentTarget();
		return true;
	}

	private bool FireScanner()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		NetworkServer.Spawn(Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/ChestScanner"), characterBody.corePosition, Quaternion.identity));
		return true;
	}

	private bool FireCrippleWard()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		characterBody.master.GetDeployableCount(DeployableSlot.PowerWard);
		Ray aimRay = GetAimRay();
		float num = 1000f;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(aimRay, ref val, num, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
		{
			GameObject obj = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/CrippleWard"), ((RaycastHit)(ref val)).point, Util.QuaternionSafeLookRotation(((RaycastHit)(ref val)).normal, Vector3.forward));
			Deployable component = obj.GetComponent<Deployable>();
			characterBody.master.AddDeployable(component, DeployableSlot.CrippleWard);
			NetworkServer.Spawn(obj);
			return true;
		}
		return true;
	}

	private bool FireTonic()
	{
		characterBody.AddTimedBuff(RoR2Content.Buffs.TonicBuff, tonicBuffDuration);
		if (!Util.CheckRoll(80f, characterBody.master))
		{
			characterBody.pendingTonicAfflictionCount++;
		}
		return true;
	}

	private bool FireCleanse()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Vector3 corePosition = characterBody.corePosition;
		EffectData effectData = new EffectData
		{
			origin = corePosition
		};
		effectData.SetHurtBoxReference(characterBody.mainHurtBox);
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/CleanseEffect"), effectData, transmit: true);
		Util.CleanseBody(characterBody, removeDebuffs: true, removeBuffs: false, removeCooldownBuffs: true, removeDots: true, removeStun: true, removeNearbyProjectiles: true);
		return true;
	}

	private bool FireFireBallDash()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/FireballVehicle"), aimRay.origin, Quaternion.LookRotation(aimRay.direction));
		val.GetComponent<VehicleSeat>().AssignPassenger(((Component)this).gameObject);
		NetworkUser networkUser = characterBody?.master?.playerCharacterMasterController?.networkUser;
		if (Object.op_Implicit((Object)(object)networkUser))
		{
			NetworkServer.SpawnWithClientAuthority(val, ((Component)networkUser).gameObject);
		}
		else
		{
			NetworkServer.Spawn(val);
		}
		subcooldownTimer = 2f;
		return true;
	}

	private bool FireGainArmor()
	{
		characterBody.AddTimedBuff(RoR2Content.Buffs.ElephantArmorBoost, 5f);
		return true;
	}

	private bool FireRecycle()
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		UpdateTargets(RoR2Content.Equipment.Recycle.equipmentIndex, userShouldAnticipateTarget: false);
		GenericPickupController pickupController = currentTarget.pickupController;
		if (Object.op_Implicit((Object)(object)pickupController) && !pickupController.Recycled)
		{
			PickupIndex initialPickupIndex = pickupController.pickupIndex;
			subcooldownTimer = 0.2f;
			PickupIndex[] array = (from pickupIndex in PickupTransmutationManager.GetAvailableGroupFromPickupIndex(pickupController.pickupIndex)
				where pickupIndex != initialPickupIndex
				select pickupIndex).ToArray();
			if (array == null)
			{
				return false;
			}
			if (array.Length == 0)
			{
				return false;
			}
			pickupController.NetworkpickupIndex = Run.instance.treasureRng.NextElementUniform<PickupIndex>(array);
			EffectManager.SimpleEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniRecycleEffect"), ((Component)pickupController.pickupDisplay).transform.position, Quaternion.identity, transmit: true);
			pickupController.NetworkRecycled = true;
			InvalidateCurrentTarget();
			return true;
		}
		return false;
	}

	private bool FireGateway()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		_ = characterBody.footPosition;
		Ray aimRay = GetAimRay();
		float num = 2f;
		float num2 = num * 2f;
		float num3 = 1000f;
		Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return false;
		}
		Vector3 position = ((Component)this).transform.position;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(aimRay, ref val, num3, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
		{
			Vector3 val2 = ((RaycastHit)(ref val)).point + ((RaycastHit)(ref val)).normal * num;
			Vector3 val3 = val2 - position;
			Vector3 normalized = ((Vector3)(ref val3)).normalized;
			Vector3 pointBPosition = val2;
			RaycastHit val4 = default(RaycastHit);
			if (component.SweepTest(normalized, ref val4, ((Vector3)(ref val3)).magnitude))
			{
				if (((RaycastHit)(ref val4)).distance < num2)
				{
					return false;
				}
				pointBPosition = position + normalized * ((RaycastHit)(ref val4)).distance;
			}
			GameObject obj = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/Zipline"));
			ZiplineController component2 = obj.GetComponent<ZiplineController>();
			component2.SetPointAPosition(position + normalized * num);
			component2.SetPointBPosition(pointBPosition);
			obj.AddComponent<DestroyOnTimer>().duration = 30f;
			NetworkServer.Spawn(obj);
			return true;
		}
		return false;
	}

	private bool FireLifeStealOnHit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData
		{
			origin = characterBody.corePosition
		};
		effectData.SetHurtBoxReference(((Component)characterBody).gameObject);
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/LifeStealOnHitActivation"), effectData, transmit: false);
		characterBody.AddTimedBuff(RoR2Content.Buffs.LifeSteal, 8f);
		return true;
	}

	private bool FireTeamWarCry()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound("Play_teamWarCry_activate", ((Component)characterBody).gameObject);
		Vector3 corePosition = characterBody.corePosition;
		EffectData effectData = new EffectData
		{
			origin = corePosition
		};
		effectData.SetNetworkedObjectReference(((Component)characterBody).gameObject);
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TeamWarCryActivation"), effectData, transmit: true);
		characterBody.AddTimedBuff(RoR2Content.Buffs.TeamWarCry, 7f);
		TeamComponent[] array = Object.FindObjectsOfType<TeamComponent>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].teamIndex == teamComponent.teamIndex)
			{
				((Component)array[i]).GetComponent<CharacterBody>().AddTimedBuff(RoR2Content.Buffs.TeamWarCry, 7f);
			}
		}
		return true;
	}

	private bool FireDeathProjectile()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		CharacterMaster master = characterBody.master;
		if (!Object.op_Implicit((Object)(object)master))
		{
			return false;
		}
		if (master.IsDeployableLimited(DeployableSlot.DeathProjectile))
		{
			return false;
		}
		Ray aimRay = GetAimRay();
		Quaternion rotation = Quaternion.LookRotation(aimRay.direction);
		GameObject val = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/DeathProjectile");
		val.GetComponent<DeathProjectile>().teamIndex = teamComponent.teamIndex;
		FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
		fireProjectileInfo.projectilePrefab = val;
		fireProjectileInfo.crit = characterBody.RollCrit();
		fireProjectileInfo.damage = characterBody.damage;
		fireProjectileInfo.damageColorIndex = DamageColorIndex.Item;
		fireProjectileInfo.force = 0f;
		fireProjectileInfo.owner = ((Component)this).gameObject;
		fireProjectileInfo.position = aimRay.origin;
		fireProjectileInfo.rotation = rotation;
		FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
		ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
		return true;
	}

	private bool FireMolotov()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		GameObject prefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/MolotovClusterProjectile");
		ProjectileManager.instance.FireProjectile(prefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), ((Component)this).gameObject, characterBody.damage, 0f, Util.CheckRoll(characterBody.crit, characterBody.master));
		return true;
	}

	private bool FireVendingMachine()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		Ray ray = default(Ray);
		((Ray)(ref ray))._002Ector(aimRay.origin, Vector3.down);
		if (Util.CharacterRaycast(((Component)this).gameObject, ray, out var hitInfo, 1000f, LayerIndex.world.mask, (QueryTriggerInteraction)0))
		{
			GameObject prefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/VendingMachineProjectile");
			ProjectileManager.instance.FireProjectile(prefab, ((RaycastHit)(ref hitInfo)).point, Quaternion.identity, ((Component)this).gameObject, characterBody.damage, 0f, Util.CheckRoll(characterBody.crit, characterBody.master));
			subcooldownTimer = 0.5f;
			return true;
		}
		return false;
	}

	private bool FireGummyClone()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		CharacterMaster characterMaster = characterBody?.master;
		if (!Object.op_Implicit((Object)(object)characterMaster) || characterMaster.IsDeployableLimited(DeployableSlot.GummyClone))
		{
			return false;
		}
		Ray aimRay = GetAimRay();
		Quaternion rotation = Quaternion.LookRotation(aimRay.direction);
		GameObject projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/GummyCloneProjectile");
		FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
		fireProjectileInfo.projectilePrefab = projectilePrefab;
		fireProjectileInfo.crit = characterBody.RollCrit();
		fireProjectileInfo.damage = 0f;
		fireProjectileInfo.damageColorIndex = DamageColorIndex.Item;
		fireProjectileInfo.force = 0f;
		fireProjectileInfo.owner = ((Component)this).gameObject;
		fireProjectileInfo.position = aimRay.origin;
		fireProjectileInfo.rotation = rotation;
		FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
		ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
		return true;
	}

	private bool FireLunarPortalOnUse()
	{
		TeleporterInteraction.instance.shouldAttemptToSpawnShopPortal = true;
		return true;
	}

	static EquipmentSlot()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		tonicBuffDuration = 20f;
		equipmentActivateString = "Play_UI_equipment_activate";
		kCmdCmdExecuteIfReady = -303452611;
		NetworkBehaviour.RegisterCommandDelegate(typeof(EquipmentSlot), kCmdCmdExecuteIfReady, new CmdDelegate(InvokeCmdCmdExecuteIfReady));
		kCmdCmdOnEquipmentExecuted = 1725820338;
		NetworkBehaviour.RegisterCommandDelegate(typeof(EquipmentSlot), kCmdCmdOnEquipmentExecuted, new CmdDelegate(InvokeCmdCmdOnEquipmentExecuted));
		kRpcRpcOnClientEquipmentActivationRecieved = 1342577121;
		NetworkBehaviour.RegisterRpcDelegate(typeof(EquipmentSlot), kRpcRpcOnClientEquipmentActivationRecieved, new CmdDelegate(InvokeRpcRpcOnClientEquipmentActivationRecieved));
		NetworkCRC.RegisterBehaviour("EquipmentSlot", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdExecuteIfReady(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdExecuteIfReady called on client.");
		}
		else
		{
			((EquipmentSlot)(object)obj).CmdExecuteIfReady();
		}
	}

	protected static void InvokeCmdCmdOnEquipmentExecuted(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdOnEquipmentExecuted called on client.");
		}
		else
		{
			((EquipmentSlot)(object)obj).CmdOnEquipmentExecuted();
		}
	}

	public void CallCmdExecuteIfReady()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdExecuteIfReady called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdExecuteIfReady();
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdExecuteIfReady);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdExecuteIfReady");
	}

	public void CallCmdOnEquipmentExecuted()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdOnEquipmentExecuted called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdOnEquipmentExecuted();
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdOnEquipmentExecuted);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdOnEquipmentExecuted");
	}

	protected static void InvokeRpcRpcOnClientEquipmentActivationRecieved(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcOnClientEquipmentActivationRecieved called on server.");
		}
		else
		{
			((EquipmentSlot)(object)obj).RpcOnClientEquipmentActivationRecieved();
		}
	}

	public void CallRpcOnClientEquipmentActivationRecieved()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcOnClientEquipmentActivationRecieved called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcOnClientEquipmentActivationRecieved);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcOnClientEquipmentActivationRecieved");
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
