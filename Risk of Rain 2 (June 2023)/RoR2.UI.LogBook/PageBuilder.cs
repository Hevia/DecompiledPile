using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using HG;
using RoR2.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI.LogBook;

public class PageBuilder
{
	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	public UserProfile userProfile;

	public RectTransform container;

	public Entry entry;

	public readonly List<GameObject> managedObjects = new List<GameObject>();

	private StatSheet statSheet => userProfile.statSheet;

	public void Destroy()
	{
		foreach (GameObject managedObject in managedObjects)
		{
			Object.Destroy((Object)(object)managedObject);
		}
	}

	public void AddSimpleTextPanel(string text)
	{
		((TMP_Text)((Component)AddPrefabInstance(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Logbook/SimpleTextPanel")).GetComponent<ChildLocator>().FindChild("MainLabel")).GetComponent<TextMeshProUGUI>()).text = text;
	}

	public GameObject AddPrefabInstance(GameObject prefab)
	{
		GameObject val = Object.Instantiate<GameObject>(prefab, (Transform)(object)container);
		managedObjects.Add(val);
		return val;
	}

	public void AddSimpleTextPanel(params string[] textLines)
	{
		AddSimpleTextPanel(string.Join("\n", textLines));
	}

	public void AddSimplePickup(PickupIndex pickupIndex)
	{
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
		ItemIndex itemIndex = pickupDef?.itemIndex ?? ItemIndex.None;
		EquipmentIndex equipmentIndex = pickupDef?.equipmentIndex ?? EquipmentIndex.None;
		string token = null;
		if (itemIndex != ItemIndex.None)
		{
			ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
			AddDescriptionPanel(Language.GetString(itemDef.descriptionToken));
			token = itemDef.loreToken;
			ulong statValueULong = statSheet.GetStatValueULong(PerItemStatDef.totalCollected.FindStatDef(itemIndex));
			ulong statValueULong2 = statSheet.GetStatValueULong(PerItemStatDef.highestCollected.FindStatDef(itemIndex));
			string stringFormatted = Language.GetStringFormatted("GENERIC_PREFIX_FOUND", statValueULong);
			string stringFormatted2 = Language.GetStringFormatted("ITEM_PREFIX_STACKCOUNT", statValueULong2);
			AddSimpleTextPanel(stringFormatted, stringFormatted2);
		}
		else if (equipmentIndex != EquipmentIndex.None)
		{
			EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
			AddDescriptionPanel(Language.GetString(equipmentDef.descriptionToken));
			token = equipmentDef.loreToken;
			string stringFormatted3 = Language.GetStringFormatted("EQUIPMENT_PREFIX_COOLDOWN", equipmentDef.cooldown);
			string stringFormatted4 = Language.GetStringFormatted("EQUIPMENT_PREFIX_TOTALTIMEHELD", statSheet.GetStatDisplayValue(PerEquipmentStatDef.totalTimeHeld.FindStatDef(equipmentIndex)));
			string stringFormatted5 = Language.GetStringFormatted("EQUIPMENT_PREFIX_USECOUNT", statSheet.GetStatDisplayValue(PerEquipmentStatDef.totalTimesFired.FindStatDef(equipmentIndex)));
			AddSimpleTextPanel(stringFormatted3);
			AddSimpleTextPanel(stringFormatted4, stringFormatted5);
		}
		AddNotesPanel(Language.IsTokenInvalid(token) ? Language.GetString("EARLY_ACCESS_LORE") : Language.GetString(token));
	}

	public void AddDescriptionPanel(string content)
	{
		AddSimpleTextPanel(Language.GetStringFormatted("DESCRIPTION_PREFIX_FORMAT", content));
	}

	public void AddNotesPanel(string content)
	{
		AddSimpleTextPanel(Language.GetStringFormatted("NOTES_PREFIX_FORMAT", content));
	}

	public void AddBodyStatsPanel(CharacterBody bodyPrefabComponent)
	{
		float baseMaxHealth = bodyPrefabComponent.baseMaxHealth;
		float levelMaxHealth = bodyPrefabComponent.levelMaxHealth;
		float baseDamage = bodyPrefabComponent.baseDamage;
		float levelDamage = bodyPrefabComponent.levelDamage;
		float baseArmor = bodyPrefabComponent.baseArmor;
		float baseRegen = bodyPrefabComponent.baseRegen;
		float levelRegen = bodyPrefabComponent.levelRegen;
		float baseMoveSpeed = bodyPrefabComponent.baseMoveSpeed;
		AddSimpleTextPanel(Language.GetStringFormatted("BODY_HEALTH_FORMAT", Language.GetStringFormatted("BODY_STATS_FORMAT", baseMaxHealth.ToString(), levelMaxHealth.ToString())) + "\n" + Language.GetStringFormatted("BODY_DAMAGE_FORMAT", Language.GetStringFormatted("BODY_STATS_FORMAT", baseDamage.ToString(), levelDamage.ToString())) + "\n" + ((baseRegen >= Mathf.Epsilon) ? (Language.GetStringFormatted("BODY_REGEN_FORMAT", Language.GetStringFormatted("BODY_STATS_FORMAT", baseRegen.ToString(), levelRegen.ToString())) + "\n") : "") + Language.GetStringFormatted("BODY_MOVESPEED_FORMAT", baseMoveSpeed) + "\n" + Language.GetStringFormatted("BODY_ARMOR_FORMAT", baseArmor.ToString()));
	}

	public void AddMonsterPanel(CharacterBody bodyPrefabComponent)
	{
		ulong statValueULong = statSheet.GetStatValueULong(PerBodyStatDef.killsAgainst, ((Object)((Component)bodyPrefabComponent).gameObject).name);
		ulong statValueULong2 = statSheet.GetStatValueULong(PerBodyStatDef.killsAgainstElite, ((Object)((Component)bodyPrefabComponent).gameObject).name);
		ulong statValueULong3 = statSheet.GetStatValueULong(PerBodyStatDef.deathsFrom, ((Object)((Component)bodyPrefabComponent).gameObject).name);
		string stringFormatted = Language.GetStringFormatted("MONSTER_PREFIX_KILLED", statValueULong);
		string stringFormatted2 = Language.GetStringFormatted("MONSTER_PREFIX_ELITESKILLED", statValueULong2);
		string stringFormatted3 = Language.GetStringFormatted("MONSTER_PREFIX_DEATH", statValueULong3);
		AddSimpleTextPanel(stringFormatted, stringFormatted2, stringFormatted3);
	}

	public void AddSurvivorPanel(CharacterBody bodyPrefabComponent)
	{
		string statDisplayValue = statSheet.GetStatDisplayValue(PerBodyStatDef.longestRun.FindStatDef(((Object)bodyPrefabComponent).name));
		ulong statValueULong = statSheet.GetStatValueULong(PerBodyStatDef.timesPicked.FindStatDef(((Object)bodyPrefabComponent).name));
		ulong statValueULong2 = statSheet.GetStatValueULong(StatDef.totalGamesPlayed);
		double num = 0.0;
		if (statValueULong2 != 0L)
		{
			num = (double)statValueULong / (double)statValueULong2 * 100.0;
		}
		sharedStringBuilder.Clear();
		sharedStringBuilder.AppendLine(Language.GetStringFormatted("SURVIVOR_PREFIX_LONGESTRUN", statDisplayValue));
		sharedStringBuilder.AppendLine(Language.GetStringFormatted("SURVIVOR_PREFIX_TIMESPICKED", statValueULong));
		sharedStringBuilder.AppendLine(Language.GetStringFormatted("SURVIVOR_PREFIX_PICKPERCENTAGE", num));
		AddSimpleTextPanel(sharedStringBuilder.ToString());
	}

	public void AddSimpleBody(CharacterBody bodyPrefabComponent)
	{
		AddBodyStatsPanel(bodyPrefabComponent);
	}

	public void AddBodyLore(CharacterBody characterBody)
	{
		bool flag = false;
		string token = "";
		string baseNameToken = characterBody.baseNameToken;
		if (!string.IsNullOrEmpty(baseNameToken))
		{
			token = baseNameToken.Replace("_NAME", "_LORE");
			if (!Language.IsTokenInvalid(token))
			{
				flag = true;
			}
		}
		if (flag)
		{
			AddNotesPanel(Language.GetString(token));
		}
		else
		{
			AddNotesPanel(Language.GetString("EARLY_ACCESS_LORE"));
		}
	}

	public void AddStagePanel(SceneDef sceneDef)
	{
		string statDisplayValue = userProfile.statSheet.GetStatDisplayValue(PerStageStatDef.totalTimesVisited.FindStatDef(sceneDef.baseSceneName));
		string statDisplayValue2 = userProfile.statSheet.GetStatDisplayValue(PerStageStatDef.totalTimesCleared.FindStatDef(sceneDef.baseSceneName));
		string stringFormatted = Language.GetStringFormatted("STAGE_PREFIX_TOTALTIMESVISITED", statDisplayValue);
		string stringFormatted2 = Language.GetStringFormatted("STAGE_PREFIX_TOTALTIMESCLEARED", statDisplayValue2);
		sharedStringBuilder.Clear();
		sharedStringBuilder.Append(stringFormatted);
		sharedStringBuilder.Append("\n");
		sharedStringBuilder.Append(stringFormatted2);
		AddSimpleTextPanel(sharedStringBuilder.ToString());
	}

	public void AddPieChart(PieChartMeshController.SliceInfo[] sliceInfos)
	{
		GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/PieChartPanel"), (Transform)(object)container);
		val.GetComponent<PieChartMeshController>().SetSlices(sliceInfos);
		managedObjects.Add(val);
	}

	public static void Stage(PageBuilder builder)
	{
		SceneDef sceneDef = (SceneDef)builder.entry.extraData;
		builder.AddStagePanel(sceneDef);
		builder.AddNotesPanel(Language.IsTokenInvalid(sceneDef.loreToken) ? Language.GetString("EARLY_ACCESS_LORE") : Language.GetString(sceneDef.loreToken));
	}

	public static void SimplePickup(PageBuilder builder)
	{
		builder.AddSimplePickup((PickupIndex)builder.entry.extraData);
	}

	public static void SimpleBody(PageBuilder builder)
	{
		builder.AddSimpleBody((CharacterBody)builder.entry.extraData);
	}

	public static void MonsterBody(PageBuilder builder)
	{
		CharacterBody characterBody = (CharacterBody)builder.entry.extraData;
		builder.AddSimpleBody(characterBody);
		builder.AddMonsterPanel(characterBody);
		builder.AddBodyLore(characterBody);
	}

	public static void SurvivorBody(PageBuilder builder)
	{
		CharacterBody characterBody = (CharacterBody)builder.entry.extraData;
		builder.AddSimpleBody(characterBody);
		builder.AddSurvivorPanel(characterBody);
		builder.AddBodyLore(characterBody);
	}

	public static void StatsPanel(PageBuilder builder)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_093e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0943: Unknown result type (might be due to invalid IL or missing references)
		//IL_0950: Unknown result type (might be due to invalid IL or missing references)
		UserProfile userProfile = (UserProfile)builder.entry.extraData;
		GameCompletionStatsHelper gameCompletionStatsHelper = new GameCompletionStatsHelper();
		StatSheet statSheet = userProfile.statSheet;
		CalcAllBodyStatTotalDouble(PerBodyStatDef.totalTimeAlive);
		_ = (double)CalcAllBodyStatTotalULong(PerBodyStatDef.timesPicked);
		double value = (double)CalcAllBodyStatTotalULong(PerBodyStatDef.totalWins);
		_ = (double)CalcAllBodyStatTotalULong(PerBodyStatDef.deathsAs);
		ChildLocator component = builder.AddPrefabInstance(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Logbook/ProfileStatsPanel")).GetComponent<ChildLocator>();
		RectTransform val = (RectTransform)component.FindChild("CharacterPieChart");
		RectTransform val2 = (RectTransform)component.FindChild("CompletionBarPanel");
		RectTransform val3 = (RectTransform)component.FindChild("CompletionLabel");
		RectTransform val4 = (RectTransform)component.FindChild("CharacterStatsCarousel");
		RectTransform val5 = (RectTransform)component.FindChild("TotalsStatsList");
		RectTransform val6 = (RectTransform)component.FindChild("RecordsStatsList");
		RectTransform val7 = (RectTransform)component.FindChild("MiscStatsList");
		RectTransform val8 = (RectTransform)component.FindChild("CompletionStatsList");
		PieChartMeshController characterPieChartMeshController = ((Component)val).GetComponent<PieChartMeshController>();
		CarouselNavigationController carousel = ((Component)val4).GetComponent<CarouselNavigationController>();
		List<string> statNames = new List<string>();
		List<Action> callbacks = new List<Action>();
		AddPerBodyStat(PerBodyStatDef.totalWins);
		AddPerBodyStat(PerBodyStatDef.timesPicked);
		AddPerBodyStat(PerBodyStatDef.totalTimeAlive);
		AddPerBodyStat(PerBodyStatDef.longestRun);
		AddPerBodyStat(PerBodyStatDef.deathsAs);
		AddPerBodyStat(PerBodyStatDef.damageDealtAs);
		AddPerBodyStat(PerBodyStatDef.damageTakenAs);
		AddPerBodyStat(PerBodyStatDef.damageDealtTo);
		AddPerBodyStat(PerBodyStatDef.damageTakenFrom);
		AddPerBodyStat(PerBodyStatDef.killsAgainst);
		AddPerBodyStat(PerBodyStatDef.killsAgainstElite);
		AddPerBodyStat(PerBodyStatDef.deathsFrom);
		AddPerBodyStat(PerBodyStatDef.minionDamageDealtAs);
		AddPerBodyStat(PerBodyStatDef.minionKillsAs);
		AddPerBodyStat(PerBodyStatDef.killsAs);
		carousel.onPageChangeSubmitted += OnPageChangeSubmitted;
		carousel.SetDisplayData(new CarouselNavigationController.DisplayData(statNames.Count, 0));
		OnPageChangeSubmitted(0);
		GameObject statStripPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Logbook/LogbookStatStrip");
		StatDef statDef2 = PerBodyStatDef.longestRun.FindStatDef(statSheet.FindBodyWithHighestStat(PerBodyStatDef.longestRun)) ?? PerBodyStatDef.longestRun.FindStatDef(BodyCatalog.FindBodyIndex("CommandoBody"));
		CharacterBody bodyPrefabBodyComponent = BodyCatalog.GetBodyPrefabBodyComponent(statSheet.FindBodyWithHighestStat(PerBodyStatDef.deathsFrom));
		EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(statSheet.FindEquipmentWithHighestStat(PerEquipmentStatDef.totalTimeHeld));
		(string, string, Texture)[] array = new(string, string, Texture)[20];
		(string, string, Texture2D) tuple = StatStripDataFromStatDef(StatDef.totalGamesPlayed);
		array[0] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalTimeAlive);
		array[1] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalDeaths);
		array[2] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		array[3] = (Language.GetString("STATNAME_TOTALWINS"), TextSerialization.ToStringNumeric(value), null);
		tuple = StatStripDataFromStatDef(StatDef.totalKills);
		array[4] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalEliteKills);
		array[5] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalDamageDealt);
		array[6] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalStagesCompleted);
		array[7] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalDamageTaken);
		array[8] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalHealthHealed);
		array[9] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.goldCollected);
		array[10] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalDistanceTraveled);
		array[11] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalPurchases);
		array[12] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalBloodPurchases);
		array[13] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalDronesPurchased);
		array[14] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalTurretsPurchased);
		array[15] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalCrocoInfectionsInflicted);
		array[16] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalMinionDamageDealt);
		array[17] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalMinionKills);
		array[18] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.totalDeathsWhileBurning);
		array[19] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		SetStats(val5, array);
		(string, string, Texture)[] obj = new(string, string, Texture)[6]
		{
			(Language.GetString("STATNAME_LONGESTRUN"), statSheet.GetStatDisplayValue(statDef2), null),
			default((string, string, Texture)),
			default((string, string, Texture)),
			default((string, string, Texture)),
			default((string, string, Texture)),
			default((string, string, Texture))
		};
		tuple = StatStripDataFromStatDef(StatDef.highestStagesCompleted);
		obj[1] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.highestLevel);
		obj[2] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.highestDamageDealt);
		obj[3] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.maxGoldCollected);
		obj[4] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromStatDef(StatDef.highestPurchases);
		obj[5] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		SetStats(val6, obj);
		SetStats(val7, new(string, string, Texture)[2]
		{
			(Language.GetString("STATNAME_NEMESIS"), Object.op_Implicit((Object)(object)bodyPrefabBodyComponent) ? Language.GetString(bodyPrefabBodyComponent.baseNameToken) : string.Empty, Object.op_Implicit((Object)(object)bodyPrefabBodyComponent) ? bodyPrefabBodyComponent.portraitIcon : null),
			(Language.GetString("STATNAME_FAVORITEEQUIPMENT"), ((Object)(object)equipmentDef != (Object)null) ? Language.GetString(equipmentDef.nameToken) : string.Empty, equipmentDef?.pickupIconTexture)
		});
		(string, string, Texture)[] array2 = new(string, string, Texture)[5];
		tuple = StatStripDataFromCompletionFraction("STATNAME_COMPLETION_ACHIEVEMENTS", gameCompletionStatsHelper.GetAchievementCompletion(userProfile));
		array2[0] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromCompletionFraction("STATNAME_COMPLETION_COLLECTIBLES", gameCompletionStatsHelper.GetCollectibleCompletion(userProfile));
		array2[1] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromCompletionFraction("STATNAME_COMPLETION_PICKUPDISCOVERY", gameCompletionStatsHelper.GetPickupEncounterCompletion(userProfile));
		array2[2] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromCompletionFraction("STATNAME_COMPLETION_SURVIVORSPICKED", gameCompletionStatsHelper.GetSurvivorPickCompletion(userProfile));
		array2[3] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		tuple = StatStripDataFromCompletionFraction("STATNAME_COMPLETION_SURVIVORSWON", gameCompletionStatsHelper.GetSurvivorWinCompletion(userProfile));
		array2[4] = (tuple.Item1, tuple.Item2, (Texture)(object)tuple.Item3);
		SetStats(val8, array2);
		IntFraction value2 = gameCompletionStatsHelper.GetTotalCompletion(userProfile);
		float num = (float)value2;
		Vector2 anchorMax = val2.anchorMax;
		anchorMax.x = num;
		val2.anchorMax = anchorMax;
		((Component)val3).GetComponent<TMP_Text>().SetText($"{num:0%}", true);
		void AddLine(StatDef statDef, string statNameToken, double? allBodyTotal)
		{
			string @string = Language.GetString("STAT_NAME_VALUE_FORMAT");
			string statDisplayValue = statSheet.GetStatDisplayValue(statDef);
			P_3.bodyTextStringBuilder.AppendFormat(@string, Language.GetString(statNameToken), statDisplayValue);
			if (allBodyTotal.HasValue)
			{
				double statValueAsDouble = statSheet.GetStatValueAsDouble(statDef);
				double num2 = 0.0;
				if (allBodyTotal != 0.0)
				{
					num2 = statValueAsDouble / allBodyTotal.Value;
				}
				P_3.bodyTextStringBuilder.Append(" ").AppendFormat(P_3.rateFormat, num2);
			}
			P_3.bodyTextStringBuilder.AppendLine();
		}
		void AddLineFromPerBodyStat(PerBodyStatDef perBodyStatDef, double? total)
		{
			if (!total.HasValue)
			{
				total = CalcAllBodyStatTotalDouble(perBodyStatDef);
			}
			StatDef statDef3 = perBodyStatDef.FindStatDef(P_2.bodyIndex);
			AddLine(statDef3, perBodyStatDef.nameToken, total);
		}
		void AddPerBodyStat(PerBodyStatDef perBodyStatDef)
		{
			statNames.Add(Language.GetString(perBodyStatDef.nameToken));
			callbacks.Add(Callback);
			void Callback()
			{
				BuildCharacterPieChart(characterPieChartMeshController, (BodyIndex bodyIndex) => GetStatWeight(perBodyStatDef.FindStatDef(bodyIndex)));
			}
		}
		TooltipContent BuildBodyTooltipContent(BodyIndex bodyIndex, Color bodyColor)
		{
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Unknown result type (might be due to invalid IL or missing references)
			CharacterBody bodyPrefabBodyComponent2 = BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex);
			StringBuilder bodyTextStringBuilder = StringBuilderPool.RentStringBuilder();
			string rateFormat = Language.GetString("PERCENT_FORMAT_PARENTHESES");
			AddLineFromPerBodyStat(PerBodyStatDef.timesPicked, null);
			AddLineFromPerBodyStat(PerBodyStatDef.totalTimeAlive, null);
			AddLineFromPerBodyStat(PerBodyStatDef.longestRun, null);
			AddLineFromPerBodyStat(PerBodyStatDef.totalWins, null);
			AddLineFromPerBodyStat(PerBodyStatDef.deathsAs, null);
			AddLineFromPerBodyStat(PerBodyStatDef.damageDealtAs, null);
			AddLineFromPerBodyStat(PerBodyStatDef.damageTakenAs, null);
			AddLineFromPerBodyStat(PerBodyStatDef.damageDealtTo, null);
			AddLineFromPerBodyStat(PerBodyStatDef.damageTakenFrom, null);
			AddLineFromPerBodyStat(PerBodyStatDef.killsAgainst, null);
			AddLineFromPerBodyStat(PerBodyStatDef.killsAgainstElite, null);
			AddLineFromPerBodyStat(PerBodyStatDef.deathsFrom, null);
			AddLineFromPerBodyStat(PerBodyStatDef.minionDamageDealtAs, null);
			AddLineFromPerBodyStat(PerBodyStatDef.minionKillsAs, null);
			AddLineFromPerBodyStat(PerBodyStatDef.killsAs, null);
			TooltipContent tooltipContent = default(TooltipContent);
			tooltipContent.titleToken = bodyPrefabBodyComponent2.baseNameToken;
			tooltipContent.titleColor = bodyColor;
			tooltipContent.overrideBodyText = bodyTextStringBuilder.ToString();
			TooltipContent result3 = tooltipContent;
			StringBuilderPool.ReturnStringBuilder(bodyTextStringBuilder);
			return result3;
		}
		void BuildCharacterPieChart(PieChartMeshController pieChartMeshController, Func<BodyIndex, float> bodyWeightGetter)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			List<PieChartMeshController.SliceInfo> list = new List<PieChartMeshController.SliceInfo>();
			for (BodyIndex bodyIndex2 = (BodyIndex)0; (int)bodyIndex2 < BodyCatalog.bodyCount; bodyIndex2++)
			{
				float num4 = bodyWeightGetter(bodyIndex2);
				if (num4 != 0f)
				{
					PieChartMeshController.SliceInfo item4 = default(PieChartMeshController.SliceInfo);
					item4.color = GetBodyColor(bodyIndex2);
					item4.weight = num4;
					item4.tooltipContent = BuildBodyTooltipContent(bodyIndex2, item4.color);
					list.Add(item4);
				}
			}
			pieChartMeshController.SetSlices(list.OrderBy((PieChartMeshController.SliceInfo slice) => 0f - slice.weight).ToArray());
		}
		double CalcAllBodyStatTotalDouble(PerBodyStatDef perBodyStatDef)
		{
			double num5 = 0.0;
			for (BodyIndex bodyIndex4 = (BodyIndex)0; (int)bodyIndex4 < BodyCatalog.bodyCount; bodyIndex4++)
			{
				num5 += statSheet.GetStatValueAsDouble(perBodyStatDef.FindStatDef(bodyIndex4));
			}
			return num5;
		}
		BigInteger CalcAllBodyStatTotalULong(PerBodyStatDef perBodyStatDef)
		{
			BigInteger result4 = 0;
			for (BodyIndex bodyIndex3 = (BodyIndex)0; (int)bodyIndex3 < BodyCatalog.bodyCount; bodyIndex3++)
			{
				result4 += (BigInteger)statSheet.GetStatValueULong(perBodyStatDef.FindStatDef(bodyIndex3));
			}
			return result4;
		}
		static Color GetBodyColor(BodyIndex bodyIndex)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			CharacterBody bodyPrefabBodyComponent3 = BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex);
			if (bodyPrefabBodyComponent3.bodyColor != Color.clear)
			{
				return bodyPrefabBodyComponent3.bodyColor;
			}
			string bodyName = BodyCatalog.GetBodyName(bodyIndex);
			ulong num3 = 0uL;
			for (int j = 0; j < bodyName.Length; j++)
			{
				num3 += bodyName[j];
			}
			Xoroshiro128Plus val9 = new Xoroshiro128Plus(num3);
			return Color.HSVToRGB(val9.nextNormalizedFloat, val9.RangeFloat(0.5f, 1f), val9.RangeFloat(0.6f, 0.8f));
		}
		float GetStatWeight(StatDef statDef)
		{
			return statDef.dataType switch
			{
				StatDataType.ULong => statSheet.GetStatValueULong(statDef), 
				StatDataType.Double => (float)statSheet.GetStatValueDouble(statDef), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		void OnPageChangeSubmitted(int newPage)
		{
			callbacks[newPage]();
			((TMP_Text)((Component)((Component)carousel).GetComponent<ChildLocator>().FindChild("StatLabel")).GetComponent<HGTextMeshProUGUI>()).text = statNames[newPage];
		}
		void SetStats(RectTransform container, (string name, string value, Texture texture)[] data)
		{
			UIElementAllocator<ChildLocator> uIElementAllocator = new UIElementAllocator<ChildLocator>(container, statStripPrefab, markElementsUnsavable: true, acquireExistingChildren: true);
			uIElementAllocator.AllocateElements(data.Length);
			ReadOnlyCollection<ChildLocator> elements = uIElementAllocator.elements;
			for (int i = 0; i < data.Length; i++)
			{
				(string name, string value, Texture texture) tuple2 = data[i];
				string item = tuple2.name;
				string item2 = tuple2.value;
				Texture item3 = tuple2.texture;
				ChildLocator childLocator = elements[i];
				((Component)childLocator.FindChild("NameLabel")).GetComponent<TMP_Text>().SetText(item, true);
				((Component)childLocator.FindChild("ValueLabel")).GetComponent<TMP_Text>().SetText("<color=#FFFF7F>" + item2 + "</color>", true);
				RawImage component2 = ((Component)childLocator.FindChild("IconRawImage")).GetComponent<RawImage>();
				((Component)((Component)component2).transform.parent).gameObject.SetActive(Object.op_Implicit((Object)(object)item3));
				component2.texture = item3;
			}
		}
		static (string name, string value, Texture2D texture) StatStripDataFromCompletionFraction(string displayToken, IntFraction completionFraction)
		{
			(string, string, Texture2D) result = default((string, string, Texture2D));
			result.Item1 = Language.GetString(displayToken);
			result.Item2 = Language.GetStringFormatted("STAT_COMPLETION_VALUE_FORMAT", completionFraction.numerator, completionFraction.denominator, (float)completionFraction);
			result.Item3 = null;
			return result;
		}
		(string name, string value, Texture2D texture) StatStripDataFromStatDef(StatDef statDef)
		{
			(string, string, Texture2D) result2 = default((string, string, Texture2D));
			result2.Item1 = Language.GetString(statDef.displayToken);
			result2.Item2 = statSheet.GetStatDisplayValue(statDef);
			result2.Item3 = null;
			return result2;
		}
	}

	public void AddRunReportPanel(RunReport runReport)
	{
		GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/GameEndReportPanelScrolling"), (Transform)(object)container);
		val.GetComponent<GameEndReportPanelController>().SetDisplayData(new GameEndReportPanelController.DisplayData
		{
			runReport = runReport,
			playerIndex = 0
		});
		val.GetComponent<MPEventSystemProvider>().fallBackToMainEventSystem = true;
		managedObjects.Add(val);
	}

	public static void RunReportPanel(PageBuilder builder)
	{
		builder.AddRunReportPanel((RunReport)builder.entry.extraData);
	}
}
