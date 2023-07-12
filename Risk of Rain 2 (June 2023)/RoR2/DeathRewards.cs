using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(CharacterBody))]
public class DeathRewards : MonoBehaviour, IOnKilledServerReceiver
{
	[Tooltip("'logUnlockableName' is discontinued. Use 'logUnlockableDef' instead.")]
	[Obsolete("'logUnlockableName' is discontinued. Use 'logUnlockableDef' instead.", true)]
	public string logUnlockableName = "";

	public UnlockableDef logUnlockableDef;

	public PickupDropTable bossDropTable;

	private uint fallbackGold;

	private CharacterBody characterBody;

	private static GameObject coinEffectPrefab;

	private static GameObject logbookPrefab;

	[Header("Deprecated")]
	public SerializablePickupIndex bossPickup;

	public uint goldReward
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)characterBody.master))
			{
				return fallbackGold;
			}
			return characterBody.master.money;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)characterBody.master))
			{
				characterBody.master.money = value;
			}
			else
			{
				fallbackGold = value;
			}
		}
	}

	public uint expReward { get; set; }

	public int spawnValue { get; set; }

	[RuntimeInitializeOnLoadMethod]
	private static void LoadAssets()
	{
		coinEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/CoinEmitter");
		logbookPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/LogPickup");
	}

	private void Awake()
	{
		characterBody = ((Component)this).GetComponent<CharacterBody>();
	}

	public void OnKilledServer(DamageReport damageReport)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		CharacterBody attackerBody = damageReport.attackerBody;
		if (Object.op_Implicit((Object)(object)attackerBody))
		{
			Vector3 corePosition = characterBody.corePosition;
			uint num = goldReward;
			if (Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse6)
			{
				num = (uint)((float)num * 0.8f);
			}
			TeamManager.instance.GiveTeamMoney(damageReport.attackerTeamIndex, num);
			EffectManager.SpawnEffect(coinEffectPrefab, new EffectData
			{
				origin = corePosition,
				genericFloat = goldReward,
				scale = characterBody.radius
			}, transmit: true);
			float num2 = 1f + (characterBody.level - 1f) * 0.3f;
			ExperienceManager.instance.AwardExperience(corePosition, attackerBody, (uint)((float)expReward * num2));
			if (Object.op_Implicit((Object)(object)logUnlockableDef) && Run.instance.CanUnlockableBeGrantedThisRun(logUnlockableDef) && Util.CheckRoll(characterBody.isChampion ? 3f : 1f, damageReport.attackerMaster))
			{
				GameObject obj = Object.Instantiate<GameObject>(logbookPrefab, corePosition, Random.rotation);
				obj.GetComponentInChildren<UnlockPickup>().unlockableDef = logUnlockableDef;
				obj.GetComponent<TeamFilter>().teamIndex = TeamIndex.Player;
				NetworkServer.Spawn(obj);
			}
		}
	}

	[ConCommand(commandName = "migrate_death_rewards_unlockables", flags = ConVarFlags.Cheat, helpText = "Migrates CharacterDeath component .logUnlockableName to .LogUnlockableDef for all instances.")]
	private static void CCMigrateDeathRewardUnlockables(ConCommandArgs args)
	{
		DeathRewards[] array = Resources.FindObjectsOfTypeAll<DeathRewards>();
		foreach (DeathRewards deathRewards in array)
		{
			FieldInfo field = typeof(DeathRewards).GetField("logUnlockableName");
			string text = ((string)field.GetValue(deathRewards)) ?? string.Empty;
			UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(text);
			if (!Object.op_Implicit((Object)(object)unlockableDef) && text != string.Empty)
			{
				args.Log("DeathRewards component on object " + ((Object)((Component)deathRewards).gameObject).name + " has a defined value for 'logUnlockableName' but it doesn't map to any known unlockable. Migration skipped. logUnlockableName='logUnlockableName'");
			}
			else if ((Object)(object)deathRewards.logUnlockableDef == (Object)(object)unlockableDef)
			{
				field.SetValue(deathRewards, string.Empty);
				EditorUtil.SetDirty((Object)(object)deathRewards);
				EditorUtil.SetDirty((Object)(object)((Component)deathRewards).gameObject);
			}
			else if (Object.op_Implicit((Object)(object)deathRewards.logUnlockableDef))
			{
				args.Log($"DeathRewards component on object {((Object)((Component)deathRewards).gameObject).name} has a 'logUnlockableDef' field value which differs from the 'logUnlockableName' lookup. Migration skipped. logUnlockableDef={deathRewards.logUnlockableDef} logUnlockableName={text}");
			}
			else
			{
				deathRewards.logUnlockableDef = unlockableDef;
				field.SetValue(deathRewards, string.Empty);
				EditorUtil.SetDirty((Object)(object)deathRewards);
				EditorUtil.SetDirty((Object)(object)((Component)deathRewards).gameObject);
				args.Log($"DeathRewards component on object {((Object)((Component)deathRewards).gameObject).name} migrated. logUnlockableDef={deathRewards.logUnlockableDef} logUnlockableName={text}");
			}
		}
	}
}
