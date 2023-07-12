using System;
using System.Collections.Generic;
using System.Linq;
using HG;
using JetBrains.Annotations;
using RoR2.Orbs;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RoR2;

public class GlobalEventManager : MonoBehaviour
{
	private static class CommonAssets
	{
		public static CharacterMaster wispSoulMasterPrefabMasterComponent;

		public static GameObject igniteOnKillExplosionEffectPrefab;

		public static GameObject missilePrefab;

		public static GameObject explodeOnDeathPrefab;

		public static GameObject daggerPrefab;

		public static GameObject bleedOnHitAndExplodeImpactEffect;

		public static GameObject bleedOnHitAndExplodeBlastEffect;

		public static GameObject minorConstructOnKillProjectile;

		public static GameObject missileVoidPrefab;

		public static GameObject eliteEarthHealerMaster;

		public static void Load()
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			wispSoulMasterPrefabMasterComponent = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/WispSoulMaster").GetComponent<CharacterMaster>();
			igniteOnKillExplosionEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/IgniteExplosionVFX");
			missilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/MissileProjectile");
			explodeOnDeathPrefab = Addressables.LoadAssetAsync<GameObject>((object)"RoR2/Base/ExplodeOnDeath/WilloWispDelay.prefab").WaitForCompletion();
			daggerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/DaggerProjectile");
			bleedOnHitAndExplodeImpactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/BleedOnHitAndExplode_Impact");
			bleedOnHitAndExplodeBlastEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BleedOnHitAndExplodeDelay");
			missileVoidPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/MissileVoidProjectile");
			eliteEarthHealerMaster = Addressables.LoadAssetAsync<GameObject>((object)"RoR2/DLC1/EliteEarth/AffixEarthHealerMaster.prefab").WaitForCompletion();
			minorConstructOnKillProjectile = Addressables.LoadAssetAsync<GameObject>((object)"RoR2/DLC1/MinorConstructOnKill/MinorConstructOnKillProjectile.prefab").WaitForCompletion();
		}
	}

	public static GlobalEventManager instance;

	[Obsolete("Transform of the global event manager should not be used! You probably meant something else instead.", true)]
	private Transform transform;

	[Obsolete("GameObject of the global event manager should not be used! You probably meant something else instead.", true)]
	private GameObject gameObject;

	private static readonly string[] standardDeathQuoteTokens = (from i in Enumerable.Range(0, 37)
		select "PLAYER_DEATH_QUOTE_" + TextSerialization.ToStringInvariant(i)).ToArray();

	private static readonly string[] fallDamageDeathQuoteTokens = (from i in Enumerable.Range(0, 5)
		select "PLAYER_DEATH_QUOTE_FALLDAMAGE_" + TextSerialization.ToStringInvariant(i)).ToArray();

	private static readonly SphereSearch igniteOnKillSphereSearch = new SphereSearch();

	private static readonly List<HurtBox> igniteOnKillHurtBoxBuffer = new List<HurtBox>();

	public static event Action<DamageReport> onCharacterDeathGlobal;

	public static event Action<TeamIndex> onTeamLevelUp;

	public static event Action<CharacterBody> onCharacterLevelUp;

	public static event Action<Interactor, IInteractable, GameObject> OnInteractionsGlobal;

	public static event Action<DamageDealtMessage> onClientDamageNotified;

	public static event Action<DamageReport> onServerDamageDealt;

	public static event Action<DamageReport, float> onServerCharacterExecuted;

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		CommonAssets.Load();
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			Debug.LogError((object)"Only one GlobalEventManager can exist at a time.");
		}
		else
		{
			instance = this;
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)instance == (Object)(object)this)
		{
			instance = null;
		}
	}

	public void OnHitEnemy([NotNull] DamageInfo damageInfo, [NotNull] GameObject victim)
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0503: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_054d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_06af: Unknown result type (might be due to invalid IL or missing references)
		//IL_078c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0791: Unknown result type (might be due to invalid IL or missing references)
		//IL_0828: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0961: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bda: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ccd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ccf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cf1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cf6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d23: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d25: Unknown result type (might be due to invalid IL or missing references)
		//IL_0de6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0deb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e49: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e4e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e5d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e81: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e83: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f11: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f13: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f15: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f17: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f3f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f41: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f46: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fb0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1092: Unknown result type (might be due to invalid IL or missing references)
		//IL_1097: Unknown result type (might be due to invalid IL or missing references)
		//IL_10c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_10ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_12ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_12ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_12f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_12fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_130b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1310: Unknown result type (might be due to invalid IL or missing references)
		//IL_132c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1331: Unknown result type (might be due to invalid IL or missing references)
		//IL_1336: Unknown result type (might be due to invalid IL or missing references)
		//IL_1347: Unknown result type (might be due to invalid IL or missing references)
		//IL_1350: Unknown result type (might be due to invalid IL or missing references)
		//IL_1355: Unknown result type (might be due to invalid IL or missing references)
		//IL_1357: Unknown result type (might be due to invalid IL or missing references)
		//IL_135c: Unknown result type (might be due to invalid IL or missing references)
		//IL_138e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1449: Unknown result type (might be due to invalid IL or missing references)
		//IL_1464: Unknown result type (might be due to invalid IL or missing references)
		//IL_1469: Unknown result type (might be due to invalid IL or missing references)
		//IL_146e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1475: Unknown result type (might be due to invalid IL or missing references)
		//IL_1477: Unknown result type (might be due to invalid IL or missing references)
		//IL_147c: Unknown result type (might be due to invalid IL or missing references)
		if (damageInfo.procCoefficient == 0f || damageInfo.rejected || !NetworkServer.active || !Object.op_Implicit((Object)(object)damageInfo.attacker) || !(damageInfo.procCoefficient > 0f))
		{
			return;
		}
		uint? maxStacksFromAttacker = null;
		if (Object.op_Implicit((Object)(object)damageInfo?.inflictor))
		{
			ProjectileDamage component = damageInfo.inflictor.GetComponent<ProjectileDamage>();
			if (Object.op_Implicit((Object)(object)component) && component.useDotMaxStacksFromAttacker)
			{
				maxStacksFromAttacker = component.dotMaxStacksFromAttacker;
			}
		}
		CharacterBody component2 = damageInfo.attacker.GetComponent<CharacterBody>();
		CharacterBody characterBody = (Object.op_Implicit((Object)(object)victim) ? victim.GetComponent<CharacterBody>() : null);
		if (!Object.op_Implicit((Object)(object)component2))
		{
			return;
		}
		if ((damageInfo.damageType & DamageType.PoisonOnHit) != 0)
		{
			DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Poison, 10f * damageInfo.procCoefficient, 1f, maxStacksFromAttacker);
		}
		CharacterMaster master = component2.master;
		if (!Object.op_Implicit((Object)(object)master))
		{
			return;
		}
		Inventory inventory = master.inventory;
		TeamComponent component3 = ((Component)component2).GetComponent<TeamComponent>();
		TeamIndex teamIndex = (Object.op_Implicit((Object)(object)component3) ? component3.teamIndex : TeamIndex.Neutral);
		Vector3 aimOrigin = component2.aimOrigin;
		if (damageInfo.crit)
		{
			instance.OnCrit(component2, damageInfo, master, damageInfo.procCoefficient, damageInfo.procChainMask);
		}
		if (!damageInfo.procChainMask.HasProc(ProcType.HealOnHit))
		{
			int itemCount = inventory.GetItemCount(RoR2Content.Items.Seed);
			if (itemCount > 0)
			{
				HealthComponent component4 = ((Component)component2).GetComponent<HealthComponent>();
				if (Object.op_Implicit((Object)(object)component4))
				{
					ProcChainMask procChainMask = damageInfo.procChainMask;
					procChainMask.AddProc(ProcType.HealOnHit);
					component4.Heal((float)itemCount * damageInfo.procCoefficient, procChainMask);
				}
			}
		}
		if (!damageInfo.procChainMask.HasProc(ProcType.BleedOnHit))
		{
			bool flag = (damageInfo.damageType & DamageType.BleedOnHit) != 0 || (inventory.GetItemCount(RoR2Content.Items.BleedOnHitAndExplode) > 0 && damageInfo.crit);
			if ((component2.bleedChance > 0f || flag) && (flag || Util.CheckRoll(damageInfo.procCoefficient * component2.bleedChance, master)))
			{
				ProcChainMask procChainMask2 = damageInfo.procChainMask;
				procChainMask2.AddProc(ProcType.BleedOnHit);
				DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Bleed, 3f * damageInfo.procCoefficient, 1f, maxStacksFromAttacker);
			}
		}
		if (!damageInfo.procChainMask.HasProc(ProcType.FractureOnHit))
		{
			int itemCount2 = inventory.GetItemCount(DLC1Content.Items.BleedOnHitVoid);
			itemCount2 += (component2.HasBuff(DLC1Content.Buffs.EliteVoid) ? 10 : 0);
			if (itemCount2 > 0 && Util.CheckRoll(damageInfo.procCoefficient * (float)itemCount2 * 10f, master))
			{
				ProcChainMask procChainMask3 = damageInfo.procChainMask;
				procChainMask3.AddProc(ProcType.FractureOnHit);
				DotController.DotDef dotDef = DotController.GetDotDef(DotController.DotIndex.Fracture);
				DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Fracture, dotDef.interval, 1f, maxStacksFromAttacker);
			}
		}
		bool flag2 = (damageInfo.damageType & DamageType.BlightOnHit) != 0;
		if (flag2 && flag2)
		{
			_ = damageInfo.procChainMask;
			DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Blight, 5f * damageInfo.procCoefficient, 1f, maxStacksFromAttacker);
		}
		if ((damageInfo.damageType & DamageType.WeakOnHit) != 0)
		{
			characterBody.AddTimedBuff(RoR2Content.Buffs.Weak, 6f * damageInfo.procCoefficient);
		}
		if ((damageInfo.damageType & DamageType.IgniteOnHit) != 0 || component2.HasBuff(RoR2Content.Buffs.AffixRed))
		{
			float num = 0.5f;
			InflictDotInfo inflictDotInfo = default(InflictDotInfo);
			inflictDotInfo.attackerObject = damageInfo.attacker;
			inflictDotInfo.victimObject = victim;
			inflictDotInfo.totalDamage = damageInfo.damage * num;
			inflictDotInfo.damageMultiplier = 1f;
			inflictDotInfo.dotIndex = DotController.DotIndex.Burn;
			inflictDotInfo.maxStacksFromAttacker = maxStacksFromAttacker;
			InflictDotInfo dotInfo = inflictDotInfo;
			StrengthenBurnUtils.CheckDotForUpgrade(inventory, ref dotInfo);
			DotController.InflictDot(ref dotInfo);
		}
		int num2 = (component2.HasBuff(RoR2Content.Buffs.AffixWhite) ? 1 : 0);
		num2 += (component2.HasBuff(RoR2Content.Buffs.AffixHaunted) ? 2 : 0);
		if (num2 > 0 && Object.op_Implicit((Object)(object)characterBody))
		{
			EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/AffixWhiteImpactEffect"), damageInfo.position, Vector3.up, transmit: true);
			characterBody.AddTimedBuff(RoR2Content.Buffs.Slow80, 1.5f * damageInfo.procCoefficient * (float)num2);
		}
		int itemCount3 = master.inventory.GetItemCount(RoR2Content.Items.SlowOnHit);
		if (itemCount3 > 0 && Object.op_Implicit((Object)(object)characterBody))
		{
			characterBody.AddTimedBuff(RoR2Content.Buffs.Slow60, 2f * (float)itemCount3);
		}
		int itemCount4 = master.inventory.GetItemCount(DLC1Content.Items.SlowOnHitVoid);
		if (itemCount4 > 0 && Object.op_Implicit((Object)(object)characterBody) && Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(5f * (float)itemCount4 * damageInfo.procCoefficient), master))
		{
			characterBody.AddTimedBuff(RoR2Content.Buffs.Nullified, 1f * (float)itemCount4);
		}
		if ((component2.HasBuff(RoR2Content.Buffs.AffixPoison) ? 1 : 0) > 0 && Object.op_Implicit((Object)(object)characterBody))
		{
			characterBody.AddTimedBuff(RoR2Content.Buffs.HealingDisabled, 8f * damageInfo.procCoefficient);
		}
		int itemCount5 = inventory.GetItemCount(RoR2Content.Items.GoldOnHit);
		if (itemCount5 > 0 && Util.CheckRoll(30f * damageInfo.procCoefficient, master))
		{
			GoldOrb goldOrb = new GoldOrb();
			goldOrb.origin = damageInfo.position;
			goldOrb.target = component2.mainHurtBox;
			goldOrb.goldAmount = (uint)((float)itemCount5 * 2f * Run.instance.difficultyCoefficient);
			OrbManager.instance.AddOrb(goldOrb);
			EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CoinImpact"), damageInfo.position, Vector3.up, transmit: true);
		}
		if (!damageInfo.procChainMask.HasProc(ProcType.Missile))
		{
			int itemCount6 = inventory.GetItemCount(RoR2Content.Items.Missile);
			if (itemCount6 > 0)
			{
				if (Util.CheckRoll(10f * damageInfo.procCoefficient, master))
				{
					float damageCoefficient = 3f * (float)itemCount6;
					float missileDamage = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient);
					MissileUtils.FireMissile(component2.corePosition, component2, damageInfo.procChainMask, victim, missileDamage, damageInfo.crit, CommonAssets.missilePrefab, DamageColorIndex.Item, addMissileProc: true);
				}
			}
			else
			{
				itemCount6 = inventory.GetItemCount(DLC1Content.Items.MissileVoid);
				if (itemCount6 > 0 && component2.healthComponent.shield > 0f)
				{
					int num3 = component2?.inventory?.GetItemCount(DLC1Content.Items.MoreMissile) ?? 0;
					float num4 = Mathf.Max(1f, 1f + 0.5f * (float)(num3 - 1));
					float damageCoefficient2 = 0.4f * (float)itemCount6;
					float damageValue = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient2) * num4;
					int num5 = ((num3 <= 0) ? 1 : 3);
					for (int i = 0; i < num5; i++)
					{
						MissileVoidOrb missileVoidOrb = new MissileVoidOrb();
						missileVoidOrb.origin = aimOrigin;
						missileVoidOrb.damageValue = damageValue;
						missileVoidOrb.isCrit = damageInfo.crit;
						missileVoidOrb.teamIndex = teamIndex;
						missileVoidOrb.attacker = damageInfo.attacker;
						missileVoidOrb.procChainMask = damageInfo.procChainMask;
						missileVoidOrb.procChainMask.AddProc(ProcType.Missile);
						missileVoidOrb.procCoefficient = 0.2f;
						missileVoidOrb.damageColorIndex = DamageColorIndex.Void;
						HurtBox mainHurtBox = characterBody.mainHurtBox;
						if (Object.op_Implicit((Object)(object)mainHurtBox))
						{
							missileVoidOrb.target = mainHurtBox;
							OrbManager.instance.AddOrb(missileVoidOrb);
						}
					}
				}
			}
		}
		if (component2.HasBuff(JunkContent.Buffs.LoaderPylonPowered) && !damageInfo.procChainMask.HasProc(ProcType.LoaderLightning))
		{
			float damageCoefficient3 = 0.3f;
			float damageValue2 = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient3);
			LightningOrb lightningOrb = new LightningOrb();
			lightningOrb.origin = damageInfo.position;
			lightningOrb.damageValue = damageValue2;
			lightningOrb.isCrit = damageInfo.crit;
			lightningOrb.bouncesRemaining = 3;
			lightningOrb.teamIndex = teamIndex;
			lightningOrb.attacker = damageInfo.attacker;
			lightningOrb.bouncedObjects = new List<HealthComponent> { victim.GetComponent<HealthComponent>() };
			lightningOrb.procChainMask = damageInfo.procChainMask;
			lightningOrb.procChainMask.AddProc(ProcType.LoaderLightning);
			lightningOrb.procCoefficient = 0f;
			lightningOrb.lightningType = LightningOrb.LightningType.Loader;
			lightningOrb.damageColorIndex = DamageColorIndex.Item;
			lightningOrb.range = 20f;
			HurtBox hurtBox = lightningOrb.PickNextTarget(damageInfo.position);
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				lightningOrb.target = hurtBox;
				OrbManager.instance.AddOrb(lightningOrb);
			}
		}
		int itemCount7 = inventory.GetItemCount(RoR2Content.Items.ChainLightning);
		float num6 = 25f;
		if (itemCount7 > 0 && !damageInfo.procChainMask.HasProc(ProcType.ChainLightning) && Util.CheckRoll(num6 * damageInfo.procCoefficient, master))
		{
			float damageCoefficient4 = 0.8f;
			float damageValue3 = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient4);
			LightningOrb lightningOrb2 = new LightningOrb();
			lightningOrb2.origin = damageInfo.position;
			lightningOrb2.damageValue = damageValue3;
			lightningOrb2.isCrit = damageInfo.crit;
			lightningOrb2.bouncesRemaining = 2 * itemCount7;
			lightningOrb2.teamIndex = teamIndex;
			lightningOrb2.attacker = damageInfo.attacker;
			lightningOrb2.bouncedObjects = new List<HealthComponent> { victim.GetComponent<HealthComponent>() };
			lightningOrb2.procChainMask = damageInfo.procChainMask;
			lightningOrb2.procChainMask.AddProc(ProcType.ChainLightning);
			lightningOrb2.procCoefficient = 0.2f;
			lightningOrb2.lightningType = LightningOrb.LightningType.Ukulele;
			lightningOrb2.damageColorIndex = DamageColorIndex.Item;
			lightningOrb2.range += 2 * itemCount7;
			HurtBox hurtBox2 = lightningOrb2.PickNextTarget(damageInfo.position);
			if (Object.op_Implicit((Object)(object)hurtBox2))
			{
				lightningOrb2.target = hurtBox2;
				OrbManager.instance.AddOrb(lightningOrb2);
			}
		}
		int itemCount8 = inventory.GetItemCount(DLC1Content.Items.ChainLightningVoid);
		float num7 = 25f;
		if (itemCount8 > 0 && !damageInfo.procChainMask.HasProc(ProcType.ChainLightning) && Util.CheckRoll(num7 * damageInfo.procCoefficient, master))
		{
			float damageCoefficient5 = 0.6f;
			float damageValue4 = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient5);
			VoidLightningOrb voidLightningOrb = new VoidLightningOrb();
			voidLightningOrb.origin = damageInfo.position;
			voidLightningOrb.damageValue = damageValue4;
			voidLightningOrb.isCrit = damageInfo.crit;
			voidLightningOrb.totalStrikes = 3 * itemCount8;
			voidLightningOrb.teamIndex = teamIndex;
			voidLightningOrb.attacker = damageInfo.attacker;
			voidLightningOrb.procChainMask = damageInfo.procChainMask;
			voidLightningOrb.procChainMask.AddProc(ProcType.ChainLightning);
			voidLightningOrb.procCoefficient = 0.2f;
			voidLightningOrb.damageColorIndex = DamageColorIndex.Void;
			voidLightningOrb.secondsPerStrike = 0.1f;
			HurtBox mainHurtBox2 = characterBody.mainHurtBox;
			if (Object.op_Implicit((Object)(object)mainHurtBox2))
			{
				voidLightningOrb.target = mainHurtBox2;
				OrbManager.instance.AddOrb(voidLightningOrb);
			}
		}
		int itemCount9 = inventory.GetItemCount(RoR2Content.Items.BounceNearby);
		if (itemCount9 > 0)
		{
			float num8 = (1f - 100f / (100f + 20f * (float)itemCount9)) * 100f;
			if (!damageInfo.procChainMask.HasProc(ProcType.BounceNearby) && Util.CheckRoll(num8 * damageInfo.procCoefficient, master))
			{
				List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
				int maxTargets = 5 + itemCount9 * 5;
				BullseyeSearch search = new BullseyeSearch();
				List<HealthComponent> list2 = CollectionPool<HealthComponent, List<HealthComponent>>.RentCollection();
				if (Object.op_Implicit((Object)(object)component2) && Object.op_Implicit((Object)(object)component2.healthComponent))
				{
					list2.Add(component2.healthComponent);
				}
				if (Object.op_Implicit((Object)(object)characterBody) && Object.op_Implicit((Object)(object)characterBody.healthComponent))
				{
					list2.Add(characterBody.healthComponent);
				}
				BounceOrb.SearchForTargets(search, teamIndex, damageInfo.position, 30f, maxTargets, list, list2);
				CollectionPool<HealthComponent, List<HealthComponent>>.ReturnCollection(list2);
				List<HealthComponent> bouncedObjects = new List<HealthComponent> { victim.GetComponent<HealthComponent>() };
				float damageCoefficient6 = 1f;
				float damageValue5 = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient6);
				int j = 0;
				for (int count = list.Count; j < count; j++)
				{
					HurtBox hurtBox3 = list[j];
					if (Object.op_Implicit((Object)(object)hurtBox3))
					{
						BounceOrb bounceOrb = new BounceOrb();
						bounceOrb.origin = damageInfo.position;
						bounceOrb.damageValue = damageValue5;
						bounceOrb.isCrit = damageInfo.crit;
						bounceOrb.teamIndex = teamIndex;
						bounceOrb.attacker = damageInfo.attacker;
						bounceOrb.procChainMask = damageInfo.procChainMask;
						bounceOrb.procChainMask.AddProc(ProcType.BounceNearby);
						bounceOrb.procCoefficient = 0.33f;
						bounceOrb.damageColorIndex = DamageColorIndex.Item;
						bounceOrb.bouncedObjects = bouncedObjects;
						bounceOrb.target = hurtBox3;
						OrbManager.instance.AddOrb(bounceOrb);
					}
				}
				CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
			}
		}
		int itemCount10 = inventory.GetItemCount(RoR2Content.Items.StickyBomb);
		if (itemCount10 > 0 && Util.CheckRoll(5f * (float)itemCount10 * damageInfo.procCoefficient, master) && Object.op_Implicit((Object)(object)characterBody))
		{
			bool alive = characterBody.healthComponent.alive;
			float num9 = 5f;
			Vector3 position = damageInfo.position;
			Vector3 forward = characterBody.corePosition - position;
			float magnitude = ((Vector3)(ref forward)).magnitude;
			Quaternion rotation = ((magnitude != 0f) ? Util.QuaternionSafeLookRotation(forward) : Random.rotationUniform);
			float damageCoefficient7 = 1.8f;
			float damage = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient7);
			ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/StickyBomb"), position, rotation, damageInfo.attacker, damage, 100f, damageInfo.crit, DamageColorIndex.Item, null, alive ? (magnitude * num9) : (-1f));
		}
		if (!damageInfo.procChainMask.HasProc(ProcType.Rings) && damageInfo.damage / component2.damage >= 4f)
		{
			if (component2.HasBuff(RoR2Content.Buffs.ElementalRingsReady))
			{
				int itemCount11 = inventory.GetItemCount(RoR2Content.Items.IceRing);
				int itemCount12 = inventory.GetItemCount(RoR2Content.Items.FireRing);
				component2.RemoveBuff(RoR2Content.Buffs.ElementalRingsReady);
				for (int k = 1; (float)k <= 10f; k++)
				{
					component2.AddTimedBuff(RoR2Content.Buffs.ElementalRingsCooldown, k);
				}
				ProcChainMask procChainMask4 = damageInfo.procChainMask;
				procChainMask4.AddProc(ProcType.Rings);
				Vector3 position2 = damageInfo.position;
				if (itemCount11 > 0)
				{
					float damageCoefficient8 = 2.5f * (float)itemCount11;
					float damage2 = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient8);
					DamageInfo damageInfo2 = new DamageInfo
					{
						damage = damage2,
						damageColorIndex = DamageColorIndex.Item,
						damageType = DamageType.Generic,
						attacker = damageInfo.attacker,
						crit = damageInfo.crit,
						force = Vector3.zero,
						inflictor = null,
						position = position2,
						procChainMask = procChainMask4,
						procCoefficient = 1f
					};
					EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/IceRingExplosion"), position2, Vector3.up, transmit: true);
					characterBody.AddTimedBuff(RoR2Content.Buffs.Slow80, 3f * (float)itemCount11);
					characterBody.healthComponent.TakeDamage(damageInfo2);
				}
				if (itemCount12 > 0)
				{
					GameObject val = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/FireTornado");
					float resetInterval = val.GetComponent<ProjectileOverlapAttack>().resetInterval;
					float lifetime = val.GetComponent<ProjectileSimple>().lifetime;
					float damageCoefficient9 = 3f * (float)itemCount12;
					float damage3 = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient9) / lifetime * resetInterval;
					float speedOverride = 0f;
					Quaternion rotation2 = Quaternion.identity;
					Vector3 val2 = position2 - aimOrigin;
					val2.y = 0f;
					if (val2 != Vector3.zero)
					{
						speedOverride = -1f;
						rotation2 = Util.QuaternionSafeLookRotation(val2, Vector3.up);
					}
					ProjectileManager.instance.FireProjectile(new FireProjectileInfo
					{
						damage = damage3,
						crit = damageInfo.crit,
						damageColorIndex = DamageColorIndex.Item,
						position = position2,
						procChainMask = procChainMask4,
						force = 0f,
						owner = damageInfo.attacker,
						projectilePrefab = val,
						rotation = rotation2,
						speedOverride = speedOverride,
						target = null
					});
				}
			}
			else if (component2.HasBuff(DLC1Content.Buffs.ElementalRingVoidReady))
			{
				int itemCount13 = inventory.GetItemCount(DLC1Content.Items.ElementalRingVoid);
				component2.RemoveBuff(DLC1Content.Buffs.ElementalRingVoidReady);
				for (int l = 1; (float)l <= 20f; l++)
				{
					component2.AddTimedBuff(DLC1Content.Buffs.ElementalRingVoidCooldown, l);
				}
				ProcChainMask procChainMask5 = damageInfo.procChainMask;
				procChainMask5.AddProc(ProcType.Rings);
				if (itemCount13 > 0)
				{
					GameObject projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/ElementalRingVoidBlackHole");
					float damageCoefficient10 = 1f * (float)itemCount13;
					float damage4 = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient10);
					ProjectileManager.instance.FireProjectile(new FireProjectileInfo
					{
						damage = damage4,
						crit = damageInfo.crit,
						damageColorIndex = DamageColorIndex.Void,
						position = damageInfo.position,
						procChainMask = procChainMask5,
						force = 6000f,
						owner = damageInfo.attacker,
						projectilePrefab = projectilePrefab,
						rotation = Quaternion.identity,
						target = null
					});
				}
			}
		}
		int itemCount14 = master.inventory.GetItemCount(RoR2Content.Items.DeathMark);
		int num10 = 0;
		if (itemCount14 >= 1 && !characterBody.HasBuff(RoR2Content.Buffs.DeathMark))
		{
			BuffIndex[] debuffBuffIndices = BuffCatalog.debuffBuffIndices;
			foreach (BuffIndex buffType in debuffBuffIndices)
			{
				if (characterBody.HasBuff(buffType))
				{
					num10++;
				}
			}
			DotController dotController = DotController.FindDotController(victim.gameObject);
			if (Object.op_Implicit((Object)(object)dotController))
			{
				for (DotController.DotIndex dotIndex = DotController.DotIndex.Bleed; dotIndex < DotController.DotIndex.Count; dotIndex++)
				{
					if (dotController.HasDotActive(dotIndex))
					{
						num10++;
					}
				}
			}
			if (num10 >= 4)
			{
				characterBody.AddTimedBuff(RoR2Content.Buffs.DeathMark, 7f * (float)itemCount14);
			}
		}
		if (damageInfo != null && (Object)(object)damageInfo.inflictor != (Object)null && (Object)(object)damageInfo.inflictor.GetComponent<BoomerangProjectile>() != (Object)null && !damageInfo.procChainMask.HasProc(ProcType.BleedOnHit))
		{
			int num11 = 0;
			if (inventory.GetEquipmentIndex() == RoR2Content.Equipment.Saw.equipmentIndex)
			{
				num11 = 1;
			}
			bool flag3 = (damageInfo.damageType & DamageType.BleedOnHit) != 0;
			if ((num11 > 0 || flag3) && (flag3 || Util.CheckRoll(100f, master)))
			{
				ProcChainMask procChainMask6 = damageInfo.procChainMask;
				procChainMask6.AddProc(ProcType.BleedOnHit);
				DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Bleed, 4f * damageInfo.procCoefficient, 1f, maxStacksFromAttacker);
			}
		}
		if (damageInfo.crit && (damageInfo.damageType & DamageType.SuperBleedOnCrit) != 0)
		{
			DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.SuperBleed, 15f * damageInfo.procCoefficient, 1f, maxStacksFromAttacker);
		}
		if (component2.HasBuff(RoR2Content.Buffs.LifeSteal))
		{
			float amount = damageInfo.damage * 0.2f;
			component2.healthComponent.Heal(amount, damageInfo.procChainMask);
		}
		int itemCount15 = inventory.GetItemCount(RoR2Content.Items.FireballsOnHit);
		if (itemCount15 > 0 && !damageInfo.procChainMask.HasProc(ProcType.Meatball))
		{
			InputBankTest component5 = ((Component)component2).GetComponent<InputBankTest>();
			Vector3 val3 = (Object.op_Implicit((Object)(object)characterBody.characterMotor) ? (victim.transform.position + Vector3.up * (characterBody.characterMotor.capsuleHeight * 0.5f + 2f)) : (victim.transform.position + Vector3.up * 2f));
			Vector3 val4 = (Object.op_Implicit((Object)(object)component5) ? component5.aimDirection : victim.transform.forward);
			val4 = Vector3.up;
			float num12 = 20f;
			if (Util.CheckRoll(10f * damageInfo.procCoefficient, master))
			{
				EffectData effectData = new EffectData
				{
					scale = 1f,
					origin = val3
				};
				EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MuzzleFlashes/MuzzleflashFireMeatBall"), effectData, transmit: true);
				int num13 = 3;
				float damageCoefficient11 = 3f * (float)itemCount15;
				float damage5 = Util.OnHitProcDamage(damageInfo.damage, component2.damage, damageCoefficient11);
				float num14 = 30f;
				ProcChainMask procChainMask7 = damageInfo.procChainMask;
				procChainMask7.AddProc(ProcType.Meatball);
				float speedOverride2 = Random.Range(15f, num14);
				float num15 = 360 / num13;
				_ = num15 / 360f;
				float num16 = 1f;
				float num17 = num15;
				for (int n = 0; n < num13; n++)
				{
					float num18 = (float)n * MathF.PI * 2f / (float)num13;
					FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
					fireProjectileInfo.projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/FireMeatBall");
					fireProjectileInfo.position = val3 + new Vector3(num16 * Mathf.Sin(num18), 0f, num16 * Mathf.Cos(num18));
					fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(val4);
					fireProjectileInfo.procChainMask = procChainMask7;
					fireProjectileInfo.target = victim;
					fireProjectileInfo.owner = ((Component)component2).gameObject;
					fireProjectileInfo.damage = damage5;
					fireProjectileInfo.crit = damageInfo.crit;
					fireProjectileInfo.force = 200f;
					fireProjectileInfo.damageColorIndex = DamageColorIndex.Item;
					fireProjectileInfo.speedOverride = speedOverride2;
					fireProjectileInfo.useSpeedOverride = true;
					FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
					num17 += num15;
					ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
					val4.x += Mathf.Sin(num18 + Random.Range(0f - num12, num12));
					val4.z += Mathf.Cos(num18 + Random.Range(0f - num12, num12));
				}
			}
		}
		int itemCount16 = inventory.GetItemCount(RoR2Content.Items.LightningStrikeOnHit);
		if (itemCount16 > 0 && !damageInfo.procChainMask.HasProc(ProcType.LightningStrikeOnHit) && Util.CheckRoll(10f * damageInfo.procCoefficient, master))
		{
			float damageValue6 = Util.OnHitProcDamage(damageInfo.damage, component2.damage, 5f * (float)itemCount16);
			ProcChainMask procChainMask8 = damageInfo.procChainMask;
			procChainMask8.AddProc(ProcType.LightningStrikeOnHit);
			HurtBox target = characterBody.mainHurtBox;
			if (Object.op_Implicit((Object)(object)characterBody.hurtBoxGroup))
			{
				target = characterBody.hurtBoxGroup.hurtBoxes[Random.Range(0, characterBody.hurtBoxGroup.hurtBoxes.Length)];
			}
			OrbManager.instance.AddOrb(new SimpleLightningStrikeOrb
			{
				attacker = ((Component)component2).gameObject,
				damageColorIndex = DamageColorIndex.Item,
				damageValue = damageValue6,
				isCrit = Util.CheckRoll(component2.crit, master),
				procChainMask = procChainMask8,
				procCoefficient = 1f,
				target = target
			});
		}
		if ((damageInfo.damageType & DamageType.LunarSecondaryRootOnHit) != 0 && Object.op_Implicit((Object)(object)characterBody))
		{
			int itemCount17 = master.inventory.GetItemCount(RoR2Content.Items.LunarSecondaryReplacement);
			characterBody.AddTimedBuff(RoR2Content.Buffs.LunarSecondaryRoot, 3f * (float)itemCount17);
		}
		if ((damageInfo.damageType & DamageType.FruitOnHit) != 0 && Object.op_Implicit((Object)(object)characterBody))
		{
			characterBody.AddTimedBuff(RoR2Content.Buffs.Fruiting, 10f);
		}
		if (inventory.GetItemCount(DLC1Content.Items.DroneWeaponsBoost) > 0)
		{
			DroneWeaponsBoostBehavior component6 = ((Component)component2).GetComponent<DroneWeaponsBoostBehavior>();
			if (Object.op_Implicit((Object)(object)component6))
			{
				component6.OnEnemyHit(damageInfo, characterBody);
			}
		}
	}

	public void OnCharacterHitGroundServer(CharacterBody characterBody, Vector3 impactVelocity)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		bool flag = RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.weakAssKneesArtifactDef);
		float num = Mathf.Abs(impactVelocity.y);
		Inventory inventory = characterBody.inventory;
		_ = characterBody.master;
		CharacterMotor characterMotor = characterBody.characterMotor;
		bool flag2 = false;
		if (((Object.op_Implicit((Object)(object)inventory) && inventory.GetItemCount(RoR2Content.Items.FallBoots) != 0) ? 1 : 0) <= (false ? 1 : 0) && (characterBody.bodyFlags & CharacterBody.BodyFlags.IgnoreFallDamage) == 0)
		{
			float num2 = Mathf.Max(num - (characterBody.jumpPower + 20f), 0f);
			if (num2 > 0f)
			{
				HealthComponent component = ((Component)characterBody).GetComponent<HealthComponent>();
				if (Object.op_Implicit((Object)(object)component))
				{
					flag2 = true;
					float num3 = num2 / 60f;
					DamageInfo damageInfo = new DamageInfo();
					damageInfo.attacker = null;
					damageInfo.inflictor = null;
					damageInfo.crit = false;
					damageInfo.damage = num3 * characterBody.maxHealth;
					damageInfo.damageType = DamageType.NonLethal | DamageType.FallDamage;
					damageInfo.force = Vector3.zero;
					damageInfo.position = characterBody.footPosition;
					damageInfo.procCoefficient = 0f;
					if (flag || (characterBody.teamComponent.teamIndex == TeamIndex.Player && Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse3))
					{
						damageInfo.damage *= 2f;
						damageInfo.damageType &= ~DamageType.NonLethal;
						damageInfo.damageType |= DamageType.BypassOneShotProtection;
					}
					component.TakeDamage(damageInfo);
				}
			}
		}
		if (!Object.op_Implicit((Object)(object)characterMotor) || !(Run.FixedTimeStamp.now - characterMotor.lastGroundedTime > 0.2f))
		{
			return;
		}
		Vector3 footPosition = characterBody.footPosition;
		float radius = characterBody.radius;
		RaycastHit val = default(RaycastHit);
		if (!Physics.Raycast(new Ray(footPosition + Vector3.up * 1.5f, Vector3.down), ref val, 4f, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.water.mask), (QueryTriggerInteraction)2))
		{
			return;
		}
		SurfaceDef objectSurfaceDef = SurfaceDefProvider.GetObjectSurfaceDef(((RaycastHit)(ref val)).collider, ((RaycastHit)(ref val)).point);
		if (!Object.op_Implicit((Object)(object)objectSurfaceDef))
		{
			return;
		}
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CharacterLandImpact"), new EffectData
		{
			origin = footPosition,
			scale = radius,
			color = Color32.op_Implicit(objectSurfaceDef.approximateColor)
		}, transmit: true);
		if (Object.op_Implicit((Object)(object)objectSurfaceDef.footstepEffectPrefab))
		{
			EffectManager.SpawnEffect(objectSurfaceDef.footstepEffectPrefab, new EffectData
			{
				origin = ((RaycastHit)(ref val)).point,
				scale = radius * 3f
			}, transmit: false);
		}
		SfxLocator component2 = ((Component)characterBody).GetComponent<SfxLocator>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			if (objectSurfaceDef.materialSwitchString != null && objectSurfaceDef.materialSwitchString.Length > 0)
			{
				AkSoundEngine.SetSwitch("material", objectSurfaceDef.materialSwitchString, ((Component)characterBody).gameObject);
			}
			else
			{
				AkSoundEngine.SetSwitch("material", "dirt", ((Component)characterBody).gameObject);
			}
			Util.PlaySound(component2.landingSound, ((Component)characterBody).gameObject);
			if (flag2)
			{
				Util.PlaySound(component2.fallDamageSound, ((Component)characterBody).gameObject);
			}
		}
	}

	[Obsolete("Use OnCharacterHitGroundServer instead, which this is just a backwards-compatibility wrapper for.", false)]
	public void OnCharacterHitGround(CharacterBody characterBody, Vector3 impactVelocity)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		OnCharacterHitGroundServer(characterBody, impactVelocity);
	}

	private void OnPlayerCharacterDeath(DamageReport damageReport, NetworkUser victimNetworkUser)
	{
		if (Object.op_Implicit((Object)(object)victimNetworkUser))
		{
			CharacterBody victimBody = damageReport.victimBody;
			string text = (((damageReport.damageInfo.damageType & DamageType.VoidDeath) != 0) ? "PLAYER_DEATH_QUOTE_VOIDDEATH" : (damageReport.isFallDamage ? fallDamageDeathQuoteTokens[Random.Range(0, fallDamageDeathQuoteTokens.Length)] : ((!Object.op_Implicit((Object)(object)victimBody) || !Object.op_Implicit((Object)(object)victimBody.inventory) || victimBody.inventory.GetItemCount(RoR2Content.Items.LunarDagger) <= 0) ? standardDeathQuoteTokens[Random.Range(0, standardDeathQuoteTokens.Length)] : "PLAYER_DEATH_QUOTE_BRITTLEDEATH")));
			if (Object.op_Implicit((Object)(object)victimNetworkUser.masterController))
			{
				victimNetworkUser.masterController.finalMessageTokenServer = text;
			}
			Chat.SendBroadcastChat(new Chat.PlayerDeathChatMessage
			{
				subjectAsNetworkUser = victimNetworkUser,
				baseToken = text
			});
		}
	}

	private static void ProcIgniteOnKill(DamageReport damageReport, int igniteOnKillCount, CharacterBody victimBody, TeamIndex attackerTeamIndex)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		float num = 8f + 4f * (float)igniteOnKillCount;
		float radius = victimBody.radius;
		float num2 = num + radius;
		float num3 = 1.5f;
		float baseDamage = damageReport.attackerBody.damage * num3;
		Vector3 corePosition = victimBody.corePosition;
		igniteOnKillSphereSearch.origin = corePosition;
		igniteOnKillSphereSearch.mask = LayerIndex.entityPrecise.mask;
		igniteOnKillSphereSearch.radius = num2;
		igniteOnKillSphereSearch.RefreshCandidates();
		igniteOnKillSphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(attackerTeamIndex));
		igniteOnKillSphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
		igniteOnKillSphereSearch.OrderCandidatesByDistance();
		igniteOnKillSphereSearch.GetHurtBoxes(igniteOnKillHurtBoxBuffer);
		igniteOnKillSphereSearch.ClearCandidates();
		float value = (float)(1 + igniteOnKillCount) * 0.75f * damageReport.attackerBody.damage;
		for (int i = 0; i < igniteOnKillHurtBoxBuffer.Count; i++)
		{
			HurtBox hurtBox = igniteOnKillHurtBoxBuffer[i];
			if (Object.op_Implicit((Object)(object)hurtBox.healthComponent))
			{
				InflictDotInfo inflictDotInfo = default(InflictDotInfo);
				inflictDotInfo.victimObject = ((Component)hurtBox.healthComponent).gameObject;
				inflictDotInfo.attackerObject = damageReport.attacker;
				inflictDotInfo.totalDamage = value;
				inflictDotInfo.dotIndex = DotController.DotIndex.Burn;
				inflictDotInfo.damageMultiplier = 1f;
				InflictDotInfo dotInfo = inflictDotInfo;
				if (Object.op_Implicit((Object)(object)damageReport?.attackerMaster?.inventory))
				{
					StrengthenBurnUtils.CheckDotForUpgrade(damageReport.attackerMaster.inventory, ref dotInfo);
				}
				DotController.InflictDot(ref dotInfo);
			}
		}
		igniteOnKillHurtBoxBuffer.Clear();
		BlastAttack blastAttack = new BlastAttack();
		blastAttack.radius = num2;
		blastAttack.baseDamage = baseDamage;
		blastAttack.procCoefficient = 0f;
		blastAttack.crit = Util.CheckRoll(damageReport.attackerBody.crit, damageReport.attackerMaster);
		blastAttack.damageColorIndex = DamageColorIndex.Item;
		blastAttack.attackerFiltering = AttackerFiltering.Default;
		blastAttack.falloffModel = BlastAttack.FalloffModel.None;
		blastAttack.attacker = damageReport.attacker;
		blastAttack.teamIndex = attackerTeamIndex;
		blastAttack.position = corePosition;
		blastAttack.Fire();
		EffectManager.SpawnEffect(CommonAssets.igniteOnKillExplosionEffectPrefab, new EffectData
		{
			origin = corePosition,
			scale = num2,
			rotation = Util.QuaternionSafeLookRotation(damageReport.damageInfo.force)
		}, transmit: true);
	}

	public void OnCharacterDeath(DamageReport damageReport)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0595: Unknown result type (might be due to invalid IL or missing references)
		//IL_0597: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0607: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0709: Unknown result type (might be due to invalid IL or missing references)
		//IL_070e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0713: Unknown result type (might be due to invalid IL or missing references)
		//IL_0786: Unknown result type (might be due to invalid IL or missing references)
		//IL_0788: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0824: Unknown result type (might be due to invalid IL or missing references)
		//IL_0826: Unknown result type (might be due to invalid IL or missing references)
		//IL_086a: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_093f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0944: Unknown result type (might be due to invalid IL or missing references)
		//IL_09db: Unknown result type (might be due to invalid IL or missing references)
		//IL_09dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b16: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b1b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b25: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b34: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b39: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b56: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b58: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b61: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b66: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c36: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c5c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c5e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c60: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c70: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c75: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c8a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c8f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cf4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cfb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d05: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d79: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d7b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f2f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f31: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f70: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f72: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f83: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f85: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active || damageReport == null)
		{
			return;
		}
		DamageInfo damageInfo = damageReport.damageInfo;
		GameObject val = null;
		if (Object.op_Implicit((Object)(object)damageReport.victim))
		{
			val = ((Component)damageReport.victim).gameObject;
		}
		CharacterBody victimBody = damageReport.victimBody;
		TeamComponent teamComponent = null;
		CharacterMaster victimMaster = damageReport.victimMaster;
		TeamIndex teamIndex = damageReport.victimTeamIndex;
		Vector3 val2 = Vector3.zero;
		Quaternion val3 = Quaternion.identity;
		Vector3 val4 = Vector3.zero;
		Transform val5 = val.transform;
		if (Object.op_Implicit((Object)(object)val5))
		{
			val2 = val5.position;
			val3 = val5.rotation;
			val4 = val2;
		}
		InputBankTest inputBankTest = null;
		EquipmentIndex equipmentIndex = EquipmentIndex.None;
		EquipmentDef equipmentDef = null;
		if (Object.op_Implicit((Object)(object)victimBody))
		{
			teamComponent = victimBody.teamComponent;
			inputBankTest = victimBody.inputBank;
			val4 = victimBody.corePosition;
			if (Object.op_Implicit((Object)(object)victimBody.equipmentSlot))
			{
				equipmentIndex = victimBody.equipmentSlot.equipmentIndex;
				equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
			}
		}
		Ray val6 = (Ray)(Object.op_Implicit((Object)(object)inputBankTest) ? inputBankTest.GetAimRay() : new Ray(val2, val3 * Vector3.forward));
		GameObject attacker = damageReport.attacker;
		CharacterBody attackerBody = damageReport.attackerBody;
		CharacterMaster attackerMaster = damageReport.attackerMaster;
		Inventory inventory = (Object.op_Implicit((Object)(object)attackerMaster) ? attackerMaster.inventory : null);
		TeamIndex attackerTeamIndex = damageReport.attackerTeamIndex;
		if (Object.op_Implicit((Object)(object)teamComponent))
		{
			teamIndex = teamComponent.teamIndex;
		}
		if (Object.op_Implicit((Object)(object)victimBody) && Object.op_Implicit((Object)(object)victimMaster))
		{
			PlayerCharacterMasterController playerCharacterMasterController = victimMaster.playerCharacterMasterController;
			if (Object.op_Implicit((Object)(object)playerCharacterMasterController))
			{
				NetworkUser networkUser = playerCharacterMasterController.networkUser;
				if (Object.op_Implicit((Object)(object)networkUser))
				{
					OnPlayerCharacterDeath(damageReport, networkUser);
				}
			}
			if (victimBody.HasBuff(RoR2Content.Buffs.AffixWhite))
			{
				Vector3 val7 = val4;
				GameObject val8 = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/GenericDelayBlast"), val7, Quaternion.identity);
				float num = 12f + victimBody.radius;
				val8.transform.localScale = new Vector3(num, num, num);
				DelayBlast component = val8.GetComponent<DelayBlast>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.position = val7;
					component.baseDamage = victimBody.damage * 1.5f;
					component.baseForce = 2300f;
					component.attacker = val;
					component.radius = num;
					component.crit = Util.CheckRoll(victimBody.crit, victimMaster);
					component.procCoefficient = 0.75f;
					component.maxTimer = 2f;
					component.falloffModel = BlastAttack.FalloffModel.None;
					component.explosionEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/AffixWhiteExplosion");
					component.delayEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/AffixWhiteDelayEffect");
					component.damageType = DamageType.Freeze2s;
					TeamFilter component2 = val8.GetComponent<TeamFilter>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						component2.teamIndex = TeamComponent.GetObjectTeam(component.attacker);
					}
				}
			}
			if (victimBody.HasBuff(RoR2Content.Buffs.AffixPoison))
			{
				Vector3 val9 = val4;
				Quaternion val10 = Quaternion.LookRotation(((Ray)(ref val6)).direction);
				GameObject val11 = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/UrchinTurretMaster"), val9, val10);
				CharacterMaster component3 = val11.GetComponent<CharacterMaster>();
				if (Object.op_Implicit((Object)(object)component3))
				{
					component3.teamIndex = teamIndex;
					NetworkServer.Spawn(val11);
					component3.SpawnBodyHere();
				}
			}
			if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.wispOnDeath) && teamIndex == TeamIndex.Monster && victimMaster.masterIndex != CommonAssets.wispSoulMasterPrefabMasterComponent.masterIndex)
			{
				MasterSummon masterSummon = new MasterSummon();
				masterSummon.position = val4;
				masterSummon.ignoreTeamMemberLimit = true;
				masterSummon.masterPrefab = ((Component)CommonAssets.wispSoulMasterPrefabMasterComponent).gameObject;
				masterSummon.summonerBodyObject = val;
				masterSummon.rotation = Quaternion.LookRotation(((Ray)(ref val6)).direction);
				masterSummon.Perform();
			}
			if (victimBody.HasBuff(RoR2Content.Buffs.Fruiting) || (damageReport.damageInfo != null && (damageReport.damageInfo.damageType & DamageType.FruitOnHit) != 0))
			{
				EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TreebotFruitDeathEffect.prefab"), new EffectData
				{
					origin = val2,
					rotation = Random.rotation
				}, transmit: true);
				int num2 = Mathf.Min(Math.Max(1, (int)(victimBody.bestFitRadius * 2f)), 8);
				GameObject val12 = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/TreebotFruitPack");
				for (int i = 0; i < num2; i++)
				{
					GameObject obj = Object.Instantiate<GameObject>(val12, val2 + Random.insideUnitSphere * victimBody.radius * 0.5f, Random.rotation);
					TeamFilter component4 = obj.GetComponent<TeamFilter>();
					if (Object.op_Implicit((Object)(object)component4))
					{
						component4.teamIndex = attackerTeamIndex;
					}
					obj.GetComponentInChildren<HealthPickup>();
					obj.transform.localScale = new Vector3(1f, 1f, 1f);
					NetworkServer.Spawn(obj);
				}
			}
			if (victimBody.HasBuff(DLC1Content.Buffs.EliteEarth))
			{
				MasterSummon masterSummon2 = new MasterSummon();
				masterSummon2.position = val4;
				masterSummon2.ignoreTeamMemberLimit = true;
				masterSummon2.masterPrefab = CommonAssets.eliteEarthHealerMaster;
				masterSummon2.summonerBodyObject = val;
				masterSummon2.rotation = Quaternion.LookRotation(((Ray)(ref val6)).direction);
				masterSummon2.Perform();
			}
			if (victimBody.HasBuff(DLC1Content.Buffs.EliteVoid) && (!Object.op_Implicit((Object)(object)victimMaster) || victimMaster.IsDeadAndOutOfLivesServer()))
			{
				Vector3 val13 = val4;
				GameObject val14 = Object.Instantiate<GameObject>(Addressables.LoadAssetAsync<GameObject>((object)"RoR2/DLC1/EliteVoid/VoidInfestorMaster.prefab").WaitForCompletion(), val13, Quaternion.identity);
				CharacterMaster component5 = val14.GetComponent<CharacterMaster>();
				if (Object.op_Implicit((Object)(object)component5))
				{
					component5.teamIndex = TeamIndex.Void;
					NetworkServer.Spawn(val14);
					component5.SpawnBodyHere();
				}
			}
		}
		if (Object.op_Implicit((Object)(object)attackerBody))
		{
			attackerBody.HandleOnKillEffectsServer(damageReport);
			if (Object.op_Implicit((Object)(object)attackerMaster) && Object.op_Implicit((Object)(object)inventory))
			{
				int itemCount = inventory.GetItemCount(RoR2Content.Items.IgniteOnKill);
				if (itemCount > 0)
				{
					ProcIgniteOnKill(damageReport, itemCount, victimBody, attackerTeamIndex);
				}
				int itemCount2 = inventory.GetItemCount(RoR2Content.Items.ExplodeOnDeath);
				if (itemCount2 > 0)
				{
					Vector3 val15 = val4;
					float damageCoefficient = 3.5f * (1f + (float)(itemCount2 - 1) * 0.8f);
					float baseDamage = Util.OnKillProcDamage(attackerBody.damage, damageCoefficient);
					GameObject obj2 = Object.Instantiate<GameObject>(CommonAssets.explodeOnDeathPrefab, val15, Quaternion.identity);
					DelayBlast component6 = obj2.GetComponent<DelayBlast>();
					if (Object.op_Implicit((Object)(object)component6))
					{
						component6.position = val15;
						component6.baseDamage = baseDamage;
						component6.baseForce = 2000f;
						component6.bonusForce = Vector3.up * 1000f;
						component6.radius = 12f + 2.4f * ((float)itemCount2 - 1f);
						component6.attacker = damageInfo.attacker;
						component6.inflictor = null;
						component6.crit = Util.CheckRoll(attackerBody.crit, attackerMaster);
						component6.maxTimer = 0.5f;
						component6.damageColorIndex = DamageColorIndex.Item;
						component6.falloffModel = BlastAttack.FalloffModel.SweetSpot;
					}
					TeamFilter component7 = obj2.GetComponent<TeamFilter>();
					if (Object.op_Implicit((Object)(object)component7))
					{
						component7.teamIndex = attackerTeamIndex;
					}
					NetworkServer.Spawn(obj2);
				}
				int itemCount3 = inventory.GetItemCount(RoR2Content.Items.Dagger);
				if (itemCount3 > 0)
				{
					float damageCoefficient2 = 1.5f * (float)itemCount3;
					Vector3 val16 = val2 + Vector3.up * 1.8f;
					for (int j = 0; j < 3; j++)
					{
						ProjectileManager.instance.FireProjectile(CommonAssets.daggerPrefab, val16 + Random.insideUnitSphere * 0.5f, Util.QuaternionSafeLookRotation(Vector3.up + Random.insideUnitSphere * 0.1f), ((Component)attackerBody).gameObject, Util.OnKillProcDamage(attackerBody.damage, damageCoefficient2), 200f, Util.CheckRoll(attackerBody.crit, attackerMaster), DamageColorIndex.Item);
					}
				}
				int itemCount4 = inventory.GetItemCount(RoR2Content.Items.Tooth);
				if (itemCount4 > 0)
				{
					float num3 = Mathf.Pow((float)itemCount4, 0.25f);
					GameObject obj3 = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/HealPack"), val2, Random.rotation);
					TeamFilter component8 = obj3.GetComponent<TeamFilter>();
					if (Object.op_Implicit((Object)(object)component8))
					{
						component8.teamIndex = attackerTeamIndex;
					}
					HealthPickup componentInChildren = obj3.GetComponentInChildren<HealthPickup>();
					if (Object.op_Implicit((Object)(object)componentInChildren))
					{
						componentInChildren.flatHealing = 8f;
						componentInChildren.fractionalHealing = 0.02f * (float)itemCount4;
					}
					obj3.transform.localScale = new Vector3(num3, num3, num3);
					NetworkServer.Spawn(obj3);
				}
				int itemCount5 = inventory.GetItemCount(RoR2Content.Items.Infusion);
				if (itemCount5 > 0)
				{
					int num4 = itemCount5 * 100;
					if (inventory.infusionBonus < num4)
					{
						InfusionOrb infusionOrb = new InfusionOrb();
						infusionOrb.origin = val2;
						infusionOrb.target = Util.FindBodyMainHurtBox(attackerBody);
						infusionOrb.maxHpValue = itemCount5;
						OrbManager.instance.AddOrb(infusionOrb);
					}
				}
				if ((damageInfo.damageType & DamageType.ResetCooldownsOnKill) == DamageType.ResetCooldownsOnKill)
				{
					EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2ResetEffect"), new EffectData
					{
						origin = damageInfo.position
					}, transmit: true);
					SkillLocator skillLocator = attackerBody.skillLocator;
					if (Object.op_Implicit((Object)(object)skillLocator))
					{
						skillLocator.ResetSkills();
					}
				}
				if ((damageInfo.damageType & DamageType.GiveSkullOnKill) == DamageType.GiveSkullOnKill && Object.op_Implicit((Object)(object)victimMaster))
				{
					EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2KillEffect"), new EffectData
					{
						origin = damageInfo.position
					}, transmit: true);
					attackerBody.AddBuff(RoR2Content.Buffs.BanditSkull);
				}
				int itemCount6 = inventory.GetItemCount(RoR2Content.Items.Talisman);
				if (itemCount6 > 0 && Object.op_Implicit((Object)(object)attackerBody.equipmentSlot))
				{
					inventory.DeductActiveEquipmentCooldown(2f + (float)itemCount6 * 2f);
				}
				int itemCount7 = inventory.GetItemCount(JunkContent.Items.TempestOnKill);
				if (itemCount7 > 0 && Util.CheckRoll(25f, attackerMaster))
				{
					GameObject obj4 = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/TempestWard"), victimBody.footPosition, Quaternion.identity);
					TeamFilter component9 = obj4.GetComponent<TeamFilter>();
					if (Object.op_Implicit((Object)(object)component9))
					{
						component9.teamIndex = attackerTeamIndex;
					}
					BuffWard component10 = obj4.GetComponent<BuffWard>();
					if (Object.op_Implicit((Object)(object)component10))
					{
						component10.expireDuration = 2f + 6f * (float)itemCount7;
					}
					NetworkServer.Spawn(obj4);
				}
				int itemCount8 = inventory.GetItemCount(RoR2Content.Items.Bandolier);
				if (itemCount8 > 0 && Util.CheckRoll((1f - 1f / Mathf.Pow((float)(itemCount8 + 1), 0.33f)) * 100f, attackerMaster))
				{
					GameObject obj5 = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/AmmoPack"), val2, Random.rotation);
					TeamFilter component11 = obj5.GetComponent<TeamFilter>();
					if (Object.op_Implicit((Object)(object)component11))
					{
						component11.teamIndex = attackerTeamIndex;
					}
					NetworkServer.Spawn(obj5);
				}
				if (Object.op_Implicit((Object)(object)victimBody) && damageReport.victimIsElite)
				{
					int itemCount9 = inventory.GetItemCount(RoR2Content.Items.HeadHunter);
					int itemCount10 = inventory.GetItemCount(RoR2Content.Items.KillEliteFrenzy);
					if (itemCount9 > 0)
					{
						float duration = 3f + 5f * (float)itemCount9;
						for (int k = 0; k < BuffCatalog.eliteBuffIndices.Length; k++)
						{
							BuffIndex buffIndex = BuffCatalog.eliteBuffIndices[k];
							if (victimBody.HasBuff(buffIndex))
							{
								attackerBody.AddTimedBuff(buffIndex, duration);
							}
						}
					}
					if (itemCount10 > 0)
					{
						attackerBody.AddTimedBuff(RoR2Content.Buffs.NoCooldowns, (float)itemCount10 * 4f);
					}
				}
				int itemCount11 = inventory.GetItemCount(RoR2Content.Items.GhostOnKill);
				if (itemCount11 > 0 && Object.op_Implicit((Object)(object)victimBody) && Util.CheckRoll(7f, attackerMaster))
				{
					Util.TryToCreateGhost(victimBody, attackerBody, itemCount11 * 30);
				}
				if (inventory.GetItemCount(DLC1Content.Items.MinorConstructOnKill) > 0 && Object.op_Implicit((Object)(object)victimBody) && victimBody.isElite && !attackerMaster.IsDeployableLimited(DeployableSlot.MinorConstructOnKill))
				{
					Vector3 forward = Quaternion.AngleAxis((float)Random.Range(0, 360), Vector3.up) * Quaternion.AngleAxis(-80f, Vector3.right) * Vector3.forward;
					FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
					fireProjectileInfo.projectilePrefab = CommonAssets.minorConstructOnKillProjectile;
					fireProjectileInfo.position = val2;
					fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(forward);
					fireProjectileInfo.procChainMask = default(ProcChainMask);
					fireProjectileInfo.target = val;
					fireProjectileInfo.owner = ((Component)attackerBody).gameObject;
					fireProjectileInfo.damage = 0f;
					fireProjectileInfo.crit = false;
					fireProjectileInfo.force = 0f;
					fireProjectileInfo.damageColorIndex = DamageColorIndex.Item;
					FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
					ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
				}
				int itemCount12 = inventory.GetItemCount(DLC1Content.Items.MoveSpeedOnKill);
				if (itemCount12 > 0)
				{
					int num5 = itemCount12 - 1;
					int num6 = 5;
					float num7 = 1f + (float)num5 * 0.5f;
					attackerBody.ClearTimedBuffs(DLC1Content.Buffs.KillMoveSpeed);
					for (int l = 0; l < num6; l++)
					{
						attackerBody.AddTimedBuff(DLC1Content.Buffs.KillMoveSpeed, num7 * (float)(l + 1) / (float)num6);
					}
					EffectData effectData = new EffectData();
					effectData.origin = attackerBody.corePosition;
					CharacterMotor characterMotor = attackerBody.characterMotor;
					bool flag = false;
					if (Object.op_Implicit((Object)(object)characterMotor))
					{
						Vector3 moveDirection = characterMotor.moveDirection;
						if (moveDirection != Vector3.zero)
						{
							effectData.rotation = Util.QuaternionSafeLookRotation(moveDirection);
							flag = true;
						}
					}
					if (!flag)
					{
						effectData.rotation = ((Component)attackerBody).transform.rotation;
					}
					EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MoveSpeedOnKillActivate"), effectData, transmit: true);
				}
				if (Object.op_Implicit((Object)(object)equipmentDef) && Util.CheckRoll(equipmentDef.dropOnDeathChance * 100f, attackerMaster) && Object.op_Implicit((Object)(object)victimBody))
				{
					PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(equipmentIndex), val2 + Vector3.up * 1.5f, Vector3.up * 20f + ((Ray)(ref val6)).direction * 2f);
				}
				int itemCount13 = inventory.GetItemCount(RoR2Content.Items.BarrierOnKill);
				if (itemCount13 > 0 && Object.op_Implicit((Object)(object)attackerBody.healthComponent))
				{
					attackerBody.healthComponent.AddBarrier(15f * (float)itemCount13);
				}
				int itemCount14 = inventory.GetItemCount(RoR2Content.Items.BonusGoldPackOnKill);
				if (itemCount14 > 0 && Util.CheckRoll(4f * (float)itemCount14, attackerMaster))
				{
					GameObject obj6 = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BonusMoneyPack"), val2, Random.rotation);
					TeamFilter component12 = obj6.GetComponent<TeamFilter>();
					if (Object.op_Implicit((Object)(object)component12))
					{
						component12.teamIndex = attackerTeamIndex;
					}
					NetworkServer.Spawn(obj6);
				}
				int itemCount15 = inventory.GetItemCount(RoR2Content.Items.Plant);
				if (itemCount15 > 0)
				{
					GameObject obj7 = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/InterstellarDeskPlant"), victimBody.footPosition, Quaternion.identity);
					DeskPlantController component13 = obj7.GetComponent<DeskPlantController>();
					if (Object.op_Implicit((Object)(object)component13))
					{
						if (Object.op_Implicit((Object)(object)component13.teamFilter))
						{
							component13.teamFilter.teamIndex = attackerTeamIndex;
						}
						component13.itemCount = itemCount15;
					}
					NetworkServer.Spawn(obj7);
				}
				int incubatorOnKillCount = attackerMaster.inventory.GetItemCount(JunkContent.Items.Incubator);
				if (incubatorOnKillCount > 0 && attackerMaster.GetDeployableCount(DeployableSlot.ParentPodAlly) + attackerMaster.GetDeployableCount(DeployableSlot.ParentAlly) < incubatorOnKillCount && Util.CheckRoll(7f + 1f * (float)incubatorOnKillCount, attackerMaster))
				{
					DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/CharacterSpawnCards/cscParentPod"), new DirectorPlacementRule
					{
						placementMode = DirectorPlacementRule.PlacementMode.Approximate,
						minDistance = 3f,
						maxDistance = 20f,
						spawnOnTarget = val5
					}, RoR2Application.rng);
					directorSpawnRequest.summonerBodyObject = attacker;
					directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate(SpawnCard.SpawnResult spawnResult)
					{
						if (spawnResult.success)
						{
							Inventory inventory2 = spawnResult.spawnedInstance.GetComponent<CharacterMaster>().inventory;
							if (Object.op_Implicit((Object)(object)inventory2))
							{
								inventory2.GiveItem(RoR2Content.Items.BoostDamage, 30);
								inventory2.GiveItem(RoR2Content.Items.BoostHp, 10 * incubatorOnKillCount);
							}
						}
					});
					DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
				}
				int itemCount16 = inventory.GetItemCount(RoR2Content.Items.BleedOnHitAndExplode);
				if (itemCount16 > 0 && Object.op_Implicit((Object)(object)victimBody) && (victimBody.HasBuff(RoR2Content.Buffs.Bleeding) || victimBody.HasBuff(RoR2Content.Buffs.SuperBleed)))
				{
					Util.PlaySound("Play_bleedOnCritAndExplode_explode", val);
					Vector3 val17 = val4;
					float damageCoefficient3 = 4f * (float)(1 + (itemCount16 - 1));
					float num8 = 0.15f * (float)(1 + (itemCount16 - 1));
					float baseDamage2 = Util.OnKillProcDamage(attackerBody.damage, damageCoefficient3) + victimBody.maxHealth * num8;
					GameObject obj8 = Object.Instantiate<GameObject>(CommonAssets.bleedOnHitAndExplodeBlastEffect, val17, Quaternion.identity);
					DelayBlast component14 = obj8.GetComponent<DelayBlast>();
					component14.position = val17;
					component14.baseDamage = baseDamage2;
					component14.baseForce = 0f;
					component14.radius = 16f;
					component14.attacker = damageInfo.attacker;
					component14.inflictor = null;
					component14.crit = Util.CheckRoll(attackerBody.crit, attackerMaster);
					component14.maxTimer = 0f;
					component14.damageColorIndex = DamageColorIndex.Item;
					component14.falloffModel = BlastAttack.FalloffModel.SweetSpot;
					obj8.GetComponent<TeamFilter>().teamIndex = attackerTeamIndex;
					NetworkServer.Spawn(obj8);
				}
			}
		}
		GlobalEventManager.onCharacterDeathGlobal?.Invoke(damageReport);
	}

	public void OnHitAll(DamageInfo damageInfo, GameObject hitObject)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		if (damageInfo.procCoefficient == 0f || damageInfo.rejected)
		{
			return;
		}
		_ = NetworkServer.active;
		if (!Object.op_Implicit((Object)(object)damageInfo.attacker))
		{
			return;
		}
		CharacterBody component = damageInfo.attacker.GetComponent<CharacterBody>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		CharacterMaster master = component.master;
		if (!Object.op_Implicit((Object)(object)master))
		{
			return;
		}
		Inventory inventory = master.inventory;
		if (!Object.op_Implicit((Object)(object)master.inventory))
		{
			return;
		}
		if (!damageInfo.procChainMask.HasProc(ProcType.Behemoth))
		{
			int itemCount = inventory.GetItemCount(RoR2Content.Items.Behemoth);
			if (itemCount > 0 && damageInfo.procCoefficient != 0f)
			{
				float num = (1.5f + 2.5f * (float)itemCount) * damageInfo.procCoefficient;
				float damageCoefficient = 0.6f;
				float baseDamage = Util.OnHitProcDamage(damageInfo.damage, component.damage, damageCoefficient);
				EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXQuick"), new EffectData
				{
					origin = damageInfo.position,
					scale = num,
					rotation = Util.QuaternionSafeLookRotation(damageInfo.force)
				}, transmit: true);
				BlastAttack obj = new BlastAttack
				{
					position = damageInfo.position,
					baseDamage = baseDamage,
					baseForce = 0f,
					radius = num,
					attacker = damageInfo.attacker,
					inflictor = null
				};
				obj.teamIndex = TeamComponent.GetObjectTeam(obj.attacker);
				obj.crit = damageInfo.crit;
				obj.procChainMask = damageInfo.procChainMask;
				obj.procCoefficient = 0f;
				obj.damageColorIndex = DamageColorIndex.Item;
				obj.falloffModel = BlastAttack.FalloffModel.None;
				obj.damageType = damageInfo.damageType;
				obj.Fire();
			}
		}
		if ((component.HasBuff(RoR2Content.Buffs.AffixBlue) ? 1 : 0) > 0)
		{
			float damageCoefficient2 = 0.5f;
			float damage = Util.OnHitProcDamage(damageInfo.damage, component.damage, damageCoefficient2);
			float force = 0f;
			Vector3 position = damageInfo.position;
			ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/LightningStake"), position, Quaternion.identity, damageInfo.attacker, damage, force, damageInfo.crit, DamageColorIndex.Item);
		}
	}

	public void OnCrit(CharacterBody body, DamageInfo damageInfo, CharacterMaster master, float procCoefficient, ProcChainMask procChainMask)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		Vector3 hitPos = body.corePosition;
		GameObject val = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Critspark");
		if (Object.op_Implicit((Object)(object)body))
		{
			if (body.critMultiplier > 2f)
			{
				val = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CritsparkHeavy");
			}
			if (Object.op_Implicit((Object)(object)body) && procCoefficient > 0f && Object.op_Implicit((Object)(object)master) && Object.op_Implicit((Object)(object)master.inventory))
			{
				Inventory inventory = master.inventory;
				if (!procChainMask.HasProc(ProcType.HealOnCrit))
				{
					procChainMask.AddProc(ProcType.HealOnCrit);
					int itemCount = inventory.GetItemCount(RoR2Content.Items.HealOnCrit);
					if (itemCount > 0 && Object.op_Implicit((Object)(object)body.healthComponent))
					{
						Util.PlaySound("Play_item_proc_crit_heal", ((Component)body).gameObject);
						if (NetworkServer.active)
						{
							body.healthComponent.Heal((4f + (float)itemCount * 4f) * procCoefficient, procChainMask);
						}
					}
				}
				if (inventory.GetItemCount(RoR2Content.Items.AttackSpeedOnCrit) > 0)
				{
					body.AddTimedBuff(RoR2Content.Buffs.AttackSpeedOnCrit, 3f * procCoefficient);
				}
				int itemCount2 = inventory.GetItemCount(JunkContent.Items.CooldownOnCrit);
				if (itemCount2 > 0)
				{
					Util.PlaySound("Play_item_proc_crit_cooldown", ((Component)body).gameObject);
					SkillLocator component = ((Component)body).GetComponent<SkillLocator>();
					if (Object.op_Implicit((Object)(object)component))
					{
						float dt = (float)itemCount2 * procCoefficient;
						if (Object.op_Implicit((Object)(object)component.primary))
						{
							component.primary.RunRecharge(dt);
						}
						if (Object.op_Implicit((Object)(object)component.secondary))
						{
							component.secondary.RunRecharge(dt);
						}
						if (Object.op_Implicit((Object)(object)component.utility))
						{
							component.utility.RunRecharge(dt);
						}
						if (Object.op_Implicit((Object)(object)component.special))
						{
							component.special.RunRecharge(dt);
						}
					}
				}
			}
		}
		if (damageInfo != null)
		{
			hitPos = damageInfo.position;
		}
		if (Object.op_Implicit((Object)(object)val))
		{
			EffectManager.SimpleImpactEffect(val, hitPos, Vector3.up, transmit: true);
		}
	}

	public static void OnTeamLevelUp(TeamIndex teamIndex)
	{
		GlobalEventManager.onTeamLevelUp?.Invoke(teamIndex);
	}

	public static void OnCharacterLevelUp(CharacterBody characterBody)
	{
		GlobalEventManager.onCharacterLevelUp?.Invoke(characterBody);
	}

	public void OnInteractionBegin(Interactor interactor, IInteractable interactable, GameObject interactableObject)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)interactor))
		{
			Debug.LogError((object)"OnInteractionBegin invalid interactor!");
			return;
		}
		if (interactable == null)
		{
			Debug.LogError((object)"OnInteractionBegin invalid interactable!");
			return;
		}
		if (!Object.op_Implicit((Object)(object)interactableObject))
		{
			Debug.LogError((object)"OnInteractionBegin invalid interactableObject!");
			return;
		}
		GlobalEventManager.OnInteractionsGlobal?.Invoke(interactor, interactable, interactableObject);
		CharacterBody component = ((Component)interactor).GetComponent<CharacterBody>();
		Vector3 val = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		Transform val2 = interactableObject.transform;
		if (Object.op_Implicit((Object)(object)val2))
		{
			val = val2.position;
			rotation = val2.rotation;
		}
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		Inventory inventory = component.inventory;
		if (!Object.op_Implicit((Object)(object)inventory))
		{
			return;
		}
		InteractionProcFilter interactionProcFilter = interactableObject.GetComponent<InteractionProcFilter>();
		int itemCount = inventory.GetItemCount(RoR2Content.Items.Firework);
		if (itemCount > 0 && InteractableIsPermittedForSpawn((MonoBehaviour)interactable))
		{
			ModelLocator component2 = interactableObject.GetComponent<ModelLocator>();
			object obj;
			if (component2 == null)
			{
				obj = null;
			}
			else
			{
				Transform modelTransform = component2.modelTransform;
				obj = ((modelTransform == null) ? null : ((Component)modelTransform).GetComponent<ChildLocator>()?.FindChild("FireworkOrigin"));
			}
			Transform val3 = (Transform)obj;
			Vector3 val4 = (Object.op_Implicit((Object)(object)val3) ? val3.position : (interactableObject.transform.position + Vector3.up * 2f));
			int remaining = 4 + itemCount * 4;
			FireworkLauncher component3 = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/FireworkLauncher"), val4, Quaternion.identity).GetComponent<FireworkLauncher>();
			component3.owner = ((Component)interactor).gameObject;
			component3.crit = Util.CheckRoll(component.crit, component.master);
			component3.remaining = remaining;
		}
		int squidStacks = inventory.GetItemCount(RoR2Content.Items.Squid);
		if (squidStacks > 0 && InteractableIsPermittedForSpawn((MonoBehaviour)interactable))
		{
			CharacterSpawnCard spawnCard = LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscSquidTurret");
			DirectorPlacementRule placementRule = new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Approximate,
				minDistance = 5f,
				maxDistance = 25f,
				position = interactableObject.transform.position
			};
			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng);
			directorSpawnRequest.teamIndexOverride = TeamIndex.Player;
			directorSpawnRequest.summonerBodyObject = ((Component)interactor).gameObject;
			directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate(SpawnCard.SpawnResult result)
			{
				if (result.success && Object.op_Implicit((Object)(object)result.spawnedInstance))
				{
					CharacterMaster component6 = result.spawnedInstance.GetComponent<CharacterMaster>();
					if (Object.op_Implicit((Object)(object)component6) && Object.op_Implicit((Object)(object)component6.inventory))
					{
						component6.inventory.GiveItem(RoR2Content.Items.HealthDecay, 30);
						component6.inventory.GiveItem(RoR2Content.Items.BoostAttackSpeed, 10 * (squidStacks - 1));
					}
				}
			});
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		}
		int itemCount2 = inventory.GetItemCount(RoR2Content.Items.MonstersOnShrineUse);
		if (itemCount2 <= 0)
		{
			return;
		}
		PurchaseInteraction component4 = interactableObject.GetComponent<PurchaseInteraction>();
		if (!Object.op_Implicit((Object)(object)component4) || !component4.isShrine || Object.op_Implicit((Object)(object)interactableObject.GetComponent<ShrineCombatBehavior>()))
		{
			return;
		}
		GameObject val5 = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/Encounters/MonstersOnShrineUseEncounter");
		if (!Object.op_Implicit((Object)(object)val5))
		{
			return;
		}
		GameObject val6 = Object.Instantiate<GameObject>(val5, val, Quaternion.identity);
		NetworkServer.Spawn(val6);
		CombatDirector component5 = val6.GetComponent<CombatDirector>();
		if (Object.op_Implicit((Object)(object)component5) && Object.op_Implicit((Object)(object)Stage.instance))
		{
			float monsterCredit = 40f * Stage.instance.entryDifficultyCoefficient * (float)itemCount2;
			DirectorCard directorCard = component5.SelectMonsterCardForCombatShrine(monsterCredit);
			if (directorCard != null)
			{
				component5.CombatShrineActivation(interactor, monsterCredit, directorCard);
				EffectData effectData = new EffectData
				{
					origin = val,
					rotation = rotation
				};
				EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MonstersOnShrineUse"), effectData, transmit: true);
			}
			else
			{
				NetworkServer.Destroy(val6);
			}
		}
		bool InteractableIsPermittedForSpawn(MonoBehaviour interactableAsMonoBehaviour)
		{
			if (!Object.op_Implicit((Object)(object)interactableAsMonoBehaviour))
			{
				return false;
			}
			if (Object.op_Implicit((Object)(object)interactionProcFilter))
			{
				return interactionProcFilter.shouldAllowOnInteractionBeginProc;
			}
			if (Object.op_Implicit((Object)(object)((Component)interactableAsMonoBehaviour).GetComponent<GenericPickupController>()))
			{
				return false;
			}
			if (Object.op_Implicit((Object)(object)((Component)interactableAsMonoBehaviour).GetComponent<VehicleSeat>()))
			{
				return false;
			}
			if (Object.op_Implicit((Object)(object)((Component)interactableAsMonoBehaviour).GetComponent<NetworkUIPromptController>()))
			{
				return false;
			}
			return true;
		}
	}

	public static void ClientDamageNotified(DamageDealtMessage damageDealtMessage)
	{
		GlobalEventManager.onClientDamageNotified?.Invoke(damageDealtMessage);
	}

	public static void ServerDamageDealt(DamageReport damageReport)
	{
		GlobalEventManager.onServerDamageDealt?.Invoke(damageReport);
	}

	public static void ServerCharacterExecuted(DamageReport damageReport, float executionHealthLost)
	{
		GlobalEventManager.onServerCharacterExecuted?.Invoke(damageReport, executionHealthLost);
	}
}
