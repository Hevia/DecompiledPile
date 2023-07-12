using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EntityStates;
using JetBrains.Annotations;
using RoR2.Audio;
using RoR2.Networking;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(CharacterBody))]
[DisallowMultipleComponent]
public class HealthComponent : NetworkBehaviour
{
	private static class AssetReferences
	{
		public static GameObject bearEffectPrefab;

		public static GameObject bearVoidEffectPrefab;

		public static GameObject executeEffectPrefab;

		public static GameObject critGlassesVoidExecuteEffectPrefab;

		public static GameObject shieldBreakEffectPrefab;

		public static GameObject loseCoinsImpactEffectPrefab;

		public static GameObject gainCoinsImpactEffectPrefab;

		public static GameObject damageRejectedPrefab;

		public static GameObject bossDamageBonusImpactEffectPrefab;

		public static GameObject pulverizedEffectPrefab;

		public static GameObject diamondDamageBonusImpactEffectPrefab;

		public static GameObject crowbarImpactEffectPrefab;

		public static GameObject captainBodyArmorBlockEffectPrefab;

		public static GameObject mercExposeConsumeEffectPrefab;

		public static GameObject explodeOnDeathVoidExplosionPrefab;

		public static GameObject fragileDamageBonusBreakEffectPrefab;

		public static GameObject permanentDebuffEffectPrefab;

		public static void Resolve()
		{
			bearEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BearProc");
			bearVoidEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BearVoidProc");
			executeEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniImpactExecute");
			shieldBreakEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShieldBreakEffect");
			loseCoinsImpactEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CoinImpact");
			gainCoinsImpactEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/GainCoinsImpact");
			damageRejectedPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/DamageRejected");
			bossDamageBonusImpactEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactBossDamageBonus");
			pulverizedEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/PulverizedEffect");
			diamondDamageBonusImpactEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/DiamondDamageBonusEffect");
			crowbarImpactEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactCrowbar");
			captainBodyArmorBlockEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CaptainBodyArmorBlockEffect");
			permanentDebuffEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/PermanentDebuffEffect");
			mercExposeConsumeEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/MercExposeConsumeEffect");
			critGlassesVoidExecuteEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/CritGlassesVoidExecuteEffect");
			explodeOnDeathVoidExplosionPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/ExplodeOnDeathVoidExplosion");
			fragileDamageBonusBreakEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/DelicateWatchProcEffect");
		}
	}

	private class HealMessage : MessageBase
	{
		public GameObject target;

		public float amount;

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(target);
			writer.Write(amount);
		}

		public override void Deserialize(NetworkReader reader)
		{
			target = reader.ReadGameObject();
			amount = reader.ReadSingle();
		}
	}

	private struct ItemCounts
	{
		public int bear;

		public int armorPlate;

		public int goldOnHit;

		public int goldOnHurt;

		public int phasing;

		public int thorns;

		public int invadingDoppelganger;

		public int medkit;

		public int parentEgg;

		public int fragileDamageBonus;

		public int minHealthPercentage;

		public int increaseHealing;

		public int barrierOnOverHeal;

		public int repeatHeal;

		public int novaOnHeal;

		public int adaptiveArmor;

		public int healingPotion;

		public int infusion;

		public int missileVoid;

		public ItemCounts([NotNull] Inventory src)
		{
			bear = src.GetItemCount(RoR2Content.Items.Bear);
			armorPlate = src.GetItemCount(RoR2Content.Items.ArmorPlate);
			goldOnHit = src.GetItemCount(RoR2Content.Items.GoldOnHit);
			goldOnHurt = src.GetItemCount(DLC1Content.Items.GoldOnHurt);
			phasing = src.GetItemCount(RoR2Content.Items.Phasing);
			thorns = src.GetItemCount(RoR2Content.Items.Thorns);
			invadingDoppelganger = src.GetItemCount(RoR2Content.Items.InvadingDoppelganger);
			medkit = src.GetItemCount(RoR2Content.Items.Medkit);
			fragileDamageBonus = src.GetItemCount(DLC1Content.Items.FragileDamageBonus);
			minHealthPercentage = src.GetItemCount(RoR2Content.Items.MinHealthPercentage);
			increaseHealing = src.GetItemCount(RoR2Content.Items.IncreaseHealing);
			barrierOnOverHeal = src.GetItemCount(RoR2Content.Items.BarrierOnOverHeal);
			repeatHeal = src.GetItemCount(RoR2Content.Items.RepeatHeal);
			novaOnHeal = src.GetItemCount(RoR2Content.Items.NovaOnHeal);
			adaptiveArmor = src.GetItemCount(RoR2Content.Items.AdaptiveArmor);
			healingPotion = src.GetItemCount(DLC1Content.Items.HealingPotion);
			infusion = src.GetItemCount(RoR2Content.Items.Infusion);
			parentEgg = src.GetItemCount(RoR2Content.Items.ParentEgg);
			missileVoid = src.GetItemCount(DLC1Content.Items.MissileVoid);
		}
	}

	public struct HealthBarValues
	{
		public bool hasInfusion;

		public bool hasVoidShields;

		public bool isVoid;

		public bool isElite;

		public bool isBoss;

		public float cullFraction;

		public float healthFraction;

		public float shieldFraction;

		public float barrierFraction;

		public float magneticFraction;

		public float curseFraction;

		public float ospFraction;

		public int healthDisplayValue;

		public int maxHealthDisplayValue;
	}

	private class RepeatHealComponent : MonoBehaviour
	{
		private float reserve;

		private float timer;

		private const float interval = 0.2f;

		public float healthFractionToRestorePerSecond = 0.1f;

		public HealthComponent healthComponent;

		private void FixedUpdate()
		{
			timer -= Time.fixedDeltaTime;
			if (timer <= 0f)
			{
				timer = 0.2f;
				if (reserve > 0f)
				{
					float num = Mathf.Min(healthComponent.fullHealth * healthFractionToRestorePerSecond * 0.2f, reserve);
					reserve -= num;
					ProcChainMask procChainMask = default(ProcChainMask);
					procChainMask.AddProc(ProcType.RepeatHeal);
					healthComponent.Heal(num, procChainMask);
				}
			}
		}

		public void AddReserve(float amount, float max)
		{
			reserve = Mathf.Min(reserve + amount, max);
		}
	}

	public static readonly float lowHealthFraction;

	[Tooltip("How much health this object has.")]
	[HideInInspector]
	[SyncVar]
	public float health = 100f;

	[Tooltip("How much shield this object has.")]
	[HideInInspector]
	[SyncVar]
	public float shield;

	[Tooltip("How much barrier this object has.")]
	[SyncVar]
	[HideInInspector]
	public float barrier;

	[SyncVar]
	[HideInInspector]
	public float magnetiCharge;

	public bool dontShowHealthbar;

	public float globalDeathEventChanceCoefficient = 1f;

	[SyncVar]
	private uint _killingDamageType;

	public CharacterBody body;

	private ModelLocator modelLocator;

	private IPainAnimationHandler painAnimationHandler;

	private IOnIncomingDamageServerReceiver[] onIncomingDamageReceivers;

	private IOnTakeDamageServerReceiver[] onTakeDamageReceivers;

	public const float frozenExecuteThreshold = 0.3f;

	private const float adaptiveArmorPerOnePercentTaken = 30f;

	private const float adaptiveArmorDecayPerSecond = 40f;

	private const float adaptiveArmorCap = 400f;

	public const float medkitActivationDelay = 2f;

	private const float devilOrbMaxTimer = 0.1f;

	private float devilOrbHealPool;

	private float devilOrbTimer;

	private float regenAccumulator;

	private bool wasAlive = true;

	private float adaptiveArmorValue;

	private bool isShieldRegenForced;

	private float ospTimer;

	private const float ospBufferDuration = 0.1f;

	private float serverDamageTakenThisUpdate;

	private RepeatHealComponent repeatHealComponent;

	private ItemCounts itemCounts;

	private EquipmentIndex currentEquipmentIndex;

	private static int kCmdCmdHealFull;

	private static int kCmdCmdRechargeShieldFull;

	private static int kCmdCmdAddBarrier;

	private static int kCmdCmdForceShieldRegen;

	public DamageType killingDamageType
	{
		get
		{
			return (DamageType)_killingDamageType;
		}
		private set
		{
			Network_killingDamageType = (uint)value;
		}
	}

	public bool alive => health > 0f;

	public float fullHealth => body.maxHealth;

	public float fullShield => body.maxShield;

	public float fullBarrier => body.maxBarrier;

	public float combinedHealth => health + shield + barrier;

	public float fullCombinedHealth => fullHealth + fullShield;

	public float combinedHealthFraction => combinedHealth / fullCombinedHealth;

	public float missingCombinedHealth => fullCombinedHealth - (combinedHealth - barrier);

	public Run.FixedTimeStamp lastHitTime { get; private set; }

	public Run.FixedTimeStamp lastHealTime { get; private set; }

	public GameObject lastHitAttacker { get; private set; }

	public float timeSinceLastHit => lastHitTime.timeSince;

	public float timeSinceLastHeal => lastHealTime.timeSince;

	public bool godMode { get; set; }

	public float potionReserve { get; private set; }

	public bool isInFrozenState { get; set; }

	public bool isHealthLow => (health + shield) / fullCombinedHealth <= lowHealthFraction;

	public float Networkhealth
	{
		get
		{
			return health;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref health, 1u);
		}
	}

	public float Networkshield
	{
		get
		{
			return shield;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref shield, 2u);
		}
	}

	public float Networkbarrier
	{
		get
		{
			return barrier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref barrier, 4u);
		}
	}

	public float NetworkmagnetiCharge
	{
		get
		{
			return magnetiCharge;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref magnetiCharge, 8u);
		}
	}

	public uint Network_killingDamageType
	{
		get
		{
			return _killingDamageType;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<uint>(value, ref _killingDamageType, 16u);
		}
	}

	public static event Action<HealthComponent, float, ProcChainMask> onCharacterHealServer;

	public void OnValidate()
	{
		if (((Component)this).gameObject.GetComponents<HealthComponent>().Length > 1)
		{
			Debug.LogErrorFormat((Object)(object)((Component)this).gameObject, "{0} has multiple health components!!", new object[1] { ((Component)this).gameObject });
		}
	}

	public float Heal(float amount, ProcChainMask procChainMask, bool nonRegen = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Single RoR2.HealthComponent::Heal(System.Single, RoR2.ProcChainMask, System.Boolean)' called on client");
			return 0f;
		}
		if (!alive || amount <= 0f || body.HasBuff(RoR2Content.Buffs.HealingDisabled))
		{
			return 0f;
		}
		float num = health;
		bool flag = false;
		if (currentEquipmentIndex == RoR2Content.Equipment.LunarPotion.equipmentIndex && !procChainMask.HasProc(ProcType.LunarPotionActivation))
		{
			potionReserve += amount;
			return amount;
		}
		if (nonRegen && !procChainMask.HasProc(ProcType.CritHeal) && Util.CheckRoll(body.critHeal, body.master))
		{
			procChainMask.AddProc(ProcType.CritHeal);
			flag = true;
		}
		if (flag)
		{
			amount *= 2f;
		}
		if (itemCounts.increaseHealing > 0)
		{
			amount *= 1f + (float)itemCounts.increaseHealing;
		}
		if (body.teamComponent.teamIndex == TeamIndex.Player && Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse5)
		{
			amount /= 2f;
		}
		if (nonRegen && Object.op_Implicit((Object)(object)repeatHealComponent) && !procChainMask.HasProc(ProcType.RepeatHeal))
		{
			repeatHealComponent.healthFractionToRestorePerSecond = 0.1f / (float)itemCounts.repeatHeal;
			repeatHealComponent.AddReserve(amount * (float)(1 + itemCounts.repeatHeal), fullHealth);
			return 0f;
		}
		float num2 = amount;
		if (health < fullHealth)
		{
			float num3 = Mathf.Max(Mathf.Min(amount, fullHealth - health), 0f);
			num2 = amount - num3;
			Networkhealth = health + num3;
		}
		if (num2 > 0f && nonRegen && itemCounts.barrierOnOverHeal > 0)
		{
			float value = num2 * ((float)itemCounts.barrierOnOverHeal * 0.5f);
			AddBarrier(value);
		}
		if (nonRegen)
		{
			lastHealTime = Run.FixedTimeStamp.now;
			SendHeal(((Component)this).gameObject, amount, flag);
			if (itemCounts.novaOnHeal > 0 && !procChainMask.HasProc(ProcType.HealNova))
			{
				devilOrbHealPool = Mathf.Min(devilOrbHealPool + amount * (float)itemCounts.novaOnHeal, fullCombinedHealth);
			}
		}
		if (flag)
		{
			GlobalEventManager.instance.OnCrit(body, null, body.master, amount / fullHealth * 10f, procChainMask);
		}
		if (nonRegen)
		{
			HealthComponent.onCharacterHealServer?.Invoke(this, amount, procChainMask);
		}
		return health - num;
	}

	public void UsePotion()
	{
		ProcChainMask procChainMask = default(ProcChainMask);
		procChainMask.AddProc(ProcType.LunarPotionActivation);
		Heal(potionReserve, procChainMask);
	}

	public float HealFraction(float fraction, ProcChainMask procChainMask)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Single RoR2.HealthComponent::HealFraction(System.Single, RoR2.ProcChainMask)' called on client");
			return 0f;
		}
		return Heal(fraction * fullHealth, procChainMask);
	}

	[Command]
	public void CmdHealFull()
	{
		HealFraction(1f, default(ProcChainMask));
	}

	[Server]
	public void RechargeShieldFull()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealthComponent::RechargeShieldFull()' called on client");
		}
		else if (shield < fullShield)
		{
			Networkshield = fullShield;
		}
	}

	[Command]
	public void CmdRechargeShieldFull()
	{
		RechargeShieldFull();
	}

	[Server]
	public void RechargeShield(float value)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealthComponent::RechargeShield(System.Single)' called on client");
		}
		else if (shield < fullShield)
		{
			Networkshield = shield + value;
			if (shield > fullShield)
			{
				Networkshield = fullShield;
			}
		}
	}

	[Server]
	public void AddBarrier(float value)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealthComponent::AddBarrier(System.Single)' called on client");
		}
		else if (alive && barrier < fullBarrier)
		{
			Networkbarrier = Mathf.Min(barrier + value, fullBarrier);
		}
	}

	[Server]
	public void AddCharge(float value)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealthComponent::AddCharge(System.Single)' called on client");
		}
		else if (alive && magnetiCharge < fullHealth)
		{
			NetworkmagnetiCharge = Mathf.Min(barrier + value, fullBarrier);
		}
	}

	[Command]
	private void CmdAddBarrier(float value)
	{
		AddBarrier(value);
	}

	public void AddBarrierAuthority(float value)
	{
		if (NetworkServer.active)
		{
			AddBarrier(value);
		}
		else
		{
			CallCmdAddBarrier(value);
		}
	}

	[Command]
	private void CmdForceShieldRegen()
	{
		ForceShieldRegen();
	}

	public void ForceShieldRegen()
	{
		if (NetworkServer.active)
		{
			isShieldRegenForced = true;
		}
		else
		{
			CallCmdForceShieldRegen();
		}
	}

	[Server]
	public void TakeDamageForce(DamageInfo damageInfo, bool alwaysApply = false, bool disableAirControlUntilCollision = false)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealthComponent::TakeDamageForce(RoR2.DamageInfo,System.Boolean,System.Boolean)' called on client");
		}
		else if (!body.HasBuff(RoR2Content.Buffs.EngiShield) || !(shield > 0f))
		{
			CharacterMotor component = ((Component)this).GetComponent<CharacterMotor>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.ApplyForce(damageInfo.force, alwaysApply, disableAirControlUntilCollision);
			}
			Rigidbody component2 = ((Component)this).GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.AddForce(damageInfo.force, (ForceMode)1);
			}
		}
	}

	[Server]
	public void TakeDamageForce(Vector3 force, bool alwaysApply = false, bool disableAirControlUntilCollision = false)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealthComponent::TakeDamageForce(UnityEngine.Vector3,System.Boolean,System.Boolean)' called on client");
		}
		else if (!body.HasBuff(RoR2Content.Buffs.EngiShield) || !(shield > 0f))
		{
			CharacterMotor component = ((Component)this).GetComponent<CharacterMotor>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.ApplyForce(force, alwaysApply, disableAirControlUntilCollision);
			}
			Rigidbody component2 = ((Component)this).GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.AddForce(force, (ForceMode)1);
			}
		}
	}

	[Server]
	public void TakeDamage(DamageInfo damageInfo)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0596: Unknown result type (might be due to invalid IL or missing references)
		//IL_059b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0675: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0792: Unknown result type (might be due to invalid IL or missing references)
		//IL_0797: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c34: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c39: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c79: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c7e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_10a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_11ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_131c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1321: Unknown result type (might be due to invalid IL or missing references)
		//IL_1338: Unknown result type (might be due to invalid IL or missing references)
		//IL_133d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1344: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_13d3: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealthComponent::TakeDamage(RoR2.DamageInfo)' called on client");
			return;
		}
		if (!damageInfo.canRejectForce)
		{
			TakeDamageForce(damageInfo);
		}
		if (!alive || godMode || ospTimer > 0f)
		{
			return;
		}
		CharacterMaster characterMaster = null;
		CharacterBody characterBody = null;
		TeamIndex teamIndex = TeamIndex.None;
		Vector3 val = Vector3.zero;
		float num = combinedHealth;
		if (Object.op_Implicit((Object)(object)damageInfo.attacker))
		{
			characterBody = damageInfo.attacker.GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)characterBody))
			{
				teamIndex = characterBody.teamComponent.teamIndex;
				val = characterBody.corePosition - damageInfo.position;
			}
		}
		bool flag = (damageInfo.damageType & DamageType.BypassArmor) != 0;
		bool num2 = (damageInfo.damageType & DamageType.BypassBlock) != 0;
		if (!num2 && itemCounts.bear > 0 && Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(15f * (float)itemCounts.bear)))
		{
			EffectManager.SpawnEffect(effectData: new EffectData
			{
				origin = damageInfo.position,
				rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : Random.onUnitSphere)
			}, effectPrefab: AssetReferences.bearEffectPrefab, transmit: true);
			damageInfo.rejected = true;
		}
		if (!num2 && body.HasBuff(DLC1Content.Buffs.BearVoidReady) && damageInfo.damage > 0f)
		{
			EffectData effectData2 = new EffectData
			{
				origin = damageInfo.position,
				rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : Random.onUnitSphere)
			};
			EffectManager.SpawnEffect(AssetReferences.bearVoidEffectPrefab, effectData2, transmit: true);
			damageInfo.rejected = true;
			body.RemoveBuff(DLC1Content.Buffs.BearVoidReady);
			int itemCount = body.inventory.GetItemCount(DLC1Content.Items.BearVoid);
			body.AddTimedBuff(DLC1Content.Buffs.BearVoidCooldown, 15f * Mathf.Pow(0.9f, (float)itemCount));
		}
		if (body.HasBuff(RoR2Content.Buffs.HiddenInvincibility) && !flag)
		{
			damageInfo.rejected = true;
		}
		if (body.HasBuff(RoR2Content.Buffs.Immune) && (!Object.op_Implicit((Object)(object)characterBody) || !characterBody.HasBuff(JunkContent.Buffs.GoldEmpowered)))
		{
			EffectManager.SpawnEffect(AssetReferences.damageRejectedPrefab, new EffectData
			{
				origin = damageInfo.position
			}, transmit: true);
			damageInfo.rejected = true;
		}
		if (!damageInfo.rejected && body.HasBuff(JunkContent.Buffs.BodyArmor))
		{
			body.RemoveBuff(JunkContent.Buffs.BodyArmor);
			EffectData effectData3 = new EffectData
			{
				origin = damageInfo.position,
				rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : Random.onUnitSphere)
			};
			EffectManager.SpawnEffect(AssetReferences.captainBodyArmorBlockEffectPrefab, effectData3, transmit: true);
			damageInfo.rejected = true;
		}
		IOnIncomingDamageServerReceiver[] array = onIncomingDamageReceivers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnIncomingDamageServer(damageInfo);
		}
		if (damageInfo.rejected)
		{
			return;
		}
		float num3 = damageInfo.damage;
		if (teamIndex == body.teamComponent.teamIndex)
		{
			TeamDef teamDef = TeamCatalog.GetTeamDef(teamIndex);
			if (teamDef != null)
			{
				num3 *= teamDef.friendlyFireScaling;
			}
		}
		if (num3 > 0f)
		{
			if (Object.op_Implicit((Object)(object)characterBody))
			{
				if (characterBody.canPerformBackstab && (damageInfo.damageType & DamageType.DoT) != DamageType.DoT && (damageInfo.procChainMask.HasProc(ProcType.Backstab) || BackstabManager.IsBackstab(-val, body)))
				{
					damageInfo.crit = true;
					damageInfo.procChainMask.AddProc(ProcType.Backstab);
					if (Object.op_Implicit((Object)(object)BackstabManager.backstabImpactEffectPrefab))
					{
						EffectManager.SimpleImpactEffect(BackstabManager.backstabImpactEffectPrefab, damageInfo.position, -damageInfo.force, transmit: true);
					}
				}
				characterMaster = characterBody.master;
				if (Object.op_Implicit((Object)(object)characterMaster) && Object.op_Implicit((Object)(object)characterMaster.inventory))
				{
					if (num >= fullCombinedHealth * 0.9f)
					{
						int itemCount2 = characterMaster.inventory.GetItemCount(RoR2Content.Items.Crowbar);
						if (itemCount2 > 0)
						{
							num3 *= 1f + 0.75f * (float)itemCount2;
							EffectManager.SimpleImpactEffect(AssetReferences.crowbarImpactEffectPrefab, damageInfo.position, -damageInfo.force, transmit: true);
						}
					}
					if (num >= fullCombinedHealth && !damageInfo.rejected)
					{
						int itemCount3 = characterMaster.inventory.GetItemCount(DLC1Content.Items.ExplodeOnDeathVoid);
						if (itemCount3 > 0)
						{
							Vector3 corePosition = Util.GetCorePosition(body);
							float damageCoefficient = 2.6f * (1f + (float)(itemCount3 - 1) * 0.6f);
							float baseDamage = Util.OnKillProcDamage(characterBody.damage, damageCoefficient);
							GameObject obj = Object.Instantiate<GameObject>(AssetReferences.explodeOnDeathVoidExplosionPrefab, corePosition, Quaternion.identity);
							DelayBlast component = obj.GetComponent<DelayBlast>();
							component.position = corePosition;
							component.baseDamage = baseDamage;
							component.baseForce = 1000f;
							component.radius = 12f + 2.4f * ((float)itemCount3 - 1f);
							component.attacker = damageInfo.attacker;
							component.inflictor = null;
							component.crit = Util.CheckRoll(characterBody.crit, characterMaster);
							component.maxTimer = 0.2f;
							component.damageColorIndex = DamageColorIndex.Void;
							component.falloffModel = BlastAttack.FalloffModel.SweetSpot;
							obj.GetComponent<TeamFilter>().teamIndex = teamIndex;
							NetworkServer.Spawn(obj);
						}
					}
					int itemCount4 = characterMaster.inventory.GetItemCount(RoR2Content.Items.NearbyDamageBonus);
					if (itemCount4 > 0 && ((Vector3)(ref val)).sqrMagnitude <= 169f)
					{
						damageInfo.damageColorIndex = DamageColorIndex.Nearby;
						num3 *= 1f + (float)itemCount4 * 0.2f;
						EffectManager.SimpleImpactEffect(AssetReferences.diamondDamageBonusImpactEffectPrefab, damageInfo.position, val, transmit: true);
					}
					int itemCount5 = characterMaster.inventory.GetItemCount(DLC1Content.Items.FragileDamageBonus);
					if (itemCount5 > 0)
					{
						num3 *= 1f + (float)itemCount5 * 0.2f;
					}
					if (damageInfo.procCoefficient > 0f)
					{
						int itemCount6 = characterMaster.inventory.GetItemCount(RoR2Content.Items.ArmorReductionOnHit);
						if (itemCount6 > 0 && !body.HasBuff(RoR2Content.Buffs.Pulverized))
						{
							body.AddTimedBuff(RoR2Content.Buffs.PulverizeBuildup, 2f * damageInfo.procCoefficient);
							if (body.GetBuffCount(RoR2Content.Buffs.PulverizeBuildup) >= 5)
							{
								body.ClearTimedBuffs(RoR2Content.Buffs.PulverizeBuildup);
								body.AddTimedBuff(RoR2Content.Buffs.Pulverized, 8f * (float)itemCount6);
								EffectManager.SpawnEffect(AssetReferences.pulverizedEffectPrefab, new EffectData
								{
									origin = body.corePosition,
									scale = body.radius
								}, transmit: true);
							}
						}
						int itemCount7 = characterMaster.inventory.GetItemCount(DLC1Content.Items.PermanentDebuffOnHit);
						bool flag2 = false;
						for (int j = 0; j < itemCount7; j++)
						{
							if (Util.CheckRoll(100f * damageInfo.procCoefficient, characterMaster))
							{
								body.AddBuff(DLC1Content.Buffs.PermanentDebuff);
								flag2 = true;
							}
						}
						if (flag2)
						{
							EffectManager.SpawnEffect(AssetReferences.permanentDebuffEffectPrefab, new EffectData
							{
								origin = damageInfo.position,
								scale = itemCount7
							}, transmit: true);
						}
						if (body.HasBuff(RoR2Content.Buffs.MercExpose) && Object.op_Implicit((Object)(object)characterBody) && characterBody.bodyIndex == BodyCatalog.FindBodyIndex("MercBody"))
						{
							body.RemoveBuff(RoR2Content.Buffs.MercExpose);
							float num4 = characterBody.damage * 3.5f;
							num3 += num4;
							damageInfo.damage += num4;
							SkillLocator skillLocator = characterBody.skillLocator;
							if (Object.op_Implicit((Object)(object)skillLocator))
							{
								skillLocator.DeductCooldownFromAllSkillsServer(1f);
							}
							EffectManager.SimpleImpactEffect(AssetReferences.mercExposeConsumeEffectPrefab, damageInfo.position, Vector3.up, transmit: true);
						}
					}
					if (body.isBoss)
					{
						int itemCount8 = characterMaster.inventory.GetItemCount(RoR2Content.Items.BossDamageBonus);
						if (itemCount8 > 0)
						{
							num3 *= 1f + 0.2f * (float)itemCount8;
							damageInfo.damageColorIndex = DamageColorIndex.WeakPoint;
							EffectManager.SimpleImpactEffect(AssetReferences.bossDamageBonusImpactEffectPrefab, damageInfo.position, -damageInfo.force, transmit: true);
						}
					}
				}
				if (damageInfo.crit)
				{
					num3 *= characterBody.critMultiplier;
				}
			}
			if ((damageInfo.damageType & DamageType.WeakPointHit) != 0)
			{
				num3 *= 1.5f;
				damageInfo.damageColorIndex = DamageColorIndex.WeakPoint;
			}
			if (body.HasBuff(RoR2Content.Buffs.DeathMark))
			{
				num3 *= 1.5f;
				damageInfo.damageColorIndex = DamageColorIndex.DeathMark;
			}
			if (!flag)
			{
				float armor = body.armor;
				armor += adaptiveArmorValue;
				bool flag3 = (damageInfo.damageType & DamageType.AOE) != 0;
				if ((body.bodyFlags & CharacterBody.BodyFlags.ResistantToAOE) != 0 && flag3)
				{
					armor += 300f;
				}
				float num5 = ((armor >= 0f) ? (1f - armor / (armor + 100f)) : (2f - 100f / (100f - armor)));
				num3 = Mathf.Max(1f, num3 * num5);
				if (itemCounts.armorPlate > 0)
				{
					num3 = Mathf.Max(1f, num3 - 5f * (float)itemCounts.armorPlate);
					EntitySoundManager.EmitSoundServer(LegacyResourcesAPI.Load<NetworkSoundEventDef>("NetworkSoundEventDefs/nseArmorPlateBlock").index, ((Component)this).gameObject);
				}
				if (itemCounts.parentEgg > 0)
				{
					Heal((float)itemCounts.parentEgg * 15f, default(ProcChainMask));
					EntitySoundManager.EmitSoundServer(LegacyResourcesAPI.Load<NetworkSoundEventDef>("NetworkSoundEventDefs/nseParentEggHeal").index, ((Component)this).gameObject);
				}
			}
			if (body.hasOneShotProtection && (damageInfo.damageType & DamageType.BypassOneShotProtection) != DamageType.BypassOneShotProtection)
			{
				float num6 = (fullCombinedHealth + barrier) * (1f - body.oneShotProtectionFraction);
				float num7 = Mathf.Max(0f, num6 - serverDamageTakenThisUpdate);
				float num8 = num3;
				num3 = Mathf.Min(num3, num7);
				if (num3 != num8)
				{
					TriggerOneShotProtection();
				}
			}
			if ((damageInfo.damageType & DamageType.BonusToLowHealth) != 0)
			{
				float num9 = Mathf.Lerp(3f, 1f, combinedHealthFraction);
				num3 *= num9;
			}
			if (body.HasBuff(RoR2Content.Buffs.LunarShell) && num3 > fullHealth * 0.1f)
			{
				num3 = fullHealth * 0.1f;
			}
			if (itemCounts.minHealthPercentage > 0)
			{
				float num10 = fullCombinedHealth * ((float)itemCounts.minHealthPercentage / 100f);
				num3 = Mathf.Max(0f, Mathf.Min(num3, combinedHealth - num10));
			}
		}
		if ((damageInfo.damageType & DamageType.SlowOnHit) != 0)
		{
			body.AddTimedBuff(RoR2Content.Buffs.Slow50, 2f);
		}
		if ((damageInfo.damageType & DamageType.ClayGoo) != 0 && (body.bodyFlags & CharacterBody.BodyFlags.ImmuneToGoo) == 0)
		{
			body.AddTimedBuff(RoR2Content.Buffs.ClayGoo, 2f);
		}
		if ((damageInfo.damageType & DamageType.Nullify) != 0)
		{
			body.AddTimedBuff(RoR2Content.Buffs.NullifyStack, 8f);
		}
		if ((damageInfo.damageType & DamageType.CrippleOnHit) != 0 || (Object.op_Implicit((Object)(object)characterBody) && characterBody.HasBuff(RoR2Content.Buffs.AffixLunar)))
		{
			body.AddTimedBuff(RoR2Content.Buffs.Cripple, 3f);
		}
		if ((damageInfo.damageType & DamageType.ApplyMercExpose) != 0)
		{
			Debug.LogFormat("Adding expose", Array.Empty<object>());
			body.AddBuff(RoR2Content.Buffs.MercExpose);
		}
		CharacterMaster master = body.master;
		if (Object.op_Implicit((Object)(object)master))
		{
			if (itemCounts.goldOnHit > 0)
			{
				uint num11 = (uint)(num3 / fullCombinedHealth * (float)master.money * (float)itemCounts.goldOnHit);
				uint money = master.money;
				master.money = (uint)Mathf.Max(0f, (float)master.money - (float)num11);
				if (money - master.money != 0)
				{
					GoldOrb goldOrb = new GoldOrb();
					goldOrb.origin = damageInfo.position;
					goldOrb.target = (Object.op_Implicit((Object)(object)characterBody) ? characterBody.mainHurtBox : body.mainHurtBox);
					goldOrb.goldAmount = 0u;
					OrbManager.instance.AddOrb(goldOrb);
					EffectManager.SimpleImpactEffect(AssetReferences.loseCoinsImpactEffectPrefab, damageInfo.position, Vector3.up, transmit: true);
				}
			}
			if (itemCounts.goldOnHurt > 0 && (Object)(object)characterBody != (Object)(object)body && (Object)(object)characterBody != (Object)null)
			{
				int num12 = 3;
				GoldOrb goldOrb2 = new GoldOrb();
				goldOrb2.origin = damageInfo.position;
				goldOrb2.target = body.mainHurtBox;
				goldOrb2.goldAmount = (uint)((float)(itemCounts.goldOnHurt * num12) * Run.instance.difficultyCoefficient);
				OrbManager.instance.AddOrb(goldOrb2);
				EffectManager.SimpleImpactEffect(AssetReferences.gainCoinsImpactEffectPrefab, damageInfo.position, Vector3.up, transmit: true);
			}
		}
		if (itemCounts.adaptiveArmor > 0)
		{
			float num13 = num3 / fullCombinedHealth * 100f * 30f * (float)itemCounts.adaptiveArmor;
			adaptiveArmorValue = Mathf.Min(adaptiveArmorValue + num13, 400f);
		}
		float num14 = num3;
		if (num14 > 0f)
		{
			isShieldRegenForced = false;
		}
		if (body.teamComponent.teamIndex == TeamIndex.Player && Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse8)
		{
			float num15 = num14 / fullCombinedHealth * 100f;
			float num16 = 0.4f;
			int num17 = Mathf.FloorToInt(num15 * num16);
			for (int k = 0; k < num17; k++)
			{
				body.AddBuff(RoR2Content.Buffs.PermanentCurse);
			}
		}
		if (num14 > 0f && barrier > 0f)
		{
			if (num14 <= barrier)
			{
				Networkbarrier = barrier - num14;
				num14 = 0f;
			}
			else
			{
				num14 -= barrier;
				Networkbarrier = 0f;
			}
		}
		if (num14 > 0f && shield > 0f)
		{
			if (num14 <= shield)
			{
				Networkshield = shield - num14;
				num14 = 0f;
			}
			else
			{
				num14 -= shield;
				Networkshield = 0f;
				float scale = 1f;
				if (Object.op_Implicit((Object)(object)body))
				{
					scale = body.radius;
				}
				EffectManager.SpawnEffect(AssetReferences.shieldBreakEffectPrefab, new EffectData
				{
					origin = ((Component)this).transform.position,
					scale = scale
				}, transmit: true);
			}
		}
		bool flag4 = (damageInfo.damageType & DamageType.VoidDeath) != 0 && (body.bodyFlags & CharacterBody.BodyFlags.ImmuneToVoidDeath) == 0;
		float executionHealthLost = 0f;
		GameObject val2 = null;
		if (num14 > 0f)
		{
			float num18 = health - num14;
			if (num18 < 1f && (damageInfo.damageType & DamageType.NonLethal) != 0 && health >= 1f)
			{
				num18 = 1f;
			}
			Networkhealth = num18;
		}
		float num19 = float.NegativeInfinity;
		bool flag5 = (body.bodyFlags & CharacterBody.BodyFlags.ImmuneToExecutes) != 0;
		if (!flag4 && !flag5)
		{
			if (isInFrozenState && num19 < 0.3f)
			{
				num19 = 0.3f;
				val2 = FrozenState.executeEffectPrefab;
			}
			if (Object.op_Implicit((Object)(object)characterBody))
			{
				if (body.isElite)
				{
					float executeEliteHealthFraction = characterBody.executeEliteHealthFraction;
					if (num19 < executeEliteHealthFraction)
					{
						num19 = executeEliteHealthFraction;
						val2 = AssetReferences.executeEffectPrefab;
					}
				}
				if (!body.isBoss && Object.op_Implicit((Object)(object)characterBody.inventory) && Util.CheckRoll((float)characterBody.inventory.GetItemCount(DLC1Content.Items.CritGlassesVoid) * 0.5f * damageInfo.procCoefficient, characterBody.master))
				{
					flag4 = true;
					val2 = AssetReferences.critGlassesVoidExecuteEffectPrefab;
					damageInfo.damageType |= DamageType.VoidDeath;
				}
			}
		}
		if (flag4 || (num19 > 0f && combinedHealthFraction <= num19))
		{
			flag4 = true;
			executionHealthLost = Mathf.Max(combinedHealth, 0f);
			if (health > 0f)
			{
				Networkhealth = 0f;
			}
			if (shield > 0f)
			{
				Networkshield = 0f;
			}
			if (barrier > 0f)
			{
				Networkbarrier = 0f;
			}
		}
		if (damageInfo.canRejectForce)
		{
			TakeDamageForce(damageInfo);
		}
		DamageReport damageReport = new DamageReport(damageInfo, this, num3, num);
		IOnTakeDamageServerReceiver[] array2 = onTakeDamageReceivers;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].OnTakeDamageServer(damageReport);
		}
		if (num3 > 0f)
		{
			SendDamageDealt(damageReport);
		}
		UpdateLastHitTime(damageReport.damageDealt, damageInfo.position, (damageInfo.damageType & DamageType.Silent) != 0, damageInfo.attacker);
		if (Object.op_Implicit((Object)(object)damageInfo.attacker))
		{
			List<IOnDamageDealtServerReceiver> gameObjectComponents = GetComponentsCache<IOnDamageDealtServerReceiver>.GetGameObjectComponents(damageInfo.attacker);
			foreach (IOnDamageDealtServerReceiver item in gameObjectComponents)
			{
				item.OnDamageDealtServer(damageReport);
			}
			GetComponentsCache<IOnDamageDealtServerReceiver>.ReturnBuffer(gameObjectComponents);
		}
		if (Object.op_Implicit((Object)(object)damageInfo.inflictor))
		{
			List<IOnDamageInflictedServerReceiver> gameObjectComponents2 = GetComponentsCache<IOnDamageInflictedServerReceiver>.GetGameObjectComponents(damageInfo.inflictor);
			foreach (IOnDamageInflictedServerReceiver item2 in gameObjectComponents2)
			{
				item2.OnDamageInflictedServer(damageReport);
			}
			GetComponentsCache<IOnDamageInflictedServerReceiver>.ReturnBuffer(gameObjectComponents2);
		}
		GlobalEventManager.ServerDamageDealt(damageReport);
		if (!alive)
		{
			killingDamageType = damageInfo.damageType;
			if (flag4)
			{
				GlobalEventManager.ServerCharacterExecuted(damageReport, executionHealthLost);
				if (val2 != null)
				{
					EffectManager.SpawnEffect(val2, new EffectData
					{
						origin = body.corePosition,
						scale = (Object.op_Implicit((Object)(object)body) ? body.radius : 1f)
					}, transmit: true);
				}
			}
			IOnKilledServerReceiver[] components = ((Component)this).GetComponents<IOnKilledServerReceiver>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnKilledServer(damageReport);
			}
			if (Object.op_Implicit((Object)(object)damageInfo.attacker))
			{
				IOnKilledOtherServerReceiver[] components2 = damageInfo.attacker.GetComponents<IOnKilledOtherServerReceiver>();
				for (int i = 0; i < components2.Length; i++)
				{
					components2[i].OnKilledOtherServer(damageReport);
				}
			}
			if (Util.CheckRoll(globalDeathEventChanceCoefficient * 100f))
			{
				GlobalEventManager.instance.OnCharacterDeath(damageReport);
			}
		}
		else
		{
			if (!(num3 > 0f))
			{
				return;
			}
			int num20 = 5 + 2 * (itemCounts.thorns - 1);
			if (itemCounts.thorns > 0 && !damageReport.damageInfo.procChainMask.HasProc(ProcType.Thorns))
			{
				bool flag6 = itemCounts.invadingDoppelganger > 0;
				float radius = 25f + 10f * (float)(itemCounts.thorns - 1);
				bool isCrit = body.RollCrit();
				float damageValue = 1.6f * body.damage;
				TeamIndex teamIndex2 = body.teamComponent.teamIndex;
				HurtBox[] hurtBoxes = new SphereSearch
				{
					origin = damageReport.damageInfo.position,
					radius = radius,
					mask = LayerIndex.entityPrecise.mask,
					queryTriggerInteraction = (QueryTriggerInteraction)0
				}.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamIndex2)).OrderCandidatesByDistance()
					.FilterCandidatesByDistinctHurtBoxEntities()
					.GetHurtBoxes();
				for (int l = 0; l < Mathf.Min(num20, hurtBoxes.Length); l++)
				{
					LightningOrb lightningOrb = new LightningOrb();
					lightningOrb.attacker = ((Component)this).gameObject;
					lightningOrb.bouncedObjects = null;
					lightningOrb.bouncesRemaining = 0;
					lightningOrb.damageCoefficientPerBounce = 1f;
					lightningOrb.damageColorIndex = DamageColorIndex.Item;
					lightningOrb.damageValue = damageValue;
					lightningOrb.isCrit = isCrit;
					lightningOrb.lightningType = LightningOrb.LightningType.RazorWire;
					lightningOrb.origin = damageReport.damageInfo.position;
					lightningOrb.procChainMask = default(ProcChainMask);
					lightningOrb.procChainMask.AddProc(ProcType.Thorns);
					lightningOrb.procCoefficient = (flag6 ? 0f : 0.5f);
					lightningOrb.range = 0f;
					lightningOrb.teamIndex = teamIndex2;
					lightningOrb.target = hurtBoxes[l];
					OrbManager.instance.AddOrb(lightningOrb);
				}
			}
		}
	}

	[Server]
	private void TriggerOneShotProtection()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealthComponent::TriggerOneShotProtection()' called on client");
			return;
		}
		ospTimer = 0.1f;
		Debug.Log((object)"OSP Triggered.");
	}

	[Server]
	public void Suicide(GameObject killerOverride = null, GameObject inflictorOverride = null, DamageType damageType = DamageType.Generic)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HealthComponent::Suicide(UnityEngine.GameObject,UnityEngine.GameObject,RoR2.DamageType)' called on client");
		}
		else if (alive && !godMode)
		{
			float combinedHealthBeforeDamage = combinedHealth;
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.damage = combinedHealth;
			damageInfo.position = ((Component)this).transform.position;
			damageInfo.damageType = damageType;
			damageInfo.procCoefficient = 1f;
			if (Object.op_Implicit((Object)(object)killerOverride))
			{
				damageInfo.attacker = killerOverride;
			}
			if (Object.op_Implicit((Object)(object)inflictorOverride))
			{
				damageInfo.inflictor = inflictorOverride;
			}
			Networkhealth = 0f;
			DamageReport damageReport = new DamageReport(damageInfo, this, damageInfo.damage, combinedHealthBeforeDamage);
			killingDamageType = damageInfo.damageType;
			IOnKilledServerReceiver[] components = ((Component)this).GetComponents<IOnKilledServerReceiver>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnKilledServer(damageReport);
			}
			GlobalEventManager.instance.OnCharacterDeath(damageReport);
		}
	}

	public void UpdateLastHitTime(float damageValue, Vector3 damagePosition, bool damageIsSilent, GameObject attacker)
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && Object.op_Implicit((Object)(object)body) && damageValue > 0f)
		{
			if (itemCounts.medkit > 0)
			{
				body.AddTimedBuff(RoR2Content.Buffs.MedkitHeal, 2f);
			}
			if (itemCounts.healingPotion > 0 && isHealthLow)
			{
				body.inventory.RemoveItem(DLC1Content.Items.HealingPotion);
				body.inventory.GiveItem(DLC1Content.Items.HealingPotionConsumed);
				CharacterMasterNotificationQueue.SendTransformNotification(body.master, DLC1Content.Items.HealingPotion.itemIndex, DLC1Content.Items.HealingPotionConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
				HealFraction(0.75f, default(ProcChainMask));
				EffectData effectData = new EffectData
				{
					origin = ((Component)this).transform.position
				};
				effectData.SetNetworkedObjectReference(((Component)this).gameObject);
				EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HealingPotionEffect"), effectData, transmit: true);
			}
			if (itemCounts.fragileDamageBonus > 0 && isHealthLow)
			{
				body.inventory.GiveItem(DLC1Content.Items.FragileDamageBonusConsumed, itemCounts.fragileDamageBonus);
				body.inventory.RemoveItem(DLC1Content.Items.FragileDamageBonus, itemCounts.fragileDamageBonus);
				CharacterMasterNotificationQueue.SendTransformNotification(body.master, DLC1Content.Items.FragileDamageBonus.itemIndex, DLC1Content.Items.FragileDamageBonusConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
				EffectData effectData2 = new EffectData
				{
					origin = ((Component)this).transform.position
				};
				effectData2.SetNetworkedObjectReference(((Component)this).gameObject);
				EffectManager.SpawnEffect(AssetReferences.fragileDamageBonusBreakEffectPrefab, effectData2, transmit: true);
			}
		}
		if (damageIsSilent)
		{
			return;
		}
		lastHitTime = Run.FixedTimeStamp.now;
		lastHitAttacker = attacker;
		serverDamageTakenThisUpdate += damageValue;
		if (Object.op_Implicit((Object)(object)modelLocator))
		{
			Transform modelTransform = modelLocator.modelTransform;
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				Animator component = ((Component)modelTransform).GetComponent<Animator>();
				if (Object.op_Implicit((Object)(object)component))
				{
					string text = "Flinch";
					int layerIndex = component.GetLayerIndex(text);
					if (layerIndex >= 0)
					{
						component.SetLayerWeight(layerIndex, 1f + Mathf.Clamp01(damageValue / fullCombinedHealth * 10f) * 3f);
						component.Play("FlinchStart", layerIndex);
					}
				}
			}
		}
		painAnimationHandler?.HandlePain(damageValue, damagePosition);
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		AssetReferences.Resolve();
	}

	private void Awake()
	{
		body = ((Component)this).GetComponent<CharacterBody>();
		modelLocator = ((Component)this).GetComponent<ModelLocator>();
		painAnimationHandler = ((Component)this).GetComponent<IPainAnimationHandler>();
		onIncomingDamageReceivers = ((Component)this).GetComponents<IOnIncomingDamageServerReceiver>();
		onTakeDamageReceivers = ((Component)this).GetComponents<IOnTakeDamageServerReceiver>();
		lastHitTime = Run.FixedTimeStamp.negativeInfinity;
		lastHealTime = Run.FixedTimeStamp.negativeInfinity;
		body.onInventoryChanged += OnInventoryChanged;
	}

	private void OnDestroy()
	{
		body.onInventoryChanged -= OnInventoryChanged;
	}

	public void FixedUpdate()
	{
		if (NetworkServer.active)
		{
			ServerFixedUpdate();
		}
		if (!alive && wasAlive)
		{
			wasAlive = false;
			((Component)this).GetComponent<CharacterDeathBehavior>()?.OnDeath();
		}
	}

	private void ServerFixedUpdate()
	{
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		if (!alive)
		{
			return;
		}
		regenAccumulator += body.regen * Time.fixedDeltaTime;
		if (barrier > 0f)
		{
			Networkbarrier = Mathf.Max(barrier - body.barrierDecayRate * Time.fixedDeltaTime, 0f);
		}
		if (regenAccumulator > 1f)
		{
			float num = Mathf.Floor(regenAccumulator);
			regenAccumulator -= num;
			Heal(num, default(ProcChainMask), nonRegen: false);
		}
		if (regenAccumulator < -1f)
		{
			float num2 = Mathf.Ceil(regenAccumulator);
			regenAccumulator -= num2;
			Networkhealth = health + num2;
			if (health <= 0f)
			{
				Suicide();
			}
		}
		float num3 = shield;
		bool flag = num3 >= body.maxShield;
		if ((body.outOfDanger || isShieldRegenForced) && !flag)
		{
			num3 += body.maxShield * 0.5f * Time.fixedDeltaTime;
			if (num3 > body.maxShield)
			{
				num3 = body.maxShield;
			}
		}
		if (num3 >= body.maxShield && !flag)
		{
			Util.PlaySound("Play_item_proc_personal_shield_end", ((Component)this).gameObject);
		}
		if (!num3.Equals(shield))
		{
			Networkshield = num3;
		}
		if (devilOrbHealPool > 0f)
		{
			devilOrbTimer -= Time.fixedDeltaTime;
			if (devilOrbTimer <= 0f)
			{
				devilOrbTimer += 0.1f;
				float scale = 1f;
				float num4 = fullCombinedHealth / 10f;
				float num5 = 2.5f;
				devilOrbHealPool -= num4;
				DevilOrb devilOrb = new DevilOrb();
				devilOrb.origin = body.aimOriginTransform.position;
				devilOrb.damageValue = num4 * num5;
				devilOrb.teamIndex = TeamComponent.GetObjectTeam(((Component)this).gameObject);
				devilOrb.attacker = ((Component)this).gameObject;
				devilOrb.damageColorIndex = DamageColorIndex.Poison;
				devilOrb.scale = scale;
				devilOrb.procChainMask.AddProc(ProcType.HealNova);
				devilOrb.effectType = DevilOrb.EffectType.Skull;
				HurtBox hurtBox = devilOrb.PickNextTarget(devilOrb.origin, 40f);
				if (Object.op_Implicit((Object)(object)hurtBox))
				{
					devilOrb.target = hurtBox;
					devilOrb.isCrit = Util.CheckRoll(body.crit, body.master);
					OrbManager.instance.AddOrb(devilOrb);
				}
			}
		}
		adaptiveArmorValue = Mathf.Max(0f, adaptiveArmorValue - 40f * Time.fixedDeltaTime);
		serverDamageTakenThisUpdate = 0f;
		ospTimer -= Time.fixedDeltaTime;
	}

	private static void SendDamageDealt(DamageReport damageReport)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		DamageInfo damageInfo = damageReport.damageInfo;
		DamageDealtMessage damageDealtMessage = new DamageDealtMessage();
		damageDealtMessage.victim = ((Component)damageReport.victim).gameObject;
		damageDealtMessage.damage = damageReport.damageDealt;
		damageDealtMessage.attacker = damageInfo.attacker;
		damageDealtMessage.position = damageInfo.position;
		damageDealtMessage.crit = damageInfo.crit;
		damageDealtMessage.damageType = damageInfo.damageType;
		damageDealtMessage.damageColorIndex = damageInfo.damageColorIndex;
		damageDealtMessage.hitLowHealth = damageReport.hitLowHealth;
		NetworkServer.SendToAll((short)60, (MessageBase)(object)damageDealtMessage);
	}

	[NetworkMessageHandler(msgType = 60, client = true)]
	private static void HandleDamageDealt(NetworkMessage netMsg)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		DamageDealtMessage damageDealtMessage = netMsg.ReadMessage<DamageDealtMessage>();
		if (Object.op_Implicit((Object)(object)damageDealtMessage.victim))
		{
			HealthComponent component = damageDealtMessage.victim.GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component) && !NetworkServer.active)
			{
				component.UpdateLastHitTime(damageDealtMessage.damage, damageDealtMessage.position, damageDealtMessage.isSilent, damageDealtMessage.attacker);
			}
		}
		if (SettingsConVars.enableDamageNumbers.value && Object.op_Implicit((Object)(object)DamageNumberManager.instance))
		{
			TeamComponent teamComponent = null;
			if (Object.op_Implicit((Object)(object)damageDealtMessage.attacker))
			{
				teamComponent = damageDealtMessage.attacker.GetComponent<TeamComponent>();
			}
			DamageNumberManager.instance.SpawnDamageNumber(damageDealtMessage.damage, damageDealtMessage.position, damageDealtMessage.crit, Object.op_Implicit((Object)(object)teamComponent) ? teamComponent.teamIndex : TeamIndex.None, damageDealtMessage.damageColorIndex);
		}
		GlobalEventManager.ClientDamageNotified(damageDealtMessage);
	}

	private static void SendHeal(GameObject target, float amount, bool isCrit)
	{
		HealMessage healMessage = new HealMessage();
		healMessage.target = target;
		healMessage.amount = (isCrit ? (0f - amount) : amount);
		NetworkServer.SendToAll((short)61, (MessageBase)(object)healMessage);
	}

	[NetworkMessageHandler(msgType = 61, client = true)]
	private static void HandleHeal(NetworkMessage netMsg)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		HealMessage healMessage = netMsg.ReadMessage<HealMessage>();
		if (SettingsConVars.enableDamageNumbers.value && Object.op_Implicit((Object)(object)healMessage.target) && Object.op_Implicit((Object)(object)DamageNumberManager.instance))
		{
			DamageNumberManager.instance.SpawnDamageNumber(healMessage.amount, Util.GetCorePosition(healMessage.target), healMessage.amount < 0f, TeamIndex.Player, DamageColorIndex.Heal);
		}
	}

	private void OnInventoryChanged()
	{
		itemCounts = default(ItemCounts);
		Inventory inventory = body.inventory;
		itemCounts = (Object.op_Implicit((Object)(object)inventory) ? new ItemCounts(inventory) : default(ItemCounts));
		currentEquipmentIndex = (Object.op_Implicit((Object)(object)inventory) ? inventory.currentEquipmentIndex : EquipmentIndex.None);
		if (!NetworkServer.active)
		{
			return;
		}
		bool flag = itemCounts.repeatHeal != 0;
		if (flag != Object.op_Implicit((Object)(object)repeatHealComponent))
		{
			if (flag)
			{
				repeatHealComponent = ((Component)this).gameObject.AddComponent<RepeatHealComponent>();
				repeatHealComponent.healthComponent = this;
			}
			else
			{
				Object.Destroy((Object)(object)repeatHealComponent);
				repeatHealComponent = null;
			}
		}
	}

	public HealthBarValues GetHealthBarValues()
	{
		float num = 1f - 1f / body.cursePenalty;
		float num2 = (1f - num) / fullCombinedHealth;
		float num3 = body.oneShotProtectionFraction * fullCombinedHealth - missingCombinedHealth;
		HealthBarValues result = default(HealthBarValues);
		result.hasInfusion = (float)itemCounts.infusion > 0f;
		result.hasVoidShields = (float)itemCounts.missileVoid > 0f;
		result.isElite = body.isElite;
		result.isBoss = body.isBoss;
		result.isVoid = (body.bodyFlags & CharacterBody.BodyFlags.Void) != 0;
		result.cullFraction = ((isInFrozenState && (body.bodyFlags & CharacterBody.BodyFlags.ImmuneToExecutes) == 0) ? Mathf.Clamp01(0.3f * fullCombinedHealth * num2) : 0f);
		result.healthFraction = Mathf.Clamp01(health * num2);
		result.shieldFraction = Mathf.Clamp01(shield * num2);
		result.barrierFraction = Mathf.Clamp01(barrier * num2);
		result.magneticFraction = Mathf.Clamp01(magnetiCharge * num2);
		result.curseFraction = num;
		result.ospFraction = num3 * num2;
		result.healthDisplayValue = (int)combinedHealth;
		result.maxHealthDisplayValue = (int)fullHealth;
		return result;
	}

	static HealthComponent()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		lowHealthFraction = 0.25f;
		kCmdCmdHealFull = -290141736;
		NetworkBehaviour.RegisterCommandDelegate(typeof(HealthComponent), kCmdCmdHealFull, new CmdDelegate(InvokeCmdCmdHealFull));
		kCmdCmdRechargeShieldFull = -833942624;
		NetworkBehaviour.RegisterCommandDelegate(typeof(HealthComponent), kCmdCmdRechargeShieldFull, new CmdDelegate(InvokeCmdCmdRechargeShieldFull));
		kCmdCmdAddBarrier = -1976809257;
		NetworkBehaviour.RegisterCommandDelegate(typeof(HealthComponent), kCmdCmdAddBarrier, new CmdDelegate(InvokeCmdCmdAddBarrier));
		kCmdCmdForceShieldRegen = -1029271894;
		NetworkBehaviour.RegisterCommandDelegate(typeof(HealthComponent), kCmdCmdForceShieldRegen, new CmdDelegate(InvokeCmdCmdForceShieldRegen));
		NetworkCRC.RegisterBehaviour("HealthComponent", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdHealFull(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdHealFull called on client.");
		}
		else
		{
			((HealthComponent)(object)obj).CmdHealFull();
		}
	}

	protected static void InvokeCmdCmdRechargeShieldFull(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdRechargeShieldFull called on client.");
		}
		else
		{
			((HealthComponent)(object)obj).CmdRechargeShieldFull();
		}
	}

	protected static void InvokeCmdCmdAddBarrier(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdAddBarrier called on client.");
		}
		else
		{
			((HealthComponent)(object)obj).CmdAddBarrier(reader.ReadSingle());
		}
	}

	protected static void InvokeCmdCmdForceShieldRegen(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdForceShieldRegen called on client.");
		}
		else
		{
			((HealthComponent)(object)obj).CmdForceShieldRegen();
		}
	}

	public void CallCmdHealFull()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdHealFull called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdHealFull();
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdHealFull);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdHealFull");
	}

	public void CallCmdRechargeShieldFull()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdRechargeShieldFull called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdRechargeShieldFull();
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdRechargeShieldFull);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdRechargeShieldFull");
	}

	public void CallCmdAddBarrier(float value)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdAddBarrier called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdAddBarrier(value);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdAddBarrier);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(value);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdAddBarrier");
	}

	public void CallCmdForceShieldRegen()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdForceShieldRegen called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdForceShieldRegen();
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdForceShieldRegen);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdForceShieldRegen");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(health);
			writer.Write(shield);
			writer.Write(barrier);
			writer.Write(magnetiCharge);
			writer.WritePackedUInt32(_killingDamageType);
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
			writer.Write(health);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(shield);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(barrier);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(magnetiCharge);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32(_killingDamageType);
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
			health = reader.ReadSingle();
			shield = reader.ReadSingle();
			barrier = reader.ReadSingle();
			magnetiCharge = reader.ReadSingle();
			_killingDamageType = reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			health = reader.ReadSingle();
		}
		if (((uint)num & 2u) != 0)
		{
			shield = reader.ReadSingle();
		}
		if (((uint)num & 4u) != 0)
		{
			barrier = reader.ReadSingle();
		}
		if (((uint)num & 8u) != 0)
		{
			magnetiCharge = reader.ReadSingle();
		}
		if (((uint)num & 0x10u) != 0)
		{
			_killingDamageType = reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
	}
}
